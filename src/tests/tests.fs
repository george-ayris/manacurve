namespace Tests

open Xunit
open Swensen.Unquote

module tests =
  [<Fact>]
  let ``When 2 is added to 2 expect 4``() =
    test <@ 2 + 2 = 4 @>
