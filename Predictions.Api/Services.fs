namespace Predictions.Api

open System
open Predictions.Api.Domain
open Predictions.Api.FormGuide
open Predictions.Api.Common
open Predictions.Api.Data
open Predictions.Api.LeagueTableCalculation
open Predictions.Api.TeamNames

[<AutoOpen>]
module Services =

    let getPlayerViewModel (p:Player) = { PlayerViewModel.id=getPlayerId p.id|>str; name=p.name|>getPlayerName; isAdmin=p.isAdmin }
    let noPlayerViewModel() = { PlayerViewModel.id=""; name=""; isAdmin=false }
    let toScoreViewModel (s:Score) = { ScoreViewModel.home=(fst s); away=(snd s) }
    let noScoreViewModel = { ScoreViewModel.home=0; away=0 }
    let getTeamViewModel teamName = { Team.full=teamName; abrv=teamName|>getAbrvTeamName }

    let toFixtureViewModel (fd:FixtureData) (gw:GameWeek) =
        let f = fixtureDataToFixture fd None
        let createVm isOpen = { FixtureViewModel.home=fd.home|>getTeamViewModel; away=fd.away|>getTeamViewModel; fxId=(getFxId fd.id)|>str; kickoff=fd.kickoff; gameWeekNumber=(getGameWeekNo gw.number); isOpen=isOpen }
        f |> isFixtureOpen |> createVm

    let season() = buildSeason currentSeason
    let gameWeeks() = season().gameWeeks |> Array.sortBy(fun gw -> gw.number)
    let gameWeeksWithClosedFixtures() = gameWeeks() |> Array.filter(fun gw -> not(getFixturesForGameWeeks [|gw|] |> Array.choose(onlyClosedFixtures) |> Seq.isEmpty))
    let gameWeeksWithResults() = gameWeeks() |> getGameWeeksWithAnyClosedFixturesWithResults

    let formGuideOutcomeToString = function | Win -> "w" | Draw -> "d" | Lose -> "l"

    let getIsDoubleDown (pr:Prediction option) =
        match pr with | Some p -> p.modifier = DoubleDown | None -> false

    let toOpenFixtureViewModelRow (isDoubleDownAvailable, gw:GameWeek, fd:FixtureData, pr:Prediction option) gameWeeks =
        let predictionOptionToScoreViewModel (pr:Prediction option) =
            match pr with | Some p -> toScoreViewModel p.score | None -> noScoreViewModel
        let getPredictionId (pr:Prediction option) =
            match pr with | Some p -> p.id |> getPrId |> str | None -> ""
        let getFormGuide team =
            (getTeamFormGuideOutcome gameWeeks team) |> Array.map(formGuideOutcomeToString)
        //let allFixturesAreOpenInGw = gw.fixtures |> Array.forall isFixtureOpen
        { OpenFixturesViewModelRow.fixture=(toFixtureViewModel fd gw)
          scoreSubmitted=pr.IsSome
          newScore=None
          existingScore=pr|>predictionOptionToScoreViewModel
          homeFormGuide=getFormGuide fd.home
          awayFormGuide=getFormGuide fd.away
          isDoubleDown=pr|>getIsDoubleDown
          predictionId=pr|>getPredictionId
          isDoubleDownAvailable=isDoubleDownAvailable }

    let getOpenFixturesForPlayer (player:Player) =
        let gws = gameWeeks()
        let rows =
            gws
            |> Array.map(fun gw -> gw, getOpenFixturesAndPredictionForPlayer [|gw|] player
                        >> fun (gw, fdps) ->
                            let ddAvailability = isDoubleDownAvailableForPlayerInGameWeek gw player
                            fdps |> Array.map(fun (fd, p) -> toOpenFixtureViewModelRow (ddAvailability, gw,fd,p) gws))
            |> Array.collect(id)
            |> Array.sortBy(fun ofvmr -> ofvmr.fixture.kickoff)
        { OpenFixturesViewModel.rows=rows }

    let getPlayerPointsForFixture (player:Player) (fxid:FxId) =
        let gws = gameWeeks()
        let getResult (gw, (fd:FixtureData), (r:Result option)) =
