using Castle.Core.Logging;
using GB.tnLabs.AzureFacade;
using GB.tnLabs.AzureFacade.Enums;
using GB.tnLabs.AzureFacade.Interfaces;
using GB.tnLabs.AzureFacade.Models;
using GB.tnLabs.Core.Repository;
using GB.tnLabs.Web.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace GB.tnLabs.Web.Controllers
{
    public class AzureTestController : ApiController
    {

        private readonly ILogger _logger = NullLogger.Instance;
        private readonly IVMManagement _vmManagement;


        public AzureTestController(ILogger logger, AzureFacadeFactory azureFacadeFactory)
        {
            _logger = logger;

			using (ApplicationDbContext context = new ApplicationDbContext())
			{
				Subscription subscription = context.Subscriptions
					.Single(x => x.SubscriptionId == 1);

				_vmManagement = azureFacadeFactory.VMManagement(subscription.AzureSubscriptionId,
					subscription.Certificate, subscription.CertificateKey, subscription.BlobStorageName,
					subscription.BlobStorageKey);
			}
        }

        // GET api/azuretest
        public string Get()
        {
            _logger.Info("Entering AzureTest.Get");

            try
            {
                //important: sitecore reference image:
                //http://portalvhdsjkm3zryfjc71m.blob.core.windows.net/vhds/gbLabsTest111.vhd

				var result = _vmManagement.GetVmState("tnlabimage34629");

              //  _vmManagement.GetVmState("tnvm12");
                //var dump = _vmManagement.GetAvailableTemplateImages(VmImageSourceEnum.Gallery);
                //var studio = dump.Where(x => x.Description.Contains("Code"));
                //_vmManagement.GetVmInfo();

                //var result = management.GenerateVMsForUsers(
                //    "gbUbuntuLabsTest14-",
                //    "gbUbuntuServer-gbUbuntuServer-0-201311231058050679", //"sitecorecVM1",
                //    new List<VMUserModel>
                //    {
                //     new VMUserModel { UserId = 1, Username = "adminuser", Password = "Prl#!ss1" },
                //     //new VMUserModel { UserId = 2, Username = "adminuser", Password = "Prl#!ss1" },
                //    });
                //management.CloneBlob("sitecoreImg1.vhd", "http://portalvhdsjkm3zryfjc71m.blob.core.windows.net/vhds/gbLabsTest111.vhd");
                //management.CloneBlob("sitecoreImg2.vhd", "http://portalvhdsjkm3zryfjc71m.blob.core.windows.net/vhds/gbLabsTest111.vhd");
                //management.CloneBlob("sitecoreImg3.vhd", "http://portalvhdsjkm3zryfjc71m.blob.core.windows.net/vhds/gbLabsTest111.vhd");
                //management.CloneBlob("sitecoreImg4.vhd", "http://portalvhdsjkm3zryfjc71m.blob.core.windows.net/vhds/gbLabsTest111.vhd");

                //management.CloneBlob("sitecoreTempl.vhd", "http://portalvhdsjkm3zryfjc71m.blob.core.windows.net/vhds/gbLabsTest111.vhd", createDisk: false);

                //var result = new List<AssignedVmModel>();
                //return result;

                return "OK";

            }
            catch (Exception e)
            {
                return e.ToString();
            }
            finally
            {
                _logger.Info("Exiting AzureTest.Get");
            }
        }
    }
}
