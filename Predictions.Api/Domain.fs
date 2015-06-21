﻿namespace Predictions.Api

open System

module Domain =

    type LgId = LgId of Guid
    type FxId = FxId of Guid
    type GwId = GwId of Guid
    type PlId = PlId of Guid
    type PrId = PrId of Guid
    type RsId = RsId of Guid
    type SnId = SnId of Guid
    type GwNo = GwNo of int
    type SnYr = SnYr of string
    type Team = string
    type KickOff = DateTime
    type Role = User | Admin
    type LeagueName = LeagueName of string
    type ShareableLeagueId = ShareableLeagueId of string
    type ExternalPlayerId = ExternalPlayerId of string
    type ExternalLoginProvider = ExternalLoginProvider of string
    type PlayerName = PlayerName of string

    let nguid() = Guid.NewGuid()
    let newLgId() = nguid()|>LgId
    let newFxId() = nguid()|>FxId
    let newPrId() = nguid()|>PrId
    let newGwId() = nguid()|>GwId
    let newRsId() = nguid()|>RsId
    let newSnId() = nguid()|>SnId
    let newPlId() = nguid()|>PlId
    let makeLeagueName (name:string) =
        (if name.Length > 50 then name.Substring(0, 50) else name) |> LeagueName
    
    let getPlayerId (PlId id) = id
    let getGameWeekNo (GwNo n) = n
    let getLgId (LgId id) = id
    let getFxId (FxId id) = id
    let getGwId (GwId id) = id
    let getPrId (PrId id) = id
    let getRsId (RsId id) = id
    let getSnId (SnId id) = id
    let getSnYr (SnYr year) = year
    let getPlayerName (PlayerName plrName) = plrName
    let getLeagueName (LeagueName lgeName) = lgeName

    let currentSeason = SnYr "2014/15"
    let monthFormat = "MMMM yyyy"

    type Score = int * int
    type Result = { id:RsId; score:Score }
    type FixtureData = { id:FxId; home:Team; away:Team; kickoff:KickOff }
    type Fixture =
         | OpenFixture of FixtureData
         | ClosedFixture of (FixtureData * Result option)
    type Prediction = { id:PrId; score:Score; fixtureId:FxId }
    type Player = { id:PlId; name:PlayerName; predictions:Prediction list }
    type GameWeek = { id:GwId; number:GwNo; description:string; fixtures:Fixture list }
    type Season = { id:SnId; year:SnYr; gameWeeks:GameWeek list }
    type Outcome = HomeWin | AwayWin | Draw
    type Bracket = CorrectScore | CorrectOutcome | Incorrect

    type League = { id:LgId; name:LeagueName; players:Player list}

    type AppError =
        | NotLoggedIn of string
        | Forbidden of string
        | Invalid of string
        | NotFound of string
        | InternalError of string

    let fixtureDataToFixture fd r =
        match fd.kickoff > DateTime.Now with
        | true -> OpenFixture fd
        | false -> ClosedFixture (fd, r)

    let fixtureToFixtureData f =
        match f with
        | OpenFixture fd -> fd
        | ClosedFixture (fd, _) -> fd

    let fixtureToFixtureDataWithResult f =
        match f with
        | OpenFixture fd -> fd, None 
        | ClosedFixture (fd, r) -> (fd, r)

    let isFixtureOpen f = match f with | OpenFixture _ -> true | ClosedFixture _ -> false
    let isFixtureClosedAndHaveResult f = match f with | OpenFixture _ -> false | ClosedFixture (_, r) -> r.IsSome
    let isFixtureClosedAndHaveNoResult f = match f with | OpenFixture _ -> false | ClosedFixture (_, r) -> r.IsNone

    let getFixturesForGameWeeks (gws:GameWeek list) =
        gws |> List.collect(fun gw -> gw.fixtures)

    let getGameWeeksWithAnyClosedFixturesWithResults (gws:GameWeek list) =
        gws |> List.filter(fun gw -> gw.fixtures |> List.exists(isFixtureClosedAndHaveResult))

    let tryFindFixture (gws:GameWeek list) fxid =
        gws
        |> List.collect(fun gw -> gw.fixtures)
        |> List.map(fun f -> f, fixtureToFixtureData f)
        |> List.tryFind(fun (f, fd) -> fd.id = fxid)
        |> fstOption

    let tryFindFixtureWithGameWeek (gws:GameWeek list) fxid =
        gws
        |> List.collect(fun gw -> gw.fixtures |> List.map(fun f -> gw, f))
        |> List.map(fun (gw, f) -> gw, f, fixtureToFixtureData f)
        |> List.tryFind(fun (gw, f, fd) -> fd.id = fxid)

