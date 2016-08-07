namespace Predictions.Api

open System
open System.Linq
open System.Net
open System.Web
open System.Web.Http
open System.Web.Routing
open System.Net.Http
open System.Net.Http.Headers
open Predictions.Api.Domain
open Predictions.Api.ViewModels
open Predictions.Api.PostModels
open Predictions.Api.Services
open Predictions.Api.Attributes
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
        Logging.info model.provider 
        new ChallengeResult(model.provider, this.Request)

    [<HttpGet>][<Route("time")>]
    member this.GetTime() =
        GMTDateTime.Now()

    //[<HttpPost>][<Route("settime")>]
    //member this.SetTime(model:TimeContainer) =
    //    GMTDateTime.Now <- (fun () -> model.time)
    //    this.Redirect(this.BaseUri)

    //[<HttpPost>][<Route("resettime")>]
    //member this.ResetTime() =
    //    GMTDateTime.Now <- GMTDateTime.defaultTime
    //    this.Redirect(this.BaseUri)

    [<HttpGet>][<Route("logout")>]
    member this.GetLogOut() =
        this.AuthManager.SignOut()
        this.Redirect(this.BaseUri)

    [<HttpGet>][<Route("callback")>]
    member this.GetCallback([<FromUri>]redirect:string) =
        Logging.info (sprintf "redirect=%s" redirect)
        Logging.info "getting ex login info"
        if (box this.AuthManager = null) then
            Logging.errorNx "Authmanager is null"
        else Logging.info (sprintf "notnull=%A" this.AuthManager)
        let loginInfo = this.AuthManager.GetExternalLoginInfo()
        Logging.info (sprintf "providerkey=%s" loginInfo.Login.ProviderKey)
        if (box loginInfo <> null) then
            Logging.info(sprintf "loggedin=%s" loginInfo.DefaultUserName)
            let signInUser = register loginInfo
            this.SignInManager.SignIn(signInUser, true, true)
            let uri = sprintf "%s#%s" (str this.BaseUri) redirect
            this.Redirect(uri)
        else
            Logging.errorNx "should not be null" 
            this.Redirect(this.BaseUri)

