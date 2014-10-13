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

[<EnableCors("*", "*", "*")>]
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
        let rows = getLeagueTableRows() |> List.map(fun r -> { LeagueTableRowViewModel.position=r.position; player=getPlayerViewModel r.player; points=r.points })
        { LeagueTableViewModel.rows=rows }

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
                     >> bind (switch getOpenFixtures)
                     >> resultToHttp)
                     
    [<Route("prediction")>][<HttpPost>]
    member this.AddPrediction (prediction) =
        base.Request |> (getPlayerIdCookie
                     >> bind (trySavePredictionPostModel prediction)
                     >> resultToHttp)

    [<Route("pastgameweeks")>]
    member this.GetPastGameWeeks() =
        () |> (switch getPastGameWeeks >> resultToHttp)
        
    [<Route("gameweekscores/{gwno:int}")>]
    member this.GetGameWeekPoints (gwno:int) =
        GwNo gwno |> (switch getGameWeekPoints >> resultToHttp)
        
    [<Route("fixture/{fxId:Guid}")>]
    member this.GetFixture (fxId:Guid) =
        FxId fxId |> (switch getPlayerPointsForFixture >> resultToHttp)
                
[<EnableCors("*", "*", "*")>]
[<RoutePrefix("api/admin")>]
type AdminController() =
    inherit ApiController()

    [<Route("getnewgameweekno")>]
    member this.GetNewGameWeekNo () =
        base.Request |> (makeSurePlayerIsAdmin
                     >> bind (switch getNewGameWeekNo)
                     >> resultToHttp)

    [<Route("gameweek")>][<HttpPost>]
    member this.CreateGameWeek (gameWeek:GameWeekPostModel) =
        let saveGameWeek() = saveGameWeekPostModel gameWeek
        base.Request |> (makeSurePlayerIsAdmin
                     >> bind saveGameWeek
                     >> resultToHttp)
        
    [<Route("getfixturesawaitingresults")>]
    member this.GetFixturesAwaitingResults() =
        base.Request |> (makeSurePlayerIsAdmin
                     >> bind (switch getFixturesAwaitingResults)
                     >> resultToHttp)

    [<Route("result")>][<HttpPost>]
    member this.AddResult (result:ResultPostModel) =
        let saveResult() = saveResultPostModel result
        base.Request |> (makeSurePlayerIsAdmin
                     >> bind (switch saveResult)
                     >> resultToHttp)
