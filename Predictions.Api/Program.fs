namespace Predictions.Api

open System
open Microsoft.Owin.Hosting
open System.Threading

module EntryPoint =

    let quitEvent = new ManualResetEvent(false)

    [<EntryPoint>]
    let main argv =
        let hostUrl = Config.config "HostUrl"
        Console.WriteLine("Starting at {0}", hostUrl)
        WebApp.Start(hostUrl, Config.buildApp) |> ignore
        quitEvent.WaitOne() |> ignore
        //Console.ReadLine() |> ignore
        0
