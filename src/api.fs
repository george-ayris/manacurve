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
open ServiceStack.Redis
open Logary

module ApiHelpers =
  open Manacurve.Api.Serialisation

  let onlyTheseMethods webParts =
    choose (webParts @ [METHOD_NOT_ALLOWED ""])

  let toJson v =
    let jsonSerializerSettings = new JsonSerializerSettings()
    let converters = new System.Collections.Generic.List<JsonConverter>()
    converters.Add(new TupleArrayConverter())
    jsonSerializerSettings.Converters <- converters
    jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()

    JsonConvert.SerializeObject(v, jsonSerializerSettings)

  let JSON v =
    toJson v
    |> OK
    >=> Writers.setMimeType "application/json; charset=utf-8"

  let fromJson<'a> json =
    JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a

  let fromJsonTripleNestedList json =
    let tripleList = JsonConvert.DeserializeObject(json, typeof<float list list list>) :?> float list list list
    let round x = int (System.Math.Round(x : float))
    let listToTuple = function
      | x::y::[] -> (round x,y)
      | x -> raise (System.ArgumentException(x.ToString() + " is of the wrong form."))
    List.map (List.map listToTuple) tripleList


module Redis =
  open ApiHelpers

  let logger = Logging.getCurrentLogger ()

  let monoKey n = "mono:" + n.ToString()
  let monoAveragesKey n = monoKey n + ":averages"
  let monoDistributionsKey n = monoKey n + ":distributions"
  let otherKey key n =
    if key = (monoAveragesKey n)
    then monoDistributionsKey n
    else monoAveragesKey n

  let redis() = new RedisClient("localhost")

  let saveValue key value =
    redis().SetValue(key, toJson value)

  let checkCacheAndReact keyFunction deserialiser valueFunction n =
    let r = redis()
    let key = keyFunction n

    let deleteMonoKey =
      (if r.ContainsKey(otherKey key n)
      then r.Remove(monoKey n)
      else false) |> ignore

    let cache value =
      r.SetValue(key, toJson value)
      deleteMonoKey

    if r.ContainsKey(key)
    then
      LogLine.info ("Cache hit for " + key) |> logger.Log
      deserialiser (r.GetValue(key))
    else
      LogLine.info ("Cache miss for " + key) |> logger.Log
      let value = valueFunction (fromJson (r.GetValue(monoKey n)))
      cache value
      value

  let averagesCheckCacheAndReact : int -> float list =
    checkCacheAndReact monoAveragesKey fromJson averageLandsPerTurn

  let distributionsCheckCacheAndReact : int -> (int * float) list list =
    checkCacheAndReact monoDistributionsKey fromJsonTripleNestedList landDistributionsPerTurn

module Monodeck =
  open ApiHelpers
  open Redis

  let createDeckSimulations n =
    let deck = createMonoDeck n
    let simulationResults = simulateGames deck
    saveValue ("mono:" + n.ToString()) simulationResults
    OK ""

  type AverageLands = { averages: float list }
  let averageLands n =
    let averages = averagesCheckCacheAndReact n
    { averages = averages }

  type Distributions = { distributions: (int * float) list list }
  let landDistributions n =
    let distributions = distributionsCheckCacheAndReact n
    { distributions =  distributions }

  let endpoints =
    choose
      [ pathScan "/monodeck/%d" (fun d ->
          GET >=> createDeckSimulations d)
        pathScan "/monodeck/%d/averages" (fun d ->
            GET >=> JSON (averageLands d))
        pathScan "/monodeck/%d/distributions" (fun d ->
            GET >=> JSON (landDistributions d)) ]
