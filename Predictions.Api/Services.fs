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
module Services =
    
    let getPlayerViewModel (p:Player) = { id=getPlayerId p.id|>str; name=p.name; isAdmin=(p.role=Admin) } 
    let toFixtureViewModel (f:Fixture) = { home=f.home; away=f.away; fxId=(getFxId f.id)|>str; kickoff=f.kickoff; gameWeekNumber=(getGameWeekNo f.gameWeek.number) }
    let toScoreViewModel (p:Prediction) = { ScoreViewModel.home=(fst p.score); away=(snd p.score) }
    let getNewGameWeekNo() = getNewGameWeekNo() |> GwNo

    let longStrToDateTime (s:string) =
        let d = s.Split('+').[0];
        Convert.ToDateTime(d)

    let rec validateFixtures fixtures r =
        match fixtures with
        | h::t -> let res = validateFixture h
                  match res with
                  | Success _ -> validateFixtures t r
                  | Failure _ -> res
        | [] -> r

    let buildGameWeek (gwno:GwNo) = { GameWeek.id=Guid.NewGuid()|>GwId; number=gwno; description="" }

    // build gameweek from post model
    let createFixtures (gwpm:GameWeekPostModel) gw =
        let toFixture (f:FixturePostModel) = { Fixture.id=Guid.NewGuid()|>FxId; gameWeek=gw; home=f.home; away=f.away; kickoff=f.kickOff|>longStrToDateTime }
        gwpm.fixtures |> List.map(toFixture)
        
    // try save gameweek
    let tryToSaveGameWeek gw =
        let addGw() = addGameWeek gw; gw
        tryToWithReturn addGw

    let tryToSaveFixtures fixtures =
        let addFixtures() = fixtures |> List.iter(fun f -> addFixture f); ()
        tryToWithReturn addFixtures

    // try save fixtures
    let saveGameWeekPostModel (gwpm:GameWeekPostModel) =
        let gw = getNewGameWeekNo()|>buildGameWeek
        let fixtures = createFixtures gwpm gw
        let saveFixtures gw = tryToSaveFixtures fixtures
        gw |> (tryToSaveGameWeek >> bind saveFixtures)

    let getOpenFixtures (playerId:string) =
        let plId = PlId (sToGuid playerId)
        let (_, fixtures) = getGameWeeksAndFixtures()
        let (players, _, predictions) = getPlayersAndResultsAndPredictions()
        let rows = (getOpenFixturesForPlayer predictions fixtures players plId) |> List.map(toFixtureViewModel)
        { OpenFixturesViewModel.rows=rows }
        
    let getFixturesAwaitingResults() =
        let (_, fixtures, results) = getGameWeeksAndFixturesAndResults()
        let rows = getFixturesAwaitingResults fixtures results |> List.map(toFixtureViewModel)
        { FixturesAwaitingResultsViewModel.rows = rows }
        
    let getPastGameWeeks() =
        let (players, results, predictions) = getPlayersAndResultsAndPredictions()
        let rows = (getPastGameWeeks predictions results) |> List.map(fun (player, gameWeekNo, points) ->
                {PastGameWeekRowViewModel.gameWeekNo=(getGameWeekNo gameWeekNo); winner=(getPlayerViewModel player); points=points})
        { PastGameWeeksViewModel.rows = rows }

    let getPlayerPointsForFixture (fxId:FxId) =
        let (_, fixtures) = getGameWeeksAndFixtures()
        let fixture = findFixtureById fixtures fxId
        let (players, results, predictions) = getPlayersAndResultsAndPredictions()
        let result = results |> List.find(fun r -> r.fixture = fixture)
        let resultScore = { ScoreViewModel.home=(fst result.score); away=(snd result.score) }
        let getPredictionViewModel prediction =
            match prediction with
            | Some p -> toScoreViewModel p
            | None -> { ScoreViewModel.home=0; away=0 }
        let rows = (getPlayerPointsForFixture players predictions results fixture)
                    |> List.map(fun (player, prediction, points) -> { FixturePointsRowViewModel.player=(getPlayerViewModel player); predictionSubmitted=prediction.IsSome; prediction=(getPredictionViewModel prediction); points=points })
                    |> List.sortBy(fun p -> p.player.name)
        { FixturePointsViewModel.fixture=(toFixtureViewModel fixture); result=resultScore; rows=rows }




    let getPlayer playerId = getPlayerById (playerId|>PlId)

    let getGameWeeks() = readGameWeeks() |> List.sortBy(fun gw -> gw.number)
        
    let leagueTableRowToViewModel r = { LeagueTableRowViewModel.position=r.position; player=getPlayerViewModel r.player; correctScores=r.correctScores; correctOutcomes=r.correctOutcomes; points=r.points }

    let getLeagueTableView() =
        let (players, results, predictions) = getPlayersAndResultsAndPredictions()
        let rows = (getLeagueTable predictions results players) |> List.map(leagueTableRowToViewModel)
        { LeagueTableViewModel.rows=rows }

    let getGameWeekPointsView gwno =
        let (players, results, predictions) = getPlayersAndResultsAndPredictions()
        let predictionsForGameWeek = predictions |> List.filter(fun p -> p.fixture.gameWeek.number = gwno)
        let rows = (getLeagueTable predictionsForGameWeek results players) |> List.map(leagueTableRowToViewModel)
        { GameWeekPointsViewModel.gameWeekNo = (getGameWeekNo gwno); rows=rows }
        

    let getGameWeeksPointsForPlayer playerId =
        let (players, results, predictions) = getPlayersAndResultsAndPredictions()
        let gameWeeks = getGameWeeks()
        let player = findPlayerById players (playerId|>PlId)
        let gameWeeksPoints = (getAllGameWeekPointsForPlayer predictions results player gameWeeks)
                              |> List.map(fun (_, gameWeekNo, points) -> { PlayerGameWeekViewModel.gameWeekNo=getGameWeekNo gameWeekNo; points=points })
        { PlayerGameWeeksViewModel.player=(getPlayerViewModel player); rows=gameWeeksPoints }        

    let getFixtureDetail fxid =
        let (_, results, predictions) = getPlayersAndResultsAndPredictions()
        getPlayerPredictionsForFixture predictions results (FxId fxid)

    let getPlayerGameWeek playerId gameWeekNo =
        let (players, results, predictions) = getPlayersAndResultsAndPredictions()
        let player = findPlayerById players (playerId|>PlId)
        let gameWeekDetailRows = getGameWeekDetailsForPlayer predictions results player (gameWeekNo|>GwNo)
        let rowToViewModel (d:GameWeekDetailsRow) =
            let getVmPred (pred:Prediction option) =
                match pred with
                | Some p -> { ScoreViewModel.home=fst p.score;away=snd p.score }
                | None -> { ScoreViewModel.home=0;away=0 }
            {
                GameWeekDetailsRowViewModel.fixture=(toFixtureViewModel d.fixture)
                predictionSubmitted=d.prediction.IsSome
                prediction=getVmPred d.prediction
                result={home=fst d.result.score; away=snd d.result.score}
                points=d.points
            }
        let rows = gameWeekDetailRows |> List.map(rowToViewModel) |> List.sortBy(fun g -> g.fixture.kickoff)
        { GameWeekDetailsViewModel.gameWeekNo=gameWeekNo; player=(getPlayerViewModel player); totalPoints=rows|>List.sumBy(fun r -> r.points); rows=rows }

    let saveResultPostModel (rpm:ResultPostModel) =
        let fxId = FxId (sToGuid rpm.fixtureId)
        let (_, fixtures) = getGameWeeksAndFixtures()
        let fixture = findFixtureById fixtures fxId
        let result = { Result.fixture=fixture; score=(rpm.score.home,rpm.score.away) }
        addResult result

    let trySavePredictionPostModel (ppm:PredictionPostModel) (playerId:string) =
        let plId = PlId (sToGuid playerId)
        let fxId = FxId (sToGuid ppm.fixtureId)
        let (_, fixtures) = getGameWeeksAndFixtures()
        let players = getPlayers()
        let fixture = findFixtureById fixtures fxId
        let player = findPlayerById players plId

        let tryCreatePrediction() =
            let isPredictionForFixtureInTheFuture = fixture |> isFixtureOpen
            match isPredictionForFixtureInTheFuture with
            | true -> Success { Prediction.fixture=fixture; player=player; score=(ppm.score.home,ppm.score.away) }
            | false -> Failure "Fixture has already kicked off" 
        
        let tryAddPrediction p =
            let addPredictionWithReturn() =
                addPrediction p; ()
            tryToWithReturn addPredictionWithReturn
        
        () |> (tryCreatePrediction >> bind tryAddPrediction)

        
    let playerIdCookieName = "playerId"

    let getCookieValue (request:HttpRequestMessage) key =
        let cookie = request.Headers.GetCookies(key) |> Seq.toList |> getFirst
        match cookie with
        | Some c -> Success c.[key].Value
        | None -> Failure "No cookie found"

    let logPlayerIn (request:HttpRequestMessage) (player:PlayerViewModel) =
        let nd d = new Nullable<DateTimeOffset>(d)
        let c = new CookieHeaderValue(playerIdCookieName, player.id)
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

    // get player cookie value
    let getPlayerIdCookie r =
        getCookieValue r playerIdCookieName

    // convert value to guid
    let convertStringToGuid v =
        let (isParsed, guid) = trySToGuid v
        if isParsed then Success guid else Failure (sprintf "could not convert %s to guid" v)

    // get player from guid
    let getPlayerFromGuid guid =
        let player = getPlayer guid
        match player with
        | Some p -> Success (p|>getPlayerViewModel)
        | None -> Failure (sprintf "no player found matching id %s" (str guid))

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

    let checkPlayerIsAdmin (player:PlayerViewModel) =
        match player.isAdmin with
        | true -> Success ()
        | false -> Failure "player not admin"

    let makeSurePlayerIsAdmin req =
        req |> (getPlayerIdCookie
            >> bind convertStringToGuid
            >> bind getPlayerFromGuid
            >> bind checkPlayerIsAdmin)
    
    let resultToHttp result =
        match result with
        | Success body -> getOkResponseWithBody body
        | Failure msg -> unauthorised msg 
