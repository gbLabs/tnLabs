using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace GB.tnLabs.AzureFacade.Settings
{
    [Serializable]
    public class Subscription
    {
        [XmlAttribute]
        public string ServiceManagementUrl { get; set; }

        [XmlAttribute]
        public string Id { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string ManagementCertificate { get; set; }
    }
}
