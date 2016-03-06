import React from 'react';
import LandsStore from '../stores/landsStore'
import LandsActions from '../actions/landsActionCreators'
import Constants from '../constants'
import Utils from '../utils/utils'
import AnalysisScreenLayout from '../components/analysisScreenLayout'

const AnalysisScreenLayoutContainer = React.createClass({
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
    var numberOfEachColourCopy = Utils.createColoursObject(this.state.numberOfEachColour);
    numberOfEachColourCopy[colour] = parseInt(newValue);
    LandsActions.updateNumberOfLands(numberOfEachColourCopy);
  },

  handleQueryNumbersChanged(colour, event) {
    var queryNumbersCopy = Utils.objectMap(this.state.queryNumbers, x => x);
    var int = parseInt(event.target.value)

    if (int || int === 0) {
      queryNumbersCopy[colour] = parseInt(event.target.value);
      LandsActions.updateQueryNumbers(queryNumbersCopy);

    } else if (event.target.value === "") {
      queryNumbersCopy[colour] = "";
      LandsActions.updateQueryNumbers(queryNumbersCopy);
    }
  },

  render() {
    return (
      <AnalysisScreenLayout
        isSimulationRunning={this.state.isSimulationRunning}
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
        onQueryNumbersChanged={this.handleQueryNumbersChanged}
      />
    )
  }
});

export default AnalysisScreenLayoutContainer;
