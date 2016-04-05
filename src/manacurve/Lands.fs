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

  let totalManaInPlay manaInPlay =
    manaInPlay.colour1 + manaInPlay.colour2 + manaInPlay.colour3

  let landsInPlayToManaPossibilities lands  =
    let addColourToManaInPlay c (manaInPlay : ManaInPlay) =
      match c with
          | Colour1 -> { manaInPlay with colour1 = manaInPlay.colour1 + 1 }
          | Colour2 -> { manaInPlay with colour2 = manaInPlay.colour2 + 1 }
          | Colour3 -> { manaInPlay with colour3 = manaInPlay.colour3 + 1 }

    let groupLikeManaInPlay manaInPlays =
      let groupedByMana =
        List.groupBy
          (fun {colour1=c1; colour2=c2; colour3=c3; count=count;} -> (c1,c2,c3))
          manaInPlays

      let sumCountOfGroups ((c1,c2,c3), listOfLikeMana) =
        let count = List.sumBy (fun manaInPlay -> manaInPlay.count) listOfLikeMana
        {colour1=c1; colour2=c2; colour3=c3; count=count }

      List.map sumCountOfGroups groupedByMana

    let folder manaInPlayWithCount l =
      match l with
        | BasicLand(c)    -> List.map (addColourToManaInPlay c) manaInPlayWithCount
        | DualLand(c1,c2) ->
          let c1Option = List.map (addColourToManaInPlay c1) manaInPlayWithCount
          let c2Option = List.map (addColourToManaInPlay c2) manaInPlayWithCount
          groupLikeManaInPlay <| c1Option @ c2Option

    { manaPossibilities = List.fold folder [{ colour1=0; colour2=0; colour3=0; count=1 }] lands }
