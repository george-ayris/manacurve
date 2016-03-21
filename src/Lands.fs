namespace Manacurve

open DomainTypes

module Lands =
  let isALand = function
    | (Land _) -> true
    | NonLand -> false

  let isColour c x =
    let isBasicLand = x = (Land (BasicLand c))
    let isDualLand =
      match x with
        | Land(DualLand (c1,c2)) ->
          c1 = c || c2 = c
        | _ -> false
    isBasicLand || isDualLand

  let numberOfLands cards = (List.filter isALand cards).Length

  let amountOfManaByColour cards =
    let c1 = (List.filter (isColour Colour1) cards).Length
    let c2 = (List.filter (isColour Colour2) cards).Length
    let c3 = (List.filter (isColour Colour3) cards).Length
    [c1; c2; c3]
