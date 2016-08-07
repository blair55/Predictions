namespace Predictions.Api

open System

module Domain =

    let fixtureDataToFixture (fd:FixtureData) r =
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

    let getFixturesForGameWeeks (gws:GameWeek array) =
        gws |> Array.collect(fun gw -> gw.fixtures)

    let getFixtureDatasForGameWeeks (gws:GameWeek array) =
        gws
        |> Array.collect(fun gw -> gw.fixtures)
        |> Array.map(fixtureToFixtureData)
        |> Array.sortBy(fun fd -> fd.kickoff)

    let getGameWeeksWithAnyClosedFixturesWithResults (gws:GameWeek array) =
        gws |> Array.filter(fun gw -> gw.fixtures |> Array.exists(isFixtureClosedAndHaveResult))

    let tryFindFixture (gws:GameWeek array) fxid =
        gws
        |> Array.collect(fun gw -> gw.fixtures)
        |> Array.map(fun f -> f, fixtureToFixtureData f)
        |> Array.tryFind(fun (f, fd) -> fd.id = fxid)
        |> fstOption

    let tryFindFixtureWithGameWeek (gws:GameWeek array) fxid =
        gws
        |> Array.collect(fun gw -> gw.fixtures |> Array.map(fun f -> gw, f))
        |> Array.map(fun (gw, f) -> gw, f, fixtureToFixtureData f)
        |> Array.tryFind(fun (gw, f, fd) -> fd.id = fxid)

    let getMonthForGameWeek (gw:GameWeek) =
        gw.fixtures
        |> Array.map(fixtureToFixtureData)
        |> Array.minBy(fun fd -> fd.kickoff)
        |> fun fd -> fd.kickoff.ToString(monthFormat)

    let getGameWeeksForMonth (gws:GameWeek array) (month) =
        gws
        |> Array.map(fun gw -> gw, (gw.fixtures |> Array.map(fixtureToFixtureData) |> Array.minBy(fun fd -> fd.kickoff)))
        |> Array.filter(fun (_, f) -> f.kickoff.ToString(monthFormat) = month)
        |> Array.map(fun (gw, _) -> gw)

    let getFixturesInPlay gws =
        gws |> Array.map(fun gw -> gw, ([|gw|] |> getFixturesForGameWeeks |> Array.filter(isFixtureClosedAndHaveNoResult)))

    let getFirstKoForGw (gw:GameWeek) =
        gw.fixtures
        |> Array.map(fixtureToFixtureData)
        |> Array.minBy(fun f -> f.kickoff) |> fun f -> f.kickoff

    let doesGameWeekHaveAnyResults (gw:GameWeek) =
        getFixturesForGameWeeks [|gw|] |> Array.exists(isFixtureClosedAndHaveResult)

    // base calculations

    let getModifierMultiplier m =
        match m with
        | DoubleDown -> 2
        | NoModifier -> 1

    let getPointsForBracket b =
        match b with
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

    let onlyClosedFixtures f =
        match f with
        | OpenFixture _ -> None
        | ClosedFixture fr -> fr|>ClosedFixture|>Some

    let onlyOpenFixtures f =
        match f with
        | OpenFixture fd -> fd|>OpenFixture|>Some
        | ClosedFixture _ -> None

    let getPlayerBracketProfile (fixtures:Fixture array) player =
        let brackets = fixtures
                       |> Array.choose onlyClosedFixtures
                       |> Array.map fixtureToFixtureDataWithResult
                       |> Array.map(fun (fd, r) -> (tryFindPlayerPredictionForFixture player fd, r))
                       |> Array.map(fun (p, r) -> getBracketForPredictionComparedToResult p r)
        let countBracket bracketTest = brackets |> Array.filter(bracketTest) |> Array.length
        let totalCorrectScores = (function | CorrectScore _ -> true | _ -> false) |> countBracket
        let totalCorrectOutcomes = (function | CorrectOutcome _ -> true | _ -> false) |> countBracket
        let totalPoints = brackets |> Array.sumBy(getPointsForBracket)
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
        |> Array.map(fun p -> getPlayerBracketProfile fixtures p)
        |> Array.map(fun (p, cs, co, tp) -> (p, (cs, co, tp)))
        |> Array.sortBy(fun (_, (cs, co, totalPoints)) -> -totalPoints, -cs, -co)
        |> rank
        |> Seq.map(fun (r, (p, (cs, co, tp))) -> (r, p, cs, co, tp))
        |> Seq.toArray

    let getPlayerPointsForGameWeeks allPlayers (player:Player) gameWeeks =
        gameWeeks
        |> Array.map(fun gw -> gw, (getFixturesForGameWeeks [|gw|]))
        |> Array.map(fun (gw, fixtures) -> gw, getLeagueTable allPlayers fixtures)
        |> Array.map(fun (gwno, ltrList) -> gwno, ltrList |> Array.tryFind(fun (_, plr, _, _, _) -> plr.id = player.id))

    let getPlayerProfilePointsForGameWeeks player gameWeeks =
        gameWeeks
        |> Array.map(fun gw ->
                    let fixtures = getFixturesForGameWeeks [|gw|]
                    let (p, cs, co, tp) = getPlayerBracketProfile fixtures player
                    (gw, cs, co, tp))

    let getGameWeekDetailsForPlayer player gameWeek =
        getFixturesForGameWeeks [|gameWeek|]
        |> Array.map(fixtureToFixtureDataWithResult)
        |> Array.map(fun (fd, r) -> (fd, r, tryFindPlayerPredictionForFixture player fd))
        |> Array.map(fun (fd, r, p) -> (fd, r, p, (getBracketForPredictionComparedToResult p r |> getPointsForBracket)))

    let getOpenFixturesAndPredictionForPlayer (gws:GameWeek array) (player:Player) =
        gws
        |> getFixturesForGameWeeks
        |> Array.choose(onlyOpenFixtures)
        |> Array.map(fixtureToFixtureData)
        |> Array.map(fun fd -> fd, player.predictions |> Array.tryFind(fun p -> p.fixtureId = fd.id))
        |> Array.sortBy(fun (fd, _) -> fd.kickoff)

    let getOpenFixturesWithNoPredictionForPlayer (gws:GameWeek array) (player:Player) =
        gws
        |> getFixturesForGameWeeks
        |> Array.choose(onlyOpenFixtures)
        |> Array.map(fixtureToFixtureData)
        |> Array.filter(fun fd -> player.predictions |> Array.exists(fun p -> p.fixtureId = fd.id) = false)
        |> Array.sortBy(fun fd -> fd.kickoff)

    let getPlayersInPosition1 lgtbl =
        let plrs = lgtbl |> Array.filter(fun (pos, _, _, _, _) -> pos = 1) |> Array.map(   fun (_, plr, _, _, _) -> plr)
        let pts = lgtbl |> Array.maxBy(fun (_, _, _, _, pts) -> pts) |> fun (_, _, _, _, pts) -> pts
        (plrs, pts)

    let getPastGameWeeksWithWinner (gws:GameWeek array) players =
        gws
        |> Array.map(fun gw -> gw, getFixturesForGameWeeks [|gw|])
        |> Array.map(fun (gw, fixtures) -> gw, getLeagueTable players fixtures)
        |> Array.map(fun (gw, lgtbl) -> gw, lgtbl |> getPlayersInPosition1)
        |> Array.map(fun (gw, (plr, pts)) -> gw, plr, pts)

    let getPastMonthsWithWinner (gws:GameWeek array) players =
        gws
        |> Seq.groupBy(getMonthForGameWeek)
        |> Seq.toArray
        |> Array.map(fun (m, gws) -> m, getFixturesForGameWeeks (gws|>Seq.toArray))
        |> Array.map(fun (m, fixtures) -> m, getLeagueTable players fixtures)
        |> Array.map(fun (m, lgtbl) -> m, lgtbl |> getPlayersInPosition1)
        |> Array.map(fun (m, (plr, pts)) -> m, plr, pts)

    let getLeaguePositionForFixturesForPlayer (fixtures:Fixture array) players (player:Player) =
        fixtures
        |> getLeagueTable players
        |> Array.tryFind(fun (_, plr, _, _, _) -> plr.id = player.id)
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
        |> Array.choose(onlyClosedFixtures)
        |> Array.map(fixtureToFixtureDataWithResult)

    let getGameWeeksWithClosedFixtures (gws:GameWeek array) =
        gws |> Array.filter(fun gw -> [|gw|] |> getClosedFixturesForGameWeeks |> Array.isEmpty = false)

    let getIsGameWeekComplete (gw:GameWeek) =
        gw.fixtures |> Array.forall(isFixtureClosedAndHaveResult)

    let makeSureFixtureExists gws fxid =
        let fixture = tryFindFixture gws fxid
        NotFound "fixture does not exist" |> optionToResult fixture

    let isDoubleDownAvailableForPlayerInGameWeek (gw:GameWeek) (player:Player) =
        let gwFixtureDatas =
            gw.fixtures
            |> Array.map fixtureToFixtureData
        let isFixtureInGw fxid =
            gwFixtureDatas
            |> Array.exists (fun fd -> fd.id = fxid)
        let ddPrediction =
            player.predictions
            |> Array.filter(fun p -> p.fixtureId |> isFixtureInGw)
            |> Array.tryFind(fun p -> p.modifier = DoubleDown)
        match ddPrediction with
        | Some pr ->
            let fd = gwFixtureDatas |> Array.find(fun fd -> fd.id = pr.fixtureId)
            fixtureDataToFixture fd None |> isFixtureOpen
        | None -> true

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

    let getTeamFormGuide (gws:GameWeek array) team =
        let isTeamInFixture (fd:FixtureData) = fd.home = team || fd.away = team
        let getResultForTeam (fd:FixtureData, result:Result) =
            let gw = gws |> Array.find(fun gw -> gw.id = fd.gwId)
            let isHomeTeam = fd.home = team
            let outcome =
                match getResultOutcome result.score with
                | Outcome.Draw -> Draw
                | HomeWin -> if isHomeTeam then Win else Lose
                | AwayWin -> if isHomeTeam then Lose else Win
            { FormGuideResultContainer.gameWeek=gw; fd=fd; result=result; outcome=outcome }
        gws
        |> getFixturesForGameWeeks
        |> Array.choose(onlyClosedFixtures)
        |> Array.map(fixtureToFixtureDataWithResult)
        |> Array.filter(fun (_, r) -> r.IsSome)
        |> Array.map(fun (fd, r) -> fd, r.Value)
        |> Array.sortBy(fun (fd, _) -> fd.kickoff) |> Array.rev
        |> Array.filter(fun (fd, _) -> isTeamInFixture fd)
        |> Seq.truncate 6
        |> Seq.map(getResultForTeam)
        |> Seq.toArray
        |> Array.rev

    let getTeamFormGuideOutcome (gws:GameWeek array) team =
        getTeamFormGuide gws team |> Array.map(fun r -> r.outcome)


