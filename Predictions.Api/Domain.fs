namespace Predictions.Api

open System

module Domain =

    type FxId = FxId of Guid
    type GwId = GwId of Guid
    type PlId = PlId of Guid
    type PrId = PrId of Guid
    type Team = string
    type KickOff = DateTime
    type GwNo = GwNo of int
    type Role = User | Admin

    let sToGuid s = Guid.Parse(s)
    let trySToGuid s = Guid.TryParse(s)

    // deconstructors
    let getPlayerId (PlId id) = id
    let getGameWeekNo (GwNo n) = n
    let getFxId (FxId id) = id
    let getGwId (GwId id) = id

    type Score = int * int
    type Player = { id:PlId; name:string; role:Role }
    type GameWeek = { id:GwId; number:GwNo; description:string }
    type Fixture = { id:FxId; gameWeek:GameWeek; home:Team; away:Team; kickoff:KickOff }
    type Prediction = { fixture:Fixture; score:Score; player:Player }
    type Result = { fixture:Fixture; score:Score }
    type Outcome = HomeWin | AwayWin | Draw
    type Bracket = CorrectScore | CorrectOutcome | Incorrect
    type GameWeekWinner = { gameWeek:GameWeek; player:Player; points:int }

    let isPlayerAdmin (player:Player) =
        match player.role with
        | Admin -> true
        | _ -> false

    // dtos / viewmodels
    type LeagueTableRow = { position:int; player:Player; correctScores:int; correctOutcomes:int; points:int }
    type GameWeekAndPoints = GwNo * int
    type GameWeekDetailsRow = { fixture:Fixture; prediction:Prediction option; result:Result; points:int }
    type FixtureDetails = { fixture:Fixture; result:Result; predictions:Prediction list }

    // filters
    let findGameWeekById (gameWeeks:GameWeek list) id = gameWeeks |> List.find(fun gw -> gw.id = id)
    let findFixtureById (fixtures:Fixture list) id = fixtures |> List.find(fun f -> f.id = id)
    let findPlayerById (players:Player list) id = players |> List.find(fun p -> p.id = id)
    
    let getPlayersPredictions (predictions:Prediction list) player = predictions |> List.filter(fun p -> p.player = player)
    let getGameWeekPredictions (predictions:Prediction list) gameWeekNo = predictions |> List.filter(fun p -> p.fixture.gameWeek.number = gameWeekNo)
    let getPlayerGameWeekPredictions (predictions:Prediction list) player gameWeekNo = getGameWeekPredictions (getPlayersPredictions predictions player) gameWeekNo
    let getGameWeekResults (results:Result list) gameWeekNo = results |> List.filter(fun r -> r.fixture.gameWeek.number = gameWeekNo)


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
        
    let getPointsForPrediction (prediction:Prediction) (results:Result list) =
        let bracket = let result = results |> List.tryFind(fun r -> r.fixture = prediction.fixture)
                      getBracketForPredictionComparedToResult (Some prediction) result
        getPointsForBracket bracket

    let getTotalPlayerScore (predictions:Prediction list) results player =
        let predictionsForPlayer = predictions |> List.filter(fun p -> p.player = player)
        let score = predictionsForPlayer |> List.sumBy(fun p -> getPointsForPrediction p results)
        predictionsForPlayer.Length, player, score



    let getBracketForPrediction (prediction:Prediction) (results:Result list) =
        let result = results |> List.tryFind(fun r -> r.fixture = prediction.fixture)
        getBracketForPredictionComparedToResult (Some prediction) result

    let getPlayerBracketProfile (predictions:Prediction list) results player =
        let countBracket l bracket = l |> List.filter(fun b -> b = bracket) |> List.length
        let playerPredictions = predictions |> List.filter(fun p -> p.player = player)
        let brackets = playerPredictions |> List.map(fun p -> getBracketForPrediction p results)
        let totalPoints = brackets |> List.sumBy(fun b -> getPointsForBracket b)
        let totalCorrectScores = CorrectScore |> (countBracket brackets)
        let totalCorrectOutcomes = CorrectOutcome |> (countBracket brackets)
        player, totalCorrectScores, totalCorrectOutcomes, totalPoints


    // entry points

    // to get player movement for player:
    // get league table rows for all predictions 
    // get league table rows for predictions excluding latest gameweek
    // diff position

    let getLeagueTable (predictions:Prediction list) results players =
        players
        |> List.map(fun p -> getPlayerBracketProfile predictions results p)
        |> List.sortBy(fun (_, _, _, totalPoints) -> -totalPoints)
        |> List.mapi(fun i (p, cs, co, tp) -> { position=(i+1); player=p; correctScores=cs; correctOutcomes=co; points=tp })

    let getGameWeekPointsForPlayer (predictions:Prediction list) results player gameWeekNo =
        let playerGameWeekPredictions = getPlayerGameWeekPredictions predictions player gameWeekNo
        let points = playerGameWeekPredictions |> List.sumBy(fun p -> getPointsForPrediction p results)
        player, gameWeekNo, points



    let getPlayerPointsForGameWeeks (predictions:Prediction list) results player gameWeeks =
        // need to get league table for each week to get players position
        //gw, pos, cs, co, pts
        ()

    let getAllGameWeekPointsForPlayer (predictions:Prediction list) results player gameWeeks =
        gameWeeks
        |> List.map(fun gw -> getGameWeekPointsForPlayer predictions results player gw.number)
        |> List.sortBy(fun (_, gwno, _) -> getGameWeekNo gwno)
        
    let getGameWeekDetailsForPlayer (predictions:Prediction list) results player gameWeekNo =
        let playerGameWeekPredictions = getPlayerGameWeekPredictions predictions player gameWeekNo
        let gameWeekResults = getGameWeekResults results gameWeekNo
        let getGameWeekDetailsRow (prediction:Prediction option) result =
            let points = getBracketForPredictionComparedToResult prediction (Some result) |> getPointsForBracket
            { GameWeekDetailsRow.fixture=result.fixture; prediction=prediction; result=result; points=points }
        gameWeekResults |> List.map(fun r -> let prediction = playerGameWeekPredictions |> List.tryFind(fun p -> r.fixture = p.fixture )
                                             getGameWeekDetailsRow prediction r)
    
    let getPlayerPredictionsForFixture (predictions:Prediction list) (results:Result list) fxid =
        let fixture = (predictions |> List.find(fun p -> p.fixture.id = fxid)).fixture
        let fixtureResult = results |> List.find(fun r -> r.fixture.id = fxid)
        let fixturePredictions = predictions |> List.filter(fun p -> p.fixture.id = fxid)
        { fixture=fixture; result = fixtureResult; predictions=fixturePredictions }

    let isFixtureOpen f = f.kickoff > DateTime.Now

    let getOpenFixturesForPlayer (predictions:Prediction list) (fixtures:Fixture list) (players:Player list) (plId:PlId) =
        let player = findPlayerById players plId
        let doesFixtureAlreadyHavePredictionFromPlayer (predictions:Prediction list) player fixture =
            let predictionsForFixtureByPlayer =
                predictions |> List.filter(fun p -> p.player = player) |> List.filter(fun p -> p.fixture = fixture)
            (List.isEmpty predictionsForFixtureByPlayer) = false
        fixtures
        |> List.filter(isFixtureOpen)
        |> List.filter(fun f -> (doesFixtureAlreadyHavePredictionFromPlayer predictions player f) = false)
        |> List.sortBy(fun f -> f.gameWeek.number)

    let getFixturesAwaitingResults (fixtures:Fixture list) (results:Result list) =
        let hasFilterGotMatchingResult f =
            let res = results |> List.tryFind(fun r -> r.fixture = f)
            match res with | Some r -> true | None -> false
        fixtures
        |> List.filter(fun f -> isFixtureOpen f = false)
        |> List.filter(fun f -> hasFilterGotMatchingResult f = false)


    let getGameWeekWinner (predictions:Prediction list) (results:Result list) (gameWeek:GameWeek) =
        predictions
        |> List.map(fun pr -> pr.player)
        |> List.map(fun pl -> getGameWeekPointsForPlayer predictions results pl gameWeek.number)
        |> List.maxBy(fun (_,_,points) -> points)

    let getPastGameWeeks (predictions:Prediction list) (results:Result list) =
        results
        |> List.map(fun r -> r.fixture.gameWeek)
        |> Seq.distinct
        |> Seq.map(getGameWeekWinner predictions results)
        |> Seq.sortBy(fun (_, gwno, _) -> gwno)
        |> Seq.toList
