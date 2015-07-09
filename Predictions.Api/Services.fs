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
open Predictions.Api.FormGuide
open Predictions.Api.Common
open Predictions.Api.Data
open Predictions.Api.LeagueTableCalculation

[<AutoOpen>]
module Services =
    
    let getPlayerViewModel (p:Player) = { PlayerViewModel.id=getPlayerId p.id|>str; name=p.name|>getPlayerName; isAdmin=true } 
    let toScoreViewModel (s:Score) = { ScoreViewModel.home=(fst s); away=(snd s) }
    let noScoreViewModel = { ScoreViewModel.home=0; away=0 }
    let toFixtureViewModel (fd:FixtureData) (gw:GameWeek) =
        let f = fixtureDataToFixture fd None
        let createVm isOpen = { FixtureViewModel.home=fd.home; away=fd.away; fxId=(getFxId fd.id)|>str; kickoff=fd.kickoff; gameWeekNumber=(getGameWeekNo gw.number); isOpen=isOpen }
        f |> isFixtureOpen |> createVm
    
    let season() = buildSeason currentSeason
    let gameWeeks() = season().gameWeeks |> List.sortBy(fun gw -> gw.number)
    let gameWeeksWithClosedFixtures() = gameWeeks() |> List.filter(fun gw -> getFixturesForGameWeeks [gw] |> List.choose(onlyClosedFixtures) |> Seq.isEmpty = false)
    let gameWeeksWithResults() = gameWeeks() |> getGameWeeksWithAnyClosedFixturesWithResults

    let formGuideOutcomeToString = function | Win -> "w" | Draw -> "d" | Lose -> "l"

    let toOpenFixtureViewModelRow (gw:GameWeek, fd:FixtureData, pr:Prediction option) gameWeeks =
        let predictionOptionToScoreViewModel (pr:Prediction option) =
            match pr with
            | Some p -> toScoreViewModel p.score
            | None -> noScoreViewModel
        let getFormGuide team =
            (getTeamFormGuideOutcome gameWeeks team)
            |> List.map(formGuideOutcomeToString)
        {
            OpenFixturesViewModelRow.fixture=(toFixtureViewModel fd gw)
            scoreSubmitted=pr.IsSome
            newScore=None
            existingScore=pr|>predictionOptionToScoreViewModel
            homeFormGuide=getFormGuide fd.home
            awayFormGuide=getFormGuide fd.away
        }

    let getOpenFixturesForPlayer (player:Player) =
        let gws = gameWeeks()
        let rows = gws
                   |> List.map(fun gw -> gw, getOpenFixturesAndPredictionForPlayer [gw] player)
                   |> List.map(fun (gw, fdps) -> fdps |> List.map(fun (fd, p) -> toOpenFixtureViewModelRow (gw,fd,p) gws))
                   |> List.collect(fun ofvmr -> ofvmr)
                   |> List.sortBy(fun ofvmr -> ofvmr.fixture.kickoff)
        { OpenFixturesViewModel.rows=rows }

    let getPlayerPointsForFixture (player:Player) (fxid:FxId) =
        let gws = gameWeeks()
        let getResult (gw, (fd:FixtureData), (r:Result option)) =
            let getPredictionScoreViewModel (prediction:Prediction option) =
                match prediction with
                | Some p -> toScoreViewModel p.score
                | None -> noScoreViewModel
            let getResultScoreViewModel (result:Result option) =
                match result with
                | Some rslt -> toScoreViewModel rslt.score
                | None -> noScoreViewModel
            let prediction = tryFindPlayerPredictionForFixture player fd
            let row = toOpenFixtureViewModelRow (gw, fd, prediction) gws 
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

    let leagueTableTupleToRowViewModel (pos, pl, cs, co, pts) =
        let ltr = {
            LeagueTableRow.diffPosition = 0
            position = pos
            player = pl
            correctScores = cs
            correctOutcomes = co
            points = pts }
        leagueTableRowToViewModel ltr

    let getLeaguePositionForPlayer players player =
        let fixtures = gameWeeksWithResults() |> getFixturesForGameWeeks
        getLeaguePositionForFixturesForPlayer fixtures players player

    let leagueIdToString leagueId = leagueId |> getLgId |> str

    let getMircoLeagueViewModel (league:League) =
        { MicroLeagueViewModel.id=league.id|>leagueIdToString; name=league.name|>getLeagueName }

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
            |> Seq.map getLeagueUnsafe
            |> Seq.map(fun l -> leagueToRowViewModel l player)
            |> Seq.toList
        { LeaguesViewModel.rows=rows }

    let getGameWeeksPointsForPlayerIdAndLeagueId (playerId, leagueId) =
        let getResult (player:Player, league:League) =
            let gameWeeks = gameWeeks()
            let buildRow (gw:GameWeek) pos cs co pts = {
                    PlayerGameWeeksViewModelRow.gameWeekNo=(getGameWeekNo gw.number);
                    hasResults=(doesGameWeekHaveAnyResults gw); firstKo=(getFirstKoForGw gw);
                    position=pos; correctScores=cs; correctOutcomes=co; points=pts }
            let getPlayerGameWeeksViewModelRow ((gw:GameWeek), r) =
                match r with
                | Some (pos, _, cs, co, pts) -> buildRow gw pos cs co pts
                | None -> buildRow gw 0 0 0 0
            let rows = (getPlayerPointsForGameWeeks league.players player gameWeeks) |> List.map(getPlayerGameWeeksViewModelRow)
            let fixtures = gameWeeks |> getFixturesForGameWeeks
            let pos = getLeaguePositionForFixturesForPlayer fixtures league.players player
            { PlayerGameWeeksViewModel.player=(getPlayerViewModel player); position=pos; rows=rows; league=league|>getMircoLeagueViewModel }
        let getLeagueAndCarryPlayer player =
            LgId leagueId |> (getLeague >> bind (switch (fun lge -> (player, lge))))
        PlId playerId |> (getPlayer
                      >> bind getLeagueAndCarryPlayer
                      >> bind (switch getResult))

    let getPlayerGameWeekByPlayerIdAndGameWeekNo gameWeekNo playerId =
        let getResult player =
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
        PlId playerId |> (getPlayer >> bind (switch getResult))

    let getLeaguePositionGraphDataForPlayerIdAndLeagueId (playerId, leagueId) =
        let getResult (league:League) =
            let plid = PlId playerId
            let gws = gameWeeksWithResults()
            let fixtures = gws |> compoundList |> List.map(getFixturesForGameWeeks)
            let labels = gws |> List.map(fun gw -> "GW#" + (gw.number|>getGameWeekNo|>str))
            let data = league.players
                       |> List.filter(fun p -> p.id = plid)
                       |> List.map(fun plr -> (plr, fixtures |> List.map(fun fs -> getLeaguePositionForFixturesForPlayer fs league.players plr)))
                       |> List.map(fun (_, posList) -> posList)
            { LeaguePositionGraphData.data=data; labels=labels }
        LgId leagueId |> (getLeague >> bind (switch getResult))

    let getPlayerProfileView playerId =
        let getResult player =
            let gws = gameWeeks()
            let buildRow ((gw:GameWeek), cs, co, pts) = {
                    PlayerProfileViewModelRow.gameWeekNo=(getGameWeekNo gw.number);
                    hasResults=(doesGameWeekHaveAnyResults gw); firstKo=(getFirstKoForGw gw);
                    correctScores=cs; correctOutcomes=co; points=pts }
            let rows = (getPlayerProfilePointsForGameWeeks player gws) |> List.map(buildRow)
            let totalPoints = rows |> List.sumBy(fun r -> r.points)
            { PlayerProfileViewModel.player=player|>getPlayerViewModel; totalPoints=totalPoints; rows=rows }
        PlId playerId |> (getPlayer >> bind (switch getResult))

    let getPlayerProfileGraph playerId =
        let getResult player =
            let gws = gameWeeks()
            let data = gws |> List.map(fun gw -> getGameWeekDetailsForPlayer player gw |> List.sumBy(fun (_, _, _, pts) -> pts))
            let labels = gws |> List.map(fun gw -> "GW#" + (gw.number|>getGameWeekNo|>str))
            { PlayerProfileGraphData.data=[data]; labels=labels }
        PlId playerId |> (getPlayer >> bind (switch getResult))

    let makeSureFixtureExists gws fxid =
        let fixture = tryFindFixture gws fxid
        NotFound "fixture does not exist" |> optionToResult fixture

    let getFixturePredictionGraphData fxid =
        let gws = gameWeeks()
        fxid |> ((makeSureFixtureExists gws)
             >> bind (switch fixtureToFixtureData)
             >> bind (switch (fun fd -> fd, (GetOutcomeCounts (getAllPredictionsForFixture fd.id) (0, 0, 0))))
             >> bind (switch (fun (fd, (hw, d, aw)) -> { FixturePredictionGraphData.data=[hw; d; aw]; labels=[fd.home; "Draw"; fd.away] })))
             
    let abrvFixtureViewModel (vm:FixtureViewModel) =
        { vm with home = vm.home.Substring(0,3); away = vm.away.Substring(0, 3) }

    let getFixturePreviousMeetingsView fxid =
        let gws = gameWeeks()
        let getResult (fixture:Fixture) =
            let fd = fixtureToFixtureData fixture
            let rows = getFixturePreviousMeetingsData fd.home fd.away
                        |> List.map(fun (kickoff, homeTeamName, awayTeamName, homeTeamScore, awayTeamScore) ->
                            { FixturePreviousMeetingsQueryResultViewModelRow.kickoff=kickoff; homeTeamName=homeTeamName; awayTeamName=awayTeamName; homeTeamScore=homeTeamScore; awayTeamScore=awayTeamScore } )
                        |> List.sortBy(fun r -> r.kickoff)
                        |> List.rev
            let thisFixtureRows = rows |> List.filter(fun r -> r.homeTeamName=fd.home)
            let reverseFixtureRows = rows |> List.filter(fun r -> r.homeTeamName=fd.away)
            { FixturePreviousMeetingsQueryResultViewModel.rows=rows; thisFixtureRows=thisFixtureRows; reverseFixtureRows=reverseFixtureRows }
        fxid |> ((makeSureFixtureExists gws)
             >> bind (switch getResult))
    
    let getFixtureFormGuideView fxid =
        let gws = gameWeeks()
        let getResult (fixture:Fixture) =
            let fd = fixtureToFixtureData fixture
            let homeFormGuide = getTeamFormGuide gws fd.home
            let awayFormGuide = getTeamFormGuide gws fd.away
            let zipped = List.zip homeFormGuide awayFormGuide
            let rows =
                zipped
                |> List.map(fun (h, a) -> {
                                            homeFixture=toFixtureViewModel h.fd h.gameWeek |> abrvFixtureViewModel
                                            awayFixture=toFixtureViewModel a.fd a.gameWeek |> abrvFixtureViewModel
                                            homeResult=toScoreViewModel h.result.score
                                            awayResult=toScoreViewModel a.result.score
                                            homeOutcome=formGuideOutcomeToString h.outcome
                                            awayOutcome=formGuideOutcomeToString a.outcome})
            { FixtureFormGuideViewModel.rows=rows }
        fxid |> ((makeSureFixtureExists gws)
             >> bind (switch getResult))
    

    let getGameWeeksWithClosedFixtures() =
        let rows = gameWeeks()
                   |> getGameWeeksWithClosedFixtures
                   |> List.map(fun gw -> { GameWeeksWithClosedFixturesRowViewModel.gwno=gw.number|>getGameWeekNo; closedFixtureCount=[gw]|>getClosedFixturesForGameWeeks|>List.length })
        { GameWeeksWithClosedFixturesViewModel.rows=rows }

    let resultToScoreViewModel (result:Result option) =
        match result with
        | Some r -> toScoreViewModel r.score
        | None -> noScoreViewModel

    let getClosedFixturesForGameWeek gwno =
        let rows = gameWeeks()
                   |> List.filter(fun gw -> gw.number = gwno)
                   |> List.map(fun gw -> gw, getClosedFixturesForGameWeeks [gw])
                   |> List.map(fun (gw, fdr) -> fdr |> List.map(fun (fd, r) -> { OpenFixturesViewModelRow.fixture=(toFixtureViewModel fd gw); newScore=None; scoreSubmitted=r.IsSome; existingScore=r|>resultToScoreViewModel; homeFormGuide=[]; awayFormGuide=[] }))
                   |> List.collect(fun o -> o)
        { ClosedFixturesForGameWeekViewModel.gameWeekNo=gwno|>getGameWeekNo; rows=rows }

    let getOpenFixturesWithNoPredictionsForPlayerCount player =
        getOpenFixturesWithNoPredictionForPlayer (gameWeeks()) player
        |> List.length

    let getInPlay() =
        let rows = gameWeeksWithClosedFixtures()
                   |> getFixturesInPlay
                   |> List.map(fun (gw, fs) -> fs |> List.map(fun f -> toFixtureViewModel (f|>fixtureToFixtureData) gw))
                   |> List.collect(fun fvm -> fvm)
                   |> List.sortBy(fun fvm -> fvm.kickoff)
                   |> List.map(fun fvm -> { InPlayRowViewModel.fixture=fvm })
        { InPlayViewModel.rows = rows }

    let getlatestGameWeekNo gws = gws |> List.maxBy(fun gw -> gw.number|>getGameWeekNo) |> (fun gw -> gw.number|>getGameWeekNo)

    let leagueToViewModel (league:League) = 
        let gws = gameWeeksWithResults()
        let leagueTable = getLeagueTableRows league gws
        let rows = leagueTable |> List.map(leagueTableRowToViewModel)
        let latestGameWeekNo = gws |> getlatestGameWeekNo
        { LeagueViewModel.id=league.id|>leagueIdToString; name=league.name|>getLeagueName; rows=rows; latestGameWeekNo=latestGameWeekNo }

    let getLeagueView leagueId =
        LgId leagueId |> (getLeague >> bind (switch leagueToViewModel))
        
    let getLeagueInviteView host leagueId =
        let buildModel (league:League) =
            let link = league.id |> getShareableLeagueId |> sprintf "%s/#/joinleague/%s" host
            { LeagueInviteViewModel.id=league.id|>leagueIdToString; name=getLeagueName league.name; inviteLink=link }
        LgId leagueId |> (getLeague >> bind (switch buildModel))

    let getLeagueJoinView shareableLeagueId =
        shareableLeagueId |> (getLeagueByShareableId >> bind (switch leagueToViewModel))

    let joinLeague (player:Player) leagueId =
        let lgid = leagueId|>LgId
        let checkLeagueNotFull (league:League) =
            if league.players.Length < maxPlayersPerLeague then league |> Success
            else Invalid "League has reached maximum number of players" |> Failure
        let joinLge (league:League) =
            let playerAlreadyInLeague = league.players |> List.exists(fun p -> p = player)
            if playerAlreadyInLeague then () else joinLeagueInDb { JoinLeagueCommand.leagueId=lgid; playerId=player.id }
        let returnLeague() = getLeagueView leagueId
        lgid |> (getLeague
                 >> bind (checkLeagueNotFull)
                 >> bind (switch joinLge)
                 >> bind returnLeague)
    
    let leaveLeague (player:Player) leagueId =
        let lgid = leagueId|>LgId
        let leaveLge league = leaveLeagueInDb { LeaveLeagueCommand.leagueId=lgid; playerId=player.id }
        lgid |> (getLeague >> bind (switch leaveLge))

    let getPastMonthsWithWinnerView leagueId =
        let getHistoryByMonthViewModel (league:League) =
            let rows = (getPastMonthsWithWinner (gameWeeksWithResults()) league.players)
                       |> List.map(fun (m, plr, pts) -> { HistoryByMonthRowViewModel.month=m; winner=(getPlayerViewModel plr); points=pts })
            { HistoryByMonthViewModel.rows=rows; league=league|>getMircoLeagueViewModel }
        LgId leagueId |> (getLeague >> bind (switch getHistoryByMonthViewModel))

    let getMonthPointsView month leagueId =
        let getHistoryByMonthWithMonthViewModel league =
            let gws = month |> getGameWeeksForMonth (gameWeeksWithResults())
            let fixtures = gws |> getFixturesForGameWeeks
            let rows = (getLeagueTable league.players fixtures) |> List.map(leagueTableTupleToRowViewModel)
            { HistoryByMonthWithMonthViewModel.month=month; rows=rows; gameweeks=gws|>List.map(fun gw -> gw.number|>getGameWeekNo); league=league|>getMircoLeagueViewModel }
        LgId leagueId |> (getLeague >> bind (switch getHistoryByMonthWithMonthViewModel))
    
    let getPastGameWeeksWithWinnerView leagueId =
        let getPastGameWeeksViewModel (league:League) =
            let rows = (getPastGameWeeksWithWinner (gameWeeksWithResults()) league.players)
                       |> List.map(fun (gw, plr, pts) -> { PastGameWeekRowViewModel.gameWeekNo=(getGameWeekNo gw.number); winner=(getPlayerViewModel plr); points=pts; hasResult=true})
            { PastGameWeeksViewModel.rows=rows; league=league|>getMircoLeagueViewModel }
        LgId leagueId |> (getLeague >> bind (switch getPastGameWeeksViewModel))

    let getGameWeekPointsView gwno leagueId =
        let getGameWeekPointsViewModel (league:League) =
            let fixtures = gameWeeks() |> List.filter(fun gw -> gw.number = gwno) |> getFixturesForGameWeeks
            let month = fixtures |> List.map(fixtureToFixtureData) |> List.minBy(fun fd -> fd.kickoff) |> fun fd -> fd.kickoff.ToString(monthFormat)
            let rows = (getLeagueTable league.players fixtures) |> List.map(leagueTableTupleToRowViewModel)
            { GameWeekPointsViewModel.gameWeekNo=(getGameWeekNo gwno); rows=rows; month=month; league=league|>getMircoLeagueViewModel }
        LgId leagueId |> (getLeague >> bind (switch getGameWeekPointsViewModel))

    let getGameWeekMatrix gwno leagueId =
        let getGameWeek gwno =
            match gameWeeks() |> List.tryFind(fun gw -> gw.number = gwno) with
            | Some gw -> Success gw
            | None -> NotFound "game week not found" |> Failure

        let getLeagueAndCarryGameWeek gw =
            LgId leagueId |> (getLeague >> bind (switch (fun lge -> (gw, lge))))

        let getGameWeekMatrixViewModel (gameWeek:GameWeek, league:League) =
            let fixtures = getFixturesForGameWeeks [gameWeek]
            let fixtureDataWithResults = fixtures |> List.map(fixtureToFixtureDataWithResult)
            let columns =
                let toColumn (fd, result:Result option) =
                     let vm = toFixtureViewModel fd gameWeek |> abrvFixtureViewModel
                     match result with
                     | Some r -> { GameWeekMatrixFixtureColumnViewModel.fixture=vm; isSubmitted=true; score=r.score|>toScoreViewModel }
                     | None -> { GameWeekMatrixFixtureColumnViewModel.fixture=vm; isSubmitted=false; score=noScoreViewModel }
                fixtures
                |> List.map fixtureToFixtureDataWithResult
                |> List.sortBy(fun (fd, _) -> fd.kickoff)
                |> List.map toColumn
            let playerToMatrixRow (player:Player) =
                let getPredictionViewModel ((fd:FixtureData), r) =
                    let prd = player.predictions |> List.tryFind(fun p -> p.fixtureId = fd.id)
                    let bracketClass =
                        match getBracketForPredictionComparedToResult prd r with
                        | CorrectScore -> "cs" | CorrectOutcome -> "co" | Incorrect -> ""
                    match fixtureDataToFixture fd r with
                    | OpenFixture fd -> { GameWeekMatrixPlayerRowPredictionViewModel.isFixtureOpen=true; isSubmitted=false; score=noScoreViewModel; bracketClass=bracketClass }
                     | ClosedFixture (fd, r) ->
                        match prd with
                        | Some p -> { GameWeekMatrixPlayerRowPredictionViewModel.isFixtureOpen=false; isSubmitted=true; score=(p.score|>toScoreViewModel); bracketClass=bracketClass }
                        | None -> { GameWeekMatrixPlayerRowPredictionViewModel.isFixtureOpen=false; isSubmitted=false; score=noScoreViewModel; bracketClass=bracketClass }
                let predictions = fixtureDataWithResults |> List.map(getPredictionViewModel)
                let (_, _, _, points) = getPlayerBracketProfile fixtures player
                { GameWeekMatrixPlayerRowViewModel.player=player|>getPlayerViewModel; predictions=predictions; points=points }
            let rows = league.players |> List.map(playerToMatrixRow) |> List.sortBy(fun row -> -row.points)
            { GameWeekMatrixViewModel.gameWeekNo=(getGameWeekNo gwno); rows=rows; columns=columns; league=league|>getMircoLeagueViewModel }
        gwno |> (getGameWeek
             >> bind getLeagueAndCarryGameWeek
             >> bind (switch getGameWeekMatrixViewModel))

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

    let rec tryCreateSaveFixtureCommandsFromPostModels (viewModels:FixturePostModel list) cmds =
        match viewModels with
        | h::t -> let result = tryToCreateKoFromSbm h.home h.away h.kickOff
                  match result with
                  | Success ko ->
                      let saveFixtureCommand = { SaveFixtureCommand.id=newFxId(); home=h.home; away=h.away; kickoff=ko }
                      tryCreateSaveFixtureCommandsFromPostModels t (saveFixtureCommand::cmds)
                  | Failure msg -> Failure msg
        | [] -> Success cmds

    let trySaveGameWeekPostModel (gwpm:GameWeekPostModel) =
        let createFixtures viewModels = tryCreateSaveFixtureCommandsFromPostModels viewModels []
        let createGameWeek fixtures = { SaveGameWeekCommand.id=newGwId(); seasonId=season().id; saveFixtureCommands=fixtures; description="" }
        gwpm.fixtures |> (createFixtures
                      >> bind (switch createGameWeek)
                      >> bind (switch saveGameWeek))

    let trySavePrediction (ppm:PredictionPostModel) (player:Player) =
        let gws = gameWeeks()
        let fxId = FxId (sToGuid ppm.fixtureId)
        let makeSureFixtureExists fxid = match (tryFindFixture gws fxid) with | Some f -> Success f | None -> NotFound "fixture does not exist" |> Failure
        let makeSureFixtureIsOpen (f:Fixture) = match f with | OpenFixture fd -> Success fd | ClosedFixture f -> Invalid "fixture is closed" |> Failure
        let createScore fd = match tryToCreateScoreFromSbm ppm.score.home ppm.score.away with | Success s -> Success(fd, s) | Failure msg -> Failure msg
        let createPrediction (fd:FixtureData, s) = savePrediction { SavePredictionCommand.id=newPrId(); fixtureId=fd.id; playerId=player.id; score=s }
        fxId |> (makeSureFixtureExists
               >> bind makeSureFixtureIsOpen
               >> bind createScore
               >> bind (switch createPrediction))

    let trySaveLeague (player:Player) (createLeague:CreateLeaguePostModel) =
        let name = makeLeagueName createLeague.name
        let lgid = newLgId()
        saveLeagueInDb { SaveLeagueCommand.id=lgid; name=name }
        joinLeagueInDb { JoinLeagueCommand.leagueId=lgid; playerId=player.id }
        getLeagueView (getLgId lgid)

    let registerPlayerWithUserInfo externalId provider userName =
        match getPlayerByExternalLogin (externalId,provider) with
        | Success player ->
            updateUserNameInDb { UpdateUserNameCommand.playerId=player.id; playerName=userName }
            player
        | _ -> 
            let player = { Player.id=newPlId(); name=userName; predictions=[] }
            registerPlayerInDb { RegisterPlayerCommand.player=player; explid=externalId; exProvider=provider; }
            player

    let getLoggedInPlayer plId = 
        match getPlayer plId with
        | Success p -> p
        | _ -> failwith "no player found"


    // global league functions

    let getGlobalTableRows gws =
        let allPlayers = getAllPlayers()
        let globalLeague = { League.id=newLgId(); name=""|>LeagueName; players=allPlayers }
        getLeagueTableRows globalLeague gws
        
    let getleaguePositionforplayer player =
        let gws = gameWeeksWithResults()
        let globalTableRows = getGlobalTableRows gws
        let pos = globalTableRows |> Seq.find(fun r -> r.player = player) |> (fun r -> r.position)
        let total = globalTableRows.Length
        let page = (pos/globalLeaguePageSize)
        { LeaguePositionViewModelRow.leaguePosition=pos; totalPlayerCount=total; playerLeaguePage=page }

    let getGlobalLeagueTablePage player page =
        let gws = gameWeeksWithResults()
        let globalTableRows = getGlobalTableRows gws
        let amountToTake =
            let totalPages = (globalTableRows.Length/globalLeaguePageSize)
            if totalPages = page then
                let totalPossibleRows = ((totalPages + 1) * globalLeaguePageSize)
                (globalLeaguePageSize - (totalPossibleRows - globalTableRows.Length))
            else globalLeaguePageSize
        let rows =
            globalTableRows
            |> Seq.skip(page*globalLeaguePageSize)
            |> Seq.take(amountToTake)
            |> Seq.map(leagueTableRowToViewModel)
            |> Seq.toList
        let latestGameWeekNo = gws |> getlatestGameWeekNo
        { LeagueViewModel.id=""; name=""; rows=rows; latestGameWeekNo=latestGameWeekNo }
    
    let getGlobalLastGameWeekAndWinner() = 
        let gws = gameWeeksWithResults()
        let allPlayers = getAllPlayers()
        getPastGameWeeksWithWinner gws allPlayers
        |> List.map(fun (gw, plr, pts) -> { PastGameWeekRowViewModel.gameWeekNo=(getGameWeekNo gw.number); winner=(getPlayerViewModel plr); points=pts; hasResult=true})
        |> List.maxBy(fun r -> r.gameWeekNo)
