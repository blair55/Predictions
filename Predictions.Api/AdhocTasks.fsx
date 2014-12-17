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

//let dunphyId = "ebd3cd36-db54-42b2-a354-7d5743b21082"
//
//let dunphyPredictions =
//    [//(dunphyId,"5743dc91-8b38-42ff-b1eb-85a0fa97891a","0-2")
//     (dunphyId,"ead66d56-17db-4b70-8dea-e36570734822","0-1")
//     (dunphyId,"d4f2fc3e-59d5-442a-bf14-abafb9e86ee5","1-0")
//     (dunphyId,"8ade3762-d02b-46e8-867d-eb7c909bc683","2-0")
//     (dunphyId,"53f378e7-24b5-46e7-95a3-96bdb35ee2d3","0-2")
//     (dunphyId,"30333cca-6678-4aa6-84f4-bf7a31581ace","3-0")
//     (dunphyId,"356d0c74-4385-4b78-9d67-ed54786d6af7","1-2")
//     (dunphyId,"4cafa48b-1fbb-474c-98fa-70432d5b4dba","0-4")
//     (dunphyId,"d4b9efbf-359f-4fcf-b4dc-4054a8f4acb9","2-2")
//     (dunphyId,"06bbf9c5-0502-4fbb-9c7b-f54ad3e92f5c","0-1")]
//
//let getScoreFromString (s:string) =
//    let i (sd:string) = Convert.ToInt32(sd)
//    let arr = s.Split('-')
//    i arr.[0], i arr.[1]
//
//let fixts = readFixtures()
//
//let printFixtureAndScore (c:SavePredictionCommand) =
//    let fixture = fixts |> List.find(fun f -> FxId f.id = c.fixtureId)
//    printfn "|%20s|%-20s|%i - %i|" fixture.home fixture.away (fst c.score) (snd c.score)

//let saveDunphyPreds() =
//    dunphyPredictions
//    |> List.map(fun (plid, fxid, scoreString) -> { SavePredictionCommand.id=newPrId(); fixtureId=fxid|>sToGuid|>FxId; playerId=plid|>sToGuid|>PlId; score=scoreString|>getScoreFromString })
//    //|> List.iter(fun c -> printfn "%A" c)
//    |> List.iter(printFixtureAndScore)
//    //|> List.iter(savePrediction)


// update gw 17 games to gw 16

let readgw16Fixtures() = (executeQuery "select * from fixtures where gameweekid = 'b1b5d8cb-fc27-4587-b524-ff933dd00f17'" readerToFixtureDto)
let readgw17Fixtures() = (executeQuery "select * from fixtures where gameweekid = 'a297c75f-bd4d-48ed-b82b-548ac53c087a'" readerToFixtureDto)

let u = "update fixtures set gameweekid = 'b1b5d8cb-fc27-4587-b524-ff933dd00f17' where gameweekid = 'a297c75f-bd4d-48ed-b82b-548ac53c087a'"
let m = "update players set name = 'Michael Mansfield' where name = 'Michael Manfield'"

