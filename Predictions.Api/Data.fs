namespace Predictions.Api

open System
open System.Data
open System.Data.SqlClient
open System.Configuration
open Dapper
open Predictions.Api.Domain
open Predictions.Api.AppSettings

module Data =

    let connString = AppSettings.get "SQLSERVER_CONNECTION_STRING"

    let agent = MailboxProcessor.Start(fun inbox -> async {
        while true do
            let! msg = inbox.Receive()
            msg() })

    let nonQuery sql args =
        agent.Post(fun () ->
            try 
                use conn = new SqlConnection(connString)
                conn.Execute(sql, args) |> ignore
            with ex -> Logging.error ex)

    type RegisterPlayerCommand = { player:Player; explid:ExternalPlayerId; exProvider:ExternalLoginProvider; email:string }
    type RegisterPlayerCommandArgs = { id:Guid; externalId:string; externalProvider:string; name:string; email:string }
    let registerPlayerInDb (cmd:RegisterPlayerCommand) =
        nonQuery @"
        IF EXISTS(select playerId, playerName, isAdmin from players where ExternalLoginId = @externalId and ExternalLoginProvider = @externalProvider)
        BEGIN UPDATE Players SET PlayerName = @name, IsActive = 1 WHERE ExternalLoginId = @externalId and ExternalLoginProvider = @externalProvider END ELSE
        BEGIN INSERT INTO Players(PlayerId, ExternalLoginId, ExternalLoginProvider, PlayerName, IsAdmin, Email, IsActive) values (@id, @externalId, @externalProvider, @Name, 0, @email, 1) END"
            { RegisterPlayerCommandArgs.id=cmd.player.id|>getPlayerId; name=cmd.player.name|>getPlayerName; externalId=cmd.explid|>getExternalPlayerId; externalProvider=cmd.exProvider|>getExternalLoginProvider; email=cmd.email }

    type SaveLeagueCommand = { id:LgId; name:LeagueName; admin:Player }
    type SaveLeagueCommandArgs = { id:Guid; name:string; shareableId:string; adminId:Guid }
    let saveLeagueInDb (cmd:SaveLeagueCommand) =
        nonQuery @"insert into Leagues(LeagueId, LeagueShareableId, LeagueName, LeagueAdminId, LeagueIsDeleted) values (@id, @shareableId, @name, @adminId, 0)"
            { SaveLeagueCommandArgs.id=cmd.id|>getLgId; name=cmd.name|>getLeagueName; shareableId=cmd.id|>getShareableLeagueId; adminId=cmd.admin.id|>getPlayerId }

    type JoinLeagueCommand = { leagueId:LgId; playerId:PlId }
    type JoinLeagueCommandArgs = { leagueId:Guid; playerId:Guid }
    let joinLeagueInDb (cmd:JoinLeagueCommand) =
        nonQuery @"insert into LeaguePlayerBridge(LeagueId, PlayerId) values (@leagueId, @playerId)"
            { JoinLeagueCommandArgs.leagueId=cmd.leagueId|>getLgId; playerId=cmd.playerId|>getPlayerId }

    type LeaveLeagueCommand = { leagueId:LgId; playerId:PlId }
    type LeaveLeagueCommandArgs = { leagueId:Guid; playerId:Guid }
    let leaveLeagueInDb (cmd:LeaveLeagueCommand) =
        nonQuery @"delete from LeaguePlayerBridge where LeagueId = @leagueId and PlayerId = @playerId"
            { LeaveLeagueCommandArgs.leagueId=cmd.leagueId|>getLgId; playerId=cmd.playerId|>getPlayerId }

    type DeleteLeagueCommand = { leagueId:LgId; }
    type DeleteLeagueCommandArgs = { leagueId:Guid; }
    let deleteLeagueInDb (cmd:DeleteLeagueCommand) =
        nonQuery @"update Leagues set LeagueIsDeleted = 1 where LeagueId = @leagueId"
            { DeleteLeagueCommandArgs.leagueId=cmd.leagueId|>getLgId }

    //type UpdateUserNameCommand = { playerId:PlId; playerName:PlayerName }
    //type UpdateUserNameCommandArgs = { playerId:Guid; playerName:string }
    //let updateUserNameInDb (cmd:UpdateUserNameCommand) =
    //    nonQuery @"UPDATE Players SET PlayerName = @playerName, IsActive = 1 WHERE PlayerId = @playerId"
    //        { UpdateUserNameCommandArgs.playerId=cmd.playerId|>getPlayerId; playerName=cmd.playerName|>getPlayerName }

    type SaveResultCommand = { fixtureId:FxId; score:Score }
    type SaveResultCommandArgs = { fixtureId:Guid; homeScore:int; awayScore:int }
    let saveResult (cmd:SaveResultCommand) =
        nonQuery @"UPDATE Fixtures SET HomeTeamScore = @homeScore, AwayTeamScore = @AwayScore WHERE FixtureId = @fixtureId"
            { SaveResultCommandArgs.fixtureId=cmd.fixtureId|>getFxId; homeScore=cmd.score|>fst; awayScore=cmd.score|>snd }

    type SavePredictionCommand = { id:PrId; fixtureId:FxId; playerId:PlId; score:Score }
    type SavePredictionCommandArgs = { id:Guid; fixtureId:Guid; playerId:Guid; homeScore:int; awayScore:int }
    let savePrediction (cmd:SavePredictionCommand) =
        nonQuery @"
        IF EXISTS(SELECT * FROM Predictions WHERE FixtureId = @FixtureId AND PlayerId = @PlayerId)
        BEGIN UPDATE Predictions SET HomeTeamScore = @homeScore, AwayTeamScore = @AwayScore WHERE FixtureId = @FixtureId AND PlayerId = @PlayerId END ELSE
        BEGIN INSERT INTO Predictions(PredictionId, FixtureId, PlayerId, HomeTeamScore, AwayTeamScore) VALUES (@Id, @FixtureId, @PlayerId, @HomeScore, @AwayScore) END"
            { SavePredictionCommandArgs.id=cmd.id|>getPrId; fixtureId=cmd.fixtureId|>getFxId; playerId=cmd.playerId|>getPlayerId; homeScore=cmd.score|>fst; awayScore=cmd.score|>snd }

    type SaveFixtureCommand = { id:FxId; home:Team; away:Team; kickoff:KickOff }
    type SaveGameWeekCommand = { id:GwId; seasonYear:SnYr; description:string; saveFixtureCommands:SaveFixtureCommand array }
    type SaveGameWeekCommandArgs = { id:Guid; seasonYear:string; description:string; }
    type SaveFixtureCommandArgs = { id:Guid; gameWeekId:Guid; ko:DateTime; home:string; away:string }
    let saveGameWeek (cmd:SaveGameWeekCommand) =
        nonQuery @"
        declare @seasonId uniqueidentifier
        select @seasonId = seasonId from seasons where SeasonYear = @SeasonYear
        declare @nextGameWeek int
        select @nextGameWeek = case when max(gameweeknumber) IS NULL then 1 else max(gameweeknumber) + 1 end from gameweeks where seasonid = @SeasonId
        insert into GameWeeks(GameWeekId, SeasonId, GameWeekNumber, GameWeekDescription) values (@Id, @SeasonId, @nextGameWeek, @description)"
                { SaveGameWeekCommandArgs.id=cmd.id|>getGwId; seasonYear=cmd.seasonYear|>getSnYr; description=cmd.description }
        let saveFixture (fd:SaveFixtureCommand) =
            nonQuery @"insert into Fixtures(FixtureId, GameWeekId, KickOff, HomeTeamName, AwayTeamName) values (@Id, @GameWeekId, @ko, @Home, @Away)"
                { SaveFixtureCommandArgs.id=fd.id|>getFxId; gameWeekId=cmd.id|>getGwId; ko=fd.kickoff; home=fd.home; away=fd.away }
        cmd.saveFixtureCommands |> Array.iter saveFixture

    type SaveDoubleDownCommand = { playerId:PlId; gameWeekId:GwId; predictionId:PrId }
    type SaveDoubleDownCommandArgs = { playerId:Guid; gameWeekId:Guid; predictionId:Guid }
    let saveDoubleDownInDb (cmd:SaveDoubleDownCommand) =
        nonQuery @"
        IF EXISTS(SELECT * FROM DoubleDowns WHERE PlayerId = @PlayerId AND GameWeekId = @GameWeekId)
        BEGIN UPDATE DoubleDowns SET PredictionId = @PredictionId WHERE PlayerId = @PlayerId AND GameWeekId = @GameWeekId END ELSE
        BEGIN INSERT INTO DoubleDowns(PlayerId, GameWeekId, PredictionId) VALUES (@PlayerId, @GameWeekId, @PredictionId) END"
            { SaveDoubleDownCommandArgs.playerId=cmd.playerId|>getPlayerId; gameWeekId=cmd.gameWeekId|>getGwId; predictionId=cmd.predictionId|>getPrId }

    type BuildSeasonQueryArgs = { seasonYear:string; }
    type LeagueIdsPlayerIsInQueryArgs = { playerId:Guid }
    type FindPlayerByPlayerIdQueryArgs = { playerId:Guid }
    type FindPlayerByExternalIdQueryArgs = { externalId:string; externalProvider:string }
    type FindLeagueByLeagueIdQueryArgs = { leagueId:Guid }
    type FindLeagueByShareableLeagueIdQueryArgs = { shareableLeagueId:string }
    type GetAllPredictionsForFixtureQueryArgs = { fixtureId:Guid }
    type GetFixturePreviousMeetingsQueryArgs = { homeTeamName:string; awayTeamName:string }

    type [<CLIMutable>] BuildSeasonQueryResult = { seasonId:Guid; seasonYear:string }
    type [<CLIMutable>] BuildGameWeekQueryResult = { gameWeekId:Guid; seasonId:Guid; gameWeekNumber:int; gameWeekDescription:string }
    type [<CLIMutable>] BuildFixtureQueryResult = { fixtureId:Guid; gameWeekId:Guid; kickoff:DateTime; homeTeamName:string; awayTeamName:string; homeTeamScore:Nullable<int>; awayTeamScore:Nullable<int> }
    type [<CLIMutable>] LeagueIdsPlayerIsInQueryResult = { leagueId:Guid }
    type [<CLIMutable>] PlayersTableQueryResult = { playerId:Guid; playerName:string; isAdmin:bool }
    type [<CLIMutable>] LeaguesTableQueryResult = { leagueId:Guid; leagueName:string; leagueAdminId:Guid }
    type [<CLIMutable>] PlayersTableWithLeagueJoinDateQueryResult = { playerId:Guid; playerName:string; isAdmin:bool; leagueJoinDate:DateTime }
    type [<CLIMutable>] GetFixturePreviousMeetingsQueryResult = { kickoff:DateTime; homeTeamName:string; awayTeamName:string; homeTeamScore:int; awayTeamScore:int; }
    type [<CLIMutable>] PredictionsTableQueryResult = { predictionId:Guid; fixtureId:Guid; playerId:Guid; homeTeamScore:int; awayTeamScore:int; doubleDown:int; created:DateTime }

    type Query =
        | BuildSeason of BuildSeasonQueryArgs
        | BuildGameWeek of BuildSeasonQueryArgs
        | BuildFixture of BuildSeasonQueryArgs
        | GetLeagueIdsPlayerIsIn of LeagueIdsPlayerIsInQueryArgs
        | FindPlayerByPlayerId of FindPlayerByPlayerIdQueryArgs
        | GetPredictionsForPlayer of FindPlayerByPlayerIdQueryArgs
        | FindPlayerByExternalId of FindPlayerByExternalIdQueryArgs
        | FindLeagueByLeagueId of FindLeagueByLeagueIdQueryArgs
        | GetPlayersWithLeagueJoinDate of FindLeagueByLeagueIdQueryArgs
        | GetPredictionsForPlayersInLeague of FindLeagueByLeagueIdQueryArgs
        | FindLeagueByShareableLeagueId of FindLeagueByShareableLeagueIdQueryArgs
        | GetAllPredictionsForFixture of GetAllPredictionsForFixtureQueryArgs
        | GetFixturePreviousMeetings of GetFixturePreviousMeetingsQueryArgs
        | GetAllPlayers
        | GetAllPredictions

    let query<'r> query =
        let getQueryFuncWithArgs sql args = (fun (conn:IDbConnection) -> conn.Query<'r>(sql, args))
        let getQueryFuncWithNoArgs (sql:string) = (fun (conn:IDbConnection) -> conn.Query<'r>(sql))
        let queryF =
            match query with
            | BuildSeason a -> getQueryFuncWithArgs "select sns.seasonId, sns.seasonYear from seasons sns where sns.SeasonYear = @seasonYear" a
            | BuildGameWeek a -> getQueryFuncWithArgs "select gws.gameWeekId, gws.seasonId, gws.gameWeekNumber, gws.gameWeekDescription from seasons sns join gameWeeks gws on sns.SeasonId = gws.SeasonId where sns.SeasonYear = @seasonYear" a
            | BuildFixture a -> getQueryFuncWithArgs "select fxs.fixtureId, fxs.gameWeekId, fxs.kickoff, fxs.homeTeamName, fxs.awayTeamName, fxs.homeTeamScore, fxs.awayTeamScore from seasons sns join gameWeeks gws on sns.SeasonId = gws.SeasonId join fixtures fxs on gws.GameWeekId = fxs.GameWeekId where sns.SeasonYear = @seasonYear" a
            | GetLeagueIdsPlayerIsIn a -> getQueryFuncWithArgs "select lgs.leagueId from leagues lgs join leaguePlayerBridge lpb on lgs.leagueId = lpb.leagueId where lpb.playerId = @playerId and lgs.LeagueIsDeleted = 0" a
            | FindPlayerByPlayerId a -> getQueryFuncWithArgs "select playerId, playerName, isAdmin from players where IsActive = 1 and playerId = @playerId" a
            | FindPlayerByExternalId a -> getQueryFuncWithArgs "select playerId, playerName, isAdmin from players where IsActive = 1 and ExternalLoginId = @externalId and ExternalLoginProvider = @externalProvider" a
            | GetPredictionsForPlayer a -> getQueryFuncWithArgs "select pds.predictionId, pds.fixtureId, pds.playerId, pds.homeTeamScore, pds.awayTeamScore, pds.created, case when dd.predictionid is null then 0 else 1 end as DoubleDown from predictions pds left outer join DoubleDowns dd on dd.PlayerId = @playerId and pds.PredictionId = dd.PredictionId where pds.playerId = @playerId" a
            | FindLeagueByLeagueId a -> getQueryFuncWithArgs "select lgs.LeagueId, lgs.LeagueName, lgs.LeagueAdminId from Leagues lgs where lgs.LeagueId = @leagueId and lgs.LeagueIsDeleted = 0" a
            | GetPlayersWithLeagueJoinDate a -> getQueryFuncWithArgs "select pls.PlayerId, pls.PlayerName, pls.IsAdmin, lpb.Created as LeagueJoinDate from Players pls join LeaguePlayerBridge lpb on pls.PlayerId = lpb.PlayerId where pls.IsActive = 1 and lpb.LeagueId = @leagueId" a
            | GetPredictionsForPlayersInLeague a -> getQueryFuncWithArgs "select pds.PredictionId, pds.FixtureId, pds.PlayerId, pds.HomeTeamScore, pds.AwayTeamScore, pds.created, case when dd.predictionid is null then 0 else 1 end as DoubleDown from Predictions pds join Players pls on pds.PlayerId = pls.PlayerId join LeaguePlayerBridge lpb on pls.PlayerId = lpb.PlayerId left outer join DoubleDowns dd on lpb.PlayerId = dd.PlayerId and pds.PredictionId = dd.PredictionId where pls.IsActive = 1 and lpb.LeagueId = @leagueId" a
            | FindLeagueByShareableLeagueId a -> getQueryFuncWithArgs "select LeagueId, LeagueName, LeagueAdminId from Leagues where LeagueShareableId = @shareableLeagueId and LeagueIsDeleted = 0" a
            | GetAllPredictionsForFixture a -> getQueryFuncWithArgs "select pds.predictionId, pds.fixtureId, pds.playerId, pds.homeTeamScore, pds.awayTeamScore, pds.created, case when dd.predictionid is null then 0 else 1 end as DoubleDown from predictions pds left outer join DoubleDowns dd on dd.PlayerId = pds.playerId and pds.PredictionId = dd.PredictionId where pds.fixtureId = @fixtureId" a
            | GetFixturePreviousMeetings a -> getQueryFuncWithArgs "select kickoff, hometeamname, awayteamname, hometeamscore, awayteamscore from fixtures where hometeamscore is not null and awayteamscore is not null and ((hometeamname = @hometeamname and awayteamname = @awayteamname) or (awayteamname = @hometeamname and hometeamname = @awayteamname))" a
            | GetAllPlayers -> getQueryFuncWithNoArgs "select playerId, playerName, isAdmin from players where isactive = 1"
            | GetAllPredictions -> getQueryFuncWithNoArgs "select pds.predictionId, pds.fixtureId, pds.playerId, pds.homeTeamScore, pds.awayTeamScore, pds.created, case when dd.predictionid is null then 0 else 1 end as DoubleDown from predictions pds left outer join DoubleDowns dd on dd.PlayerId = pds.playerId and pds.PredictionId = dd.PredictionId"
        agent.PostAndReply(fun channel ->
            (fun () ->
                try 
                    use conn = new SqlConnection(connString)
                    conn |> queryF |> channel.Reply
                with ex -> Logging.error ex))

    let buildSeason (year:SnYr) =
        let args = { BuildSeasonQueryArgs.seasonYear=year|>getSnYr; }
        let seasonResult = BuildSeason args |> query<BuildSeasonQueryResult> |> Seq.head
        let gameWeeksResult = BuildGameWeek args |> query<BuildGameWeekQueryResult>
        let fixturesResult = BuildFixture args |> query<BuildFixtureQueryResult>
        let buildFixture (result:BuildFixtureQueryResult) =
            let fd = { FixtureData.id=result.fixtureId|>FxId; gwId=result.gameWeekId|>GwId; home=result.homeTeamName; away=result.awayTeamName; kickoff=result.kickoff }
            let resultExists = result.homeTeamScore.HasValue && result.awayTeamScore.HasValue
            let r = if resultExists then Some { Result.score=(result.homeTeamScore.Value, result.awayTeamScore.Value) } else None
            fd, r
        let buildGameWeek (fdrs:seq<(FixtureData*Result option)>) (result:BuildGameWeekQueryResult) =
            let gwFixtures =
                fdrs
                |> Seq.filter(fun (fd, _) -> fd.gwId=(result.gameWeekId|>GwId))
                |> Seq.map(fun (fd, r) -> fixtureDataToFixture fd r)
                |> Seq.toArray
            { GameWeek.id=result.gameWeekId|>GwId; number=result.gameWeekNumber|>GwNo; description=result.gameWeekDescription; fixtures=gwFixtures }
        let fdrs = fixturesResult |> Seq.map buildFixture
        let gameWeeks = gameWeeksResult |> Seq.map(buildGameWeek fdrs) |> Seq.toArray
        { id=seasonResult.seasonId|>SnId; year=seasonResult.seasonYear|>SnYr; gameWeeks=gameWeeks }

    let getLeagueIdsThatPlayerIsIn (player:Player) =
        let args = { LeagueIdsPlayerIsInQueryArgs.playerId=player.id|>getPlayerId }
        let result = GetLeagueIdsPlayerIsIn args |> query<LeagueIdsPlayerIsInQueryResult>
        result |> Seq.map(fun r -> r.leagueId|>LgId) |> Seq.toArray

    let queryResultToPlayer (player:PlayersTableQueryResult) predictions =
        { Player.id=player.playerId|>PlId; name=player.playerName|>PlayerName; predictions=predictions; isAdmin=player.isAdmin }
    let queryResultToPrediction (p:PredictionsTableQueryResult) =
        let modifier = match p.doubleDown with | 1 -> DoubleDown | _ -> NoModifier
        { Prediction.id=p.predictionId|>PrId; fixtureId=p.fixtureId|>FxId; playerId=p.playerId|>PlId; score=(p.homeTeamScore, p.awayTeamScore); modifier=modifier }
    let tryFindPlayerByPlayerId playerId =
        let args = { FindPlayerByPlayerIdQueryArgs.playerId=playerId|>getPlayerId }
        let playersResult = FindPlayerByPlayerId args |> query<PlayersTableQueryResult> |> Seq.toArray
        if playersResult |> Array.isEmpty then None
        else
            let predictions =
                GetPredictionsForPlayer args
                |> query<PredictionsTableQueryResult>
                |> Seq.map queryResultToPrediction
                |> Seq.toArray
            let player = playersResult.[0]
            Some (queryResultToPlayer player predictions)

    let findPlayerByExternalId externalPlayerId externalLoginProvider =
        let args = { FindPlayerByExternalIdQueryArgs.externalId=externalPlayerId|>getExternalPlayerId; externalProvider=externalLoginProvider|>getExternalLoginProvider }
        let result = FindPlayerByExternalId args |> query<PlayersTableQueryResult> |> Seq.toArray
        let player = result.[0]
        queryResultToPlayer player [||]

    let tryFindLeagueByLeagueId leagueId =
        let args = { FindLeagueByLeagueIdQueryArgs.leagueId=leagueId|>getLgId }
        let leagueResult = FindLeagueByLeagueId args |> query<LeaguesTableQueryResult> |> Seq.toArray
        if leagueResult |> Array.isEmpty then None
        else
            let playersResult = GetPlayersWithLeagueJoinDate args |> query<PlayersTableWithLeagueJoinDateQueryResult> |> Seq.toArray
            let predictionsResult = GetPredictionsForPlayersInLeague args |> query<PredictionsTableQueryResult> |> Seq.toArray
            let players =
                let getPlayerPredictionsSinceJoinedLeague (p:PlayersTableWithLeagueJoinDateQueryResult) =
                    predictionsResult
                    |> Array.filter(fun pr -> pr.playerId = p.playerId)
                    |> Array.filter(fun pr -> pr.created > p.leagueJoinDate)
                    |> Array.map queryResultToPrediction
                playersResult
                |> Array.map(fun p -> { Player.id=p.playerId|>PlId; name=p.playerName|>PlayerName; predictions=p|>getPlayerPredictionsSinceJoinedLeague; isAdmin=p.isAdmin })
            let league = leagueResult.[0]
            Some { League.id=league.leagueId|>LgId; name=league.leagueName|>LeagueName; players=players; adminId=league.leagueAdminId|>PlId }

    let tryFindLeagueByShareableId shareableLeagueId =
        let args = { FindLeagueByShareableLeagueIdQueryArgs.shareableLeagueId=shareableLeagueId }
        let result = FindLeagueByShareableLeagueId args |> query<LeaguesTableQueryResult> |> Seq.toArray
        if result |> Array.isEmpty then None
        else
            let league = result.[0]
            Some { League.id=league.leagueId|>LgId; name=league.leagueName|>LeagueName; players=Array.empty; adminId=league.leagueAdminId|>PlId }

    let getAllPredictionsForFixture (fxid:FxId) =
        { GetAllPredictionsForFixtureQueryArgs.fixtureId=fxid|>getFxId }
        |> GetAllPredictionsForFixture
        |> query<PredictionsTableQueryResult>
        |> Seq.map queryResultToPrediction
        |> Seq.toArray

    let getFixturePreviousMeetingsData home away =
        { GetFixturePreviousMeetingsQueryArgs.homeTeamName=home; awayTeamName=away }
        |> GetFixturePreviousMeetings
        |> query<GetFixturePreviousMeetingsQueryResult>
        |> Seq.map(fun r -> (r.kickoff, r.homeTeamName, r.awayTeamName, r.homeTeamScore, r.awayTeamScore))
        |> Seq.toArray

    let getAllPlayers() =
        let playersResult =
            GetAllPlayers
            |> query<PlayersTableQueryResult>
            |> Seq.toArray
        let predictions =
            GetAllPredictions
            |> query<PredictionsTableQueryResult>
            |> Seq.map queryResultToPrediction
            |> Seq.toArray
        let getPredictionsForPlayerId plid =
            predictions
            |> Array.filter(fun pr -> pr.playerId = (plid|>PlId))
        playersResult
            |> Array.map(fun pl -> queryResultToPlayer pl (getPredictionsForPlayerId pl.playerId))

    //let getPlayerByExternalLogin (explid:ExternalPlayerId, exprov:ExternalLoginProvider) =
    //    match tryFindPlayerByExternalId explid exprov with
    //    | Some p -> p |> Success
    //    | None -> NotFound "Player not found" |> Failure

    let getPlayer (plId:PlId) =
        match tryFindPlayerByPlayerId plId with
        | Some p -> p |> Success
        | None -> NotFound "Player not found" |> Failure

    let getLeagueByShareableId shareableLeagueId =
        match tryFindLeagueByShareableId shareableLeagueId with
        | Some league -> league |> Success
        | None -> NotFound "League not found" |> Failure

    let getLeague (leagueId:LgId) =
        match tryFindLeagueByLeagueId leagueId with
        | Some league -> league |> Success
        | None -> NotFound "League not found" |> Failure

    let getLeagueUnsafe (leagueId:LgId) =
        match tryFindLeagueByLeagueId leagueId with
        | Some league -> league
        | None -> failwith "League not found"
