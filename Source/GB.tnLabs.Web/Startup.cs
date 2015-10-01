using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GB.tnLabs.Web.Startup))]
namespace GB.tnLabs.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
