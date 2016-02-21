import React from 'react';
import ReactDOM from 'react-dom';
import '../css/index.css';
import LandsChart from './views/landsChart.jsx'

const app = document.createElement('div');
document.body.appendChild(app);
ReactDOM.render(<LandsChart />, app);
