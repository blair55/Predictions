namespace Predictions.Api

open System
open System.Collections.Generic
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
open Predictions.Api.LeagueTableCalculation

[<AutoOpen>]
module DummyData =

    type SaveLeagueCommand = { id:LgId; name:LeagueName }
    type JoinLeagueCommand = { leagueId:LgId; playerId:PlId }
    type RegisterPlayerCommand = { player:Player; explid:ExternalPlayerId; exProvider:ExternalLoginProvider }
    type UpdateUserNameCommand = { playerId:PlId; playerName:PlayerName }
     
    let buildSeason _ = { id=newSnId(); year=SnYr ""; gameWeeks=[] }
    
    let playersDictionary = new List<(PlId*ExternalPlayerId*ExternalLoginProvider*PlayerName)>()
    let leaguesDictionary = new Dictionary<LgId,LeagueName>()
    let leaguePlayersBridgeTable = new List<(LgId*PlId)>()

    let saveLeagueInDb (cmd:SaveLeagueCommand) = leaguesDictionary.Add(cmd.id, cmd.name)
    let joinLeagueInDb (cmd:JoinLeagueCommand) = leaguePlayersBridgeTable.Add(cmd.leagueId, cmd.playerId)
    let registerPlayerInDb (cmd:RegisterPlayerCommand) = playersDictionary.Add(cmd.player.id, cmd.explid, cmd.exProvider, cmd.player.name)
    let updateUserNameInDb (cmd:UpdateUserNameCommand) = ()

    let getLeagueIdsThatPlayerIsIn (player:Player) =
        leaguePlayersBridgeTable
        |> Seq.filter(fun (_,plid) -> plid = player.id)
        |> Seq.map(fun (lgid,_) -> lgid)

    let tryFindPlayerAndMap findFunc =
        playersDictionary
        |> Seq.tryFind(findFunc)
        |> function
        | Some (plid, _, _, name) -> Some { Player.id=plid; name=name }
        | None -> None

    let tryFindPlayerByPlayerId playerId =
        tryFindPlayerAndMap (fun (plid, _, _, _) -> plid = playerId)

    let tryFindPlayerByExternalId externalPlayerId =
        tryFindPlayerAndMap (fun (_, explid, _, _) -> explid = externalPlayerId)

    let getPlayerIdsForLeague leagueId =
        leaguePlayersBridgeTable
        |> Seq.filter(fun (lgid,_) -> lgid = leagueId)
        |> Seq.map(fun (_,plid) -> plid)

    let getPlayer (plId:PlId) =
        match tryFindPlayerByPlayerId (plId) with
        | Some p -> p |> Success
        | None -> NotFound "player not found" |> Failure

    let getLeague (lgId:LgId) =
        let found,name = leaguesDictionary.TryGetValue(lgId)
        match found with
        | false -> NotFound "League not found" |> Failure
        | true ->
            let players = getPlayerIdsForLeague lgId |> Seq.choose(tryFindPlayerByPlayerId) |> Seq.toList
            Success { League.id=lgId; name=name; players=players }

    let getLeagueUnsafe (lgId:LgId) =
        let name = leaguesDictionary.[lgId]
        let players = getPlayerIdsForLeague lgId |> Seq.choose(tryFindPlayerByPlayerId) |> Seq.toList
        { League.id=lgId; name=name; players=players }

    let getLeagueByShareableId shareableLeagueId =
        let result = leaguesDictionary.Keys |> Seq.tryFind(fun k -> (k|>getShareableLeagueId) = shareableLeagueId)
        match result with
        | None -> NotFound "League not found" |> Failure
        | Some lgid -> getLeague lgid

[<AutoOpen>]
module Services =
    
    let getPlayerViewModel (p:Player) = { PlayerViewModel.id=getPlayerId p.id|>str; name=p.name|>getPlayerName; isAdmin=false } 
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

//    let getImmediateSiblings collection item =
//        match collection |> List.tryFindIndex(fun c -> c = item) with
//        | None -> (None, None)
//        | Some i -> let p = i - 1
//                    let n = i + 1
//                    let prev = if (p >= 0) then collection.[p] else None
//                    let next = if (n <= collection.Length) then collection.[p] else None
//                    (prev, next)
        
    let predictionOptionToScoreViewModel (pr:Prediction option) =
        match pr with
        | Some p -> toScoreViewModel p.score
        | None -> noScoreViewModel

    let toOpenFixtureViewModelRow (gw:GameWeek, fd:FixtureData, pr:Prediction option) gameWeeks =
        let getFormGuide team =
            (getTeamFormGuide gameWeeks team)
            |> List.map(fun o -> match o with | Win -> "w" | Draw -> "d" | Lose -> "l" )
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

    let getFixtureViewDetails (gw, (fd:FixtureData), (r:Result option)) =
        let getPredictionScoreViewModel (prediction:Prediction option) =
            match prediction with
            | Some p -> toScoreViewModel p.score
            | None -> noScoreViewModel
        let getResultScoreViewModel (result:Result option) =
            match result with
            | Some rslt -> toScoreViewModel rslt.score
            | None -> noScoreViewModel
