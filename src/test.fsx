#load "api/DomainTypes.fs"
#load "api/ListHelpers.fs"
#load "api/Lands.fs"
#load "api/Domain.fs"
//#load "src/analysis.fs"

open Manacurve.DomainTypes
open Manacurve.Domain
open Manacurve.Lands

let d = createDeck { colour1=1; colour2=1; colour3=1;
  colour1Colour2=1; colour1Colour3=4; colour2Colour3=1}

numberOfLands d
numberOfLandsByColour d