using GB.tnLabs.AzureFacade.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.AzureFacade.Models
{
	public class VMConfigModel
	{
		public string ImageName { get; private set; }

        public string Region { get; private set; }

		public VmSizeEnum VmSize { get; set; }

		// Insert Capture Script
        public bool InsertCaptureScript { get; set; }

        public string CaptureKey { get; set; }

		public List<string> ChocoPackageList { get; set; }
		// END insert capture script

		public VMConfigModel()
		{
			VmSize = VmSizeEnum.Medium;
		}

		public VMConfigModel(string imageName, string region) : this()
		{
			ImageName = imageName;
			Region = region;
		}



		
	}
}
