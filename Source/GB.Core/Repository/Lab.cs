using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.Core.Repository
{
    public class Lab
    {
        public Lab()
        {
            Sessions = new HashSet<Session>();
        }

        public int LabId { get; set; }

        public string Name { get; set; }

        public string ImageName { get; set; }

        public string Description { get; set; }

        public int SubscriptionId { get; set; }

        public DateTime CreationDate { get; set; }

        public bool Removed { get; set; }

        public virtual Subscription Subscription { get; set; }

        public virtual ICollection<Session> Sessions { get; set; }
    }
}
