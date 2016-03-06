import Constants from '../constants'
import Utils from './utils'
import Axios from 'axios'

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
  return (response) => {
    apiCache[key] = response.data;
  };
}

function checkCache(key) {
  return apiCache[key];
}

function checkCacheAnd(numberOfLandsByColour, keySuffix, f) {
  var key = cacheKey(numberOfLandsByColour, keySuffix);
  var cachedValue = checkCache(key);
  if (cachedValue) {
    return Promise.resolve({ data: cachedValue });
  } else {
    const promise = f();
    promise.then(addToCache(key));
    return promise;
  }
}

export default {
  createDeck(numberOfLandsByColour) {
    return checkCacheAnd(numberOfLandsByColour, "", () =>
      Axios.post('/deck', Utils.createApiColoursObject(numberOfLandsByColour))
    );
  },

  getAverages(numberOfLandsByColour) {
    return checkCacheAnd(numberOfLandsByColour, ":averages",
      () => Axios.get('/deck/' + numberOfLandsToDeckId(numberOfLandsByColour) + '/averages'));
  },

  getMostCommonScenarios(numberOfLandsByColour) {
    return checkCacheAnd(numberOfLandsByColour, ":mostcommon",
      () => Axios.get('/deck/' + numberOfLandsToDeckId(numberOfLandsByColour) + '/mostcommon'));
  }
}
