namespace Predictions.Api

open Owin
open Microsoft.Owin
open Microsoft.Owin.Cors
open Microsoft.Owin.Security
open Microsoft.Owin.Security.Cookies
open Microsoft.AspNet.Identity
open Microsoft.AspNet.Identity.Owin
open System.Web.Http.Owin
open System
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open System.Web
open System.Web.Http
open System.Web.Routing
open System.Web.Http.Filters

//type HttpRoute = { controller:string; id:RouteParameter }

type ErrorFilter() =
    inherit ExceptionFilterAttribute()

    override this.OnException(context:HttpActionExecutedContext) =
        let response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        response.Content <- new StringContent(context.Exception.Message)
        context.Response <- response

type Config() =

    static member RegisterWebApi(config: HttpConfiguration) =
        config.MapHttpAttributeRoutes()
        config.EnableCors()
        config.Filters.Add(new ErrorFilter())
        config.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
        config.Formatters.JsonFormatter.SerializerSettings.ContractResolver
            <- Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()

    static member BuildApp(app:IAppBuilder) =
    
        let userManager = new PlUserManager(new PlUserStore())
        app.CreatePerOwinContext<PlSignInManager>(fun _ (c:IOwinContext) -> new PlSignInManager(userManager, c.Authentication)) |> ignore
        
        app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie)

        let cookieOptions = new CookieAuthenticationOptions()
        cookieOptions.AuthenticationType <- DefaultAuthenticationTypes.ApplicationCookie
        app.UseCookieAuthentication(cookieOptions) |> ignore

        let twitterOptions = new Twitter.TwitterAuthenticationOptions()
        twitterOptions.ConsumerKey <- "zo4kCGVlAp2esQpqXfFvGqow1"
        twitterOptions.ConsumerSecret <- "qSkztP6VB1a7jxVtLyHSVX13QqyzCGiT1zX5psvbco8UiTkohr"
        app.UseTwitterAuthentication(twitterOptions) |> ignore

        let facebookOptions = new Facebook.FacebookAuthenticationOptions()
        facebookOptions.AppId <- "701632913289116"
        facebookOptions.AppSecret <- "67e25b79fa689840e26520f20f620fa8"
        app.UseFacebookAuthentication(facebookOptions) |> ignore

        app.UseCors(CorsOptions.AllowAll) |> ignore

        let config = new HttpConfiguration()
        Config.RegisterWebApi(config)
        app.UseWebApi(config) |> ignore