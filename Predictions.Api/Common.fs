namespace Predictions.Api

open System

[<AutoOpen>]
module Common =

    let str o = o.ToString()

    let getFirst l =
        match l with
        | h::_ -> Some h
        | [] -> None

    type Result<'TSuccess,'TFailure> = 
        | Success of 'TSuccess
        | Failure of 'TFailure

    let bind switchFunction twoTrackInput = 
        match twoTrackInput with
        | Success s -> switchFunction s
        | Failure f -> Failure f

    let switch f x = f x |> Success

    let optionToResult x msg = match x with | Some y -> Success y | None -> Failure msg
    let doIfSome o f = match o with | Some r -> r |> f |> Some | None -> None

    let log msg =
        printfn "%s" msg
        System.Diagnostics.Debug.WriteLine(msg)

    let tryToWithReturn x = 
        try
            let r = x()
            Success r
        with
            | ex -> Failure ex.Message

    let fstOption a =
        match a with
        | Some (b, _) -> Some b
        | None -> None

    let sndOption a =
        match a with
        | Some (_, b) -> Some b
        | None -> None
        
    let sToGuid s = Guid.Parse(s)
    let trySToGuid s = Guid.TryParse(s)

    
    let compoundList collection =
        collection
        |> List.scan (fun x y -> x @ [y]) []
        |> List.tail


open Microsoft.AspNet.Identity
open Microsoft.AspNet.Identity.Owin
open System.Threading.Tasks

    type PlUser(id, provider, username) =
        let mutable _username = username
        interface IUser with
            member this.Id with get() = id
            member this.UserName
                with get() = _username
                and set(value) = _username <- value
        member this.Provider with get() = provider
        member this.Id with get() = id
        member this.UserName with get() = _username


    type PlUserStore() =
        interface IUserStore<PlUser> with
            member this.FindByNameAsync(name) = failwith ""
            member this.FindByIdAsync(name) = failwith ""
            member this.UpdateAsync(user) = failwith ""
            member this.DeleteAsync(user) = failwith ""
            member this.CreateAsync(user) = failwith ""
            member this.Dispose() = ()

    type PlUserManager(store) =
        inherit UserManager<PlUser>(store)

    type PlSignInManager(userManager, authenticationManager) =
        inherit SignInManager<PlUser, string>(userManager, authenticationManager)


open NLog
open NLog.Config
open NLog.Targets
open NLog.Layouts
open System.Configuration

module Logging =

        let private layout = "${longdate} | ${level:uppercase=True} | ${message} ${exception:format=Type,StackTrace:innerFormat=Message,Type,StackTrace:maxInnerExceptionLevel=1} "

//        private static void ConfigureConsoleTarget(LoggingConfiguration configuration)
//        {
//            var consoleTarget = new ConsoleTarget { Layout = Layout };
//            var consoleRule = new LoggingRule("*", LogLevel.Debug, consoleTarget);
//            configuration.AddTarget("console", consoleTarget);
//            configuration.LoggingRules.Add(consoleRule);
//        }
//
        let configuration = new LoggingConfiguration()
        //let token = ConfigurationManager.AppSettings.["SQLSERVER_CONNECTION_STRING"]
        let token = "ecb263d0-5fae-45b8-807a-ca72f60d0d3f"
        let logEntriesTarget = new LogentriesTarget()
        logEntriesTarget.Token <- token
        logEntriesTarget.Layout <- Layout.FromString(layout)
        let logEntriesRule = new LoggingRule("*", LogLevel.Info, logEntriesTarget)
        configuration.AddTarget("logentries", logEntriesTarget)
        configuration.LoggingRules.Add(logEntriesRule)

//        ConfigureConsoleTarget(configuration);
        LogManager.Configuration <- configuration
        LogManager.Configuration.Reload() |> ignore
        LogManager.ReconfigExistingLoggers()
        let log = LogManager.GetCurrentClassLogger()

        let debug (msg:string) = log.Debug(msg)
        let info (msg:string) = log.Info(msg)
        let warn (msg:string) = log.Warn(msg)
        let errorNx (msg:string) = log.Error(msg)
        let error (ex:Exception) = log.Error(ex.Message, ex, [])

