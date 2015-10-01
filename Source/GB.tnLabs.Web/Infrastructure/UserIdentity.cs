using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GB.tnLabs.Web.Infrastructure
{
	public class UserIdentity
	{
		public int ActiveSubscriptionId { get; set; }

		//TODO: decide if we'll leave this here! - it might be useless
		//public string ActiveAzureSubscription { get; set; }



		public int IdentityId { get; set; }
	}
}