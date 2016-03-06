import React from 'react'
const PropTypes = React.PropTypes;
import Constants from '../constants'
import LandsActions from '../actions/landsActionCreators'
import IfATurnHasBeenSelected from './ifATurnHasBeenSelected'

const ProbabilityQuery = props => {
  var selectedTurn = selectedTurn == null ? '?' : props.selectedTurn + 1;
  return (
    <div>
      <div>
        On turn {selectedTurn}, what is the probability of:
        {renderColourInputs(props)}
        {renderInput("Any", props)}
      </div>
      <span>Result: </span>
      <IfATurnHasBeenSelected selectedTurn={props.selectedTurn}>
         {props.probability}
      </IfATurnHasBeenSelected>
    </div>
  );
};

const renderColourInputs = props => Constants.Colours.map(c => renderInput(c, props));

const renderInput = (colour, props) => (
  <div key={"probability-" + colour}>
    <label htmlFor={"probability-" + colour}>{colour}</label>
    <input id={"probability-" + colour} onChange={e => props.onQueryNumbersChanged(colour, e)} value={props.queryNumbers[colour]}/>
  </div>
);

ProbabilityQuery.propTypes = {
  queryNumbers: PropTypes.object.isRequired,
  currentTurn: PropTypes.number,
  probability: PropTypes.number,
  onQueryNumbersChanged: PropTypes.func.isRequired
};

export default ProbabilityQuery;
