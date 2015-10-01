using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.Core.Repository
{
    public class Session
    {
        public Session()
        {
            SessionUsers = new HashSet<SessionUser>();
            VirtualMachines = new HashSet<VirtualMachine>();
        }

        public int SessionId { get; set; }

        public string SessionName { get; set; }

        public int LabId { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public TimeSpan? StartInterval { get; set; }

        public TimeSpan? EndInterval { get; set; }

        public int SchedulingType { get; set; }

        public string VmSize { get; set; }

        public int SubscriptionId { get; set; }

        public bool Removed { get; set; }

        public string Version { get; set; }

        public virtual Lab Lab { get; set; }

        public virtual ICollection<SessionUser> SessionUsers { get; set; }

        public virtual ICollection<VirtualMachine> VirtualMachines { get; set; }
    }
}
