using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Predictions.MvcOwin.Startup))]
namespace Predictions.MvcOwin
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
