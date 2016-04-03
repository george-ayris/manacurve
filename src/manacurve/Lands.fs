namespace Manacurve

module Lands =
  let isALand = function
    | (Land _) -> true
    | NonLand -> false

  let isBasicLandOf c x = x = (Land (BasicLand c))
  let isBasicLand x =
    match x with
      | Land(BasicLand(_)) -> true
      | _ -> false

  let isDualLand x =
    match x with
      | Land(DualLand(_,_)) -> true
      | _ -> false

  let numberOfLands cards = (List.filter isALand cards).Length

  let landsInPlayToManaPossibilities lands  =
    let addColourToManaInPlay c (manaInPlay : ManaInPlay) =
      match c with
        | Colour1 -> { manaInPlay with colour1 = manaInPlay.colour1 + 1 }
        | Colour2 -> { manaInPlay with colour2 = manaInPlay.colour2 + 1 }
        | Colour3 -> { manaInPlay with colour3 = manaInPlay.colour3 + 1 }


    let folder manaPossibilities (card : Card) =
      match card with
        | Land(x) ->
          match x with
            | BasicLand(c) -> List.map (addColourToManaInPlay c) manaPossibilities
            | DualLand(c1,c2) ->
              let c1Option = List.map (addColourToManaInPlay c1) manaPossibilities
              let c2Option = List.map (addColourToManaInPlay c2) manaPossibilities
              c1Option @ c2Option
        | NonLand -> manaPossibilities

    { manaPossibilities = List.fold folder [{ colour1=0; colour2=0; colour3=0; }] lands }
