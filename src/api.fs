namespace Manacurve.Api

open Suave
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors
open Suave.Filters
open Manacurve.Domain.Analysis
open Manacurve.Domain.Cards
open Newtonsoft.Json
open Newtonsoft.Json.Serialization

module ApiHelpers =
  open Manacurve.Api.Serialisation

  let onlyTheseMethods webParts =
    choose (webParts @ [METHOD_NOT_ALLOWED ""])

  let JSON v =
    let jsonSerializerSettings = new JsonSerializerSettings()
    let converters = new System.Collections.Generic.List<JsonConverter>() 
    converters.Add(new TupleArrayConverter())
    jsonSerializerSettings.Converters <- converters
    jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()

    JsonConvert.SerializeObject(v, jsonSerializerSettings)
    |> OK
    >=> Writers.setMimeType "application/json; charset=utf-8"

  let fromJson<'a> json =
    JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a

module Monodeck =
  open ApiHelpers

  type AverageLands = { averages: float list }
  let averageLands n =
    let deck = createMonoDeck n
    { averages = averageLandsPerTurn deck }

  type Distributions = { distributions: (int * float) list list }
  let landDistributions n =
    let deck = createMonoDeck n
    { distributions = landDistributionsPerTurn deck }

  let endpoints =
    choose
      [ pathScan "/monodeck/%d" (fun d -> GET >=> OK "")
        pathScan "/monodeck/%d/averages" (fun d ->
            GET >=> JSON (averageLands d))
        pathScan "/monodeck/%d/distributions" (fun d ->
            GET >=> JSON (landDistributions d)) ]
