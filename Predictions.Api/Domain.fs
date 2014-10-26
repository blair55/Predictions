namespace Predictions.Api

open System

module Domain =

    type FxId = FxId of Guid
    type GwId = GwId of Guid
    type PlId = PlId of Guid
    type PrId = PrId of Guid
    type RsId = RsId of Guid
    type SnId = SnId of Guid
    type Team = string
    type KickOff = DateTime
    type GwNo = GwNo of int
    type Role = User | Admin

    let nguid() = Guid.NewGuid()
    let newFxId = nguid()|>FxId
    let newPrId = nguid()|>PrId
    let newGwId = nguid()|>GwId
    let newRsId = nguid()|>RsId

    // deconstructors
    let getPlayerId (PlId id) = id
    let getGameWeekNo (GwNo n) = n
    let getFxId (FxId id) = id
    let getGwId (GwId id) = id
    let getPrId (PrId id) = id
    let getRsId (RsId id) = id
    let getSnId (SnId id) = id


    type Score = int * int
//    type Player = { id:PlId; name:string; role:Role }
//    type GameWeek = { id:GwId; number:GwNo; description:string }
//    type Fixture = { id:FxId; gameWeek:GameWeek; home:Team; away:Team; kickoff:KickOff }
//    type Prediction = { fixture:Fixture; score:Score; player:Player }
//    type Result = { fixture:Fixture; score:Score }


    type Player = { id:PlId; name:string; role:Role }
    type Prediction = { id:PrId; score:Score; player:Player }
    type Result = { id:RsId; score:Score }
    type FixtureData = { id:FxId; home:Team; away:Team; kickoff:KickOff; predictions:Prediction list; }
    type Fixture =
        | OpenFixture of FixtureData
        | ClosedFixture of (FixtureData * Result option)
    type GameWeek = { id:GwId; number:GwNo; description:string; fixtures:Fixture list }
    type Season = { id:SnId; year:string; gameWeeks:GameWeek list }

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


//    let getPredictionsForGameWeeks (gws:GameWeek list) =
//        gws
//        |> List.collect(fun gw -> gw.fixtures)
//        |> List.map(fixtureToFixtureData)
//        |> List.collect(fun fd -> fd.predictions)
    
    let getFixturesForGameWeeks (gws:GameWeek list) =
        gws |> List.collect(fun gw -> gw.fixtures)

    let getPlayersForGameWeeks (gws:GameWeek list) =
        gws
        |> List.collect(fun gw -> gw.fixtures)
        |> List.map(fixtureToFixtureData)
        |> List.collect(fun fd -> fd.predictions)
        |> List.map(fun pr -> pr.player)
        |> Seq.distinct |> Seq.toList

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
        |> List.map(fun gw -> gw, gw.fixtures)
        |> List.collect(fun (gw, fixtures) -> gw.fixtures |> List.map(fun f -> gw, f))
        |> List.map(fun (gw, f) -> gw, f, fixtureToFixtureData f)
        |> List.tryFind(fun (gw, f, fd) -> fd.id = fxid)

    type Outcome = HomeWin | AwayWin | Draw
    type Bracket = CorrectScore | CorrectOutcome | Incorrect
    type ClosedFixtureStatus = AwaitingResult | ResultAdded
    type FixtureStatus = Open | ClosedFixtureStatus

    let isPlayerAdmin (player:Player) =
        match player.role with
        | Admin -> true
        | _ -> false


    //type GameWeekDetailsRow = { fixture:Fixture; prediction:Prediction option; result:Result option; points:int }
    //type FixtureDetails = { fixture:Fixture; result:Result; predictions:Prediction list }

    // filters
//    let findGameWeekById (gameWeeks:GameWeek list) id = gameWeeks |> List.find(fun gw -> gw.id = id)
//    let findFixtureById (fixtures:Fixture list) id = fixtures |> List.find(fun f -> f.id = id)
    let findPlayerById (players:Player list) id = players |> List.find(fun p -> p.id = id)
//    
//    let getPlayersPredictions (predictions:Prediction list) player = predictions |> List.filter(fun p -> p.player = player)
//    let getGameWeekPredictions (predictions:Prediction list) gameWeekNo = predictions |> List.filter(fun p -> p.fixture.gameWeek.number = gameWeekNo)
//    let getPlayerGameWeekPredictions (predictions:Prediction list) player gameWeekNo = getGameWeekPredictions (getPlayersPredictions predictions player) gameWeekNo
//    let getGameWeekResults (results:Result list) gameWeekNo = results |> List.filter(fun r -> r.fixture.gameWeek.number = gameWeekNo)


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

