using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.Core
{
	public static class Settings
	{
		/// <summary>
		/// Time ahead of Session when the VMs start spinning up.
		/// </summary>
		public static TimeSpan SessionWarmUp { get { return TimeSpan.FromMinutes(30); } }

		/// <summary>
		/// Time after the session when the VMs are being deleted.
		/// </summary>
		public static TimeSpan SessionCoolDown { get { return TimeSpan.FromMinutes(30); } }
	}
}
