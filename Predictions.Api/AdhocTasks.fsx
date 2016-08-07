#r """/Users/nblair/lab/Predictions/packages/FSharp.Data/lib/net40/FSharp.Data.dll"""
#r """C:\Code\Predictions\packages\Npgsql.2.2.1\lib\net40\Npgsql.dll"""
#r """C:\Code\Predictions\Predictions.Api\bin\Debug\Predictions.Api.dll"""
open System
open System.IO
open System.Data
open System.Collections.Generic
open FSharp.Data
open Npgsql
open Predictions.Api.Common
open Predictions.Api.Domain
open Predictions.Api.Data
//
//let flipPlayerNames() =
//    let getFlippedName (player:PlayerDto) =
//        let arr = player.name.Split(' ')
//        arr.[1] + " " + arr.[0]
//    let flipPlayersName player =
//        let flippedName = getFlippedName player
//        let updateQuery = sprintf "update players set name = '%s' where id = '%s'" flippedName (player.id|>str)
//        executeNonQuery updateQuery
//    readPlayers() |> List.iter(fun p -> flipPlayersName p)

(*
Burnley 2-1
Man u 2-0
Newcastle 2-0
Stoke 2-0
Southampton 2-0
*)
//let dunphyId = "ebd3cd36-db54-42b2-a354-7d5743b21082"
//let biggsId = "5ac435de-2c70-4a49-ab41-a02a0657b0d0"
let antId = "ebe847df-994f-4f8b-b61f-c82b8afa94ef"

let antPredictions =
    [(antId,"eaf8cb61-7b35-4aef-bb20-76ab74e5dda9","2-0")]

let getScoreFromString (s:string) =
    let i (sd:string) = Convert.ToInt32(sd)
    let arr = s.Split('-')
    i arr.[0], i arr.[1]

let fixts = readFixtures()

let printFixtureAndScore (c:SavePredictionCommand) =
    let fixture = fixts |> List.find(fun f -> FxId f.id = c.fixtureId)
    printfn "|%20s|%-20s|%i - %i|" fixture.home fixture.away (fst c.score) (snd c.score)

let tupleToSavePreditionCmd (plid, fxid, scoreString) =
    { SavePredictionCommand.id=newPrId(); fixtureId=fxid|>sToGuid|>FxId; playerId=plid|>sToGuid|>PlId; score=scoreString|>getScoreFromString }

let printPreds prds = prds |> List.map(tupleToSavePreditionCmd) |> List.iter(printFixtureAndScore)
let savePreds prds = prds |> List.map(tupleToSavePreditionCmd) |> List.iter(savePrediction)


// update gw 17 games to gw 16

//let readgw16Fixtures() = (executeQuery "select * from fixtures where gameweekid = 'b1b5d8cb-fc27-4587-b524-ff933dd00f17'" readerToFixtureDto)
//let readgw17Fixtures() = (executeQuery "select * from fixtures where gameweekid = 'a297c75f-bd4d-48ed-b82b-548ac53c087a'" readerToFixtureDto)
//
//let u = "update fixtures set gameweekid = 'b1b5d8cb-fc27-4587-b524-ff933dd00f17' where gameweekid = 'a297c75f-bd4d-48ed-b82b-548ac53c087a'"
//let m = "update players set name = 'Michael Mansfield' where name = 'Michael Manfield'"


// position grouping

let np =
    [ ("bob", (4, 2)) 
      ("jon", (4, 8))
      ("ert", (6, 2))
      ("tyu", (4, 2))
      ("uio", (4, 1))
      ("tom", (4, 5)) 
      ("dfg", (4, 6))
      ("fgh", (4, 7))
      ("jim", (2, 3)) 
      ("rob", (4, 5)) 
      ("ron", (4, 3)) ]

let pr a = a |> Seq.iter(fun a -> printfn "%A" a)

