using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace GB.tnLabs.Web.Infrastructure
{
    public class WindsorControllerFactory : DefaultControllerFactory
    {

        public override void ReleaseController(IController controller)
        {
            DependencyContainer.Release(controller);
        }

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            if (controllerType == null)
            {
                throw new HttpException(404, string.Format("The controller for path '{0}' could not be found.", requestContext.HttpContext.Request.Path));
            }
            return (IController)DependencyContainer.Get(controllerType.FullName, controllerType);
        }
    }
}