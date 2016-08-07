#r "/Users/nblair/lab/Predictions/packages/FSharp.Data/lib/net40/FSharp.Data.dll"
open FSharp.Data
open System
open System.IO

let [<Literal>] Url = """http://football-data.co.uk/mmz4281/1516/E0.csv"""
type Results = CsvProvider<Url, IgnoreErrors=true>
let leagues = [ "E0"; "E1"; "E2"; "E3"; "EC" ]
let sqlFile = "/Users/nblair/lab/Predictions/Predictions.Sql/ResultScripts/r_001_Insert_Fixture_History_2015-16.sql" 
let buildUrl = sprintf "http://football-data.co.uk/mmz4281/1516/%s.csv" 
let getSeasonResults (s:string) = Results.Load(s)
let writeLine (writer:StreamWriter) (s:string) = writer.WriteLine(s)
let buildInsert (ko:DateTime) home away =
    let sko = ko.ToString("yyyy-MM-dd")
    let clean (s:string) = s.Replace("'", "''")
    sprintf "insert into Fixtures(FixtureId, GameWeekId, KickOff, HomeTeamName, AwayTeamName, HomeTeamScore, AwayTeamScore) values (newid(), 'BEB96D20-8765-4F24-80A0-8BA72FE02FC7', '%s', '%s', '%s', %i, %i);" sko (clean home) (clean away)
let rec writeToFile newLineCounter (writer:StreamWriter) (rows:Results.Row list) =
    match rows with
    | h::t ->
        if newLineCounter % 10 = 0 then writeLine writer "GO;"
        let ko = Convert.ToDateTime(h.Date)
        let insertLine = buildInsert ko h.HomeTeam h.AwayTeam h.FTHG h.FTAG 
        writeLine writer insertLine
        writeToFile (newLineCounter+1) writer t
    | _ ->
        writer.Close()
        writer.Dispose()

File.Delete(sqlFile)

leagues
|> Seq.map buildUrl
|> Seq.map getSeasonResults
|> Seq.collect (fun x -> x.Rows)
|> Seq.toList
|> writeToFile 1 (new StreamWriter(sqlFile))