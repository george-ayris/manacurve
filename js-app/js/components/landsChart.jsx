import React from 'react';
import StackedBarChart from './d3StackedBarChart.jsx'
import Slider from './slider.jsx'
import ProbabilityQuery from './probabilityQuery.jsx'
import Constants from '../constants'
import Utils from '../utils/utils'

const LandsChart = props => {
  if (props.numberOfSimulationsRunning === 0) {
    if (props.selectedTurn || props.selectedTurn === 0) {
      var turnChart =
        <StackedBarChart
          data={landScenariosWeightedByProbability(props)}
          indexToAxisLabel={i => landScenarioLabels(i, props)}
          dataToBarLabel={Utils.sum}
        />

    } else {
      var turnChart = <div>No turn selected</div>;
    }

    var body =
      <div>
        <StackedBarChart
          data={props.averages}
          indexToAxisLabel={ i => { return 'Turn ' + (i+1); }}
          barClicked={props.onBarSelected}
          selectedBar={props.selectedTurn}
        />
        <br />
        {turnChart}
      </div>
      var runSimulationsButton = <button onClick={props.onRunSimulation}>Run simulation</button>;
  } else {
    var body = <span>Running simulation</span>;
    var runSimulationsButton = <button onClick={props.onRunSimulation} disabled>Run simulation</button>;
  }

  var sliders =
    <div>
      {renderSliders(props)}
      <div>{props.simulationError}</div>
      {runSimulationsButton}
    </div>;

  return (
    <div>
      {sliders}
      <br />
      <ProbabilityQuery
        queryNumbers={props.queryNumbers}
        currentTurn={props.selectedTurn}
        probability={props.probability}
      />
      <br />
      {body}
    </div>
  );
};

const renderSliders = (props) => {
  return Constants.Colours.map(c => renderSlider({
    colour: c,
    numberOfColour: props.numberOfEachColour[c],
    onSliderChanged: n => props.onSliderChanged(c, n)
  }))
};

const renderSlider = props => (
  <Slider
    sliderValue={props.numberOfColour}
    label={props.colour}
    sliderChanged={props.onSliderChanged}
    key={props.colour}
  />
);

const landScenariosWeightedByProbability = props => {
  return props.mostCommonLandScenarios[props.selectedTurn]
    .map((x, i) => {
      var totalLandsInScenario = Utils.sum(x.landScenario);

      return x.landScenario
        .map(landCountByColour => {
          if (totalLandsInScenario === 0) return 0;
          return landCountByColour * x.probability / totalLandsInScenario;
        });
    });
};

const landScenarioLabels = (index, props) => {
  var landScenarioNamesForSelectedTurn =
    props.mostCommonLandScenarios[props.selectedTurn]
      .map((x, i) => { return x.landScenario; });
  return landScenarioNamesForSelectedTurn[index];
};


export default LandsChart;
