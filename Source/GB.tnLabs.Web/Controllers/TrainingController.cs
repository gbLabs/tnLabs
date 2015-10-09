using GB.tnLabs.Web.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GB.tnLabs.Web.Controllers
{
    public class TrainingController : Controller
    {
        // GET: Training
        public ActionResult Index()
        {
            ViewBag.IsOwner = HttpContext.User.IsInRole(RoleTypes.Owner);
            return View();
        }
    }
}