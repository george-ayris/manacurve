namespace Manacurve

module ApiTypes =

  (*type ApiDeckDescription = {
    white: int;
    blue: int;
    black: int;
    red: int;
    green: int;
    whiteBlue: int;
    blueBlack: int;
    blackRed: int;
    redGreen: int;
    greenWhite: int;
    whiteBlack: int;
    blueRed: int;
    blackGreen: int;
    redWhite: int;
    greenBlue: int;
  }*)

  type AverageLands = { averages: float list list }
  type LandScenarioAndProbability = { landScenario: int list; probability: float }
  type MostCommonLandScenarios = { mostCommonLandScenarios: LandScenarioAndProbability list list }
