namespace Manacurve

open ListHelpers
open Lands

module Domain =
  let deckSize = 60
  let startingHandSize = 7

  let createPlayerState deck = {hand=[]; deck=deck; lands=[]}

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

  let drawHand n {hand=h; deck=d; lands=l} =
    let hand, deck = List.splitAt n d
    {hand=hand; deck=deck; lands=l}

  let rec shuffleAndDrawHand shuffleF handSize state =
    let newState = drawHand handSize {state with deck=shuffleF state.deck}
    if shouldMulligan newState.hand
    then shuffleAndDrawHand shuffleF (handSize-1) state
    else newState

  let shuffleAndDrawOpeningHand shuffleF state =
    shuffleAndDrawHand shuffleF startingHandSize state

  let drawCard {hand=h; deck=d; lands=l} =
    match d with
      | card::deck -> {hand=card::h; deck=deck; lands=l}
      | []      -> raise (System.ArgumentException("Deck can't be empty"))

  let playLand {hand=h; deck=d; lands=l} =
    let landToPlay = List.tryFind isALand h
    match landToPlay with
      | Some(x) -> {hand=removeFirst isALand h; deck=d; lands=x::l}
      | None    -> {hand=h; deck=d; lands=l}

  let playTurn = drawCard >> playLand

  let playTurns n state =
    iterate playTurn state
    |> Seq.take n
    |> Seq.toList

  let deckToPlayedGame shuffleF numberOfTurns =
    createPlayerState >>
    shuffleAndDrawOpeningHand shuffleF >>
    playTurns numberOfTurns
