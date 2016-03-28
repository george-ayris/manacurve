namespace Manacurve

open Lands
open Domain
open ApiTypes

module Analysis =
  type T = {
    simulateGames: Card list -> ManaInPlay list list;
    averageLandsPerTurn: ManaInPlay list list -> float list list;
    mostCommonLandScenariosPerTurn: ManaInPlay list list -> (LandScenario*Probability) list list
  }

  let loadAnalysis simulationCount numberOfTurns shuffleF =

    let simulateGames deck =
      let landsInPlay =  (fun (s : PlayerState) -> s.lands)
      let deckToLandsInPlay = deckToPlayedGame shuffleF numberOfTurns >> List.map landsInPlay
      let simulations = List.replicate simulationCount deck |> List.map deckToLandsInPlay

      let manaInPlayByTurn = List.map manaInPlay [1..numberOfTurns]

      let calculateManaInPlayPerTurn simulation =
        List.map2
          (fun g s -> g s)
          manaInPlayByTurn
          simulation
      List.map calculateManaInPlayPerTurn simulations

    let applyAnalysis f landsInPlay =
      landsInPlay |> transpose |> List.map f

    let averageLandsPerTurn (simulationResults : ManaInPlay list list) =
      let toColour1Averages = List.averageBy (fun (lands : ManaInPlay) -> float lands.colour1)
      let toColour2Averages = List.averageBy (fun (lands : ManaInPlay) -> float lands.colour2)
      let toColour3Averages = List.averageBy (fun (lands : ManaInPlay) -> float lands.colour3)
      let colour1Averages = applyAnalysis toColour1Averages simulationResults
      let colour2Averages = applyAnalysis toColour2Averages simulationResults
      let colour3Averages = applyAnalysis toColour3Averages simulationResults
      transpose [colour1Averages; colour2Averages; colour3Averages]

    let mostCommonLandScenariosPerTurn simulationResults =
      let probabilityOfLand xs =
          let count = Seq.length xs |> float
          count / (float simulationCount)

      let manaInPlayToLandScenario (x : ManaInPlay) =
        [x.colour1; x.colour2; x.colour3]

      let toMostCommonLandScenarios =
        (Seq.groupBy id)
        >> Seq.map (fun (x,y) -> (manaInPlayToLandScenario x, probabilityOfLand y))
        >> Seq.toList
        >> List.sortBy (snd >> (fun x -> -x))

      applyAnalysis toMostCommonLandScenarios simulationResults

    { simulateGames = simulateGames
      averageLandsPerTurn = averageLandsPerTurn
      mostCommonLandScenariosPerTurn = mostCommonLandScenariosPerTurn }
