using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GB.tnLabs.Web.Models
{
	public class SubscriptionCreateModel
	{
		[Display(Name = "Subscription ID")]
		public string AzureSubscriptionId { get; set; }

		public string CertificateKey { get; set; }

		public string BlobStorageName { get; set; }

		public string BlobStorageKey { get; set; }
	}
}