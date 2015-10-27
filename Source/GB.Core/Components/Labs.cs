using Castle.Core.Logging;
using GB.tnLabs.AzureFacade;
using GB.tnLabs.AzureFacade.Enums;
using GB.tnLabs.AzureFacade.Interfaces;
using GB.tnLabs.AzureFacade.Models;
using GB.tnLabs.Core.Annotations;
using GB.tnLabs.Core.Enums;
using GB.tnLabs.Core.Repository;
using GB.tnLabs.Core.SBDtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GB.tnLabs.Core.Components
{
	[UsedImplicitly]
	public class Labs
	{
		private readonly Lazy<AzureFacadeFactory> _azureFacadeFactoryLazy;
		private readonly Lazy<ServiceBusMessageHandler> _messagingLazy;
		private ILogger _logger = NullLogger.Instance;

		protected AzureFacadeFactory AzureFacadeFactory
		{
			get { return _azureFacadeFactoryLazy.Value; }
		}

		protected ServiceBusMessageHandler Messaging
		{
			get { return _messagingLazy.Value; }
		}

		public Labs(Lazy<AzureFacadeFactory> azureFacadeFactoryLazy, Lazy<ServiceBusMessageHandler> messagingLazy,
			ILogger logger)
		{
			_azureFacadeFactoryLazy = azureFacadeFactoryLazy;
			_messagingLazy = messagingLazy;
			_logger = logger;
		}

		public void VMReadyForCapture(VMReadyForCaptureRequest request)
		{
			using (ApplicationDbContext repository = new ApplicationDbContext())
			{
				_logger.Info("Checking if Template VM is ready for capture.");

				//check if the vm is ready for capture
				//if not post another SB message for later time
				TemplateVM templateVm = repository.TemplateVMs.Include("Subscription")
					.Single(x => x.State == (int)TemplateVMStatusEnum.ReadyToCapture &&
						x.TemplateVMId == request.TemplateVMId);

				Subscription subscription = templateVm.Subscription;

				IVMManagement vmManagement = AzureFacadeFactory.VMManagement(subscription.AzureSubscriptionId,
					subscription.Certificate, subscription.CertificateKey, subscription.BlobStorageName,
					subscription.BlobStorageKey);

				//can capture the VM only if it's stopped
				Dictionary<string, PowerStatesEnum> powerStates = vmManagement.GetVmState(templateVm.ServiceName);
				if (powerStates[templateVm.VMName] == PowerStatesEnum.Stopped)
				{
					_logger.Info("Template VM ready for capture. Trying to capture.");

					templateVm.State = (int)TemplateVMStatusEnum.Capturing;
					templateVm.StateChangedTimestamp = DateTimeOffset.Now;
					repository.SaveChanges();

					string vmImageName = vmManagement.CaptureVM(templateVm.ServiceName,
						templateVm.VMName, templateVm.VMLabel);

					//save the image name back to db, mark the vm as created
					templateVm.ImageName = vmImageName;
					templateVm.State = (int)TemplateVMStatusEnum.Ready;
					templateVm.StateChangedTimestamp = DateTimeOffset.Now;
					repository.SaveChanges();

					//need to delete the service manually as it is left behind by the capture process
					vmManagement.DeleteService(templateVm.ServiceName);

					Email.BuildTemplateVMCaptured(templateVm, labCreated: true).Send();

					//TODO: trigger the creation of the lab if necessary (how do we decide that?)
					//TODO: now this is hacky, and needs to be moved into an appropriate place
					Lab lab = new Lab
					{
						CreationDate = DateTime.Now,
						Description = templateVm.Description,
						ImageName = templateVm.ImageName,
						Name = templateVm.VMLabel,
						SubscriptionId = templateVm.SubscriptionId
					};
					repository.Labs.Add(lab);
					repository.SaveChanges();

					_logger.Info("Template VM captured.");
				}
				else
				{
					_logger.Info("Template VM not ready for capture, scheduling a check in 1 minute.");
					//put another scheduled check in a minute
					Messaging.Send(request, DateTime.UtcNow.AddMinutes(1));
				}
			}
		}

		public void CreateBaseVMImage(CreateBaseVMImageRequest request)
		{
			using (ApplicationDbContext repository = new ApplicationDbContext())
			{
				_logger.InfoFormat("Creating Base VM Image with image description {0}, and subscription ID {1}.",
					request.ImageDescription, request.SubscriptionId);

				Guid key = Guid.NewGuid();

				Subscription subscription = repository.Subscriptions
					.Single(x=>x.SubscriptionId == request.SubscriptionId);

				IVMManagement vmManagement = AzureFacadeFactory.VMManagement(subscription.AzureSubscriptionId,
						subscription.Certificate, subscription.CertificateKey, subscription.BlobStorageName,
						subscription.BlobStorageKey);

				//TODO: get region from subscription but at this moment there is no such field
				string region = "North Europe";// LocationNames.NorthEurope;

				VMConfigModel vmConfig = new VMConfigModel(request.ImageName, region)
				{
					InsertCaptureScript = true,
					CaptureKey = key.ToString(),
					ChocoPackageList = request.ChocoPackageList
				};
				VMUserModel vmUser = new VMUserModel
				{
					IdentityId = request.UserId,
					//TODO: make it a bit random or smth?
					Username = "TemplateUser",
					Password = "TemplatePass1!"
				};
				AssignedVmModel assignedVm = vmManagement.GenerateVm("tnlabimage", vmConfig, vmUser);

				Identity identity = repository.Identities.Single(x => x.IdentityId == request.UserId);

				TemplateVM templateVm = new TemplateVM
				{
					ServiceName = assignedVm.VmName,
					VMName = assignedVm.VmName,
					VmRdpPort = assignedVm.VmRdpPort,
					SubscriptionId = request.SubscriptionId,
					CreatorId = vmUser.IdentityId,
					VMAdminUser = vmUser.Username,
					VMAdminPass = vmUser.Password,
					Key = key,
					VMLabel = request.NewImageName,
					Description = request.ImageDescription,
					Identity = identity,
					SourceImageName = request.ImageName
				};

				repository.TemplateVMs.Add(templateVm);
				repository.SaveChanges();

				Email.BuildTemplateVMReady(templateVm).Send();

				_logger.InfoFormat("Created Base VM Image with image description {0}, and subscription ID {1}.",
					request.ImageDescription, request.SubscriptionId);
			}
		}
	}
}
