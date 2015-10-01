using Castle.Core.Logging;
using GB.tnLabs.AzureFacade.Interfaces;
using GB.tnLabs.Core;
using GB.tnLabs.Core.Components;
using GB.tnLabs.Core.Enums;
using GB.tnLabs.Core.Repository;
using GB.tnLabs.Core.SBDtos;
using GB.tnLabs.Web.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using HealthCheckService = GB.tnLabs.Core.Components.HealthCheck;

namespace GB.tnLabs.Web.Controllers
{
    public class AzureManagementController : ApiController
    {
        #region private fields

        private readonly ILogger _logger = NullLogger.Instance;
		private readonly Lazy<IUnitOfWork> _unitOfWorkLazy;
		private readonly Lazy<ServiceBusMessageHandler> _messagingLazy;
		private readonly Lazy<HealthCheckService> _healthCheckServiceLazy;


        #endregion private fields

		#region properties

		protected IUnitOfWork UnitOfWork
		{
			get { return _unitOfWorkLazy.Value; }
		}

		protected ServiceBusMessageHandler Messaging
		{
			get { return _messagingLazy.Value; }
		}

		protected HealthCheckService HealthCheckService { get { return _healthCheckServiceLazy.Value; } }

		#endregion properties

		#region .ctor

		public AzureManagementController(ILogger logger, 
			Lazy<IUnitOfWork> unitOfWorkLazy, Lazy<ServiceBusMessageHandler> messagingLazy,
			Lazy<HealthCheckService> healthCheckLazy )
        {
            _logger = logger;
			_unitOfWorkLazy = unitOfWorkLazy;
			_messagingLazy = messagingLazy;
			_healthCheckServiceLazy = healthCheckLazy;
        }

        #endregion .ctor

        #region API methods

		/// <summary>
        /// Will create a VM for setting up by the user. Will insert special script to ease the Capture process.
		/// Service method is non-blocking, and returns immediately.
        /// </summary>
		/// <param name="imageName">The OS image from the gallery or a valid image. </param> 
		/// <param name="imageDescription">A friendly description for the image.</param>
		/// <param name="newImageName">A friendly image name.</param>
		[HttpGet]
		public async Task<string> CreateBaseVmImage(string imageName, string imageDescription, 
			string newImageName)
		{
			return await CreateBaseVmImage(imageName, imageDescription, newImageName, null);
		}

	    /// <summary>
	    /// Will create a VM for setting up by the user. Will insert special script to ease the Capture process.
	    /// Service method is non-blocking, and returns immediately.
	    /// </summary>
	    /// <param name="imageName">The OS image from the gallery or a valid image. </param> 
	    /// <param name="imageDescription">A friendly description for the image.</param>
	    /// <param name="newImageName">A friendly image name.</param>
	    /// <param name="chocoPackages">The list of chocolatey packages to be installed.</param>
	    [HttpGet]
		public async Task<string> CreateBaseVmImage(string imageName, string imageDescription, 
			string newImageName, string chocoPackages)
		{
			// Security considerations: because it requires an SubscriptionId that is retrieved from the
			// UnitOfWork, this method will succeede only for a logged in user.

			_logger.Info("Entering AzureManagement.CreateBaseVmImage");

			string result = "OK";

			try
			{
				int userId = UnitOfWork.CurrentIdentity.IdentityId;
				int activeSubscriptionId = UnitOfWork.ActiveSubscriptionId;

				List<string> chocoPackageList = chocoPackages != null ? 
					chocoPackages.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList() : 
					new List<string>();

				CreateBaseVMImageRequest request = new CreateBaseVMImageRequest
				{
					ImageName = imageName,
					ImageDescription = imageDescription,
					NewImageName = newImageName,
					UserId = userId,
					SubscriptionId = activeSubscriptionId,
					ChocoPackageList = chocoPackageList
				};

				await Messaging.SendAsync(request);
			}
			catch (Exception ex)
			{
				_logger.Fatal("Exception when creating base vm image.", ex);
				result = "Error";
			}

			_logger.Info("Exiting AzureManagement.CreateBaseVmImage");

			return result;
		}

		/// <summary>
		/// Api method to be called from the extension script. It notifies the system that the extensions
		/// have finished installing.
		/// </summary>
		/// <param name="templateVmKey">The key assigned to the VM.</param>
		[HttpGet]
		public string BaseVmProvisionedWithExtensions(string templateVmKey)
		{
			_logger.Info("Entering AzureManagement.BaseVmProvisionedWithExtensions");

			string result = "OK";

			try
			{
				using (tnLabsDBEntities repository = new tnLabsDBEntities())
				{
					Guid key = new Guid(templateVmKey);
					TemplateVM templateVm = repository.TemplateVMs.Single(x => x.Key == key);

					Email.BuildTemplateVMReady(templateVm).Send();
				}
			}
			catch (Exception ex)
			{
				_logger.Fatal("Exception when marking VM provisioned.", ex);
				result = "Error";
			}

			_logger.Info("Exiting AzureManagement.BaseVmProvisionedWithExtensions");

			return result;
		}
		
		/// <summary>
		/// Will capture the vm when it's status changes to 'Stopped'.
		/// </summary>
		/// <param name="templateVmKey">The key assigned to the VM.</param>
		/// <returns>OK - for success.</returns>
		[HttpGet]
		public async Task<string> ReadyForCapture(string templateVmKey)
		{
			_logger.Info("Entering AzureManagement.ReadyForCapture");

			string result = "OK";

			try
			{
				using (tnLabsDBEntities repository = new tnLabsDBEntities())
				{
					Guid key = new Guid(templateVmKey);
					TemplateVM templateVM = repository.TemplateVMs.Single(x => x.Key == key);
					templateVM.State = (int)TemplateVMStatusEnum.ReadyToCapture;
					templateVM.StateChangedTimestamp = DateTimeOffset.Now;
					repository.SaveChanges();

					//schedule the check and capture in 5 minutes
					await Messaging.SendAsync(new VMReadyForCaptureRequest 
						{ TemplateVMId = templateVM.TemplateVMId },
						DateTime.UtcNow.AddMinutes(5));
				}
			}
			catch (Exception ex)
			{
				_logger.Fatal("Exception when marking VM for capture.", ex);
				result = "Error";
			}

			_logger.Info("Exiting AzureManagement.ReadyForCapture");

			return result;
		}

		[HttpGet]
		public async Task<string> HealthCheck(string resourceId)
		{
			string result = "OK";

			try
			{
				Guid resourceGuid = Guid.Parse(resourceId);
				await HealthCheckService.CheckInAsync(resourceGuid);
			}
			catch (Exception ex)
			{
				_logger.Fatal("Exception when health checking.", ex);
				result = "Error";
			}

			return result;
		}

        #endregion API methods
	}
}
