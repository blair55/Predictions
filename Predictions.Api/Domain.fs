﻿namespace Predictions.Api

open System

module Domain =

    type LgId = LgId of Guid
    type FxId = FxId of Guid
    type GwId = GwId of Guid
    type PlId = PlId of Guid
    type PrId = PrId of Guid
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
    let getSnId (SnId id) = id
    let getSnYr (SnYr year) = year
    let getPlayerName (PlayerName plrName) = plrName
    let getLeagueName (LeagueName lgeName) = lgeName
    let getExternalPlayerId (ExternalPlayerId expid) = expid
    let getExternalLoginProvider (ExternalLoginProvider exprovider) = exprovider

    let currentSeason = SnYr "prem-2015/16"
    let monthFormat = "MMMM yyyy"
    let globalLeagueId = "global"
    let globalLeaguePageSize = 30
    let maxPlayersPerLeague = 100

    type Score = int * int
    type Result = { score:Score }
    type FixtureData = { id:FxId; gwId:GwId; home:Team; away:Team; kickoff:KickOff }
    type Fixture =
         | OpenFixture of FixtureData
         | ClosedFixture of (FixtureData * Result option)
    type PredictionModifier = | DoubleDown | NoModifier
    type Prediction = { id:PrId; score:Score; fixtureId:FxId; playerId:PlId; modifier:PredictionModifier }
    type Player = { id:PlId; name:PlayerName; predictions:Prediction list; isAdmin:bool }
    type GameWeek = { id:GwId; number:GwNo; description:string; fixtures:Fixture list }
    type Season = { id:SnId; year:SnYr; gameWeeks:GameWeek list }
    type Outcome = HomeWin | AwayWin | Draw
    type Bracket = CorrectScore of PredictionModifier | CorrectOutcome of PredictionModifier | Incorrect

    type League = { id:LgId; name:LeagueName; players:Player list; adminId:PlId }

    type AppError =
        | NotLoggedIn of string
        | Forbidden of string
        | Invalid of string
        | NotFound of string
        | InternalError of string

    let fixtureDataToFixture fd r =
        match fd.kickoff > GMTDateTime.Now() with
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
        
    let getFixtureDatasForGameWeeks (gws:GameWeek list) =
        gws
        |> List.collect(fun gw -> gw.fixtures)
        |> List.map(fixtureToFixtureData)
        |> List.sortBy(fun fd -> fd.kickoff)

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

    let getModifierMultiplier = function
        | DoubleDown -> 2
        | NoModifier -> 1

    let getPointsForBracket = function
        | CorrectScore m -> 3 * getModifierMultiplier m
        | CorrectOutcome m -> 1 * getModifierMultiplier m
        | Incorrect -> 0

    let getResultOutcome score =
        if fst score > snd score then HomeWin
        else if fst score < snd score then AwayWin
        else Draw

    let getBracketForPredictionComparedToResult (prediction:Prediction option) (result:Result option) =
        if prediction.IsNone || result.IsNone then Incorrect
        else if prediction.Value.score = result.Value.score then CorrectScore prediction.Value.modifier
        else
            let predictionOutcome = getResultOutcome prediction.Value.score
            let resultOutcome = getResultOutcome result.Value.score
            if predictionOutcome = resultOutcome then CorrectOutcome prediction.Value.modifier
            else Incorrect

    let tryFindPlayerPredictionForFixture (player:Player) (fd:FixtureData) =
        player.predictions |> Seq.tryFind(fun pr -> pr.fixtureId = fd.id)
    
    let onlyClosedFixtures = function
        | OpenFixture _ -> None
        | ClosedFixture fr -> fr|>ClosedFixture|>Some
        
    let onlyOpenFixtures = function
        | OpenFixture fd -> fd|>OpenFixture|>Some
        | ClosedFixture _ -> None

    let getPlayerBracketProfile (fixtures:Fixture list) player =
        let brackets = fixtures
                       |> List.choose onlyClosedFixtures
                       |> List.map fixtureToFixtureDataWithResult
                       |> List.map(fun (fd, r) -> (tryFindPlayerPredictionForFixture player fd, r))
                       |> List.map(fun (p, r) -> getBracketForPredictionComparedToResult p r)
        let countBracket bracketTest = brackets |> List.filter(bracketTest) |> List.length
        let totalCorrectScores = (function | CorrectScore _ -> true | _ -> false) |> countBracket
        let totalCorrectOutcomes = (function | CorrectOutcome _ -> true | _ -> false) |> countBracket
        let totalPoints = brackets |> List.sumBy(fun b -> getPointsForBracket b)
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

    let getPlayerPointsForGameWeeks allPlayers (player:Player) gameWeeks =
        gameWeeks
        |> List.map(fun gw -> gw, (getFixturesForGameWeeks [gw]))
        |> List.map(fun (gw, fixtures) -> gw, getLeagueTable allPlayers fixtures)
        |> List.map(fun (gwno, ltrList) -> gwno, ltrList |> List.tryFind(fun (_, plr, _, _, _) -> plr.id = player.id))

    let getPlayerProfilePointsForGameWeeks player gameWeeks =
        gameWeeks
        |> List.map(fun gw -> 
                    let fixtures = getFixturesForGameWeeks [gw]
                    let (p, cs, co, tp) = getPlayerBracketProfile fixtures player
                    (gw, cs, co, tp))

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
        |> List.map(fun fd -> fd, player.predictions |> List.tryFind(fun p -> p.fixtureId = fd.id))
        |> List.sortBy(fun (fd, _) -> fd.kickoff)

    let getOpenFixturesWithNoPredictionForPlayer (gws:GameWeek list) (player:Player) =
        gws
        |> getFixturesForGameWeeks
        |> List.choose(onlyOpenFixtures)
        |> List.map(fixtureToFixtureData)
        |> List.filter(fun fd -> player.predictions |> List.exists(fun p -> p.fixtureId = fd.id) = false)
        |> List.sortBy(fun fd -> fd.kickoff)

    let getPlayersInPosition1 lgtbl =
        let plrs = lgtbl |> List.filter(fun (pos, _, _, _, _) -> pos = 1) |> List.map(   fun (_, plr, _, _, _) -> plr)
        let pts = lgtbl |> List.maxBy(fun (_, _, _, _, pts) -> pts) |> fun (_, _, _, _, pts) -> pts
        (plrs, pts)

    let getPastGameWeeksWithWinner (gws:GameWeek list) players =
        gws
        |> List.map(fun gw -> gw, getFixturesForGameWeeks [gw])
        |> List.map(fun (gw, fixtures) -> gw, getLeagueTable players fixtures)
        |> List.map(fun (gw, lgtbl) -> gw, lgtbl |> getPlayersInPosition1)
        |> List.map(fun (gw, (plr, pts)) -> gw, plr, pts)

    let getPastMonthsWithWinner (gws:GameWeek list) players =
        gws
        |> Seq.groupBy(getMonthForGameWeek)
        |> Seq.toList
        |> List.map(fun (m, gws) -> m, getFixturesForGameWeeks (gws|>Seq.toList))
        |> List.map(fun (m, fixtures) -> m, getLeagueTable players fixtures)
        |> List.map(fun (m, lgtbl) -> m, lgtbl |> getPlayersInPosition1)
        |> List.map(fun (m, (plr, pts)) -> m, plr, pts)

    let getLeaguePositionForFixturesForPlayer (fixtures:Fixture list) players (player:Player) =
        fixtures
        |> getLeagueTable players
        |> List.tryFind(fun (_, plr, _, _, _) -> plr.id = player.id)
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

    let getIsGameWeekComplete (gw:GameWeek) =
        gw.fixtures |> List.forall(isFixtureClosedAndHaveResult)

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

    let tryToCreateScoreFromSbm home away =
        if home >= 0 && away >= 0 then Success(home, away) else invalid "cannot submit negative score"

    let getShareableLeagueId (leagueId:LgId) =
        (str <| getLgId leagueId).Substring(0, 8)

