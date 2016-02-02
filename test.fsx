#load "src/domain.fs"
//#load "src/analysis.fs"

open Manacurve.Domain.Cards
open Manacurve.Domain.ListHelpers
//open Manacurve.Domain.Analysis

let simulationCount = 100
let numberOfTurns = 10
let deck = createMonoDeck 20

let landsInPlay = (fun (s : PlayerState) -> s.lands) >> numberOfLands
let deckToLandsInPlay = deckToPlayedGame numberOfTurns >> List.map landsInPlay

let addMissingTurnsAtTheStart = function
  | ((turn, landCount)::rest) as x ->
    let missingTuples = List.map (fun x -> (x, float 0)) [1..(turn-1)]
    missingTuples @ x
  | x -> x

let probabilityOfLand (xs: int seq) =
    let count = Seq.length xs |> float
    count / (float simulationCount)

let toProbabilityDistribution =
  Seq.sort
  >> (Seq.groupBy id)
  >> Seq.map (fun (x, y) -> (x, probabilityOfLand y))
  >> Seq.toList
  >> addMissingTurnsAtTheStart

List.replicate simulationCount deck
|> List.map deckToLandsInPlay
|> transpose
|> List.map toProbabilityDistribution
