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

            var config = new System.Web.Http.HttpConfiguration();

            Predictions.Api.Config.RegisterWebApi(config);

            app.UseWebApi(config);
        }
    }
}
