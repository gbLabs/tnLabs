using GB.tnLabs.Core.Repository;
using GB.tnLabs.Web.Infrastructure;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace GB.tnLabs.Web.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            // Add custom user claims here

            ClaimsPrincipal claimsPrincipal = Thread.CurrentPrincipal as ClaimsPrincipal;
            //HttpContext.Current.User.Identity.

            //TODO: look-up the roles from the DB!

            userIdentity.AddClaim(new Claim(ClaimTypes.Role, RoleTypes.User));

            userIdentity.AddClaim(new Claim(ClaimTypes.Role, RoleTypes.Member));

            string userId = userIdentity.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;

            ApplicationDbContext context = new ApplicationDbContext();
            Identity identity = context.Identities.Single(x => x.NameIdentifier == userId);

            userIdentity.AddClaim(new Claim(SpecialClaimTypes.UserId, identity.IdentityId.ToString(), ClaimValueTypes.Integer));

            //get first found subscription
            //TODO: update the databasee to be i nline with the entity model code first
            int subscriptionId = 9;// identity.SubscriptionIdentityRoles.First().SubscriptionId;
            //PersistUserSubscription(subscriptionId);



            return userIdentity;
        }

        private static void PersistUserSubscription(int subscriptionId)
        {
            var subscriptionCookie = new HttpCookie(SpecialClaimTypes.Subscription, subscriptionId.ToString());
            HttpContext.Current.Response.Cookies.Add(subscriptionCookie);
        }
    }
}