//            let getPredictionScoreViewModel (prediction:Prediction option) =
//                match prediction with
//                | Some p -> toScoreViewModel p.score
//                | None -> noScoreViewModel
            let getResultScoreViewModel (result:Result option) =
                match result with
                | Some rslt -> toScoreViewModel rslt.score
                | None -> noScoreViewModel
            let prediction = tryFindPlayerPredictionForFixture player fd
            let ddAvailability = isDoubleDownAvailableForPlayerInGameWeek gw player
            let row = toOpenFixtureViewModelRow (ddAvailability, gw, fd, prediction) gws
            { FixturePointsViewModel.fixture=(toFixtureViewModel fd gw); resultSubmitted=r.IsSome; result=r|>getResultScoreViewModel; openFixturesViewModelRow=row }
        let findFixtureAndGameWeek() =
            match tryFindFixtureWithGameWeek gws fxid with
            | Some f -> Success f
            | None -> NotFound "could not find fixture" |> Failure
        let viewFixture (gw, f, _) =
            let (fd, r) = fixtureToFixtureDataWithResult f
            (gw, fd, r)
        () |> (findFixtureAndGameWeek
           >> bind (switch viewFixture)
           >> bind (switch getResult))

    let leagueTableRowToViewModel (ltr:LeagueTableRow) = {
        LeagueTableRowViewModel.diffPos=ltr.diffPosition
        position=ltr.position
        player=getPlayerViewModel ltr.player
        correctScores=ltr.correctScores
        correctOutcomes=ltr.correctOutcomes
        points=ltr.points }

    let leagueIdToString leagueId = leagueId |> getLgId |> str

    let getMicroLeagueViewModel (league:League) =
        { MicroLeagueViewModel.id=league.id|>leagueIdToString; name=league.name|>getLeagueName }

    let getGlobalLeagueMircoViewModel _ =
        { MicroLeagueViewModel.id=globalLeagueId; name="Global League" }

    let getLeaguesView player =
        let gws = gameWeeksWithResults()
        let leagueToRowViewModel (league:League) (player:Player) =
            let buildRow position diffPos =
                { LeaguesRowViewModel.id=str (getLgId league.id); name=(getLeagueName league.name); position=position; diffPos=diffPos }
            let result = getLeagueTableRows league gws |> Seq.tryFind(fun row -> row.player.id = player.id)
            match result with
            | Some row -> buildRow row.position row.diffPosition
            | None -> buildRow 1 0
        let rows =
            getLeagueIdsThatPlayerIsIn player
            |> Seq.map(getLeagueUnsafe >> (fun l -> leagueToRowViewModel l player))
            |> Seq.toArray
        { LeaguesViewModel.rows=rows }

    let getBracketClass prd r =
        match getBracketForPredictionComparedToResult prd r with
        | CorrectScore _ -> "correct-score" | CorrectOutcome _ -> "correct-outcome" | Incorrect -> ""

    let getPlayerGameWeekByPlayerIdAndGameWeekNo gameWeekNo revealPlayerScoresEvenIfFixtureIsOpen playerId =
        let getResult player =
            let gw = gameWeeks() |> Array.find(fun gw -> (getGameWeekNo gw.number) = gameWeekNo)
            let rowToViewModel (fd, (r:Result option), (p:Prediction option), pts) =
                let fixture = fixtureDataToFixture fd r
                let getVmPred (pred:Prediction option) =
                    match pred with
                    | Some p -> if revealPlayerScoresEvenIfFixtureIsOpen then p.score|>toScoreViewModel else
                                match fixture with
                                | OpenFixture _ -> noScoreViewModel
                                | ClosedFixture _ -> p.score|>toScoreViewModel
                    | None -> noScoreViewModel
                let getVmResult (result:Result option) =
                    match result with
                    | Some r -> { ScoreViewModel.home=fst r.score;away=snd r.score }
                    | None -> noScoreViewModel
                let getDoubleDown (pred:Prediction option) =
                    if revealPlayerScoresEvenIfFixtureIsOpen then pred|>getIsDoubleDown else
                    match fixture with
                    | OpenFixture _ -> false
                    | ClosedFixture _ -> pred|>getIsDoubleDown
                let bracketClass = getBracketClass p r
                { GameWeekDetailsRowViewModel.fixture=(toFixtureViewModel fd gw); predictionSubmitted=p.IsSome; prediction=getVmPred p; resultSubmitted=r.IsSome; result=getVmResult r; points=pts; isDoubleDown=p|>getDoubleDown; bracketClass=bracketClass }
            let rows = (getGameWeekDetailsForPlayer player gw) |> Array.map(rowToViewModel) |> Array.sortBy(fun g -> g.fixture.kickoff)
            { GameWeekDetailsViewModel.gameWeekNo=gameWeekNo; player=(getPlayerViewModel player); totalPoints=rows|>Array.sumBy(fun r -> r.points); rows=rows }
        PlId playerId |> (getPlayer >> bind (switch getResult))

    let getPlayerProfileView playerId =
        let getResult player =
            let gws = gameWeeks()
            let buildRow ((gw:GameWeek), cs, co, pts) = {
                    PlayerProfileViewModelRow.gameWeekNo=(getGameWeekNo gw.number);
                    hasResults=(doesGameWeekHaveAnyResults gw); firstKo=(getFirstKoForGw gw);
                    correctScores=cs; correctOutcomes=co; points=pts }
            let rows = (getPlayerProfilePointsForGameWeeks player gws) |> Array.map(buildRow)
            let totalPoints = rows |> Array.sumBy(fun r -> r.points)
            { PlayerProfileViewModel.player=player|>getPlayerViewModel; totalPoints=totalPoints; rows=rows }
        PlId playerId |> (getPlayer >> bind (switch getResult))

    let getPlayerProfileGraph playerId =
        let getResult player =
            let gws = gameWeeks()
            let data = gws |> Array.map(getGameWeekDetailsForPlayer player >> Array.sumBy(fun (_, _, _, pts) -> pts))
            let labels = gws |> Array.map(fun gw -> "GW#" + (gw.number|>getGameWeekNo|>str))
            { PlayerProfileGraphData.data=[|data|]; labels=labels }
        PlId playerId |> (getPlayer >> bind (switch getResult))

    let makeSureFixtureExists gws fxid =
        let fixture = tryFindFixture gws fxid
        NotFound "fixture does not exist" |> optionToResult fixture

    let getFixturePredictionGraphData fxid =
        let gws = gameWeeks()
        fxid |> ((makeSureFixtureExists gws)
             >> bind (switch fixtureToFixtureData)
             >> bind (switch (fun fd -> fd, (GetOutcomeCounts (getAllPredictionsForFixture fd.id |> Array.toList) (0, 0, 0))))
             >> bind (switch (fun (fd, (hw, d, aw)) -> { FixturePredictionGraphData.data=[hw; d; aw] |> List.toArray; labels=[fd.home; "Draw"; fd.away] |> List.toArray })))

    let getFixtureDoubleDowns fxid =
        let gws = gameWeeks()
        let getAllPredictions (fd:FixtureData) = getAllPredictionsForFixture fd.id
        let getDoubleDownPercentage (prds:Prediction array) =
            let ddPrds = prds |> Array.filter(fun p -> p.modifier = DoubleDown)
            let totalPrds = Convert.ToDecimal(prds.Length)
            let totalDdPrds = Convert.ToDecimal(ddPrds.Length)
            let pc = if totalPrds = 0m then 0m else (totalDdPrds / totalPrds) * (100m)
            Math.Ceiling(pc)
        fxid |> ((makeSureFixtureExists gws)
             >> bind (switch fixtureToFixtureData)
             >> bind (switch (getAllPredictions))
             >> bind (switch (getDoubleDownPercentage)))

    let getFixturePreviousMeetingsView fxid =
        let gws = gameWeeks()
        let getResult (fixture:Fixture) =
            let fd = fixtureToFixtureData fixture
            let rows = getFixturePreviousMeetingsData fd.home fd.away
                        |> Array.map(fun (kickoff, homeTeamName, awayTeamName, homeTeamScore, awayTeamScore) ->
                            { FixturePreviousMeetingsQueryResultViewModelRow.kickoff=kickoff; home=homeTeamName|>getTeamViewModel; away=awayTeamName|>getTeamViewModel; homeTeamScore=homeTeamScore; awayTeamScore=awayTeamScore } )
                        |> Array.sortBy(fun r -> r.kickoff)
                        |> Array.rev
            let thisFixtureRows = rows |> Array.filter(fun r -> r.home.full=fd.home)
            let reverseFixtureRows = rows |> Array.filter(fun r -> r.home.full=fd.away)
            { FixturePreviousMeetingsQueryResultViewModel.allRows=rows; thisFixtureRows=thisFixtureRows; reverseFixtureRows=reverseFixtureRows }
        fxid |> ((makeSureFixtureExists gws)
             >> bind (switch getResult))

    let getFixtureFormGuideView fxid =
        let gws = gameWeeks()
        let getResult (fixture:Fixture) =
            let fd = fixtureToFixtureData fixture
            let homeFormGuide = getTeamFormGuide gws fd.home |> Array.rev
            let awayFormGuide = getTeamFormGuide gws fd.away |> Array.rev
            let toRow (fg:FormGuideResultContainer) = {
                FixtureFormGuideViewModelRow.fixture=toFixtureViewModel fg.fd fg.gameWeek
                result=toScoreViewModel fg.result.score
                outcome=formGuideOutcomeToString fg.outcome }
            { FixtureFormGuideViewModel.homeRows=homeFormGuide|>Array.map toRow; awayRows=awayFormGuide|>Array.map toRow }
        fxid |> ((makeSureFixtureExists gws)
             >> bind (switch getResult))

    let getNeighbours (col:_[]) getId findItem =
        let i = col |> Seq.findIndex(findItem)
        let iToVal index = col.[index] |> getId
        let max = col.Length - 1
        let defaultModel = { NeighboursViewModel.prev=""; hasPrev=false; next=""; hasNext=false; }
        match i with
        | 0 when i = max -> defaultModel
        | 0 -> { defaultModel with hasNext=true; next=i+1|>iToVal; }
        | _ when i = max -> { defaultModel with hasPrev=true; prev=i-1|>iToVal; }
        | _ -> { NeighboursViewModel.hasPrev=true; prev=i-1|>iToVal; hasNext=true; next=i+1|>iToVal; }

    let getFixtureNeighbours fxid =
        let gws = gameWeeks()
        let fds = getFixtureDatasForGameWeeks gws
        let getResult (fixtureData:FixtureData) =
            let findItem = (fun (fd:FixtureData) -> fd.id = fixtureData.id)
            let getId = (fun (fd:FixtureData) -> fd.id |> getFxId |> str)
            getNeighbours fds getId findItem
        fxid |> ((makeSureFixtureExists gws)
            >> bind (switch fixtureToFixtureData)
            >> bind (switch getResult))

    let getGameWeekNeighbours gwno =
        let gws = gameWeeks()
        let getResult gwNo =
            let findItem = (fun (gw:GameWeek) -> gw.number = gwNo)
            let getId = (fun (gw:GameWeek) -> gw.number |> getGameWeekNo |> str)
            getNeighbours gws getId findItem
        GwNo gwno |> getResult

    let getMonthNeighbours month =
        let months =
            gameWeeksWithResults()
            |> Array.map(getMonthForGameWeek)
            |> Seq.distinct |> Seq.toArray
        let getResult m =
            let findItem = (fun mth -> m = mth)
            let getId = (fun mth -> mth |> str)
            getNeighbours months getId findItem
        month |> getResult

    let getGameWeeksWithClosedFixtures() =
        let rows = gameWeeks()
                   |> getGameWeeksWithClosedFixtures
                   |> Array.map(fun gw -> { GameWeeksWithClosedFixturesRowViewModel.gwno=gw.number|>getGameWeekNo; closedFixtureCount=[|gw|]|>getClosedFixturesForGameWeeks|>Array.length })
        { GameWeeksWithClosedFixturesViewModel.rows=rows }

    let resultToScoreViewModel (result:Result option) =
        match result with
        | Some r -> toScoreViewModel r.score
        | None -> noScoreViewModel

    let getClosedFixturesForGameWeek gwno =
        let rows = gameWeeks()
                   |> Array.filter(fun gw -> gw.number = gwno)
                   |> Array.map(fun gw -> gw, getClosedFixturesForGameWeeks [|gw|])
                   |> Array.map(fun (gw, fdr) -> fdr |> Array.map(fun (fd, r) ->
                      { OpenFixturesViewModelRow.fixture=(toFixtureViewModel fd gw)
                        newScore=None
                        scoreSubmitted=r.IsSome
                        existingScore=r|>resultToScoreViewModel
                        homeFormGuide=Array.empty
                        awayFormGuide=Array.empty
                        isDoubleDown=false
                        predictionId=""
                        isDoubleDownAvailable=false }))
                   |> Array.collect(id)
                   |> Array.sortBy(fun r -> r.fixture.kickoff)
        { ClosedFixturesForGameWeekViewModel.gameWeekNo=gwno|>getGameWeekNo; rows=rows }

    let getOpenFixturesWithNoPredictionsForPlayerCount player =
        getOpenFixturesWithNoPredictionForPlayer (gameWeeks()) player
        |> Array.length

    let getlatestGameWeekNo gws =
        if gws |> Array.isEmpty then 0
        else gws |> Array.maxBy(fun gw -> gw.number|>getGameWeekNo) |> (fun gw -> gw.number|>getGameWeekNo)

    let getInPlay playerId =
        let latestGwNo = gameWeeks() |> getlatestGameWeekNo
        playerId |> getPlayerGameWeekByPlayerIdAndGameWeekNo latestGwNo true

    open TeamLeagueTable

    let getPredictedLeagueTable player =
        let gws = gameWeeks()
        let teamRowToViewModel (tr:TeamRow) =
            { PredictedLeagueTableRowViewModel.pos=tr.pos; team=tr.team|>getTeamViewModel; played=tr.played; won=tr.won; drawn=tr.drawn; lost=tr.lost; gf=tr.gf; ga=tr.ga; gd=tr.gd; points=tr.points}
        { PredictedLeagueTableViewModel.rows = getTeamLeagueTableForPlayerPredictions player gws |> Seq.map(teamRowToViewModel) |> Array.ofSeq }

    // persistence

    let trySaveResultPostModel (rpm:ResultPostModel) =
        let gws = gameWeeks()
        let fxid = FxId (sToGuid rpm.fixtureId)
        let createScore() = tryToCreateScoreFromSbm rpm.score.home rpm.score.away
        let createResult score = { Result.score=score }
        let makeSureFixtureExists r =
            match (tryFindFixture gws fxid) with
            | Some _ -> Success (fxid,r)
            | None -> invalid "fixture does not exist"
        let saveResult (fxid,(r:Result)) = saveResult { SaveResultCommand.fixtureId=fxid; score=r.score }
        () |> (createScore
           >> bind (switch createResult)
           >> bind makeSureFixtureExists
           >> bind (switch saveResult))

    let trySavePrediction (ppm:PredictionPostModel) (player:Player) =
        let gws = gameWeeks()
        let fxId = FxId (sToGuid ppm.fixtureId)
        let makeSureFixtureExists fxid = match (tryFindFixture gws fxid) with | Some f -> Success f | None -> NotFound "fixture does not exist" |> Failure
        let makeSureFixtureIsOpen (f:Fixture) = match f with | OpenFixture fd -> Success fd | ClosedFixture f -> Invalid "fixture is closed" |> Failure
        let createScore fd = match tryToCreateScoreFromSbm ppm.score.home ppm.score.away with | Success s -> Success(fd, s) | Failure msg -> Failure msg
        let createPrediction (fd:FixtureData, s) =
            let prid = newPrId()
            savePrediction { SavePredictionCommand.id=prid; fixtureId=fd.id; playerId=player.id; score=s }
            { SubmittedPredictionResultModel.predictionId=prid|>getPrId }
        fxId |> (makeSureFixtureExists
               >> bind makeSureFixtureIsOpen
               >> bind createScore
               >> bind (switch createPrediction))

    let registerPlayerWithUserInfo externalId provider userName email =
        //match getPlayerByExternalLogin (externalId, provider) with
        //| Success player ->
        //    updateUserNameInDb { UpdateUserNameCommand.playerId=player.id; playerName=userName }
        //    player
        //| _ ->
        let newPlayer = { Player.id=newPlId(); name=userName; predictions=Array.empty; isAdmin=false }
        registerPlayerInDb { RegisterPlayerCommand.player=newPlayer; explid=externalId; exProvider=provider; email=email }
        findPlayerByExternalId externalId provider

    let doubleDown predictionId (player:Player) =
        let prId = PrId predictionId
        let gws = gameWeeks()
        let getGwForPrd (prediction:Prediction) =
            let fxs = gws |> getFixtureDatasForGameWeeks
            let fd = fxs |> Array.find(fun fd -> fd.id = prediction.fixtureId)
            gws |> Array.find(fun gw -> gw.id = fd.gwId)
        let makeSurePredictionBelongsToPlayer prid =
            let prediction = player.predictions |> Array.tryFind(fun p -> p.id = prid)
            Forbidden "Prediction does not exist!" |> optionToResult prediction
        let makeSureFixtureIsOpen (prediction:Prediction) =
            let fxs = gws |> getFixtureDatasForGameWeeks
            let fd = fxs |> Array.find(fun fd -> fd.id = prediction.fixtureId)
            let fx = fixtureDataToFixture fd None
            if isFixtureOpen fx then Success prediction else Forbidden "Fixture is closed" |> Failure
        let makeSureAnyExistingDoubleDownFixtureIsOpen prediction =
            let gw = getGwForPrd prediction
            match isDoubleDownAvailableForPlayerInGameWeek gw player with
            | true -> Success gw
            | false -> Forbidden "Double Down already used for this Gameweek" |> Failure
        //let makeSureGameWeekHasNotKickedOff (prediction:Prediction) =
        //    let fxs = gws |> getFixtureDatasForGameWeeks
        //    let fd = fxs |> Array.find(fun fd -> fd.id = prediction.fixtureId)
        //    let gw = gws |> Array.find(fun gw -> gw.id = fd.gwId)
        //    let allFixturesAreOpenInGw = gw.fixtures |> Array.forall isFixtureOpen
        //    if allFixturesAreOpenInGw then Success gw
        //    else gw.number |> getGameWeekNo |> sprintf "Too late! GW%i has already kicked off" |> Forbidden |> Failure
        let saveDoubleDown (gw:GameWeek) = { SaveDoubleDownCommand.playerId=player.id; gameWeekId=gw.id; predictionId=prId } |> saveDoubleDownInDb
        prId |> (makeSurePredictionBelongsToPlayer
             >> bind makeSureFixtureIsOpen
             >> bind makeSureAnyExistingDoubleDownFixtureIsOpen
            // >> bind makeSureGameWeekHasNotKickedOff
             >> bind (switch saveDoubleDown))

    let getLoggedInPlayer plId =
        match getPlayer plId with
        | Success p -> p
        | _ -> failwith "no player found"

    // global league functions

    let getGlobalLeague() =
        let allPlayers = getAllPlayers()
        { League.id=newLgId(); name="Global League"|>LeagueName; players=allPlayers; adminId=newPlId() }

    let getGlobalTableRows gws =
        let globalLeague = getGlobalLeague()
        gws |> getLeagueTableRows globalLeague

    let getGlobalLeaguePositionforplayer (player:Player) =
        let gws = gameWeeksWithResults()
        let globalTableRows = getGlobalTableRows gws
        let index = globalTableRows |> Seq.findIndex(fun r -> r.player.id = player.id)
        let pos = globalTableRows.[index] |> (fun r -> r.position)
        let total = globalTableRows.Length
        let page = (index/globalLeaguePageSize)
        { LeaguePositionViewModelRow.leaguePosition=pos; totalPlayerCount=total; playerLeaguePage=page }

        //leagueTableRowToViewModel //LeagueTableRow
    let getPagedList page f (allRows:_[]) =
        let totalPages = (allRows.Length/globalLeaguePageSize)
        let amountToTake =
            if totalPages = page then
                let totalPossibleRows = ((totalPages + 1) * globalLeaguePageSize)
                (globalLeaguePageSize - (totalPossibleRows - allRows.Length))
            else globalLeaguePageSize
        let rows =
            allRows
            |> Seq.skip(page*globalLeaguePageSize)
            |> Seq.take amountToTake
            |> Seq.map f
            |> Seq.toArray
        let neighbours =
            let allPages = [|0..totalPages|]
            let findItem = (fun i -> i = page)
            let getId = (fun i -> i |> str)
            getNeighbours allPages getId findItem
