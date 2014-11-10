﻿#r """C:\code\Predictions\packages\FSharp.Data.2.0.15\lib\net40\FSharp.Data.dll"""
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

let rnd = new System.Random()
let seasonId = newSnId()
let seasonYear = SnYr "2014/15"

type RealData = CsvProvider<"../import.csv">
let realData = RealData.Load("../import.csv")

let saveSeasonCommand = { SaveSeasonCommand.id=seasonId; year=seasonYear; }

let newAuthId() = Guid.NewGuid().ToString().Substring(0, 7) 

let playersListDtos = readPlayers()
//let playersListDtos =
//    ["Biggs Nick"; "Blair Nick"; "Bourke Dan"; "Curmi Ant"; "Dunphy Paul"; "Fergus Martin"; "Jones Matt"; "Jones Nick"; "Lewis Michael"; "Manfield Michael"; "Penman Matt"; "Russell Adam"; "Satar Salim"; "Sims Mark"; "Walsh James"; "West Dan"; "Woolley Michael"]
//    |> List.map(fun p -> { PlayerDto.id=nguid(); name=p; role="Admin"; email=""; authToken=newAuthId() })

let getSavePlayerCommandList (playerDtos:PlayerDto list) =
    playerDtos |> List.map(fun p -> { SavePlayerCommand.id=PlId p.id; name=p.name; role=Admin; email=""; authToken=p.authToken })

let getPlIdForPlayer (plrs:SavePlayerCommand list) name = plrs |> List.find(fun p -> p.name = name) |> (fun cmd -> cmd.id)

let getScore (s:string):Score option =
    let e = s.ToLower().Split('v') |> Seq.toList
    let clean (v:string) = printfn "conv %s" v; Convert.ToInt32(v.Trim())
    match e with
    | h::t -> if h = "n/a" then None else Some (h|>clean, t|>Seq.head|>clean)
    | _ -> None

let getko (s:string) =
    printfn "converting %s" s
    Convert.ToDateTime(s)

let getFixtureDataList gwno = 
    realData.Rows
    |> Seq.filter(fun r -> r.GW = gwno)
    |> Seq.map(fun r -> { FixtureData.id=r.FixtureId|>FxId; home=r.home; away=r.away; kickoff=r.KO; predictions=[] })
    |> Seq.toList

let getSaveGameWeekCommandList() =
    realData.Rows
    |> Seq.map(fun r -> r.GW)
    |> Seq.distinct
    |> Seq.map(fun gwno -> { SaveGameWeekCommand.id=newGwId(); seasonId=seasonId; description=""; fixtures=getFixtureDataList gwno })
    |> Seq.toList

let getSaveResultCommandList() =
    realData.Rows
    |> Seq.map(fun r -> let s = getScore r.Score
                        match s with
                        | None -> failwith "no result found for row"
                        | Some scr -> { SaveResultCommand.id=newRsId(); fixtureId=r.FixtureId|>FxId; score=scr })
    |> Seq.toList

let getPredictionCommand (plCmds:SavePlayerCommand list) fxid scr plr =
    match getScore scr with
    | Some s -> Some { SavePredictionCommand.id=newPrId(); fixtureId=fxid|>FxId; playerId=getPlIdForPlayer plCmds plr; score=s}
    | None -> None

let getSavePredictionCommandList (plCmds:SavePlayerCommand list) =
    realData.Rows
    |> Seq.map(fun r -> [
                        getPredictionCommand plCmds r.FixtureId r.``Biggs Nick`` "Biggs Nick"
                        getPredictionCommand plCmds r.FixtureId r.``Blair Nick`` "Blair Nick"
                        getPredictionCommand plCmds r.FixtureId r.``Bourke Dan`` "Bourke Dan"
                        getPredictionCommand plCmds r.FixtureId r.``Curmi Ant`` "Curmi Ant"
                        getPredictionCommand plCmds r.FixtureId r.``Dunphy Paul`` "Dunphy Paul"
                        getPredictionCommand plCmds r.FixtureId r.``Fergus Martin`` "Fergus Martin"
                        getPredictionCommand plCmds r.FixtureId r.``Jones Matt`` "Jones Matt"
                        getPredictionCommand plCmds r.FixtureId r.``Jones Nick`` "Jones Nick"
                        getPredictionCommand plCmds r.FixtureId r.``Lewis Michael`` "Lewis Michael"
                        getPredictionCommand plCmds r.FixtureId r.``Manfield Michael`` "Manfield Michael"
                        getPredictionCommand plCmds r.FixtureId r.``Penman Matt`` "Penman Matt"
                        getPredictionCommand plCmds r.FixtureId r.``Russell Adam`` "Russell Adam"
                        getPredictionCommand plCmds r.FixtureId r.``Satar Salim`` "Satar Salim"
                        getPredictionCommand plCmds r.FixtureId r.``Sims Mark`` "Sims Mark"
                        getPredictionCommand plCmds r.FixtureId r.``Walsh James`` "Walsh James"
                        getPredictionCommand plCmds r.FixtureId r.``West Dan`` "West Dan"
                        getPredictionCommand plCmds r.FixtureId r.``Woolley Michael`` "Woolley Michael"
                        ] )
    |> Seq.collect(fun r -> r)
    |> Seq.choose(fun r -> r)
    |> Seq.toList

//let getSavePlayerCommands (playerDtos:PlayerDto list) =
//    playerDtos |> List.map(fun p -> { SavePlayerCommand.id=PlId p.id; name=p.name; role=Admin; email="" })

let initAll (plrs:SavePlayerCommand list) (sn:SaveSeasonCommand) (gws:SaveGameWeekCommand list) (rs:SaveResultCommand list) (prs:SavePredictionCommand list) =
    executeNonQuery "drop table if exists players; create table players (id uuid, name text, role text, email text, authToken text)"
    executeNonQuery "drop table if exists seasons; create table seasons (id uuid, year text)"
    executeNonQuery "drop table if exists gameweeks; create table gameweeks (id uuid, seasonId uuid, number SERIAL, description text)"
    executeNonQuery "drop table if exists fixtures; create table fixtures (id uuid, gameWeekid uuid, home text, away text, kickoff timestamp)"
    executeNonQuery "drop table if exists results; create table results (id uuid, fixtureId uuid, homeScore int, awayScore int)"
    executeNonQuery "drop table if exists predictions; create table predictions (id uuid, fixtureId uuid, homeScore int, awayScore int, playerId uuid)" 
    saveSeason sn
    plrs |> List.iter(savePlayer)
    gws |> List.iter(saveGameWeek)
    rs |> List.iter(saveResult)
    prs |> List.iter(savePrediction)

let players = getSavePlayerCommandList playersListDtos
let gameWeeks = getSaveGameWeekCommandList()
let results = getSaveResultCommandList()
let predictions = getSavePredictionCommandList players
        
//initAll players saveSeasonCommand gameWeeks results predictions


let pidtos pid = pid |> getPlayerId |> str
let localUrl = "http://localhost:49782/api/auth/"
let liveUrl = "http://predictions.apphb.com/api/auth/"
let publishPlayers url =
    players
    |> List.map(fun p -> (p.name, sprintf "%s%s" url p.authToken))
    |> List.sortBy(fun (p, _) -> p)
    |> List.iter(fun (name, url) -> printf "%s %s %s" name url Environment.NewLine)

