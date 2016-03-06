import React from 'react'
import Constants from '../constants'
import LandsActions from '../actions/landsActionCreators'

const ProbabilityQuery = React.createClass({
  propTypes: {
    queryNumbers: React.PropTypes.object.isRequired,
    currentTurn: React.PropTypes.number,
    probability: React.PropTypes.number
  },

  onChange(colour, event) {
    var int = parseInt(event.target.value)
    if (int || int === 0) {
      this.props.queryNumbers[colour] = parseInt(event.target.value);
      LandsActions.updateQueryNumbers(this.props.queryNumbers);

    } else if (event.target.value === "") {
      this.props.queryNumbers[colour] = "";
      LandsActions.updateQueryNumbers(this.props.queryNumbers);
    }
  },

  render() {
    var colourInputs = Constants.Colours.map(this.makeInput);

    if (this.props.currentTurn || this.props.currentTurn === 0) {
      return(
        <div>
          <div>
            What is the probability of:
            {colourInputs}
            {this.makeInput("Any")}
            on turn {this.props.currentTurn + 1}?
          </div>
          <div>
            {this.props.probability}
          </div>
        </div>);
    } else {
      return <div>No turn selected</div>
    }
  },

  makeInput(colour) {
    return(
      <div key={"probability-" + colour}>
        <label htmlFor={"probability-" + colour}>{colour}</label>
        <input id={"probability-" + colour} onChange={e => this.onChange(colour, e)} value={this.props.queryNumbers[colour]}/>
      </div>);
  }
});

export default ProbabilityQuery;
