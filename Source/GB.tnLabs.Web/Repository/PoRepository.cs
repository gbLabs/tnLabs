using GB.tnLabs.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GB.tnLabs.Web.Repository
{
	/// <summary>
	/// Plain old repository
	/// </summary>
	public class PoRepository
	{
		private readonly ApplicationDbContext _context;

		public PoRepository()
		{
			_context = new ApplicationDbContext();
		}

		public int? GetLocalIdentityId(string nameIdentifier, string identityProvider)
		{
			int? identityId = null;

			Identity foundIdentity = _context.Identities.SingleOrDefault(x => x.NameIdentifier == nameIdentifier /*&& x.IdentityProvider == identityProvider*/);
			if (foundIdentity != null)
			{
				identityId = foundIdentity.IdentityId;
			}

			return identityId;
		}

		public List<Tuple<int, string>> GetUserRoles(int userId)
		{
			return _context.SubscriptionIdentityRoles.Where(x => x.IdentityId == userId)
				.Select(x =>
					new
					{
						x.SubscriptionId,
						x.Role
					})
				.ToList()
				.Select(x => new Tuple<int, string>(x.SubscriptionId, x.Role))
				.ToList();
		}
	}
}