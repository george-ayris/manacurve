import Constants from '../constants'

function createObject(numberOfLandsByColour, keyFunction) {
  var createData = (obj, colour, currentIndex) => {
    var key = keyFunction(colour, currentIndex);
    obj[key] = numberOfLandsByColour[colour];
    return obj;
  }
  return Constants.Colours.reduce(createData, {});
}

export default {
  createApiColoursObject(numberOfLandsByColour) {
    return createObject(numberOfLandsByColour, (_, index) => { return "colour" + (index+1); });
  },
  createColoursObject(numberOfLandsByColour) {
    return createObject(numberOfLandsByColour, (colour, _) => { return colour; });
  }
}