//        let rows = players
//                   |> List.map(fun plr -> plr, (fd.predictions |> List.tryFind(fun pr -> pr.player = plr)))
//                   |> List.map(fun (plr, pr) -> plr, pr, (getBracketForPredictionComparedToResult pr r) |> getPointsForBracket)
//                   |> List.map(fun (plr, pr, pts) -> { FixturePointsRowViewModel.player=(getPlayerViewModel plr); predictionSubmitted=pr.IsSome; prediction=(getPredictionScoreViewModel pr); points=pts })
        { FixturePointsViewModel.fixture=(toFixtureViewModel fd gw); resultSubmitted=r.IsSome; result=r|>getResultScoreViewModel; (*rows=rows*) }

    let getPlayerPointsForFixture (fxid:FxId) =
        let findFixtureAndGameWeek() =
            match tryFindFixtureWithGameWeek (gameWeeks()) fxid with
            | Some f -> Success f
            | None -> NotFound "could not find fixture" |> Failure
        let viewFixture (gw, f, _) =
            match tryViewFixture f with
            | Success (fd, r) -> Success(gw, fd, r)
            | Failure msg -> Failure msg
        () |> (findFixtureAndGameWeek
           >> bind viewFixture
           >> bind (switch getFixtureViewDetails))

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

    let leagueToRowViewModel (league:League) player =
        let leagueTableRowForPlayer = getLeagueTableRows league (gameWeeksWithResults()) |> Seq.find(fun row -> row.player = player)
        { LeaguesRowViewModel.id=str (getLgId league.id); name=(getLeagueName league.name); position=leagueTableRowForPlayer.position; diffPos=leagueTableRowForPlayer.diffPosition }

    let getLeaguesView player =
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

    let getFixturePredictionGraphData fxid =
        let gws = gameWeeks()
        let makeSureFixtureExists fxid =
            let fixture = tryFindFixture gws fxid
            NotFound "fixture does not exist" |> optionToResult fixture
        fxid |> (makeSureFixtureExists
             >> bind (switch fixtureToFixtureData)
             >> bind (switch (fun fd -> fd, (GetOutcomeCounts fd.predictions (0, 0, 0))))
             >> bind (switch (fun (fd, (hw, d, aw)) -> { FixturePredictionGraphData.data=[hw; d; aw]; labels=[fd.home; "Draw"; fd.away] })))

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
        let gws = gameWeeks()
        let rows = gws
                   |> List.filter(fun gw -> gw.number = gwno)
                   |> List.map(fun gw -> gw, getClosedFixturesForGameWeeks [gw])
                   |> List.map(fun (gw, fdr) -> fdr |> List.map(fun (fd, r) -> { OpenFixturesViewModelRow.fixture=(toFixtureViewModel fd gw); newScore=None; scoreSubmitted=r.IsSome; existingScore=r|>resultToScoreViewModel; homeFormGuide=[]; awayFormGuide=[] }))
                   |> List.collect(fun o -> o)
        { ClosedFixturesForGameWeekViewModel.gameWeekNo=gwno|>getGameWeekNo; rows=rows }

    let getLastGameWeekAndWinner() = 
