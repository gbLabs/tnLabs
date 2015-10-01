using GB.tnLabs.AzureFacade.Enums;
using GB.tnLabs.AzureFacade.Interfaces;
using GB.tnLabs.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.Core.SBDtos
{
	public class VMReadyForCaptureRequest
	{
		public int TemplateVMId { get; set; }
	}
}
