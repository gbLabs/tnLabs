using GB.tnLabs.Web.Infrastructure;
using System;
using System.Collections.Generic;
//using System.IdentityModel.Services;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace GB.tnLabs.Web.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
			if (!HttpContext.User.IsInRole(RoleTypes.Trainer))
			{
				return RedirectToAction("Index", "SignUp");
			}

			return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {

            //TODO: rewrite
            //FederatedAuthentication.SessionAuthenticationModule.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}