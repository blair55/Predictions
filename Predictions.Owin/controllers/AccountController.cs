using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace Predictions.Owin.controllers
{
    [RoutePrefix("account")]
    public class AccountController : ApiController
    {
        public PlSignInManager SignInManager
        {
            get { return Request.GetOwinContext().Get<PlSignInManager>(); }
        }

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private Uri BaseUri
        {
            get { return new Uri(Request.RequestUri.AbsoluteUri.Replace(Request.RequestUri.PathAndQuery, String.Empty)); }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public IHttpActionResult LoginWithExternal(ExternalLoginPostModel model)
        {
            return new ChallengeResult(model.Provider, this);
        }

        [HttpGet]
        [Route("logout")]
        public IHttpActionResult GetLogOut()
        {
            Authentication.SignOut();
            return Redirect(BaseUri);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("callback")]
        public async Task<IHttpActionResult> GetCallback()
        {
            var loginInfo = await Authentication.GetExternalLoginInfoAsync();

            if (loginInfo == null)
            {
                return Redirect(BaseUri);
            }

            var user = new PlUser { Id = loginInfo.ExternalIdentity.GetUserId(), UserName = loginInfo.DefaultUserName };
            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

            return Redirect(BaseUri);
        }
    }

    public class ExternalLoginPostModel
    {
        public string Provider { get; set; }
    }
}