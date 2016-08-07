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

module Attributes =

    let getPlayerIdFromAuth(actionContext:HttpActionContext) =
        let authManager = actionContext.Request.GetOwinContext().Authentication
        let idtype = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
        authManager.User.Claims
        |> Seq.find(fun c -> c.Type = idtype)
        |> (fun c -> c.Value)
        |> sToGuid|>PlId

    let unauthResponse(actionContext:HttpActionContext) =
        actionContext.Response <- actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized)

    type ErrorFilter() =
        inherit ExceptionFilterAttribute()
        override this.OnException(context:HttpActionExecutedContext) =
            let response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            Logging.error(context.Exception)
            response.Content <- new StringContent("Something went wrong!")
            context.Response <- response

    type AdminAuthorizeAttribute() =
        inherit AuthorizationFilterAttribute()
        override this.OnAuthorization(actionContext:HttpActionContext) =
            let plid = getPlayerIdFromAuth(actionContext)
            match tryFindPlayerByPlayerId plid with
            | Some p -> if p.isAdmin then () else unauthResponse(actionContext)
            | None -> unauthResponse(actionContext)

    type ActiveUserAuthorizeAttribute() =
        inherit AuthorizationFilterAttribute()
        override this.OnAuthorization(actionContext:HttpActionContext) =
            let plid = getPlayerIdFromAuth(actionContext)
            let splid = getPlayerId plid |> string
            match tryFindPlayerByPlayerId plid with
            | Some p -> ()
            | None ->
                Logging.warn(sprintf "no player found with id %s" splid)
                unauthResponse(actionContext)

    type LogRouteAttribute() =
        inherit ActionFilterAttribute()

        override this.OnActionExecuting(actionContext:HttpActionContext) =
            let authManager = actionContext.Request.GetOwinContext().Authentication
            let path = actionContext.Request.RequestUri.PathAndQuery
            let user = authManager.User.Identity.GetUserName()
            Logging.info(sprintf "path=%s user='%s'" path user)
