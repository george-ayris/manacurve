import React from 'react';

// TODO: Consider removing state from here
const Slider = React.createClass({
  propTypes: {
    sliderChanged: React.PropTypes.func.isRequired,
    label: React.PropTypes.string.isRequired,
    sliderValue: React.PropTypes.number.isRequired
  },
  getInitialState() {
    return {
      previousValue: this.props.value
    };
  },
  handleChange(event) {
    var sliderValue = event.target.value;
    var component = this;
    this.setState({
      previousValue: sliderValue
    });
    window.setTimeout(function() {
      if (sliderValue == component.state.previousValue) {
        component.props.sliderChanged(sliderValue);
      }
    }, 200);
  },
  render() {
    return (
      <div>
        <label htmlFor="range">{this.props.label}</label>
        <input type="range" id="range" min="0" value={this.props.sliderValue} onChange={this.handleChange} max="60" step="1" />
        <output htmlFor="range">{this.props.sliderValue}</output>
      </div>
    );
  }
});

export default Slider;
