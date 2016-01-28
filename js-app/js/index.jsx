import React from 'react';
import ReactDOM from 'react-dom';
import '../css/index.css';
import AverageLandsChart from './averageLandsChart.jsx'

const app = document.createElement('div');
document.body.appendChild(app);
ReactDOM.render(<AverageLandsChart />, app);
