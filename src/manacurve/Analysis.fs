namespace Manacurve

open Lands
open Domain
open ApiTypes

module Analysis =
  type T = {
    simulateGames: Card list -> int list list list;
    averageLandsPerTurn: int list list list -> float list list;
    mostCommonLandScenariosPerTurn: int list list list -> (LandScenario*Probability) list list
  }

  let loadAnalysis simulationCount numberOfTurns =

    let simulateGames deck =
      let landsInPlay = (fun (s : PlayerState) -> s.lands) >> amountOfManaByColour
      let deckToLandsInPlay = deckToPlayedGame numberOfTurns >> List.map landsInPlay

      List.replicate simulationCount deck
      |> List.map deckToLandsInPlay

    let applyAnalysis f landsInPlay =
      landsInPlay |> transpose |> List.map f

    let averageLandsPerTurn (simulationResults : int list list list) =
      let toColour1Averages = List.averageBy (fun (lands : int list) -> float lands.[0])
      let toColour2Averages = List.averageBy (fun (lands : int list) -> float lands.[1])
      let toColour3Averages = List.averageBy (fun (lands : int list) -> float lands.[2])
      let colour1Averages = applyAnalysis toColour1Averages simulationResults
      let colour2Averages = applyAnalysis toColour2Averages simulationResults
      let colour3Averages = applyAnalysis toColour3Averages simulationResults
      transpose [colour1Averages; colour2Averages; colour3Averages]

    let mostCommonLandScenariosPerTurn simulationResults =
      let probabilityOfLand xs =
          let count = Seq.length xs |> float
          count / (float simulationCount)

      let toMostCommonLandScenarios =
        (Seq.groupBy id)
        >> Seq.map (fun (x,y) -> (x, probabilityOfLand y))
        >> Seq.toList
        >> List.sortBy (snd >> (fun x -> -x))

      applyAnalysis toMostCommonLandScenarios simulationResults

    {
      simulateGames = simulateGames
      averageLandsPerTurn = averageLandsPerTurn
      mostCommonLandScenariosPerTurn = mostCommonLandScenariosPerTurn
    }
