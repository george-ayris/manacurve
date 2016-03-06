import React from 'react'
const PropTypes = React.PropTypes;
import Constants from '../constants'
import Slider from './slider'

const DeckCreator = props => (
  <div>
    {renderSliders(props)}
    <div>{props.simulationError}</div>
    <button onClick={props.onRunSimulation} disabled={props.isSimulationRunning}>
      Run simulation
    </button>
  </div>
)

const renderSliders = (props) =>
  Constants.Colours.map(c => renderSlider({
    colour: c,
    numberOfColour: props.numberOfEachColour[c],
    onSliderChanged: n => props.onSliderChanged(c, n)
  }));

const renderSlider = props => (
  <Slider
    sliderValue={props.numberOfColour}
    label={props.colour}
    sliderChanged={props.onSliderChanged}
    key={props.colour}
  />
);

DeckCreator.propTypes = {
  simulationError: PropTypes.string,
  numberOfEachColour: PropTypes.object.isRequired,
  onSliderChanged: PropTypes.func.isRequired,
  isSimulationRunning: PropTypes.bool.isRequired,
  onRunSimulation: PropTypes.func.isRequired
}

export default DeckCreator
