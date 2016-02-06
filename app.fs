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
open Logary
open Logary.Configuration
open Logary.Targets
open Logary.Adapters

let app : WebPart =
  choose
    [ path "/" >=> browseFileHome "index.html"
      Monodeck.endpoints
      browseHome
      NOT_FOUND ""]

let homeFolder = Directory.GetCurrentDirectory() + "/static/"

[<EntryPoint>]
let main argv =
  use logary =
    withLogary' "Manacurve" (
      withRule (Rule.createForTarget "console")
      >> withTarget (Console.create Console.empty "console")
    )

  let config =
    { defaultConfig with
        logger = SuaveAdapter(logary.GetLogger "suave")
        homeFolder = Some homeFolder }

  startWebServer config app
  0
