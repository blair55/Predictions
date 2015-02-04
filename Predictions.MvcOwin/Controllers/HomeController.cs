using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Predictions.MvcOwin.Models;

namespace Predictions.MvcOwin.Controllers
{
    public class HomeController : Controller
    {
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

        public ActionResult Login(string returnUrl)
        {
            // Request a redirect to the external login provider
            var uri = Url.Action("callback", "home", new { ReturnUrl = returnUrl });
            return new ChallengeResult("Twitter", uri);
        }

        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        public async Task<ActionResult> Callback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();

            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            return View("~/Views/Account/ExternalLoginConfirmation.cshtml", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            //return new RedirectResult(returnUrl);
        }
    }

    public class ChallengeResult : HttpUnauthorizedResult
    {
        public ChallengeResult(string provider, string redirectUri)
        {
            LoginProvider = provider;
            RedirectUri = redirectUri;
        }

        public string LoginProvider { get; set; }
        public string RedirectUri { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
            context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
        }
    }
}