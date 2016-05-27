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
                              PackagePrerelease = "beta"
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

Target "PackNuget" <| fun _ -> createNugetPackages solution

Target "PackUnity" <| fun _ ->
    packUnityPackage "./core/UnityPackage/TrackableData.unitypackage.json"

Target "Pack" <| fun _ -> ()

Target "PublishNuget" <| fun _ -> publishNugetPackages solution

Target "PublishUnity" <| fun _ -> ()

Target "Publish" <| fun _ -> ()

Target "CI" <| fun _ -> ()

Target "Help" <| fun _ -> 
    showUsage solution (fun _ -> None)

"Clean"
  ==> "AssemblyInfo"
  ==> "Restore"
  ==> "Build"
  ==> "Test"

"Build" ==> "Cover"
"Restore" ==> "Coverity"

let isPublishOnly = getBuildParam "publishonly"

"Build" ==> "PackNuget" =?> ("PublishNuget", isPublishOnly = "")
"Build" ==> "PackUnity" =?> ("PublishUnity", isPublishOnly = "")
"PackNuget" ==> "Pack"
"PackUnity" ==> "Pack"
"PublishNuget" ==> "Publish"
"PublishUnity" ==> "Publish"

"Test" ==> "CI"
// "Cover" ==> "CI" // make run faster on appveyor to avoid timeout
"Publish" ==> "CI"

RunTargetOrDefault "Help"