[<Authorize>]
[<ActiveUserAuthorize>]
[<LogRoute>]
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

    [<Route("league/global/{page:int}")>]
    member this.GetGlobalLeaguePage(page:int) =
        page |> (switch getGlobalLeagueTablePage >> resultToHttp)

    [<Route("league/{leagueId:Guid}/{page:int=0}")>]
    member this.GetLeagueView (leagueId:Guid, page:int) =
        leagueId |> (getLeagueView page >> resultToHttp)

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

    [<HttpPost>][<Route("leaguedelete/{leagueId:Guid}")>]
    member this.DeleteLeague (leagueId:Guid) =
        let player = this.GetLoggedInPlayerId() |> getLoggedInPlayer
        leagueId |> ((deleteLeague player) >> resultToHttp)

    [<Route("player/{playerId:Guid}/global")>]
    member this.GetGlobalLeaguePlayer (playerId:Guid) =
        playerId |> (getGameWeekPointsForPlayerInGlobalLeague >> resultToHttp)

    [<Route("player/{playerId:Guid}/{leagueId:Guid}")>]
    member this.GetLeaguePlayer (playerId:Guid, leagueId:Guid) =
        (playerId, leagueId) |> (getGameWeeksPointsForPlayerIdAndLeagueId >> resultToHttp)

    [<Route("leaguepositiongraphforplayer/{playerId:Guid}/global")>]
    member this.GetGlobalLeaguePositionGraph (playerId:Guid) =
        playerId |> (getLeaguePositionGraphDataForPlayerInGlobalLeague >> resultToHttp)

    [<Route("leaguepositiongraphforplayer/{playerId:Guid}/{leagueId:Guid}")>]
    member this.GetLeaguePositionGraph (playerId:Guid, leagueId:Guid) =
        (playerId, leagueId) |> (getLeaguePositionGraphDataForPlayerIdAndLeagueId >> resultToHttp)

    [<Route("playergameweek/{playerId:Guid}/{gameWeekNo:int}")>]
    member this.GetPlayerGameWeek (playerId) (gameWeekNo:int) =
        let getPoints = getPlayerGameWeekByPlayerIdAndGameWeekNo gameWeekNo false
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

    [<HttpPost>][<Route("doubledown/{predictionId:Guid}")>]
    member this.DoubleDown (predictionId:Guid) =
        let player = this.GetLoggedInPlayerId() |> getLoggedInPlayer
        player |> ((doubleDown predictionId) >> resultToHttp)

    [<Route("leaguehistory/{leagueId:Guid}/month")>]
    member this.GetHistoryByMonth(leagueId:Guid) =
        leagueId |> (getPastMonthsWithWinnerView >> resultToHttp)

    [<Route("leaguehistory/{leagueId:Guid}/month/{month}/page/{page:int}")>]
    member this.GetHistoryByMonth (leagueId:Guid, month, page) =
        leagueId |> (page |> getMonthPointsView month >> resultToHttp)

    [<Route("leaguehistory/{leagueId:Guid}/gameweek")>]
    member this.GetPastGameWeeks(leagueId:Guid) =
        leagueId |> (getPastGameWeeksWithWinnerView >> resultToHttp)

    [<Route("leaguehistory/{leagueId:Guid}/gameweek/{gwno:int}/page/{page:int}")>]
    member this.GetGameWeekPoints (leagueId:Guid, gwno, page) =
        let getGwPointsView = gwno |> GwNo |> getGameWeekPointsView
        leagueId |> (page |> getGwPointsView >> resultToHttp)

    [<Route("leaguehistory/global/month")>]
    member this.GetGlobalHistoryByMonth() =
        getGlobalLeague() |> (getHistoryByMonthViewModel getGlobalLeagueMircoViewModel |> switch >> resultToHttp)

    [<Route("leaguehistory/global/month/{month}/page/{page:int}")>]
    member this.GetGlobalHistoryByMonth (month, page) =
        getGlobalLeague() |> (getGlobalLeagueMircoViewModel |> getHistoryByMonthWithMonthViewModel month page |> switch >> resultToHttp)

    [<Route("leaguehistory/global/gameweek")>]
    member this.GetGlobalPastGameWeeks() =
        getGlobalLeague() |> (getHistoryByGameWeekViewModel getGlobalLeagueMircoViewModel |> switch >> resultToHttp)

    [<Route("leaguehistory/global/gameweek/{gwno:int}/page/{page:int}")>]
    member this.GetGlobalGameWeekPoints (gwno, page) =
        getGlobalLeague() |> (getGlobalLeagueMircoViewModel |> getHistoryByGameWeekWithGameWeekViewModel (gwno |> GwNo) page |> switch >> resultToHttp)

    [<Route("gameweekmatrix/global/gameweek/{gwno:int}/page/{page:int}")>]
    member this.GetGlobalGameWeekMatrix (gwno, page) =
        gwno |> GwNo |> (getGlobalGameWeekMatrix page >> resultToHttp)

    [<Route("gameweekmatrix/{leagueId:Guid}/gameweek/{gwno:int}/page/{page:int}")>]
    member this.GetGameWeekMatrix (leagueId:Guid, gwno, page) =
        leagueId |> (gwno |> GwNo |> getGameWeekMatrix page >> resultToHttp)

    [<Route("fixture/{fxId:Guid}")>]
    member this.GetFixture (fxId:Guid) =
        let getResult = this.GetLoggedInPlayerId() |> getLoggedInPlayer |> getPlayerPointsForFixture
        FxId fxId |> (getResult >> resultToHttp)

    [<Route("fixturepredictiongraph/{fxId:Guid}")>]
    member this.GetFixturePredictionGraph (fxId:Guid) =
        FxId fxId |> (getFixturePredictionGraphData >> resultToHttp)

    [<Route("fixturedoubledowns/{fxId:Guid}")>]
    member this.GetFixtureDoubleDowns (fxId:Guid) =
        FxId fxId |> (getFixtureDoubleDowns >> resultToHttp)

    [<Route("fixturepreviousmeetings/{fxId:Guid}")>]
    member this.GetFixturePreviousMeetings (fxId:Guid) =
        FxId fxId |> (getFixturePreviousMeetingsView >> resultToHttp)

    [<Route("fixtureformguide/{fxId:Guid}")>]
    member this.GetFixtureFormGuide (fxId:Guid) =
        FxId fxId |> (getFixtureFormGuideView >> resultToHttp)

    [<Route("getleaguepositionforplayer")>]
    member this.Getleaguepositionforplayer() =
        let player = this.GetLoggedInPlayerId() |> getLoggedInPlayer
        player |> (switch getGlobalLeaguePositionforplayer >> resultToHttp)

    [<Route("getlastgameweekandwinner")>]
    member this.GetLastGameWeekAndWinner() =
        () |> (switch getGlobalLastGameWeekAndWinner >> resultToHttp)

    [<Route("getopenfixtureswithnopredictionsforplayercount")>]
    member this.GetOpenFixturesWithNoPredictionsForPlayerCount() =
        let player = this.GetLoggedInPlayerId() |> getLoggedInPlayer
        player |> ((switch getOpenFixturesWithNoPredictionsForPlayerCount) >> resultToHttp)

    [<Route("inplay")>]
    member this.GetInPlay() =
        let player = this.GetLoggedInPlayerId() |> getLoggedInPlayer
        player.id |> getPlayerId |> (getInPlay >> resultToHttp)

    [<Route("getfixtureneighbours/{fxId:Guid}")>]
    member this.GetFixtureNeighbours(fxId:Guid) =
        FxId fxId |> (getFixtureNeighbours >> resultToHttp)

    [<Route("gameweekneighbours/{gwno:int}")>]
    member this.GetGameWeekNeighbours(gwno:int) =
        gwno |> (switch getGameWeekNeighbours >> resultToHttp)

    [<Route("monthneighbours/{month}")>]
    member this.GetMonthNeighbours(month:string) =
        month |> (switch getMonthNeighbours >> resultToHttp)

    [<Route("predictedleaguetable")>]
    member this.GetPredictedLeagueTable() =
        let player = this.GetLoggedInPlayerId() |> getLoggedInPlayer
        player |> (switch getPredictedLeagueTable >> resultToHttp)

[<Authorize>]
[<AdminAuthorize>]
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
