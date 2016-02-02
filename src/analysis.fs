namespace Manacurve.Domain

open Manacurve.Domain.Cards
open Manacurve.Domain.ListHelpers

module Analysis =
  let simulationCount = 10000
  let numberOfTurns = 10

  let applyAnalysis f deck =
    let landsInPlay = (fun (s : PlayerState) -> s.lands) >> numberOfLands
    let deckToLandsInPlay = deckToPlayedGame numberOfTurns >> List.map landsInPlay

    List.replicate simulationCount deck
    |> List.map deckToLandsInPlay
    |> transpose
    |> List.map f

  let averageLandsPerTurn deck =
    let toAverageLands = List.averageBy float
    applyAnalysis toAverageLands deck

  let landDistributionsPerTurn deck =
    let addMissingTurnsAtTheStart = function
      | ((turn, landCount)::rest) as x ->
        let missingTuples = List.map (fun x -> (x, float 0)) [1..(turn-1)]
        missingTuples @ x
      | x -> x

    let probabilityOfLand (xs: int seq) =
        let count = Seq.length xs |> float
        count / (float simulationCount)

    let toLandDistribution =
      Seq.sort
      >> (Seq.groupBy id)
      >> Seq.map (fun (x, y) -> (x, probabilityOfLand y))
      >> Seq.toList
      >> addMissingTurnsAtTheStart

    applyAnalysis toLandDistribution deck
