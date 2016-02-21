import assign from 'object-assign'
import e from 'events'

var Store = assign({}, e.EventEmitter.prototype, {
  emitChange: function() {
    this.emit('change');
  },
  addChangeListener: function(listener) {
    this.on('change', listener);
  },
  removeChangeListener: function(listener) {
    this.removeListener('change', listener);
  }
});

export default Store;
