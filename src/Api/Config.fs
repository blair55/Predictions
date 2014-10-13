namespace Predictions.Api

open System
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open System.Web
open System.Web.Http
open System.Web.Routing
open System.Web.Http.Filters

type HttpRoute = { controller:string; id:RouteParameter }

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
        //config.Formatters.XmlFormatter.UseXmlSerializer <- false
        config.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
        config.Formatters.JsonFormatter.SerializerSettings.ContractResolver
            <- Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