let rec bumprank sumdelta acc a =
    match a with
    | [] -> List.rev acc
    | h::t -> let (i, g) = h
              let newi = i + sumdelta
              let newsumdelta = sumdelta + ((Seq.length g)-1)
              let newacc = (newi, g)::acc
              bumprank newsumdelta newacc t

let rank a =
    a
    |> Seq.sortBy(fun (_, (s, p)) -> -p, -s)
    |> Seq.groupBy(fun (_, x) -> x)
    |> Seq.mapi(fun i (_, g) -> i+1, g)
    |> Seq.toList
    |> bumprank 0 []
    |> Seq.collect(fun (i, g) -> g |> Seq.map(fun x -> i, x))
    |> pr



// gw21 extra fixtures

let fxs21 =
    [("West Ham", "Hull",      "2015-01-18 13:30:00")
     ("Man City", "Arsenal",   "2015-01-18 16:00:00")
     ("Everton",  "West Brom", "2015-01-19 20:00:00")]

let gwid21 = sToGuid "b65012d6-2726-4c26-98de-e042be508da0" |> GwId

// gw28 extra fixtures

let fxs28 =
    [("Southampton","Burnley", "2015-03-21 15:00:00")
     ("Stoke","Crystal Palace", "2015-03-21 15:00:00")
     ("Tottenham","Leicester", "2015-03-21 15:00:00")
     ("West Ham","Sunderland", "2015-03-21 17:30:00")
     ("Liverpool","Man Utd", "2015-03-22 13:30:00")
     ("Hull","Chelsea", "2015-03-22 16:00:00")
     ("QPR","Everton", "2015-03-22 16:00:00")]
     
let gwid28 = sToGuid "2787a537-6c5e-4d96-9b5f-7e6e5afe56ff" |> GwId


// save fixtures 

let getSaveFixtureCommands gwid fixtures =
    let getSaveFixtureCommand gwid (h, a, t:string) =
        {
            SaveFixtureCommand.id=newFxId()
            SaveFixtureCommand.home = h
            SaveFixtureCommand.away = a
            SaveFixtureCommand.ko = Convert.ToDateTime(t)
            SaveFixtureCommand.gameWeekId = gwid
        }
    fixtures |> List.map(fun f -> getSaveFixtureCommand gwid f)

let savecmds thecmds = 
    thecmds |> List.iter(fun fd -> fd |> getFixtureDto |> getInsertFixtureQuery |> executeNonQuery)




/// accuracy index

let i = [ ( (2, 2), (1, 1) ) ]
let o = [ (2, 1); (2, 1) ]

let nrnd = new Random()
let rdm() = nrnd.Next(0, 8)

let setOfScores m = [ for i in 1..m -> (rdm(), rdm())]

let someResults = 20 |> setOfScores
let somePrdctns = 20 |> setOfScores

let accInd prds rlts =
//    let prds = 20 |> setOfScores
//    let rlts = 20 |> setOfScores
    let inline tod (d:int) = Convert.ToDecimal(d)
    List.zip prds rlts
    |> List.collect(fun ((ph, pa), (rh, ra)) -> [ (ph, rh); (pa, ra) ])
    |> List.map(fun ((p:int), (r:int)) -> (p - r) |> abs |> tod)
    |> List.average

let ssn = buildSeason currentSeason
let gws = ssn.gameWeeks // |> List.filter(fun gw -> (getGameWeekNo gw.number) = 9)
let pls = getPlayers()

let stddev (input : float list) =
    let sampleSize = float input.Length
    let mean = input |> List.average
    let differenceOfSquares =
        input |> List.fold(fun sum item -> sum + Math.Pow(item - mean, 2.0)) 0.0
    let variance = differenceOfSquares / sampleSize
    Math.Sqrt(variance)

let round2 (d:float) = Math.Round(d, 3)

