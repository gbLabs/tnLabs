using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GB.tnLabs.AzureFacade.AzureSerializationModels
{
    [Serializable]
    public class RoleInstance
    {
        public string RoleName { get; set; }

        public string PowerState { get; set; }
    }
}
