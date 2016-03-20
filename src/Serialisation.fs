namespace Manacurve

open System
open System.Collections.Generic
open Microsoft.FSharp.Reflection
open Newtonsoft.Json
open Newtonsoft.Json.Converters

(*-------------------------------------------------------------------------
Copyright (c) Paulmichael Blasucci.

This source code is subject to terms and conditions of the Apache License,
Version 2.0. A copy of the license can be found in the License.html file
at the root of this distribution.

By using this source code in any fashion, you are agreeing to be bound
by the terms of the Apache License, Version 2.0.

You must not remove this notice, or any other, from this software.
-------------------------------------------------------------------------*)

exception UnexpectedToken of JsonToken
exception InvalidPropertySet

[<AutoOpen>]
module internal Library =

  // in the JSON.NET library, "system" metadata names begin with a '$'
  let [<Literal>] JSON_ID   = "$id"   // uniquely identifies complex value
  let [<Literal>] JSON_REF  = "$ref"  // refers to unique identifier

  /// defines extensions which simplify using the JSON.NET serializer
  type Newtonsoft.Json.JsonSerializer with

    member self.IsTracking =
      self.PreserveReferencesHandling
          .HasFlag(PreserveReferencesHandling.Objects)

    member self.AddReference(name,value) =
        self.ReferenceResolver.AddReference(self,name,value)

    member self.HasReference(value) =
        self.ReferenceResolver.IsReferenced(self,value)

    member self.MakeReference(value) =
        self.ReferenceResolver.GetReference(self,value)

    member self.GetReference(name) =
        self.ReferenceResolver.ResolveReference(self,name)

  /// defines extensions which simplify using the JSON.NET writer
  type Newtonsoft.Json.JsonWriter with

    member self.WriteIndentity(serializer:JsonSerializer,value) =
      // { "$id" : <an-identity> }
      if serializer.IsTracking then
        let identity = serializer.MakeReference(value)
        self.WritePropertyName(JSON_ID)
        self.WriteValue(identity)

    member self.WriteReference(serializer:JsonSerializer,value) =
      // { "$ref" : <an-identity> }
      if serializer.HasReference(value) then
        let identity = serializer.MakeReference(value)
        self.WriteStartObject()
        self.WritePropertyName(JSON_REF)
        self.WriteValue(identity)
        self.WriteEndObject()

  //  analyzes a key/value collection to determine its purpose...
  //    a 'Ref' contains a single key/value pair with a of key "$ref"
  //    a 'Map' has one or more key/value pairs, but NOT a "$ref" pair
  //    'Empty' contains no key/value pairs
  //    'Invalid' has a "$ref" pair AND one or more other pairs
  let (|Ref|Map|Empty|Invalid|) data =
    match data |> Map.tryFindKey (fun k _ -> k = JSON_REF) with
    | Some(k) -> if data.Count = 1 then Ref(data.[k]) else Invalid
    | None    -> if data.Count > 0 then Map(data)     else Empty

  // since JSON only defines a single numeric data type, and JSON.NET
  // forces all numeric data to be either Int64 or Double, this function
  // attempts to coerce said Int64s and/or Doubles into the appropriate
  // integral, floating point, or enum value, based on supplied definition
  let coerceType (vType:Type) (value:obj) =
    if   vType.IsEnum            then Enum.ToObject (vType,value)
    elif vType = typeof<byte>    then Convert.ToByte    value |> box
    elif vType = typeof<uint16>  then Convert.ToUInt16  value |> box
    elif vType = typeof<uint32>  then Convert.ToUInt32  value |> box
    elif vType = typeof<sbyte>   then Convert.ToSByte   value |> box
    elif vType = typeof<int16>   then Convert.ToInt16   value |> box
    elif vType = typeof<int32>   then Convert.ToInt32   value |> box
    elif vType = typeof<float32> then Convert.ToSingle  value |> box
    elif vType = typeof<decimal> then Convert.ToDecimal value |> box
    else value // no type coercion required

  // simplifies raising UnexpectedToken exceptions
  let invalidToken (r:JsonReader) = raise <| UnexpectedToken(r.TokenType)

  //  given a JsonReader and a JsonSerializer, this function produces
  //  several helper functions which simplify working with the given
  //  reader and serializer.
  let makeHelpers (reader:JsonReader) (serializer:JsonSerializer) =
    let decode () = serializer.Deserialize(reader)
    let decode' t = serializer.Deserialize(reader,t)
    let advance = reader.Read >> ignore
    let readName () = let n = reader.Value |> string in advance (); n
    decode,decode',advance,readName

