namespace PredictionsManager.Domain

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

    let log msg =
        printfn "%s" msg
        System.Diagnostics.Debug.WriteLine(msg)

    let tryToWithReturn x = 
        try
            let r = x()
            Success r
        with
            | :? (System.Exception) as ex -> Failure ex.Message