module LeagueTableCalculation =

    open Domain

    type LeagueTableRow = { diffPosition:int; position:int; player:Player; correctScores:int; correctOutcomes:int; points:int }

    let getLeagueTableRows (league:League) gwsWithResults =
        let players = league.players
//        let getSafeTail collection = if collection |> Array.exists(fun _ -> true) then collection |> Array.tail else collection
        let getSafeTail collection =
            collection |> Array.mapi(fun index item -> if index = 0 then None else Some item) |> Array.choose(fun r -> r)
        let gwsWithResultsWithoutMax = gwsWithResults |> Array.sortBy(fun gw -> -(getGameWeekNo gw.number)) |> getSafeTail
        let recentFixtures = getFixturesForGameWeeks gwsWithResults
        let priorFixtures = getFixturesForGameWeeks gwsWithResultsWithoutMax
        let recentLge = getLeagueTable players recentFixtures
        let priorLge = getLeagueTable players priorFixtures
        let findPlayerPriorPos (player:Player) currentPos =
            let playerPriorLgeRow = priorLge |> Array.tryFind(fun (_, plr, _, _, _) -> plr.id = player.id)
            match playerPriorLgeRow with | Some (pos, _, _, _, _) -> pos | None -> currentPos
        let toDiffLgeRow (pos, plr, cs, co, pts) =
            let priorPos = findPlayerPriorPos plr pos
            let diffPos = priorPos - pos
            { diffPosition=diffPos; position=pos; player=plr; correctScores=cs; correctOutcomes=co; points=pts }
        recentLge |> Array.map(toDiffLgeRow)

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