//    let getPointsForPrediction (prediction:Prediction) (results:Result list) =
//        let bracket = let result = results |> List.tryFind(fun r -> r.fixture = prediction.fixture)
//                      getBracketForPredictionComparedToResult (Some prediction) result
//        getPointsForBracket bracket
//
//    let getTotalPlayerScore (predictions:Prediction list) results player =
//        let predictionsForPlayer = predictions |> List.filter(fun p -> p.player = player)
//        let score = predictionsForPlayer |> List.sumBy(fun p -> getPointsForPrediction p results)
//        predictionsForPlayer.Length, player, score



    let tryFindPlayerPrediction (predictions:Prediction list) player = predictions |> List.tryFind(fun p -> p.player = player)
    
    let onlyClosedFixtures f =
        match f with
        | OpenFixture _ -> None
        | ClosedFixture fr -> Some fr
        
    let onlyOpenFixtures f =
        match f with
        | OpenFixture fd -> Some fd
        | ClosedFixture _ -> None

    let getPlayerBracketProfile (fixtures:Fixture list) player =
        let brackets = fixtures
                       |> List.choose(onlyClosedFixtures)
                       |> List.map(fun (fd, r) -> (tryFindPlayerPrediction fd.predictions player, r))
                       |> List.map(fun (p, r) -> getBracketForPredictionComparedToResult p r)
        let totalPoints = brackets |> List.sumBy(fun b -> getPointsForBracket b)
        let countBracket l bracket = l |> List.filter(fun b -> b = bracket) |> List.length
        let totalCorrectScores = CorrectScore |> (countBracket brackets)
        let totalCorrectOutcomes = CorrectOutcome |> (countBracket brackets)
        player, totalCorrectScores, totalCorrectOutcomes, totalPoints


    // entry points

    let getLeagueTable players fixtures =
        players
        |> List.map(fun p -> getPlayerBracketProfile fixtures p)
        |> List.sortBy(fun (_, _, _, totalPoints) -> -totalPoints)
        |> List.mapi(fun i (p, cs, co, tp) -> (i+1), p, cs, co, tp)

//    let getGameWeekPointsForPlayer (predictions:Prediction list) results player gameWeekNo =
//        let playerGameWeekPredictions = getPlayerGameWeekPredictions predictions player gameWeekNo
//        let points = playerGameWeekPredictions |> List.sumBy(fun p -> getPointsForPrediction p results)
//        player, gameWeekNo, points

    let getPlayerPointsForGameWeeks allPlayers player gameWeeks =
        gameWeeks
        |> List.map(fun gw -> gw, (getFixturesForGameWeeks [gw])) // (gw * (fixture list)) list
        |> List.map(fun (gw, fixtures) -> gw, getLeagueTable allPlayers fixtures) // (gw * (leaguetablerow list)) list
        |> List.map(fun (gwno, ltrList) -> gwno, ltrList |> List.tryFind(fun (_, p, _, _, _) -> p = player)) // (gw * leaguetablerow option) list

//    let getAllGameWeekPointsForPlayer (predictions:Prediction list) results player gameWeeks =
//        gameWeeks
//        |> List.map(fun gw -> getGameWeekPointsForPlayer predictions results player gw.number)
//        |> List.sortBy(fun (_, gwno, _) -> getGameWeekNo gwno)
        
    let getGameWeekDetailsForPlayer player gameWeek =
        getFixturesForGameWeeks [gameWeek]
        |> List.map(fixtureToFixtureDataWithResult)
        |> List.map(fun (fd, r) -> (fd, r, tryFindPlayerPrediction fd.predictions player))
        |> List.map(fun (fd, r, p) -> (fd, r, p, (getBracketForPredictionComparedToResult p r |> getPointsForBracket)))
    // (f, pr, r, pts) list

