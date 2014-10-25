#r """C:\Code\Predictions\packages\Npgsql.2.2.1\lib\net40\Npgsql.dll"""
#r """C:\Code\Predictions\Predictions.Api\bin\Debug\Predictions.Api.dll"""
open System
open System.IO
open System.Data
open Npgsql
open Predictions.Api.Domain
open Predictions.Api.Data

let rnd = new System.Random()
let teamsList = [ "Arsenal"; "Chelsea"; "Liverpool"; "Everton"; "WestHam"; "Qpr"; "Man Utd"; "Man City"; "Newcastle"; "Sunderland"; 
                    "Stoke"; "Leicester"; "Spurs"; "Aston Villa"; "West Brom"; "Crystal Palace"; "Hull"; "Burnley"; "Southampton"; "Swansea" ]
let playersList = readPlayers()
//let playersList = [ for p in [ "Adam"; "Anthony"; "Blair"; "Dan"; "Dave"; "Lewis"; "Jones"; "Pete"; "Walsh"; "Woolley";  ] -> { Player.id=(Guid.NewGuid()|>PlId); name=p; role=Admin } ]
let gameWeeksList = [ for i in 1..6 -> { GameWeek.id=Guid.NewGuid()|>GwId; number=(GwNo i); description="" } ]

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

let buildFixtureList (teams:string list) gameWeeks =    
    let fixturesPerWeek = teams.Length / 2;
    let buildFixturesForGameWeek teams gw =
        [ for i in 1..fixturesPerWeek -> (  let randomTeams = getTwoDifferentRndTeams teams
                                            { Fixture.id= Guid.NewGuid()|>FxId; gameWeek=gw; home=fst randomTeams; away=snd randomTeams; kickoff=ko }  ) ]
    gameWeeks
        |> List.map(fun gw -> buildFixturesForGameWeek teams gw)    
        |> List.collect(fun f -> f)

let getRndScore() =
    let getRndGoals() = rnd.Next(0, 4)
    getRndGoals(), getRndGoals()

let buildPredictionsList (fixtures:Fixture list) players =
    let generatePrediction pl f = { Prediction.fixture=f; score=getRndScore(); player=pl }
    players
    |> List.map(fun pl -> fixtures |> List.map(fun f -> generatePrediction pl f))
    |> List.collect(fun p -> p)
        
let buildResults (fixtures:Fixture list) =
    let generateResult f = {Result.fixture=f; score=getRndScore()}
    fixtures |> List.map(fun f -> generateResult f)

let players = playersList
let gameWeeks = gameWeeksList
let fixtures = buildFixtureList teamsList gameWeeksList
let predictions = buildPredictionsList fixtures playersList
let results = buildResults fixtures
        
// initialising

let initAll (pl:Player list) (s:Season list) (g:GameWeek list) (f:Fixture list) (r:Result list) (p:Prediction list) =
    executeNonQuery "drop table if exists players; create table players (id uuid, name text, role text, email text)"
    executeNonQuery "drop table if exists seasons; create table seasons (id uuid, seasonId uuid, year text)"
    executeNonQuery "drop table if exists gameweeks; create table gameweeks (id uuid, number int, description text)"
    executeNonQuery "drop table if exists fixtures; create table fixtures (id uuid, gameWeekid uuid, home text, away text, kickoff timestamp)"
    executeNonQuery "drop table if exists results; create table results (id uuid, fixtureId uuid, homeScore int, awayScore int)"
    executeNonQuery "drop table if exists predictions; create table predictions (id uuid, fixtureId uuid, homeScore int, awayScore int, playerId uuid)" 
    pl |> List.iter(addPlayer)
    g |> List.iter(addGameWeek)
    f |> List.iter(addFixture)
    r |> List.iter(addResult)
    p |> List.iter(addPrediction)


//initAll players gameWeeks fixtures results predictions