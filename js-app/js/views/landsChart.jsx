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
    LandsActions.updateNumberOfLands(this.state.numberOfEachColour);
  },

  componentWillUnmount() {
    LandsStore.removeChangeListener(this._onChange);
  },

  _onChange() {
    this.setState(LandsStore.getState());
  },

  colourSliderChanged(colour, newValue) {
    var copyOfNumberOfEachColour = Utils.createColoursObject(this.state.numberOfEachColour);
    copyOfNumberOfEachColour[colour] = parseInt(newValue);
    LandsActions.updateNumberOfLands(copyOfNumberOfEachColour);
  },

  render() {
    var self = this;

    var makeSlider = colour => {
      return (
        <Slider
          sliderValue={self.state.numberOfEachColour[colour]}
          label={colour}
          sliderChanged={n => { return self.colourSliderChanged(colour, n); }}
          key={colour}
        />
      );
    };

    var sliders =
      <div>
        {Constants.Colours.map(x => { return makeSlider(x); })}
      </div>;

    if (this.state.numberOfSimulationsRunning !== 0) {
      var body = <span>Running simulation</span>

    } else {
      if (this.state.selectedTurn || this.state.selectedTurn === 0) {
        var landScenarioWeightedByProbability = this.state.mostCommonLandScenarios[this.state.selectedTurn]
          .map((x, i) => {
            var totalLandsInScenario = x.landScenario.reduce((a, b) => a + b, 0);
            return x.landScenario.map(y => {
              if (totalLandsInScenario === 0) return 0;
              return y * x.probability / totalLandsInScenario;
            });
          });

        var landScenarioLabels = i => {
          var landScenarios = self.state.mostCommonLandScenarios[self.state.selectedTurn].map((x, i) => { return x.landScenario; });
          return landScenarios[i];
        };

        var combineWeightedProbabilities = x => x.reduce((a, b) => a + b, 0);

        var turnChart =
          <StackedBarChart
            data={landScenarioWeightedByProbability}
            indexToAxisLabel={landScenarioLabels}
            dataToBarLabel={combineWeightedProbabilities}
          />

      } else {
        var turnChart = <div>No turn selected</div>;
      }

      var updateSelectedBar = barClicked => {
        var selectedTurn = barClicked === self.state.selectedTurn ? null : barClicked;
        LandsActions.updateSelectedTurn(selectedTurn);
      }

      var body =
        <div>
          <StackedBarChart
            data={this.state.averages}
            indexToAxisLabel={ i => { return 'Turn ' + (i+1); }}
            barClicked={updateSelectedBar}
            selectedBar={self.state.selectedTurn}
          />
          <br />
          {turnChart}
        </div>
    }

    return (
      <div>
        {sliders}
        {body}
      </div>
    );
  }
});

export default LandsChart;
