using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.Scheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            const string requestUri = "api/azuremanagement";
            try
            {
                var baseAdress = ConfigurationManager.AppSettings["tnLabsBaseUri"];
                var client = new HttpClient() { BaseAddress = new Uri(baseAdress) };

                var response = client.GetAsync(requestUri).Result;

                Console.WriteLine(string.Format("Call to {0} returned status code {1}", requestUri, response.StatusCode));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
