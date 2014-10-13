namespace PredictionsManager.Domain.Tests

open NUnit.Framework
open FsUnit
open PredictionsManager.Domain

[<TestFixture>] 
type ``Outcome Tests`` ()=

   [<Test>] member test.``when home score is greater than away score the outcome is home win`` ()=
               let score = 2, 1
               Domain.getOutcome score |> should equal Domain.HomeWin 

   [<Test>] member test.``when home score is less than away score the outcome is away win `` ()=
               Domain.getOutcome (1, 2) |> should equal Domain.AwayWin

