import assign from 'object-assign'
import Store from './store'
import Dispatcher from '../dispatcher'
import Constants from '../constants'
const ActionTypes = Constants.ActionTypes;

var state = {
  averages: [],
  mostCommonLandScenarios: [],
  numberOfColour1: 20,
  numberOfColour2: 0,
  numberOfSimulationsRunning: 0,
  selectedTurn: null,
};

var LandsStore = assign({}, Store, {
  getState: () => { return state }
});

LandsStore.dispatchToken = Dispatcher.register(function(action) {
  switch(action.type) {

    case ActionTypes.NUMBER_OF_LANDS_UPDATED:
      // TODO: Change state to have an array of colours
      state.numberOfColour1 = action.data[0];
      state.numberOfColour2 = action.data[1];
      break;

    case ActionTypes.AVERAGES_UPDATED:
      state.averages = action.data;
      break;

    case ActionTypes.MOST_COMMON_SCENARIOS_UPDATED:
      state.mostCommonLandScenarios = action.data;
      break;
      
    default:
      return true;
  }

  LandsStore.emitChange();
  return true;
});

export default LandsStore;