//
//    let getGameWeekPoints (predictions:Prediction list) (results:Result list) (gwno:GwNo) =
//        predictions
//        |> List.map(fun pr -> pr.player)
//        |> Seq.distinct
//        |> Seq.map(fun pl -> getGameWeekPointsForPlayer predictions results pl gwno)
//        |> Seq.sortBy(fun (_, _, points) -> -points)
//        |> Seq.toList

    let getPointsForFixtureForPlayer (predictions:Prediction list) (results:Result list) fixture player =
        let prediction = predictions |> List.filter(fun p -> p.player = player) |> List.tryFind(fun p -> p.fixture = fixture)
        let result = results |> List.tryFind(fun r -> r.fixture = fixture)
        let points = getBracketForPredictionComparedToResult prediction result |> getPointsForBracket
        (prediction, points)

    let getPlayerPointsForFixture (players:Player list) (predictions:Prediction list) (results:Result list) (fixture:Fixture) =
        players
        |> Seq.map(fun pl -> let (prediction, points) = getPointsForFixtureForPlayer predictions results fixture pl
                             (pl, prediction, points))
        |> Seq.toList


    let checkFixtureIsInFuture f = if isFixtureOpen f then Success f else Failure "Fixture must be in the future"
    let checkFixtureTeamsAreUnique f = if f.home = f.away then Success f else Failure "Teams cannot play themselves"
    
    let validateFixture f = f |> (checkFixtureIsInFuture >> bind checkFixtureTeamsAreUnique)