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
                     >> bind (trySavePredictionPostModel prediction)
                     >> resultToHttp)
                     
    [<Route("editpredictions")>]
    member this.GetEditPredictions() =
        base.Request |> (getPlayerIdCookie
                     >> bind (switch getOpenFixturesWithPredictionsForPlayer)
                     >> resultToHttp)
                
    [<HttpPost>][<Route("editprediction")>]
    member this.EditPrediction (prediction) =
        base.Request |> (getPlayerIdCookie
                     >> bind (tryEditPrediction prediction)
                     >> resultToHttp)

    [<Route("pastgameweeks")>]
    member this.GetPastGameWeeks() =
        () |> (switch getPastGameWeeksWithWinner >> resultToHttp)
        
    [<Route("gameweekscores/{gwno:int}")>]
    member this.GetGameWeekPoints (gwno:int) =
        GwNo gwno |> (switch getGameWeekPointsView >> resultToHttp)
        
    [<Route("fixture/{fxId:Guid}")>]
    member this.GetFixture (fxId:Guid) =
        FxId fxId |> (getPlayerPointsForFixture >> resultToHttp)
    
    [<Route("leagueposition")>]
    member this.GetLeaguePosition() =
        () |> ((switch getLeaguePositionGraphData) >> resultToHttp)

[<RoutePrefix("api/admin")>]
type AdminController() =
    inherit ApiController()

    [<Route("getnewgameweekno")>]
    member this.GetNewGameWeekNo () =
        base.Request |> (makeSurePlayerIsAdmin
                     >> bind (switch getNewGameWeekNo)
                     >> bind (switch (fun gwno -> getGameWeekNo gwno))
                     >> resultToHttp)

    [<HttpPost>][<Route("gameweek")>]
    member this.CreateGameWeek (gameWeek:GameWeekPostModel) =
        let saveGameWeek() = trySaveGameWeekPostModel gameWeek
        base.Request |> (makeSurePlayerIsAdmin
                     >> bind saveGameWeek
                     >> resultToHttp)
        
    [<Route("getfixturesawaitingresults")>]
    member this.GetFixturesAwaitingResults() =
        base.Request |> (makeSurePlayerIsAdmin
                     >> bind (switch getFixturesAwaitingResults)
                     >> resultToHttp)

    [<HttpPost>][<Route("result")>]
    member this.AddResult (result:ResultPostModel) =
        let saveResult() = trySaveResultPostModel result
        base.Request |> (makeSurePlayerIsAdmin
                     >> bind saveResult
                     >> resultToHttp)
