using Castle.Facilities.Logging;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using GB.tnLabs.AzureFacade;
using GB.tnLabs.Core.Components;

namespace GB.tnLabs.Core
{
	public class DependencyConventions : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<ILazyComponentLoader>().ImplementedBy<LazyOfTComponentLoader>());
			container.AddFacility<LoggingFacility>(f => f.UseNLog());
			container.Register(Component.For<AzureFacadeFactory>().LifestyleSingleton());
			container.Register(Component.For<ServiceBusMessageHandler>().LifestyleTransient());
			container.Register(Component.For<Sessions>().LifestyleTransient());
			container.Register(Component.For<SignUp>().LifestyleTransient());
			container.Register(Component.For<Labs>().LifestyleTransient());
            container.Register(Component.For<SubscriptionService>().LifestyleTransient());
		}
	}
}
