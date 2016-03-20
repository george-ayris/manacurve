namespace Manacurve

open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave
open Suave.Operators
open Suave.RequestErrors
open Suave.Successful

module ApiHelpers =

  let onlyTheseMethods webParts =
    choose (webParts @ [METHOD_NOT_ALLOWED ""])

  let toJson v =
    let jsonSerializerSettings = new JsonSerializerSettings()
    let converters = new System.Collections.Generic.List<JsonConverter>()
    converters.Add(new TupleConverter())
    jsonSerializerSettings.Converters <- converters
    jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()

    JsonConvert.SerializeObject(v, jsonSerializerSettings)

  let JSON v =
    toJson v
    |> OK
    >=> Writers.setMimeType "application/json; charset=utf-8"

  let fromJson<'a> json =
    JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a

  let getResourceFromReq<'a> (req : HttpRequest) =
    let getString rawForm =
      System.Text.Encoding.UTF8.GetString(rawForm)
    req.rawForm |> getString |> fromJson<'a>
