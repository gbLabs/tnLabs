using Castle.Core.Logging;
using GB.tnLabs.Core;
using GB.tnLabs.Core.Components;
using GB.tnLabs.Core.Repository;
using GB.tnLabs.Core.SBDtos;
using GB.tnLabs.Web.Enums;
using GB.tnLabs.Web.Infrastructure;
using GB.tnLabs.Web.Models;
using GB.tnLabs.Web.Repository;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace GB.tnLabs.Web.Controllers
{
    public class SignUpController : Controller
    {
		#region private fields

		private ILogger _logger = NullLogger.Instance;
		private readonly Lazy<IUnitOfWork> _unitOfWorkLazy;
		private Lazy<ServiceBusMessageHandler> _messagingLazy;

        #endregion private fields

		#region properties

		protected IUnitOfWork UnitOfWork { get { return _unitOfWorkLazy.Value; } }

		protected ServiceBusMessageHandler Messaging { get { return _messagingLazy.Value; } }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        #endregion properties

        #region .ctor

        public SignUpController(ILogger logger, Lazy<IUnitOfWork> unitOfWorkLazy, 
			Lazy<ServiceBusMessageHandler> messagingLazy)
		{
			_logger = logger;
			_unitOfWorkLazy = unitOfWorkLazy;
            _messagingLazy = messagingLazy;
		}

        #endregion .ctor

		//[HttpPost]
		//public async Task<ActionResult> Index(ExternalLoginConfirmationViewModel model)
		//{
  //          if (!TryValidateModel(model))
		//	{
		//		return View(model);
		//	}

		//	tnLabsDBEntities context = new tnLabsDBEntities();

		//	string email = model.Email.Trim();
		//	if (context.Identities.Any(x=>x.Email == email))
		//	{
		//		ModelState.AddModelError("Email", "This email is already in use.");

		//		return View(model);
		//	}

		//	//creat the new identity in the database

		//	ClaimsPrincipal claimsPrincipal = Thread.CurrentPrincipal as ClaimsPrincipal;
		//	ClaimsIdentity claimsIdentity = (ClaimsIdentity)claimsPrincipal.Identity;
		//	string identityProvider = claimsIdentity.Claims.Single(x => x.Type == "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider").Value;
		//	string nameIdentifier = claimsIdentity.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;

		//	Identity identity = new Identity
		//	{
		//		IdentityProvider = identityProvider,
		//		NameIdentifier = nameIdentifier,
		//		Email = model.Email,
		//		DisplayName = model.DisplayName,
		//		FirstName = model.FirstName,
		//		LastName = model.LastName
		//	};

		//	context.Identities.Add(identity);
		//	context.SaveChanges();
			
  //          Email.BuildSignUpEmail(identity).Send();


  //          //TODO: figure out what to do with this
		//CustomClaimsAuthenticationManager.PersistUserIdentity(identity.IdentityId);

		//	return RedirectToAction("Subscription");
			
		//}

		public ActionResult Subscription()
		{
			return View();
		}
		
		[HttpPost]
		public ActionResult Subscription(string option)
		{
			switch (option)
			{
				case "Yes!":
					return RedirectToAction("Index", "Home");
					break;
				case "Definetly!":
					//create subscription
					throw new NotImplementedException();
					break;
				case "Yep!":
					//link with existing azure subscription
					return RedirectToAction("OwnSubscription");
					break;
				default:
					throw new InvalidOperationException("Unexpected option: " + option);
			}
		}

		public ActionResult OwnSubscription()
		{
			return View();
		}

		[HttpPost]
		public ActionResult OwnSubscription(AzureSubscriptionModel model)
		{
			//TODO: check the azuer subscription id if has been already added to db

			Subscription subscription = new Core.Repository.Subscription
				{
					AzureSubscriptionId = model.AzureSubscriptionId,
					BlobStorageKey = "", // will be deleted in next iteration
					BlobStorageName= "", // will be deleted in next iteration
					Certificate = new byte[0],
					CertificateKey = "pending",
					Name = model.SubscriptionName
				};

			CreateSubscriptionIdentityMap(subscription, UnitOfWork.CurrentIdentity.IdentityId);

            //TODO: figure out what to do with this
			//CustomClaimsAuthenticationManager.PersistUserSubscription(subscription.SubscriptionId);

			//now send message to back-end to create a certificate and send it by email,
			//and assign it to the subscription
			AssignCertificateRequest assignCertificate = new AssignCertificateRequest
			{
				IdentityId = UnitOfWork.CurrentIdentity.IdentityId,
				SubscriptionId = subscription.SubscriptionId
			};

			Messaging.Send(assignCertificate);

			return View("Done", SignUpTypeEnum.OwnSubscription);
		}

		private void CreateSubscriptionIdentityMap(Subscription subscription, int identityId)
		{
			using (ApplicationDbContext context = new ApplicationDbContext())
			{
				CreateSubscriptionIdentityMap(subscription, context, RoleTypes.Administrator);
				CreateSubscriptionIdentityMap(subscription, context, RoleTypes.Member);
				CreateSubscriptionIdentityMap(subscription, context, RoleTypes.Owner);
				CreateSubscriptionIdentityMap(subscription, context, RoleTypes.User);

				context.SaveChanges();
			}
		}

		private void CreateSubscriptionIdentityMap(Subscription subscription, ApplicationDbContext context, string role)
		{
			SubscriptionIdentityRole subscriptionIdentity = new SubscriptionIdentityRole
			{
				Subscription = subscription,
				IdentityId = UnitOfWork.CurrentIdentity.IdentityId,
				Role = role
			};

			context.SubscriptionIdentityRoles.Add(subscriptionIdentity);
		}
    }
}