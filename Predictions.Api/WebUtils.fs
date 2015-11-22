namespace Predictions.Api

open System
open System.Net
open System.Net.Http
open System.Web
open System.Web.Http
open System.Web.Routing
open System.Net.Http.Headers
open System.Threading
open System.Threading.Tasks
open Newtonsoft.Json
open Predictions.Api.Domain
open Predictions.Api.Common
open Microsoft.Owin.Security
open Microsoft.AspNet.Identity
open Microsoft.AspNet.Identity.Owin

[<AutoOpen>]
module WebUtils =
    
    let getOkResponseWithBody (body:'T) =
        let response = new HttpResponseMessage(HttpStatusCode.OK)
        response.Content <- new ObjectContent<'T>(body, new Formatting.JsonMediaTypeFormatter())
        response
    
    let errorResponse status msg =
        Logging.warn msg
        let response = new HttpResponseMessage(status)
        response.Content <- new StringContent(msg)
        response

    let getErrorResponseFromAppError appError =
        match appError with
        | NotFound msg -> errorResponse HttpStatusCode.NotFound msg
        | Invalid msg -> errorResponse HttpStatusCode.BadRequest msg
        | InternalError msg -> errorResponse HttpStatusCode.InternalServerError msg
        | Forbidden msg -> errorResponse HttpStatusCode.Forbidden msg
        | NotLoggedIn msg -> errorResponse HttpStatusCode.Unauthorized msg

    let httpResponseFromResult result =
        match result with
        | Success httpResponse -> httpResponse
        | Failure appError -> getErrorResponseFromAppError appError

    let resultToHttp result =
        match result with
        | Success body -> getOkResponseWithBody body
        | Failure appError -> getErrorResponseFromAppError appError

    type ChallengeResult(loginProvider:string, request:HttpRequestMessage) =
        let getQueryParamFromReferrerUri (uri:Uri) (param:string) =
            if uri <> null then uri.ParseQueryString().Get(param) else ""
        interface IHttpActionResult with
            member this.ExecuteAsync(cancellationToken) =
                let authProperties = new AuthenticationProperties()
                let referrer = getQueryParamFromReferrerUri request.Headers.Referrer "redirect"
                authProperties.RedirectUri <- "/account/callback?redirect=" + referrer
                request.GetOwinContext().Authentication.Challenge(authProperties, loginProvider)
                let response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                response.RequestMessage <- request
                Task.FromResult(response)

    let register (loginInfo:ExternalLoginInfo) =
        let externalId = loginInfo.ExternalIdentity.GetUserId() |> ExternalPlayerId
        let provider = loginInfo.Login.LoginProvider |> ExternalLoginProvider
        let userName = loginInfo.ExternalIdentity.GetUserName() |> PlayerName
        let registeredPlayer = registerPlayerWithUserInfo externalId provider userName loginInfo.Email
        let userId = registeredPlayer.id |> getPlayerId |> str
        let playerName = registeredPlayer.name |> getPlayerName
        new PlUser(userId, provider, playerName)
