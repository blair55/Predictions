namespace Predictions.Api

open Owin
open Microsoft.Owin
open Microsoft.Owin.Cors
open Microsoft.Owin.Security
open Microsoft.Owin.Security.Cookies
open Microsoft.Owin.Security.DataHandler
open Microsoft.AspNet.Identity
open Microsoft.AspNet.Identity.Owin
open Microsoft.Owin.StaticFiles
open Microsoft.Owin.FileSystems
open Owin.Security.AesDataProtectorProvider
open System.Web.Http.Owin
open System
open System.Configuration
open System.Net
open System.Net.Http
open System.Web.Http
open System.Web.Http.Filters
open System.Web.Http.Controllers
open Predictions.Api.Domain
open Predictions.Api.Data

type ErrorFilter() =
    inherit ExceptionFilterAttribute()
    override this.OnException(context:HttpActionExecutedContext) =
        let response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        Logging.error(context.Exception)
        System.Console.WriteLine("**********************")
        System.Console.WriteLine("{0}", context.Exception.Message)
        System.Console.WriteLine("{0}", context.Exception.StackTrace)
        System.Console.WriteLine(String.Empty);
        response.Content <- new StringContent("Something went wrong!")
        context.Response <- response

type CustomAuthorizeAttribute() =
    inherit AuthorizationFilterAttribute()

    override this.OnAuthorization(actionContext:HttpActionContext) =
        let authManager = actionContext.Request.GetOwinContext().Authentication
        let idtype = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
        let plid = authManager.User.Claims
                    |> Seq.find(fun c -> c.Type = idtype)
                    |> (fun c -> c.Value)
                    |> sToGuid|>PlId
        let unauthResponse() = 
            actionContext.Response <- actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized)
        match tryFindPlayerByPlayerId plid with
        | Some p -> if p.isAdmin then () else unauthResponse()
        | None -> unauthResponse()

type LogRouteAttribute() =
    inherit ActionFilterAttribute()
    
    override this.OnActionExecuting(actionContext:HttpActionContext) =
        let authManager = actionContext.Request.GetOwinContext().Authentication
        let path = actionContext.Request.RequestUri.PathAndQuery
        let user = authManager.User.Identity.GetUserName()
        Logging.info(sprintf "path=%s user=%s" path user)

module Config =

    let config (key:string) = ConfigurationManager.AppSettings.[key]

    let buildApp(app:IAppBuilder) =
    
        let userManager() = new PlUserManager(new PlUserStore())
        let cookieOptions = new CookieAuthenticationOptions()
        cookieOptions.AuthenticationType <- DefaultAuthenticationTypes.ApplicationCookie
        let ids = ["A5EF0B11CEC04103A34A659048B21CE0572D7D47"  // VeriSign Class 3 Secure Server CA - G2
                   "0D445C165344C1827E1D20AB25F40163D8BE79A5"  // VeriSign Class 3 Secure Server CA - G3
                   "7FD365A7C2DDECBBF03009F34339FA02AF333133"  // VeriSign Class 3 Public Primary Certification Authority - G5
                   "39A55D933676616E73A761DFA16A7E59CDE66FAD"  // Symantec Class 3 Secure Server CA - G4
                   "4eb6d578499b1ccf5f581ead56be3d9b6744a5e5"  // VeriSign Class 3 Primary CA - G5
                   "5168FF90AF0207753CCCD9656462A212B859723B"  // DigiCert SHA2 High Assurance Server C‎A 
                   "B13EC36903F8BF4701D498261A0802EF63642BC3"] // DigiCert High Assurance EV Root CA
        let twitterOptions = new Twitter.TwitterAuthenticationOptions()
        twitterOptions.ConsumerKey <- config "TwitterConsumerKey"
        twitterOptions.ConsumerSecret <- config "TwitterConsumerSecret"
        twitterOptions.BackchannelCertificateValidator <- new CertificateSubjectKeyIdentifierValidator(ids)

        let facebookOptions = new Facebook.FacebookAuthenticationOptions()
        facebookOptions.Scope.Add("email")
        facebookOptions.AppId <- config "FacebookAppId"
        facebookOptions.AppSecret <- config "FacebookAppSecret"

        let fsOptions = new FileServerOptions()
        fsOptions.EnableDirectoryBrowsing <- true
        fsOptions.RequestPath <- PathString.Empty
        fsOptions.FileSystem <- new PhysicalFileSystem(config "StaticFileRoot")

        let config = new HttpConfiguration()
//        config.EnableSystemDiagnosticsTracing() |> ignore
        //config.IncludeErrorDetailPolicy <- IncludeErrorDetailPolicy.Always
        //config.EnableCors()
        config.MapHttpAttributeRoutes()
        config.Filters.Add(new ErrorFilter())
        config.Formatters.XmlFormatter.SupportedMediaTypes.Clear()
        config.Formatters.JsonFormatter.SerializerSettings.ContractResolver
            <- Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()

        app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie)
        app.UseAesDataProtectorProvider()
        
        app.UseErrorPage()
           .UseTwitterAuthentication(twitterOptions)
           .UseFacebookAuthentication(facebookOptions)
           .UseCookieAuthentication(cookieOptions)
           .CreatePerOwinContext<PlUserManager>(fun _ (c:IOwinContext) -> userManager())
           .CreatePerOwinContext<PlSignInManager>(fun _ (c:IOwinContext) -> new PlSignInManager(userManager(), c.Authentication))
           .UseFileServer(fsOptions)
           .UseWebApi(config) |> ignore