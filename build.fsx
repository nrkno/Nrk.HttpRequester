#r @"tools/FAKE/tools/FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile
open Fake.Git
open Fake.MSBuildHelper
open Fake.Testing.XUnit2

let buildDir = "./build/"
let testDir = "./source/Nrk.HttpRequester.UnitTests"
let testOutputDir = "./tests/"
let packageConfigs = !! "./source/**/packages.config"
let projectReferences = !! "./source/**/*.csproj"
                        -- "./source/Nrk.HttpRequester.UnitTests/*.csproj"
let projectName = "NRK.HttpRequester"
let description = "Library for sending Http Requests, including a fluent interface for creating HttpClient instances"
let version = environVarOrDefault "version" "0.0.0"
let commitHash = Information.getCurrentSHA1(".")

Target "Clean" (fun _ ->
  CleanDirs [buildDir; testOutputDir]
)

Target "RestoreNugetPackages" (fun _ ->
  packageConfigs
  |> Seq.iter(RestorePackage (fun p ->
    { p with ToolPath = "./tools/nuget/nuget.exe"
             OutputPath = "./source/packages"}))
)

Target "Build" (fun _ ->
    CreateCSharpAssemblyInfo "./source/Nrk.HttpRequester/Properties/AssemblyInfo.cs"
        [Attribute.Title projectName
         Attribute.Product projectName
         Attribute.Description description
         Attribute.Version version
         Attribute.InformationalVersion version
         Attribute.FileVersion version
         Attribute.Guid "d51e7a23-73d5-49a1-be63-f73e432e9ab3"
         Attribute.Metadata("githash", commitHash)]
    MSBuildRelease buildDir "Build" projectReferences
      |> Log "Building project: "
)

Target "BuildTests" (fun _ ->
  !! (testDir + "/*.csproj")
    |> MSBuildDebug testOutputDir "Build"
    |> Log "Building test project: "
)

Target "Test" (fun _ ->
  !! (testOutputDir @@ "*UnitTests.dll")
  |> xUnit2 (fun p ->
                 { p with HtmlOutputPath = Some (testOutputDir @@ "xunit.html") })
)

Target "CreateNugetPackage" (fun _ ->
  NuGet (fun p ->
    {p with
      Authors = ["NRK"]
      Project = projectName
      Description = description
      Version = version
      OutputPath = buildDir
      WorkingDir = buildDir
      Publish = false
      Dependencies =
        ["Polly", GetPackageVersion "./source/packages/" "Polly"
         "Microsoft.AspNet.WebApi.Client", GetPackageVersion "./source/packages/" "Microsoft.AspNet.WebApi.Client"]
      Files =
        [(@"Nrk.HttpRequester.dll", Some @"lib\net45", None)
         (@"Nrk.HttpRequester.pdb", Some @"lib\net45", None)]
    })
    "Nrk.HttpRequester.nuspec"
)

Target "Default" DoNothing

Target "PublishNugetPackage" (fun _ ->
    NuGetPublish (fun p ->
    {p with
        PublishUrl = "https://www.nuget.org/api/v2/package"
        AccessKey = environVarOrDefault "nugetKey" ""
        Project = projectName
        WorkingDir = buildDir
        OutputPath = buildDir
        Version = version
    })
)

// Dependencies
"Clean"
 ==> "RestoreNugetPackages"
 ==> "Build"
 ==> "BuildTests"
 ==> "Test"
 ==> "CreateNugetPackage"
 ==> "Default"
 ==> "PublishNugetPackage"

RunTargetOrDefault "Default"
