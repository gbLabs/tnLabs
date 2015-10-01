using Microsoft.WindowsAzure.Management.Compute.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.AzureFacade.Models
{
    public class VirtualMachineImageModel
    {

        public string Description { get; set; }

        public string FriendlyName { get; private set; }

        public string Name { get; private set; }

		public bool IsUserImage { get; private set; }

		internal VirtualMachineImageModel(VirtualMachineOSImageListResponse.VirtualMachineOSImage image)
        {
            Name = image.Name;
		    FriendlyName = image.ImageFamily ?? image.Label;
		    Description = image.Description;
			IsUserImage = image.Category == "User";
        }

        internal VirtualMachineImageModel(VirtualMachineVMImageListResponse.VirtualMachineVMImage image)
        {
            Name = image.Name;
            FriendlyName = image.Label;
            Description = image.DeploymentName;
			IsUserImage = image.Category == "User";
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", FriendlyName, Name);
        }
    }
}
