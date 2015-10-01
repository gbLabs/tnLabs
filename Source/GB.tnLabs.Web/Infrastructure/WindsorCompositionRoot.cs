using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace GB.tnLabs.Web.Infrastructure
{

    public class WindsorCompositionRoot : IHttpControllerActivator
    {
        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            var controller = (IHttpController)DependencyContainer.Get(controllerType);

            request.RegisterForDispose(new Release(() => DependencyContainer.Release(controller)));

            return controller;
        }
    }
}