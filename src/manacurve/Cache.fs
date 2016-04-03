namespace Manacurve

open ApiHelpers
open Logary
open ServiceStack.Redis
open Domain
open Analysis
open ApiTypes
open ListHelpers

module Cache =
  let analysis = loadAnalysis 10000 10 shuffle

  let redisUrl =
    let redisUrl = System.Environment.GetEnvironmentVariable("REDIS_URL")
    if redisUrl = null
    then "redis://localhost:6379"
    else redisUrl

  let logger = Logging.getLoggerByName "Manacurve"

  let encodeLandColours (l : DeckDescription) =
    sprintf "%02i%02i%02i%02i%02i%02i"
      l.colour1
      l.colour2
      l.colour3
      l.colour1Colour2
      l.colour1Colour3
      l.colour2Colour3
  let stringifyUrlParam (n : int) = sprintf "%012i" n
  let simulationKey s = "simulation:" + s
  let averagesKey s = simulationKey s + ":averages"
  let mostCommonLandsKey s = simulationKey s + ":mostcommon"
  let otherKey key s =
    if key = (averagesKey s)
    then mostCommonLandsKey s
    else averagesKey s

  let redis() = new RedisClient(redisUrl)

  let saveValue (r : RedisClient) key value =
    r.SetValue(key, toJson value)

  let checkAnalysisCacheAndReact landColours =
    let compressSimulationResults (simulations : Simulation list) =
      let manaPossibilityToIntList xs = List.map (fun x -> [x.colour1; x.colour2; x.colour3]) xs
      let resultsToIntList xs = List.map (fun x -> manaPossibilityToIntList x.manaPossibilities) xs
      List.map (fun x -> resultsToIntList x.results) simulations

    let r = redis()
    let key = averagesKey (encodeLandColours landColours)
    if not (r.ContainsKey(key))
    then
      LogLine.info ("Simulation - cache miss for " + key) |> logger.Log
      let deck = createDeck landColours
      let simulationResults = analysis.simulateGames deck
      let compressedSimulationResults = compressSimulationResults simulationResults
      LogLine.info "Simulation - results created" |> logger.Log
      saveValue r (simulationKey (encodeLandColours landColours)) compressedSimulationResults
      LogLine.info "Simulation - results saved" |> logger.Log
      ()
    else
      LogLine.info ("Simulation - cache hit for " + key) |> logger.Log

  let checkSimulationCacheAndReact keyFunction valueFunction n =
    let uncompressSimulationResults (redisValue : int list list list list) =
      let constructManaInPlay (xs : int list) = { colour1=xs.[0]; colour2=xs.[1]; colour3=xs.[2] }
      let constructManaPossibilities xs = { manaPossibilities=List.map constructManaInPlay xs }
      let constructSimulation xs = { results=List.map constructManaPossibilities xs }
      List.map constructSimulation redisValue

    LogLine.info ("Checking simulation cache for " + n) |> logger.Log
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
        let value = valueFunction <| uncompressSimulationResults (fromJson (r.GetValue(simulationKey n)))
        cache value
        Some value
      else
        None

  let averagesCheckCacheAndReact n =
    checkSimulationCacheAndReact averagesKey analysis.averageLandsPerTurn (stringifyUrlParam n)

  let mostCommonLandsCheckCacheAndReact n =
    checkSimulationCacheAndReact mostCommonLandsKey analysis.mostCommonLandScenariosPerTurn (stringifyUrlParam n)
