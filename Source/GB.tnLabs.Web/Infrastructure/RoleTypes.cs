using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GB.tnLabs.Web.Infrastructure
{
	public static class RoleTypes
	{
		/// <summary>
		/// Any registered user. With or without a subscription.
		/// </summary>
		public const string Trainer = "Trainer";

		/// <summary>
		/// A user that has access to a subscription.
		/// </summary>
		public const string Member = "Member";

		/// <summary>
		/// User with owner rights on a subscription.
		/// </summary>
		public const string Owner = "Owner";
	}
}