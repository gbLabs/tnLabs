using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.Core.Repository
{
    public class Subscription
    {
        public Subscription()
        {
            SubscriptionIdentityRoles = new HashSet<SubscriptionIdentityRole>();
            Labs = new HashSet<Lab>();
            TemplateVMs = new HashSet<TemplateVM>();
            Users = new HashSet<User>();
        }

        public int SubscriptionId { get; set; }

        public string AzureSubscriptionId { get; set; }

        public string CertificateKey { get; set; }

        public string BlobStorageName { get; set; }

        public string BlobStorageKey { get; set; }

        public byte[] Certificate { get; set; }

        public string Name { get; set; }

        public virtual ICollection<SubscriptionIdentityRole> SubscriptionIdentityRoles { get; set; }

        public virtual ICollection<Lab> Labs { get; set; }

        public virtual ICollection<TemplateVM> TemplateVMs { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}
