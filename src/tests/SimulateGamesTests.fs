namespace Tests

open Xunit
open Swensen.Unquote
open FsCheck
open FsCheck.Xunit
open Manacurve.Analysis
open Manacurve.Domain
open Manacurve.DomainTypes
open Manacurve.Lands

module SimulateGamesTests =
  let emptyDeck = { colour1=0; colour2=0; colour3=0; colour1Colour2=0; colour1Colour3=0; colour2Colour3=0 }
  let noManaInPlay = { colour1=0; colour2=0; colour3=0; count=1 }
  let mono60Deck = createDeck { emptyDeck with colour1=60 }
  let fakeShuffle x = x
  let deckSizeMinusStartingHand = Gen.elements [0..53] |> Arb.fromGen
  let twoTo20 = Gen.elements [2..20] |> Arb.fromGen

  let assertListContainsNOf n elem list =
    test <@ (List.filter (fun x -> x = elem) list).Length = n @>
  let assertListOnlyContainsOneOf = assertListContainsNOf 1

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

      assertListContainsNOf ns.[0] (Land(BasicLand Colour1)) deck
      assertListContainsNOf ns.[1] (Land(BasicLand Colour2)) deck
      assertListContainsNOf ns.[2] (Land(BasicLand Colour3)) deck
      assertListContainsNOf ns.[3] (Land(DualLand (Colour1,Colour2))) deck
      assertListContainsNOf ns.[4] (Land(DualLand (Colour1,Colour3))) deck
      assertListContainsNOf ns.[5] (Land(DualLand (Colour2,Colour3))) deck
    )

  [<Property>]
  let ``No lands in deck always returns no lands in play``(simulationCount:PositiveInt) =
    Prop.forAll deckSizeMinusStartingHand (fun numberOfTurns ->
      let analysis = loadAnalysis simulationCount.Get numberOfTurns fakeShuffle
      let simulations = analysis.simulateGames <| createDeck emptyDeck
      simulations
        |> Seq.map (fun x -> x.results)
        |> Seq.concat
        |> Seq.iter (fun x -> test <@ x.manaPossibilities.[0] = noManaInPlay @>)
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

      Seq.iter2
        (fun turn mana -> test <@ turn = mana.colour1 @>)
        turnNumber
        manaInPlay
    )

  [<Property>]
  let ``Cannot have more lands in play than lands in deck``(simulationCount:PositiveInt) =
    let nSimulations53Turns = loadAnalysis simulationCount.Get 53 fakeShuffle
    Prop.forAll deckSizeMinusStartingHand (fun numberOfLands ->
      let deck = createDeck { emptyDeck with colour1=numberOfLands }
      let simulations = nSimulations53Turns.simulateGames deck
      let lastTurnManaPossibilities = simulations |> Seq.map (fun x -> x.results) |> Seq.concat |> Seq.last
      test <@ lastTurnManaPossibilities.manaPossibilities.[0] = { colour1=numberOfLands; colour2=0; colour3=0; count=1 } @>
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

        test <@ Seq.length (manaPossibilitiesAcrossAllTurnsAndSimulations) = numberOfTurns*simulationCount.Get @>

        Seq.iter
          (fun (x : ManaPossibilities) -> test <@ x.manaPossibilities.Length = 1 @>)
          manaPossibilitiesAcrossAllTurnsAndSimulations
      )
    )

  [<Fact>]
  let ``Dual coloured lands - turns 1, 2 and 3``() =
    let oneSimulation2Turns = loadAnalysis 1 3 fakeShuffle
    let deck = createDeck { emptyDeck with colour1Colour2=2; }
    let simulations = oneSimulation2Turns.simulateGames deck
    let turn1Possibilities = simulations.[0].results.[0].manaPossibilities

    test <@ turn1Possibilities.Length = 1 @>
    test <@ turn1Possibilities.[0] = noManaInPlay @>

    let turn2Possibilities = simulations.[0].results.[1].manaPossibilities

    test <@ turn2Possibilities.Length = 2 @>
    assertListOnlyContainsOneOf { colour1=1; colour2=0; colour3=0; count=1 } turn2Possibilities
    assertListOnlyContainsOneOf { colour1=0; colour2=1; colour3=0; count=1 } turn2Possibilities

    let turn3Possibilities = simulations.[0].results.[2].manaPossibilities

    test <@ turn3Possibilities.Length = 3 @>
    assertListOnlyContainsOneOf { colour1=2; colour2=0; colour3=0; count=1 } turn3Possibilities
    assertListOnlyContainsOneOf { colour1=0; colour2=2; colour3=0; count=1 } turn3Possibilities
    assertListOnlyContainsOneOf { colour1=1; colour2=1; colour3=0; count=2 } turn3Possibilities

  [<Property>]
  let ``Dual coloured lands follow a binomial expansion with their mana possibilities``() =
    let factorial (n : bigint) : bigint =
      if n = bigint(0)
      then bigint(1)
      else [bigint(1)..n] |> List.reduce (*)
    let coefficient n k = (factorial n) / ((factorial (n - k)) * (factorial k))
    let binomial n = [0..n] |> List.map (fun x -> int32 (coefficient (bigint n) (bigint x)))

    Prop.forAll twoTo20 (fun numberOfTurns ->
      let oneSimulationNTurns = loadAnalysis 1 numberOfTurns fakeShuffle
      let deck = createDeck { emptyDeck with colour1Colour2=60; }
      let simulations = oneSimulationNTurns.simulateGames deck

      let assertAgainstLengthAndContents n (x : ManaPossibilities) =
        let binomialCoeffs = binomial n
        test <@ x.manaPossibilities.Length = binomialCoeffs.Length @>
        List.iteri
          (fun i binomialCoeff ->
            assertListOnlyContainsOneOf { colour1=i; colour2=(n-i); colour3=0; count=binomialCoeff } x.manaPossibilities)
            binomialCoeffs

      Seq.iteri assertAgainstLengthAndContents simulations.[0].results
    )

  [<Fact>]
  let ``Play mana in an even fashion``() =
    let oneSimulation6Turns = loadAnalysis 1 6 fakeShuffle
    let deck = createDeck { emptyDeck with colour1=3; colour2=3; }
    let simulations = oneSimulation6Turns.simulateGames deck

    let manaPossibiltiies = simulations.[0].results
    let turn1 = manaPossibiltiies.[0]
    test <@ turn1.manaPossibilities.Length = 1 @>
    assertListOnlyContainsOneOf
      turn1.manaPossibilities.[0]
      [{colour1=1; colour2=0; colour3=0; count=1; }
       {colour1=0; colour2=1; colour3=0; count=1; }]

    let turn2 = manaPossibiltiies.[1]
    assertListOnlyContainsOneOf {colour1=1; colour2=1; colour3=0; count=1; } turn2.manaPossibilities
