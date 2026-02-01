export default {
  mounted(el, binding) {
    format(el, binding);
  },
  updated(el, binding) {
    format(el, binding);
  }
};

function format(el, binding) {
  const value = binding.value?.amount ?? 0;
  const locale = binding.value?.locale ?? 'it-IT';
  const currency = binding.value?.currency ?? 'EUR';

  el.innerText = new Intl.NumberFormat(locale, {
    style: 'currency',
    currency,
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  }).format(value);
}
