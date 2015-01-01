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

    let log msg =
        printfn "%s" msg
        System.Diagnostics.Debug.WriteLine(msg)

    let tryToWithReturn x = 
        try
            let r = x()
            Success r
        with
            | :? (System.Exception) as ex -> Failure ex.Message

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