import React from 'react';
import BarChart from './barChart.jsx'
import Slider from './slider.jsx'

const AverageLandsChart = React.createClass({
  sliderChanged: function(newValue) {
    console.log('Slider changed', newValue);
    this.setState({
      numberOfLands: newValue
    });
  },
  updateData: function(numberOfLands) {
    console.log('Making API call');
    this.setState({
      numberOfSimulationsRunning: this.state.numberOfSimulationsRunning + 2
    });
    $.get('/monodeck/' + numberOfLands, _ => {
      $.get('/monodeck/' + numberOfLands + '/averages',
            this.processSimulationResults(
              result => ({ averages: result.averages })
            )
      ).fail(this.apiErrorHandling);
      $.get('/monodeck/' + numberOfLands + '/distributions',
            this.processSimulationResults(
              result => ({ distributions: result.distributions })
            )
      ).fail(this.apiErrorHandling);
    }).fail(this.apiErrorHandling);
  },
  processSimulationResults: function(stateToUpdate) {
    return function(result) {
      if (this.isMounted()) {
        let newState = stateToUpdate(result);
        console.log(newState, result);

        if (this.state.numberOfSimulationsRunning > 0) {
          newState.numberOfSimulationsRunning = this.state.numberOfSimulationsRunning - 1;
        } else {
          newState.numberOfSimulationsRunning = 0;
        }

        this.setState(newState);
      }
    }.bind(this);
  },
  apiErrorHandling: function() {
    console.log('Api call has failed');
    this.setState({ numberOfSimulationsRunning: 0 });
  },
  getInitialState: function() {
    return {
      averages: [],
      distributions: [],
      numberOfLands: 20,
      numberOfSimulationsRunning: 0,
      selectedTurn: null,
    };
  },
  render: function() {
    console.log('Rendering component', this.state.averages, this.state.numberOfSimulationsRunning);
    var self = this;
    if (this.state.numberOfSimulationsRunning === 0) {
      if (this.state.selectedTurn || this.state.selectedTurn === 0) {
        var dataForSelectedTurn = this.state.distributions[this.state.selectedTurn].map(function(x, i) { return x[1]; });
        var turnChart = <BarChart data={dataForSelectedTurn} mouseEvents={false}/>
      } else {
        var turnChart = <div>No turn selected</div>;
      }
      return (
        <div>
          <Slider sliderChanged={this.sliderChanged} />
          <BarChart
            data={this.state.averages}
            indexToLabel={function(i) { return 'Turn ' + (i+1); }}
            barClicked={function(barClicked) {
              var selectedTurn = barClicked === self.state.selectedTurn ? null : barClicked;
              self.setState({ selectedTurn: selectedTurn });
            }}
            selectedBar={self.state.selectedTurn}
            mouseEvents={true}
          />
          <br />
          {turnChart}
        </div>
      );
    }

    return (
      <div>
        <Slider sliderChanged={this.sliderChanged} />
        <span>Running simulation</span>
      </div>
    );
  },
  componentDidMount: function() {
    this.updateData(this.state.numberOfLands);
  },
  componentWillUpdate: function(nextProps, nextState) {
    console.log('componentWillUpdate', nextState.numberOfLands, this.state.numberOfLands);
    if (nextState.numberOfLands !== this.state.numberOfLands) {
      this.updateData(nextState.numberOfLands);
    }
  }
});

export default AverageLandsChart;
