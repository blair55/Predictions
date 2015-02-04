using System;
using System.Threading.Tasks;
using Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Extensions;

[assembly: OwinStartup(typeof(Predictions.Owin.Startup1))]

namespace Predictions.Owin
{
    public class Startup1
    {
        public void Configuration(IAppBuilder app)
        {
            var cookieOptions = new CookieAuthenticationOptions
            {
                LoginPath = new PathString("/Login")
            };

            app.UseCookieAuthentication(cookieOptions);
            
            app.SetDefaultSignInAsAuthenticationType(cookieOptions.AuthenticationType);

            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions
            {
                ClientId = "184413536018-r1o4piddeir2l43hkkrct5mcgjlnrper.apps.googleusercontent.com",
                ClientSecret = "4uVr-m56GieU7pP0IiXgl_Nf"
            });

            app.UseNancy();

            app.UseStageMarker(PipelineStage.MapHandler);

            //app.Run(context =>
            //{
            //    context.Response.ContentType = "text/plain";
            //    return context.Response.WriteAsync("Hello, world.");
            //});
        }
    }
}