module FixtureSourcing =

    open FSharp.Data

    let [<Literal>] PremFixturesUrl = "https://fantasy.premierleague.com/drf/fixtures/?event=1"
    type PremFixtures = JsonProvider<PremFixturesUrl>

    let premTeamIdToName = function
        | 1 -> "Arsenal" | 2 -> "Bournemouth" | 3 -> "Burnley" | 4 -> "Chelsea"
        | 5 -> "Crystal Palace" | 6 -> "Everton" | 7 -> "Hull" | 8 -> "Leicester"
        | 9 -> "Liverpool" | 10 -> "Man City" | 11 -> "Man Utd" | 12 -> "Middlesbrough"
        | 13 -> "Southampton" | 14 -> "Stoke" | 15 -> "Sunderland" | 16 -> "Swansea"
        | 17 -> "Spurs" | 18 -> "Watford" | 19 -> "West Brom" | 20 -> "West Ham"
        | _ -> failwith "Unrecognised team id" 

    let getNewPremGwFixtures no =
        let premFixturesUrl = sprintf "https://fantasy.premierleague.com/drf/fixtures/?event=%i" no
        PremFixtures.Load(premFixturesUrl)
        |> Seq.map(fun f -> f.KickoffTime.AddHours 1., f.TeamH |> premTeamIdToName, f.TeamA |> premTeamIdToName)
        |> Seq.toList

    let [<Literal>] EuroUrl = "http://api.football-data.org/v1/soccerseasons/424/fixtures"
    type Fixtures = JsonProvider<EuroUrl>

    let getNewEuroGwFixtures no =
        let fixtures = Fixtures.Load(EuroUrl)
        fixtures.Fixtures
        |> Seq.filter(fun f -> f.Matchday = no)
        |> Seq.map(fun f -> (f.Date.AddHours(1.)), f.HomeTeamName, f.AwayTeamName)
        |> Seq.toList

