import React from 'react';
import ReactDOM from 'react-dom';
import '../css/index.css';
import LandsChartContainer from './containers/landsChartContainer.jsx'

const app = document.createElement('div');
document.body.appendChild(app);
ReactDOM.render(<LandsChartContainer />, app);
