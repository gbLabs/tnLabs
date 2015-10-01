using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.AzureFacade.Models
{
	public class SubscriptionDetails
	{
		public int AvailableCoreCount { get; set; }

		public int MaxCoreCount { get; set; }

		public int AvailableServiceCount { get; set; }

		public int MaxServiceCount { get; set; }
	}
}