module TeamNames =

    let getAbrvTeamName team =
        match team with
        | "Man City"       -> "MCI"
        | "Man Utd"        -> "MUN"
        | "Spurs"          -> "TOT"
        | "Stoke"          -> "STK"
        | "West Brom"      -> "WBA"
        | "West Ham"       -> "WHU"
        | _ -> team.Substring(0, 3)

open Domain

module TeamLeagueTable =

    type TeamRow = { pos:int; team:string; played:int; won:int; drawn:int; lost:int; gf:int; ga:int; gd:int; points:int }

    let getTeamLeagueTableForPlayerPredictions (plr:Player) (gws:GameWeek array) =
        let fxs =
            getFixturesForGameWeeks gws
            |> Array.map fixtureToFixtureData
            |> List.ofArray

        let distinctTeams =
            (fxs |> List.map(fun f -> f.home)) @ (fxs |> List.map(fun f -> f.away))
            |> Seq.distinct

        let isPredictionForTeam team fixtureId fn =
            match fxs |> List.tryFind(fun f -> f.id = fixtureId) with
            | Some f -> fn(f) = team
            | _ -> false

        let groupedByTeam teams =
            teams
            |> Seq.map(
                fun t ->
                    let homePredictions =
                        plr.predictions |> Array.filter(fun pr -> isPredictionForTeam t pr.fixtureId (fun f -> f.home)) |> List.ofArray
                    let awayPredictions =
                        plr.predictions |> Array.filter(fun pr -> isPredictionForTeam t pr.fixtureId (fun f -> f.away)) |> List.ofArray
                    t, homePredictions, awayPredictions)

        let getTeamRow (team, homePredictions, awayPredictions) =
            let (homewin, homedraw, homeloss) = GetOutcomeCounts homePredictions (0, 0, 0)
            let (awayloss, awaydraw, awaywin) = GetOutcomeCounts awayPredictions (0, 0, 0)
            let gf = (homePredictions |> List.sumBy(fun pr -> fst pr.score)) + (awayPredictions |> List.sumBy(fun pr -> snd pr.score))
            let ga = (homePredictions |> List.sumBy(fun pr -> snd pr.score)) + (awayPredictions |> List.sumBy(fun pr -> fst pr.score))
            let played = homePredictions.Length + awayPredictions.Length
            let won = homewin + awaywin
            let drawn = homedraw + awaydraw
            let lost = homeloss + awayloss
            let gd = gf - ga
            let points = (won * 3) + drawn
            (team, played, won, drawn, lost, gf, ga, gd, points)

        distinctTeams
        |> groupedByTeam
        |> Array.ofSeq
        |> Array.map(getTeamRow)
        |> Array.map(fun (t, p, w, d, l, gf, ga, gd, points) -> ((t, p, w, d, l, ga), (gf, gd, points)))
        |> Array.sortBy(fun (_, (gf, gd, pts)) -> -pts, -gd, -gf)
        |> rank
        |> Seq.map(fun (r, ((t, p, w, d, l, ga), (gf, gd, points))) ->
                            { pos = r; team = t; played = p; won = w; drawn = d; lost = l; gf = gf; ga = ga; gd = gd; points = points })

