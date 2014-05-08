using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Bespoker.Web.Startup))]
namespace Bespoker.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
