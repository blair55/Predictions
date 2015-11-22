#r "packages/FAKE/tools/FakeLib.dll"
open System
open Fake

let buildDir = "./build"

Target "Clean" (fun _ ->
  CleanDir buildDir
)

Target "RestorePackages" (fun _ ->
  "./Predictions.sln"
  |> RestoreMSSolutionPackages (fun p -> { p with ToolPath = "./nuget.exe" })
)

Target "Compile" (fun _ ->
  !! "./Predictions.sln"
  |> MSBuildRelease buildDir "Build"
  |> ignore
)

"Clean"
  ==> "RestorePackages"
  ==> "Compile"

RunTargetOrDefault "Compile"
