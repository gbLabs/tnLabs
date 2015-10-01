using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using GB.tnLabs.AzureFacade;
using GB.tnLabs.AzureFacade.Enums;
using GB.tnLabs.AzureFacade.Interfaces;
using GB.tnLabs.AzureFacade.Models;
using GB.tnLabs.Core.Repository;
using IMPLTYPEFLAGS = System.Runtime.InteropServices.IMPLTYPEFLAGS;

namespace GB.tnLabs.Core.Components
{
    public class SubscriptionService
    {
        private readonly Lazy<AzureFacadeFactory> _azureFacadeFactoryLazy;
		private readonly ILogger _logger = NullLogger.Instance;

		protected AzureFacadeFactory AzureFacadeFactory
		{
			get { return _azureFacadeFactoryLazy.Value; }
		}

        public SubscriptionService(Lazy<AzureFacadeFactory> azureFacadeFactoryLazy, ILogger logger)
		{
			_azureFacadeFactoryLazy = azureFacadeFactoryLazy;
			_logger = logger;
		}

        public int CheckForAvailableResources(int subscriptionId, DateTimeOffset sessionStart, DateTimeOffset sessionEnd)
        {
            int availableCPUs;

            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                //TODO: move warmUP and coolDown into the config
                TimeSpan warmUp = new TimeSpan(0, 20, 0);
                //there is an extra +5 as this is a safe assumption as how long will it take for azure to
                //delete the VMs
                TimeSpan coolDown = new TimeSpan(0, 30 + 5, 0);

                DateTimeOffset start = sessionStart.Subtract(warmUp);
                DateTimeOffset end = sessionEnd.Add(coolDown);

                var sameWindowSessions = context.Sessions.Where(x => x.SubscriptionId == subscriptionId &&
                    x.StartDate <= end && x.EndDate >= start && !x.Removed).ToList();

                //int projectedCPUs = sameWindowSessions.Sum(x => Sessions.CalculateSessionCPUs(x));
				int projectedCPUs = MaxOverlappingCPUs(sameWindowSessions);

                //now check how many VMs programmed are running at the moment
                List<VmSizeEnum> runningVMs = context.VirtualMachines
                    .Where(x => !x.Deleted && !x.Stopped && x.Session.SubscriptionId == subscriptionId)
                    .Select(x => x.Session.VmSize).ToList()
                    .Select(x => (VmSizeEnum)Enum.Parse(typeof(VmSizeEnum), x)).ToList();

                int currentPlannedCPUs = runningVMs.Sum(x => (int)x);

                Subscription subscription = context.Subscriptions
                        .Single(x => x.SubscriptionId == subscriptionId);

                IVMManagement management = AzureFacadeFactory.VMManagement(subscription.AzureSubscriptionId,
                    subscription.Certificate, subscription.CertificateKey, subscription.BlobStorageName,
                    subscription.BlobStorageKey);
                SubscriptionDetails subscriptionDetails = management.GetSubscriptionDetails();
                int unknownCPUs = subscriptionDetails.MaxCoreCount - subscriptionDetails.AvailableCoreCount -
                    currentPlannedCPUs;

                unknownCPUs = Math.Max(unknownCPUs, 0);

                availableCPUs = subscriptionDetails.MaxCoreCount - unknownCPUs - projectedCPUs;
            }

            return availableCPUs;
        }

		private int MaxOverlappingCPUs(List<Session> windowsSessions)
		{
			if (!windowsSessions.Any()) return 0;

			TimeSpan warmUp = new TimeSpan(0, 20, 0);
			//there is an extra +5 as this is a safe assumption as how long will it take for azure to
			//delete the VMs
			TimeSpan coolDown = new TimeSpan(0, 30 + 5, 0);

			//TODO: treat it as immutable
			windowsSessions.ForEach(x =>
				{
					x.StartDate = x.StartDate.Subtract(warmUp);
					x.EndDate = x.EndDate.Add(coolDown);
				});

			//TODO: Distinct() doesn't seem to work on DateTimeOffset, investigate why
			var orderedTimes = windowsSessions.SelectMany(x => new List<DateTimeOffset> { x.StartDate, x.EndDate })
				.Distinct().OrderBy(x => x).ToList();

			List<int> cpuCount = new List<int>();
			for (int i = 0; i < orderedTimes.Count-1; i++)
			{
				DateTimeOffset start = orderedTimes[i];
				DateTimeOffset end = orderedTimes[i + 1];

				int cpus = windowsSessions.Where(x => x.StartDate <= end && x.EndDate >= start && !x.Removed)
					.Sum(x => Sessions.CalculateSessionCPUs(x));

				cpuCount.Add(cpus);
			}

			return cpuCount.Max();
		}
    }
}
