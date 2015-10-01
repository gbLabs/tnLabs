using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GB.tnLabs.Web.Enums
{
    public enum SessionSchedulingEnum
    {
        None = 0,
		/// <summary>
		/// The VM will be started at the beggining of the specified interval, 
		/// and stopped and deleted at the end of that interval.
		/// </summary>
        Continuous = 1,

		/// <summary>
		/// The Vm will be created at the begining of the date interval, started at the beggining of the specified
		/// daily interval, stopped at the end of the specified daily interval. The vm will be deleted at the end of
		/// the specified date interval.
		/// </summary>
        Daily = 2,

		/// <summary>
		/// The VM will be created at the start date. Starting the VM and stopping will be done based on
		/// an external signal. Deleting the VM will be done from the management console.
		/// </summary>
		OnDemand = 3
    }
}