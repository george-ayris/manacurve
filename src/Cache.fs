namespace Manacurve

open ApiHelpers
open Logary
open ServiceStack.Redis
open Domain
open Analysis

module Cache =

  let redisUrl =
    let redisUrl = System.Environment.GetEnvironmentVariable("REDIS_URL")
    if redisUrl = null
    then "redis://localhost:6379"
    else redisUrl

  let logger = Logging.getLoggerByName "Manacurve"

  let encodeLandColours (l : DeckLandQuantities) = l.colour1 * 10000 + l.colour2 * 100 + l.colour3
  let simulationKey n = "simulation:" + (sprintf "%06i" n)
  let averagesKey n = simulationKey n + ":averages"
  let mostCommonLandsKey n = simulationKey n + ":mostcommon"
  let otherKey key n =
    if key = (averagesKey n)
    then mostCommonLandsKey n
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

  let mostCommonLandsCheckCacheAndReact n =
    checkSimulationCacheAndReact mostCommonLandsKey mostCommonLandScenariosPerTurn n
