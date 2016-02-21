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
            console.log('ERROR: getAverages - jqXHR: ', jqXHR);
            console.log('ERROR: getAverages - textStatus: ', textStatus);
            console.log('ERROR: getAverages - errorThrown: ', errorThrown);
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
              console.log('ERROR: getMostCommonScenarios - jqXHR: ', jqXHR);
              console.log('ERROR: getMostCommonScenarios - textStatus: ', textStatus);
              console.log('ERROR: getMostCommonScenarios - errorThrown: ', errorThrown);
              Dispatcher.dispatch({
                type: ActionTypes.ANALYSIS_ERROR,
                data: textStatus
              });
            });
      }).fail((jqXHR, textStatus, errorThrown) => {
        console.log('ERROR: createDeck - jqXHR: ', jqXHR);
        console.log('ERROR: createDeck - textStatus: ', textStatus);
        console.log('ERROR: createDeck - errorThrown: ', errorThrown);
        Dispatcher.dispatch({
          type: ActionTypes.SIMULATION_DIDNT_START,
          data: textStatus
        });
      });
    // then call analysis endpoints
    // each with a then that dispatches an event
    // do some error handling (dispatching events as appropriate)
  }
}