//    let getPlayerPredictionsForFixture (predictions:Prediction list) (results:Result list) fxid =
//        let fixture = (predictions |> List.find(fun p -> p.fixture.id = fxid)).fixture
//        let fixtureResult = results |> List.find(fun r -> r.fixture.id = fxid)
//        let fixturePredictions = predictions |> List.filter(fun p -> p.fixture.id = fxid)
//        { fixture=fixture; result = fixtureResult; predictions=fixturePredictions }
//
//    let isFixtureOpen f = f.kickoff > DateTime.Now

    let getOpenFixturesForPlayer (fixtures:Fixture list) (players:Player list) (plId:PlId) =
        let player = findPlayerById players plId
        // todo: filter for fixtures with no prediction
        fixtures |> List.choose(onlyOpenFixtures) |> List.sortBy(fun fd -> fd.kickoff)

        
//
//    let getFixturesAwaitingResults (fixtures:Fixture list) (results:Result list) =
//        let hasFilterGotMatchingResult f =
//            let res = results |> List.tryFind(fun r -> r.fixture = f)
//            match res with | Some r -> true | None -> false
//        fixtures
//        |> List.filter(fun f -> isFixtureOpen f = false)
//        |> List.filter(fun f -> hasFilterGotMatchingResult f = false)

//
//    let getGameWeekWinner (predictions:Prediction list) (results:Result list) (gameWeek:GameWeek) =
//        predictions
//        |> List.map(fun pr -> pr.player)
//        |> List.map(fun pl -> getGameWeekPointsForPlayer predictions results pl gameWeek.number)
//        |> List.maxBy(fun (_,_,points) -> points)

    let getPastGameWeeksWithWinner (gameWeeks:GameWeek list) players =
        gameWeeks
        |> List.map(fun gw -> gw, getFixturesForGameWeeks [gw])
        |> List.map(fun (gw, fixtures) -> gw, getLeagueTable players fixtures)
        |> List.map(fun (gw, lgtbl) -> gw, lgtbl.Head)
        |> List.map(fun (gw, (_, plr, _, _, pts)) -> gw, plr, pts)

//    let getPointsForFixtureForPlayer (predictions:Prediction list) (results:Result list) fixture player =
//        let prediction = predictions |> List.filter(fun p -> p.player = player) |> List.tryFind(fun p -> p.fixture = fixture)
//        let result = results |> List.tryFind(fun r -> r.fixture = fixture)
//        let points = getBracketForPredictionComparedToResult prediction result |> getPointsForBracket
//        (prediction, points)

//    let getPlayerPointsForFixture (players:Player list) (predictions:Prediction list) (results:Result list) (fixture:Fixture) =
//        players
//        |> Seq.map(fun pl -> let (prediction, points) = getPointsForFixtureForPlayer predictions results fixture pl
//                             (pl, prediction, points))
//        |> Seq.toList





    // Rules 
    
    // when adding gameweek:
        // cannot add fixture to gameweek with same home & away teams
        // cannot add fixture to gameweek with ko in past
    
    // when saving prediction/result:
        // cannot add score with negative scores

    // cannot save result to fixture with ko in future
    // cannot save prediction to fixture with ko in past
    // cannot view fixture with ko in future

    
    let createPrediction player score = { Prediction.id=newPrId; player=player; score=score }
    //let createFixture home away (ko:string) = { FixtureData.id=newFxId; home=home; away=away; kickoff=Convert.ToDateTime(ko); predictions=[] }

    let tryAddResultToFixture r f =
        match f with
        | OpenFixture f -> Failure "cannot add result to fixture with ko in future"
        | ClosedFixture (f, _) -> Success(ClosedFixture(f, Some r))

    let tryAddPredictionToFixture p f =
        match f with
        | OpenFixture f -> Success((p, OpenFixture({f with predictions=p::f.predictions})))
        | ClosedFixture _ -> Failure "cannot add prediction to fixture with ko in past"

    let tryViewFixture f =
        match f with
        | OpenFixture _ -> Failure "cannot view fixture with ko in future"
        | ClosedFixture(fd, r) -> Success(fd, r)

    let tryToCreateScoreFromSbm home away =
        if home >= 0 && away >= 0 then Success(home, away) else Failure "scores must be positive"

    let tryToCreateFixtureDataFromSbm home away (ko:string) =
        let (isKoValid, kickoff) = DateTime.TryParse(ko)
        if isKoValid=false then Failure "fixture kickoff time is invalid"
        else if kickoff > DateTime.Now then Failure "fixture kickoff cannot be in the future"
        else if home = away then Failure "fixture home and away team cannot be the same"
        else Success({id=newFxId; home=home; away=away; kickoff=kickoff; predictions=[]})
    