using GB.tnLabs.Core.Repository;
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
        private readonly Lazy<IUnitOfWork> _unitOfWorkLazy;

        protected IUnitOfWork UnitOfWork
        {
            get { return _unitOfWorkLazy.Value; }
        }
        public TrainingController(Lazy<IUnitOfWork> unitOfWorkLazy)
        {
            _unitOfWorkLazy = unitOfWorkLazy;
        }

        // GET: Training
        public ActionResult Index()
        {
            using (var context = new ApplicationDbContext())
            {
                var email = context.Identities.Where(x => x.IdentityId == UnitOfWork.CurrentIdentity.IdentityId).Select(x => x.Email).First();
                var roles = Session[String.Format("Roles{0}", email)];
                if (roles != null)
                {
                    ViewBag.IsOwnerOrTrainer = ((List<string>)roles).Contains(RoleTypes.Owner) || ((List<string>)roles).Contains(RoleTypes.Trainer);
                }
            }
            return View();
        }
    }
}