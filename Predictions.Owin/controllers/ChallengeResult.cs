using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin.Security;

namespace Predictions.Owin.controllers
{
    public class ChallengeResult : IHttpActionResult
    {
        public string LoginProvider { get; set; }
        public HttpRequestMessage Request { get; set; }

        public ChallengeResult(string loginProvider, ApiController controller)
        {
            LoginProvider = loginProvider;
            Request = controller.Request;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var properties = new AuthenticationProperties { RedirectUri = "/account/callback" };
            Request.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = Request };
            return Task.FromResult(response);
        }
    }
}