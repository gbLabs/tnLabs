using Castle.Core.Logging;
using GB.tnLabs.AzureFacade.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.AzureFacade
{
	public class AzureFacadeFactory
	{
		private readonly ILogger _logger;

		public AzureFacadeFactory(ILogger logger)
		{
			_logger = logger;
		}

		public IVMManagement VMManagement(string subscriptionId, byte[] managementCertificate,
			string certificateKey, string blobStorageName, string blobStorageKey)
		{
			return new VMManagement(subscriptionId, managementCertificate, certificateKey, blobStorageName,
				blobStorageKey, _logger);
		}
	}
}