//    let tryFindPredictionWithFixture (gws:GameWeek list) prid =
//        gws
//        |> List.collect(fun gw -> gw.fixtures)
//        |> List.map(fixtureToFixtureData)
//        |> List.map(fun fd -> fd, fd.predictions)
//        |> List.collect(fun (fd, predictions) -> predictions |> List.map(fun p -> fd, p))
//        |> List.tryFind(fun (_, p) -> p.id = prid)

    let getMonthForGameWeek (gw:GameWeek) =
        gw.fixtures
        |> List.map(fixtureToFixtureData)
        |> List.minBy(fun fd -> fd.kickoff)
        |> fun fd -> fd.kickoff.ToString(monthFormat)

    let getGameWeeksForMonth (gws:GameWeek list) (month) =
        gws
        |> List.map(fun gw -> gw, (gw.fixtures |> List.map(fixtureToFixtureData) |> List.minBy(fun fd -> fd.kickoff)))
        |> List.filter(fun (_, f) -> f.kickoff.ToString(monthFormat) = month)
        |> List.map(fun (gw, _) -> gw)

    let getFixturesInPlay gws =
        gws |> List.map(fun gw -> gw, ([gw] |> getFixturesForGameWeeks |> List.filter(isFixtureClosedAndHaveNoResult)))
        
    let getFirstKoForGw (gw:GameWeek) =
        gw.fixtures
        |> List.map(fixtureToFixtureData)
        |> List.minBy(fun f -> f.kickoff) |> fun f -> f.kickoff

    let doesGameWeekHaveAnyResults (gw:GameWeek) =
        getFixturesForGameWeeks [gw] |> List.exists(isFixtureClosedAndHaveResult)

    // base calculations

    let getPointsForBracket b =
        match b with
        | CorrectScore -> 3
        | CorrectOutcome -> 1
        | Incorrect -> 0

    let getResultOutcome score =
        if fst score > snd score then HomeWin
        else if fst score < snd score then AwayWin
        else Draw

    let getBracketForPredictionComparedToResult (prediction:Prediction option) (result:Result option) =
        if prediction.IsNone || result.IsNone then Incorrect
        else if prediction.Value.score = result.Value.score then CorrectScore
        else
            let predictionOutcome = getResultOutcome prediction.Value.score
            let resultOutcome = getResultOutcome result.Value.score
            if predictionOutcome = resultOutcome then CorrectOutcome
            else Incorrect

    let tryFindPlayerPredictionForFixture (player:Player) (fd:FixtureData) =
        player.predictions |> Seq.tryFind(fun pr -> pr.fixtureId = fd.id)
    
    let onlyClosedFixtures f =
        match f with
        | OpenFixture _ -> None
        | ClosedFixture fr -> fr|>ClosedFixture|>Some
        
    let onlyOpenFixtures f =
        match f with
        | OpenFixture fd -> fd|>OpenFixture|>Some
        | ClosedFixture _ -> None

    let getPlayerBracketProfile (fixtures:Fixture list) player =
        let brackets = fixtures
                       |> List.choose onlyClosedFixtures
                       |> List.map fixtureToFixtureDataWithResult
                       |> List.map(fun (fd, r) -> (tryFindPlayerPredictionForFixture player fd, r))
                       |> List.map(fun (p, r) -> getBracketForPredictionComparedToResult p r)
        let totalPoints = brackets |> List.sumBy(fun b -> getPointsForBracket b)
        let countBracket l bracket = l |> List.filter(fun b -> b = bracket) |> List.length
        let totalCorrectScores = CorrectScore |> (countBracket brackets)
        let totalCorrectOutcomes = CorrectOutcome |> (countBracket brackets)
        player, totalCorrectScores, totalCorrectOutcomes, totalPoints

    let rec bumprank sumdelta acc a =
        match a with
        | [] -> List.rev acc
        | h::t -> let (i, g) = h
                  let newi = i + sumdelta
                  let newsumdelta = sumdelta + ((Seq.length g)-1)
                  let newacc = (newi, g)::acc
                  bumprank newsumdelta newacc t

    let rank rows =
        rows
        |> Seq.groupBy(fun (_, x) -> x)
        |> Seq.mapi(fun i (_, g) -> i+1, g) |> Seq.toList
        |> bumprank 0 []
        |> Seq.collect(fun (i, g) -> g |> Seq.map(fun x -> i, x))
        
    let getLeagueTable players fixtures =
        players
        |> List.map(fun p -> getPlayerBracketProfile fixtures p)
        |> List.map(fun (p, cs, co, tp) -> (p, (cs, co, tp)))
        |> List.sortBy(fun (_, (cs, co, totalPoints)) -> -totalPoints, -cs, -co)
        |> rank
        |> Seq.map(fun (r, (p, (cs, co, tp))) -> (r, p, cs, co, tp))
        |> Seq.toList

    let getPlayerPointsForGameWeeks allPlayers player gameWeeks =
        gameWeeks
        |> List.map(fun gw -> gw, (getFixturesForGameWeeks [gw]))
        |> List.map(fun (gw, fixtures) -> gw, getLeagueTable allPlayers fixtures)
        |> List.map(fun (gwno, ltrList) -> gwno, ltrList |> List.tryFind(fun (_, p, _, _, _) -> p = player))

    let getGameWeekDetailsForPlayer player gameWeek =
        getFixturesForGameWeeks [gameWeek]
        |> List.map(fixtureToFixtureDataWithResult)
        |> List.map(fun (fd, r) -> (fd, r, tryFindPlayerPredictionForFixture player fd))
        |> List.map(fun (fd, r, p) -> (fd, r, p, (getBracketForPredictionComparedToResult p r |> getPointsForBracket)))
        
    let getOpenFixturesAndPredictionForPlayer (gws:GameWeek list) (player:Player) =
        gws
        |> getFixturesForGameWeeks
        |> List.choose(onlyOpenFixtures)
        |> List.map(fixtureToFixtureData)
