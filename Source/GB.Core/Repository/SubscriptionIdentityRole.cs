using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.Core.Repository
{
    public class SubscriptionIdentityRole
    {
        public int SubscriptionIdentityRoleId { get; set; }

        public int SubscriptionId { get; set; }

        public int IdentityId { get; set; }

        public string Role { get; set; }

        public virtual Identity Identity { get; set; }

        public virtual Subscription Subscription { get; set; }
    }
}
