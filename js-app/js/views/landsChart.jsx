import React from 'react';
import StackedBarChart from './stackedBarChart.jsx'
import BarChart from './barChart.jsx'
import Slider from './slider.jsx'
import LandsStore from '../stores/LandsStore'
import LandsActions from '../actions/landsActionCreators'

const LandsChart = React.createClass({
  getInitialState: LandsStore.getState,

  componentDidMount() {
    LandsStore.addChangeListener(this._onChange);
    //this.updateData(this.state.numberOfLands);
    LandsActions.updateNumberOfLands([this.state.numberOfColour1, this.state.numberOfColour2]);
  },

  componentWillUnmount() {
    LandsStore.removeChangeListener(this._onChange);
  },

  _onChange() {
    this.setState(LandsStore.getState());
  },

  /*componentWillUpdate(nextProps, nextState) {
    console.log('componentWillUpdate', nextState.numberOfLands, this.state.numberOfLands);
    if (nextState.numberOfLands !== this.state.numberOfLands) {
      this.updateData(nextState.numberOfLands);
    }
  },*/

  colour1SliderChanged(newValue) {
    /*console.log('Slider changed', newValue);
    this.setState({
      numberOfLands: newValue
    });*/
    LandsActions.updateNumberOfLands([newValue, this.state.numberOfColour2]);
  },

  /*updateData(numberOfLands) {
    console.log('Making API call');
    this.setState({
      numberOfSimulationsRunning: this.state.numberOfSimulationsRunning + 2
    });
    $.get('/deck/' + numberOfLands, _ => {
      $.get('/deck/' + numberOfLands + '/averages',
            this.processSimulationResults(
              result => ({ averages: result.averages })
            )
      ).fail(this.apiErrorHandling);
      $.get('/deck/' + numberOfLands + '/distributions',
            this.processSimulationResults(
              result => ({ distributions: result.distributions })
            )
      ).fail(this.apiErrorHandling);
    }).fail(this.apiErrorHandling);
  },*/

  /*processSimulationResults(stateToUpdate) {
    return function(result) {
      if (this.isMounted()) {
        let newState = stateToUpdate(result);
        console.log(newState, result);

        if (this.state.numberOfSimulationsRunning > 0) {
          newState.numberOfSimulationsRunning = this.state.numberOfSimulationsRunning - 1;
        } else {
          newState.numberOfSimulationsRunning = 0;
        }

        this.setState(newState);
      }
    }.bind(this);
  },*/

  /*apiErrorHandling() {
    console.log('Api call has failed');
    this.setState({ numberOfSimulationsRunning: 0 });
  },*/

  render() {
    var self = this;
    if (this.state.numberOfSimulationsRunning === 0) {

      if (this.state.selectedTurn || this.state.selectedTurn === 0) {
        var dataForSelectedTurn = this.state.mostCommonLandScenarios[this.state.selectedTurn].map((x, i) => { return x.probability; });
        console.log(dataForSelectedTurn);
        var turnChart =
          <BarChart
            data={dataForSelectedTurn}
            indexToLabel={ i => {
              var landScenarios = self.state.mostCommonLandScenarios[self.state.selectedTurn].map((x, i) => { return x.landScenario; });
              return landScenarios[i];
            }}
          />
      } else {
        var turnChart = <div>No turn selected</div>;
      }

      return (
        <div>
          <Slider manaColour={"Red"} sliderChanged={this.colour1SliderChanged} />
          <StackedBarChart
            data={this.state.averages}
            indexToLabel={ i => { return 'Turn ' + (i+1); }}
            barClicked={ barClicked => {
              var selectedTurn = barClicked === self.state.selectedTurn ? null : barClicked;
              self.setState({ selectedTurn: selectedTurn });
            }}
            selectedBar={self.state.selectedTurn}
          />
          <br />
          {turnChart}
        </div>
      );
    }

    return (
      <div>
        <Slider manaColour={"Red"} sliderChanged={this.colour1SliderChanged} />
        <span>Running simulation</span>
      </div>
    );
  }
});

export default LandsChart;
