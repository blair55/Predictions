#r "packages/FAKE/tools/FakeLib.dll"
open System
open Fake
open Fake.ProcessHelper

let buildDir = "./build"
let slnFile = "./Predictions.sln"

Target "Clean" (fun _ ->
  CleanDir buildDir
)

Target "RestorePackages" (fun _ ->
  slnFile
  |> RestoreMSSolutionPackages (fun p -> { p with ToolPath = "./nuget.exe" })
)

Target "Compile" (fun _ ->
  !! slnFile
  |> MSBuildRelease buildDir "Build"
  |> ignore
)

Target "FrontEnd" (fun _ ->
  let grunt = tryFindFileOnPath (if isUnix then "grunt" else "grunt.cmd")
  match grunt with
  | Some g -> Shell.Exec(g, "build") |> ignore
  | None -> ()
)

"Clean"
  ==> "RestorePackages"
  ==> "Compile"
  //==> "FrontEnd"

RunTargetOrDefault "Compile"
