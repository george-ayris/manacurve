namespace Manacurve.Domain

open System

module ListHelpers =
  let rec removeFirst pred list =
    match list with
      | x::xs when pred x -> xs
      | x::xs -> x::(removeFirst pred xs)
      | _ -> []

  let rec iterate f value = seq {
    yield  f value
    yield! iterate f (f value)
  }

  let rec transpose = function
    | (_::_)::_ as M ->
      List.map List.head M :: transpose (List.map List.tail M)
    | _ -> []

module Shuffle =
  let inline swap fst snd i =
     if i = fst then snd else
     if i = snd then fst else
     i

  let shuffleR items (rng: Random) =
     let rec shuffleTo items upTo =
        match upTo with
        | 0 -> items
        | _ ->
           let fst = rng.Next(upTo)
           let shuffled = Array.permute (swap fst (upTo - 1)) items
           shuffleTo shuffled (upTo - 1)
     let array = List.toArray items
     let length = Array.length array
     shuffleTo array length |> Array.toList

  let shuffle items =
    let rng = new Random()
    shuffleR items rng

module Cards =
  open ListHelpers
  open Shuffle

  type Card = Land | NonLand
  type PlayerState =
    { hand: Card list; deck: Card list; lands: Card list }

  let deckSize = 60
  let startingHandSize = 7

  let createPlayerState deck = {hand=[]; deck=deck; lands=[]}

  let createMonoDeck numberOfLands =
    let lands = List.replicate numberOfLands Land
    let nonLands = List.replicate (deckSize - numberOfLands) NonLand
    lands @ nonLands

  let isALand = function
    | Land -> true
    | NonLand -> false

  let numberOfLands cards = (List.filter isALand cards).Length

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
    let newState = drawHand startingHandSize {state with deck=shuffle state.deck}
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
