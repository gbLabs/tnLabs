using System;
using System.Collections.Generic;
using System.Linq;
using Breeze.ContextProvider;
using GB.tnLabs.Web.Data;
using GB.tnLabs.Web.Repository;
using Newtonsoft.Json.Linq;
using Breeze.WebApi2;

namespace GB.tnLabs.Web.APIControllers
{
    using System.Web.Http;
    using GB.tnLabs.Web.Infrastructure;
    using GB.tnLabs.AzureFacade.Models;
    using GB.tnLabs.AzureFacade;
    using GB.tnLabs.AzureFacade.Interfaces;
    using GB.tnLabs.AzureFacade.Enums;
    using GB.tnLabs.Core.Repository;
    using GB.tnLabs.Core;
    using GB.tnLabs.Core.Components;
    using System.Reflection;

    [BreezeController]
    public class TrainingsController : ApiController
    { 
        private readonly LabRepository _repository;// = new LabRepository();

        private readonly Lazy<IUnitOfWork> _unitOfWorkLazy;
        private readonly Lazy<AzureFacadeFactory> _azureFacadeFactoryLazy;
		private readonly Lazy<ServiceBusMessageHandler> _messagingLazy;
		private readonly Lazy<SubscriptionService> _subscriptionService;

        protected IUnitOfWork UnitOfWork
        {
            get { return _unitOfWorkLazy.Value; }
        }

        protected AzureFacadeFactory AzureFacadeFactory
        {
            get { return _azureFacadeFactoryLazy.Value; }
        }

		protected ServiceBusMessageHandler Messaging
		{
			get { return _messagingLazy.Value; }
		}

		protected SubscriptionService SubscriptionService
		{
			get { return _subscriptionService.Value; }
		}

        public TrainingsController(Lazy<IUnitOfWork> unitOfWorkLazy, Lazy<AzureFacadeFactory> azureFacadeFactoryLazy,
			LabRepository repository, Lazy<ServiceBusMessageHandler> messagingLazy, Lazy<SubscriptionService> subscriptionService)
        {
            _unitOfWorkLazy = unitOfWorkLazy;
            _azureFacadeFactoryLazy = azureFacadeFactoryLazy;
			_repository = repository;
			_messagingLazy = messagingLazy;
			_subscriptionService = subscriptionService;
        }

        [HttpGet]
        public string Metadata()
        {
            return _repository.Metadata;
        }
        // GET api/quote
        [HttpGet]
        public IQueryable<Lab> Labs()
        {
			return _repository.Labs.Where(x => x.SubscriptionId == UnitOfWork.ActiveSubscriptionId); ;
        }

        // GET api/quote
        [HttpGet]
        public IQueryable<Session> Sessions()
        {
            return _repository.Sessions.Where(x=>x.SubscriptionId == UnitOfWork.ActiveSubscriptionId);
        }

        // GET api/quote
        [HttpGet]
        public IQueryable<SessionUser> SessionUsers()
        {
            var subscriptionIdentityRoles = _repository.SubscriptionIdentityRoles.Where(x => x.SubscriptionId == UnitOfWork.ActiveSubscriptionId);
            if(subscriptionIdentityRoles.Any())
            {
                var identities = subscriptionIdentityRoles.Select(i => i.IdentityId).Distinct();
                if(identities.Any())
                    return _repository.SessionUsers.Where(x => identities.ToList().Contains(x.IdentityId));
            }
            return new List<SessionUser>().AsQueryable();
        }

        [HttpGet]
        public IQueryable<Identity> Identities()
        {
            var subscriptionIdentityRoles = _repository.SubscriptionIdentityRoles.Where(x => x.SubscriptionId == UnitOfWork.ActiveSubscriptionId);
            if (subscriptionIdentityRoles.Any())
            {
                var identities = subscriptionIdentityRoles.Select(i => i.IdentityId).Distinct();
                if (identities.Any())
                    return _repository.Identities.Where(x => identities.ToList().Contains(x.IdentityId));
            }
            return new List<Identity>().AsQueryable();
        }

        [HttpGet]
        public IEnumerable<VirtualMachineImageModel> AvailableTemplateImages()
        {
            Subscription subscription;
            List<string> preferredVmFamilies;
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                subscription = context.Subscriptions.Single(x => x.SubscriptionId == UnitOfWork.ActiveSubscriptionId);

                preferredVmFamilies = context.RecommendedVMImages.Select(x => x.ImageFamily).ToList();
            }

            IVMManagement management = AzureFacadeFactory.VMManagement(subscription.AzureSubscriptionId,
                subscription.Certificate, subscription.CertificateKey, subscription.BlobStorageName,
                subscription.BlobStorageKey);


            List<VirtualMachineImageModel> vmImageList = management.GetAvailableTemplateImages(VmImageSourceEnum.All, preferredVmFamilies);

			// Filter the image list:
			// User images must check if are for the current subscription
			// also enrich the user images with a friendly description
			using (ApplicationDbContext context = new ApplicationDbContext())
			{
				int subscriptionId = UnitOfWork.ActiveSubscriptionId;
				List<TemplateVM> subscriptionTemplates = context.TemplateVMs
					.Where(x=>x.SubscriptionId == subscriptionId).ToList();

				List<string> subscriptionImageNames = subscriptionTemplates.Select(x=>x.ImageName).ToList();

				vmImageList = vmImageList.Where(x => !x.IsUserImage || subscriptionImageNames.Contains(x.Name))
					.ToList();

				foreach(VirtualMachineImageModel image in vmImageList.Where(x=>x.IsUserImage))
				{
					image.Description = subscriptionTemplates.Single(x => x.ImageName == image.Name).Description;
				}
			}

