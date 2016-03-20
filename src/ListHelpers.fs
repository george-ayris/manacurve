namespace Manacurve

open System

[<AutoOpen>]
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
