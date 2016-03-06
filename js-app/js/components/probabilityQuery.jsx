import React from 'react'
const PropTypes = React.PropTypes;
import Constants from '../constants'
import LandsActions from '../actions/landsActionCreators'
import IfATurnHasBeenSelected from './ifATurnHasBeenSelected'

const ProbabilityQuery = props => (
  <IfATurnHasBeenSelected selectedTurn={props.selectedTurn}>
    <div>
      What is the probability of:
      {renderColourInputs(props)}
      {renderInput("Any", props)}
      on turn {props.selectedTurn + 1}?
    </div>
    <div>
      {props.probability}
    </div>
  </IfATurnHasBeenSelected>
);

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
