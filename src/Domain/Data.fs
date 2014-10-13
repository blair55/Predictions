namespace PredictionsManager.Domain

open System
open System.IO
open System.Data
open Npgsql

open PredictionsManager.Domain.Domain

module Data =

    let connStr =
        let uriString = "postgres://iobxdsep:sQ5pYLEEjC8Ih6V3OVS_V8F0N67VQ-28@horton.elephantsql.com:5432/iobxdsep"
        let uri = new Uri(uriString)
        let db = uri.AbsolutePath.Trim('/')
        let user = uri.UserInfo.Split(':').[0]
        let passwd = uri.UserInfo.Split(':').[1]
        let port = if uri.Port > 0 then uri.Port else 5432
        String.Format("Server={0};Database={1};User Id={2};Password={3};Port={4}", uri.Host, db, user, passwd, port);
        

    let getConn() = new NpgsqlConnection(connStr)
    let getQuery cn s = new NpgsqlCommand(s, cn)

    let executeNonQuery nq =
        use cn = new NpgsqlConnection(connStr)
        cn.Open()
        let cmd = getQuery cn nq
        let sw = System.Diagnostics.Stopwatch.StartNew()
        cmd.ExecuteNonQuery() |> ignore
        sw.Stop()
        sprintf "%i / %s" sw.ElapsedMilliseconds nq |> log
        cn.Close()
        
    let rec getListFromReader (r:NpgsqlDataReader) readerToTypeStrategy list =
        match r.Read() with
        | true -> let item = readerToTypeStrategy r
                  let newList = item::list
                  getListFromReader r readerToTypeStrategy newList
        | false -> list

    let executeQuery q readerToTypeStrategy =
        use cn = getConn()
        cn.Open()
        let cmd = getQuery cn q
        let sw = System.Diagnostics.Stopwatch.StartNew()
        let reader = cmd.ExecuteReader()
        sw.Stop()
        sprintf "%s / %i" q sw.ElapsedMilliseconds |> log
        let results = getListFromReader reader readerToTypeStrategy []
        cn.Close()
        results

    // writing
    
    type PlayerDto = { id:Guid; name:string; role:string; email:string }
    type GameWeekDto = { id:Guid; number:int; description:string; }
    type FixtureDto = { id:Guid; gameWeekId:Guid; home:string; away:string; kickoff:DateTime }
    type ResultDto = { fixtureId:Guid; homeScore:int; awayScore:int }
    type PredictionDto = { fixtureId:Guid; playerId:Guid; homeScore:int; awayScore:int }
    
    let str o = o.ToString()
    let roleToString r = match r with | User -> "User" | Admin -> "Admin"
    let stringToRole s = match s with | "User" -> User | "Admin" -> Admin | _ -> User

    let kostr (k:DateTime) =
        let c = String.Format("{0:u}", k)
        c

    let insertPlayerQuery (p:PlayerDto) = sprintf "insert into players values ('%s', '%s', '%s', '%s')" (str p.id) p.name p.role p.email
    let insertGameWeekQuery (g:GameWeekDto) = sprintf "insert into gameweeks values ('%s', %i, '%s')" (str g.id) g.number g.description
    let insertFixtureQuery (f:FixtureDto) = sprintf "insert into fixtures values ('%s', '%s', '%s', '%s', '%s')" (str f.id) (str f.gameWeekId) f.home f.away (kostr f.kickoff)
    let insertResultQuery (r:ResultDto) = sprintf "insert into results values ('%s', %i, %i)" (str r.fixtureId) (r.homeScore) (r.awayScore)
    let insertPredictionQuery (p:PredictionDto) = sprintf "insert into predictions values ('%s', %i, %i, '%s')" (str p.fixtureId) (p.homeScore) (p.awayScore) (str p.playerId)
    
    let getPlayerDto (p:Player) = { PlayerDto.id=(getPlayerId p.id); name=(p.name); role=(roleToString p.role); email="" }
    let getGameWeekDto (g:GameWeek) = { GameWeekDto.id=(getGwId g.id); number=(getGameWeekNo g.number); description=g.description; }
    let getFixtureDto (f:Fixture) = { FixtureDto.id=(getFxId f.id); gameWeekId=(getGwId f.gameWeek.id); home=f.home; away=f.away; kickoff=f.kickoff }
    let getResultDto (r:Result) = { ResultDto.fixtureId=(getFxId r.fixture.id); homeScore=(fst r.score); awayScore=(snd r.score); }
    let getPredictionDto (p:Prediction) = { PredictionDto.fixtureId=(getFxId p.fixture.id); playerId=(getPlayerId p.player.id); homeScore=(fst p.score); awayScore=(snd p.score); }

    let addPlayer pl = getPlayerDto pl |> insertPlayerQuery |> executeNonQuery
    let addGameWeek g = getGameWeekDto g |> insertGameWeekQuery |> executeNonQuery
    let addFixture f = getFixtureDto f |> insertFixtureQuery |> executeNonQuery
    let addResult r = getResultDto r |> insertResultQuery |> executeNonQuery
    let addPrediction p = getPredictionDto p |> insertPredictionQuery |> executeNonQuery

    // reading
    
    let readGuidAtPosition (r:NpgsqlDataReader) i = r.GetGuid(i)
    let readStringAtPosition (r:NpgsqlDataReader) i = r.GetString(i)
    let readIntAtPosition (r:NpgsqlDataReader) i = r.GetInt32(i)
    let readDateTimeAtPosition (r:NpgsqlDataReader) i = r.GetDateTime(i)    
    
    let readerToPlayerDto (r:NpgsqlDataReader) =
        { PlayerDto.id = (readGuidAtPosition r 0); name=(readStringAtPosition r 1); role=(readStringAtPosition r 2); email=(readStringAtPosition r 3) }

    let readerToGameWeekDto (r:NpgsqlDataReader) =
        { GameWeekDto.id = (readGuidAtPosition r 0); number=(readIntAtPosition r 1); description=(readStringAtPosition r 2) }
    
    let readerToFixtureDto (r:NpgsqlDataReader) =
        { FixtureDto.id = (readGuidAtPosition r 0); gameWeekId=(readGuidAtPosition r 1); home=(readStringAtPosition r 2); away=(readStringAtPosition r 3); kickoff=(readDateTimeAtPosition r 4) }
    
    let readerToResultDto (r:NpgsqlDataReader) =
        { ResultDto.fixtureId = (readGuidAtPosition r 0); homeScore=(readIntAtPosition r 1); awayScore=(readIntAtPosition r 2) }
        
    let readerToPredictionDto (r:NpgsqlDataReader) =
        { PredictionDto.fixtureId = (readGuidAtPosition r 0); homeScore=(readIntAtPosition r 1); awayScore=(readIntAtPosition r 2); playerId=(readGuidAtPosition r 3) }
    
    
    let readPlayers () =
        (executeQuery "select * from players" readerToPlayerDto)
        |> List.map(fun p -> { Player.id=p.id|>PlId; name=p.name; role=(stringToRole p.role) })

    let readGameWeeks() =
        (executeQuery "select * from gameweeks" readerToGameWeekDto)
        |> List.map(fun gw -> { GameWeek.id=gw.id|>GwId; number=gw.number|>GwNo; description=gw.description })
    
    let readFixtures (gameWeeks:GameWeek list) =
        (executeQuery "select * from fixtures" readerToFixtureDto)
        |> List.map(fun f -> { Fixture.id=f.id|>FxId; gameWeek=(findGameWeekById gameWeeks (f.gameWeekId|>GwId)); home=f.home; away=f.away; kickoff=f.kickoff })
        
    let readResults (fixtures:Fixture list) =
        (executeQuery "select * from results" readerToResultDto)
        |> List.map(fun r -> { Result.fixture=(findFixtureById fixtures (r.fixtureId|>FxId)); score=(r.homeScore, r.awayScore) })
        
    let readPredictions (players:Player list) (fixtures:Fixture list) =
        (executeQuery "select * from predictions" readerToPredictionDto)
        |> List.map(fun r -> { Prediction.fixture=(findFixtureById fixtures (r.fixtureId|>FxId)); score=(r.homeScore, r.awayScore); player=(findPlayerById players (r.playerId|>PlId)) })
    
    let getPlayersAndResultsAndPredictions() =
        let players = readPlayers()
        let gameWeeks = readGameWeeks()
        let fixtures = readFixtures gameWeeks
        let results = readResults fixtures
        let predictions = readPredictions players fixtures
        players, results, predictions

    let getPlayerById playerId =
        readPlayers() |> List.tryFind(fun p -> p.id = playerId)

    let getNewGameWeekNo() =
        let readIntAt0 r = readIntAtPosition r 0
        let gwn = getFirst (executeQuery "select number from gameweeks ORDER BY number DESC LIMIT 1" readIntAt0)
        match gwn with
        | Some n -> n+1
        | None -> 1

    let getGameWeeksAndFixtures() =
        let gameWeeks = readGameWeeks()
        let fixtures = readFixtures gameWeeks
        gameWeeks, fixtures

    let getGameWeeksAndFixturesAndResults() =
        let gameWeeks = readGameWeeks()
        let fixtures = readFixtures gameWeeks
        let results = readResults fixtures
        gameWeeks, fixtures, results

    let getPlayers() = readPlayers()
    