using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace GB.tnLabs.AzureFacade.Settings
{
    [Serializable]
    public class PublishProfile
    {
        [XmlAttribute]
        public string SchemaVersion { get; set; }

        [XmlAttribute]
        public string PublishMethod { get; set; }

        public Subscription Subscription { get; set; }
    }
}
