namespace Predictions.Api

open System
open Microsoft.Owin.Hosting
open System.Threading

module EntryPoint =

    let quitEvent = new ManualResetEvent(false)

    [<EntryPoint>]
    let main argv =
        
        // let ts = TimeSpan.FromMinutes 5. 
        // createTimer ts findAndSaveResultsFromFplForClosedFixturesWithNoResults |> Async.Start

        let hostUrl = Config.hostUrl
        Logging.info(sprintf "Starting at %s" hostUrl)
        WebApp.Start(hostUrl, Config.buildApp) |> ignore
        quitEvent.WaitOne() |> ignore
        0
