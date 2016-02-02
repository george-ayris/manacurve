#r "packages/Suave/lib/net40/Suave.dll"
#load "src/api.fs"

open Suave
open Suave.Successful
open Suave.RequestErrors
open Suave.Web
open Suave.Filters
open Suave.Operators
open Suave.Files
open System.IO
open Suave.Logging
open Manacurve.Api

#if INTERACTIVE
Directory.SetCurrentDirectory("/Users/george/Documents/manacurve")
#endif

let app : WebPart =
  choose
    [ path "/" >=> browseFileHome "index.html"
      Monodeck.endpoints
      browseHome
      NOT_FOUND ""]

let homeFolder = Directory.GetCurrentDirectory() + "/static/"
let config =
    { defaultConfig with
        logger = Loggers.saneDefaultsFor LogLevel.Info
        homeFolder = Some homeFolder }
startWebServer config app
