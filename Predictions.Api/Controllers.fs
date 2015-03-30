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

[<Authorize>]
[<RoutePrefix("api")>]
type HomeController() =
    inherit ApiController()

    [<Route("whoami")>]
    member this.GetWhoAmI() =
        base.Request |> (getLoggedInPlayerAuthToken
                     >> bind getPlayerFromAuthToken
                     >> bind (switch getPlayerViewModel)
                     >> resultToHttp)

    [<Route("auth/{authToken}")>]
    member this.GetAuthenticate (authToken:string) =
        let login = logPlayerIn base.Request
        authToken |> (getPlayerFromAuthToken
                  >> bind (switch login)
                  >> httpResponseFromResult)
        
    [<Route("logout")>]
    member this.GetLogOut() =
        base.Request |> logPlayerOut

    [<Route("leaguetable")>]
    member this.GetLeagueTable () =
        () |> (switch getLeagueTableView >> resultToHttp)
        
    [<Route("player/{playerId:Guid}")>]
    member this.GetPlayer (playerId:Guid) =
        playerId |> (switch getGameWeeksPointsForPlayer >> resultToHttp)

    [<Route("playergameweek/{playerId:Guid}/{gameWeekNo:int}")>]
    member this.GetPlayerGameWeek (playerId:Guid) (gameWeekNo:int) =
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
    member this.GetLeaguePositionGraph (plId:Guid) =
        PlId plId |> ((switch getLeaguePositionGraphDataForPlayer) >> resultToHttp)
    
    [<Route("fixturepredictiongraph/{fxId:Guid}")>]
    member this.GetFixturePredictionGraph (fxId:Guid) =
        FxId fxId |> (getFixturePredictionGraphData >> resultToHttp)

    [<Route("getleaguepositionforplayer")>]
    member this.GetLeaguePositionForPlayer() =
        base.Request |> (getLoggedInPlayerAuthToken
                     >> bind getPlayerFromAuthToken
                     >> bind getLeaguePositionForPlayer
                     >> resultToHttp)
                     
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
