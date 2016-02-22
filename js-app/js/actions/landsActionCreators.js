import Dispatcher from '../dispatcher'
import Api from '../utils/api'
import Constants from '../constants'
const ActionTypes = Constants.ActionTypes

export default {
  updateNumberOfLands(numberOfLandsByColour) {
    Dispatcher.dispatch({
      type: ActionTypes.NUMBER_OF_LANDS_UPDATED,
      data: numberOfLandsByColour
    });

    Api.createDeck(numberOfLandsByColour)
      .done((data, textStatus, jqXHR) => {
        Api.getAverages(numberOfLandsByColour)
          .done((data, textStatus, jqXHR) => {
            Dispatcher.dispatch({
              type: ActionTypes.AVERAGES_UPDATED,
              data: data.averages
            });

          }).fail((jqXHR, textStatus, errorThrown) => {
            Dispatcher.dispatch({
              type: ActionTypes.ANALYSIS_ERROR,
              data: textStatus
            });
          });

          Api.getMostCommonScenarios(numberOfLandsByColour)
            .done((data, textStatus, jqXHR) => {
              Dispatcher.dispatch({
                type: ActionTypes.MOST_COMMON_SCENARIOS_UPDATED,
                data: data.mostCommonLandScenarios
              });

            }).fail((jqXHR, textStatus, errorThrown) => {
              Dispatcher.dispatch({
                type: ActionTypes.ANALYSIS_ERROR,
                data: textStatus
              });
            });

      }).fail((jqXHR, textStatus, errorThrown) => {
        Dispatcher.dispatch({
          type: ActionTypes.SIMULATION_DIDNT_START,
          data: textStatus
        });
      });
  },

  updateSelectedTurn(selectedTurn) {
    Dispatcher.dispatch({
      type: ActionTypes.SELECTED_TURN_UPDATED,
      data: selectedTurn
    });
  }
}
