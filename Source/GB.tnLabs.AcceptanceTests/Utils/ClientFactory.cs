using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.AcceptanceTests.Utils
{
    using System.Net.Http;
    using System.Web.Http.SelfHost;

    using GB.tnLabs.Web;

    public class ClientFactory
    {
        private static Uri baseAdress = new Uri("http://localhost:58264/");

        public static Uri BaseAdress
        {
            get
            {
                return baseAdress;
            }

            set
            {
                baseAdress = value;
            }
        }

        public static HttpClient Create()
        {
            var configuration = new HttpSelfHostConfiguration(baseAdress);
            WebApiConfig.Register(configuration);

            var server = new HttpSelfHostServer(configuration);
            var client = new HttpClient(server) { BaseAddress = baseAdress };

            return client;
        }
    }
}
