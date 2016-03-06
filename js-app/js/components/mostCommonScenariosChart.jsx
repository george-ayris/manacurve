import React from 'react'
const PropTypes = React.PropTypes;
import StackedBarChart from './d3StackedBarChart'
import IfATurnHasBeenSelected from './ifATurnHasBeenSelected'
import Utils from '../utils/utils'

const MostCommonScenariosChart = props => (
  <IfATurnHasBeenSelected selectedTurn={props.selectedTurn}>
    <StackedBarChart
      data={landScenariosWeightedByProbability(props)}
      indexToAxisLabel={i => landScenarioLabels(i, props)}
      dataToBarLabel={Utils.sum}
    />
  </IfATurnHasBeenSelected>
);

const landScenariosWeightedByProbability = props => {
  if (props.selectedTurn == null) return [];

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

const landScenarioLabels = (scenarioIndex, props) => {
  var landScenarioNamesForSelectedTurn =
    props.mostCommonLandScenarios[props.selectedTurn]
      .map((x, i) => { return x.landScenario; });
  return landScenarioNamesForSelectedTurn[scenarioIndex];
};

MostCommonScenariosChart.propTypes = {
  selectedTurn: PropTypes.number,
  mostCommonLandScenarios: PropTypes.array.isRequired
};

export default MostCommonScenariosChart