//        |> List.map(fun fd -> fd, fd.predictions |> List.tryFind(fun p -> p.player = player))
        |> List.map(fun fd -> fd, player.predictions |> List.tryFind(fun p -> p.fixtureId = fd.id))
        |> List.sortBy(fun (fd, _) -> fd.kickoff)

    let getOpenFixturesWithNoPredictionForPlayer (gws:GameWeek list) (player:Player) =
        gws
        |> getFixturesForGameWeeks
        |> List.choose(onlyOpenFixtures)
        |> List.map(fixtureToFixtureData)
//        |> List.filter(fun fd -> fd.predictions |> List.exists(fun p -> p.player = player) = false)
        |> List.filter(fun fd -> player.predictions |> List.exists(fun p -> p.fixtureId = fd.id) = false)
        |> List.sortBy(fun fd -> fd.kickoff)

    let getPastGameWeeksWithWinner (gws:GameWeek list) players =
        gws
        |> List.map(fun gw -> gw, getFixturesForGameWeeks [gw])
        |> List.map(fun (gw, fixtures) -> gw, getLeagueTable players fixtures)
        |> List.map(fun (gw, lgtbl) -> gw, lgtbl.Head)
        |> List.map(fun (gw, (_, plr, _, _, pts)) -> gw, plr, pts)

    let getPastMonthsWithWinner (gws:GameWeek list) players =
        gws
        |> Seq.groupBy(getMonthForGameWeek)
        |> Seq.toList
        |> List.map(fun (m, gws) -> m, getFixturesForGameWeeks (gws|>Seq.toList))
        |> List.map(fun (m, fixtures) -> m, getLeagueTable players fixtures)
        |> List.map(fun (m, lgtbl) -> m, lgtbl.Head)
        |> List.map(fun (m, (_, plr, _, _, pts)) -> m, plr, pts)

    let getLeaguePositionForFixturesForPlayer (fixtures:Fixture list) players player =
        fixtures
        |> getLeagueTable players
        |> List.tryFind(fun (_, plr, _, _, _) -> plr = player)
        |> (function
            | Some (pos, _, _, _, _) -> pos
            | None -> -1)

    let rec GetOutcomeCounts (p:Prediction list) (hw, d, aw) =
        match p with
        | h::t -> match getResultOutcome h.score with
                  | HomeWin -> GetOutcomeCounts t (hw+1, d, aw)
                  | Draw -> GetOutcomeCounts t (hw, d+1, aw)
                  | AwayWin -> GetOutcomeCounts t (hw, d, aw+1)
        | [] -> (hw, d, aw)

    let getClosedFixturesForGameWeeks gws =
        gws
        |> getFixturesForGameWeeks
        |> List.choose(onlyClosedFixtures)
        |> List.map(fixtureToFixtureDataWithResult)

    let getGameWeeksWithClosedFixtures (gws:GameWeek list) =
        gws |> List.filter(fun gw -> [gw] |> getClosedFixturesForGameWeeks |> List.isEmpty = false)


    // Rules 
    
    // when adding gameweek:
        // cannot add fixture to gameweek with same home & away teams
        // cannot add fixture to gameweek with ko in past
    
    // when saving prediction/result:
        // cannot add score with negative scores
        // cannot add more than one prediction per user

    // cannot save result to fixture with ko in future
    // cannot save prediction to fixture with ko in past
    // cannot view fixture with ko in future

    let invalid msg = Invalid msg |> Failure
    
    let createPrediction fixtureId score = { Prediction.id=newPrId(); score=score; fixtureId=fixtureId }

    let checkFixtureIsOpen (f, p) =
        match f with
        | OpenFixture fd -> Success (fd, p)
        | ClosedFixture _ -> invalid "cannot add prediction to fixture with ko in past"

