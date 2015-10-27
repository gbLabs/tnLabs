using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.Core.Repository
{
    public class TemplateVM
    {
        public int TemplateVMId { get; set; }

        public string ServiceName { get; set; }

        public string VMName { get; set; }

        public int SubscriptionId { get; set; }

        public int CreatorId { get; set; }

        public int IdentityId { get; set; }

        public int State { get; set; }

        public string VMAdminUser { get; set; }

        public string VMAdminPass { get; set; }

        public Guid Key { get; set; }

        public string VMLabel { get; set; }

        public string Description { get; set; }

        public int VmRdpPort { get; set; }

        public string ImageName { get; set; }

        public string SourceImageName { get; set; }
        
        public DateTimeOffset StateChangedTimestamp { get; set; }

        [ForeignKey("CreatorId")]
        public virtual Identity Identity { get; set; }

        [ForeignKey("IdentityId")]
        public virtual Identity UserIdentity { get; set; }

        public virtual Subscription Subscription { get; set; }
    }
}