/// <summary>
/// A JSON.NET converter which can serialize/deserialize F# tuple values.
/// </summary>
type TupleConverter() =
  inherit JsonConverter()

  override __.CanRead  = true
  override __.CanWrite = true

  override __.CanConvert(vType) = FSharpType.IsTuple vType

  override __.WriteJson(writer,value,serializer) =
    match value with
    | null -> nullArg "value" // a 'null' tuple doesn't make sense!
    | data ->
        writer.WriteStartObject()

        let fields = value |> FSharpValue.GetTupleFields
        if fields.Length > 0 then
          // emit "system" metadata, if necessary
          if serializer.IsTracking then
            writer.WriteIndentity(serializer,value)

          fields |> Array.iteri (fun i v ->
            // emit name based on values position in tuple
            let n = sprintf "Item%i" (i + 1)
            writer.WritePropertyName(n)
            // emit value or reference thereto, if necessary
            if v <> null && serializer.HasReference(v)
              then writer.WriteReference(serializer,v)
              else serializer.Serialize(writer,v))

        writer.WriteEndObject()

  override __.ReadJson(reader,vType,_,serializer) =

    let decode,decode',advance,readName = makeHelpers reader serializer

    let readProperties (fields:Type[]) =
      let rec readProps index pairs =
        match reader.TokenType with
        | JsonToken.EndObject     -> pairs // no more pairs, return map
        | JsonToken.PropertyName  ->
            // get the key of the next key/value pair
            let name = readName ()
            let value,index' =  match name with
                                //  for "system" metadata, process normally
                                | JSON_ID | JSON_REF -> decode (),index
                                //  for tuple data...
                                //    use type info for current field
                                //    bump offset to the next type info
                                | _ -> decode' fields.[index],index+1
            advance ()
            // add decoded key/value pair to map and continue to next pair
            readProps (index') (pairs |> Map.add name value)
        | _ -> reader |> invalidToken
      advance ()
      readProps 0 Map.empty

    match reader.TokenType with
    | JsonToken.StartObject ->
        let fields = vType |> FSharpType.GetTupleElements
        // read all key/value pairs, reifying with tuple field types
        match readProperties fields with
        | Ref(trackingId) ->
            // tuple value is a reference, de-reference to actual value
            serializer.GetReference(string trackingId)
        | Map(data) ->
            let inputs =
              data
                // strip away "system" meta data
                |> Seq.filter (fun (KeyValue(k,_)) -> k <> JSON_ID)
                // discard keys, retain values
                |> Seq.map (fun (KeyValue(_,v)) -> v)
                // merge values with type info
                |> Seq.zip fields
                // marshal values to correct data types
                |> Seq.map (fun (t,v) -> v |> coerceType t)
                |> Seq.toArray
            // create tuple instance
            let value = FSharpValue.MakeTuple(inputs,vType)
            if serializer.IsTracking then
              match data |> Map.tryFindKey (fun k _ -> k = JSON_ID) with
              // use existing "$id"
              | Some(k) -> serializer.AddReference(string data.[k],value)
              // make a new "$id"
              | None    -> serializer.MakeReference(value) |> ignore
            value
        | _ -> raise InvalidPropertySet
    | _ -> reader |> invalidToken
