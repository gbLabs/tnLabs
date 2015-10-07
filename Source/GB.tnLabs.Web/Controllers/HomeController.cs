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
		public ActionResult Index()
		{
			ViewBag.IsSignedUser = HttpContext.User.IsInRole(RoleTypes.Trainer);
			ViewBag.IsMember = HttpContext.User.IsInRole(RoleTypes.Member);
			return View();
		}
	}
}