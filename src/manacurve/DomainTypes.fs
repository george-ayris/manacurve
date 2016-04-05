namespace Manacurve

[<AutoOpen>]
module DomainTypes =

  type ManaColour = Colour1 | Colour2 | Colour3
  type Land = BasicLand of ManaColour | DualLand of ManaColour*ManaColour
  type Card = Land of Land | NonLand
  type PlayerState =
    { hand: Card list; deck: Card list; lands: Land list; tappedLands: Land list }
  type DeckDescription =
    { colour1: int;
      colour2: int;
      colour3: int;
      colour1Colour2: int;
      colour1Colour3: int;
      colour2Colour3: int; }
  type ManaInPlay =
    { colour1: int; colour2: int; colour3: int; count: int; }
  type ManaPossibilities =
    { manaPossibilities: ManaInPlay list }
  type Simulation =
    { results: ManaPossibilities list }
