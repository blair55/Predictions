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

[<RoutePrefix("api")>]
type HomeController() =
    inherit ApiController()
    
    [<Route("whoami")>]
    member this.GetWhoAmI() =
        base.Request |> (getPlayerIdCookie
        >> bind convertStringToGuid
        >> bind getPlayerFromGuid
        >> getWhoAmIResponse)

    [<Route("auth/{playerId:Guid}")>]
    member this.GetAuthenticate (playerId:Guid) =
        playerId |> (getPlayerFromGuid >> doLogin base.Request)
    
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
        base.Request |> (getPlayerIdCookie
                     >> bind (switch getOpenFixturesForPlayer)
                     >> resultToHttp)
                
    [<HttpPost>][<Route("prediction")>]
    member this.AddPrediction (prediction) =
        base.Request |> (getPlayerIdCookie
                     >> bind (trySavePrediction prediction)
                     >> resultToHttp)
                     
    [<Route("editpredictions")>]
    member this.GetEditPredictions() =
        base.Request |> (getPlayerIdCookie
                     >> bind (switch getOpenFixturesWithPredictionsForPlayer)
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
        GwNo gwno |> (switch getGameWeekPointsView >> resultToHttp)

    [<Route("fixture/{fxId:Guid}")>]
    member this.GetFixture (fxId:Guid) =
        FxId fxId |> (getPlayerPointsForFixture >> resultToHttp)
    
    [<Route("leaguepositiongraphforplayer/{plId:Guid}")>]
    member this.GetLeaguePositionGraph (plId:Guid) =
        PlId plId |> ((switch getLeaguePositionGraphDataForPlayer) >> resultToHttp)
    
    [<Route("fixturepredictiongraph/{fxId:Guid}")>]
    member this.GetFixturePredictionGraph (fxId:Guid) =
        fxId |> (getFixturePredictionGraphData >> resultToHttp)

    [<Route("getleaguepositionforplayer")>]
    member this.GetLeaguePositionForPlayer() =
        base.Request |> (getPlayerIdCookie
                     >> bind getLeaguePositionForPlayer
                     >> resultToHttp)
                     
    [<Route("getlastgameweekandwinner")>]
    member this.GetLastGameWeekAndWinner() =
        () |> (switch getLastGameWeekAndWinner >> resultToHttp)

    

[<RoutePrefix("api/admin")>]
type AdminController() =
    inherit ApiController()
//
//    [<Route("getnewgameweekno")>]
//    member this.GetNewGameWeekNo () =
//        base.Request |> (makeSurePlayerIsAdmin
//                     >> bind (switch getNewGameWeekNo)
//                     >> bind (switch (fun gwno -> getGameWeekNo gwno))
//                     >> resultToHttp)

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
