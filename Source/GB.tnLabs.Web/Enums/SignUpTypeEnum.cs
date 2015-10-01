using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GB.tnLabs.Web.Enums
{
	public enum SignUpTypeEnum
	{
		/// <summary>
		/// The user does not want a subscription assigned at sign-up.
		/// </summary>
		NoSubscription = 0,

		/// <summary>
		/// The user wants the subscription to be auto-created and managed for him. 
		/// </summary>
		AutoSubscription = 1,

		/// <summary>
		/// The user wants to use his own subscription.
		/// </summary>
		OwnSubscription = 2
	}
}