namespace Predictions.Api

open System
open System.Linq
open System.Net
open System.Net.Http
open System.Web
open System.Web.Http
open System.Web.Routing
open System.Web.Http.Cors
open System.Net.Http.Headers
open Predictions.Api.Domain
open Predictions.Api.ViewModels
open Predictions.Api.PostModels
open Predictions.Api.Services
open Predictions.Api.WebUtils
open Microsoft.Owin.Security
open Microsoft.AspNet.Identity
open Microsoft.AspNet.Identity.Owin

[<AllowAnonymous>]
[<RoutePrefix("account")>]
type AccountController() =
    inherit ApiController()

    member private this.SignInManager = this.Request.GetOwinContext().Get<PlSignInManager>()
    member private this.AuthManager = this.Request.GetOwinContext().Authentication
    member private this.BaseUri = new Uri(this.Request.RequestUri.AbsoluteUri.Replace(this.Request.RequestUri.PathAndQuery, String.Empty))

    [<HttpPost>][<Route("login")>]
    member this.LoginWithExternal(model:ExternalLoginPostModel) =
        new ChallengeResult(model.provider, this.Request)

    [<HttpGet>][<Route("logout")>]
    member this.GetLogOut() =
        this.AuthManager.SignOut()
        this.Redirect(this.BaseUri)

    [<HttpGet>][<Route("callback")>]
    member this.GetCallback() =
        let loginInfo = this.AuthManager.GetExternalLoginInfo()
        if (box loginInfo <> null) then
            let signInUser = register loginInfo
            this.SignInManager.SignIn(signInUser, false, true)
        this.Redirect(this.BaseUri)

[<Authorize>]
[<RoutePrefix("api")>]
type HomeController() =
    inherit ApiController()

    member private this.AuthManager = this.Request.GetOwinContext().Authentication

    member this.GetLoggedInPlayerId() =
        let idtype = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
        let id = this.AuthManager.User.Claims
                 |> Seq.find(fun c -> c.Type = idtype)
                 |> (fun c -> c.Value)
        id|>sToGuid|>PlId

    member this.GetPlayerViewModel() =
        let name = this.AuthManager.User.Identity.GetUserName()
        let idtype = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
        let id = this.AuthManager.User.Claims
                 |> Seq.find(fun c -> c.Type = idtype)
                 |> (fun c -> c.Value)
        { PlayerViewModel.id=id; name=name; isAdmin=false }

    member this.Host() = this.Request.RequestUri.GetLeftPart(UriPartial.Authority)

    [<Route("whoami")>]
    member this.GetWhoAmI() =
        () |> (switch this.GetPlayerViewModel >> resultToHttp)

    [<Route("logout")>]
    member this.GetLogOut() =
        base.Request |> logPlayerOut

    [<Route("leagues")>]
    member this.GetLeagues() =
        let player = this.GetLoggedInPlayerId() |> getLoggedInPlayer
        player |> (switch getLeaguesView >> resultToHttp)

    [<HttpPost>][<Route("createleague")>]
    member this.AddLeague (createLeague:CreateLeaguePostModel) =
        let player = this.GetLoggedInPlayerId() |> getLoggedInPlayer
        let saveLge = trySaveLeague player
        createLeague |> (saveLge >> resultToHttp)

    [<Route("league/{leagueId:Guid}")>]
    member this.GetLeagueView (leagueId:Guid) =
        leagueId |> (getLeagueView >> resultToHttp)

    [<Route("leagueinvite/{leagueId:Guid}")>]
    member this.GetLeagueInviteView (leagueId:Guid) =
        let getLge = getLeagueInviteView (this.Host())
        leagueId |> (getLge >> resultToHttp)

    [<Route("leaguejoin/{shareableLeagueId}")>]
    member this.GetLeagueJoinView (shareableLeagueId) =
        shareableLeagueId |> (getLeagueJoinView >> resultToHttp)

    [<HttpPost>][<Route("leaguejoin/{leagueId:Guid}")>]
    member this.JoinLeague (leagueId:Guid) =
        let player = this.GetLoggedInPlayerId() |> getLoggedInPlayer
        leagueId |> ((joinLeague player) >> resultToHttp)



    [<Route("player/{playerId:Guid}")>]
    member this.GetPlayer (playerId) =
        playerId |> (getGameWeeksPointsForPlayerId >> resultToHttp)

    [<Route("playergameweek/{playerId:Guid}/{gameWeekNo:int}")>]
    member this.GetPlayerGameWeek (playerId) (gameWeekNo:int) =
        let getPoints = getPlayerGameWeek playerId
        gameWeekNo |> (switch getPoints >> resultToHttp)
        
    [<Route("openfixtures")>]
    member this.GetOpenFixtures() =
        base.Request |> (getLoggedInPlayerAuthToken
                     >> bind getPlayerFromAuthToken
                     >> bind (switch getOpenFixturesForPlayer)
                     >> resultToHttp)

    [<HttpPost>][<Route("prediction")>]
    member this.AddPrediction (prediction) =
        base.Request |> (getLoggedInPlayerAuthToken
                     >> bind getPlayerFromAuthToken
                     >> bind (trySavePrediction prediction)
                     >> resultToHttp)

    [<Route("history/month")>]
    member this.GetHistoryByMonth() =
        () |> ((switch getPastMonthsWithWinner) >> resultToHttp)

    [<Route("history/month/{month}")>]
    member this.GetHistoryByMonth month =
        month |> ((switch getMonthPointsView) >> resultToHttp)

    [<Route("history/gameweek")>]
    member this.GetPastGameWeeks() =
        () |> (switch getPastGameWeeksWithWinner >> resultToHttp)

    [<Route("history/gameweek/{gwno:int}")>]
    member this.GetGameWeekPoints gwno =
        let getGwPointsView = (GwNo gwno) |> getGameWeekPointsView
        base.Request |> (getLoggedInPlayerAuthToken
                     >> bind getPlayerFromAuthToken
                     >> bind (switch getGwPointsView)
                     >> resultToHttp)

    [<Route("fixture/{fxId:Guid}")>]
    member this.GetFixture (fxId:Guid) =
        FxId fxId |> (getPlayerPointsForFixture >> resultToHttp)

    [<Route("leaguepositiongraphforplayer/{plId:Guid}")>]
    member this.GetLeaguePositionGraph (plId) =
        PlId plId |> ((switch getLeaguePositionGraphDataForPlayer) >> resultToHttp)

    [<Route("fixturepredictiongraph/{fxId:Guid}")>]
    member this.GetFixturePredictionGraph (fxId:Guid) =
        FxId fxId |> (getFixturePredictionGraphData >> resultToHttp)

