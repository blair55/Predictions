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
    let toFixtureViewModel (f:FixtureData) (gw:GameWeek)= { home=f.home; away=f.away; fxId=(getFxId f.id)|>str; kickoff=f.kickoff; gameWeekNumber=(getGameWeekNo gw.number) }
    let toScoreViewModel (s:Score) = { ScoreViewModel.home=(fst s); away=(snd s) }
    let noScoreViewModel = { ScoreViewModel.home=0; away=0 }
    let getNewGameWeekNo() = getNewGameWeekNo() |> GwNo

    let longStrToDateTime (s:string) =
        let d = s.Split('+').[0];
        Convert.ToDateTime(d)
    
    let season() = buildSeason Const.CurrentSeason

//    let rec validateFixtures fixtures r =
//        match fixtures with
//        | h::t -> let res = validateFixture h
//                  match res with
//                  | Success _ -> validateFixtures t r
//                  | Failure _ -> res
//        | [] -> r

    
        
    // try save gameweek
    let tryToSaveGameWeek gw snid =
        let addGw() = addGameWeek gw snid; gw
        tryToWithReturn addGw

    let tryToSaveFixtures fixtures =
        let addFixtures() = fixtures |> List.iter(fun f -> addFixture f); ()
        tryToWithReturn addFixtures

    // try save fixtures
    let saveGameWeekPostModel (gwpm:GameWeekPostModel) =
        let gwno = getNewGameWeekNo()
        let gw = createGameWeek gwno
        let fixtures = gwpm.fixtures |> List.map(fun f -> createFixture f.home f.away f.kickOff)
        let snid = 
        let saveGameWeek snid = 
        let saveFixtures gw = tryToSaveFixtures fixtures
        gw |> (tryToSaveGameWeek >> bind saveFixtures)

    let getOpenFixturesForPlayer (playerId:string) =
        let plId = PlId (sToGuid playerId)
        let players = getPlayers()
        let season = season()
        let rows = season.gameWeeks
                   |> List.map(fun gw -> gw, getFixturesForGameWeeks [gw])
                   |> List.map(fun (gw, fixtures) -> gw, getOpenFixturesForPlayer fixtures players plId)
                   |> List.map(fun (gw, fds) -> fds |> List.map(fun fd -> toFixtureViewModel fd gw))
                   |> List.collect(fun e -> e)
        { OpenFixturesViewModel.rows=rows }
        
    let getFixturesAwaitingResults() =
        let (_, fixtures, results) = getGameWeeksAndFixturesAndResults()
        let rows = getFixturesAwaitingResults fixtures results |> List.map(toFixtureViewModel)
        { FixturesAwaitingResultsViewModel.rows = rows }
        
    let getPastGameWeeksWithWinner() =
        let gws = season().gameWeeks
        let players = getPlayers()
        let rows = (getPastGameWeeksWithWinner gws players)
                   |> List.map(fun (gw, plr, pts) -> { PastGameWeekRowViewModel.gameWeekNo=(getGameWeekNo gw.number); winner=(getPlayerViewModel plr); points=pts })
        { PastGameWeeksViewModel.rows = rows }


    let getFixtureViewDetails (gw, (fd:FixtureData), r, players) =
        let getPredictionScoreViewModel (prediction:Prediction option) =
            match prediction with
            | Some p -> toScoreViewModel p.score
            | None -> { ScoreViewModel.home=0; away=0 }
        let getResultScoreViewModel (result:Result option) =
            match result with
            | Some r -> toScoreViewModel r.score
            | None -> { ScoreViewModel.home=0; away=0 }
        let rows = players |> List.sort
                   |> List.map(fun plr -> plr, (fd.predictions |> List.tryFind(fun pr -> pr.player = plr)))
                   |> List.map(fun (plr, pr) -> plr, pr, (getBracketForPredictionComparedToResult pr r) |> getPointsForBracket)
                   |> List.map(fun (plr, pr, pts) -> { FixturePointsRowViewModel.player=(getPlayerViewModel plr); predictionSubmitted=pr.IsSome; prediction=(getPredictionScoreViewModel pr); points=pts })
        { FixturePointsViewModel.fixture=(toFixtureViewModel fd gw); resultSubmitted=r.IsSome; result=r|>getResultScoreViewModel; rows=rows }

    let getPlayerPointsForFixture (fxid:FxId) =
        let gws = season().gameWeeks
        let players = getPlayers()
        
        let findFixtureAndGameWeek() =
            match tryFindFixtureWithGameWeek gws fxid with
            | Some f -> Success f
            | None -> Failure "could not find fixture"

        let viewFixture (gw, f, _) =
            match tryViewFixture f with
            | Success (fd, r) -> Success(gw, fd, r, players)
            | Failure msg -> Failure msg

        () |> (findFixtureAndGameWeek
           >> bind viewFixture
           >> bind (switch getFixtureViewDetails))


    let getPlayer playerId = getPlayerById (playerId|>PlId)

    let getGameWeeks() = readGameWeeks() |> List.sortBy(fun gw -> gw.number)
        
    let leagueTableRowToViewModel (pos, pl, cs, co, pts) = { LeagueTableRowViewModel.position=pos; player=getPlayerViewModel pl; correctScores=cs; correctOutcomes=co; points=pts }

    let getLeagueTableView() =
        let players = getPlayers()
        let gws = season().gameWeeks
        let fixtures = getFixturesForGameWeeks gws
        let rows = (getLeagueTable players fixtures) |> List.map(leagueTableRowToViewModel)
        { LeagueTableViewModel.rows=rows }

    let getGameWeekPointsView gwno =
        let players = getPlayers()
        let gws = season().gameWeeks |> List.filter(fun gw -> gw.number = gwno)
        let fixtures = getFixturesForGameWeeks gws
        let rows = (getLeagueTable players fixtures) |> List.map(leagueTableRowToViewModel)
        { GameWeekPointsViewModel.gameWeekNo = (getGameWeekNo gwno); rows=rows }
        


    let getGameWeeksPointsForPlayer playerId =
        let getPlayerGameWeeksViewModelRow ((gw:GameWeek), r) =
            match r with
            | Some (pos, _, cs, co, pts) -> {PlayerGameWeeksViewModelRow.gameWeekNo=(getGameWeekNo gw.number); position=pos; correctScores=cs; correctOutcomes=co; points=pts}
            | None -> {PlayerGameWeeksViewModelRow.gameWeekNo=(getGameWeekNo gw.number); position=0; correctScores=0; correctOutcomes=0; points=0}
        let players = getPlayers()
        let season = season()
        let gameWeeks = season.gameWeeks
        let player = findPlayerById players (playerId|>PlId)
        let x = getPlayerPointsForGameWeeks players player gameWeeks
        let rows = x |> List.map(getPlayerGameWeeksViewModelRow)
        { PlayerGameWeeksViewModel.player=(getPlayerViewModel player); rows=rows }

    let getFixtureDetail fxid =
        let (_, results, predictions) = getPlayersAndResultsAndPredictions()
        getPlayerPredictionsForFixture predictions results (FxId fxid)

    let getPlayerGameWeek playerId gameWeekNo =
        let players = getPlayers()
        let player = findPlayerById players (playerId|>PlId)
        let season = season()
        let gw = season.gameWeeks |> List.find(fun gw -> (getGameWeekNo gw.number) = gameWeekNo)
        let rowToViewModel (fd, (r:Result option), (p:Prediction option), pts) =
            let getVmPred (pred:Prediction option) =
                match pred with
                | Some p -> { ScoreViewModel.home=fst p.score;away=snd p.score }
                | None -> { ScoreViewModel.home=0;away=0 }
            let getVmResult (result:Result option) =
                match result with
                | Some p -> { ScoreViewModel.home=fst p.score;away=snd p.score }
                | None -> { ScoreViewModel.home=0;away=0 }
            { GameWeekDetailsRowViewModel.fixture=(toFixtureViewModel fd gw); predictionSubmitted=p.IsSome; prediction=getVmPred p; resultSubmitted=r.IsSome; result=getVmResult r; points=pts }
        let rows = (getGameWeekDetailsForPlayer player gw) |> List.map(rowToViewModel) |> List.sortBy(fun g -> g.fixture.kickoff)
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
        let player = getPlayers() |> List.find(fun p -> p.id = plId)
        let gws = season().gameWeeks

        let createScore() = tryToCreateScoreFromSbm ppm.score.home ppm.score.away
        let createPrediction score = createPrediction player score
        let makeSureFixtureExists p =
            match (tryFindFixture gws fxId) with
            | Some f -> Success (p, f)
            | None -> Failure "fixture does not exist"
        let addPredictionToFixture (p, f) =
            tryAddPredictionToFixture p f
        let trySavePrediction (p, f) =
            let fd = fixtureToFixtureData f
            let addPredictionWithReturn() = addPrediction p fd.id; ()
            tryToWithReturn addPredictionWithReturn
        
        () |> (createScore
               >> bind (switch createPrediction)
               >> bind makeSureFixtureExists
               >> bind addPredictionToFixture
               >> bind trySavePrediction)

    // web helpers


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
