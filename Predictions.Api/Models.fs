namespace Predictions.Api

open System
open Predictions.Api.Common
open Predictions.Api.Domain
open Newtonsoft.Json

[<AutoOpen>]
module ViewModels =

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type TimeContainer = { time:DateTime }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PlayerViewModel = { id:string; name:string; isAdmin:bool }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type ScoreViewModel = { home:int; away:int; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type Team = { full:string; abrv:string; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type FixtureViewModel = { home:Team; away:Team; fxId:string; kickoff:DateTime; gameWeekNumber:int; isOpen:bool; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type GameWeekDetailsRowViewModel = { fixture:FixtureViewModel; predictionSubmitted:bool; prediction:ScoreViewModel; resultSubmitted:bool; result:ScoreViewModel; points:int; isDoubleDown:bool; bracketClass:string; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type GameWeekDetailsViewModel = { gameWeekNo:int; player:PlayerViewModel; totalPoints:int; rows:GameWeekDetailsRowViewModel array }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type OpenFixturesViewModelRow = { fixture:FixtureViewModel; scoreSubmitted:bool; newScore:string option; existingScore:ScoreViewModel; homeFormGuide:string array; awayFormGuide:string array; isDoubleDown:bool; predictionId:string; isDoubleDownAvailable:bool }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type OpenFixturesViewModel = { rows:OpenFixturesViewModelRow array }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type ClosedFixturesForGameWeekViewModel = { gameWeekNo:int; rows:OpenFixturesViewModelRow array }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type SubmittedPredictionResultModel = { predictionId:Guid }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type MicroLeagueViewModel = { id:string; name:string; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PastGameWeekRowViewModel = { gameWeekNo:int; winners:PlayerViewModel array; points:int; hasResult:bool; isGameWeekComplete:bool }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PastGameWeeksViewModel = { league:MicroLeagueViewModel; rows:PastGameWeekRowViewModel array }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type HistoryByMonthRowViewModel = { month:string; winners:PlayerViewModel array; points:int }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type HistoryByMonthViewModel = { league:MicroLeagueViewModel; rows:HistoryByMonthRowViewModel array }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type NeighboursViewModel = { hasPrev:bool; prev:string; next:string; hasNext:bool }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type LeagueTableRowViewModel = { diffPos:int; position:int; player:PlayerViewModel; correctScores:int; correctOutcomes:int; points:int }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type HistoryByMonthWithMonthViewModel = { league:MicroLeagueViewModel; month:string; rows:LeagueTableRowViewModel array; gameweeks:int array; neighbours:NeighboursViewModel }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type HistoryByGameWeekWithGameWeekViewModel = { league:MicroLeagueViewModel; gameWeekNo:int; rows:LeagueTableRowViewModel array; month:string; neighbours:NeighboursViewModel }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PlayerGameWeeksViewModelRow = { gameWeekNo:int; hasResults:bool; firstKo:DateTime; position:int; correctScores:int; correctOutcomes:int; points:int }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PlayerGameWeeksViewModel = { player:PlayerViewModel; position:int; rows:PlayerGameWeeksViewModelRow array; league:MicroLeagueViewModel }


    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type FixturePointsRowViewModel = { player:PlayerViewModel; predictionSubmitted:bool; prediction:ScoreViewModel; points:int }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type FixturePointsViewModel = { fixture:FixtureViewModel; resultSubmitted:bool; result:ScoreViewModel; openFixturesViewModelRow:OpenFixturesViewModelRow }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type LeaguePositionGraphData = { data:int array array; labels:string array; scaleSteps:int }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type FixturePredictionGraphData = { data:int array; labels:string array }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type FixturePreviousMeetingsQueryResultViewModelRow = { kickoff:DateTime; home:Team; away:Team; homeTeamScore:int; awayTeamScore:int; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type FixtureFormGuideViewModelRow = { fixture:FixtureViewModel; result:ScoreViewModel; outcome:string}

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type FixtureFormGuideViewModel = { homeRows:FixtureFormGuideViewModelRow array; awayRows:FixtureFormGuideViewModelRow array; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type FixturePreviousMeetingsQueryResultViewModel = {
        thisFixtureRows:FixturePreviousMeetingsQueryResultViewModelRow array;
        reverseFixtureRows:FixturePreviousMeetingsQueryResultViewModelRow array;
        allRows:FixturePreviousMeetingsQueryResultViewModelRow array; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type GameWeeksWithClosedFixturesRowViewModel = { gwno:int; closedFixtureCount:int }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type GameWeeksWithClosedFixturesViewModel = { rows:GameWeeksWithClosedFixturesRowViewModel array; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type InPlayRowViewModel = { fixture:FixtureViewModel; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type InPlayViewModel = { rows:InPlayRowViewModel array; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type ImportNewGameWeekViewModelRow = { home:string; away:string; kickoff:DateTime }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type ImportNewGameWeekViewModel = { gwno:int; rows:ImportNewGameWeekViewModelRow array }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PagedLeagueViewModel = { neighbours:NeighboursViewModel; rows:LeagueTableRowViewModel array }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PredictedLeagueTableRowViewModel = { pos:int; team:Team; played:int; won:int; drawn:int; lost:int; gf:int; ga:int; gd:int; points:int }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PredictedLeagueTableViewModel = { rows:PredictedLeagueTableRowViewModel array }

[<AutoOpen>]
module HomePageModels =

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type LeaguePositionViewModelRow = { leaguePosition:int; totalPlayerCount:int; playerLeaguePage:int }


[<AutoOpen>]
module PlayerProfileModels =

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PlayerProfileViewModelRow = { gameWeekNo:int; hasResults:bool; firstKo:DateTime; correctScores:int; correctOutcomes:int; points:int; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PlayerProfileViewModel = { player:PlayerViewModel; totalPoints:int; rows:PlayerProfileViewModelRow array }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PlayerProfileGraphData = { data:int array array; labels:string array }

[<AutoOpen>]
module LeagueModels =

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type LeaguesRowViewModel = { id:string; name:string; position:int; diffPos:int }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type LeaguesViewModel = { rows:LeaguesRowViewModel array }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type CreateLeaguePostModel = { name:string; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type LeagueViewModel = { id:string; name:string; rows:LeagueTableRowViewModel array; latestGameWeekNo:int; adminId:string; neighbours:NeighboursViewModel }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type LeagueInviteViewModel = { id:string; name:string; inviteLink:string; }

[<AutoOpen>]
module GameWeekMatrixModels =

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type GameWeekMatrixPlayerRowPredictionViewModel = { isSubmitted:bool; isFixtureOpen:bool; score:ScoreViewModel; bracketClass:string; isDoubleDown:bool }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type GameWeekMatrixPlayerRowViewModel = { player:PlayerViewModel; predictions:GameWeekMatrixPlayerRowPredictionViewModel array; points:int }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type GameWeekMatrixFixtureColumnViewModel = { fixture:FixtureViewModel; isSubmitted:bool; score:ScoreViewModel; }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type GameWeekMatrixViewModel = { gameWeekNo:int; league:MicroLeagueViewModel; rows:GameWeekMatrixPlayerRowViewModel array; columns:GameWeekMatrixFixtureColumnViewModel array; neighbours:NeighboursViewModel }

[<AutoOpen>]
module PostModels =

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type FixturePostModel = { home:string; away:string; kickOff:string }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type GameWeekPostModel = { fixtures:FixturePostModel array }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type ResultPostModel = { fixtureId:string; score:ScoreViewModel }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type PredictionPostModel = { fixtureId:string; score:ScoreViewModel }

    [<CLIMutable>][<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type ExternalLoginPostModel = { provider:string }
