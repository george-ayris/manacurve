import React from 'react'

const IfATurnHasBeenSelected = props => {
  if(props.selectedTurn || props.selectTurn === 0) {
    return <div>{props.children}</div>;
  } else {
    return <div>No turn selected</div>;
  }
}

IfATurnHasBeenSelected.propTypes = {
  selectTurn: React.PropTypes.number
}

export default IfATurnHasBeenSelected
