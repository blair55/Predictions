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
        GwNo gwno |> (switch getGameWeekPointsView >> resultToHttp)
        
    [<Route("fixture/{fxId:Guid}")>]
    member this.GetFixture (fxId:Guid) =
        FxId fxId |> (switch getPlayerPointsForFixture >> resultToHttp)
                
[<RoutePrefix("api/admin")>]
type AdminController() =
    inherit ApiController()

    [<Route("getnewgameweekno")>]
    member this.GetNewGameWeekNo () =
        base.Request |> (makeSurePlayerIsAdmin
                     >> bind (switch getNewGameWeekNo)
                     >> bind (switch (fun gwno -> getGameWeekNo gwno))
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

    // when adding gameweek:
        // cannot add fixture to gameweek with same home & away teams
        // cannot add fixture to gameweek with ko in past
        // cannot save gameweek with no fixtures

    // when saving prediction/result:
        // cannot add score with negative scores

    // cannot save result to fixture with ko in future
    // cannot save prediction to fixture with ko in past
    // cannot view fixture with ko in future

type Player = { id:PlId; name:string; role:Role }
type Prediction = { score:Score; player:Player }
type Result = { score:Score }
type FixtureData = { id:FxId; home:Team; away:Team; kickoff:KickOff; predictions:Prediction list; }

type Fixture =
    | OpenFixture of FixtureData
    | ClosedFixture of (FixtureData * Result option)

type GameWeekData = { number:GwNo; description:string; }

type GameWeek =
    | EmptyGameWeek of GameWeekData
    | GameWeekWithFixtures of (GameWeekData * Fixture list)


module Rules =

    let initGameWeek gwno = EmptyGameWeek {number=gwno; description=""}

    let addFixtureToGameWeek f gw =
        match gw with
        | EmptyGameWeek gwd -> GameWeekWithFixtures(gwd, [f]) 
        | GameWeekWithFixtures (gwd, fs) -> GameWeekWithFixtures(gwd, f::fs)

    let saveGameWeek gw =
        match gw with
        | EmptyGameWeek gwd -> Failure "cannot save empty gameweek"
        | GameWeekWithFixtures (gwd, fs) -> Success () // persist gw...

    let tryAddResultToFixture r f =
        match f with
        | OpenFixture f -> Failure "cannot add result to fixture with ko in future"
        | ClosedFixture (f, _) -> Success(ClosedFixture(f, Some r))

    let tryAddPredictionToFixture p f =
        match f with
        | OpenFixture f -> Success(OpenFixture({f with predictions=p::f.predictions}))
        | ClosedFixture _ -> Failure "cannot add prediction to fixture with ko in past"

    let tryGetFixtureToView f =
        match f with
        | OpenFixture _ -> Failure "cannot view fixture with ko in future"
        | ClosedFixture f -> Success(ClosedFixture f)

    let newFxId = Guid.NewGuid()|>FxId
    
    let tryToCreateScoreFromSbm home away =
        if home >= 0 && away >= 0 then Success(home, away) else Failure "scores must be positive"

    let tryToCreateFixtureDataFromSbm home away ko =
        if home = away then Failure "fixture home and away team cannot be the same"
        else if ko > DateTime.Now then Failure "fixture not in the future"
        else Success({id=newFxId; home=home; away=away; kickoff=ko; predictions=[]})
    
    let readFixtureFromDb f =
        match f.kickoff > DateTime.Now with
        | true -> OpenFixture f
        | false -> ClosedFixture (f, None)