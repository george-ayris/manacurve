function numberOfLandsToDeckId(numberOfLandsByColour) {
  return parseInt(numberOfLandsByColour['Red']) * 100 + parseInt(numberOfLandsByColour['Blue']);
}

export default {
  createDeck(numberOfLandsByColour) {
    return $.ajax({
      type: "POST",
      url: "/deck",
      data: JSON.stringify({
        colour1: numberOfLandsByColour[0],
        colour2: numberOfLandsByColour[1]
      }),
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
