import Constants from '../constants'
import Utils from './utils'

function zeroPad(n) {
  var s = n+"";
  while (s.length < 2) s = "0" + s;
  return s;
}

function numberOfLandsToDeckId(numberOfLandsByColour) {
  var createDeckId = (acc, colour) => {
    var numberOfCurrentColour = zeroPad(numberOfLandsByColour[colour]);
    return acc + numberOfCurrentColour;
  };
  return Constants.Colours.reduce(createDeckId, "")
}

export default {
  createDeck(numberOfLandsByColour) {
    return $.ajax({
      type: "POST",
      url: "/deck",
      data: JSON.stringify(Utils.createApiColoursObject(numberOfLandsByColour)),
      contentType: "application/json; charset=utf-8"
    });
  },

  getAverages(numberOfLandsByColour) {
    return $.get('/deck/' + numberOfLandsToDeckId(numberOfLandsByColour) + '/averages');
  },

  getMostCommonScenarios(numberOfLandsByColour) {
    return $.get('/deck/' + numberOfLandsToDeckId(numberOfLandsByColour) + '/mostcommon');
  }
}