//    [<Route("getleaguepositionforplayer")>]
//    member this.GetLeaguePositionForPlayer() =
//        base.Request |> (getLoggedInPlayerAuthToken
//                     >> bind getPlayerFromAuthToken
//                     >> bind getLeaguePositionForPlayer
//                     >> resultToHttp)

    [<Route("getlastgameweekandwinner")>]
    member this.GetLastGameWeekAndWinner() =
        () |> (switch getLastGameWeekAndWinner >> resultToHttp)

    [<Route("getopenfixtureswithnopredictionsforplayercount")>]
    member this.GetOpenFixturesWithNoPredictionsForPlayerCount() =
        base.Request |> (getLoggedInPlayerAuthToken
                     >> bind getPlayerFromAuthToken
                     >> bind (switch getOpenFixturesWithNoPredictionsForPlayerCount)
                     >> resultToHttp)

    [<Route("inplay")>]
    member this.GetInPlay() =
        () |> (switch getInPlay >> resultToHttp)
                     

[<RoutePrefix("api/admin")>]
type AdminController() =
    inherit ApiController()

    [<HttpPost>][<Route("gameweek")>]
    member this.CreateGameWeek (gameWeek:GameWeekPostModel) =
        let saveGameWeek() = trySaveGameWeekPostModel gameWeek
        base.Request |> (makeSurePlayerIsAdmin
                     >> bind saveGameWeek
                     >> resultToHttp)

    [<Route("getgameweekswithclosedfixtures")>]
    member this.GetGameWeeksWithClosedFixtures() =
        base.Request |> (makeSurePlayerIsAdmin
                     >> bind (switch getGameWeeksWithClosedFixtures)
                     >> resultToHttp)

    [<Route("getclosedfixturesforgameweek/{gwno:int}")>]
    member this.GetClosedFixturesForGameWeek gwno =
        let getFixtures() = gwno |> GwNo |> getClosedFixturesForGameWeek
        base.Request |> (makeSurePlayerIsAdmin
                     >> bind (switch getFixtures)
                     >> resultToHttp)

    [<HttpPost>][<Route("result")>]
    member this.AddResult (result:ResultPostModel) =
        let saveResult() = trySaveResultPostModel result
        base.Request |> (makeSurePlayerIsAdmin
                     >> bind saveResult
                     >> resultToHttp)
