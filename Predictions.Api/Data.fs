namespace Predictions.Api

open System
open System.IO
open System.Data
open System.Configuration
open Npgsql
open Predictions.Api.Domain

module Data =

    let connStr =
        let s = ConfigurationManager.AppSettings.["ELEPHANTSQL_URL"]
        let uriString = if String.IsNullOrEmpty(s) then "postgres://vagrant:password@127.0.0.1:5433/vagrant" else s
        let uri = new Uri(uriString)
        let db = uri.AbsolutePath.Trim('/')
        let user = uri.UserInfo.Split(':').[0]
        let passwd = uri.UserInfo.Split(':').[1]
        let port = if uri.Port > 0 then uri.Port else 5432
        String.Format("Server={0};Database={1};User Id={2};Password={3};Port={4}", uri.Host, db, user, passwd, port);
       
    //let getConn() = new NpgsqlConnection("Server=127.0.0.1;Port=5433;User Id=vagrant; Password=password; Database=vagrant;")
    let getConn() = new NpgsqlConnection(connStr)
    let getQuery cn s = new NpgsqlCommand(s, cn)

    let executeNonQuery nq =
        use cn = getConn()
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
    type PlayerDto = { id:Guid; name:string; role:string; email:string; authToken:string }
    
    type SaveSeasonCommand = { id:SnId; year:SnYr; }
    type SaveGameWeekCommand = { id:GwId; seasonId:SnId; description:string; fixtures:FixtureData list }
    type SaveResultCommand = { id:RsId; fixtureId:FxId; score:Score }
    type SavePredictionCommand = { id:PrId; fixtureId:FxId; playerId:PlId; score:Score }
    type SavePlayerCommand = { id:PlId; name:string; role:Role; email:string; authToken:string }
    type SaveFixtureCommand = { id:FxId; gameWeekId:GwId; home:Team; away:Team; ko:KickOff }
    
    let roleToString r = match r with | User -> "User" | Admin -> "Admin"
    let stringToRole s = match s with | "User" -> User | "Admin" -> Admin | _ -> User
    let kostr (k:DateTime) = String.Format("{0:u}", k)

    let getInsertSeasonQuery (s:SeasonDto) = sprintf "insert into seasons values ('%s', '%s')" (str s.id) s.year
    let getInsertGameWeekQuery (g:GameWeekDto) = sprintf "insert into gameweeks (id, seasonId, description) values ('%s', '%s', '%s')" (str g.id) (str g.seasonId) g.description
    let getInsertFixtureQuery (f:FixtureDto) = sprintf "insert into fixtures values ('%s', '%s', '%s', '%s', '%s')" (str f.id) (str f.gameWeekId) f.home f.away (kostr f.kickoff)
    let getInsertResultQuery (r:ResultDto) = sprintf "insert into results values ('%s', '%s', %i, %i)" (str r.id) (str r.fixtureId) (r.homeScore) (r.awayScore)
    let getInsertPredictionQuery (p:PredictionDto) = sprintf "insert into predictions values ('%s', '%s', %i, %i, '%s')" (str p.id) (str p.fixtureId) (p.homeScore) (p.awayScore) (str p.playerId)
    let getInsertPlayerQuery (p:PlayerDto) = sprintf "insert into players values ('%s', '%s', '%s', '%s', '%s')" (str p.id) p.name p.role p.email p.authToken
    let getDeletePredictionQuery (p:PredictionDto) = sprintf "delete from predictions where fixtureId = '%s' and playerId = '%s'" (str p.fixtureId) (str p.playerId)
    let getDeleteResultQuery (p:ResultDto) = sprintf "delete from results where fixtureId = '%s'" (str p.fixtureId)

    let getSeasonDto (cmd:SaveSeasonCommand) = { SeasonDto.id=cmd.id|>getSnId; year=cmd.year|>getSnYr; }
    let getGameWeekDto (cmd:SaveGameWeekCommand) = { GameWeekDto.id=cmd.id|>getGwId; number=0(*cmd.number|>getGameWeekNo*); seasonId=cmd.seasonId|>getSnId; description=cmd.description; }
    let getFixtureDto (cmd:SaveFixtureCommand) = { FixtureDto.id=cmd.id|>getFxId; gameWeekId=cmd.gameWeekId|>getGwId; home=cmd.home; away=cmd.away; kickoff=cmd.ko }
    let getResultDto (cmd:SaveResultCommand) = { ResultDto.id=cmd.id|>getRsId; fixtureId=cmd.fixtureId|>getFxId; homeScore=(fst cmd.score); awayScore=(snd cmd.score); }
    let getPredictionDto (cmd:SavePredictionCommand) = { PredictionDto.id=cmd.id|>getPrId; fixtureId=cmd.fixtureId|>getFxId; playerId=cmd.playerId|>getPlayerId; homeScore=(fst cmd.score); awayScore=(snd cmd.score); }
    let getPlayerDto (cmd:SavePlayerCommand) = { PlayerDto.id=cmd.id|>getPlayerId; name=(cmd.name); role=(roleToString cmd.role); email=cmd.email; authToken=cmd.authToken }

    let savePlayer (cmd:SavePlayerCommand) = cmd |> getPlayerDto |> getInsertPlayerQuery |> executeNonQuery
    let saveSeason (cmd:SaveSeasonCommand) = cmd |> getSeasonDto |> getInsertSeasonQuery |> executeNonQuery
    let saveResult (cmd:SaveResultCommand) =
        let rdto = cmd |> getResultDto
        rdto |> getDeleteResultQuery |> executeNonQuery
        rdto |> getInsertResultQuery |> executeNonQuery
    let savePrediction (cmd:SavePredictionCommand) =
        let pdto = cmd |> getPredictionDto
        pdto |> getDeletePredictionQuery |> executeNonQuery
        pdto |> getInsertPredictionQuery |> executeNonQuery
    let saveGameWeek (cmd:SaveGameWeekCommand) =
        let saveFixtureCommands = cmd.fixtures |> List.map(fun fd -> { SaveFixtureCommand.id=fd.id; gameWeekId=cmd.id; home=fd.home; away=fd.away; ko=fd.kickoff })
        cmd |> getGameWeekDto |> getInsertGameWeekQuery |> fun igwq -> printfn "%s" |> ignore; igwq |> executeNonQuery
        saveFixtureCommands |> List.iter(fun fd -> fd |> getFixtureDto |> getInsertFixtureQuery |> executeNonQuery)


    // reading
    
    let readGuidAtPosition (r:NpgsqlDataReader) i = r.GetGuid(i)
    let readStringAtPosition (r:NpgsqlDataReader) i = r.GetString(i)
    let readIntAtPosition (r:NpgsqlDataReader) i = r.GetInt32(i)
    let readDateTimeAtPosition (r:NpgsqlDataReader) i = r.GetDateTime(i)    
    
    let readerToSeasonDto r = { SeasonDto.id = (readGuidAtPosition r 0); year=(readStringAtPosition r 1) }
    let readerToGameWeekDto (r) = { GameWeekDto.id = (readGuidAtPosition r 0); seasonId=(readGuidAtPosition r 1); number=(readIntAtPosition r 2); description=(readStringAtPosition r 3) }
    let readerToFixtureDto (r) = { FixtureDto.id = (readGuidAtPosition r 0); gameWeekId=(readGuidAtPosition r 1); home=(readStringAtPosition r 2); away=(readStringAtPosition r 3); kickoff=(readDateTimeAtPosition r 4) }
    let readerToResultDto (r) = { ResultDto.id = (readGuidAtPosition r 0); fixtureId = (readGuidAtPosition r 1); homeScore=(readIntAtPosition r 2); awayScore=(readIntAtPosition r 3) }
    let readerToPredictionDto (r) = { PredictionDto.id = (readGuidAtPosition r 0); fixtureId = (readGuidAtPosition r 1); homeScore=(readIntAtPosition r 2); awayScore=(readIntAtPosition r 3); playerId=(readGuidAtPosition r 4) }
    let readerToPlayerDto (r) = { PlayerDto.id = (readGuidAtPosition r 0); name=(readStringAtPosition r 1); role=(readStringAtPosition r 2); email=(readStringAtPosition r 3); authToken=(readStringAtPosition r 4) }
    
    let readPlayers() = (executeQuery "select * from players" readerToPlayerDto)
    let readResults() = (executeQuery "select * from results" readerToResultDto)
    let readPredictions() = (executeQuery "select * from predictions" readerToPredictionDto)
    let readFixtures() = (executeQuery "select * from fixtures" readerToFixtureDto)
    let readGameWeeks() = (executeQuery "select * from gameweeks" readerToGameWeekDto)
    let readSeasons() = (executeQuery "select * from seasons" readerToSeasonDto)

    let toPlayer (p:PlayerDto) = { Player.id=PlId p.id; name=p.name; role=(stringToRole p.role); authToken=p.authToken }
    let toResult (r:ResultDto) = { Result.id=RsId r.id; score=(r.homeScore, r.awayScore) }
    let toPrediction (p:PredictionDto) player = { Prediction.id=PrId p.id; score=(p.homeScore, p.awayScore); player=player }
    let toFixture (f:FixtureDto) predictions = { FixtureData.id=FxId f.id; home=f.home; away=f.away; kickoff=f.kickoff; predictions=predictions }
    let toGameWeek (gw:GameWeekDto) fixtures = { GameWeek.id=gw.id|>GwId; number=gw.number|>GwNo; description=gw.description; fixtures=fixtures }
    let toSeason (s:SeasonDto) gameWeeks = { Season.id=SnId s.id; year=SnYr s.year; gameWeeks=gameWeeks }
    
    let buildSeason year =

        let snDtos = readSeasons()
        let gwDtos = readGameWeeks()
        let fxDtos = readFixtures()
        let prDtos = readPredictions()
        let rsDtos = readResults()
        let plDtos = readPlayers()

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
        

    let getPlayers() = readPlayers() |> List.map(toPlayer) |> List.sortBy(fun p -> p.name)


    let getPlayerById playerId =
        readPlayers() |> List.tryFind(fun p -> p.id = playerId)

    