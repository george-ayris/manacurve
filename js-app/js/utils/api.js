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

var apiCache = {};

function cacheKey(numberOfLandsByColour, keySuffix) {
  return numberOfLandsToDeckId(numberOfLandsByColour) + keySuffix;
}

function addToCache(key) {
  return (data, textStatus, jqXHR) => {
    apiCache[key] = data;
  };
}

function checkCache(key) {
  return apiCache[key];
}

function checkCacheAnd(numberOfLandsByColour, keySuffix, f) {
  var key = cacheKey(numberOfLandsByColour, keySuffix);
  var cachedValue = checkCache(key);
  if (cachedValue) {
    return $.when(cachedValue);
  } else {
    return f().done(addToCache(key));
  }
}

export default {
  createDeck(numberOfLandsByColour) {
    return checkCacheAnd(numberOfLandsByColour, "", () =>
      $.ajax({
        type: "POST",
        url: "/deck",
        data: JSON.stringify(Utils.createApiColoursObject(numberOfLandsByColour)),
        contentType: "application/json; charset=utf-8"
      })
    );
  },

  getAverages(numberOfLandsByColour) {
    return checkCacheAnd(numberOfLandsByColour, ":averages",
      () => $.get('/deck/' + numberOfLandsToDeckId(numberOfLandsByColour) + '/averages'));
  },

  getMostCommonScenarios(numberOfLandsByColour) {
    return checkCacheAnd(numberOfLandsByColour, ":mostcommon",
      () => $.get('/deck/' + numberOfLandsToDeckId(numberOfLandsByColour) + '/mostcommon'));
  }
}
