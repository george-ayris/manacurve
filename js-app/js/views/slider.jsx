import React from 'react';

// TODO: Consider removing state from here
const Slider = React.createClass({
  propTypes: {
    sliderChanged: React.PropTypes.func.isRequired,
    manaColour: React.PropTypes.string.isRequired
  },
  getInitialState: function() {
    return {
      value: 20,
    };
  },
  handleChange: function(event) {
    var sliderValue = event.target.value;
    var component = this;
    this.setState({
      value: sliderValue
    });
    window.setTimeout(function() {
      console.log(sliderValue, component.state.value)
      if (sliderValue == component.state.value) {
        component.props.sliderChanged(sliderValue);
      }
    }, 200);
  },
  render: function() {
    return (
      <div>
        <label htmlFor="range">Number {this.props.manaColour} of Lands</label>
        <input type="range" id="range" min="0" value={this.state.value} onChange={this.handleChange} max="60" step="1" />
        <output htmlFor="range">{this.state.value}</output>
      </div>
    );
  }
});

export default Slider;
