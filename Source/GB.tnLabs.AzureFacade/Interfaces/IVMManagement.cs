using GB.tnLabs.AzureFacade.AzureSerializationModels;
using GB.tnLabs.AzureFacade.Enums;
using GB.tnLabs.AzureFacade.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.AzureFacade.Interfaces
{
    public interface IVMManagement: IDisposable
    {
		SubscriptionDetails GetSubscriptionDetails();

        Dictionary<string, PowerStatesEnum> GetVmState(string serviceName);

        List<VirtualMachineImageModel> GetAvailableTemplateImages(VmImageSourceEnum imageSource,
			List<string> preferredImageFamilies = null);

		/// <summary>
		/// Generates a single VM use the serviceBaseName as a base, and appending a random number to generate a
		/// name that is available.
		/// </summary>
		/// <param name="serviceBaseName">A service name base, of maximum 10 characters.</param>
		/// <param name="sourceVhdName">The configuration for the VM.</param>
		/// <param name="user">User details.</param>
		/// <returns>The details of the created VM service.</returns>
		AssignedVmModel GenerateVm(string serviceBaseName, VMConfigModel vmConfig, VMUserModel user);

        void DeleteService(string serviceName);

        void ShutdownVM(string serviceName, string deploymentName, string vmName);

        void StartVM(string serviceName, string deploymentName, string vmName);

		List<AssignedVmModel> GenerateVMsForUsers(string serviceNameBase, VMConfigModel vmConfig, List<VMUserModel> users);

		string CaptureVM(string serviceName, string vmName, string vmLabel);
    }
}
