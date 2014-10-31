#r """C:\Code\Predictions\packages\Npgsql.2.2.1\lib\net40\Npgsql.dll"""
#r """C:\Code\Predictions\Predictions.Api\bin\Debug\Predictions.Api.dll"""
open System
open System.IO
open System.Data
open Npgsql
open Predictions.Api.Common
open Predictions.Api.Domain
open Predictions.Api.Data

let rnd = new System.Random()

let seasonId = newSnId()
let seasonYear = SnYr "2014/15"

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

let ko = new DateTime(2014,10,2);

let generateFixtureDataList() = [ for i in 1..10 -> let randomTeams = getTwoDifferentRndTeams teamsList
                                                    { FixtureData.id=newFxId(); home=fst randomTeams; away=snd randomTeams; kickoff=ko; predictions=[] } ]


let getSaveGameWeekCommandList() = [ for i in 1..6 -> { SaveGameWeekCommand.id=newGwId(); number=(GwNo i); seasonId=seasonId; description=""; fixtures=generateFixtureDataList() } ]

let saveSeasonCommand = { SaveSeasonCommand.id=seasonId; year=seasonYear; }

let getRndScore() =
    let getRndGoals() = rnd.Next(0, 4)
    getRndGoals(), getRndGoals()

let getSaveResultCommandList (gwCmds:SaveGameWeekCommand list) =
    gwCmds
    |> List.collect(fun cmd -> cmd.fixtures)
    |> List.map(fun f -> { SaveResultCommand.id=newRsId(); fixtureId=f.id; score=getRndScore() })
    
let getSavePredictionCommandList (gwCmds:SaveGameWeekCommand list) (plCmds:SavePlayerCommand list) =
    gwCmds
    |> List.collect(fun cmd -> cmd.fixtures)
    |> List.map(fun f -> plCmds |> List.map(fun p -> { SavePredictionCommand.id=newPrId(); fixtureId=f.id; playerId=p.id; score=getRndScore() }))
    |> List.collect(fun p -> p)

let playersList = readPlayers()
//let playersList = [ for p in [ "Adam"; "Antony"; "Blair"; "Dan"; "Dave"; "Lewis"; "Jones"; "Pete"; "Walsh"; "Woolley";  ] -> { PlayerDto.id=nguid(); name=p; role="Admin"; email="" } ]

let getSavePlayerCommands (playerDtos:PlayerDto list) =
    playerDtos |> List.map(fun p -> { SavePlayerCommand.id=PlId p.id; name=p.name; role=Admin; email="" })

// initialising

let initAll (plrs:SavePlayerCommand list) (sn:SaveSeasonCommand) (gws:SaveGameWeekCommand list) (rs:SaveResultCommand list) (prs:SavePredictionCommand list) =
    executeNonQuery "drop table if exists players; create table players (id uuid, name text, role text, email text)"
    executeNonQuery "drop table if exists seasons; create table seasons (id uuid, year text)"
    executeNonQuery "drop table if exists gameweeks; create table gameweeks (id uuid, seasonId uuid, number int, description text)"
    executeNonQuery "drop table if exists fixtures; create table fixtures (id uuid, gameWeekid uuid, home text, away text, kickoff timestamp)"
    executeNonQuery "drop table if exists results; create table results (id uuid, fixtureId uuid, homeScore int, awayScore int)"
    executeNonQuery "drop table if exists predictions; create table predictions (id uuid, fixtureId uuid, homeScore int, awayScore int, playerId uuid)" 
    saveSeason sn
    plrs |> List.iter(savePlayer)
    gws |> List.iter(saveGameWeek)
    rs |> List.iter(saveResult)
    prs |> List.iter(savePrediction)

let players = getSavePlayerCommands playersList
let gameWeeks = getSaveGameWeekCommandList()
let predictions = getSavePredictionCommandList gameWeeks players
let results = getSaveResultCommandList gameWeeks
        
initAll players saveSeasonCommand gameWeeks results predictions

let pidtos pid = pid |> getPlayerId |> str
let localUrl = "http://localhost:49782/api/auth/"
let liveUrl = "http://predictions.apphb.com/api/auth/"
let publishPlayers url =
    players
    |> List.map(fun p -> (p.name, sprintf "%s%s" url (pidtos p.id)))
    |> List.sortBy(fun (p, _) -> p)
    |> List.iter(fun (name, url) -> printf "%s %s %s" name url Environment.NewLine)

