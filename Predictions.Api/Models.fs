namespace Predictions.Api

open System
open Predictions.Api.Common
open Predictions.Api.Domain
open Newtonsoft.Json

[<AutoOpen>]
module ViewModels =
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PlayerViewModel = { id:string; name:string; isAdmin:bool }
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type ScoreViewModel = { home:int; away:int; }
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type FixtureViewModel = { home:string; away:string; fxId:string; kickoff:DateTime; gameWeekNumber:int; isOpen:bool; }
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type GameWeekDetailsRowViewModel = { fixture:FixtureViewModel; predictionSubmitted:bool; prediction:ScoreViewModel; resultSubmitted:bool; result:ScoreViewModel; points:int }
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type GameWeekDetailsViewModel = { gameWeekNo:int; player:PlayerViewModel; totalPoints:int; rows:GameWeekDetailsRowViewModel list }
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type OpenFixturesViewModelRow = { fixture:FixtureViewModel; scoreSubmitted:bool; newScore:string option; existingScore:ScoreViewModel; homeFormGuide:string list; awayFormGuide:string list; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type OpenFixturesViewModel = { rows:OpenFixturesViewModelRow list }
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type ClosedFixturesForGameWeekViewModel = { gameWeekNo:int; rows:OpenFixturesViewModelRow list }

    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type MicroLeagueViewModel = { id:string; name:string; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PastGameWeekRowViewModel = { gameWeekNo:int; winner:PlayerViewModel; points:int; hasResult:bool }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PastGameWeeksViewModel = { league:MicroLeagueViewModel; rows:PastGameWeekRowViewModel list }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type HistoryByMonthRowViewModel = { month:string; winner:PlayerViewModel; points:int }
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type HistoryByMonthViewModel = { league:MicroLeagueViewModel; rows:HistoryByMonthRowViewModel list }
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type LeagueTableRowViewModel = { diffPos:int; position:int; player:PlayerViewModel; correctScores:int; correctOutcomes:int; points:int }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type HistoryByMonthWithMonthViewModel = { league:MicroLeagueViewModel; month:string; rows:LeagueTableRowViewModel list; gameweeks:int list }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type GameWeekPointsViewModel = { league:MicroLeagueViewModel; gameWeekNo:int; rows:LeagueTableRowViewModel list; month:string; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PlayerGameWeeksViewModelRow = { gameWeekNo:int; hasResults:bool; firstKo:DateTime; position:int; correctScores:int; correctOutcomes:int; points:int; }
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PlayerGameWeeksViewModel = { player:PlayerViewModel; position:int; rows:PlayerGameWeeksViewModelRow list; league:MicroLeagueViewModel }


    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type FixturePointsRowViewModel = { player:PlayerViewModel; predictionSubmitted:bool; prediction:ScoreViewModel; points:int }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type FixturePointsViewModel = { fixture:FixtureViewModel; resultSubmitted:bool; result:ScoreViewModel; openFixturesViewModelRow:OpenFixturesViewModelRow }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type LeaguePositionGraphData = { data:int list list; labels:string list }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type FixturePredictionGraphData = { data:int list; labels:string list }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type FixturePreviousMeetingsQueryResultViewModelRow = { kickoff:DateTime; homeTeamName:string; awayTeamName:string; homeTeamScore:int; awayTeamScore:int; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type FixtureFormGuideViewModelRow = { fixture:FixtureViewModel; result:ScoreViewModel; outcome:string}

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type FixtureFormGuideViewModel = { homeRows:FixtureFormGuideViewModelRow list; awayRows:FixtureFormGuideViewModelRow list; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type FixturePreviousMeetingsQueryResultViewModel = {
        thisFixtureRows:FixturePreviousMeetingsQueryResultViewModelRow list;
        reverseFixtureRows:FixturePreviousMeetingsQueryResultViewModelRow list; 
        rows:FixturePreviousMeetingsQueryResultViewModelRow list; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type GameWeeksWithClosedFixturesRowViewModel = { gwno:int; closedFixtureCount:int }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type GameWeeksWithClosedFixturesViewModel = { rows:GameWeeksWithClosedFixturesRowViewModel list; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type InPlayRowViewModel = { fixture:FixtureViewModel; }
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type InPlayViewModel = { rows:InPlayRowViewModel list; }
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type ImportNewGameWeekViewModel = { rows:FixtureViewModel list; gwno:int }
    
[<AutoOpen>]
module HomePageModels =

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type LeaguePositionViewModelRow = { leaguePosition:int; totalPlayerCount:int; playerLeaguePage:int }


[<AutoOpen>]
module PlayerProfileModels =
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PlayerProfileViewModelRow = { gameWeekNo:int; hasResults:bool; firstKo:DateTime; correctScores:int; correctOutcomes:int; points:int; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PlayerProfileViewModel = { player:PlayerViewModel; totalPoints:int; rows:PlayerProfileViewModelRow list }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PlayerProfileGraphData = { data:int list list; labels:string list }

[<AutoOpen>]
module LeagueModels =

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type LeaguesRowViewModel = { id:string; name:string; position:int; diffPos:int }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type LeaguesViewModel = { rows:LeaguesRowViewModel list }
    
    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type CreateLeaguePostModel = { name:string; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type LeagueViewModel = { id:string; name:string; rows:LeagueTableRowViewModel list; latestGameWeekNo:int }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type LeagueInviteViewModel = { id:string; name:string; inviteLink:string; }

[<AutoOpen>]
module GameWeekMatrixModels =

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type GameWeekMatrixPlayerRowPredictionViewModel = { isSubmitted:bool; isFixtureOpen:bool; score:ScoreViewModel; bracketClass:string }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type GameWeekMatrixPlayerRowViewModel = { player:PlayerViewModel; predictions:GameWeekMatrixPlayerRowPredictionViewModel list; points:int }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type GameWeekMatrixFixtureColumnViewModel = { fixture:FixtureViewModel; isSubmitted:bool; score:ScoreViewModel; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type GameWeekMatrixViewModel = { gameWeekNo:int; league:MicroLeagueViewModel; rows:GameWeekMatrixPlayerRowViewModel list; columns:GameWeekMatrixFixtureColumnViewModel list }

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

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type ExternalLoginPostModel = { provider:string }
