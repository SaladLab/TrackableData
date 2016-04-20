#I @"packages/FAKE/tools"
#I @"packages/FAKE.BuildLib/lib/net451"
#r "FakeLib.dll"
#r "BuildLib.dll"

open Fake
open BuildLib

let solution = 
    initSolution 
        "./TrackableData.sln" "Release"
        [ // Core Libraries
          { emptyProject with Name = "TrackableData"
                              Folder = "./core/TrackableData" }
          { emptyProject with Name = "TrackableData.Templates"
                              Folder = "./core/CodeGenerator-Templates"
                              Template = true
                              Dependencies = [ ("TrackableData", "") ] }
          // Plugin Libraries
          { emptyProject with Name = "TrackableData.Json"
                              Folder = "./plugins/TrackableData.Json"
                              Dependencies = [ ("TrackableData", "");
                                               ("Newtonsoft.Json", "") ] }
          { emptyProject with Name = "TrackableData.MongoDB"
                              Folder = "./plugins/TrackableData.MongoDB"
                              Dependencies = [ ("TrackableData", "")
                                               ("MongoDB.Bson", "")
                                               ("MongoDB.Driver", "")
                                               ("MongoDB.Driver.Core", "") ] }
          { emptyProject with Name = "TrackableData.MsSql"
                              Folder = "./plugins/TrackableData.MsSql"
                              Dependencies = [ ("TrackableData", "")
                                               ("TrackableData.Sql", "") ] }
          { emptyProject with Name = "TrackableData.MySql"
                              Folder = "./plugins/TrackableData.MySql"
                              Dependencies = [ ("TrackableData", "")
                                               ("TrackableData.Sql", "")
                                               ("MySql.Data", "") ] }
          { emptyProject with Name = "TrackableData.PostgreSql"
                              Folder = "./plugins/TrackableData.PostgreSql"
                              Dependencies = [ ("TrackableData", "")
                                               ("TrackableData.Sql", "")
                                               ("Npgsql", "") ] }
          { emptyProject with Name = "TrackableData.Protobuf"
                              Folder = "./plugins/TrackableData.Protobuf"
                              Dependencies = [ ("TrackableData", "")
                                               ("protobuf-net", "") ] }
          { emptyProject with Name = "TrackableData.Redis"
                              Folder = "./plugins/TrackableData.Redis"
                              Dependencies = [ ("TrackableData", "")
                                               ("StackExchange.Redis", "")
                                               ("Newtonsoft.Json", "") ] }
          { emptyProject with Name = "TrackableData.Sql"
                              Folder = "./plugins/TrackableData.Sql"
                              Dependencies = [ ("TrackableData", "") ] } ]

Target "Clean" <| fun _ -> cleanBin

Target "AssemblyInfo" <| fun _ -> generateAssemblyInfo solution

Target "Restore" <| fun _ -> restoreNugetPackages solution

Target "Build" <| fun _ -> buildSolution solution

Target "Test" <| fun _ -> testSolution solution

Target "Cover" <| fun _ ->
    coverSolutionWithParams 
        (fun p -> { p with Filter = "+[TrackableData*]* -[*.Tests]*" })
        solution

Target "Coverity" <| fun _ -> coveritySolution solution "SaladLab/TrackableData"

Target "Nuget" <| fun _ ->
    createNugetPackages solution
    publishNugetPackages solution

Target "CreateNuget" <| fun _ ->
    createNugetPackages solution

Target "PublishNuget" <| fun _ ->
    publishNugetPackages solution

Target "Unity" <| fun _ -> buildUnityPackage "./core/UnityPackage"

Target "CI" <| fun _ -> ()

Target "Help" <| fun _ ->
    showUsage solution (fun name -> 
        if name = "unity" then Some("Build UnityPackage", "")
        else None)

"Clean"
  ==> "AssemblyInfo"
  ==> "Restore"
  ==> "Build"
  ==> "Test"

"Build" ==> "Nuget"
"Build" ==> "CreateNuget"
"Build" ==> "Cover"
"Restore" ==> "Coverity"

"Test" ==> "CI"
// "Cover" ==> "CI" // make run faster on appveyor to avoid timeout
"Nuget" ==> "CI"

RunTargetOrDefault "Help"
