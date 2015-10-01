using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GB.tnLabs.Web.Infrastructure
{
	public interface IUnitOfWork: IDisposable
	{
		int ActiveSubscriptionId { get; }

		UserIdentity CurrentIdentity { get; }

	}
}