            return vmImageList;
        }

		[HttpGet]
		public IEnumerable<object> AvailableChocoPackages()
		{
			return new List<object>
			{
				new 
				{
					FriendlyName = "Chrome",
					PackageName = "GoogleChrome",
					Description = "Chrome Browser A modern web browser with an MSI and admin tools designed for organizations that need standardized, easy and secure deployment and management."
				},
				new
				{
					FriendlyName = "Mozilla Firefox",
					PackageName = "Firefox",
					Description = "Bringing together all kinds of awesomeness to make browsing better for you."
				},
				new
				{
					FriendlyName = "Notepad++",
					PackageName = "notepadplusplus.install",
					Description = "Notepad++ is a free (as in \"free speech\" and also as in \"free beer\") source code editor and Notepad replacement that supports several languages. Running in the MS Windows environment, its use is governed by GPL License."
				},
				new
				{
					FriendlyName = "Sublime Text",
					PackageName = "SublimeText3",
					Description = "Sublime Text 3 is a sophisticated text editor for code, html and prose. You'll love the slick user interface and extraordinary features. Sublime Text may be downloaded and evaluated for free, however a license must be purchased for continued use."
				},
				new
				{
					FriendlyName = "TeamViewer",
					PackageName = "teamviewer",
					Description = "Remote control any computer or Mac over the internet within seconds or use TeamViewer for online meetings. Free for private use. Commercial users are welcome to use for trial purposes."
				},
				new
				{
					FriendlyName = "Fiddler",
					PackageName = "fiddler4",
					Description = "Fiddler is a Web Debugging Proxy which logs all HTTP(S) traffic between your computer and the Internet. Fiddler allows you to inspect all HTTP(S) traffic, set breakpoints, and \"fiddle\" with incoming or outgoing data. Fiddler includes a powerful event-based scripting subsystem, and can be extended using any .NET language."
				},
				new
				{
					FriendlyName = "Eclipse IDE for Java Developers",
					PackageName = "eclipse-java-kepler",
					Description = "an open source, robust, full-featured, commercial-quality, industry platform for the development of highly integrated tools and rich client applications"
				},
				new
				{
					FriendlyName = "Java.JDK 7",
					PackageName = "java.jdk",
					Description = "he Java Development Kit (JDK) version 7.0.60."
				},
				new
				{
					FriendlyName = "Git (for Windows)",
					PackageName = "git.install",
					Description = "Git (for Windows) - Git is a powerful distributed Source Code Management tool. If you just want to use Git to do your version control in Windows, you will need to download Git for Windows, run the installer, and you are ready to start. Note: Git for Windows is a project run by volunteers, so if you want it to improve, volunteer!"
				},
				new
				{
					FriendlyName = "TortoiseSVN",
					PackageName = "tortoisesvn",
					Description = "TortoiseSVN is an easy-to-use SCM / source control software for Microsoft Windows and possibly the best standalone Apache™ Subversion®client there is. Please install with chocolatey (http://nuget.org/List/Packages/chocolatey). This will install TortoiseSVN on your system!"
				},
				new
				{
					FriendlyName = "Jenkins CI",
					PackageName = "jenkins",
					Description = "Jenkins is an award-winning application that monitors executions of repeated jobs, such as building a software project or jobs run by cron."
				},
				new
				{
					FriendlyName = "Node JS",
					PackageName = "nodejs.install",
					Description = "Node JS - Evented I/O for v8 JavaScript. Node's goal is to provide an easy way to build scalable network programs."
				},
				new
				{
					FriendlyName = "Ruby",
					PackageName = "ruby",
					Description = "Ruby - A dynamic, open source programming language with a focus on simplicity and productivity. It has an elegant syntax that is natural to read and easy to write."
				},
			};
		}

		[HttpGet]
		public IEnumerable<object> AvailableSubscriptions()
		{
			using (ApplicationDbContext context = new ApplicationDbContext())
            {
				int activeSubscriptionId = UnitOfWork.CurrentIdentity.ActiveSubscriptionId;

				var availableSubscriptions = context.Subscriptions.Where(x => x.SubscriptionIdentityRoles.Any(y => y.SubscriptionId == activeSubscriptionId)).Distinct().ToList()
					.Select(x=> new
					{
						SubscriptionId = x.SubscriptionId,
						SubscriptionName = x.Name,
						Active = x.SubscriptionId == UnitOfWork.ActiveSubscriptionId
					});

				return availableSubscriptions;
			}
		}

        [HttpGet]
        public string Version()
        {
            var assemblyName = typeof(TrainingsController).Assembly.GetName();
            return assemblyName.Version.ToString();
        }

		/// <summary>
		/// For a given session window, gets the expected number of available free CPUs.
		/// </summary>
		/// <param name="sessionStart">Session start time (without warm-up).</param>
		/// <param name="sessionEnd">Session end time (without cool-down).</param>
		/// <returns>??</returns>
		[HttpGet]
		public int CheckForAvailableResources(DateTimeOffset sessionStart, DateTimeOffset sessionEnd)
		{
			return SubscriptionService.CheckForAvailableResources(UnitOfWork.CurrentIdentity.ActiveSubscriptionId, sessionStart, sessionEnd);
		}

        [HttpPost]
        public SaveResult SaveChanges(JObject saveBundle)
        {
            return _repository.SaveChanges(saveBundle);
        }
    }
}