module FormGuide =

    open Domain

    type FormGuideOutcome = Win | Lose | Draw
    type FormGuideResultContainer = { gameWeek:GameWeek; fd:FixtureData; result:Result; outcome:FormGuideOutcome }

    let getTeamFormGuide (gws:GameWeek list) team =
        let isTeamInFixture (fd:FixtureData) = fd.home = team || fd.away = team
        let getResultForTeam (fd:FixtureData, result:Result) =
            let gw = gws |> List.find(fun gw -> gw.id = fd.gwId)
            let isHomeTeam = fd.home = team
            let outcome = 
                match getResultOutcome result.score with
                | Outcome.Draw -> Draw
                | HomeWin -> if isHomeTeam then Win else Lose
                | AwayWin -> if isHomeTeam then Lose else Win
            { FormGuideResultContainer.gameWeek=gw; fd=fd; result=result; outcome=outcome }
        gws
        |> getFixturesForGameWeeks
        |> List.choose(onlyClosedFixtures)
        |> List.map(fixtureToFixtureDataWithResult)
        |> List.filter(fun (_, r) -> r.IsSome)
        |> List.map(fun (fd, r) -> fd, r.Value)
        |> List.sortBy(fun (fd, _) -> fd.kickoff) |> List.rev
        |> List.filter(fun (fd, _) -> isTeamInFixture fd)
        |> Seq.truncate 6
        |> Seq.map(getResultForTeam)
        |> Seq.toList
        |> List.rev

    let getTeamFormGuideOutcome (gws:GameWeek list) team =
        getTeamFormGuide gws team |> List.map(fun r -> r.outcome)


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
        let findPlayerPriorPos (player:Player) currentPos =
            let playerPriorLgeRow = priorLge |> List.tryFind(fun (_, plr, _, _, _) -> plr.id = player.id)
            match playerPriorLgeRow with | Some (pos, _, _, _, _) -> pos | None -> currentPos
        let toDiffLgeRow (pos, plr, cs, co, pts) =
            let priorPos = findPlayerPriorPos plr pos
            let diffPos = priorPos - pos
            { diffPosition=diffPos; position=pos; player=plr; correctScores=cs; correctOutcomes=co; points=pts }
        recentLge |> List.map(toDiffLgeRow)

