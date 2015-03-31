using Microsoft.Owin;
using Owin;
using Predictions.Api;
using Predictions.Host;

[assembly: OwinStartup(typeof(Startup))]

namespace Predictions.Host
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Config.BuildApp(app);
        }
    }
}