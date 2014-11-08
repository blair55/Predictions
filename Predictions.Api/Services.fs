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
    let toScoreViewModel (s:Score) = { ScoreViewModel.home=(fst s); away=(snd s) }
    let noScoreViewModel = { ScoreViewModel.home=0; away=0 }
    let toFixtureViewModel (fd:FixtureData) (gw:GameWeek) =
        let f = fixtureDataToFixture fd None
        let createVm isOpen = { FixtureViewModel.home=fd.home; away=fd.away; fxId=(getFxId fd.id)|>str; kickoff=fd.kickoff; gameWeekNumber=(getGameWeekNo gw.number); isOpen=isOpen }
        f |> isFixtureOpen |> createVm
    let toEditPredictionViewModelRow (f:FixtureData) (gw:GameWeek) (p:Prediction) = { EditPredictionsViewModelRow.home=f.home; away=f.away; fxId=(getFxId f.id)|>str; kickoff=f.kickoff; gameWeekNumber=(getGameWeekNo gw.number); predictionId=(getPrId p.id); score=(toScoreViewModel p.score) }
    let season() = buildSeason currentSeason
    let gameWeeks() = season().gameWeeks |> List.sortBy(fun gw -> gw.number)
    //let getNewGameWeekNo() = getNewGameWeekNo() |> GwNo

    let getImmediateSiblings collection item =
        match collection |> List.tryFindIndex(fun c -> c = item) with
        | None -> (None, None)
        | Some i -> let p = i - 1
                    let n = i + 1
                    let prev = if (p >= 0) then collection.[p] else None
                    let next = if (n <= collection.Length) then collection.[p] else None
                    (prev, next)
        
    let predictionOptionToScoreViewModel (pr:Prediction option) =
        match pr with
        | Some p -> toScoreViewModel p.score
        | None -> noScoreViewModel

    let toOpenFixtureViewModelRow (gw:GameWeek, fd:FixtureData, pr:Prediction option) =
        { OpenFixturesViewModelRow.fixture=(toFixtureViewModel fd gw); scoreSubmitted=pr.IsSome; newScore=None; existingScore=pr|>predictionOptionToScoreViewModel }

    let getOpenFixturesForPlayer (playerId:string) =
        let plId = PlId (sToGuid playerId)
        let players = getPlayers()
        let rows = season().gameWeeks
                   |> List.map(fun gw -> gw, getOpenFixturesAndPredictionForPlayer [gw] players plId)
                   |> List.map(fun (gw, fdps) -> fdps |> List.map(fun (fd, p) -> toOpenFixtureViewModelRow(gw,fd,p)))
                   |> List.collect(fun ofvmr -> ofvmr)
                   |> List.sortBy(fun ofvmr -> ofvmr.fixture.kickoff)
        { OpenFixturesViewModel.rows=rows }

    let getOpenFixturesWithPredictionsForPlayer(playerId:string) =
        let plId = PlId (sToGuid playerId)
        let players = getPlayers()
        let rows = season().gameWeeks
                   |> List.map(fun (gw) -> gw, getOpenFixturesWithPredictionForPlayer [gw] players plId)
                   |> List.map(fun (gw, fdps) -> fdps |> List.map(fun (fd,pr) -> toEditPredictionViewModelRow fd gw pr))
                   |> List.collect(fun fvm -> fvm)
                   |> List.sortBy(fun fvm -> fvm.kickoff)
        { EditPredictionsViewModel.rows = rows }

    let getFixturesAwaitingResults() =
        let rows = gameWeeks()
                   |> List.map(fun gw -> gw, getFixturesForGameWeeks [gw])
                   |> List.map(fun (gw, fixtures) -> gw, (fixtures |> List.choose(onlyClosedFixtures) |> List.map(fixtureToFixtureDataWithResult)))
                   |> List.collect(fun (gw, fdrs) -> fdrs |> List.map(fun (fd, r) -> gw, fd, r))
                   |> List.filter(fun (_, _, r) -> r.IsNone)
                   |> List.map(fun (gw, fd, _) -> toFixtureViewModel fd gw)
        { FixturesAwaitingResultsViewModel.rows = rows }
        
    let getFixtureViewDetails (gw, (fd:FixtureData), r, players) =
        let getPredictionScoreViewModel (prediction:Prediction option) =
            match prediction with
            | Some p -> toScoreViewModel p.score
            | None -> { ScoreViewModel.home=0; away=0 }
        let getResultScoreViewModel (result:Result option) =
            match result with
            | Some r -> toScoreViewModel r.score
            | None -> { ScoreViewModel.home=0; away=0 }
        let rows = players
                   |> List.map(fun plr -> plr, (fd.predictions |> List.tryFind(fun pr -> pr.player = plr)))
                   |> List.map(fun (plr, pr) -> plr, pr, (getBracketForPredictionComparedToResult pr r) |> getPointsForBracket)
                   |> List.map(fun (plr, pr, pts) -> { FixturePointsRowViewModel.player=(getPlayerViewModel plr); predictionSubmitted=pr.IsSome; prediction=(getPredictionScoreViewModel pr); points=pts })
        { FixturePointsViewModel.fixture=(toFixtureViewModel fd gw); resultSubmitted=r.IsSome; result=r|>getResultScoreViewModel; rows=rows }

    let getPlayerPointsForFixture (fxid:FxId) =
        let players = getPlayers()
        let findFixtureAndGameWeek() =
            match tryFindFixtureWithGameWeek (gameWeeks()) fxid with
            | Some f -> Success f
            | None -> Failure "could not find fixture"
        let viewFixture (gw, f, _) =
            match tryViewFixture f with
            | Success (fd, r) -> Success(gw, fd, r, players)
            | Failure msg -> Failure msg
        () |> (findFixtureAndGameWeek
           >> bind viewFixture
           >> bind (switch getFixtureViewDetails))

    //let getGameWeeks() = readGameWeeks() |> List.sortBy(fun gw -> gw.number)
    let leagueTableRowToViewModel (diffPos, pos, pl, cs, co, pts) = { LeagueTableRowViewModel.diffPos=diffPos; position=pos; player=getPlayerViewModel pl; correctScores=cs; correctOutcomes=co; points=pts }

    let getLeagueTableView() =
        let players = getPlayers()
        let gws = gameWeeks()
        let gwsWithResults = getGameWeeksWithAnyClosedFixturesWithResults gws
        let mxGw = gwsWithResults |> List.max
        let gwsWithResultsWithoutMax = gwsWithResults |> List.filter(fun gw -> gw <> mxGw)
        let recentFixtures = getFixturesForGameWeeks gwsWithResults
        let priorFixtures = getFixturesForGameWeeks gwsWithResultsWithoutMax
        let recentLge = getLeagueTable players recentFixtures
        let priorLge = getLeagueTable players priorFixtures

        let findPlayerPriorPos player currentPos =
            let playerPriorLgeRow = priorLge |> List.tryFind(fun (_, pl, _, _, _) -> pl = player)
            match playerPriorLgeRow with | Some (pos, _, _, _, _) -> pos | None -> currentPos
        let toDiffLgeRow (pos, pl, cs, co, pts) =
            let priorPos = findPlayerPriorPos pl pos
            let diffPos = priorPos - pos
            (diffPos, pos, pl, cs, co, pts)
        let rows = recentLge |> List.map(toDiffLgeRow) |> List.map(leagueTableRowToViewModel)
        { LeagueTableViewModel.rows=rows }

    let getGameWeekPointsView gwno =
        let players = getPlayers()
        let fixtures = gameWeeks() |> List.filter(fun gw -> gw.number = gwno) |> getFixturesForGameWeeks
        let month = fixtures |> List.map(fixtureToFixtureData) |> List.minBy(fun fd -> fd.kickoff) |> fun fd -> fd.kickoff.ToString("MMMM")
        let rows = (getLeagueTable players fixtures) |> List.map(fun (pos, pl, cs, co, pts) -> leagueTableRowToViewModel (0, pos, pl, cs, co, pts))
        { GameWeekPointsViewModel.gameWeekNo=(getGameWeekNo gwno); rows=rows; month=month }
        
    let getPlayerFromGuid guid =
        let player = getPlayers() |> List.tryFind(fun plr -> plr.id = PlId guid)
        match player with
        | Some p -> Success (p|>getPlayerViewModel)
        | None -> Failure (sprintf "no player found matching id %s" (str guid))

    let getGameWeeksPointsForPlayer playerId =
        let getPlayerGameWeeksViewModelRow ((gw:GameWeek), r) =
            match r with
            | Some (pos, _, cs, co, pts) -> {PlayerGameWeeksViewModelRow.gameWeekNo=(getGameWeekNo gw.number); position=pos; correctScores=cs; correctOutcomes=co; points=pts}
            | None -> {PlayerGameWeeksViewModelRow.gameWeekNo=(getGameWeekNo gw.number); position=0; correctScores=0; correctOutcomes=0; points=0}
        let players = getPlayers()
        let gameWeeks = gameWeeks()
        let player = findPlayerById players (playerId|>PlId)
        let rows = (getPlayerPointsForGameWeeks players player gameWeeks) |> List.map(getPlayerGameWeeksViewModelRow)

        { PlayerGameWeeksViewModel.player=(getPlayerViewModel player); rows=rows }
        
    let getPastGameWeeksWithWinner() =
        let players = getPlayers()
        let rows = (getPastGameWeeksWithWinner (gameWeeks()) players)
                   |> List.map(fun (gw, plr, pts) -> { PastGameWeekRowViewModel.gameWeekNo=(getGameWeekNo gw.number); winner=(getPlayerViewModel plr); points=pts })
        { PastGameWeeksViewModel.rows = rows }

    let getPastMonthsWithWinner() =
        let players = getPlayers()
        let rows = (getPastMonthsWithWinner (gameWeeks()) players)
                   |> List.map(fun (m, plr, pts) -> { HistoryByMonthRowViewModel.month=m; winner=(getPlayerViewModel plr); points=pts })
        { HistoryByMonthViewModel.rows = rows }
    
    let getMonthPointsView month =
        let players = getPlayers()
        let gws = month |> getGameWeeksForMonth (gameWeeks())
        let fixtures = gws |> getFixturesForGameWeeks
        let rows = (getLeagueTable players fixtures) |> List.map(fun (pos, pl, cs, co, pts) -> leagueTableRowToViewModel (0, pos, pl, cs, co, pts))
        { HistoryByMonthWithMonthViewModel.month=month; rows=rows; gameweeks=gws|>List.map(fun gw -> gw.number|>getGameWeekNo) }
    

    let getPlayerGameWeek playerId gameWeekNo =
        let players = getPlayers()
        let player = findPlayerById players (playerId|>PlId)
        let gw = gameWeeks() |> List.find(fun gw -> (getGameWeekNo gw.number) = gameWeekNo)
        let rowToViewModel (fd, (r:Result option), (p:Prediction option), pts) =
            let getVmPred (pred:Prediction option) =
                match pred with
                | Some p -> { ScoreViewModel.home=fst p.score;away=snd p.score }
                | None -> noScoreViewModel
            let getVmResult (result:Result option) =
                match result with
                | Some r -> { ScoreViewModel.home=fst r.score;away=snd r.score }
                | None -> noScoreViewModel
            { GameWeekDetailsRowViewModel.fixture=(toFixtureViewModel fd gw); predictionSubmitted=p.IsSome; prediction=getVmPred p; resultSubmitted=r.IsSome; result=getVmResult r; points=pts }
        let rows = (getGameWeekDetailsForPlayer player gw) |> List.map(rowToViewModel) |> List.sortBy(fun g -> g.fixture.kickoff)
        { GameWeekDetailsViewModel.gameWeekNo=gameWeekNo; player=(getPlayerViewModel player); totalPoints=rows|>List.sumBy(fun r -> r.points); rows=rows }

    // [1;2;3;4;5]  []
    // [2;3;4;5]    [[1]]
    // [3;4;5]      [[1];[1;2]]
    // [4;5]        [[1];[1;2];[1;2;3]]
    // [5]          [[1];[1;2];[1;2;3];[1,2,3,4]]
    // []           [[1];[1;2];[1;2;3];[1,2,3,4];[1;2;3;4;5]]
    
    let rec compoundList comp collection =
        match collection with
        | h::t -> compoundList ((h::t)::comp) t
        | [] -> comp

    let getLeaguePositionGraphDataForPlayer playerId =
        let players = getPlayers()
        let gws = gameWeeks()
        let fixtures = gameWeeks() |> compoundList [] |> List.map(getFixturesForGameWeeks)
        let labels = gws |> List.map(fun gw -> "GW#" + (gw.number|>getGameWeekNo|>str))
        let data = players
                   |> List.filter(fun p -> p.id = playerId)
                   |> List.map(fun plr -> (plr, fixtures |> List.map(fun fs -> getLeaguePositionForFixturesForPlayer fs players plr)))
                   |> List.map(fun (_, posList) -> posList)
        { LeaguePositionGraphData.data=data; labels=labels }

    let getFixturePredictionGraphData fxid =
        let gws = gameWeeks()
        let makeSureFixtureExists fxid =
            let fixture = tryFindFixture gws (fxid|>FxId)
            match fixture with
            | Some f -> Success f
            | None -> Failure "fixture does not exist"
        fxid |> (makeSureFixtureExists
             >> bind (switch fixtureToFixtureData)
             >> bind (switch (fun fd -> GetOutcomeCounts fd.predictions (0, 0, 0)))
             >> bind (switch (fun (hw, d, aw) -> { FixturePredictionGraphData.data=[hw; d; aw;]; labels=["home win"; "draw"; "away win"] })))

    let getGameWeeksWithClosedFixtures() =
        let rows = gameWeeks()
                   |> getGameWeeksWithClosedFixtures
                   |> List.map(fun gw -> gw.number|>getGameWeekNo)
        { GameWeeksWithClosedFixturesViewModel.rows=rows }

    let resultToScoreViewModel (result:Result option) =
        match result with
        | Some r -> toScoreViewModel r.score
        | None -> noScoreViewModel

    let getClosedFixturesForGameWeek gwno =
        let rows = gameWeeks()
                   |> List.filter(fun gw -> gw.number = gwno)
                   |> List.map(fun gw -> gw, getClosedFixturesForGameWeeks [gw])
                   |> List.map(fun (gw, fdr) -> fdr |> List.map(fun (fd, r) -> { OpenFixturesViewModelRow.fixture=(toFixtureViewModel fd gw); newScore=None; scoreSubmitted=r.IsSome; existingScore=r|>resultToScoreViewModel }))
                   |> List.collect(fun o -> o)
        { ClosedFixturesForGameWeekViewModel.gameWeekNo=gwno|>getGameWeekNo; rows=rows }

    let optionToResult x msg = match x with | Some y -> Success y | None -> Failure msg


    let getLeaguePositionForPlayer playerId =
        let plId = PlId (sToGuid playerId)
        let players = getPlayers()
        let findP id = players |> List.tryFind(fun p -> p.id = id)
        let fixtures = gameWeeks() |> getFixturesForGameWeeks
        let tryFindPlayer id = optionToResult (findP id) "could not find player"
        let getLeaguePosition = getLeaguePositionForFixturesForPlayer fixtures players
        plId |> (tryFindPlayer >> bind (switch getLeaguePosition))

    let getLastGameWeekAndWinner() = getPastGameWeeksWithWinner().rows |> Seq.last

    // persistence

    let trySaveResultPostModel (rpm:ResultPostModel) =
        let gws = gameWeeks()
        let fxid = FxId (sToGuid rpm.fixtureId)
        let createScore() = tryToCreateScoreFromSbm rpm.score.home rpm.score.away
        let createResult score = { Result.id=newRsId(); score=score }
        let makeSureFixtureExists r =
            match (tryFindFixture gws fxid) with
            | Some _ -> Success (fxid,r)
            | None -> Failure "fixture does not exist"
        let saveResult (fxid,(r:Result)) = saveResult { SaveResultCommand.id=r.id; fixtureId=fxid; score=r.score }
        () |> (createScore
           >> bind (switch createResult)
           >> bind makeSureFixtureExists
           >> bind (switch saveResult))

    let rec tryCreateFixturesFromPostModels (viewModels:FixturePostModel list) fixtures =
        match viewModels with
        | h::t -> let fixtureData = tryToCreateFixtureDataFromSbm h.home h.away h.kickOff
                  match fixtureData with
                  | Success fd -> tryCreateFixturesFromPostModels t (fd::fixtures)
                  | Failure msg -> Failure msg
        | [] -> Success fixtures

    let trySaveGameWeekPostModel (gwpm:GameWeekPostModel) =
        //let gwno = getNewGameWeekNo()
        let snid = season().id
        let createFixtures viewModels = tryCreateFixturesFromPostModels viewModels []
        let createGameWeek fixtures = { SaveGameWeekCommand.id=newGwId(); seasonId=snid; fixtures=fixtures; description="" }
        gwpm.fixtures |> (createFixtures
                      >> bind (switch createGameWeek)
                      >> bind (switch saveGameWeek))

    let trySavePrediction (ppm:PredictionPostModel) (playerId:string) =
        let players = getPlayers()
        let gws = gameWeeks()
        let plid = playerId |> sToGuid |> PlId
        let fxId = FxId (sToGuid ppm.fixtureId)
        let player = players |> List.tryFind(fun p -> p.id = plid)
        let findPlayer plr = match plr with | Some p -> Success p | None -> Failure "could not find player" 
        let makeSureFixtureExists p = match (tryFindFixture gws fxId) with | Some f -> Success (p, f) | None -> Failure "fixture does not exist"
        let makeSureFixtureIsOpen (plr, f:Fixture) = match f with | OpenFixture fd -> Success (plr, fd) | ClosedFixture f -> Failure "fixture is closed"
        let createScore (plr, fd) = match tryToCreateScoreFromSbm ppm.score.home ppm.score.away with | Success s -> Success(plr, fd, s) | Failure msg -> Failure msg
        let createPrediction (plr, fd, s) = (plr, fd, (createPrediction plr s))
        let updatePrediction ((plr:Player), (fd:FixtureData), (pr:Prediction)) = savePrediction { SavePredictionCommand.id=pr.id; fixtureId=fd.id; playerId=plr.id; score=pr.score }
        player |> (findPlayer
               >> bind makeSureFixtureExists
               >> bind makeSureFixtureIsOpen
               >> bind createScore
               >> bind (switch createPrediction)
               >> bind (switch updatePrediction))