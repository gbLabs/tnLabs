using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GB.tnLabs.Web.Infrastructure
{
	public static class SpecialClaimTypes
	{
		public static readonly string IdentityProvider = "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider";

		public static readonly string UserId = "userId";

		public static readonly string Subscription = "subscription";
	}
}