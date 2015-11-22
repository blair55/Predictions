namespace Predictions.Api

open System
open Microsoft.Owin.Hosting

module EntryPoint =

    [<EntryPoint>]
    let main argv = 
        let hostUrl = Config.config "HostUrl"
        WebApp.Start(hostUrl, Config.buildApp) |> ignore
        Console.WriteLine("Starting at {0}", hostUrl)
        Console.ReadLine() |> ignore
        0
