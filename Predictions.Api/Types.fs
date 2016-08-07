namespace Predictions.Api

open System

[<AutoOpen>]
module Types =

    type LgId = LgId of Guid
    type FxId = FxId of Guid
    type GwId = GwId of Guid
    type PlId = PlId of Guid
    type PrId = PrId of Guid
    type SnId = SnId of Guid
    type GwNo = GwNo of int
    type SnYr = SnYr of string
    type Team = string
    type KickOff = DateTime
    type Role = User | Admin
    type LeagueName = LeagueName of string
    type ShareableLeagueId = ShareableLeagueId of string
    type ExternalPlayerId = ExternalPlayerId of string
    type ExternalLoginProvider = ExternalLoginProvider of string
    type PlayerName = PlayerName of string

    let nguid() = Guid.NewGuid()
    let newLgId() = nguid()|>LgId
    let newFxId() = nguid()|>FxId
    let newPrId() = nguid()|>PrId
    let newGwId() = nguid()|>GwId
    let newSnId() = nguid()|>SnId
    let newPlId() = nguid()|>PlId
    let makeLeagueName (name:string) =
        (if name.Length > 50 then name.Substring(0, 50) else name) |> LeagueName

    let getPlayerId (PlId id) = id
    let getGameWeekNo (GwNo n) = n
    let getLgId (LgId id) = id
    let getFxId (FxId id) = id
    let getGwId (GwId id) = id
    let getPrId (PrId id) = id
    let getSnId (SnId id) = id
    let getSnYr (SnYr year) = year
    let getPlayerName (PlayerName plrName) = plrName
    let getLeagueName (LeagueName lgeName) = lgeName
    let getExternalPlayerId (ExternalPlayerId expid) = expid
    let getExternalLoginProvider (ExternalLoginProvider exprovider) = exprovider

    let currentSeason = SnYr "prem-2016/17"
    let monthFormat = "MMMM yyyy"
    let globalLeagueId = "global"
    let globalLeaguePageSize = 30
    let maxPlayersPerLeague = 100

    type Score = int * int
    type Result = { score:Score }
    type FixtureData = { id:FxId; gwId:GwId; home:Team; away:Team; kickoff:KickOff }
    type Fixture =
         | OpenFixture of FixtureData
         | ClosedFixture of (FixtureData * Result option)
    type PredictionModifier = | DoubleDown | NoModifier
    type Prediction = { id:PrId; score:Score; fixtureId:FxId; playerId:PlId; modifier:PredictionModifier }
    type Player = { id:PlId; name:PlayerName; predictions:Prediction array; isAdmin:bool }
    type GameWeek = { id:GwId; number:GwNo; description:string; fixtures:Fixture array }
    type Season = { id:SnId; year:SnYr; gameWeeks:GameWeek array }
    type Outcome = HomeWin | AwayWin | Draw
    type Bracket = CorrectScore of PredictionModifier | CorrectOutcome of PredictionModifier | Incorrect

    type League = { id:LgId; name:LeagueName; players:Player array; adminId:PlId }

    type AppError =
        | NotLoggedIn of string
        | Forbidden of string
        | Invalid of string
        | NotFound of string
        | InternalError of string
