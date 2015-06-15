namespace Predictions.Api

open System
open System.Net
open System.Net.Http
open System.Web
open System.Web.Http
open System.Web.Routing
open System.Web.Http.Cors
open System.Net.Http.Headers
open System.Threading
open System.Threading.Tasks
open Newtonsoft.Json
open Predictions.Api.Domain
//open Predictions.Api.Data
open Predictions.Api.Common
open Microsoft.Owin.Security
open Microsoft.AspNet.Identity
open Microsoft.AspNet.Identity.Owin

[<AutoOpen>]
module WebUtils =
    
    let cookieName = "auth-token"

//    let getCookieValue (request:HttpRequestMessage) key =
//        let cookie = request.Headers.GetCookies(key) |> Seq.toList |> getFirst
//        match cookie with
//        | Some c -> Success c.[key].Value
//        | None -> NotLoggedIn "No cookie found" |> Failure

    let getCookieValue _ _ = Success ""

    let createCookie name (value:string) expiry =
        let cookie = new CookieHeaderValue(name, value)
        let nd d = new Nullable<DateTimeOffset>(d)
        cookie.Expires <- new DateTimeOffset(expiry) |> nd
        cookie.Path <- "/"
        cookie
       
    let getRedirectToResponseWithCookie (request:HttpRequestMessage) cookie =
        let res = new HttpResponseMessage(HttpStatusCode.Redirect)
        res.Headers.AddCookies([cookie])
        let components = match request.IsLocal() with
                         | true -> UriComponents.Scheme ||| UriComponents.HostAndPort
                         | false -> UriComponents.Scheme ||| UriComponents.Host
        let url = request.RequestUri.GetComponents(components, UriFormat.Unescaped)
        res.Headers.Location <- new Uri(url)
        res

    let logPlayerIn request (player:Player) =
        let july1025 = new DateTime(2015, 7, 1)
        let cookie = createCookie cookieName player.authToken july1025
        getRedirectToResponseWithCookie request cookie

    let logPlayerOut (request:HttpRequestMessage) =
        let yesterday = DateTime.Now.AddDays(-1.0)
        let cookie = createCookie cookieName "" yesterday
        getRedirectToResponseWithCookie request cookie
        
    let getLoggedInPlayerAuthToken r =
        getCookieValue r cookieName
        
//    let getLoggedInPlayerAuthToken (r:HttpRequestMessage) =
//        let id = r.GetOwinContext().Authentication.GetExternalIdentity("ApplicationCookie")
//        if id = null then NotLoggedIn "No cookie found" |> Failure else Success (id.GetUserId())

    let getOkResponseWithBody (body:'T) =
        let response = new HttpResponseMessage(HttpStatusCode.OK)
        response.Content <- new ObjectContent<'T>(body, new Formatting.JsonMediaTypeFormatter())
        response
    
    let errorResponse status msg =
        let response = new HttpResponseMessage(status)
        response.Content <- new StringContent(msg)
        response

    let checkPlayerIsAdmin (player:Player) =
        match player.role with
        | Admin -> Success ()
        | _ -> Forbidden "player not admin" |> Failure 

    let makeSurePlayerIsAdmin req =
        req |> (getLoggedInPlayerAuthToken
            >> bind getPlayerFromAuthToken
            >> bind checkPlayerIsAdmin)

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
        interface IHttpActionResult with
            member this.ExecuteAsync(cancellationToken) =
                let authProperties = new AuthenticationProperties()
                authProperties.RedirectUri <- "/account/callback"
                request.GetOwinContext().Authentication.Challenge(authProperties, loginProvider)
                let response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                response.RequestMessage <- request
                Task.FromResult(response)

    let buildPlUser (loginInfo:ExternalLoginInfo) =
        new PlUser(loginInfo.ExternalIdentity.GetUserId(), loginInfo.Login.LoginProvider, loginInfo.ExternalIdentity.GetUserName())

    let getPlayerFromViewModel (pvm:PlayerViewModel) =
        { Player.id=(PlId pvm.id); name=pvm.name; authToken=""; role=User}
        