namespace Tests

open Xunit
open Swensen.Unquote
open FsCheck
open FsCheck.Xunit
open Manacurve.Analysis
open Manacurve.Domain
open Manacurve.DomainTypes

module SimulateGamesTests =
  let emptyDeck = { colour1=0; colour2=0; colour3=0; colour1Colour2=0; colour1Colour3=0; colour2Colour3=0 }
  let noManaInPlay (x : ManaInPlay) = x = { colour1=0; colour2=0; colour3=0 }
  let mono60Deck = createDeck { emptyDeck with colour1=60 }
  let fakeShuffle x = x
  let deckSizeMinusStartingHand = Gen.elements [0..53] |> Arb.fromGen

  // This should be written with a custom Arbitrary, which generates valid mana configurations
  [<Property>]
  let ``Deck creation``() =
    let upTo10 = Gen.elements [0..10]
    let upTo10ForEachLandType =
      List.replicate 6 upTo10
      |> Gen.sequence
      |> Arb.fromGen

    Prop.forAll upTo10ForEachLandType (fun ns ->
      let deck = createDeck { colour1=ns.[0];        colour2=ns.[1];        colour3=ns.[2];
                              colour1Colour2=ns.[3]; colour1Colour3=ns.[4]; colour2Colour3=ns.[5] }

      (List.filter (fun x -> x = Land(BasicLand Colour1)) deck).Length =! ns.[0]
      (List.filter (fun x -> x = Land(BasicLand Colour2)) deck).Length =! ns.[1]
      (List.filter (fun x -> x = Land(BasicLand Colour3)) deck).Length =! ns.[2]
      (List.filter (fun x -> x = Land(DualLand (Colour1,Colour2))) deck).Length =! ns.[3]
      (List.filter (fun x -> x = Land(DualLand (Colour1,Colour3))) deck).Length =! ns.[4]
      (List.filter (fun x -> x = Land(DualLand (Colour2,Colour3))) deck).Length =! ns.[5]
    )

  [<Property>]
  let ``No lands in deck always returns no lands in play``(simulationCount:PositiveInt) =
    Prop.forAll deckSizeMinusStartingHand (fun numberOfTurns ->
      let analysis = loadAnalysis simulationCount.Get numberOfTurns fakeShuffle
      let simulations = analysis.simulateGames <| createDeck emptyDeck
      simulations
        |> Seq.map (fun x -> x.results)
        |> Seq.concat
        |> Seq.forall (fun x -> noManaInPlay x.manaPossibilities.[0] )
        =! true
    )

  [<Property>]
  let ``In a 60 land deck, turn number always equals number of lands in play``() =
    Prop.forAll deckSizeMinusStartingHand (fun numberOfTurns ->
      let oneSimulationNTurns = loadAnalysis 1 numberOfTurns fakeShuffle
      let simulations = oneSimulationNTurns.simulateGames mono60Deck
      let turnNumber = [1..numberOfTurns] |> List.toSeq
      let manaInPlay =
        simulations
        |> Seq.map (fun x -> x.results) |> Seq.concat
        |> Seq.map (fun x -> x.manaPossibilities) |> Seq.concat

      Seq.forall2
        (fun turn mana -> turn = mana.colour1)
        turnNumber
        manaInPlay
      =! true
    )

  [<Property>]
  let ``Cannot have more lands in play than lands in deck``(simulationCount:PositiveInt) =
    let nSimulations53Turns = loadAnalysis simulationCount.Get 53 fakeShuffle
    Prop.forAll deckSizeMinusStartingHand (fun numberOfLands ->
      let deck = createDeck { emptyDeck with colour1=numberOfLands }
      let simulations = nSimulations53Turns.simulateGames deck
      let lastTurnManaPossibilities = simulations |> Seq.map (fun x -> x.results) |> Seq.concat |> Seq.last
      let manaInPlay = lastTurnManaPossibilities.manaPossibilities.[0]
      manaInPlay =! { colour1=numberOfLands; colour2=0; colour3=0 }
    )

  [<Property>]
  let ``Mono deck simulations return one mana possibility per turn``(simulationCount:PositiveInt) =
    let upTo60 = Gen.elements [0..60] |> Arb.fromGen
    Prop.forAll upTo60 (fun numberOfLands ->
      Prop.forAll deckSizeMinusStartingHand (fun numberOfTurns ->
        let nSimulationsMTurns = loadAnalysis simulationCount.Get numberOfTurns fakeShuffle
        let deck = createDeck { emptyDeck with colour1=numberOfLands }
        let simulations = nSimulationsMTurns.simulateGames deck
        let manaPossibilitiesAcrossAllTurnsAndSimulations =
          simulations
          |> Seq.map (fun x -> x.results)
          |> Seq.concat

        Seq.length (manaPossibilitiesAcrossAllTurnsAndSimulations) =! numberOfTurns*simulationCount.Get

        Seq.forall
          (fun (x : ManaPossibilities) -> x.manaPossibilities.Length = 1)
          manaPossibilitiesAcrossAllTurnsAndSimulations
        =! true
      )
    )

  [<Fact>]
  let ``Dual coloured lands return two mana possibilities on turn 1``() =
    let oneSimulation2Turns = loadAnalysis 1 2 fakeShuffle
    let deck = createDeck { emptyDeck with colour1Colour2=1; }
    let simulations = oneSimulation2Turns.simulateGames deck
    let turn1Possibilities = simulations.[0].results.[0].manaPossibilities

    turn1Possibilities.Length =! 2
    (List.filter (fun x -> x = { colour1=1; colour2=0; colour3=0 }) turn1Possibilities)
      .Length =! 1
    (List.filter (fun x -> x = { colour1=0; colour2=1; colour3=0 }) turn1Possibilities)
      .Length =! 1
