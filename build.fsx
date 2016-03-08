#I @"packages/FAKE/tools"
#r "FakeLib.dll"

open Fake
open System.IO
open Fake.Testing.XUnit2
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper

// ------------------------------------------------------------------------------ Project

let buildSolutionFile = "./TrackableData.sln"
let buildConfiguration = "Release"

type Project = { 
    Name: string;
    Folder: string;
    Template: bool;
    AssemblyVersion: string;
    PackageVersion: string;
    Releases: ReleaseNotes list;
    Dependencies: (string * string) list;
}

let emptyProject = { Name=""; Folder=""; Template=false; AssemblyVersion="";
                     PackageVersion=""; Releases=[]; Dependencies=[] }

let decoratePrerelease v =
    let couldParse, parsedInt = System.Int32.TryParse(v)
    if couldParse then "build" + (sprintf "%04d" parsedInt) else v

let decoratePackageVersion v =
    if hasBuildParam "nugetprerelease" then
        v + "-" + decoratePrerelease((getBuildParam "nugetprerelease"))
    else
        v

let projects = ([
    // Core Libraries
    {   emptyProject with
        Name="TrackableData";
        Folder="./core/TrackableData";
    };
    {   emptyProject with
        Name="TrackableData.Templates";
        Folder="./core/CodeGenerator-Templates";
        Template=true;
        Dependencies=[("TrackableData", "")];
    };    
    // Plugin Libraries
    {   emptyProject with
        Name="TrackableData.Json";
        Folder="./plugins/TrackableData.Json";
        Dependencies=[("TrackableData", "");
                      ("Newtonsoft.Json", "")];
    };
    {   emptyProject with
        Name="TrackableData.MongoDB";
        Folder="./plugins/TrackableData.MongoDB";
        Dependencies=[("TrackableData", "");
                      ("MongoDB.Bson", "");
                      ("MongoDB.Driver", "");
                      ("MongoDB.Driver.Core", "");];
    };
    {   emptyProject with
        Name="TrackableData.MsSql";
        Folder="./plugins/TrackableData.MsSql";
        Dependencies=[("TrackableData", "");
                      ("TrackableData.Sql", "");];
    };
    {   emptyProject with
        Name="TrackableData.MySql";
        Folder="./plugins/TrackableData.MySql";
        Dependencies=[("TrackableData", "");
                      ("TrackableData.Sql", "");
                      ("MySql.Data", "")];
    };
    {   emptyProject with
        Name="TrackableData.PostgreSql";
        Folder="./plugins/TrackableData.PostgreSql";
        Dependencies=[("TrackableData", "");
                      ("TrackableData.Sql", "");
                      ("Npgsql", "");];
    };
    {   emptyProject with
        Name="TrackableData.Protobuf";
        Folder="./plugins/TrackableData.Protobuf";
        Dependencies=[("TrackableData", "");
                      ("protobuf-net", "")];
    };
    {   emptyProject with
        Name="TrackableData.Sql";
        Folder="./plugins/TrackableData.Sql";
        Dependencies=[("TrackableData", "");];
    };]
    |> List.map (fun p -> 
        let parsedReleases =
            File.ReadLines (p.Folder @@ (p.Name + ".Release.md"))
            |> ReleaseNotesHelper.parseAllReleaseNotes
        let latest = List.head parsedReleases
        { p with AssemblyVersion = latest.AssemblyVersion;
                 PackageVersion = decoratePackageVersion(latest.AssemblyVersion);
                 Releases = parsedReleases }
    ))

let project name =
    List.filter (fun p -> p.Name = name) projects |> List.head

let dependencies p deps =
    p.Dependencies |>
    List.map (fun d -> match d with 
                       | (id, "") -> (id, match List.tryFind (fun (x, ver) -> x = id) deps with
                                          | Some (_, ver) -> ver
                                          | None -> ((project id).PackageVersion))
                       | (id, ver) -> (id, ver))
    
// ---------------------------------------------------------------------------- Variables

let binDir = "bin"
let testDir = binDir @@ "test"
let nugetDir = binDir @@ "nuget"
let nugetWorkDir = nugetDir @@ "work"

// ------------------------------------------------------------------------- Unity Helper

let UnityPath = 
    @"C:\Program Files\Unity\Editor\Unity.exe" 

let Unity projectPath args = 
    let result = Shell.Exec(UnityPath, "-quit -batchmode -logFile -projectPath \"" + projectPath + "\" " + args) 
    if result < 0 then failwithf "Unity exited with error %d" result 

// ------------------------------------------------------------------------------ Targets

Target "Clean" (fun _ -> 
    CleanDirs [binDir]
)

Target "AssemblyInfo" (fun _ ->
    projects
    |> List.filter (fun p -> not p.Template)
    |> List.iter (fun p -> 
        CreateCSharpAssemblyInfo (p.Folder @@ "Properties" @@ "AssemblyInfoGenerated.cs")
          [ Attribute.Version p.AssemblyVersion
            Attribute.FileVersion p.AssemblyVersion
            Attribute.InformationalVersion p.PackageVersion ]
        )
)

Target "Build" (fun _ ->
    !! buildSolutionFile
    |> MSBuild "" "Rebuild" [ "Configuration", buildConfiguration ]
    |> Log "Build-Output: "
)

