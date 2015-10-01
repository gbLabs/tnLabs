using Castle.Core.Logging;
using GB.tnLabs.AzureFacade.AzureSerializationModels;
using GB.tnLabs.AzureFacade.Enums;
using GB.tnLabs.AzureFacade.Interfaces;
using GB.tnLabs.AzureFacade.Models;
using GB.Utils;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Management;
using Microsoft.WindowsAzure.Management.Compute;
using Microsoft.WindowsAzure.Management.Compute.Models;
using Microsoft.WindowsAzure.Management.Models;
using Microsoft.WindowsAzure.Management.Storage;
using Microsoft.WindowsAzure.Management.Storage.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GB.tnLabs.AzureFacade
{
    public class VMManagement : IVMManagement
    {
        #region constants

        private const int RdpPortBase = 58781;
        private const int InternalRdpPort = 3389;

        #endregion constants

        #region private fields

        private ILogger _logger = NullLogger.Instance;

        private string _subscriptionId;
        
        private readonly ManagementClient _datacenter;
        private readonly ComputeManagementClient _compute;
		private readonly StorageManagementClient _storage;

        private X509Certificate2 _managementCertificate;

        #endregion private fields

        #region .ctor

		public VMManagement(string subscriptionId, byte[] managementCertificate,
			string certificateKey, string blobStorageName, string blobStorageKey, ILogger logger)
		{
			_managementCertificate = new X509Certificate2(managementCertificate, certificateKey, X509KeyStorageFlags.MachineKeySet);

			_subscriptionId = subscriptionId;

			CertificateCloudCredentials cloudCredentials = new CertificateCloudCredentials(_subscriptionId, _managementCertificate);

            _datacenter = new ManagementClient(cloudCredentials);// CloudContext.Clients.CreateManagementClient(cloudCredentials);

            //var roleSizes = _datacenter.RoleSizes.List();
            _compute = new ComputeManagementClient(cloudCredentials);// CloudContext.Clients.CreateComputeManagementClient(cloudCredentials);
            _storage = new StorageManagementClient(cloudCredentials);// CloudContext.Clients.CreateStorageManagementClient(cloudCredentials);
		}

        #endregion .ctor

		#region interface implementation

		public SubscriptionDetails GetSubscriptionDetails()
		{
			SubscriptionGetResponse subscription = _datacenter.Subscriptions.Get();

			SubscriptionDetails result = new SubscriptionDetails
			{
				AvailableCoreCount = subscription.MaximumCoreCount - subscription.CurrentCoreCount,
				MaxCoreCount = subscription.MaximumCoreCount,
				AvailableServiceCount = subscription.MaximumHostedServices - subscription.CurrentHostedServices,
				MaxServiceCount = subscription.MaximumHostedServices
			};

			return result;
		}

		public Dictionary<string, PowerStatesEnum> GetVmState(string serviceName)
		{
			var vms = _compute.HostedServices.GetDetailed(serviceName).Deployments.First().RoleInstances.ToList();

			return vms.ToDictionary(x => x.RoleName,
				y => (PowerStatesEnum)Enum.Parse(typeof(PowerStatesEnum), y.PowerState.ToString()));
		}

        public void DeleteService(string serviceName)
        {
            List<Exception> exceptionList = new List<Exception>();

            try
            {
				bool serviceExists = _compute.HostedServices.List()
					.Any(x => string.Equals(x.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase));

				if (!serviceExists) return;

				_compute.HostedServices.DeleteAll(serviceName);
            }
            catch (Exception ex)
            {
                exceptionList.Add(new Exception(string.Format("Error when trying to delete Hosted Service: {0}", serviceName), ex));
            }

            if (exceptionList.Count > 0)
            {
                throw new AggregateException("Failed to completely delete the VM realted resources.", exceptionList);
            }
        }

        public void ShutdownVM(string serviceName, string deploymentName, string vmName)
        {

            _compute.VirtualMachines.Shutdown(serviceName, deploymentName, vmName, new VirtualMachineShutdownParameters
                {
                    PostShutdownAction = PostShutdownAction.Stopped
                });
        }

        public void StartVM(string serviceName, string deploymentName, string vmName)
        {
            _compute.VirtualMachines.Start(serviceName, deploymentName, vmName);
        }

        //we'll use for when we'll be creating the reference VM
        /// <summary>
        /// Gets available pre configured Azure templates.
        /// </summary>
        public List<VirtualMachineImageModel> GetAvailableTemplateImages(VmImageSourceEnum imageSource, 
			List<string> preferredImageFamilies = null)
        {
			//TODO: use the imageSource to provide the list of images from the gallery or from the storage
			IEnumerable<VirtualMachineOSImageListResponse.VirtualMachineOSImage> images = _compute.VirtualMachineOSImages.List();
			IEnumerable<VirtualMachineOSImageListResponse.VirtualMachineOSImage> userImages = images.Where(x => x.Category == "User");
			var galleryImages = images.Where(x => x.Category != "User" && !(x.IsPremium ?? false))
				.GroupBy(x => x.ImageFamily ?? x.Label)
                    .Select(x => x.OrderByDescending(y => y.PublishedDate).First());

			if (preferredImageFamilies != null && preferredImageFamilies.Any())
			{
				galleryImages = galleryImages.Where(x => preferredImageFamilies.Contains(x.ImageFamily));
			}
			List<VirtualMachineOSImageListResponse.VirtualMachineOSImage> selectedList;
            if (imageSource == VmImageSourceEnum.Storage)
            {
				selectedList = userImages.ToList();
            }
            else if (imageSource == VmImageSourceEnum.Gallery)
            {
				selectedList = galleryImages.ToList();
            }
			else 
			{
				selectedList = userImages.Union(galleryImages).ToList();
			}

			var result = selectedList.Select(image => new VirtualMachineImageModel(image)).ToList();
            return result;
        }

		/// <summary>
		/// Generates a single VM use the serviceBaseName as a base, and appending a random number to generate a
		/// name that is available.
		/// </summary>
		/// <param name="serviceBaseName">A service name base, of maximum 10 characters.</param>
		/// <param name="sourceVhdName">The configuration for the VM.</param>
		/// <param name="user">User details.</param>
		/// <returns>The details of the created VM service.</returns>
		[Obsolete("GenerateVMsForUsers must be used instead", true)]
		public AssignedVmModel GenerateVm(string serviceBaseName, VMConfigModel vmConfig, VMUserModel user)
		{
			//TODO: refactor GenerateVMsForUsers to be used instead of this one

			if (serviceBaseName.Length > 10) throw new ArgumentOutOfRangeException("serviceBaseName can't have more than 10 characters.");

			//find a name that is available
			Random r = new Random();
			string serviceName = null;
			HostedServiceCheckNameAvailabilityResponse availableResponse;

			do
			{
				int randomValue = r.Next(99999);
				serviceName = serviceBaseName + randomValue;

				availableResponse = _compute.HostedServices.CheckNameAvailability(serviceName);

			} while (!availableResponse.IsAvailable);

			var createCloudService = _compute.HostedServices.Create(
				new HostedServiceCreateParameters
				{
					ServiceName = serviceName,
					Location = vmConfig.Region
				});

			string virtualMachineName = serviceName;

			//TODO: need a status check
			GenerateVM(serviceName, vmConfig, virtualMachineName, RdpPortBase, user,
				firstVMInDeployment: true);
			AssignedVmModel assignedVm = new AssignedVmModel
			{
				UserId = user.UserId,
				UserName = user.Username,
				VmName = virtualMachineName,
				VmRdpPort = RdpPortBase,
				Password = user.Password
			};

			return assignedVm;
		}

        /// <summary>
        /// TODO: Summary in work....
        /// </summary>
        /// <param name="serviceName">A new service name.</param>
        /// <param name="sourceVhdName">The original Azure image 
        /// name from which the reference VM has been created.</param>
        /// <param name="users">The list of users that need VMs.</param>
		public List<AssignedVmModel> GenerateVMsForUsers(string serviceName, VMConfigModel vmConfig,
			List<VMUserModel> users)
        {
            _compute.HostedServices.Create(new HostedServiceCreateParameters
                {
                    ServiceName = serviceName,
                    Location = LocationNames.NorthEurope
                });

			int vmIndex = 0;
			int rdpPortIndex = RdpPortBase;
			List<AssignedVmModel> assignedVms = new List<AssignedVmModel>();
			List<Task> taskList = new List<Task>();

            foreach (VMUserModel user in users)
            {
				bool isFirstVmInDeployment = !assignedVms.Any();
				string virtualMachineName = serviceName + ++vmIndex;

				//TODO: use the async version of the method to avoid creating new Tasks
				//Task task = Task.Factory.StartNew(() => GenerateVM(serviceName, vmConfig, virtualMachineName,
				//	rdpPortIndex, user, isFirstVmInDeployment));
				GenerateVM(serviceName, vmConfig, virtualMachineName,
					rdpPortIndex, user, isFirstVmInDeployment);

				//if it's the first VM, then we must wait till the Deployment is set-up so we can
				//add to it the rest of the VMs
				//if (isFirstVmInDeployment) task.Wait(); else taskList.Add(task);

				assignedVms.Add(new AssignedVmModel
				{
					UserId = user.UserId,
					UserName = user.Username,
					VmName = virtualMachineName,
					VmRdpPort = rdpPortIndex,
					Password = user.Password
				});

				rdpPortIndex++;
            }
			
			//Task.WaitAll(taskList.ToArray());

			return assignedVms;
        }

		/// <returns>The VM image name</returns>
		public string CaptureVM(string serviceName, string vmName, string vmLabel)
		{
			RandomProvider rand = new RandomProvider();
			string targetImageName = Misc.GetSafeString(vmLabel) + "_" + rand.AlphaNumeric(5);

			//TODO: check that the name is valid and available

			_compute.VirtualMachines.CaptureOSImage(serviceName, serviceName, vmName,
				new VirtualMachineCaptureOSImageParameters
				{
					//the original VM will be deleted after capture
					PostCaptureAction = PostCaptureAction.Delete,
					TargetImageLabel = vmLabel,
					TargetImageName = targetImageName
				});

			return targetImageName;
		}

		public void Dispose()
		{
			_compute.Dispose();
			_datacenter.Dispose();
		}

		#endregion interface implementation

		#region private methods

		private void GenerateVM(string serviceName, VMConfigModel vmConfig, string virtualMachineName, 
			int rdpPort, VMUserModel user, bool firstVMInDeployment)
        {
            var windowsConfigurationSet = new ConfigurationSet
            {
                //TODO: depends on the OS type??
                ConfigurationSetType = ConfigurationSetTypes.WindowsProvisioningConfiguration,
                AdminPassword = user.Password,
                AdminUserName = user.Username,
                ComputerName = virtualMachineName,
                HostName = string.Format("{0}.cloudapp.net", serviceName),
                
            };

            var endpoints = new ConfigurationSet
            {
                ConfigurationSetType = "NetworkConfiguration",
                InputEndpoints = new List<InputEndpoint>
                {
                    new InputEndpoint
                    {
                        Name = "RDP",
                        Port = rdpPort,
                        Protocol = "TCP",
                        LocalPort = InternalRdpPort
                    }
                }
            };

            string newVhdName = string.Format("{0}.vhd", virtualMachineName);

			Uri mediaUri = new Uri(GetVHDStorageUrl(vmConfig.Region) + newVhdName);

            var vhd = new OSVirtualHardDisk
            {
                SourceImageName = vmConfig.ImageName,
                HostCaching = VirtualHardDiskHostCaching.ReadWrite,
				MediaLink = mediaUri,
                
            };

            OperationStatusResponse deploymentResult;
			
			if (firstVMInDeployment)
			{
				deploymentResult = CreateDeployment(serviceName, virtualMachineName, windowsConfigurationSet, endpoints, vhd, vmConfig);
			
			}
			else
			{
				deploymentResult = AddVm(serviceName, virtualMachineName, windowsConfigurationSet, endpoints, vhd, vmConfig);
			}
            //TODO: handle the deploymentResult
        }

		private OperationStatusResponse AddVm(string serviceName, string virtualMachineName, ConfigurationSet windowsConfigurationSet, ConfigurationSet endpoints, OSVirtualHardDisk vhd, VMConfigModel vmConfig)
		{
			OperationStatusResponse deploymentResult;
			var createVMParameters = new VirtualMachineCreateParameters
			{
				
				RoleName = virtualMachineName,
				RoleSize = MapToAzureVmSize(vmConfig.VmSize),
				OSVirtualHardDisk = vhd,
				ProvisionGuestAgent = true,
				ConfigurationSets = new List<ConfigurationSet>
					{
						windowsConfigurationSet,
						endpoints,
					},
            };

            if (vmConfig.InsertCaptureScript)
            {
				createVMParameters.ResourceExtensionReferences = 
					CreateCaptureScript(vmConfig.CaptureKey, vmConfig.ChocoPackageList);
            }

			deploymentResult = _compute.VirtualMachines.Create(serviceName, serviceName, createVMParameters);
			return deploymentResult;
		}

		private OperationStatusResponse CreateDeployment(string serviceName, string virtualMachineName, ConfigurationSet windowsConfigurationSet, ConfigurationSet endpoints, OSVirtualHardDisk vhd, VMConfigModel vmConfig)
		{
			OperationStatusResponse deploymentResult;
            var role = new Role
            {
                RoleName = virtualMachineName,
				RoleSize = MapToAzureVmSize(vmConfig.VmSize),
                RoleType = VirtualMachineRoleType.PersistentVMRole.ToString(),
                OSVirtualHardDisk = vhd,
                ProvisionGuestAgent = true,
                ConfigurationSets = new List<ConfigurationSet>
                {
                    windowsConfigurationSet,
                    endpoints,
                }
            };

			if (vmConfig.InsertCaptureScript)
			{
				role.ResourceExtensionReferences = 
					CreateCaptureScript(vmConfig.CaptureKey, vmConfig.ChocoPackageList);
			}

            var createDeploymentParameters = new VirtualMachineCreateDeploymentParameters
            {
				Name = serviceName,
				Label = serviceName,
                DeploymentSlot = DeploymentSlot.Production,
                Roles = new List<Role> { role },
                
            };

			deploymentResult = _compute.VirtualMachines.CreateDeployment(
                serviceName,
                createDeploymentParameters);
			return deploymentResult;
        }

		private string GetVHDStorageUrl(string location)
		{
			string storageAccount = GetVHDStorage(location);
			return string.Format("http://{0}.blob.core.windows.net/vhds/", storageAccount);
		}

		/// <summary>
		/// Will find an portalvhds storage accound for the location provided. If such a storage account is
		/// not created, it will create one.
		/// </summary>
		/// <param name="location">The region where for the storage account.</param>
		/// <returns>The name of the storage account.</returns>
		private string GetVHDStorage(string location)
		{
			StorageAccountListResponse response = _storage.StorageAccounts.List();
            //TODO: this logic is wrong, because the vhds for the vms should be made in the same storage as the source image
            StorageAccount foundStorageAccoutn = 
                //first try the "tnlabs" for the storage account
                response.FirstOrDefault(x => x.Name.StartsWith("tnlabs") && x.Properties.Location == location)
                //then try any that starts with "portalvhds"
                ?? response.FirstOrDefault(x => x.Name.StartsWith("portalvhds") && x.Properties.Location == location);

			string foundStorageName;

			if (foundStorageAccoutn == null)
			{
				string storageName = GetAvailableVHDStorageAccountName();
				
				//TODO: handle the response
				OperationStatusResponse createResponse = _storage.StorageAccounts
					.Create(new StorageAccountCreateParameters
						{
							Location = location,
							Name = storageName
						});

				StorageAccountGetKeysResponse keys = _storage.StorageAccounts.GetKeys(storageName);

				string storageConnection = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", storageName, keys.SecondaryKey);
				CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnection);
				CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
				CloudBlobContainer container = blobClient.GetContainerReference("vhds");
				container.CreateIfNotExists();
				
				foundStorageName = storageName;
			}
			else
			{
				foundStorageName = foundStorageAccoutn.Name;
			}

			return foundStorageName;
		}

		private string GetAvailableVHDStorageAccountName()
		{
			RandomProvider rand = new RandomProvider();

			string storageName = null;
			do
			{
				storageName = "portalvhds" + rand.AlphaNumeric(13);
			} while (!_storage.StorageAccounts.CheckNameAvailability(storageName).IsAvailable);

			return storageName;
		}

		private IList<ResourceExtensionReference> CreateCaptureScript(string guid, List<string> chocoPackageList)
		{
			//TODO: crete some ioc resolved class with methods related to environment
			string apiBase = ConfigurationManager.AppSettings["apiCallback"];
			if (apiBase.Last() != '/') apiBase += '/';
			string callbackForReadyToCapture = apiBase + "azuremanagement/readyforcapture?templatevmkey=" + guid;
			string callbackForVmProvisioned = apiBase + "azuremanagement/basevmprovisionedwithextensions?templatevmkey=" + guid;

			string insertedScripts = GetCustomScriptExtensionValue(callbackForReadyToCapture, callbackForVmProvisioned, chocoPackageList);
			
			var resourceExtensionReferences = new List<ResourceExtensionReference>
                {
                    new ResourceExtensionReference
                    {
                        ReferenceName = "BGInfo",
                        Publisher = "Microsoft.Compute",
                        Name = "BGInfo",
                        Version = "1.1"
                    },
                    new ResourceExtensionReference
                    {
                        ReferenceName = "CustomScriptExtension",
                        Publisher = "Microsoft.Compute",
                        Name = "CustomScriptExtension",
                        Version = "1.0",
                        State = "Enable",
                        ResourceExtensionParameterValues = new List<ResourceExtensionParameterValue>
                        {
                            new ResourceExtensionParameterValue
                            {
                                Key = "CustomScriptExtensionPublicConfigParameter",
                                Value = insertedScripts,
                                Type = "Public"
                            }
                        }
                    }
                };

			return resourceExtensionReferences;
		}

		private string GetCustomScriptExtensionValue(string callbackForReadyToCapture, 
			string callbackForVmProvisioned, List<string> chocoPackageList)
        {
			string chocoPackageArg = chocoPackageList != null && chocoPackageList.Any() ?
				" " + string.Join(",", chocoPackageList) : string.Empty;

			var template = new
			{
				fileUris = new List<string> {
					"http://portalvhds3r84wq5g3925q.blob.core.windows.net/scripts/Capture.ps1",
					"http://portalvhds3r84wq5g3925q.blob.core.windows.net/scripts/CreateImage.ps1",
					"http://portalvhds3r84wq5g3925q.blob.core.windows.net/scripts/Capture.ico"
				},
				commandToExecute = "powershell -ExecutionPolicy Unrestricted -file CreateImage.ps1 " +
					callbackForReadyToCapture + " " + callbackForVmProvisioned + chocoPackageArg
			};

			return JsonConvert.SerializeObject(template, Formatting.Indented);
        }

		private string MapToAzureVmSize(VmSizeEnum vmSize)
		{
			switch (vmSize)
			{
				case VmSizeEnum.Small:
					return "Basic_A1";
				case VmSizeEnum.Medium:
					return "Basic_A2";
				case VmSizeEnum.Large:
					return "Basic_A3";
				default:
					throw new InvalidOperationException();
			}
		}

		#endregion private methods
    }
}
