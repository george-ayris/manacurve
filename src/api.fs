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
    converters.Add(new TupleConverter())
    jsonSerializerSettings.Converters <- converters
    jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()

    JsonConvert.SerializeObject(v, jsonSerializerSettings)

  let JSON v =
    toJson v
    |> OK
    >=> Writers.setMimeType "application/json; charset=utf-8"

  let fromJson<'a> json =
    JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a

  let getResourceFromReq<'a> (req : HttpRequest) =
    let getString rawForm =
      System.Text.Encoding.UTF8.GetString(rawForm)
    req.rawForm |> getString |> fromJson<'a>


module Redis =
  open ApiHelpers

  let redisUrl =
    let redisUrl = System.Environment.GetEnvironmentVariable("REDIS_URL")
    if redisUrl = null
    then "redis://localhost:6379"
    else redisUrl

  let logger = Logging.getLoggerByName "Manacurve"

  let encodeLandColours (l : DeckLandQuantities) = l.colour1 * 100 + l.colour2
  let simulationKey n = "simulation:" + (sprintf "%5i" n)
  let averagesKey n = simulationKey n + ":averages"
  let distributionsKey n = simulationKey n + ":distributions"
  let otherKey key n =
    if key = (averagesKey n)
    then distributionsKey n
    else averagesKey n

  let redis() = new RedisClient(redisUrl)

  let saveValue (r : RedisClient) key value =
    r.SetValue(key, toJson value)

  let checkAnalysisCacheAndReact landColours =
    let r = redis()
    let key = averagesKey (encodeLandColours landColours)
    if not (r.ContainsKey(key))
    then
      LogLine.info ("Simulation - cache miss for " + key) |> logger.Log
      let deck = createDeck landColours
      let simulationResults = simulateGames deck
      LogLine.info "Simulation - results created" |> logger.Log
      saveValue r (simulationKey (encodeLandColours landColours)) simulationResults
      LogLine.info "Simulation - results saved" |> logger.Log
      ()
    else
      LogLine.info ("Simulation - cache hit for " + key) |> logger.Log

  let checkSimulationCacheAndReact keyFunction valueFunction n =
    let r = redis()
    let key = keyFunction n

    let deleteMonoKey() =
      (if r.ContainsKey(otherKey key n)
      then
        LogLine.info ("Delete - cache hit for " + (otherKey key n)) |> logger.Log
        r.Remove(simulationKey n)
      else
        LogLine.info ("Delete - cache miss for " + (otherKey key n)) |> logger.Log
        false) |> ignore

    let cache value =
      r.SetValue(key, toJson value)
      deleteMonoKey()

    if r.ContainsKey(key)
    then
      LogLine.info ("Request - cache hit for " + key) |> logger.Log
      Some <| fromJson (r.GetValue(key))
    else
      if r.ContainsKey(simulationKey n)
      then
        LogLine.info ("Request - cache miss for " + key) |> logger.Log
        let value = valueFunction (fromJson (r.GetValue(simulationKey n)))
        cache value
        Some value
      else
        None

  let averagesCheckCacheAndReact =
    checkSimulationCacheAndReact averagesKey averageLandsPerTurn

  let distributionsCheckCacheAndReact =
    checkSimulationCacheAndReact distributionsKey mostCommonLandScenariosPerTurn

module Deck =
  open ApiHelpers
  open Redis

  let logger = Logging.getLoggerByName "Manacurve"

  let createDeckSimulations (landColours : DeckLandQuantities) =
    checkAnalysisCacheAndReact landColours
    OK ""

  type AverageLands = { averages: float list list }
  let averageLands n =
    LogLine.info ("Request started - average lands " + n.ToString()) |> logger.Log
    let averages = averagesCheckCacheAndReact n
    match averages with
      | Some a -> JSON { averages = a }
      | None -> NOT_FOUND "Simulation not done"

  type LandScenarioAndProbability = { landScenario: int list; probability: float }
  type MostCommonLandScenarios =
    { mostCommonLandScenarios: LandScenarioAndProbability list list }
  let landDistributions n =
    LogLine.info ("Request started - distributions " + n.ToString()) |> logger.Log
    let distributions = distributionsCheckCacheAndReact n
    match distributions with
      | Some x ->
        let tupleToRecord = fun (scenario, probability) -> { landScenario=scenario; probability=probability}
        JSON { mostCommonLandScenarios =  List.map (List.map tupleToRecord) x }
      | None -> NOT_FOUND "Simulation not done"

  let endpoints =
    choose
      [ path "/deck" >=> POST >=>
          request (getResourceFromReq >> createDeckSimulations)
        pathScan "/deck/%d/averages" (fun d ->
            GET >=> averageLands d)
        pathScan "/deck/%d/mostcommon" (fun d ->
            GET >=> landDistributions d) ]
