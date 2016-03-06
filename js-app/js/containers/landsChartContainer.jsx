import React from 'react';
import LandsStore from '../stores/landsStore'
import LandsActions from '../actions/landsActionCreators'
import Constants from '../constants'
import Utils from '../utils/utils'
import LandsChart from '../components/landsChart'

const LandsChartContainer = React.createClass({
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

  handleRunSimulation() {
    LandsActions.runSimulation(this.state.numberOfEachColour);
  },

  handleBarSelected(barClicked) {
    var selectedTurn = barClicked === this.state.selectedTurn ? null : barClicked;
    LandsActions.updateSelectedTurn(selectedTurn);
  },

  handleSliderChanged(colour, newValue) {
    var copyOfNumberOfEachColour = Utils.createColoursObject(this.state.numberOfEachColour);
    copyOfNumberOfEachColour[colour] = parseInt(newValue);
    LandsActions.updateNumberOfLands(copyOfNumberOfEachColour);
  },

  render() {
    return (
      <LandsChart
        numberOfSimulationsRunning={this.state.numberOfSimulationsRunning}
        selectedTurn={this.state.selectedTurn}
        averages={this.state.averages}
        mostCommonLandScenarios={this.state.mostCommonLandScenarios}
        queryNumbers={this.state.queryNumbers}
        probability={this.state.probability}
        numberOfEachColour={this.state.numberOfEachColour}
        simulationError={this.state.simulationError}
        onRunSimulation={this.handleRunSimulation}
        onBarSelected={this.handleBarSelected}
        onSliderChanged={this.handleSliderChanged}
      />
    )
  }
});

export default LandsChartContainer;
