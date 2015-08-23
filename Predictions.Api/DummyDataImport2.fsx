#r @"C:\Users\Nick\lab\Predictions\Predictions.Api\bin\Debug\Predictions.Api.dll"
open System
open System.IO
open System.Data
open Predictions.Api.Common
open Predictions.Api.Domain
open Predictions.Api.Data
#load @"DummyNames.fs"
open DummynamesModule

#r @"C:\Users\Nick\lab\Predictions\packages\FSharp.Data.2.2.3\lib\net40\FSharp.Data.dll"
open FSharp.Data



let rnd = new System.Random()
let ko = new DateTime(2014,10,2);
let seasonId = "2C3B0ED1-C4A8-4D99-B0F5-5AC7312EB7C9" |> sToGuid |> SnId
let teamsList = [ "Arsenal"; "Chelsea"; "Liverpool"; "Everton"; "WestHam"; "Qpr"; "Man Utd"; "Man City"; "Newcastle"; "Sunderland"; 
                    "Stoke"; "Leicester"; "Spurs"; "Aston Villa"; "West Brom"; "Crystal Palace"; "Hull"; "Burnley"; "Southampton"; "Swansea" ]

let getTwoDifferentRndTeams (teams:string list) =
    let getRandomTeamIndex() = rnd.Next(0, teams.Length - 1)
    let homeTeamIndex = getRandomTeamIndex()
    let rec getTeamIndexThatIsnt notThis =
        let index = getRandomTeamIndex()
        match index with
        | i when index=notThis -> getTeamIndexThatIsnt notThis
        | _ -> index
    let awayTeamIndex = getTeamIndexThatIsnt homeTeamIndex
    (teams.[homeTeamIndex], teams.[awayTeamIndex])


//let getSaveGameWeekCommandList() =
//    let generateSaveFixtureList() =
//        [ for i in 1..10 ->
//            let randomTeams = getTwoDifferentRndTeams teamsList
//            { SaveFixtureCommand.id=newFxId(); home=fst randomTeams; away=snd randomTeams; kickoff=ko } ]
//    [ for i in 1..6 -> { SaveGameWeekCommand.id=newGwId(); seasonId=seasonId; description=""; saveFixtureCommands=generateSaveFixtureList() } ]

let getRndScore() =
    let getRndGoals() = rnd.Next(0, 4)
    getRndGoals(), getRndGoals()

//let getSaveResultCommandList (gwCmds:SaveGameWeekCommand list) =
//    gwCmds
//    |> List.collect(fun cmd -> cmd.saveFixtureCommands)
//    |> List.map(fun f -> { SaveResultCommand.fixtureId=f.id; score=getRndScore() })



// save gameweeks & results

//let gwcmds = getSaveGameWeekCommandList();;
//gwcmds |> List.iter saveGameWeek;;
//let rescmds = gwcmds |> getSaveResultCommandList;;

let rescmds = 
    (buildSeason currentSeason).gameWeeks
    |> List.collect(fun gw -> gw.fixtures)
    |> List.map fixtureToFixtureData
    |> List.map(fun f -> { SaveResultCommand.fixtureId=f.id; score=getRndScore() })

rescmds |> List.iter saveResult;;


//let playersList = [ for p in [ "Adam"; "Antony"; "Blair"; "Dan"; "Dave"; "Lewis"; "Jones"; "Pete"; "Walsh"; "Woolley";  ] ->
let lotsofDummyNames = (dummynames)
let playersList = [ for p in lotsofDummyNames ->
                    { Player.id=newPlId(); name=p|>PlayerName; predictions=[]; isAdmin=false } ]
let getRandomExpId() = Guid.NewGuid() |> str |> ExternalPlayerId

let getRegisterPlayerCommands() =
    playersList |> Seq.truncate 200 |> Seq.toList
    |> List.map(fun p -> { RegisterPlayerCommand.player=p; explid=getRandomExpId(); exProvider="Facebook"|>ExternalLoginProvider; email="" })
    
let rpcmds = getRegisterPlayerCommands()
rpcmds |> List.iter registerPlayerInDb

//let playerIds = rpcmds |> List.map(fun p -> p.player.id)
let playerIds = getAllPlayers() |> List.map(fun p -> p.id)
let fixtureIds =
    (buildSeason currentSeason).gameWeeks
    |> List.collect(fun gw -> gw.fixtures)
    |> List.map fixtureToFixtureData
    |> List.map(fun f -> f.id)

let getSavePredictionCommandList (pids) (fids) =
    pids
    |> List.map(fun pid -> 
                    fids
                    |> List.map(fun fid ->
                    { SavePredictionCommand.id=newPrId(); playerId=pid; fixtureId=fid; score=getRndScore() }))
    |> List.collect(fun cmd -> cmd)

let prcmds = getSavePredictionCommandList playerIds fixtureIds
prcmds |> List.iter savePrediction

let getJoinLeagueCommands() =
    let lgid = "350e4e39-a303-44f3-a804-8b57d20fa459" |> sToGuid |> LgId
    playerIds |> Seq.truncate 95 |> Seq.toList
    |> List.map(fun pid -> { JoinLeagueCommand.playerId=pid; leagueId=lgid })

let jlcmds = getJoinLeagueCommands()
jlcmds |> List.iter joinLeagueInDb


