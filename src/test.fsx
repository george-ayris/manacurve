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

let factorial (n : int64) : int64 =
  if n = int64(0)
  then int64(1)
  else [1L..n] |> List.reduce (*)
let coefficient n k = (factorial n) / ((factorial (n - k)) * (factorial k))
let binomial n = [0..n] |> List.map (fun x -> int32 (coefficient (int64 n) (int64 x)))
factorial 13L
coefficient 13 1
binomial 13