module Achievements =

    type Achievement =
        | HomeBoy of FixtureData
        | Traveller of FixtureData
        | ParkedTheBus of FixtureData
        | MysticMeg of FixtureData
        | GoalFrenzy of FixtureData
        | BoreDraw of FixtureData
        | ScoreDraw of FixtureData
        | GreatWeek of FixtureData
        | PerfectWeek of FixtureData
        | ShootTheMoon of FixtureData
        | EarlyBird of GameWeek
        | GlobalLeagueTopWeek of GameWeek
        | Guvna of League

    let getAchName = function
        | HomeBoy _ -> "HomeBoy"
        | Traveller _ -> "Traveller"
        | ParkedTheBus _ -> "ParkedTheBus"
        | MysticMeg _ -> "MysticMeg"
        | GoalFrenzy _ -> "GoalFrenzy"
        | BoreDraw _ -> "BoreDraw"
        | ScoreDraw _ -> "ScoreDraw"
        | GreatWeek _ -> "GreatWeek"
        | PerfectWeek _ -> "PerfectWeek"
        | ShootTheMoon _ -> "ShootTheMoon"
        | EarlyBird _ -> "EarlyBird"
        | GlobalLeagueTopWeek _ -> "GlobalLeagueTopWeek"
        | Guvna _ -> "Guvna"

    // acked fixture achs
    // id // playerid // ach // fxid // created
    // acked gameweek achs
    // id // playerid // ach // gwid // created
    // acked league achs
    // id // playerid // ach // lgid // created

    let getAchievementsForPlayer (plr:Player) gws =

        // calculate achs

        let fs = getClosedFixturesForGameWeeks gws
                 |> Seq.filter(fun (_, r) -> r.IsSome)
                 |> Seq.map(fun (fd, r) -> (fd, r.Value))

        let correctScores (plr:Player) (fd:FixtureData) (r:Result) modF =
            let pr = plr.predictions |> Seq.tryFind(fun pr -> pr.fixtureId = fd.id)
            match Some r |> getBracketForPredictionComparedToResult pr with
            | Incorrect
            | CorrectOutcome _ -> false
            | CorrectScore m -> modF m

        let isDoubleDown = function
            | DoubleDown -> true
            | _ -> false

        let fixturesWhenCsDd outcome fs =
            fs
            |> Seq.filter(fun (fd, r:Result) ->
                if r.score |> getResultOutcome = outcome then
                    isDoubleDown |> correctScores plr fd r
                else false)
            |> Seq.map(fun (fd, _) -> fd)

        let allAchs = seq {
            yield! fs |> fixturesWhenCsDd HomeWin |> Seq.map HomeBoy
            yield! fs |> fixturesWhenCsDd AwayWin |> Seq.map Traveller
            yield! fs |> fixturesWhenCsDd Draw |> Seq.map ParkedTheBus
        }

        // filter out already acked achs

        let ackedAchs:Achievement seq = Seq.empty // from db



        let getUnAckedAchs (allAchs:Achievement seq) (ackedAchs:Achievement seq) =
            let (|Gold|Silver|Bronze|) = function
                | e when e > 4 -> Gold
                | e when e > 2 -> Silver
                | _ -> Bronze

            let group achs =
                achs
                |> Seq.groupBy getAchName
                |> Seq.map(fun (name, achs) -> name, achs |> Seq.length)
                |> Map.ofSeq
            let all, acked = allAchs |> group, ackedAchs |> group

            //let s = all |> Map.map(fun r -> )

            ()


        //let newAchs = achs |> Seq.filter(fun a -> ackedAchs |> Seq.exists(fun aa -> aa = a) = false)

        // notify of new achs

        // share ach

        ()
