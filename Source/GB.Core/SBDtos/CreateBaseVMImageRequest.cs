using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.Core.SBDtos
{
	public class CreateBaseVMImageRequest
	{
		public string ImageName { get; set; }

		public string ImageDescription { get; set; }

		public string NewImageName { get; set; }

		public int UserId { get; set; }

		public int SubscriptionId { get; set; }

		public List<string> ChocoPackageList { get; set; }
	}
}