//        getPastGameWeeksWithWinner().rows |> Seq.last
        let (p:PlayerViewModel) = {PlayerViewModel.name=""; id=""; isAdmin=false }
        { PastGameWeekRowViewModel.gameWeekNo=2; winner=p; points=2 }

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


    let leagueToViewModel (league:League) = 
        let leagueTable = getLeagueTableRows league (gameWeeksWithResults())
        let rows = leagueTable |> List.map(leagueTableRowToViewModel)
        { LeagueViewModel.id=league.id|>leagueIdToString; name=league.name|>getLeagueName; rows=rows }

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
        let lgid = LgId leagueId
        let joinLge league = joinLeagueInDb { JoinLeagueCommand.leagueId=lgid; playerId=player.id }
        let returnLeague() = getLeagueView leagueId
        lgid |> (getLeague
                 >> bind (switch joinLge)
                 >> bind (switch returnLeague))

    let getPastMonthsWithWinner leagueId =
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
    
    let getPastGameWeeksWithWinner leagueId =
        let getPastGameWeeksViewModel (league:League) =
            let rows = (getPastGameWeeksWithWinner (gameWeeksWithResults()) league.players)
                       |> List.map(fun (gw, plr, pts) -> { PastGameWeekRowViewModel.gameWeekNo=(getGameWeekNo gw.number); winner=(getPlayerViewModel plr); points=pts })
            { PastGameWeeksViewModel.rows=rows; league=league|>getMircoLeagueViewModel }
        LgId leagueId |> (getLeague >> bind (switch getPastGameWeeksViewModel))

    let getGameWeekPointsView gwno leagueId =
        let getGameWeekPointsViewModel (league:League) =
            let fixtures = gameWeeks() |> List.filter(fun gw -> gw.number = gwno) |> getFixturesForGameWeeks
            let month = fixtures |> List.map(fixtureToFixtureData) |> List.minBy(fun fd -> fd.kickoff) |> fun fd -> fd.kickoff.ToString(monthFormat)
            let rows = (getLeagueTable league.players fixtures) |> List.map(leagueTableTupleToRowViewModel)
            { GameWeekPointsViewModel.gameWeekNo=(getGameWeekNo gwno); rows=rows; month=month; league=league|>getMircoLeagueViewModel }
        LgId leagueId |> (getLeague >> bind (switch getGameWeekPointsViewModel))

    // persistence
    
    type SaveSeasonCommand = { id:SnId; year:SnYr; }
    type SaveGameWeekCommand = { id:GwId; seasonId:SnId; description:string; fixtures:FixtureData list }
    type SaveResultCommand = { id:RsId; fixtureId:FxId; score:Score }
    type SavePredictionCommand = { id:PrId; fixtureId:FxId; playerId:PlId; score:Score }
    type SavePlayerCommand = { id:PlId; name:string; role:Role; email:string; authToken:string }
    type SaveFixtureCommand = { id:FxId; gameWeekId:GwId; home:Team; away:Team; ko:KickOff }

    let savePlayer (cmd:SavePlayerCommand) = ()
    let saveSeason (cmd:SaveSeasonCommand) = ()
    let saveResult (cmd:SaveResultCommand) = ()
    let savePrediction (cmd:SavePredictionCommand) = ()
    let saveGameWeek (cmd:SaveGameWeekCommand) = ()

    let trySaveResultPostModel (rpm:ResultPostModel) =
        let gws = gameWeeks()
        let fxid = FxId (sToGuid rpm.fixtureId)
        let createScore() = tryToCreateScoreFromSbm rpm.score.home rpm.score.away
        let createResult score = { Result.id=newRsId(); score=score }
        let makeSureFixtureExists r =
            match (tryFindFixture gws fxid) with
            | Some _ -> Success (fxid,r)
            | None -> invalid "fixture does not exist"
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
        let snid = season().id
        let createFixtures viewModels = tryCreateFixturesFromPostModels viewModels []
        let createGameWeek fixtures = { SaveGameWeekCommand.id=newGwId(); seasonId=snid; fixtures=fixtures; description="" }
        gwpm.fixtures |> (createFixtures
                      >> bind (switch createGameWeek)
                      >> bind (switch saveGameWeek))

    let trySavePrediction (ppm:PredictionPostModel) (player:Player) =
        let gws = gameWeeks()
        let fxId = FxId (sToGuid ppm.fixtureId)
        let makeSureFixtureExists p = match (tryFindFixture gws fxId) with | Some f -> Success (p, f) | None -> Invalid "fixture does not exist" |> Failure
        let makeSureFixtureIsOpen (plr, f:Fixture) = match f with | OpenFixture fd -> Success (plr, fd) | ClosedFixture f -> Invalid "fixture is closed" |> Failure
        let createScore (plr, fd) = match tryToCreateScoreFromSbm ppm.score.home ppm.score.away with | Success s -> Success(plr, fd, s) | Failure msg -> Failure msg
        let createPrediction (plr, fd, s) = (plr, fd, (createPrediction plr s))
        let updatePrediction ((plr:Player), (fd:FixtureData), (pr:Prediction)) = savePrediction { SavePredictionCommand.id=pr.id; fixtureId=fd.id; playerId=plr.id; score=pr.score }
        player |> (makeSureFixtureExists
               >> bind makeSureFixtureIsOpen
               >> bind createScore
               >> bind (switch createPrediction)
               >> bind (switch updatePrediction))

    let trySaveLeague (player:Player) (createLeague:CreateLeaguePostModel) =
        let name = makeLeagueName createLeague.name
        let lgid = newLgId()
        saveLeagueInDb { SaveLeagueCommand.id=lgid; name=name }
        joinLeagueInDb { JoinLeagueCommand.leagueId=lgid; playerId=player.id }
        getLeagueView (getLgId lgid)

    let registerPlayerWithUserInfo externalId provider userName =
        let result = tryFindPlayerByExternalId externalId
        match result with
        | Some player ->
            updateUserNameInDb { UpdateUserNameCommand.playerId=player.id; playerName=userName }
            player
        | None -> 
            let player = { Player.id=newPlId(); name=userName }
            registerPlayerInDb { RegisterPlayerCommand.player=player; explid=externalId; exProvider=provider; }
            player

    let getLoggedInPlayer plId =
        let result = tryFindPlayerByPlayerId plId
        match result with
        | Some p -> p
        | None -> failwith "no player found"

