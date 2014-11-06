namespace Predictions.Api

open System

module Domain =

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

    let nguid() = Guid.NewGuid()
    let newFxId() = nguid()|>FxId
    let newPrId() = nguid()|>PrId
    let newGwId() = nguid()|>GwId
    let newRsId() = nguid()|>RsId
    let newSnId() = nguid()|>SnId
    let newPlId() = nguid()|>PlId
    
    let getPlayerId (PlId id) = id
    let getGameWeekNo (GwNo n) = n
    let getFxId (FxId id) = id
    let getGwId (GwId id) = id
    let getPrId (PrId id) = id
    let getRsId (RsId id) = id
    let getSnId (SnId id) = id
    let getSnYr (SnYr year) = year

    let currentSeason = SnYr "2014/15"

    type Score = int * int

    type Player = { id:PlId; name:string; role:Role }
    type Prediction = { id:PrId; score:Score; player:Player }
    type Result = { id:RsId; score:Score }
    type FixtureData = { id:FxId; home:Team; away:Team; kickoff:KickOff; predictions:Prediction list; }
    type Fixture =
        | OpenFixture of FixtureData
        | ClosedFixture of (FixtureData * Result option)
    type GameWeek = { id:GwId; number:GwNo; description:string; fixtures:Fixture list }
    type Season = { id:SnId; year:SnYr; gameWeeks:GameWeek list }

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
    let isFixtureClosedAndHaveResult f = match f with | OpenFixture _ -> false | ClosedFixture (fd, r) -> r.IsSome

    let getFixturesForGameWeeks (gws:GameWeek list) =
        gws |> List.collect(fun gw -> gw.fixtures)
        
    let getGameWeeksWithAnyClosedFixturesWithResults (gws:GameWeek list) =
        gws |> List.filter(fun gw -> gw.fixtures |> List.exists(isFixtureClosedAndHaveResult))
        
    let getPredictionsForGameWeeksForPlayer (gws:GameWeek list) pl =
        gws
        |> List.collect(fun gw -> gw.fixtures)
        |> List.map(fixtureToFixtureData)
        |> List.collect(fun fd -> fd.predictions)
        |> List.filter(fun pr -> pr.player = pl)

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

    let tryFindPredictionWithFixture (gws:GameWeek list) prid =
        gws
        |> List.collect(fun gw -> gw.fixtures)
        |> List.map(fixtureToFixtureData)
        |> List.map(fun fd -> fd, fd.predictions)
        |> List.collect(fun (fd, predictions) -> predictions |> List.map(fun p -> fd, p))
        |> List.tryFind(fun (_, p) -> p.id = prid)


    let getMonthForGameWeek (gw:GameWeek) =
        gw.fixtures
        |> List.map(fixtureToFixtureData)
        |> List.minBy(fun fd -> fd.kickoff)
        |> fun fd -> fd.kickoff.ToString("MMMM")

    let getGameWeeksForMonth (gws:GameWeek list) (month) =
        gws
        |> List.map(fun gw -> gw, (gw.fixtures |> List.map(fixtureToFixtureData) |> List.minBy(fun fd -> fd.kickoff)))
        |> List.filter(fun (_, f) -> f.kickoff.ToString("MMMM") = month)
        |> List.map(fun (gw, _) -> gw)

    type Outcome = HomeWin | AwayWin | Draw
    type Bracket = CorrectScore | CorrectOutcome | Incorrect
    type ClosedFixtureStatus = AwaitingResult | ResultAdded
    type FixtureStatus = Open | ClosedFixtureStatus

    let isPlayerAdmin (player:Player) =
        match player.role with
        | Admin -> true
        | _ -> false

    let findPlayerById (players:Player list) id = players |> List.find(fun p -> p.id = id)

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

    let tryFindPlayerPrediction (predictions:Prediction list) player = predictions |> List.tryFind(fun p -> p.player = player)
    
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
                       |> List.choose(onlyClosedFixtures)
                       |> List.map(fixtureToFixtureDataWithResult)
                       |> List.map(fun (fd, r) -> (tryFindPlayerPrediction fd.predictions player, r))
                       |> List.map(fun (p, r) -> getBracketForPredictionComparedToResult p r)
        let totalPoints = brackets |> List.sumBy(fun b -> getPointsForBracket b)
        let countBracket l bracket = l |> List.filter(fun b -> b = bracket) |> List.length
        let totalCorrectScores = CorrectScore |> (countBracket brackets)
        let totalCorrectOutcomes = CorrectOutcome |> (countBracket brackets)
        player, totalCorrectScores, totalCorrectOutcomes, totalPoints

    let getLeagueTable players fixtures =
        players
        |> List.map(fun p -> getPlayerBracketProfile fixtures p)
        |> List.sortBy(fun (_, cs, co, totalPoints) -> -totalPoints, -cs, -co)
        |> List.mapi(fun i (p, cs, co, tp) -> (i+1), p, cs, co, tp)

    let getPlayerPointsForGameWeeks allPlayers player gameWeeks =
        gameWeeks
        |> List.map(fun gw -> gw, (getFixturesForGameWeeks [gw]))
        |> List.map(fun (gw, fixtures) -> gw, getLeagueTable allPlayers fixtures)
        |> List.map(fun (gwno, ltrList) -> gwno, ltrList |> List.tryFind(fun (_, p, _, _, _) -> p = player))

    let getGameWeekDetailsForPlayer player gameWeek =
        getFixturesForGameWeeks [gameWeek]
        |> List.map(fixtureToFixtureDataWithResult)
        |> List.map(fun (fd, r) -> (fd, r, tryFindPlayerPrediction fd.predictions player))
        |> List.map(fun (fd, r, p) -> (fd, r, p, (getBracketForPredictionComparedToResult p r |> getPointsForBracket)))

    let getOpenFixturesAndPredictionForPlayer (gws:GameWeek list) (players:Player list) (plId:PlId) =
        let player = findPlayerById players plId
        gws
        |> getFixturesForGameWeeks
        |> List.choose(onlyOpenFixtures)
        |> List.map(fixtureToFixtureData)
        |> List.map(fun fd -> fd, fd.predictions |> List.tryFind(fun p -> p.player = player))
        |> List.sortBy(fun (fd, _) -> fd.kickoff)
    
    let getOpenFixturesWithPredictionForPlayer (gws:GameWeek list) (players:Player list) (plId:PlId) =
        let player = findPlayerById players plId
        gws
        |> getFixturesForGameWeeks
        |> List.choose(onlyOpenFixtures)
        |> List.map(fixtureToFixtureData)
        |> List.filter(fun fd -> fd.predictions |> List.exists(fun p -> p.player = player))
        |> List.map(fun fd -> fd, fd.predictions |> List.find(fun p -> p.player = player))
        |> List.sortBy(fun (fd, _) -> fd.kickoff)

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
        |> List.find(fun (_, plr, _, _, _) -> plr = player)
        |> (fun (pos, _, _, _, _) -> pos)

        
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
        gws
        |> List.filter(fun gw -> [gw] |> getClosedFixturesForGameWeeks |> List.isEmpty = false)


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

    
    let createPrediction player score = { Prediction.id=newPrId(); player=player; score=score }

    let tryAddResultToFixture r f =
        match f with
        | OpenFixture f -> Failure "cannot add result to fixture with ko in future"
        | ClosedFixture (f, _) -> Success(ClosedFixture(f, Some r))

    let checkFixtureIsOpen (f, p) =
        match f with
        | OpenFixture fd -> Success (fd, p)
        | ClosedFixture _ -> Failure "cannot add prediction to fixture with ko in past"

    let checkPlayerHasNoSubmittedPredictionsForFixture ((fd:FixtureData), (pr:Prediction)) =
        let hasAlreadySubmitted = fd.predictions |> List.exists(fun p -> p.player = pr.player)
        if hasAlreadySubmitted then Failure "cannot submit more than one prediction for fixture" else Success (fd, pr)

    let tryAddPredictionToFixture (f, p) =
        let addPredictionToFixture(fd, p) = (OpenFixture({fd with predictions=p::fd.predictions}), p)
        (f, p) |> (checkFixtureIsOpen
               >> bind checkPlayerHasNoSubmittedPredictionsForFixture
               >> bind (switch addPredictionToFixture))

    let tryViewFixture f =
        match f with
        | OpenFixture _ -> Failure "cannot view fixture with ko in future"
        | ClosedFixture(fd, r) -> Success(fd, r)

    let tryToCreateScoreFromSbm home away =
        if home >= 0 && away >= 0 then Success(home, away) else Failure "cannot submit negative score"

    let tryToCreateFixtureDataFromSbm home away (ko:string) =
        let (isKoValid, kickoff) = DateTime.TryParse(ko)
        if isKoValid=false then Failure "fixture kickoff time is invalid"
        else if kickoff < DateTime.Now then Failure "fixture kickoff cannot be in the past"
        else if home = away then Failure "fixture home and away team cannot be the same"
        else Success({id=newFxId(); home=home; away=away; kickoff=kickoff; predictions=[]})
    