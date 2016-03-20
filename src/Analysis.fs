namespace Manacurve

open Domain

module Analysis =

  let simulationCount = 10000
  let numberOfTurns = 10

  let simulateGames deck =
    let landsInPlay = (fun (s : PlayerState) -> s.lands) >> numberOfLandsByColour
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