//        { PagedLeagueViewModel.rows=rows; neighbours=neighbours }
        rows, neighbours

    let getGlobalLastGameWeekAndWinner() =
        let gws = gameWeeksWithResults()
        let allPlayers = getAllPlayers()
        let allWinners =
            getPastGameWeeksWithWinner gws allPlayers
            |> Array.map(fun (gw, plrs, pts) -> { PastGameWeekRowViewModel.gameWeekNo=(getGameWeekNo gw.number); winners=plrs|>Array.map getPlayerViewModel; points=pts; hasResult=true; isGameWeekComplete=gw|>getIsGameWeekComplete})
        if allWinners |> Array.isEmpty then { PastGameWeekRowViewModel.gameWeekNo=0; winners=[||]; points=0; hasResult=false; isGameWeekComplete=false }
        else allWinners |> Array.maxBy(fun r -> r.gameWeekNo)

    let getHistoryByMonthViewModel toMicroLeague (league:League) =
        let rows = (getPastMonthsWithWinner (gameWeeksWithResults()) league.players)
                    |> Array.map(fun (m, plrs, pts) -> { HistoryByMonthRowViewModel.month=m; winners=plrs|>Array.map getPlayerViewModel; points=pts })
        { HistoryByMonthViewModel.rows=rows; league=league|>toMicroLeague }
    let getPastMonthsWithWinnerView leagueId =
        LgId leagueId |> (getLeague >> bind (getHistoryByMonthViewModel getMicroLeagueViewModel |> switch))

    let getHistoryByMonthWithMonthViewModel month page toMicroLeague (league:League) =
        let gws = month |> getGameWeeksForMonth (gameWeeksWithResults())
        let rows, neighbours = getLeagueTableRows league gws |> getPagedList page leagueTableRowToViewModel
        { HistoryByMonthWithMonthViewModel.month=month; rows=rows; gameweeks=gws|>Array.map(fun gw -> gw.number|>getGameWeekNo); league=league|>toMicroLeague; neighbours=neighbours }
    let getMonthPointsView month page leagueId =
        LgId leagueId |> (getLeague >> bind (switch (getHistoryByMonthWithMonthViewModel month page getMicroLeagueViewModel)))

    let getHistoryByGameWeekViewModel toMicroLeague (league:League) =
        let rows = (getPastGameWeeksWithWinner (gameWeeksWithResults()) league.players)
                    |> Array.map(fun (gw, plrs, pts) -> { PastGameWeekRowViewModel.gameWeekNo=(getGameWeekNo gw.number); winners=plrs|>Array.map getPlayerViewModel; points=pts; hasResult=true; isGameWeekComplete=gw|>getIsGameWeekComplete})
        { PastGameWeeksViewModel.rows=rows; league=league|>toMicroLeague }
    let getPastGameWeeksWithWinnerView leagueId =
        LgId leagueId |> (getLeague >> bind (getHistoryByGameWeekViewModel getMicroLeagueViewModel |> switch))

    let getHistoryByGameWeekWithGameWeekViewModel gwno page toMicroLeague (league:League) =
        let gws = gameWeeks() |> Array.filter(fun gw -> gw.number = gwno)
        let fixtures = gws |> getFixturesForGameWeeks
        let month = fixtures |> Array.map fixtureToFixtureData |> Array.minBy(fun fd -> fd.kickoff) |> fun fd -> fd.kickoff.ToString(monthFormat)
        let rows, neighbours = getLeagueTableRows league gws |> getPagedList page leagueTableRowToViewModel
        { HistoryByGameWeekWithGameWeekViewModel.gameWeekNo=(getGameWeekNo gwno); rows=rows; month=month; league=league|>toMicroLeague; neighbours=neighbours }
    let getGameWeekPointsView gwno page leagueId =
        LgId leagueId |> (getLeague >> bind (switch (getHistoryByGameWeekWithGameWeekViewModel gwno page getMicroLeagueViewModel)))

    let getGlobalLeagueTablePage page =
        let gws = gameWeeks()
        let latestGameWeekNo = gws |> getlatestGameWeekNo
        let rows, neighbours = gws |> getGlobalTableRows |> getPagedList page leagueTableRowToViewModel
        { LeagueViewModel.id=globalLeagueId; name="Global League"; rows=rows; latestGameWeekNo=latestGameWeekNo; adminId=""; neighbours=neighbours }

    let leagueToViewModel page (league:League) =
        let gws = gameWeeks()
        let latestGameWeekNo = gws |> getlatestGameWeekNo
        let rows, neighbours = getLeagueTableRows league gws |> getPagedList page leagueTableRowToViewModel
        { LeagueViewModel.id=league.id|>leagueIdToString; name=league.name|>getLeagueName; rows=rows; latestGameWeekNo=latestGameWeekNo; adminId=league.adminId|>getPlayerId|>str; neighbours=neighbours }

    let getLeagueView page leagueId =
        LgId leagueId |> (getLeague >> bind (switch (leagueToViewModel page)))

    let getLeagueInviteView host leagueId =
        let buildModel (league:League) =
            let link = league.id |> getShareableLeagueId |> sprintf "%s/#/joinleague/%s" host
            { LeagueInviteViewModel.id=league.id|>leagueIdToString; name=getLeagueName league.name; inviteLink=link }
        LgId leagueId |> (getLeague >> bind (switch buildModel))

    let getLeagueJoinView shareableLeagueId =
        shareableLeagueId |> (getLeagueByShareableId >> bind (leagueToViewModel 0 |> switch))

    let joinLeague (player:Player) leagueId =
        let lgid = leagueId|>LgId
        let checkLeagueNotFull (league:League) =
            if league.players.Length < maxPlayersPerLeague then league |> Success
            else Invalid "League has reached maximum number of players" |> Failure
        let joinLge (league:League) =
            let playerAlreadyInLeague = league.players |> Array.exists(fun p -> p = player)
            if playerAlreadyInLeague then () else joinLeagueInDb { JoinLeagueCommand.leagueId=lgid; playerId=player.id }
        let returnLeague() = getLeagueView 0 leagueId
        lgid |> (getLeague
                 >> bind (checkLeagueNotFull)
                 >> bind (switch joinLge)
                 >> bind returnLeague)

    let leaveLeague (player:Player) leagueId =
        let lgid = leagueId|>LgId
        let makeSurePlayerIsNotLeagueAdmin (league:League) =
            if league.adminId <> player.id then league |> Success
            else Forbidden "League Admin cannot leave league " |> Failure
        let leaveLge league = leaveLeagueInDb { LeaveLeagueCommand.leagueId=lgid; playerId=player.id }
        lgid |> (getLeague
                >> bind (makeSurePlayerIsNotLeagueAdmin)
                >> bind (switch leaveLge))

    let deleteLeague (player:Player) leagueId =
        let lgid = leagueId|>LgId
        let makeSurePlayerIsLeagueAdmin (league:League) =
            if league.adminId = player.id then league |> Success
            else Forbidden "Current player is not league admin" |> Failure
        let deleteLge (league:League) = { DeleteLeagueCommand.leagueId = league.id } |> deleteLeagueInDb
        lgid |> (getLeague
                >> bind (makeSurePlayerIsLeagueAdmin)
                >> bind (switch deleteLge)
                >> bind (switch noPlayerViewModel))

    let trySaveLeague (player:Player) (createLeague:CreateLeaguePostModel) =
        let name = makeLeagueName createLeague.name
        let lgid = newLgId()
        saveLeagueInDb { SaveLeagueCommand.id=lgid; name=name; admin=player }
        joinLeagueInDb { JoinLeagueCommand.leagueId=lgid; playerId=player.id }
        getLeagueView 0 (getLgId lgid)

    let getGameWeekMatrixViewModel page toMicroLeague (gameWeek:GameWeek, league:League) =
        let fixtures = getFixturesForGameWeeks [|gameWeek|]
        let fixtureDataWithResults = fixtures |> Array.map(fixtureToFixtureDataWithResult)
        let columns =
            let toColumn (fd, result:Result option) =
                    let vm = toFixtureViewModel fd gameWeek
                    match result with
                    | Some r -> { GameWeekMatrixFixtureColumnViewModel.fixture=vm; isSubmitted=true; score=r.score|>toScoreViewModel }
                    | None -> { GameWeekMatrixFixtureColumnViewModel.fixture=vm; isSubmitted=false; score=noScoreViewModel }
            fixtures
            |> Array.map fixtureToFixtureDataWithResult
            |> Array.sortBy(fun (fd, _) -> fd.kickoff)
            |> Array.map toColumn
        let playerToMatrixRow (player:Player) =
            let getPredictionViewModel ((fd:FixtureData), r) =
                let prd = player.predictions |> Array.tryFind(fun p -> p.fixtureId = fd.id)
                let bracketClass = getBracketClass prd r
                let buildModel (isOpen, isSubmitted, score, bracketClass, isDoubleDown) =
                    { GameWeekMatrixPlayerRowPredictionViewModel.isFixtureOpen=isOpen; isSubmitted=isSubmitted; score=score; bracketClass=bracketClass; isDoubleDown=isDoubleDown }
                match fixtureDataToFixture fd r with
                | OpenFixture fd -> buildModel (true,false,noScoreViewModel,bracketClass,false)
                | ClosedFixture (fd, r) ->
                    match prd with
                    | Some p -> buildModel (false,true,(p.score|>toScoreViewModel),bracketClass,p.modifier=DoubleDown)
                    | None -> buildModel (false,false,noScoreViewModel,bracketClass,false)
            let predictions = fixtureDataWithResults |> Array.sortBy(fun (fd,_) -> fd.kickoff) |> Array.map(getPredictionViewModel)
            let (_, _, _, points) = getPlayerBracketProfile fixtures player
            { GameWeekMatrixPlayerRowViewModel.player=player|>getPlayerViewModel; predictions=predictions; points=points }
        let rows = league.players |> Array.map(playerToMatrixRow) |> Array.sortBy(fun row -> -row.points)
        let pagedrows, neighbours = rows |> getPagedList page (id)
        { GameWeekMatrixViewModel.gameWeekNo=(getGameWeekNo gameWeek.number); rows=pagedrows; columns=columns; league=league|>toMicroLeague; neighbours=neighbours }

    let getGameWeek gwno =
        match gameWeeks() |> Array.tryFind(fun gw -> gw.number = gwno) with
        | Some gw -> Success gw
        | None -> NotFound "game week not found" |> Failure

    let getGameWeekMatrix page gwno leagueId =
        let getLeagueAndCarryGameWeek gw =
            LgId leagueId |> (getLeague >> bind (switch (fun lge -> (gw, lge))))
        gwno |> (getGameWeek
             >> bind getLeagueAndCarryGameWeek
             >> bind (getGameWeekMatrixViewModel page getMicroLeagueViewModel |> switch))

    let getGlobalGameWeekMatrix page gwno =
        gwno |> (getGameWeek
             >> bind (switch (fun gw -> (gw, getGlobalLeague())))
             >> bind (getGameWeekMatrixViewModel page getGlobalLeagueMircoViewModel |> switch))


    let getGameWeeksPointsForPlayerInLeague toMicroLeague (player:Player, league:League) =
        let gameWeeks = gameWeeks()
        let buildRow (gw:GameWeek) pos cs co pts = {
                PlayerGameWeeksViewModelRow.gameWeekNo=(getGameWeekNo gw.number);
                hasResults=(doesGameWeekHaveAnyResults gw); firstKo=(getFirstKoForGw gw);
                position=pos; correctScores=cs; correctOutcomes=co; points=pts }
        let getPlayerGameWeeksViewModelRow ((gw:GameWeek), r) =
            match r with
            | Some (pos, _, cs, co, pts) -> buildRow gw pos cs co pts
            | None -> buildRow gw 0 0 0 0
        let rows = (getPlayerPointsForGameWeeks league.players player gameWeeks) |> Array.map(getPlayerGameWeeksViewModelRow)
        let fixtures = gameWeeks |> getFixturesForGameWeeks
        let pos = getLeaguePositionForFixturesForPlayer fixtures league.players player
        { PlayerGameWeeksViewModel.player=(getPlayerViewModel player); position=pos; rows=rows; league=league|>toMicroLeague }

    let getGameWeeksPointsForPlayerIdAndLeagueId (playerId, leagueId) =
        let getLeagueAndCarryPlayer player =
            LgId leagueId |> (getLeague >> bind (switch (fun lge -> (player, lge))))
        PlId playerId |> (getPlayer
                      >> bind getLeagueAndCarryPlayer
                      >> bind (getGameWeeksPointsForPlayerInLeague getMicroLeagueViewModel |> switch))

    let getGameWeekPointsForPlayerInGlobalLeague playerId =
        PlId playerId |> (getPlayer
                      >> bind ((fun plr -> (plr, getGlobalLeague()) ) |> switch)
                      >> bind (getGameWeeksPointsForPlayerInLeague getGlobalLeagueMircoViewModel |> switch))

    let getLeaguePositionGraphDataForPlayerInLeague playerId (league:League) =
        let plid = PlId playerId
        let gws = gameWeeksWithResults()
        let fixtures = gws |> Array.toList |> compoundList |> List.map(List.toArray >> getFixturesForGameWeeks) |> List.toArray
        let labels = gws |> Array.map(fun gw -> "GW#" + (gw.number|>getGameWeekNo|>str))
        let data = league.players
                    |> Array.filter(fun p -> p.id = plid)
                    |> Array.map(fun plr -> (plr, fixtures |> Array.map(fun fs -> getLeaguePositionForFixturesForPlayer fs league.players plr)))
                    |> Array.map(fun (_, posList) -> posList)
        { LeaguePositionGraphData.data=data; labels=labels; scaleSteps=league.players.Length }

    let getLeaguePositionGraphDataForPlayerIdAndLeagueId (playerId, leagueId) =
        LgId leagueId |> (getLeague >> bind (getLeaguePositionGraphDataForPlayerInLeague playerId |> switch))

    let getLeaguePositionGraphDataForPlayerInGlobalLeague playerId =
        getGlobalLeague() |> (getLeaguePositionGraphDataForPlayerInLeague playerId |> switch)

    open FixtureSourcing

    let getImportNextGameWeekView() =
        let gws = gameWeeks()
        let importGwNo = gws |> getlatestGameWeekNo |> (fun gwno -> gwno+1)
        getNewPremGwFixtures importGwNo
        |> List.map(fun (d, h, a) -> { ImportNewGameWeekViewModelRow.home=h; away=a; kickoff=d })
        |> fun rows -> { ImportNewGameWeekViewModel.rows=rows|>List.toArray; gwno=importGwNo }

    let submitImportNextGameWeek() =
        let gws = gameWeeks()
        let importGwNo = gws |> getlatestGameWeekNo |> (fun gwno -> gwno+1)
        let teams =
            gws
            |> getFixturesForGameWeeks
            |> Array.map(fixtureToFixtureData)
            |> Array.toList
            |> List.collect(fun fd -> [fd.home, fd.away])
            |> List.unzip
            |> fun (h,a) -> h@a

        let allKicksOffsAreInFuture fxs =
            match fxs |> List.forall(fun (d, _, _) -> d > GMTDateTime.Now()) with
            | true -> fxs |> Success
            | false -> Invalid "not all fixtures are in the future" |> Failure

        let allTeamsExist fxs =
            let rec teamExistsRec = function
            | h::t -> let exists = teams |> List.exists(fun t -> t = h)
                      if exists then teamExistsRec t
                      else sprintf "%s does not exist" h |> Invalid |> Failure
            | [] -> Success fxs
            fxs
            |> List.map(fun(_, h, a) -> [h;a])
            |> List.collect(id)
            |> teamExistsRec

        let saveGw fxs =
            let fxcmds = fxs |> List.map(fun (d,h,a) -> { SaveFixtureCommand.id=newFxId(); home=h; away=a; kickoff=d })
            { SaveGameWeekCommand.id=newGwId(); seasonYear=currentSeason; description=""; saveFixtureCommands=fxcmds|>List.toArray }
            |> saveGameWeek

        importGwNo |> (switch (getNewPremGwFixtures)
                   >> bind allKicksOffsAreInFuture
                //    >> bind allTeamsExist
                   >> bind (switch saveGw))
