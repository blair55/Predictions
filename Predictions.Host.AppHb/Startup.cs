using Microsoft.Owin;
using Owin;
using Predictions.Api;
using Predictions.Host.AppHb;

[assembly: OwinStartup(typeof(Startup))]

namespace Predictions.Host.AppHb
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Config.BuildApp(app);
        }
    }
}