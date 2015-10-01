//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
////using Microsoft.IdentityModel.Claims;
//using System.Security.Principal;
//using System.Security.Claims;
//using GB.tnLabs.Web.Repository;

//namespace GB.tnLabs.Web.Infrastructure
//{
//	public class CustomClaimsAuthenticationManager : ClaimsAuthenticationManager
//	{
//		public override ClaimsPrincipal Authenticate(string resourceName, ClaimsPrincipal incomingPrincipal)
//		{
//			ClaimsPrincipal principal = base.Authenticate(resourceName, incomingPrincipal);

//			if (principal != null && principal.Identity.IsAuthenticated == true)
//				{
//				ClaimsIdentity claimsIdentity = (ClaimsIdentity)principal.Identity;
//				string identityProvider = claimsIdentity.Claims.Single(x => x.Type == SpecialClaimTypes.IdentityProvider).Value;
//				string nameIdentifier = claimsIdentity.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
//                //we add user role to all authenticated members
//                ((ClaimsIdentity)principal.Identity).AddClaim(
//                      new Claim(ClaimTypes.Role, RoleTypes.User));

//				//check in the repository that it's an existing customer
//				PoRepository repository = new PoRepository();
//				int? identityId = repository.GetLocalIdentityId(nameIdentifier, identityProvider);

//				//DECIDE ON SOME CRITERIA IF CURRENT USER DESERVES THE ROLE
//				//IClaimsIdentity identity = (IClaimsIdentity)incomingPrincipal.Identity;
//				//principal.Identity.

//				if (identityId.HasValue)
//				{
					
//					((ClaimsIdentity)principal.Identity).AddClaim(
//						new Claim(SpecialClaimTypes.UserId, identityId.ToString(), ClaimValueTypes.Integer));

//					var userRoles = repository.GetUserRoles(identityId.Value);
//					foreach (var role in userRoles)
//					{
//						((ClaimsIdentity)principal.Identity).AddClaim(
//							new Claim(ClaimTypes.Role, role.Item2));
//					}



//                    //get the subscription id from roles tuples
//                    //all roles will be on the same subscription so it's enough to
//                    //take the firt tuple item.  
//                    //THIS MUST BE CHANGE WHEN USER WILL BE ABLE TO SELECT HIS SUBSCRIPTION
//				    if (userRoles.Count > 0)
//				    {
//				        PersistUserSubscription(userRoles.First().Item1);
//				    }
//				}
//			}
//			return principal;
//		}

//		//DO NOT USE INSIDE THIS CLASS! TODO: move elsewhere
//		public static void PersistUserIdentity(int identityId)
//		{
//			ClaimsPrincipal claimsPrincipal = System.Threading.Thread.CurrentPrincipal as ClaimsPrincipal;
//			((ClaimsIdentity)claimsPrincipal.Identity).AddClaim(
//				new Claim(SpecialClaimTypes.UserId, identityId.ToString(), ClaimValueTypes.Integer));
//		}

//	    public static void PersistUserSubscription(int subscriptionId)
//	    {
//	        var subscriptionCookie = new HttpCookie(SpecialClaimTypes.Subscription, subscriptionId.ToString());
//	        HttpContext.Current.Response.Cookies.Add(subscriptionCookie);
//        }
//	}
//}