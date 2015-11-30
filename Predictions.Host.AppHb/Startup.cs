using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Predictions.Host.AppHb;

[assembly: OwinStartup(typeof(Startup))]

namespace Predictions.Host.AppHb
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Use<ExpireCookie>();
        }
    }

    public class ExpireCookie : OwinMiddleware
    {
        public ExpireCookie(OwinMiddleware next) : base(next) { }

        public override async Task Invoke(IOwinContext context)
        {
            await Next.Invoke(context);
            var c = new CookieOptions
            {
                Expires = new DateTime(2015, 1, 1),
                HttpOnly = true,
                Path = "/"
            };
            context.Response.Cookies.Append(".AspNet.ApplicationCookie", "", c);
        }
    }
}