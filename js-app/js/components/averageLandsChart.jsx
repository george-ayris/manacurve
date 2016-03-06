import React from 'react'
const PropTypes = React.PropTypes;
import StackedBarChart from './d3StackedBarChart'

const AverageLandsChart = props => (
  <StackedBarChart
    data={props.averages}
    indexToAxisLabel={ i => { return 'Turn ' + (i+1); }}
    barClicked={props.onBarSelected}
    selectedBar={props.selectedTurn}
  />
);

AverageLandsChart.propTypes = {
  averages: PropTypes.array.isRequired,
  onBarSelected: PropTypes.func.isRequired,
  selectedTurn: PropTypes.number
}

export default AverageLandsChart
