namespace Manacurve

open ListHelpers

module Domain =
  let deckSize = 60
  let startingHandSize = 7

  let createPlayerState deck = {hand=[]; deck=deck; lands=[]}

  let createDeck landQuantities =
    let lands1 = List.replicate landQuantities.colour1 (Land Colour1)
    let lands2 = List.replicate landQuantities.colour2 (Land Colour2)
    let lands3 = List.replicate landQuantities.colour3 (Land Colour3)
    let nonLandQuantity = (deckSize - landQuantities.colour1 - landQuantities.colour2 - landQuantities.colour3)
    let nonLands = List.replicate nonLandQuantity NonLand
    lands1 @ lands2 @ lands3 @ nonLands

  let isALand = function
    | (Land _) -> true
    | NonLand -> false

  let numberOfLands cards = (List.filter isALand cards).Length

  let isColour1 = function
    | Land(c) -> match c with
                  | Colour1 -> true
                  | Colour2 -> false
                  | Colour3 -> false
    | NonLand -> false

  let isColour2 = function
    | Land(c) -> match c with
                  | Colour1 -> false
                  | Colour2 -> true
                  | Colour3 -> false
    | NonLand -> false

  let isColour3 = function
    | Land(c) -> match c with
                  | Colour1 -> false
                  | Colour2 -> false
                  | Colour3 -> true
    | NonLand -> false

  let numberOfLandsByColour cards =
    let c1 = (List.filter isColour1 cards).Length
    let c2 = (List.filter isColour2 cards).Length
    let c3 = (List.filter isColour3 cards).Length
    [c1; c2; c3]

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

  let rec shuffleAndDrawHand handSize state =
    let newState = drawHand handSize {state with deck=shuffle state.deck}
    if shouldMulligan newState.hand
    then shuffleAndDrawHand (handSize-1) state
    else newState

  let shuffleAndDrawOpeningHand state =
    shuffleAndDrawHand startingHandSize state

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

  let deckToPlayedGame numberOfTurns =
    createPlayerState >>
    shuffleAndDrawOpeningHand >>
    playTurns numberOfTurns
