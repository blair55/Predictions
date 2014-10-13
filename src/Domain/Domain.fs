namespace PredictionsManager.Domain

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
    type GameWeekWinner = { gameWeek:GameWeek; player:Player; points:int }

    let isPlayerAdmin (player:Player) =
        match player.role with
        | Admin -> true
        | _ -> false

    // dtos / viewmodels
    type LeagueTableRow = { position:int; predictions:int; player:Player; points:int }
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

    let getAllGameWeeks (results:Result list) =
        results
        |> List.map(fun r -> r.fixture.gameWeek)
        |> Seq.distinctBy(fun gw -> gw)
        |> Seq.toList

    // base calculations
    let getOutcome score =
        if fst score > snd score then HomeWin
        else if fst score < snd score then AwayWin
        else Draw

    let getPointsForPredictionComparedToResult (prediction:Prediction) result =
        if prediction.score = result.score then 3
        else
            let predictionOutcome = getOutcome prediction.score
            let resultOutcome = getOutcome result.score
            if predictionOutcome = resultOutcome then 1 else 0
        
    let getPointsForPrediction (prediction:Prediction) (results:Result list) =
        let result = results |> List.tryFind(fun r -> r.fixture = prediction.fixture)
        match result with
        | Some r -> getPointsForPredictionComparedToResult prediction r
        | None -> 0

    let getTotalPlayerScore (predictions:Prediction list) results player =
            let predictionsForPlayer = predictions |> List.filter(fun p -> p.player = player)
            let score = predictionsForPlayer |> List.sumBy(fun p -> getPointsForPrediction p results)
            predictionsForPlayer.Length, player, score

    // entry points
    let getAllPlayerScores (predictions:Prediction list) results =
        let getTotalPlayerScorePartialFunc = getTotalPlayerScore predictions results
        let allPlayers = predictions |> List.map(fun p -> p.player) |> Seq.distinctBy(fun p -> p) |> Seq.toList
        allPlayers |> List.map(getTotalPlayerScorePartialFunc)

    let getLeagueTable (predictions:Prediction list) results =
        let playerScores = getAllPlayerScores predictions results
        playerScores
        |> List.sortBy(fun (_, _, score) -> -score)
        |> List.mapi(fun i (predictions, player, score) -> { position=(i+1); predictions=predictions; player=player; points=score })

    let getGameWeekPointsForPlayer (predictions:Prediction list) results player gameWeekNo =
        let playerGameWeekPredictions = getPlayerGameWeekPredictions predictions player gameWeekNo
        let points = playerGameWeekPredictions |> List.sumBy(fun p -> getPointsForPrediction p results)
        player, gameWeekNo, points

    let getAllGameWeekPointsForPlayer (predictions:Prediction list) results player =
        (getAllGameWeeks results)
        |> List.map(fun gw -> getGameWeekPointsForPlayer predictions results player gw.number)
        |> List.sortBy(fun (_, gwno, _) -> getGameWeekNo gwno)
        
    let getGameWeekDetailsForPlayer (predictions:Prediction list) results player gameWeekNo =
        let playerGameWeekPredictions = getPlayerGameWeekPredictions predictions player gameWeekNo
        let gameWeekResults = getGameWeekResults results gameWeekNo
        let getGameWeekDetailsRow (prediction:Prediction option) result =
            let points =
                match prediction with
                | Some p -> getPointsForPredictionComparedToResult p result
                | None -> 0
            { GameWeekDetailsRow.fixture=result.fixture; prediction=prediction; result=result; points=points }
        gameWeekResults
        |> List.map(fun r -> let prediction = playerGameWeekPredictions |> List.tryFind(fun p -> r.fixture = p.fixture )
                             getGameWeekDetailsRow prediction r)
    
    let getPlayerPredictionsForFixture (predictions:Prediction list) (results:Result list) fxid =
        let fixture = (predictions |> List.find(fun p -> p.fixture.id = fxid)).fixture
        let fixtureResult = results |> List.find(fun r -> r.fixture.id = fxid)
        let fixturePredictions = predictions |> List.filter(fun p -> p.fixture.id = fxid)
        { fixture=fixture; result = fixtureResult; predictions=fixturePredictions }

    let isFixtureOpen f =
        let d = DateTime.Now
        let b = f.kickoff > d
        b

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

    let getGameWeekPoints (predictions:Prediction list) (results:Result list) (gwno:GwNo) =
        predictions
        |> List.map(fun pr -> pr.player)
        |> Seq.distinct
        |> Seq.map(fun pl -> getGameWeekPointsForPlayer predictions results pl gwno)
        |> Seq.sortBy(fun (_, _, points) -> -points)
        |> Seq.toList

    let getPointsForFixtureForPlayer (predictions:Prediction list) (results:Result list) fixture player =
        let prediction = predictions |> List.filter(fun p -> p.player = player) |> List.find(fun p -> p.fixture = fixture)
        let result = results |> List.find(fun r -> r.fixture = fixture)
        let points = getPointsForPredictionComparedToResult prediction result
        (prediction, points)

    let getPlayerPointsForFixture (players:Player list) (predictions:Prediction list) (results:Result list) (fixture:Fixture) =
        players
        |> Seq.map(fun pl -> let (prediction, points) = getPointsForFixtureForPlayer predictions results fixture pl
                             (pl, prediction, points))
        |> Seq.toList