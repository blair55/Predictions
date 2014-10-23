namespace Predictions.Api

open System
open Predictions.Api.Common
open Predictions.Api.Domain
open Predictions.Api.Data
open Newtonsoft.Json

[<AutoOpen>]
module ViewModels =
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PlayerViewModel = { id:string; name:string; isAdmin:bool }
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type LeagueTableRowViewModel = { position:int; player:PlayerViewModel; correctScores:int; correctOutcomes:int; points:int }
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type LeagueTableViewModel = { rows:LeagueTableRowViewModel list }


    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PlayerGameWeeksViewModelRow = { gameWeekNo:int; position:int; correctScores:int; correctOutcomes:int; points:int; }
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PlayerGameWeeksViewModel = { player:PlayerViewModel; rows:PlayerGameWeeksViewModelRow list }
    


    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type ScoreViewModel = { home:int; away:int; }
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type FixtureViewModel = { home:string; away:string; fxId:string; kickoff:DateTime; gameWeekNumber:int }
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type GameWeekDetailsRowViewModel = { fixture:FixtureViewModel; predictionSubmitted:bool; prediction:ScoreViewModel; resultSubmitted:bool; result:ScoreViewModel; points:int }
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type GameWeekDetailsViewModel = { gameWeekNo:int; player:PlayerViewModel; totalPoints:int; rows:GameWeekDetailsRowViewModel list }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type OpenGameWeeksViewModel = { rows: int list }
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type OpenFixturesViewModel = { rows: FixtureViewModel list }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type FixturesAwaitingResultsViewModel = { rows: FixtureViewModel list }
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PastGameWeekRowViewModel = { gameWeekNo:int; winner:PlayerViewModel; points:int }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PastGameWeeksViewModel = { rows:PastGameWeekRowViewModel list }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type GameWeekPointsViewModel = { gameWeekNo:int; rows:LeagueTableRowViewModel list }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type FixturePointsRowViewModel = { player:PlayerViewModel; predictionSubmitted:bool; prediction:ScoreViewModel; points:int }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type FixturePointsViewModel = { fixture:FixtureViewModel; result:ScoreViewModel; rows:FixturePointsRowViewModel list }


[<AutoOpen>]
module PostModels =

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type FixturePostModel = { home:string; away:string; kickOff:string }
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type GameWeekPostModel = { fixtures:FixturePostModel list }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type ResultPostModel = { fixtureId:string; score:ScoreViewModel }
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PredictionPostModel = { fixtureId:string; score:ScoreViewModel }

        