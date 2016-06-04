namespace Manacurve

open ListHelpers
open Lands

module Domain =
  let deckSize = 60
  let startingHandSize = 7

  let createPlayerState deck = {hand=[]; deck=deck; lands=[]; tappedLands=[] }

  let createDeck (x : DeckDescription) =
    let l = [x.colour1;        x.colour2;        x.colour3;
             x.colour1Colour2; x.colour1Colour3; x.colour2Colour3]
    let lands =
      [Land (BasicLand Colour1);
       Land (BasicLand Colour2);
       Land (BasicLand Colour3);
       Land (DualLand (Colour1,Colour2));
       Land (DualLand (Colour1,Colour3));
       Land (DualLand (Colour2,Colour3));]
      |> List.map2 List.replicate l
      |> List.concat

    let nonLandQuantity = deckSize - List.sum l
    let nonLands = List.replicate nonLandQuantity NonLand

    lands @ nonLands

  let landsNotBetween l u hand =
    let landsInHand = numberOfLands hand
    (l > landsInHand) || (landsInHand > u)

  let shouldMulligan (hand : list<Card>) =
    match hand.Length with
      | 7 -> landsNotBetween 2 5 hand
      | 6 -> landsNotBetween 2 4 hand
      | 5 -> landsNotBetween 1 4 hand
      | _ -> false

  let drawHand n { hand=h; deck=d; lands=l; tappedLands=tl } =
    let hand, deck = List.splitAt n d
    { hand=hand; deck=deck; lands=l; tappedLands=tl }

  let rec shuffleAndDrawHand shuffleF handSize state =
    let newState = drawHand handSize {state with deck=shuffleF state.deck}
    if shouldMulligan newState.hand
    then shuffleAndDrawHand shuffleF (handSize-1) state
    else newState

  let shuffleAndDrawOpeningHand shuffleF state =
    shuffleAndDrawHand shuffleF startingHandSize state

  let drawCard { hand=h; deck=d; lands=l; tappedLands=tl } =
    match d with
      | card::deck -> { hand=card::h; deck=deck; lands=l; tappedLands=tl }
      | []      -> raise (System.ArgumentException("Deck can't be empty"))

  let untapLands { hand=h; deck=d; lands=l; tappedLands=tl } =
    { hand=h; deck=d; lands=tl @ l; tappedLands=[] }

  let landToPlay hand landsAlreadyInPlay =
    let preferProposedLand proposedLand chosenLand =
      let numberOfEachColour = numberOfEachColourInPlay landsAlreadyInPlay
      let proposedLandMinColourNumber = List.min (numberOfEachColour proposedLand)
      let chosenLandMinColourNumber = List.min (numberOfEachColour chosenLand)

      if proposedLandMinColourNumber = chosenLandMinColourNumber
      then
        match (proposedLand, chosenLand) with
          | (DualLand(_,_), BasicLand(_)) -> true
          | _ -> false
      else if proposedLandMinColourNumber < chosenLandMinColourNumber
      then true
      else false

    let findLandWithLeastInPlay chosenLandOption card =
      match card with
        | NonLand -> chosenLandOption
        | Land(proposedLand) ->
          match chosenLandOption with
            | None -> Some proposedLand
            | Some chosenLand ->
              if preferProposedLand proposedLand chosenLand
              then Some proposedLand
              else chosenLandOption

    List.fold findLandWithLeastInPlay None hand

  let playLand { hand=h; deck=d; lands=l; tappedLands=tl } =
    match landToPlay h (l@tl) with
      | Some(x) ->
        match x with
          | BasicLand(_)  -> { hand=removeFirst ((=) (Land(x))) h; deck=d; lands=x::l; tappedLands=tl }
          | DualLand(_,_) -> { hand=removeFirst ((=) (Land(x))) h; deck=d; lands=l;    tappedLands=x::tl }
      | None -> { hand=h; deck=d; lands=l; tappedLands=tl }

  let playTurn = drawCard >> untapLands >> playLand

  let playTurns n state =
    iterate playTurn state
    |> Seq.take n
    |> Seq.toList

  let deckToPlayedGame shuffleF numberOfTurns =
    createPlayerState >>
    shuffleAndDrawOpeningHand shuffleF >>
    playTurns numberOfTurns
