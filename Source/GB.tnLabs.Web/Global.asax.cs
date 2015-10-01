using Castle.Core.Logging;
using GB.tnLabs.Web.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace GB.tnLabs.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            DependencyContainer.InstallDependencies(new DependencyConvetions(), new Core.DependencyConventions());
            //WEB API
            GlobalConfiguration.Configuration.Services.Replace(
                typeof(IHttpControllerActivator),
                new WindsorCompositionRoot());

            AreaRegistration.RegisterAllAreas();
            //BreezeWebApiConfig.RegisterBreezePreStart();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            MvcRouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //MVC
            WindsorControllerFactory controllerFactory = new WindsorControllerFactory();
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);

            ILogger logger = DependencyContainer.Get<ILogger>();
            logger.Info("Application start.");
        }
    }
}
