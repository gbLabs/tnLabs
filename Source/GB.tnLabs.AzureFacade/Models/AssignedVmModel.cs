using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.AzureFacade.Models
{
    public class AssignedVmModel
    {
        public int UserId { get; internal set; }

        public string UserName { get; internal set; }

        public string VmName { get; internal set; }

        public int VmRdpPort { get; internal set; }

        public string Password { get; set; }
    }
}
