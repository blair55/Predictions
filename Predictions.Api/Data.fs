namespace Predictions.Api

open System
open System.IO
open System.Data
open Npgsql

open Predictions.Api.Domain

module Data =

    let connStr =
        let uriString = "postgres://awisbnpt:51Y0ezlLiSkMZWqB86qaKjCP6_au2mNa@horton.elephantsql.com:5432/awisbnpt"
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
    
    type SeasonDto = { id:Guid; year:string; }
    type GameWeekDto = { id:Guid; seasonId:Guid; number:int; description:string; }
    type FixtureDto = { id:Guid; gameWeekId:Guid; home:string; away:string; kickoff:DateTime }
    type ResultDto = { id:Guid; fixtureId:Guid; homeScore:int; awayScore:int }
    type PredictionDto = { id:Guid; fixtureId:Guid; playerId:Guid; homeScore:int; awayScore:int }
    type PlayerDto = { id:Guid; name:string; role:string; email:string }
    
    let str o = o.ToString()
    let roleToString r = match r with | User -> "User" | Admin -> "Admin"
    let stringToRole s = match s with | "User" -> User | "Admin" -> Admin | _ -> User

    let kostr (k:DateTime) = String.Format("{0:u}", k)

    let insertSeasonQuery (s:SeasonDto) = sprintf "insert into seasons values ('%s', '%s')" (str s.id) s.year
    let insertGameWeekQuery (g:GameWeekDto) = sprintf "insert into gameweeks values ('%s', '%s', %i, '%s')" (str g.id) (str g.seasonId) g.number g.description
    let insertFixtureQuery (f:FixtureDto) = sprintf "insert into fixtures values ('%s', '%s', '%s', '%s', '%s')" (str f.id) (str f.gameWeekId) f.home f.away (kostr f.kickoff)
    let insertResultQuery (r:ResultDto) = sprintf "insert into results values ('%s', '%s', %i, %i)" (str r.id) (str r.fixtureId) (r.homeScore) (r.awayScore)
    let insertPredictionQuery (p:PredictionDto) = sprintf "insert into predictions values ('%s', '%s', %i, %i, '%s')" (str p.id) (str p.fixtureId) (p.homeScore) (p.awayScore) (str p.playerId)
    let insertPlayerQuery (p:PlayerDto) = sprintf "insert into players values ('%s', '%s', '%s', '%s')" (str p.id) p.name p.role p.email
    
    let getSeasonDto (s:Season) = { SeasonDto.id=s.id|>getSnId; year=s.year; }
    let getGameWeekDto (g:GameWeek) snid = { GameWeekDto.id=g.id|>getGwId; number=g.number|>getGameWeekNo; seasonId=snid|>getSnId; description=g.description; }
    let getFixtureDto (f:FixtureData) gwid = { FixtureDto.id=f.id|>getFxId; gameWeekId=gwid|>getGwId; home=f.home; away=f.away; kickoff=f.kickoff }
    let getResultDto (r:Result) fxid = { ResultDto.id=r.id|>getRsId; fixtureId=fxid|>getFxId; homeScore=(fst r.score); awayScore=(snd r.score); }
    let getPredictionDto (p:Prediction) fxid = { PredictionDto.id=p.id|>getPrId; fixtureId=fxid|>getFxId; playerId=(getPlayerId p.player.id); homeScore=(fst p.score); awayScore=(snd p.score); }
    let getPlayerDto (p:Player) = { PlayerDto.id=p.id|>getPlayerId; name=(p.name); role=(roleToString p.role); email="" }

    let addSeason s = getSeasonDto s |> insertSeasonQuery |> executeNonQuery
    let addGameWeek g snid = getGameWeekDto g snid |> insertGameWeekQuery |> executeNonQuery
    let addFixture f gwid = getFixtureDto f gwid |> insertFixtureQuery |> executeNonQuery
    let addResult r fxid = getResultDto r fxid |> insertResultQuery |> executeNonQuery
    let addPrediction p fxid = getPredictionDto p fxid |> insertPredictionQuery |> executeNonQuery
    let addPlayer pl = getPlayerDto pl |> insertPlayerQuery |> executeNonQuery

    // reading
    
    let readGuidAtPosition (r:NpgsqlDataReader) i = r.GetGuid(i)
    let readStringAtPosition (r:NpgsqlDataReader) i = r.GetString(i)
    let readIntAtPosition (r:NpgsqlDataReader) i = r.GetInt32(i)
    let readDateTimeAtPosition (r:NpgsqlDataReader) i = r.GetDateTime(i)    
    
    let readerToSeasonDto r = { SeasonDto.id = (readGuidAtPosition r 0); year=(readStringAtPosition r 1) }
    let readerToGameWeekDto (r) = { GameWeekDto.id = (readGuidAtPosition r 0); number=(readIntAtPosition r 1); seasonId=(readGuidAtPosition r 2); description=(readStringAtPosition r 3) }
    let readerToFixtureDto (r) = { FixtureDto.id = (readGuidAtPosition r 0); gameWeekId=(readGuidAtPosition r 1); home=(readStringAtPosition r 2); away=(readStringAtPosition r 3); kickoff=(readDateTimeAtPosition r 4) }
    let readerToResultDto (r) = { ResultDto.id = (readGuidAtPosition r 0); fixtureId = (readGuidAtPosition r 1); homeScore=(readIntAtPosition r 2); awayScore=(readIntAtPosition r 3) }
    let readerToPredictionDto (r) = { PredictionDto.id = (readGuidAtPosition r 0); fixtureId = (readGuidAtPosition r 1); homeScore=(readIntAtPosition r 2); awayScore=(readIntAtPosition r 3); playerId=(readGuidAtPosition r 4) }
    let readerToPlayerDto (r) = { PlayerDto.id = (readGuidAtPosition r 0); name=(readStringAtPosition r 1); role=(readStringAtPosition r 2); email=(readStringAtPosition r 3) }
    
    let readPlayers() = (executeQuery "select * from players" readerToPlayerDto)
    let readResults() = (executeQuery "select * from results" readerToResultDto)
    let readPredictions() = (executeQuery "select * from predictions" readerToPredictionDto)
    let readFixtures() = (executeQuery "select * from fixtures" readerToFixtureDto)
    let readGameWeeks() = (executeQuery "select * from gameweeks" readerToGameWeekDto)
    let readSeasons() = (executeQuery "select * from seasons" readerToSeasonDto)

    let toPlayer (p:PlayerDto) = { Player.id=PlId p.id; name=p.name; role=(stringToRole p.role) }
    let toResult (r:ResultDto) = { Result.id=RsId r.id; score=(r.homeScore, r.awayScore) }
    let toPrediction (p:PredictionDto) player = { Prediction.id=PrId p.id; score=(p.homeScore, p.awayScore); player=player }
    let toFixture (f:FixtureDto) predictions = { FixtureData.id=FxId f.id; home=f.home; away=f.away; kickoff=f.kickoff; predictions=predictions }
    let toGameWeek (gw:GameWeekDto) fixtures = { GameWeek.id=gw.id|>GwId; number=gw.number|>GwNo; description=gw.description; fixtures=fixtures }
    let toSeason (s:SeasonDto) gameWeeks = { Season.id=SnId s.id; year=s.year; gameWeeks=gameWeeks }
    
    let buildSeason year =

        let snDtos = readSeasons()
        let gwDtos = readGameWeeks()
        let fxDtos = readFixtures()
        let prDtos = readPredictions()
        let rsDtos = readResults()
        let plDtos = readPlayers()

        let sndOption a =
            match a with
            | Some (_, b) -> Some b
            | None -> None

        let playerPairs = plDtos |> List.map(fun plDto -> plDto, toPlayer plDto)
        let findPlayerById id = playerPairs |> List.find(fun (dto, model) -> dto.id = id) |> snd
        let resultPairs = rsDtos |> List.map(fun rsDto -> rsDto, toResult rsDto)
        let tryFindResultForFixture fxid = resultPairs |> List.tryFind(fun (dto, model) -> dto.fixtureId = fxid) |> sndOption
        let predictionPairs = prDtos |> List.map(fun prDto -> prDto,toPrediction prDto (findPlayerById prDto.playerId))
        let findPredictionsForFixture fxid = predictionPairs |> List.filter(fun (dto, _) -> dto.fixtureId = fxid) |> List.map(fun (_, model) -> model)
        let fixturePairs = fxDtos |> List.map(fun fxDto -> fxDto, toFixture fxDto (findPredictionsForFixture fxDto.id))
        let findFixturesForGameWeek gwid = fixturePairs |> List.filter(fun (dto, _) -> dto.gameWeekId = gwid) |> List.map(fun (_, fd) -> fixtureDataToFixture fd (tryFindResultForFixture (getFxId fd.id)))
        let gameWeekPairs = gwDtos |> List.map(fun gwDto -> gwDto, toGameWeek gwDto (findFixturesForGameWeek gwDto.id))
        let findGameWeeksForSeason snid = gameWeekPairs |> List.filter(fun (dto, _) -> dto.seasonId = snid) |> List.map(fun (_, model) -> model)
        let seasonPairs = snDtos |> List.map(fun snDto -> snDto, toSeason snDto (findGameWeeksForSeason snDto.id))

        seasonPairs |> List.find(fun (_, model) -> model.year = year) |> snd
        

    let getPlayers() = readPlayers() |> List.map(toPlayer)


    let getPlayerById playerId =
        readPlayers() |> List.tryFind(fun p -> p.id = playerId)

    let getNewGameWeekNo() =
        let readIntAt0 r = readIntAtPosition r 0
        let gwn = getFirst (executeQuery "select number from gameweeks ORDER BY number DESC LIMIT 1" readIntAt0)
        match gwn with
        | Some n -> n+1
        | None -> 1

//    let getPlayersAndResultsAndPredictions() =
//        let players = readPlayers()
//        let gameWeeks = readGameWeeks()
//        let fixtures = readFixtures gameWeeks
//        let results = readResults fixtures
//        let predictions = readPredictions players fixtures
//        players, results, predictions
//
//    let getGameWeeks() = readGameWeeks()
//
//    let getGameWeeksAndFixtures() =
//        let gameWeeks = readGameWeeks()
//        let fixtures = readFixtures gameWeeks
//        gameWeeks, fixtures
//
//    let getGameWeeksAndFixturesAndResults() =
//        let gameWeeks = readGameWeeks()
//        let fixtures = readFixtures gameWeeks
//        let results = readResults fixtures
//        gameWeeks, fixtures, results

    