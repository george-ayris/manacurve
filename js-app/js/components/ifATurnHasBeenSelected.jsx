import React from 'react'

const IfATurnHasBeenSelected = props => {
  if(props.selectedTurn || props.selectTurn === 0) {
    return <span>{props.children}</span>;
  } else {
    return <span>No turn selected</span>;
  }
}

IfATurnHasBeenSelected.propTypes = {
  selectTurn: React.PropTypes.number
}

export default IfATurnHasBeenSelected
