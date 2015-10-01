using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.Core.SBDtos
{
	public class EndSessionRequest
	{
		public int SessionId { get; set; }

		public Guid Version { get; set; }
	}
}
