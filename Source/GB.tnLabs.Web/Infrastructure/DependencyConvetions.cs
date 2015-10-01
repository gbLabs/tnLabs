namespace GB.tnLabs.Web.Infrastructure
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;
    using Data;

    public class DependencyConvetions : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Classes.FromThisAssembly()
                    .Where(t => t.Name.EndsWith("Controller"))
                    .WithService.DefaultInterfaces().LifestyleTransient());

			container.Register(Classes.FromThisAssembly()
							.BasedOn<System.Web.Mvc.IController>()
							.LifestyleTransient());

			container.Register(Component.For<LabContextProvider>().LifestyleTransient());
			container.Register(Component.For<LabRepository>().LifestyleTransient());
			container.Register(Component.For<IUnitOfWork>().ImplementedBy<UnitOfWork>().LifestylePerWebRequest());
         
        }
    }
}