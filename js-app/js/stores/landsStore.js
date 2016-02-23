import assign from 'object-assign'
import Store from './store'
import Dispatcher from '../dispatcher'
import Constants from '../constants'
const ActionTypes = Constants.ActionTypes;

var state = {
  averages: [],
  mostCommonLandScenarios: [],
  numberOfEachColour: {
    Red: 12,
    Blue: 12
  },
  numberOfSimulationsRunning: 0,
  selectedTurn: null,
  error: null
};

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
      break;

    case ActionTypes.SIMULATION_DIDNT_START:
      state.numberOfSimulationsRunning -= 2;
      break;

    case ActionTypes.ANALYSIS_ERROR:
      state.numberOfSimulationsRunning -= 1;
      break;

    case ActionTypes.SELECTED_TURN_UPDATED:
      state.selectedTurn = action.data;
      break;

    case ActionTypes.INVALID_SIMULATION:
      state.error = "Invalid deck";
      break;

    default:
      return true;
  }

  LandsStore.emitChange();
  return true;
});

export default LandsStore;
