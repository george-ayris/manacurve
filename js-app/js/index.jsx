import React from 'react';
import ReactDOM from 'react-dom';
import '../css/index.css';
import AnalysisScreenLayoutContainer from './containers/analysisScreenLayoutContainer.jsx'

const app = document.createElement('div');
document.body.appendChild(app);
ReactDOM.render(<AnalysisScreenLayoutContainer />, app);
