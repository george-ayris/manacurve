import assign from 'object-assign'
import Store from './store'
import Dispatcher from '../dispatcher'
import Constants from '../constants'
import Utils from '../utils/utils'
const ActionTypes = Constants.ActionTypes;

var state = {
  averages: [],
  mostCommonLandScenarios: [],
  numberOfEachColour: {
    Red: 8,
    Blue: 8,
    Green: 8
  },
  numberOfSimulationsRunning: 0,
  selectedTurn: null,
  simulationError: null,
  probability: 1,
  queryNumbers: {
    Red: 0,
    Blue: 0,
    Green: 0,
    Any: 0
  }
};

function calculateNewProbability(queryNumbers, landScenarios, selectedTurn) {
  if (selectedTurn || selectedTurn === 0) {
    var queryNumbersWithNoEmptyString =
      Utils.objectMap(queryNumbers, x => parseInt(x) || 0);

    var probabilityOfQuery = state.mostCommonLandScenarios[state.selectedTurn]
      .reduce((cumulativeProbability, scenario) => {

        if (queryMatchesScenario(queryNumbersWithNoEmptyString, scenario.landScenario)) {
          cumulativeProbability += scenario.probability;
        }
        return cumulativeProbability;
      } , 0);

    return Math.round(probabilityOfQuery*100)/100;

  } else {
    return 1;
  }
}

function queryMatchesScenario(queryNumbers, scenario) {
  var exactColourMatch = Constants.Colours.reduce((match, colour, i) => {
    var doesQueryMatchCurrentColour = queryNumbers[colour] <= scenario[i];
    return match && doesQueryMatchCurrentColour;
  }, true);

  var totalNumberOfExactColours = Utils.sum(Constants.Colours.map(colour => queryNumbers[colour]));
  var totalLandsInScenario = Utils.sum(scenario);
  var enoughLandsLeftToCoverAny = (totalLandsInScenario - totalNumberOfExactColours) >= queryNumbers['Any'];

  return exactColourMatch && enoughLandsLeftToCoverAny;
}

var LandsStore = assign({}, Store, {
  getState: () => { return state }
});

LandsStore.dispatchToken = Dispatcher.register(function(action) {
  if (Constants.Debug) {
    console.log(action.type, action.data);
  }

  switch(action.type) {

    case ActionTypes.NUMBER_OF_LANDS_UPDATED:
      state.error = null;
      state.numberOfEachColour = action.data;
      break;

    case ActionTypes.STARTING_SIMULATION:
      state.numberOfSimulationsRunning += 2;
      break;

    case ActionTypes.AVERAGES_UPDATED:
      state.averages = action.data;
      state.numberOfSimulationsRunning -= 1;
      break;

    case ActionTypes.MOST_COMMON_SCENARIOS_UPDATED:
      state.mostCommonLandScenarios = action.data;
      state.numberOfSimulationsRunning -= 1;
      state.probability = calculateNewProbability(state.queryNumbers, state.mostCommonLandScenarios, state.selectedTurn);
      break;

    case ActionTypes.SIMULATION_DIDNT_START:
      state.numberOfSimulationsRunning -= 2;
      break;

    case ActionTypes.ANALYSIS_ERROR:
      state.numberOfSimulationsRunning -= 1;
      break;

    case ActionTypes.SELECTED_TURN_UPDATED:
      state.selectedTurn = action.data;
      state.probability = calculateNewProbability(state.queryNumbers, state.mostCommonLandScenarios, state.selectedTurn);
      break;

    case ActionTypes.INVALID_SIMULATION:
      state.simulationError = "Invalid deck";
      break;

    case ActionTypes.QUERY_NUMBER_UPDATED:
      state.queryNumbers = action.data;
      state.probability = calculateNewProbability(state.queryNumbers, state.mostCommonLandScenarios, state.selectedTurn);
      break;

    default:
      return true;
  }

  LandsStore.emitChange();
  return true;
});

export default LandsStore;
