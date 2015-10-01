using Castle.Core.Logging;
using GB.tnLabs.AzureFacade;
using GB.tnLabs.AzureFacade.Enums;
using GB.tnLabs.AzureFacade.Interfaces;
using GB.tnLabs.AzureFacade.Models;
using GB.tnLabs.Core.Repository;
using GB.tnLabs.Core.SBDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.Core.Components
{
	public class Sessions
	{
		private readonly Lazy<AzureFacadeFactory> _azureFacadeFactoryLazy;
		private readonly ILogger _logger = NullLogger.Instance;

		protected AzureFacadeFactory AzureFacadeFactory
		{
			get { return _azureFacadeFactoryLazy.Value; }
		}

		public Sessions(Lazy<AzureFacadeFactory> azureFacadeFactoryLazy, ILogger logger)
		{
			_azureFacadeFactoryLazy = azureFacadeFactoryLazy;
			_logger = logger;
		}

		public void Start(StartSessionRequest request)
		{
			using (ApplicationDbContext context = new ApplicationDbContext())
			{
				Session session = context.Sessions.Single(x => x.SessionId == request.SessionId);
				//Session session = context.Sessions.Single(x => x.Version == version);

				//check if the session is canceled or the request is no longer valid
                if (session.Removed ||
                    (session.Version != request.Version.ToString()))
                {
                    _logger.InfoFormat("StartSessionRequest for Sessionid: {0} was ignored because Removed: {1} and Version: {2}",
                        request.SessionId, session.Removed, request.Version);
                    return;
                }

				_logger.InfoFormat("Spawning VMs for SessionId: {0} ({1} - {2})",
					session.SessionId, session.StartDate, session.EndDate);

				Subscription subscription = context.Subscriptions
					.Single(x => x.SubscriptionId == session.SubscriptionId);

				IVMManagement vmManagement = AzureFacadeFactory.VMManagement(subscription.AzureSubscriptionId,
					subscription.Certificate, subscription.CertificateKey, subscription.BlobStorageName,
					subscription.BlobStorageKey);

				//TODO: remove the hardcoding at some point in time
				string NorthEurope = "North Europe";
					
				VMConfigModel vmConfig = new VMConfigModel(session.Lab.ImageName, NorthEurope)
				{
					VmSize = (VmSizeEnum)Enum.Parse(typeof(VmSizeEnum), session.VmSize)
				};

				var sessionUsers = session.SessionUsers.ToList();
				var vmUsers = sessionUsers.Select(x =>
					new VMUserModel
					{
						UserId = x.UserId,
						Username = x.User.UserName,
						Password = x.User.Password
					}).ToList();

				//figure out a URL friendly name for the vm names
				string vmNameBase = "tn" + session.SessionId + "vm";


				var assignedVMs = vmManagement.GenerateVMsForUsers(vmNameBase, vmConfig, vmUsers);

				foreach (var assignedVM in assignedVMs)
				{
					VirtualMachine virtualMachine = new VirtualMachine
					{
						UserId = assignedVM.UserId,
						VmAdminPass = assignedVM.Password,
						VmAdminUser = assignedVM.UserName,
						VmName = assignedVM.VmName,
						VmRdpPort = assignedVM.VmRdpPort
					};

					session.VirtualMachines.Add(virtualMachine);
					context.SaveChanges();
				}

				_logger.InfoFormat("Spawned VMs for SessionId: {0} ({1} - {2})",
					session.SessionId, session.StartDate, session.EndDate);

				_logger.InfoFormat("Sending emails for SessionId: {0} ({1} - {2})",
					session.SessionId, session.StartDate, session.EndDate);
				//well.. this is a hack
				//TODO: send the correct service name, that should be returned from the Azure Facade
				Email.BuildSessionEmails(session, vmNameBase).Send();
			}
		}

		public void End(EndSessionRequest request)
		{
			using (ApplicationDbContext context = new ApplicationDbContext())
			{
				string version = request.Version.ToString();
				//Session session = context.Sessions.Single(x => x.SessionId == request.SessionId);
				Session session = context.Sessions.Single(x => x.SessionId == request.SessionId);

				//check if the request is no longer valid
				if (session.Version != request.Version.ToString())
                {
                    _logger.InfoFormat("EndSessionRequest for Sessionid: {0} was ignored because Removed: {1} and Version: {2}",
                        request.SessionId, session.Removed, request.Version);
                    return;
                }

				_logger.InfoFormat("Deleting VMs for SessionId: {0} ({1} - {2})",
						session.SessionId, session.StartDate, session.EndDate);

				Subscription subscription = context.Subscriptions
					.Single(x => x.SubscriptionId == session.SubscriptionId);

				IVMManagement vmManagement = AzureFacadeFactory.VMManagement(subscription.AzureSubscriptionId,
					subscription.Certificate, subscription.CertificateKey, subscription.BlobStorageName,
					subscription.BlobStorageKey);

				//TODO: need to keep service name in the session
				string vmNameBase = "tn" + session.SessionId + "vm";
				vmManagement.DeleteService(vmNameBase);

				foreach (VirtualMachine vm in session.VirtualMachines.Where(x => !x.Deleted))
				{
					vm.Deleted = true;
				}

				context.SaveChanges();

				_logger.InfoFormat("Deleted VMs for SessionId: {0} ({1} - {2})",
					session.SessionId, session.StartDate, session.EndDate);
			}
		}

	    public static int CalculateSessionCPUs(Session session)
	    {
	        VmSizeEnum vmSize = (VmSizeEnum)Enum.Parse(typeof(VmSizeEnum), session.VmSize);
	        int cpuCount = (int)vmSize;
	        return session.SessionUsers.Count * cpuCount;
	    }
	  
	}
}
