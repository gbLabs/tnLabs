using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.AcceptanceTests
{
    using GB.tnLabs.AcceptanceTests.Utils;

    using Xunit;

    public class LabTests
    {
        private readonly Uri requestUri = new Uri(ClientFactory.BaseAdress, "api/lab/");

        [Fact]
        public void Get_All_Labs_Response_Return_Correct_Status_Code()
        {
            using (var client = ClientFactory.Create())
            {
                var response = client.GetAsync(requestUri).Result;

                Assert.True(response.IsSuccessStatusCode, "Actual status code: " + response.StatusCode);
            }
        }
    }
}
