using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GB.tnLabs.AzureFacade.AzureSerializationModels
{
    [Serializable]
    [XmlRoot(Namespace="http://schemas.microsoft.com/windowsazure")]
    public class Deployment
    {
        
        public List<RoleInstance> RoleInstanceList { get; set; }
    }
}
