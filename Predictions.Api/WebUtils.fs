namespace Predictions.Api

open System
open System.Net
open System.Net.Http
open System.Web
open System.Web.Http
open System.Web.Routing
open System.Web.Http.Cors
open System.Net.Http.Headers
open Newtonsoft.Json
open Predictions.Api.Domain
open Predictions.Api.Data
open Predictions.Api.Common

[<AutoOpen>]
module WebUtils =
    
    let cookieName = "auth-token"

    let getCookieValue (request:HttpRequestMessage) key =
        let cookie = request.Headers.GetCookies(key) |> Seq.toList |> getFirst
        match cookie with
        | Some c -> Success c.[key].Value
        | None -> Failure "No cookie found"

    let logPlayerIn (request:HttpRequestMessage) (player:Player) =
        let nd d = new Nullable<DateTimeOffset>(d)
        let c = new CookieHeaderValue(cookieName, player.authToken)
        let july1025 = new DateTime(2015, 7, 1)        
        let r = new HttpResponseMessage(HttpStatusCode.Redirect)
        c.Expires <- new DateTimeOffset(july1025) |> nd
        c.Path <- "/"
        r.Headers.AddCookies([c])
        let components = match request.IsLocal() with
                            | true -> UriComponents.Scheme ||| UriComponents.HostAndPort
                            | false -> UriComponents.Scheme ||| UriComponents.Host
        let url = request.RequestUri.GetComponents(components, UriFormat.Unescaped)
        r.Headers.Location <- new Uri(url)
        r

    let getLoggedInPlayerAuthToken r =
        getCookieValue r cookieName

    let convertStringToGuid v =
        let (isParsed, guid) = trySToGuid v
        if isParsed then Success guid else Failure (sprintf "could not convert %s to guid" v)

    let getOkResponseWithBody (body:'T) =
        let response = new HttpResponseMessage(HttpStatusCode.OK)
        response.Content <- new ObjectContent<'T>(body, new Formatting.JsonMediaTypeFormatter())
        response
    
    let unauthorised msg =
        let response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
        response.Content <- new StringContent(msg)
        response

    let getWhoAmIResponse result =
        match result with
        | Success player -> getOkResponseWithBody player
        | Failure msg -> unauthorised msg

    let doLogin req result =
        match result with
        | Success player -> logPlayerIn req player
        | Failure msg -> unauthorised msg 

    let checkPlayerIsAdmin (player:Player) =
        match player.role with
        | Admin -> Success ()
        | _ -> Failure "player not admin"

    let makeSurePlayerIsAdmin req =
        req |> (getLoggedInPlayerAuthToken
            >> bind getPlayerFromAuthToken
            >> bind checkPlayerIsAdmin)
    
    //let getErrorMessage

    let resultToHttp result =
        match result with
        | Success body -> getOkResponseWithBody body
        | Failure msg -> unauthorised msg 
