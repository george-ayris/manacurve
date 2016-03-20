namespace Manacurve

open Suave
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors
open Suave.Filters
open Logary
open Logary.Logging
open Analysis
open ApiHelpers
open Cache

module Api =

  let logger = getLoggerByName "Manacurve"

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
  let mostCommonLands n =
    LogLine.info ("Request started - most common lands " + n.ToString()) |> logger.Log
    let mostCommonLands = mostCommonLandsCheckCacheAndReact n
    match mostCommonLands with
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
            GET >=> mostCommonLands d) ]
