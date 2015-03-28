using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

namespace Predictions.MvcOwin.Controllers
{
    public class HomeController : Controller
    {
        public PlSignInManager SignInManager
        {
            get { return HttpContext.GetOwinContext().Get<PlSignInManager>(); }
        }

        private IAuthenticationManager AuthManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }
        
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/Account/Login2.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChooseLogin(string returnUrl, string provider)
        {
            var uri = Url.Action("callback", "home", new { ReturnUrl = returnUrl });
            return new ChallengeResult(provider, uri);
        }

        public async Task<ActionResult> Callback(string returnUrl)
        {
            var loginInfo = await AuthManager.GetExternalLoginInfoAsync();

            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            var user = new PlUser { Id = loginInfo.ExternalIdentity.GetUserId(), UserName = loginInfo.DefaultUserName };
            //var r = SignInManager.CreateUserIdentityAsync(user).Result;
            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

            return new RedirectResult(returnUrl);
        }
    }

    public class ChallengeResult : HttpUnauthorizedResult
    {
        public ChallengeResult(string provider, string redirectUri)
        {
            LoginProvider = provider;
            RedirectUri = redirectUri;
        }

        public string LoginProvider { get; private set; }
        public string RedirectUri { get; private set; }

        public override void ExecuteResult(ControllerContext context)
        {
            var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
            context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
        }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }
}