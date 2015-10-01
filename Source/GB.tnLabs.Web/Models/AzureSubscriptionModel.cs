using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GB.tnLabs.Web.Models
{
	public class AzureSubscriptionModel
	{
		[Required]
		[Display(Name = "Azure Subscription ID")]
		public string AzureSubscriptionId { get; set; }

		[Required]
		[Display(Name = "Subscription name (for your reference)")]
		public string SubscriptionName { get; set; }
	}
}