import React from 'react';
import StackedBarChart from './stackedBarChart.jsx'
import BarChart from './barChart.jsx'
import Slider from './slider.jsx'
import LandsStore from '../stores/landsStore'
import LandsActions from '../actions/landsActionCreators'
import Constants from '../constants'
import Utils from '../utils/utils'

const LandsChart = React.createClass({
  getInitialState: LandsStore.getState,

  componentDidMount() {
    LandsStore.addChangeListener(this._onChange);
    //this.updateData(this.state.numberOfLands);
    LandsActions.updateNumberOfLands(this.state.numberOfEachColour);
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

  colourSliderChanged(colour, newValue) {
    var copyOfNumberOfEachColour = Utils.createColoursObject(this.state.numberOfEachColour);
    copyOfNumberOfEachColour[colour] = parseInt(newValue);
    console.log('Updating lands to ', copyOfNumberOfEachColour);
    LandsActions.updateNumberOfLands(copyOfNumberOfEachColour);
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
    var makeSlider = colour => {
      return <Slider
        sliderValue={self.state.numberOfEachColour[colour]}
        label={colour}
        sliderChanged={n => { return self.colourSliderChanged(colour, n); }}
        key={colour}
      />;
    };

    var sliders = <div>
      {Constants.Colours.map(x => { return makeSlider(x); })}
    </div>;
    if (this.state.numberOfSimulationsRunning === 0) {

      if (this.state.selectedTurn || this.state.selectedTurn === 0) {
        var dataForSelectedTurn = this.state.mostCommonLandScenarios[this.state.selectedTurn].map((x, i) => { return x.probability; });
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
          {sliders}
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
        {sliders}
        <span>Running simulation</span>
      </div>
    );
  }
});

export default LandsChart;
