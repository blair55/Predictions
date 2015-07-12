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
    member this.GetCallback([<FromUri>]redirect:string) =
        let loginInfo = this.AuthManager.GetExternalLoginInfo()
        if (box loginInfo <> null) then
            let signInUser = register loginInfo
            this.SignInManager.SignIn(signInUser, false, true)
            let uri = sprintf "%s#%s" (str this.BaseUri) redirect
            this.Redirect(new Uri(uri))
        else this.Redirect(this.BaseUri)

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
        
    [<HttpPost>][<Route("leagueleave/{leagueId:Guid}")>]
    member this.LeaveLeague (leagueId:Guid) =
        let player = this.GetLoggedInPlayerId() |> getLoggedInPlayer
        leagueId |> ((leaveLeague player) >> resultToHttp)

    [<Route("player/{playerId:Guid}/{leagueId:Guid}")>]
    member this.GetPlayer (playerId:Guid, leagueId:Guid) =
        (playerId, leagueId) |> (getGameWeeksPointsForPlayerIdAndLeagueId >> resultToHttp)
        
    [<Route("leaguepositiongraphforplayer/{playerId:Guid}/{leagueId:Guid}")>]
    member this.GetLeaguePositionGraph (playerId:Guid, leagueId:Guid) =
        (playerId, leagueId) |> (getLeaguePositionGraphDataForPlayerIdAndLeagueId >> resultToHttp)

    [<Route("playergameweek/{playerId:Guid}/{gameWeekNo:int}")>]
    member this.GetPlayerGameWeek (playerId) (gameWeekNo:int) =
        let getPoints = getPlayerGameWeekByPlayerIdAndGameWeekNo gameWeekNo
        playerId |> (getPoints >> resultToHttp)

    [<Route("playerprofile/{playerId:Guid}")>]
    member this.GetPlayerProfile (playerId:Guid) =
        (playerId) |> (getPlayerProfileView >> resultToHttp)

    [<Route("playerprofilegraph/{playerId:Guid}")>]
    member this.GetPlayerProfileGraph (playerId:Guid) =
        (playerId) |> (getPlayerProfileGraph >> resultToHttp)

    [<Route("openfixtures")>]
    member this.GetOpenFixtures() =
        let player = this.GetLoggedInPlayerId() |> getLoggedInPlayer
        player |> ((switch getOpenFixturesForPlayer) >> resultToHttp)

    [<HttpPost>][<Route("prediction")>]
    member this.AddPrediction (prediction) =
        let player = this.GetLoggedInPlayerId() |> getLoggedInPlayer
        player |> ((trySavePrediction prediction) >> resultToHttp)

    [<Route("leaguehistory/{leagueId:Guid}/month")>]
    member this.GetHistoryByMonth(leagueId:Guid) =
        leagueId |> (getPastMonthsWithWinnerView >> resultToHttp)

    [<Route("leaguehistory/{leagueId:Guid}/month/{month}")>]
    member this.GetHistoryByMonth (leagueId:Guid, month) =
        let getView = getMonthPointsView month
        leagueId |> (getView >> resultToHttp)

    [<Route("leaguehistory/{leagueId:Guid}/gameweek")>]
    member this.GetPastGameWeeks(leagueId:Guid) =
        leagueId |> (getPastGameWeeksWithWinnerView >> resultToHttp)

    [<Route("leaguehistory/{leagueId:Guid}/gameweek/{gwno:int}")>]
    member this.GetGameWeekPoints (leagueId:Guid, gwno) =
        let getGwPointsView = gwno |> GwNo |> getGameWeekPointsView
        leagueId |> (getGwPointsView >> resultToHttp)
        
    [<Route("gameweekmatrix/{leagueId:Guid}/gameweek/{gwno:int}")>]
    member this.GetGameWeekMatrix (leagueId:Guid, gwno) =
        let getResult = gwno |> GwNo |> getGameWeekMatrix
        leagueId |> (getResult >> resultToHttp)

    [<Route("fixture/{fxId:Guid}")>]
    member this.GetFixture (fxId:Guid) =
        let getResult = this.GetLoggedInPlayerId() |> getLoggedInPlayer |> getPlayerPointsForFixture
        FxId fxId |> (getResult >> resultToHttp)

    [<Route("fixturepredictiongraph/{fxId:Guid}")>]
    member this.GetFixturePredictionGraph (fxId:Guid) =
        FxId fxId |> (getFixturePredictionGraphData >> resultToHttp)

    [<Route("fixturepreviousmeetings/{fxId:Guid}")>]
    member this.GetFixturePreviousMeetings (fxId:Guid) =
        FxId fxId |> (getFixturePreviousMeetingsView >> resultToHttp)

    [<Route("fixtureformguide/{fxId:Guid}")>]
    member this.GetFixtureFormGuide (fxId:Guid) =
        FxId fxId |> (getFixtureFormGuideView >> resultToHttp)

    [<Route("getleaguepositionforplayer")>]
    member this.Getleaguepositionforplayer() =
        let player = this.GetLoggedInPlayerId() |> getLoggedInPlayer
        player |> (switch getleaguePositionforplayer >> resultToHttp)

    [<Route("globalleague/{page:int}")>]
    member this.Getleaguepositionforplayer(page:int) =
        let getResult = this.GetLoggedInPlayerId() |> getLoggedInPlayer |> getGlobalLeagueTablePage
        page |> (switch getResult >> resultToHttp)

    [<Route("getlastgameweekandwinner")>]
    member this.GetLastGameWeekAndWinner() =
        () |> (switch getGlobalLastGameWeekAndWinner >> resultToHttp)

    [<Route("getopenfixtureswithnopredictionsforplayercount")>]
    member this.GetOpenFixturesWithNoPredictionsForPlayerCount() =
        let player = this.GetLoggedInPlayerId() |> getLoggedInPlayer
        player |> ((switch getOpenFixturesWithNoPredictionsForPlayerCount) >> resultToHttp)

    [<Route("inplay")>]
    member this.GetInPlay() =
        () |> (switch getInPlay >> resultToHttp)

[<Authorize>]
[<CustomAuthorize>]
[<RoutePrefix("api/admin")>]
type AdminController() =
    inherit ApiController()


    [<Route("getgameweekswithclosedfixtures")>]
    member this.GetGameWeeksWithClosedFixtures() =
        () |> ((switch getGameWeeksWithClosedFixtures) >> resultToHttp)

    [<Route("getclosedfixturesforgameweek/{gwno:int}")>]
    member this.GetClosedFixturesForGameWeek gwno =
        let getFixtures() = gwno |> GwNo |> getClosedFixturesForGameWeek
        () |> ((switch getFixtures) >> resultToHttp)

    [<HttpPost>][<Route("result")>]
    member this.AddResult (result:ResultPostModel) =
        let saveResult() = trySaveResultPostModel result
        () |> (saveResult >> resultToHttp)
        
    [<Route("gameweek")>]
    member this.GetImportNextGameWeek() =
        () |> (switch getImportNextGameWeekView >> resultToHttp)

    [<HttpPost>][<Route("gameweek")>]
    member this.ImportNextGameWeek() =
        () |> (submitImportNextGameWeek >> resultToHttp)