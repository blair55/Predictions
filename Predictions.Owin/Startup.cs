using System;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Predictions.Api;
using Predictions.Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace Predictions.Owin
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.CreatePerOwinContext<PlSignInManager>((options, context) =>
                new PlSignInManager(new PlUserManager(new PlUserStore()), context.Authentication));

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/"),
                Provider = new CookieAuthenticationProvider {OnApplyRedirect = ctx => { }}
                //{
                //    // Enables the application to validate the security stamp when the user logs in.
                //    // This is a security feature which is used when you change a password or add an external login to your account.  
                //    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                //        validateInterval: TimeSpan.FromMinutes(30),
                //        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                //}
            });
    
            app.UseTwitterAuthentication(
               consumerKey: "zo4kCGVlAp2esQpqXfFvGqow1",
               consumerSecret: "qSkztP6VB1a7jxVtLyHSVX13QqyzCGiT1zX5psvbco8UiTkohr");

            app.UseFacebookAuthentication(
               appId: "701632913289116",
               appSecret: "67e25b79fa689840e26520f20f620fa8");

            var config = new HttpConfiguration();
            Config.RegisterWebApi(config);

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "security/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            app.UseWebApi(config);
            app.UseCors(CorsOptions.AllowAll);
        }
    }


    public class PlUser : IUser
    {
        public string Id { get; set; }
        public string UserName { get; set; }
    }

    public class PlUserStore : IUserStore<PlUser>
    {
        public async Task<PlUser> FindByNameAsync(string name) { throw new Exception(); }
        public async Task<PlUser> FindByIdAsync(string name) { throw new Exception(); }
        public async Task UpdateAsync(PlUser user) { throw new Exception(); }
        public async Task DeleteAsync(PlUser user) { throw new Exception(); }
        public async Task CreateAsync(PlUser user) { throw new Exception(); }

        public void Dispose()
        {}
    }

    public class PlUserManager : UserManager<PlUser>
    {
        public PlUserManager(IUserStore<PlUser> store)
            : base(store)
        { }
    }

    public class PlSignInManager : SignInManager<PlUser, string>
    {
        public PlSignInManager(PlUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        { }
    }
}