#load "manacurve/DomainTypes.fs"
#load "manacurve/ListHelpers.fs"
#load "manacurve/Lands.fs"
#load "manacurve/Domain.fs"
#load "manacurve/ApiTypes.fs"
#load "manacurve/Analysis.fs"

open Manacurve.DomainTypes
open Manacurve.Domain
open Manacurve.Lands
open Manacurve.Analysis

let d = createDeck { colour1=10; colour2=0; colour3=0;
  colour1Colour2=0; colour1Colour3=0; colour2Colour3=0}

let shuffle = fun x -> x

let analysis = loadAnalysis 3 6 shuffle

analysis.simulateGames d
