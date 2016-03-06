import Dispatcher from '../dispatcher'
import Api from '../utils/api'
import Utils from '../utils/utils'
import Constants from '../constants'
const ActionTypes = Constants.ActionTypes

const errorHandler = actionType => {
  return error => {
    Dispatcher.dispatch({
      type: actionType,
      data: error.message || (error.status + ' ' + error.statusText)
    });
  };
};

export default {
  updateNumberOfLands(numberOfLandsByColour) {
    Dispatcher.dispatch({
      type: ActionTypes.NUMBER_OF_LANDS_UPDATED,
      data: numberOfLandsByColour
    });
  },

  runSimulation(numberOfLandsByColour) {
    var landsArray = Utils.createNumberOfLandsArray(numberOfLandsByColour);
    if (Utils.sum(landsArray) > 60) {
      Dispatcher.dispatch({
        type: ActionTypes.INVALID_SIMULATION,
        data: numberOfLandsByColour
      });
      return;
    }

    Dispatcher.dispatch({
      type: ActionTypes.STARTING_SIMULATION,
      data: numberOfLandsByColour
    });

    Api.createDeck(numberOfLandsByColour)
      .then(response => {
        Api.getAverages(numberOfLandsByColour)
          .then(response => {
            Dispatcher.dispatch({
              type: ActionTypes.AVERAGES_UPDATED,
              data: response.data.averages
            });
          }, errorHandler(ActionTypes.ANALYSIS_ERROR));

        Api.getMostCommonScenarios(numberOfLandsByColour)
          .then(response => {
            Dispatcher.dispatch({
              type: ActionTypes.MOST_COMMON_SCENARIOS_UPDATED,
              data: response.data.mostCommonLandScenarios
            });
          }, errorHandler(ActionTypes.ANALYSIS_ERROR));

      }, errorHandler(ActionTypes.SIMULATION_DIDNT_START));
  },

  updateSelectedTurn(selectedTurn) {
    Dispatcher.dispatch({
      type: ActionTypes.SELECTED_TURN_UPDATED,
      data: selectedTurn
    });
  },

  updateQueryNumbers(queryNumbers) {
    Dispatcher.dispatch({
      type: ActionTypes.QUERY_NUMBER_UPDATED,
      data: queryNumbers
    });
  }
}
