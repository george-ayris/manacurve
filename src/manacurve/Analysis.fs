namespace Manacurve

open Lands
open Domain
open ApiTypes

module Analysis =
  type T = {
    simulateGames: Card list -> Simulation list;
    averageLandsPerTurn: Simulation list -> float list list;
    mostCommonLandScenariosPerTurn: Simulation list -> (LandScenario*Probability) list list
  }

  let loadAnalysis simulationCount numberOfTurns shuffleF =

    let simulateGames deck =
      let landsInPlay =  (fun (s : PlayerState) -> s.lands)
      let deckToLandsInPlay = deckToPlayedGame shuffleF numberOfTurns >> List.map landsInPlay
      let landsInPlayPerTurnPerSimulation = List.replicate simulationCount deck |> List.map deckToLandsInPlay

      let calculateSimulationResults landsInPlayPerTurn =
        { results = List.map landsInPlayToManaPossibilities landsInPlayPerTurn }

      List.map calculateSimulationResults landsInPlayPerTurnPerSimulation


    let applyAnalysis manaPossibilitiesToIntermediateType processIntermediateTypeByTurn simulations =
      let simulationToIntermediateType simulation = List.map manaPossibilitiesToIntermediateType simulation.results

      simulations
      |> List.map simulationToIntermediateType
      |> transpose
      |> List.map processIntermediateTypeByTurn


    let averageLandsPerTurn (simulations : Simulation list) =
      let combineManaPossibilitiesIntoSingleAverage (x : ManaPossibilities) =
        let folder acc elem =
          let runningTotal = fst acc
          let count = snd acc
          ({ colour1 = runningTotal.colour1 + elem.colour1;
             colour2 = runningTotal.colour2 + elem.colour2;
             colour3 = runningTotal.colour3 + elem.colour3; }
          , count + 1)

        let totalledManaInPlay = List.fold folder ({colour1=0; colour2=0; colour3=0},0) x.manaPossibilities
        let average field = float (field (fst totalledManaInPlay)) / float (snd totalledManaInPlay)
        [ average (fun x -> x.colour1);
          average (fun x -> x.colour2);
          average (fun x -> x.colour3) ]

      let overallAverageForEachTurn (averagePerSimulationForAGivenTurn : float list list) =
        let averagesCollectedByColour = transpose averagePerSimulationForAGivenTurn
        List.map List.average averagesCollectedByColour

      applyAnalysis combineManaPossibilitiesIntoSingleAverage overallAverageForEachTurn simulations


    let mostCommonLandScenariosPerTurn (simulations : Simulation list) =
      let manaPossibilitiesToLandScenarioList (x : ManaPossibilities) =
        List.map (fun y -> [y.colour1; y.colour2; y.colour3]) x.manaPossibilities

      let probabilityOfLand xs =
          let count = Seq.length xs |> float
          count / (float simulationCount)

      let groupLandScenarios (landScenariosListForAGivenTurn : LandScenario list list) =
        landScenariosListForAGivenTurn
        |> Seq.concat
        |> (Seq.groupBy id)
        |> Seq.map (fun (x,y) -> (x, probabilityOfLand y))
        |> Seq.toList
        |> List.sortBy (snd >> (fun x -> -x))

      applyAnalysis manaPossibilitiesToLandScenarioList groupLandScenarios simulations

    { simulateGames = simulateGames
      averageLandsPerTurn = averageLandsPerTurn
      mostCommonLandScenariosPerTurn = mostCommonLandScenariosPerTurn }
