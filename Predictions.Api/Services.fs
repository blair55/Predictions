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
    let toFixtureViewModel (f:FixtureData) (gw:GameWeek) = { FixtureViewModel.home=f.home; away=f.away; fxId=(getFxId f.id)|>str; kickoff=f.kickoff; gameWeekNumber=(getGameWeekNo gw.number) }
    let toEditPredictionViewModelRow (f:FixtureData) (gw:GameWeek) (p:Prediction) = { EditPredictionsViewModelRow.home=f.home; away=f.away; fxId=(getFxId f.id)|>str; kickoff=f.kickoff; gameWeekNumber=(getGameWeekNo gw.number); predictionId=(getPrId p.id); score=(toScoreViewModel p.score) }
        
    let season() = buildSeason currentSeason
    let gameWeeks() = season().gameWeeks |> List.sortBy(fun gw -> gw.number)
    let getNewGameWeekNo() = getNewGameWeekNo() |> GwNo

    let getOpenFixturesForPlayer (playerId:string) =
        let plId = PlId (sToGuid playerId)
        let players = getPlayers()
        let rows = season().gameWeeks
                   |> List.map(fun gw -> gw, getOpenFixturesWithNoPredictionForPlayer [gw] players plId)
                   |> List.map(fun (gw, fds) -> fds |> List.map(fun fd -> toFixtureViewModel fd gw))
                   |> List.collect(fun fvm -> fvm)
                   |> List.sortBy(fun fvm -> fvm.kickoff)
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
        
    let getPastGameWeeksWithWinner() =
        let players = getPlayers()
        let rows = (getPastGameWeeksWithWinner (gameWeeks()) players)
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

    let getGameWeeks() = readGameWeeks() |> List.sortBy(fun gw -> gw.number)
    let leagueTableRowToViewModel (pos, pl, cs, co, pts) = { LeagueTableRowViewModel.position=pos; player=getPlayerViewModel pl; correctScores=cs; correctOutcomes=co; points=pts }

    let getLeagueTableView() =
        let players = getPlayers()
        let gws = gameWeeks()
        let fixtures = getFixturesForGameWeeks gws
        let rows = (getLeagueTable players fixtures) |> List.map(leagueTableRowToViewModel)
        { LeagueTableViewModel.rows=rows }

    let getGameWeekPointsView gwno =
        let players = getPlayers()
        let gws = gameWeeks() |> List.filter(fun gw -> gw.number = gwno)
        let fixtures = getFixturesForGameWeeks gws
        let rows = (getLeagueTable players fixtures) |> List.map(leagueTableRowToViewModel)
        { GameWeekPointsViewModel.gameWeekNo = (getGameWeekNo gwno); rows=rows }
        
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
        let x = getPlayerPointsForGameWeeks players player gameWeeks
        let rows = x |> List.map(getPlayerGameWeeksViewModelRow)
        { PlayerGameWeeksViewModel.player=(getPlayerViewModel player); rows=rows }

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

    let trySavePredictionPostModel (ppm:PredictionPostModel) (playerId:string) =
        let plId = PlId (sToGuid playerId)
        let fxId = FxId (sToGuid ppm.fixtureId)
        let player = getPlayers() |> List.find(fun p -> p.id = plId)
        let gws = gameWeeks()
        let createScore() = tryToCreateScoreFromSbm ppm.score.home ppm.score.away
        let createPrediction score = createPrediction player score
        let makeSureFixtureExists p =
            match (tryFindFixture gws fxId) with
            | Some f -> Success (f, p)
            | None -> Failure "fixture does not exist"
        let trySavePrediction (f, (p:Prediction)) =
            let fd = fixtureToFixtureData f
            let addPredictionWithReturn() = savePrediction { SavePredictionCommand.id=p.id; fixtureId=fd.id; playerId=player.id; score=p.score }; ()
            tryToWithReturn addPredictionWithReturn
        // todo: make sure player exists
        () |> (createScore
               >> bind (switch createPrediction)
               >> bind makeSureFixtureExists
               >> bind tryAddPredictionToFixture
               >> bind trySavePrediction)
    
    let rec tryCreateFixturesFromPostModels (viewModels:FixturePostModel list) fixtures =
        match viewModels with
        | h::t -> let fixtureData = tryToCreateFixtureDataFromSbm h.home h.away h.kickOff
                  match fixtureData with
                  | Success fd -> tryCreateFixturesFromPostModels t (fd::fixtures)
                  | Failure msg -> Failure msg
        | [] -> Success fixtures

    let trySaveGameWeekPostModel (gwpm:GameWeekPostModel) =
        let gwno = getNewGameWeekNo()
        let snid = season().id
        let createFixtures viewModels = tryCreateFixturesFromPostModels viewModels []
        let createGameWeek fixtures = { SaveGameWeekCommand.id=newGwId(); seasonId=snid; number=gwno; fixtures=fixtures; description="" }
        gwpm.fixtures |> (createFixtures
                      >> bind (switch createGameWeek)
                      >> bind (switch saveGameWeek))

    let tryEditPrediction (eppm:EditPredictionPostModel) (playerId:string) =
        let players = getPlayers()
        let gws = gameWeeks()
        let plid = playerId |> sToGuid |> PlId
        let prid = PrId eppm.predictionId
        let player = players |> List.tryFind(fun p -> p.id = plid)
        let findPlayer p = match p with | Some p -> Success p | None -> Failure "could not find player" 
        let fdp = tryFindPredictionWithFixture gws prid
        let findPrediction plr = match fdp with | Some (fd, p) -> Success (plr, fd, p) | None -> Failure "could not find prediction" 
        let makeSurePredictionBelongsToPlayer (plr:Player, f, pr:Prediction) = if plr = pr.player then Success (plr, f, pr) else Failure "prediction is not player's to edit"
        let makeSureFixtureIsOpen (plr, f:FixtureData, pr) = match fixtureDataToFixture f None with | OpenFixture f -> Success pr | ClosedFixture f -> Failure "fixture is closed"
        let createScore pr = match tryToCreateScoreFromSbm eppm.score.home eppm.score.away with | Success s -> Success(pr, s) | Failure msg -> Failure msg
        let updatePrediction ((pr:Prediction), s) = updatePrediction { UpdatePredictionCommand.id=pr.id; score=s }
        player |> (findPlayer
               >> bind findPrediction
               >> bind makeSurePredictionBelongsToPlayer
               >> bind makeSureFixtureIsOpen
               >> bind createScore
               >> bind (switch updatePrediction))