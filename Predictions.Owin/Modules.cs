using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using Nancy.Owin;
using System.Threading;
using System.Security.Claims;
using Nancy.Security;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Extensions;

namespace Predictions.Owin
{


    public class SecureModule : NancyModule
    {
        public SecureModule()
        {
            this.RequiresMSOwinAuthentication();

            Get["secure"] = x =>
            {
                return "secure page";
            };
        }
    }

    public class ExternalcallbackModule : NancyModule
    {
        public ExternalcallbackModule()
        {
            this.RequiresMSOwinAuthentication();

            Get["externalcallback"] = x =>
            {   
                ClaimsPrincipal claimsPrincipal = Context.GetMSOwinUser();
                Console.WriteLine("==>I have been called by {0}", claimsPrincipal.FindFirst(ClaimTypes.Upn));
                return new[] {"value1", "value2"};
            };
        }
    }

    public class LoginModule : NancyModule
    {
        public LoginModule()
        {
            Get["login"] = x =>
            {
                //var properties = new AuthenticationProperties() { RedirectUri = "ExternalLoginCallback" };
                //this.Context.GetOwinEnvironment().Authentication.Challenge(properties, LoginProvider);

                return "login page";
            };
        }
    }

    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get["home"] = x =>
            {
                var env = this.Context.GetOwinEnvironment();

                var requestBody = (Stream)env["owin.RequestBody"];
                var requestHeaders = (IDictionary<string, string[]>)env["owin.RequestHeaders"];
                var requestMethod = (string)env["owin.RequestMethod"];
                var requestPath = (string)env["owin.RequestPath"];
                var requestPathBase = (string)env["owin.RequestPathBase"];
                var requestProtocol = (string)env["owin.RequestProtocol"];
                var requestQueryString = (string)env["owin.RequestQueryString"];
                var requestScheme = (string)env["owin.RequestScheme"];

                var responseBody = (Stream)env["owin.ResponseBody"];
                var responseHeaders = (IDictionary<string, string[]>)env["owin.ResponseHeaders"];

                var owinVersion = (string)env["owin.Version"];
                var cancellationToken = (CancellationToken)env["owin.CallCancelled"];

                var uri = (string)env["owin.RequestScheme"] + "://" + requestHeaders["Host"].First() +
                  (string)env["owin.RequestPathBase"] + (string)env["owin.RequestPath"];

                if (env["owin.RequestQueryString"] != "")
                    uri += "?" + (string)env["owin.RequestQueryString"];

                return string.Format("{0} {1}", requestMethod, uri);
            };
        }
    }
}