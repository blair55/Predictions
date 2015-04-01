namespace Predictions.Api

open Owin
open Microsoft.Owin
open Microsoft.Owin.Cors
open Microsoft.Owin.Security
open Microsoft.Owin.Security.Cookies
open System.Web.Http.Owin
open System
open System.Configuration
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open System.Web
open System.Web.Http
open System.Web.Routing
open System.Web.Http.Filters
open Microsoft.AspNet.Identity
open Microsoft.AspNet.Identity.Owin

type ErrorFilter() =
    inherit ExceptionFilterAttribute()

    override this.OnException(context:HttpActionExecutedContext) =
        let response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        response.Content <- new StringContent(context.Exception.Message)
        context.Response <- response

type Config() =

    static member private RegisterWebApi(config: HttpConfiguration) =
        config.MapHttpAttributeRoutes()
        config.EnableCors()
        config.Filters.Add(new ErrorFilter())
        config.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
        config.Formatters.JsonFormatter.SerializerSettings.ContractResolver
            <- Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()

    static member BuildApp(app:IAppBuilder) =
        
        app.UseCors(CorsOptions.AllowAll) |> ignore
        app.SetDefaultSignInAsAuthenticationType(DefaultAuthenticationTypes.ExternalCookie)

        let cookieOptions = new CookieAuthenticationOptions()
        cookieOptions.AuthenticationType <- DefaultAuthenticationTypes.ExternalCookie
        app.UseCookieAuthentication(cookieOptions) |> ignore

        let twitterOptions = new Twitter.TwitterAuthenticationOptions()
        twitterOptions.ConsumerKey <- "Cs5EW6KIh7gLlmmMfEPWj13uV"
        twitterOptions.ConsumerSecret <- ConfigurationManager.AppSettings.["TwitterConsumerSecret"]
        app.UseTwitterAuthentication(twitterOptions) |> ignore

        let facebookOptions = new Facebook.FacebookAuthenticationOptions()
        facebookOptions.AppId <- "701632913289116"
        facebookOptions.AppSecret <- ConfigurationManager.AppSettings.["FacebookAppSecret"]
        app.UseFacebookAuthentication(facebookOptions) |> ignore

        let config = new HttpConfiguration()
        Config.RegisterWebApi(config)
        app.UseWebApi(config) |> ignore