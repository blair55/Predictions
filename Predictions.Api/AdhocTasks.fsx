#r """C:\code\Predictions\packages\FSharp.Data.2.0.15\lib\net40\FSharp.Data.dll"""
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

let dunphyId = "ebd3cd36-db54-42b2-a354-7d5743b21082"
let biggsId = "5ac435de-2c70-4a49-ab41-a02a0657b0d0"

let biggsPredictions =
    [(biggsId,"6c459519-45bd-4be3-8a86-a853edc14633","3-0")
     (biggsId,"9c6fa2fe-3368-41aa-ac5b-b6d1508f8d52","0-4")
     (biggsId,"d1af616e-4b79-49c1-a6eb-823de693373c","2-2")
     (biggsId,"880b4c49-c8f6-4b6e-a007-223be912cbf5","1-0")
     (biggsId,"6dfbc77f-2405-4a0b-a959-19eeb61bbb11","2-0")
     (biggsId,"7744dc41-92e6-43d3-a2f9-b1284425eda7","1-2")
     (biggsId,"3d8ef3d5-e8cb-440d-9ac1-bc388ec09179","2-1")
     (biggsId,"f0bac85b-339d-4fbd-a8c2-1a026157edf6","1-1")
     (biggsId,"c52c58be-e7d4-4a1f-9a80-8ef22122dcf7","1-3")
     (biggsId,"a69bb1d0-d64d-4999-8f43-3bec207a32bc","2-1")]

let getScoreFromString (s:string) =
    let i (sd:string) = Convert.ToInt32(sd)
    let arr = s.Split('-')
    i arr.[0], i arr.[1]

let fixts = readFixtures()

let printFixtureAndScore (c:SavePredictionCommand) =
    let fixture = fixts |> List.find(fun f -> FxId f.id = c.fixtureId)
    printfn "|%20s|%-20s|%i - %i|" fixture.home fixture.away (fst c.score) (snd c.score)

let saveBiggsPreds() =
    biggsPredictions
    |> List.map(fun (plid, fxid, scoreString) -> { SavePredictionCommand.id=newPrId(); fixtureId=fxid|>sToGuid|>FxId; playerId=plid|>sToGuid|>PlId; score=scoreString|>getScoreFromString })
    //|> List.iter(fun c -> printfn "%A" c)
    //|> List.iter(printFixtureAndScore)
    |> List.iter(savePrediction)


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


/// gameweek 17 data

let fxs =
    [("Chelsea", "West Ham", "12:45")
     ("Burnley", "Liverpool", "15:00")
     ("Crystal Palace", "Southampton", "15:00")
     ("Everton", "Stoke", "15:00")
     ("Leicester", "Tottenham", "15:00")
     ("Man Utd", "Newcastle", "15:00")
     ("Sunderland", "Hull", "15:00")
     ("Swansea", "Aston Villa", "15:00")
     ("West Brom", "Man City", "15:00")
     ("Arsenal", "QPR", "17:30")]
     
let getCmd (h, a, t) =
    {
        SaveFixtureCommand.id=newFxId()
        SaveFixtureCommand.home = h
        SaveFixtureCommand.away = a
        SaveFixtureCommand.ko = Convert.ToDateTime("2014-12-26 " + t + ":00")
        SaveFixtureCommand.gameWeekId = Guid.Parse("a297c75f-bd4d-48ed-b82b-548ac53c087a") |> GwId
    }

let getCmds f = f |> List.map getCmd


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
let gws = ssn.gameWeeks |> List.filter(fun gw -> (getGameWeekNo gw.number) = 9)
let pls = getPlayers()

let getAccuracyIndexForPlayer fdrs (player:Player)=
    let inline whereBothAreSome ((o1:'a option), (o2:'b option)) = o1.IsSome && o2.IsSome
    let inline toDoubleTup ((po:Prediction option), (ro:Result option)) = po.Value.score, ro.Value.score
    let inline tod (d:int) = Convert.ToDecimal(d)
    let inline round2 (d:decimal) = Math.Round(d, 2)
    fdrs
    |> List.map(fun (fd, r) -> (tryFindPlayerPrediction fd.predictions player, r))
    |> List.filter(whereBothAreSome)
    |> List.map(toDoubleTup)
    |> List.collect(fun ((ph, pa), (rh, ra)) -> [ (ph, rh); (pa, ra) ])
    |> List.map(fun ((p:int), (r:int)) -> (p - r) |> abs |> tod)
    |> List.average |> round2

let getAccuracyIndexForPlayers (gws:GameWeek list) (players:Player list) =
    let fdrs = gws
               |> getFixturesForGameWeeks
               |> List.choose(onlyClosedFixtures)
               |> List.map(fixtureToFixtureDataWithResult)
    players
    |> List.map(fun p -> p.name, getAccuracyIndexForPlayer fdrs p)
    |> List.sortBy(fun (_, i) -> i)
    |> List.iter(fun x -> printfn "%A" x)


// update ko to 15:00

let updateto1500 = "update fixtures set kickoff = '2015-01-01 15:00:00' where kickoff = '2015-01-01 13:00:00'"

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
    
