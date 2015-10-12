using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.Core.Repository
{
    public class Invitation
    {
        public int InvitationId { get; set; }
        public string Email { get; set; }

        public Subscription Subscription { get; set; }

        public Identity Identity { get; set; }

        public bool IsRegistered { get; set; }
    }
}
