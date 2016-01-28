#r "packages/Suave/lib/net40/Suave.dll"

open Suave                 // always open suave
open Suave.Successful      // for OK-result
open Suave.Web             // for config
open Suave.Filters
open Suave.Operators
open Suave.Files
open System.IO
open Suave.Logging

#if INTERACTIVE
Directory.SetCurrentDirectory("/Users/george/Documents/manacurve")
#endif

let app : WebPart =
  choose
    [ path "/" >=> browseFileHome "index.html"
      path "/home" >=> OK "home"
      path "/test" >=> OK "test"
      browseHome ]

let homeFolder = Directory.GetCurrentDirectory() + "/static/"
let config =
    { defaultConfig with
        logger = Loggers.saneDefaultsFor LogLevel.Info
        homeFolder = Some homeFolder }
startWebServer config app