//    let checkPlayerHasNoSubmittedPredictionsForFixture ((fd:FixtureData), (pr:Prediction)) =
//        let hasAlreadySubmitted = fd.predictions |> List.exists(fun p -> p.player = pr.player)
//        if hasAlreadySubmitted then invalid "cannot submit more than one prediction for fixture" else Success (fd, pr)

    let tryViewFixture f =
        match f with
        | OpenFixture _ -> invalid "cannot view fixture with ko in future"
        | ClosedFixture(fd, r) -> Success(fd, r)

    let tryToCreateScoreFromSbm home away =
        if home >= 0 && away >= 0 then Success(home, away) else invalid "cannot submit negative score"

    let tryToCreateFixtureDataFromSbm home away (ko:string) =
        let (isKoValid, kickoff) = DateTime.TryParse(ko)
        if isKoValid=false then invalid "fixture kickoff time is invalid"
        else if kickoff < DateTime.Now then invalid "fixture kickoff cannot be in the past"
        else if home = away then invalid "fixture home and away team cannot be the same"
        else Success({ id=newFxId(); home=home; away=away; kickoff=kickoff })

    let getShareableLeagueId (leagueId:LgId) =
        (str <| getLgId leagueId).Substring(0, 8)

module FormGuide =

    open Domain

    type FormGuideOutcome = Win | Lose | Draw

    let private isTeamInFixture (fd:FixtureData) team = fd.home = team || fd.away = team
    let private getResultForTeam team (fd:FixtureData, result:Result) =
        let isHomeTeam = fd.home = team
        let outcome = getResultOutcome result.score
        match outcome with
        | Outcome.Draw -> Draw
        | HomeWin -> if isHomeTeam then Win else Lose
        | AwayWin -> if isHomeTeam then Lose else Win

    let getTeamFormGuide (gws:GameWeek list) team =
        gws
        |> getFixturesForGameWeeks
        |> List.choose(onlyClosedFixtures)
        |> List.map(fixtureToFixtureDataWithResult)
        |> List.filter(fun (_, r) -> r.IsSome)
        |> List.map(fun (fd, r) -> fd, r.Value)
        |> List.sortBy(fun (fd, _) -> fd.kickoff) |> List.rev
        |> List.filter(fun (fd, _) -> isTeamInFixture fd team)
        |> Seq.truncate 6
        |> Seq.map(getResultForTeam team)
        |> Seq.toList
        |> List.rev
    
module LeagueTableCalculation =

    open Domain

    type LeagueTableRow = { diffPosition:int; position:int; player:Player; correctScores:int; correctOutcomes:int; points:int }
    
    let getLeagueTableRows (league:League) gwsWithResults =
        let players = league.players
        let getSafeTail collection = if collection |> List.exists(fun _ -> true) then collection |> List.tail else collection
        let gwsWithResultsWithoutMax = gwsWithResults |> List.sortBy(fun gw -> -(getGameWeekNo gw.number)) |> getSafeTail
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
            { diffPosition=diffPos; position=pos; player=pl; correctScores=cs; correctOutcomes=co; points=pts }
        recentLge |> List.map(toDiffLgeRow)