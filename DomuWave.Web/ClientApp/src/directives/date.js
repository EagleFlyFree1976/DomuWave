export default {
  mounted(el, binding) {
    format(el, binding);
  },
  updated(el, binding) {
    format(el, binding);
  }
};

function format(el, binding) {
  const value = binding.value;
  const format = binding.arg || 'date'; // 'date', 'time', 'datetime'
  if (value == null || value == undefined) {
    el.textContent = "";
  }
  else {
    const date = new Date(value);
    let text = '';

    switch (format) {
      case 'time':
        text = date.toLocaleTimeString();
        break;
      case 'datetime':
        text = date.toLocaleString();
        break;
      default:
        text = date.toLocaleDateString();
    }

    el.textContent = text;
  }
}
