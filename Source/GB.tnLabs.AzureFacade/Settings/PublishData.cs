using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GB.tnLabs.AzureFacade.Settings
{
    [Serializable]
    public class PublishData
    {
        public PublishProfile PublishProfile { get; set; }
    }
}
