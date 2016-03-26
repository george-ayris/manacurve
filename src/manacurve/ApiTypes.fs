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
  type LandScenario = int list
  type Probability = float
  type AverageLands = { averages: float list list }
  type LandScenarioAndProbability = { landScenario: LandScenario; probability: Probability }
  type MostCommonLandScenarios = { mostCommonLandScenarios: LandScenarioAndProbability list list }
