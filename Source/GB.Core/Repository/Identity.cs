using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.Core.Repository
{
    public class Identity
    {
        public Identity()
        {
            SubscriptionIdentityRoles = new HashSet<SubscriptionIdentityRole>();
            TemplateVMs = new HashSet<TemplateVM>();
        }

        public int IdentityId { get; set; }

        public string NameIdentifier { get; set; }

        [Obsolete("set to string empty")]
        public string IdentityProvider { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Obsolete("Set to string empty")]
        public string DisplayName { get; set; }

        public string Email { get; set; }

        public virtual ICollection<SubscriptionIdentityRole> SubscriptionIdentityRoles { get; set; }

        public virtual ICollection<TemplateVM> TemplateVMs { get; set; }
    }
}