Target "Test" (fun _ ->  
    let xunitToolPath = findToolInSubPath "xunit.console.exe" "./packages/FAKE/xunit.runner.console*/tools"
    ensureDirectory testDir
    !! ("./core/**/bin/" + buildConfiguration + "/*.Tests.dll")
    |> xUnit2 (fun p -> 
        {p with 
            ToolPath = xunitToolPath;
            ShadowCopy = false;
            XmlOutputPath = Some (testDir @@ "TestResult.xml") })
)

Target "UnityPackage" (fun _ ->
    Shell.Exec(".\core\UnityPackage\UpdateDll.bat")
    Unity (Path.GetFullPath "core/UnityPackage") "-executeMethod PackageBuilder.BuildPackage"
    Unity (Path.GetFullPath "core/UnityPackage") "-executeMethod PackageBuilder.BuildPackageFull"
    (!! "core/UnityPackage/*.unitypackage") |> Seq.iter (fun p -> MoveFile binDir p)
)

let createNugetPackages _ =
    projects
    |> List.iter (fun project -> 
        let nugetFile = project.Folder @@ project.Name + ".nuspec";
        let workDir = nugetWorkDir @@ project.Name;

        let dllFileName = project.Folder @@ "bin/Release" @@ project.Name;
        let dllFiles = (!! (dllFileName + ".dll")
                        ++ (dllFileName + ".pdb")
                        ++ (dllFileName + ".xml"))
        dllFiles |> CopyFiles (workDir @@ "lib" @@ "net45")

        let dllFileNameNet35 = (project.Folder + ".Net35") @@ "bin/Release" @@ project.Name;
        let dllFilesNet35 = (!! (dllFileNameNet35 + ".dll")
                             ++ (dllFileNameNet35 + ".pdb")
                             ++ (dllFileNameNet35 + ".xml"))
        if (Seq.length dllFilesNet35 > 0) then (
            dllFilesNet35 |> CopyFiles (workDir @@ "lib" @@ "net35"))
         
        let isAssemblyInfo f = (filename f).Contains("AssemblyInfo")
        let isSrc f = (hasExt ".cs" f) && not (isAssemblyInfo f)
        CopyDir (workDir @@ "src") project.Folder isSrc

        let packageFile = project.Folder @@ "packages.config"
        let packageDependencies = if (fileExists packageFile) then (getDependencies packageFile) else []

        NuGet (fun p -> 
            {p with
                Project = project.Name
                OutputPath = nugetDir
                WorkingDir = workDir
                Dependencies = dependencies project packageDependencies
                SymbolPackage = (if (project.Name.Contains("Templates")) then NugetSymbolPackage.None else NugetSymbolPackage.Nuspec)
                Version = project.PackageVersion 
                ReleaseNotes = (List.head project.Releases).Notes |> String.concat "\n"
            }) nugetFile
    )

let publishNugetPackages _ =
    projects
    |> List.iter (fun project -> 
        try
            NuGetPublish (fun p -> 
                {p with
                    Project = project.Name
                    OutputPath = nugetDir
                    WorkingDir = nugetDir
                    AccessKey = getBuildParamOrDefault "nugetkey" ""
                    PublishUrl = getBuildParamOrDefault "nugetpublishurl" ""
                    Version = project.PackageVersion })
        with e -> if getBuildParam "forcepublish" = "" then raise e; ()
        if not project.Template && hasBuildParam "nugetpublishurl" then (
            // current FAKE doesn't support publishing symbol package with NuGetPublish.
            // To workaround thid limitation, let's tweak Version to cheat nuget read symbol package
            try
                NuGetPublish (fun p -> 
                    {p with
                        Project = project.Name
                        OutputPath = nugetDir
                        WorkingDir = nugetDir
                        AccessKey = getBuildParamOrDefault "nugetkey" ""
                        PublishUrl = getBuildParamOrDefault "nugetpublishurl" ""
                        Version = project.PackageVersion + ".symbols" })
            with e -> if getBuildParam "forcepublish" = "" then raise e; ()
        )
    )

Target "Nuget" <| fun _ ->
    createNugetPackages()
    publishNugetPackages()

Target "CreateNuget" <| fun _ ->
    createNugetPackages()

Target "PublishNuget" <| fun _ ->
    publishNugetPackages()

Target "Help" (fun _ ->  
    List.iter printfn [
      "usage:"
      "build [target]"
      ""
      " Targets for building:"
      " * Build        Build"
      " * Test         Build and Test"
      " * UnityPackage Build UnityPackage"    
      " * Nuget        Create and publish nugets packages"
      " * CreateNuget  Create nuget packages"
      "                [nugetprerelease={VERSION_PRERELEASE}] "
      " * PublishNuget Publish nugets packages"
      "                [nugetkey={API_KEY}] [nugetpublishurl={PUBLISH_URL}] [forcepublish=1]"
      ""]
)

// --------------------------------------------------------------------------- Dependency

// Build order
"Clean"
  ==> "AssemblyInfo"
  ==> "Build"
  ==> "Test"

"Build"
  ==> "Nuget"

"Build"
  ==> "CreateNuget"
  
RunTargetOrDefault "Help"
