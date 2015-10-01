using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace GB.tnLabs.Web.Infrastructure
{
    public sealed class DependencyContainer
    {
        private static readonly DependencyContainer _instance = new DependencyContainer();
        private readonly IWindsorContainer _container; 

        // Explicit static constructor to tell C# compiler
        // not to mark type as before field init
        static DependencyContainer()
        {
            
        }
        private DependencyContainer()
        {
            _container = new WindsorContainer();
        }

        public static void InstallDependencies(params IWindsorInstaller[] installers)
        {
            _instance._container.Install(installers);
        }

        public static T Get<T>()
        {
            return _instance._container.Resolve<T>();
        }

        public static object Get(Type t)
        {
            return _instance._container.Resolve(t);
        }

        public static object Get(string key, Type t)
        {
           return _instance._container.Resolve(key, t);
        }

        public static void Release(object instance)
        {
           _instance._container.Release(instance);
        }
       
    }
}
