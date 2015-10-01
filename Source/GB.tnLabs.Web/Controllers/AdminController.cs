using GB.tnLabs.Core.Repository;
using GB.tnLabs.Web.Models;
using GB.tnLabs.Web.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GB.tnLabs.Web.Controllers
{
	//[Authorize(RoleTypes.GB)]
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

		[HttpPost]
		public ActionResult Index(SubscriptionCreateModel model)
		{
			Subscription subscription = new Subscription
			{
				BlobStorageKey = model.BlobStorageKey,
				BlobStorageName = model.BlobStorageName,
				CertificateKey = model.CertificateKey,
				AzureSubscriptionId = model.AzureSubscriptionId
			};

			BinaryReader r = new BinaryReader(Request.Files[0].InputStream);
			subscription.Certificate = r.ReadBytes(Request.Files[0].ContentLength);

			using (tnLabsDBEntities context = new tnLabsDBEntities())
			{
				context.Subscriptions.Add(subscription);
				context.SaveChanges();
			}

			return View();
		}
    }
}