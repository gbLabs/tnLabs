using Castle.Core.Logging;
using GB.tnLabs.AzureFacade.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GB.Utils.Extensions;
using GB.tnLabs.AzureFacade.Models;
using Microsoft.WindowsAzure.Management.Compute.Models;
using GB.tnLabs.AzureFacade.Enums;
using GB.Utils;

namespace GB.tnLabs.AzureFacade.Stubs
{
	public class VMManagementStub : IVMManagement
	{
		#region private fields

		private readonly ILogger _logger;

		#endregion private fields

		#region .ctor

		public VMManagementStub(ILogger logger)
		{
			_logger = logger;
		}

		#endregion .ctor


		#region interface implementation

		public SubscriptionDetails GetSubscriptionDetails()
		{
			return new SubscriptionDetails
			{
				AvailableCoreCount = 20,
				AvailableServiceCount = 20,
				MaxCoreCount = 20,
				MaxServiceCount = 20
			};
		}

		public Dictionary<string, Enums.PowerStatesEnum> GetVmState(string serviceName)
		{
			_logger.Info("Entering GetVmState stub. [serviceName:{0}]", serviceName);
			Thread.Sleep(1000);
			_logger.Info("Exiting GetVmState stub. [serviceName:{0}]", serviceName);

			return new Dictionary<string, PowerStatesEnum> { { serviceName, Enums.PowerStatesEnum.Starting } };
		}

		public List<Models.VirtualMachineImageModel> GetAvailableTemplateImages(VmImageSourceEnum imageSource,
			List<string> preferredImageFamilies = null)
		{

			_logger.Info("Entering GetAvailableTemplateImages stub.");
			List<VirtualMachineImageModel> result = new List<VirtualMachineImageModel>();
			VirtualMachineImageModel vmImage1 = new VirtualMachineImageModel(new VirtualMachineOSImageListResponse.VirtualMachineOSImage
				{
					Name = "VM_Image_1"
				});

			VirtualMachineImageModel vmImage2 = new VirtualMachineImageModel(new VirtualMachineOSImageListResponse.VirtualMachineOSImage
			{
				Name = "VM_Image_2"
			});

			result.Add(vmImage1);
			result.Add(vmImage2);

			_logger.Info("Exiting GetAvailableTemplateImages stub.");

			return result;
		}

		public void DeleteService(string serviceName)
		{
			_logger.Info("Entering DeleteVM stub. [serviceName:{0}]", serviceName);
			Thread.Sleep(1000);
			_logger.Info("Exiting DeleteVM stub. [serviceName:{0}]", serviceName);
		}

		public void ShutdownVM(string serviceName, string deploymentName, string vmName)
		{
			_logger.Info("Entering ShutdownVM stub. [serviceName:{0}; deploymentName:{1}; vmName:{2}]",
				serviceName, deploymentName, vmName);
			Thread.Sleep(1000);
			_logger.Info("Exiting ShutdownVM stub. [serviceName:{0}; deploymentName:{1}; vmName:{2}]",
				serviceName, deploymentName, vmName);
		}

		public void StartVM(string serviceName, string deploymentName, string vmName)
		{
			_logger.Info("Entering StartVM stub. [serviceName:{0}; deploymentName:{1}; vmName:{2}]",
				serviceName, deploymentName, vmName);
			Thread.Sleep(1000);
			_logger.Info("Exiting StartVM stub. [serviceName:{0}; deploymentName:{1}; vmName:{2}]",
				serviceName, deploymentName, vmName);
		}

		public List<Models.AssignedVmModel> GenerateVMsForUsers(string serviceNameBase, VMConfigModel vmConfig, List<Models.VMUserModel> users)
		{
			_logger.Info("Entering GenerateVMsForUsers stub. [serviceNameBase:{0}; sourceVhdName:{1}; users:{2}]",
				serviceNameBase, vmConfig.ImageName, users.Count);

			List<AssignedVmModel> result = new List<AssignedVmModel>();
			foreach (VMUserModel user in users)
			{
				AssignedVmModel assignedVm = new AssignedVmModel
				{
					UserId = user.IdentityId,
					UserName = user.Username,
					Password = user.Password,
					VmName = "VM_" + user.Username,
					VmRdpPort = 567
				};
				result.Add(assignedVm);
			}
			Thread.Sleep(1000);

			_logger.Info("Exiting GenerateVMsForUsers stub. [serviceNameBase:{0}; sourceVhdName:{1}; users:{2}]",
				serviceNameBase, vmConfig.ImageName, users.Count);
			return result;
		}

		public string CaptureVM(string serviceName, string vmName, string vmLabel)
		{
			RandomProvider rand = new RandomProvider();
			string targetImageName = Misc.GetSafeString(vmLabel) + "_" + rand.AlphaNumeric(5);
			return targetImageName;
		}

		public void Dispose()
		{

		}

		#endregion interface implementation

		//TODO: stub
		public AssignedVmModel GenerateVm(string serviceBaseName, VMConfigModel vmConfig, VMUserModel user)
		{
			throw new NotImplementedException();
		}
	}
}
