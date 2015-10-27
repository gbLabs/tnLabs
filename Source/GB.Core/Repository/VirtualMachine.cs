using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.Core.Repository
{
    public class VirtualMachine
    {
        public int VirtualMachineId { get; set; }

        public int IdentityId { get; set; }

        public int SessionId { get; set; }

        public string VmName { get; set; }

        public int VmRdpPort { get; set; }

        public string VmAdminUser { get; set; }

        public string VmAdminPass { get; set; }

        public bool Deleted { get; set; }

        public int? Stopped1 { get; set; }

        public bool Stopped { get; set; }

        [ForeignKey("IdentityId")]
        public virtual Identity Identity { get; set; }

        public virtual Session Session { get; set; }
    }
}
