using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.Core.Repository
{
    public class SessionUser
    {
        public int SessionUserId { get; set; }

        public int SessionId { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }

        public virtual Session Session { get; set; }
    }
}