module List =
    
    type private ListLengthResult = | AisShorter | BisShorter | Same
    let makeListsEqualLength (a:List<_>) (b:List<_>) =
        let result =
            if a.Length < b.Length then AisShorter
            else if b.Length < a.Length then BisShorter
            else Same
        match result with
        | AisShorter -> (a, b |> Seq.truncate a.Length |> Seq.toList)
        | BisShorter -> (a |> Seq.truncate b.Length |> Seq.toList, b)
        | Same -> (a, b)

open FSharp.Data

module FixtureSourcing =

    let getNewGwFixtures no =
        let url = sprintf "http://fantasy.premierleague.com/fixtures/%i" no
        let gwhtml = Http.RequestString(url, headers = ["X-Requested-With", "XMLHttpRequest"])
        let results = HtmlDocument.Parse(gwhtml)
        results.Descendants ["tr"]
        |> Seq.map(fun tr -> 
            let tds = tr.Descendants ["td"] |> Seq.toList
            let dateA = (tds.[0].InnerText()).Split(' ') |> Seq.toList
            let dateS = sprintf "%s %s %s 2015" dateA.[2] dateA.[0] dateA.[1]
            let date = Convert.ToDateTime(dateS)
            let home = tds.[1].InnerText()
            let away = tds.[5].InnerText()
            date, home, away)
        |> Seq.toList

module TeamNames =
    
    let getAbrvTeamName team =
        match team with
        | "Arsenal"        -> "ARS"
        | "Aston Villa"    -> "AVL"
        | "Bournemouth"    -> "BOU"
        | "Chelsea"        -> "CHE"
        | "Crystal Palace" -> "CRY"
        | "Everton"        -> "EVE"
        | "Leicester"      -> "LEI"
        | "Liverpool"      -> "LIV"
        | "Man City"       -> "MCI"
        | "Man Utd"        -> "MUN"
        | "Newcastle"      -> "NEW"
        | "Norwich"        -> "NOR"
        | "Southampton"    -> "SOU"
        | "Spurs"          -> "TOT"
        | "Stoke"          -> "STK"
        | "Sunderland"     -> "SUN"
        | "Swansea"        -> "SWA"
        | "Watford"        -> "WAT"
        | "West Brom"      -> "WBA"
        | "West Ham"       -> "WHU"
        | _ -> team.Substring(0, 3)


open Domain

module Achievements =

    type Achievement =
        | HomeBoy
        | Traveller
        | ParkedTheBus
        | MysticMeg
        | GoalFrenzy
        | BoreDraw
        | ScoreDraw
        | GreatWeek
        | PerfectWeek
        | ShootTheMoon
        | EarlyBird
        | GlobalLeagueTopWeek
        | PrivateLeagueTopWeek
        | Guvna

    type AchLevel =
        | Bronze
        | Silver
        | Gold

    type AckedAch = { achievement:Achievement; level:AchLevel }

    let getAchievementsForPlayer (plr:Player) gws =

        // calculate achs

        let fs = getClosedFixturesForGameWeeks gws
                    |> Seq.filter(fun (_, r) -> r.IsSome)
                    |> Seq.map(fun (fd, r) -> (fd, r.Value))

        let correctScoreDoubleDowns (plr:Player) (fd:FixtureData) (r:Result) = 
            let pr = plr.predictions |> Seq.tryFind(fun pr -> pr.fixtureId = fd.id)
            match Some r |> getBracketForPredictionComparedToResult pr with
            | Incorrect
            | CorrectOutcome _ -> false
            | CorrectScore modifier ->
                match modifier with
                | NoModifier -> false
                | DoubleDown -> true
        
        let fixturesWhen fs outcome =
            fs |> Seq.filter(fun (fd, r:Result) -> if r.score |> getResultOutcome = outcome then correctScoreDoubleDowns plr fd r else false)

        let homeWins = fixturesWhen fs HomeWin
        let awayWins = fixturesWhen fs AwayWin
        let draws = fixturesWhen fs Draw

        // filter out already acked achs

        let ackedAchs = [
            { achievement=HomeBoy; level=Bronze } ]

//        let newAchs = achs |> Seq.filter(fun a -> ackedAchs |> Seq.exists(fun aa -> aa = a) = false)

        // notify of new achs

        // share ach
        
        ()
