import React from 'react';
import AverageLandsChart from './averageLandsChart'
import MostCommonScenariosChart from './mostCommonScenariosChart'
import ProbabilityQuery from './probabilityQuery'
import DeckCreator from './deckCreator'
import Constants from '../constants'
import Utils from '../utils/utils'

const AnalysisScreenLayout = props => (
  <div>
    <div className="row">
      <h1>Manacurve</h1>
    </div>
    <div className="row">
      <div className="columns small-6">
        <div>
          <h3>Create deck</h3>
        </div>
        <DeckCreator
          simulationError={props.simulationError}
          onRunSimulation={props.onRunSimulation}
          onSliderChanged={props.onSliderChanged}
          isSimulationRunning={props.isSimulationRunning}
          numberOfEachColour={props.numberOfEachColour}
        />
      </div>
      <div className="columns small-6">
        <div>
          <h3>Probability of card</h3>
        </div>
        <ProbabilityQuery
          queryNumbers={props.queryNumbers}
          selectedTurn={props.selectedTurn}
          probability={props.probability}
          onQueryNumbersChanged={props.onQueryNumbersChanged}
        />
      </div>
    </div>
    <br />
    <div className="row">
      {props.isSimulationRunning ?
        <div className="center-contents">
          <br />
          <h3>Running simulation</h3>
        </div>
        :
        <div>
          <div className="columns small-6">
            <div>
              <h3>Average mana per turn</h3>
            </div>
            <AverageLandsChart
              averages={props.averages}
              onBarSelected={props.onBarSelected}
              selectedTurn={props.selectedTurn}
            />
          </div>
          <div className="columns small-6">
            <div>
              <h3>Most common mana scenarios</h3>
            </div>
            <MostCommonScenariosChart
              mostCommonLandScenarios={props.mostCommonLandScenarios}
              selectedTurn={props.selectedTurn}
            />
          </div>
        </div>
      }
    </div>
  </div>
);

export default AnalysisScreenLayout;
