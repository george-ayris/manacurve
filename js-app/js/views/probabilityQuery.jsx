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
    var self = this;
    var inputs = Constants.Colours.map((colour, i) => {
        return(
          <div key={"probability-" + colour}>
            <label htmlFor={"probability-" + colour}>{colour}</label>
            <input id={"probability-" + colour} onChange={e => self.onChange(colour, e)} value={self.props.queryNumbers[colour]}/>
          </div>);
    });

    if (this.props.currentTurn || this.props.currentTurn === 0) {
      return(
        <div>
          <div>
            What is the probability of:
            {inputs}
            on turn {this.props.currentTurn + 1}?
          </div>
          <div>
            {this.props.probability}
          </div>
        </div>);
    } else {
      return <div>No turn selected</div>
    }

  }
});

export default ProbabilityQuery;
