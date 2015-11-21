namespace Predictions.Api

open System
open System.Data.SqlClient
open System.Configuration
open Dapper
open Predictions.Api.Domain


module Data =
    
    let connString = ConfigurationManager.AppSettings.["SQLSERVER_CONNECTION_STRING"]
//    let connString = "Data Source=.\SQLEXPRESS;Initial Catalog=Predictions;Integrated Security=True;"
    let newConn() = new SqlConnection(connString)
    let nonQuery sql args =
        use conn = newConn()
        conn.Execute(sql, args) |> ignore

    type RegisterPlayerCommand = { player:Player; explid:ExternalPlayerId; exProvider:ExternalLoginProvider; email:string }
    type RegisterPlayerCommandArgs = { id:Guid; externalId:string; externalProvider:string; name:string; email:string }
    let registerPlayerInDb (cmd:RegisterPlayerCommand) =
        nonQuery @"insert into Players(PlayerId, ExternalLoginId, ExternalLoginProvider, PlayerName, IsAdmin, Email) values (@id, @externalId, @externalProvider, @Name, 0, @email)"
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

    type UpdateUserNameCommand = { playerId:PlId; playerName:PlayerName }
    type UpdateUserNameCommandArgs = { playerId:Guid; playerName:string }
    let updateUserNameInDb (cmd:UpdateUserNameCommand) =
        nonQuery @"UPDATE Players SET PlayerName = @playerName WHERE PlayerId = @playerId"
            { UpdateUserNameCommandArgs.playerId=cmd.playerId|>getPlayerId; playerName=cmd.playerName|>getPlayerName }
            
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
        select @nextGameWeek = max(gameweeknumber) + 1 from gameweeks where seasonid = @SeasonId
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
    type [<CLIMutable>] BuildSeasonQueryResult = { seasonId:Guid; seasonYear:string }
    type [<CLIMutable>] BuildGameWeekQueryResult = { gameWeekId:Guid; seasonId:Guid; gameWeekNumber:int; gameWeekDescription:string }
    type [<CLIMutable>] BuildFixtureQueryResult = { fixtureId:Guid; gameWeekId:Guid; kickoff:DateTime; homeTeamName:string; awayTeamName:string; homeTeamScore:Nullable<int>; awayTeamScore:Nullable<int> }
    let buildSeason (year:SnYr) =
        let args = { BuildSeasonQueryArgs.seasonYear=year|>getSnYr; }
        let sql = @"select sns.seasonId, sns.seasonYear
                    from seasons sns
                    where sns.SeasonYear = @seasonYear
                    select gws.gameWeekId, gws.seasonId, gws.gameWeekNumber, gws.gameWeekDescription
                    from seasons sns
                    join gameWeeks gws on sns.SeasonId = gws.SeasonId
                    where sns.SeasonYear = @seasonYear
                    select fxs.fixtureId, fxs.gameWeekId, fxs.kickoff, fxs.homeTeamName, fxs.awayTeamName, fxs.homeTeamScore, fxs.awayTeamScore
                    from seasons sns
                    join gameWeeks gws on sns.SeasonId = gws.SeasonId
                    join fixtures fxs on gws.GameWeekId = fxs.GameWeekId
                    where sns.SeasonYear = @seasonYear"
        use conn = newConn()
        let multi = conn.QueryMultiple(sql, args)
        let seasonResult = multi.Read<BuildSeasonQueryResult>() |> Seq.head
        let gameWeeksResult = multi.Read<BuildGameWeekQueryResult>()
        let fixturesResult = multi.Read<BuildFixtureQueryResult>()
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
    
    type LeagueIdsPlayerIsInQueryArgs = { playerId:Guid }
    type [<CLIMutable>] LeagueIdsPlayerIsInQueryResult = { leagueId:Guid }
    let getLeagueIdsThatPlayerIsIn (player:Player) =
        let args = { LeagueIdsPlayerIsInQueryArgs.playerId=player.id|>getPlayerId }
        let sql = @"select lgs.leagueId
                    from leagues lgs
                    join leaguePlayerBridge lpb on lgs.leagueId = lpb.leagueId
                    where lpb.playerId = @playerId
                    and lgs.LeagueIsDeleted = 0"
        use conn = newConn()
        let result = conn.Query<LeagueIdsPlayerIsInQueryResult>(sql, args)
        result |> Seq.map(fun r -> r.leagueId|>LgId) |> Seq.toArray

    type FindPlayerByPlayerIdQueryArgs = { playerId:Guid }
    type [<CLIMutable>] PlayersTableQueryResult = { playerId:Guid; playerName:string; isAdmin:bool }
    let queryResultToPlayer (player:PlayersTableQueryResult) predictions =
        { Player.id=player.playerId|>PlId; name=player.playerName|>PlayerName; predictions=predictions; isAdmin=player.isAdmin }
    type [<CLIMutable>] PredictionsTableQueryResult = { predictionId:Guid; fixtureId:Guid; playerId:Guid; homeTeamScore:int; awayTeamScore:int; doubleDown:int; created:DateTime }
    let queryResultToPrediction (p:PredictionsTableQueryResult) =
        let modifier = match p.doubleDown with | 1 -> DoubleDown | _ -> NoModifier
        { Prediction.id=p.predictionId|>PrId; fixtureId=p.fixtureId|>FxId; playerId=p.playerId|>PlId; score=(p.homeTeamScore, p.awayTeamScore); modifier=modifier }
    let tryFindPlayerByPlayerId playerId =
        let args = { FindPlayerByPlayerIdQueryArgs.playerId=playerId|>getPlayerId }
        let sql = @"select playerId, playerName, isAdmin from players where playerId = @playerId
                    select pds.predictionId, pds.fixtureId, pds.playerId, pds.homeTeamScore, pds.awayTeamScore, pds.created,
                        case when dd.predictionid is null then 0 else 1 end as DoubleDown
                    from predictions pds
                    left outer join DoubleDowns dd on dd.PlayerId = @playerId and pds.PredictionId = dd.PredictionId
                    where pds.playerId = @playerId"
        use conn = newConn()
        let multi = conn.QueryMultiple(sql, args)
        let playersResult = multi.Read<PlayersTableQueryResult>() |> Seq.toArray
        if playersResult |> Array.isEmpty then None
        else
            let predictions =
                multi.Read<PredictionsTableQueryResult>()
                |> Seq.map queryResultToPrediction
                |> Seq.toArray
            let player = playersResult.[0]
            Some (queryResultToPlayer player predictions)

    type FindPlayerByExternalIdQueryArgs = { externalId:string; externalProvider:string }
    let tryFindPlayerByExternalId externalPlayerId externalLoginProvider =
        let args = { FindPlayerByExternalIdQueryArgs.externalId=externalPlayerId|>getExternalPlayerId; externalProvider=externalLoginProvider|>getExternalLoginProvider }
        let sql = @"select playerId, playerName, isAdmin from players where ExternalLoginId = @externalId and ExternalLoginProvider = @externalProvider"
        use conn = newConn()
        let result = conn.Query<PlayersTableQueryResult>(sql, args) |> Seq.toArray
        if result |> Array.isEmpty then None
        else
            let player = result.[0]
            Some (queryResultToPlayer player Array.empty)

    type FindLeagueByLeagueIdQueryArgs = { leagueId:Guid }
    type [<CLIMutable>] LeaguesTableQueryResult = { leagueId:Guid; leagueName:string; leagueAdminId:Guid }
    type [<CLIMutable>] PlayersTableWithLeagueJoinDateQueryResult = { playerId:Guid; playerName:string; isAdmin:bool; leagueJoinDate:DateTime }
    let tryFindLeagueByLeagueId leagueId =
        let args = { FindLeagueByLeagueIdQueryArgs.leagueId=leagueId|>getLgId }
        let sql = @"select lgs.LeagueId, lgs.LeagueName, lgs.LeagueAdminId
                    from Leagues lgs
                    where lgs.LeagueId = @leagueId
                    and lgs.LeagueIsDeleted = 0

                    select pls.PlayerId, pls.PlayerName, pls.IsAdmin, lpb.Created as LeagueJoinDate
                    from Players pls
                    join LeaguePlayerBridge lpb on pls.PlayerId = lpb.PlayerId
                    where lpb.LeagueId = @leagueId
                    
                    select pds.PredictionId, pds.FixtureId, pds.PlayerId, pds.HomeTeamScore, pds.AwayTeamScore, pds.created,
                        case when dd.predictionid is null then 0 else 1 end as DoubleDown
                    from Predictions pds
                    join Players pls on pds.PlayerId = pls.PlayerId
                    join LeaguePlayerBridge lpb on pls.PlayerId = lpb.PlayerId
                    left outer join DoubleDowns dd on lpb.PlayerId = dd.PlayerId and pds.PredictionId = dd.PredictionId
                    where lpb.LeagueId = @leagueId"
        use conn = newConn()
        let multi = conn.QueryMultiple(sql, args)
        let leagueResult = multi.Read<LeaguesTableQueryResult>() |> Seq.toArray
        if leagueResult |> Array.isEmpty then None
        else
            let playersResult = multi.Read<PlayersTableWithLeagueJoinDateQueryResult>() |> Seq.toArray
            let predictionsResult = multi.Read<PredictionsTableQueryResult>() |> Seq.toArray
            let players =
                let getPlayerPredictionsSinceJoinedLeague p =
                    predictionsResult
                    |> Array.filter(fun pr -> pr.playerId = p.playerId)
                    |> Array.filter(fun pr -> pr.created > p.leagueJoinDate)
                    |> Array.map queryResultToPrediction
                playersResult
                |> Array.map(fun p -> { Player.id=p.playerId|>PlId; name=p.playerName|>PlayerName; predictions=p|>getPlayerPredictionsSinceJoinedLeague; isAdmin=p.isAdmin })
            let league = leagueResult.[0]
            Some { League.id=league.leagueId|>LgId; name=league.leagueName|>LeagueName; players=players; adminId=league.leagueAdminId|>PlId }
            
    type FindLeagueByShareableLeagueIdQueryArgs = { shareableLeagueId:string }
    let tryFindLeagueByShareableId shareableLeagueId =
        let args = { FindLeagueByShareableLeagueIdQueryArgs.shareableLeagueId=shareableLeagueId }
        let sql = @"select LeagueId, LeagueName, LeagueAdminId from Leagues where LeagueShareableId = @shareableLeagueId and LeagueIsDeleted = 0"
        use conn = newConn()
        let result = conn.Query<LeaguesTableQueryResult>(sql, args) |> Seq.toArray
        if result |> Array.isEmpty then None
        else
            let league = result.[0]
            Some { League.id=league.leagueId|>LgId; name=league.leagueName|>LeagueName; players=Array.empty; adminId=league.leagueAdminId|>PlId }
    
    type GetAllPredictionsForFixtureQueryArgs = { fixtureId:Guid }
    let getAllPredictionsForFixture (fxid:FxId) =
        let args = { GetAllPredictionsForFixtureQueryArgs.fixtureId=fxid|>getFxId }
        let sql = @"select pds.predictionId, pds.fixtureId, pds.playerId, pds.homeTeamScore, pds.awayTeamScore, pds.created,
                        case when dd.predictionid is null then 0 else 1 end as DoubleDown
                    from predictions pds
                    left outer join DoubleDowns dd on dd.PlayerId = pds.playerId and pds.PredictionId = dd.PredictionId
                    where pds.fixtureId = @fixtureId"
        use conn = newConn()
        conn.Query<PredictionsTableQueryResult>(sql, args)
        |> Seq.map queryResultToPrediction
        |> Seq.toArray

    type GetFixturePreviousMeetingsArgs = { homeTeamName:string; awayTeamName:string }
    type [<CLIMutable>] GetFixturePreviousMeetingsQueryResult = { kickoff:DateTime; homeTeamName:string; awayTeamName:string; homeTeamScore:int; awayTeamScore:int; }
    let getFixturePreviousMeetingsData home away =
        let args = { GetFixturePreviousMeetingsArgs.homeTeamName=home; awayTeamName=away }
        let sql = @"SELECT kickoff, hometeamname, awayteamname, hometeamscore, awayteamscore FROM Fixtures
                    where hometeamscore is not null
                    and awayteamscore is not null
                    and ((hometeamname = @homeTeamName and awayteamname = @awayTeamName)
                          or (awayteamname = @homeTeamName and hometeamname = @awayTeamName))"
        use conn = newConn()
        conn.Query<GetFixturePreviousMeetingsQueryResult>(sql, args)
        |> Seq.map(fun r -> (r.kickoff, r.homeTeamName, r.awayTeamName, r.homeTeamScore, r.awayTeamScore))
        |> Seq.toArray

    let getAllPlayers() =
        let sql = @"select playerId, playerName, isAdmin from players
                    select pds.predictionId, pds.fixtureId, pds.playerId, pds.homeTeamScore, pds.awayTeamScore, pds.created,
                        case when dd.predictionid is null then 0 else 1 end as DoubleDown
                    from predictions pds
                    left outer join DoubleDowns dd on dd.PlayerId = pds.playerId and pds.PredictionId = dd.PredictionId"
        use conn = newConn()
        let multi = conn.QueryMultiple(sql)
        let playersResult = multi.Read<PlayersTableQueryResult>() |> Seq.toArray
        let predictions =
            multi.Read<PredictionsTableQueryResult>()
            |> Seq.map queryResultToPrediction
            |> Seq.toArray
        let getPredictionsForPlayerId plid =
            predictions
            |> Array.filter(fun pr -> pr.playerId = (plid|>PlId))
        playersResult
            |> Array.map(fun pl -> queryResultToPlayer pl (getPredictionsForPlayerId pl.playerId))

    let getPlayerByExternalLogin (explid:ExternalPlayerId, exprov:ExternalLoginProvider) =
        match tryFindPlayerByExternalId explid exprov with
        | Some p -> p |> Success
        | None -> NotFound "Player not found" |> Failure

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

    