namespace Manacurve

[<AutoOpen>]
module DomainTypes =

  type ManaColour = Colour1 | Colour2 | Colour3
  type Land = BasicLand of ManaColour | DualLand of ManaColour*ManaColour
  type Card = Land of ManaColour | NonLand
  type PlayerState =
    { hand: Card list; deck: Card list; lands: Card list }
  type DeckLandQuantities = { colour1: int; colour2: int; colour3: int }
