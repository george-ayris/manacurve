import React from 'react';
import AverageLandsChart from './averageLandsChart'
import MostCommonScenariosChart from './mostCommonScenariosChart'
import ProbabilityQuery from './probabilityQuery'
import DeckCreator from './deckCreator'
import Constants from '../constants'
import Utils from '../utils/utils'

const AnalysisScreenLayout = props => (
  <div>
    <DeckCreator
      simulationError={props.simulationError}
      onRunSimulation={props.onRunSimulation}
      onSliderChanged={props.onSliderChanged}
      isSimulationRunning={props.isSimulationRunning}
      numberOfEachColour={props.numberOfEachColour}
    />
    <br />
    {props.isSimulationRunning ?
      <span>Running simulation</span> :
      <div>
        <ProbabilityQuery
          queryNumbers={props.queryNumbers}
          selectedTurn={props.selectedTurn}
          probability={props.probability}
          onQueryNumbersChanged={props.onQueryNumbersChanged}
        />
        <br />
        <AverageLandsChart
          averages={props.averages}
          onBarSelected={props.onBarSelected}
          selectedTurn={props.selectedTurn}
        />
        <br />
        <MostCommonScenariosChart
          mostCommonLandScenarios={props.mostCommonLandScenarios}
          selectedTurn={props.selectedTurn}
        />
      </div>
    }
  </div>
);



export default AnalysisScreenLayout;