let getDiffListForPlayer fdrs (player:Player)=
    let inline whereBothAreSome ((o1:'a option), (o2:'b option)) = o1.IsSome && o2.IsSome
    let inline toDoubleTup ((po:Prediction option), (ro:Result option)) = po.Value.score, ro.Value.score
    let inline tof (d:int) = Convert.ToDouble(d)
    let correctResultOnly (fixd, reslt) =
        match getBracketForPredictionComparedToResult fixd reslt with
        | CorrectScore -> true
        | CorrectOutcome -> true
        | Incorrect -> false
    fdrs
    |> List.map(fun (fd, r) -> (tryFindPlayerPrediction fd.predictions player, r))
    |> List.filter(whereBothAreSome)
    |> List.filter(correctResultOnly)
    |> List.map(toDoubleTup)
    |> List.collect(fun ((ph, pa), (rh, ra)) -> [ (ph, rh); (pa, ra) ])
    |> List.map(fun ((p:int), (r:int)) -> (p - r) |> abs |> tof)


let getAccuracyIndexForPlayers (gws:GameWeek list) (players:Player list) =
    let inline writenice (n, _, sd, i) = printfn "%20s %.3f (%i)" n sd i
    let fdrs = gws
               |> getFixturesForGameWeeks
               |> List.choose(onlyClosedFixtures)
               |> List.map(fixtureToFixtureDataWithResult)
    players
    |> List.map(fun p -> p.name, getDiffListForPlayer fdrs p)
    |> List.map(fun (n, dl) -> (n, (dl |> List.average |> round2), dl |> stddev |> round2, dl.Length/2))
    |> List.sortBy(fun (_, _, i, _) -> i)
    |> List.iter(writenice)
    //|> List.iter(fun x -> printfn "%A" x)


// update ko to 15:00

//let updateto1500 = "update fixtures set kickoff = '2015-01-01 15:00:00' where kickoff = '2015-01-01 13:00:00'"
let updateto1500 = "update fixtures set kickoff = '2015-01-01 15:00:00' where kickoff = '2015-01-01 13:00:00'"

// correct daylight saving fixtures
let correctdls = "update fixtures set kickoff = kickoff + '1 hour' where kickoff > '2015-04-01 00:00:00'"

// form guide

type TeamOutcome = Win | Lose | Draw

let isTeamInFixture (fd:FixtureData) team = fd.home = team || fd.away = team
let getResultForTeam (fd:FixtureData, result:Result) team =
    let isHomeTeam = fd.home = team
    let outcome = getResultOutcome result.score
    match outcome with
    | Outcome.Draw -> TeamOutcome.Draw
    | HomeWin -> if isHomeTeam then Win else Lose
    | AwayWin -> if isHomeTeam then Lose else Win

let getLastResultsForTeam fixtures team =
    fixtures
    |> List.choose(onlyClosedFixtures)
    |> List.map(fixtureToFixtureDataWithResult)
    |> List.filter(fun (_, r) -> r.IsSome)
    |> List.map(fun (fd, r) -> fd, r.Value)
    |> List.sortBy(fun (fd, _) -> fd.kickoff) |> List.rev
    |> List.filter(fun (fd, _) -> isTeamInFixture fd team)
    |> Seq.truncate 6
    |> Seq.map(fun fdr -> getResultForTeam fdr team)
    |> Seq.toList

let fixturesToUpdate = [
    ( "19:45","4558501f-84e9-415c-b565-6f72d0e885a9" )
    ( "19:45","0a76779e-1670-407e-ad69-c171de35c490" )
    ( "19:45","9d17bff7-b256-4d5e-b0c8-d4bc06412343" )
    ( "19:45","64a3717b-04cd-4e23-86dd-a34088943b2f" )
    ( "20:00","65f13cf8-4190-4627-8530-9f7aa55dddc6" )
    ( "20:00","403e9ca2-035b-43b0-b3cb-774d7b32607c" )]

let kofix (time, fxid) = sprintf "update fixtures set kickoff = '2015-02-11 %s:00' where id = '%s'" time fxid

