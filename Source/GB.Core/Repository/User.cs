using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.Core.Repository
{
    public class User
    {
        public User()
        {
            SessionUsers = new HashSet<SessionUser>();
            VirtualMachines = new HashSet<VirtualMachine>();
        }

        public int UserId { get; set; }

        public string Email { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public bool Removed { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int SubscriptionId { get; set; }

        public virtual ICollection<SessionUser> SessionUsers { get; set; }

        public virtual ICollection<VirtualMachine> VirtualMachines { get; set; }

        public virtual Subscription Subscription { get; set; }
    }
}
