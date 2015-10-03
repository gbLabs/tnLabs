using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;

namespace GB.tnLabs.Web.Infrastructure
{
	public class UnitOfWork : IUnitOfWork
	{
		private UserIdentity _userIdentity;

		private readonly Dictionary<string, object> _containerDict;

		public int ActiveSubscriptionId
		{
			get
			{
				return CurrentIdentity.ActiveSubscriptionId;
			}
		}

		public UserIdentity CurrentIdentity
		{
			get
			{
				if (_userIdentity != null && 
					_userIdentity.ActiveSubscriptionId != 0 &&
					_userIdentity.IdentityId != 0)
				{
					return _userIdentity;
				}

				ClaimsPrincipal claimsPrincipal = Thread.CurrentPrincipal as ClaimsPrincipal;;

				if (claimsPrincipal == null || claimsPrincipal.Identity == null || !claimsPrincipal.Identity.IsAuthenticated)
				{

					return null;
				}
				
				//update _userIdentity if instantiated
				UserIdentity userIdentity = _userIdentity ?? new UserIdentity();

				//TODO: figure out where to keep the current subscription information
				//if (subscriptionIdCookie != null)
				//{
				//	userIdentity.ActiveSubscriptionId = int.Parse(subscriptionIdCookie.Value);
				//}
    //            else
                {
                    Claim subscriptionClaim = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == SpecialClaimTypes.Subscription);

                    if (subscriptionClaim != null)
                    {
                        string subscriptionId = subscriptionClaim.Value;

                        ////set the subscription if not selected
                        //var subscriptionCookie = new HttpCookie(SpecialClaimTypes.Subscription, subscriptionId);
                        //HttpContext.Current.Response.Cookies.Add(subscriptionCookie);

                        userIdentity.ActiveSubscriptionId = int.Parse(subscriptionId);
                    }
                }

				string identityId = claimsPrincipal.Claims.Single(x => x.Type == SpecialClaimTypes.UserId).Value;
				userIdentity.IdentityId = int.Parse(identityId);

				_userIdentity = userIdentity;

				return _userIdentity;
			}
		}

		public UnitOfWork()
		{
			_containerDict = new Dictionary<string, object>();
		}

		/// <summary>
		/// Add item to the UnitOfWork container.
		/// </summary>
		/// <param name="item"></param>
		public void Add(string key, object value)
		{
			_containerDict.Add(key, value);
		}

		public void Revert()
		{
			foreach (object item in _containerDict.Values)
			{
				DbTransaction transaction = item as DbTransaction;
				if (transaction != null) transaction.Rollback();
			}
		}

		public void Commit()
		{
			foreach (object item in _containerDict.Values)
			{
				DbContext context = item as DbContext;
				context.SaveChanges();
			}

			foreach (object item in _containerDict.Values)
			{
				DbTransaction transaction = item as DbTransaction;
				if (transaction != null) transaction.Commit();
			}
		}

		public void Dispose()
		{
			foreach (object item in _containerDict.Values)
			{
				IDisposable disposable = item as IDisposable;
				disposable.Dispose();
			}
		}
	}
}