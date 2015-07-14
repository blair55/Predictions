#r "packages/FAKE/tools/FakeLib.dll"
open System
open Fake

let buildDir = "./build"

Target "Clean" (fun _ ->
    CleanDir buildDir
)

Target "RestorePackages" (fun _ ->
    RestorePackages()
)

Target "Compile" (fun _ ->
    !! "AppHarbor.sln"
    |> MSBuildRelease buildDir "Build"
    |> ignore
)

//Target "Test" (fun _ ->
//    !! (buildDir + "/**/*UnitTests.dll")
//    |> NUnit (fun defaults -> defaults)
//)

"Clean"
//  ==> "RestorePackages"
  ==> "Compile"
//  ==> "Test"

RunTargetOrDefault "Compile"
