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
open System.Net

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
      withRules [ Rule.setLevel (LogLevel.Debug) (Rule.createForTarget "console") ]
      >> withTarget (Console.create Console.empty "console")
    )

  let config =
    let port = System.Environment.GetEnvironmentVariable("PORT")
    let ip127  = IPAddress.Parse("127.0.0.1")
    let ipZero = IPAddress.Parse("0.0.0.0")
    { defaultConfig with
        logger = SuaveAdapter(logary.GetLogger "suave")
        homeFolder = Some homeFolder
        bindings=[ (if port = null then HttpBinding.mk HTTP ip127 (uint16 8080)
                      else HttpBinding.mk HTTP ipZero (uint16 port)) ] }

  startWebServer config app
  0
