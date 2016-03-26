namespace Tests

open Xunit
open Swensen.Unquote
open Manacurve.Analysis
open Manacurve.Domain
open Manacurve.DomainTypes

module tests =
  [<Fact>]
  let ``When 2 is added to 2 expect 4``() =
    test <@ 2 + 2 = 4 @>

  [<Fact>]
  let ``When simulating mono-colour deck``() =
    let analysis = loadAnalysis 1 1
    let emptyDeck = { colour1=0; colour2=0; colour3=0; colour1Colour2=0; colour2Colour3=0; colour1Colour3=0 }
    let deck = createDeck { emptyDeck with colour1=60 }
    let simulations = analysis.simulateGames deck
    simulations =! [[[1; 0; 0]]]
