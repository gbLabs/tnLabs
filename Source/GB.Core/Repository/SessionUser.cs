using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.Core.Repository
{
    public class SessionUser
    {
        public int SessionUserId { get; set; }

        public int SessionId { get; set; }

        public int IdentityId { get; set; }

        [ForeignKey("IdentityId")]
        public virtual Identity Identity { get; set; }

        public virtual Session Session { get; set; }
    }
}
