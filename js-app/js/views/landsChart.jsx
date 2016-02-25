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
    LandsStore.addChangeListener(this.onChange);
    LandsActions.updateNumberOfLands(this.state.numberOfEachColour);
    LandsActions.runSimulation(this.state.numberOfEachColour);
  },

  componentWillUnmount() {
    LandsStore.removeChangeListener(this.onChange);
  },

  onChange() {
    this.setState(LandsStore.getState());
  },

  render() {
    var sliders =
      <div>
        {Constants.Colours.map(x => { return this.makeSlider(x); })}
        <div>{this.state.error}</div>
        <button onClick={this.runSimulation}>Run simulation</button>
      </div>;

    if (this.state.numberOfSimulationsRunning === 0) {
      if (this.state.selectedTurn || this.state.selectedTurn === 0) {
        var turnChart =
          <StackedBarChart
            data={this.landScenariosWeightedByProbability()}
            indexToAxisLabel={this.landScenarioLabels}
            dataToBarLabel={Utils.sum}
          />

      } else {
        var turnChart = <div>No turn selected</div>;
      }

      var body =
        <div>
          <StackedBarChart
            data={this.state.averages}
            indexToAxisLabel={ i => { return 'Turn ' + (i+1); }}
            barClicked={this.updateSelectedBar}
            selectedBar={this.state.selectedTurn}
          />
          <br />
          {turnChart}
        </div>
    } else {
      var body = <span>Running simulation</span>
    }

    return (
      <div>
        {sliders}
        {body}
      </div>
    );
  },

  makeSlider(colour) {
    return (
      <Slider
        sliderValue={this.state.numberOfEachColour[colour]}
        label={colour}
        sliderChanged={n => { return this.colourSliderChanged(colour, n); }}
        key={colour}
      />
    );
  },

  colourSliderChanged(colour, newValue) {
    var copyOfNumberOfEachColour = Utils.createColoursObject(this.state.numberOfEachColour);
    copyOfNumberOfEachColour[colour] = parseInt(newValue);
    LandsActions.updateNumberOfLands(copyOfNumberOfEachColour);
  },

  runSimulation() {
    LandsActions.runSimulation(this.state.numberOfEachColour);
  },

  landScenariosWeightedByProbability() {
    return this.state.mostCommonLandScenarios[this.state.selectedTurn]
      .map((x, i) => {
        var totalLandsInScenario = Utils.sum(x.landScenario);

        return x.landScenario
          .map(landCountByColour => {
            if (totalLandsInScenario === 0) return 0;
            return landCountByColour * x.probability / totalLandsInScenario;
          });
      });
  },

  landScenarioLabels(index) {
    var landScenarioNamesForSelectedTurn =
      this.state.mostCommonLandScenarios[this.state.selectedTurn]
        .map((x, i) => { return x.landScenario; });
    return landScenarioNamesForSelectedTurn[index];
  },

  updateSelectedBar(barClicked) {
    var selectedTurn = barClicked === this.state.selectedTurn ? null : barClicked;
    LandsActions.updateSelectedTurn(selectedTurn);
  }
});

export default LandsChart;
