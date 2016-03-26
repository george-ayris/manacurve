// include Fake libs
#r "./packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Testing

// Directories
let buildDir  = "./build/"
let testDir = buildDir + "tests/"
let deployDir = "./deploy/"


// Filesets
let testPattern = "/**/tests/*.fsproj"
let appReferences  =
    !! "/**/*.fsproj"
      -- testPattern
let testReferences =
  !! testPattern

// version info
let version = "0.1"  // or retrieve from CI server

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; deployDir]
)

Target "Build" (fun _ ->
    // compile all projects below src/app/
    MSBuildDebug buildDir "Build" appReferences
        |> Log "AppBuild-Output: "
)

Target "BuildTests" (fun _ ->
    MSBuildDebug testDir "Build" testReferences
        |> Log "TestBuild-Output: "
)

Target "RunTests" (fun _ ->
    trace "Running tests..."
    !! (testDir + @"\tests.dll")
      |> xUnit (fun p -> {p with ToolPath = @"packages/xunit.runner.console/tools/xunit.console.x86.exe" })
)

Target "Deploy" (fun _ ->
    !! (buildDir + "/**/*.*")
        -- "*.zip"
        |> Zip buildDir (deployDir + "ApplicationName." + version + ".zip")
)

// Build order
"Clean"
  ==> "Build"
  ==> "BuildTests"
  ==> "RunTests"
  ==> "Deploy"

// start build
RunTargetOrDefault "RunTests"
