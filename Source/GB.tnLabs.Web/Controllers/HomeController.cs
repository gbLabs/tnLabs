using GB.tnLabs.Core.Repository;
using GB.tnLabs.Web.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace WebMvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly Lazy<IUnitOfWork> _unitOfWorkLazy;

        protected IUnitOfWork UnitOfWork
        {
            get { return _unitOfWorkLazy.Value; }
        }
        public HomeController(Lazy<IUnitOfWork> unitOfWorkLazy)
        {
            _unitOfWorkLazy = unitOfWorkLazy;
        }

        public ActionResult Index()
        {
            ViewBag.IsSignedUser = false;
            ViewBag.IsMember = false;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                AssigUserSubscriptionRoles();

                using (var context = new ApplicationDbContext())
                {
                    var email = context.Identities.Where(x => x.IdentityId == UnitOfWork.CurrentIdentity.IdentityId).Select(x => x.Email).First();
                    var roles = Session[String.Format("Roles{0}", email)];
                    if (roles != null)
                    {
                        ViewBag.IsSignedUser = true;
                        ViewBag.IsMember = ((List<string>)roles).Contains(RoleTypes.Member);
                    }
                }                
            }
            return View();
        }

        private void AssigUserSubscriptionRoles()
        {
            using (var context = new ApplicationDbContext())
            {
                var roles = context.SubscriptionIdentityRoles.Where(x => x.IdentityId == UnitOfWork.CurrentIdentity.IdentityId && x.SubscriptionId == UnitOfWork.ActiveSubscriptionId).Select(y => y.Role).ToList();
                var email = context.Identities.Where(x => x.IdentityId == UnitOfWork.CurrentIdentity.IdentityId).Select(x => x.Email).First();
                Session[String.Format("Roles{0}", email)] = roles;
            }
        }
    }
}