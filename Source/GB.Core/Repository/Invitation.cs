using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.Core.Repository
{
    public class Invitation
    {
        public int InvitationId { get; set; }

        public string Email { get; set; }

        public int SubscriptionId { get; set; }

        public int IdentityId { get; set; }

        public bool Processed { get; set; }

        [ForeignKey("SubscriptionId")]
        public Subscription Subscription { get; set; }

        [ForeignKey("IdentityId")]
        public Identity Identity { get; set; }
    }
}
