
/// trasforma una data locale in formato utc pronta per essere inviata al server
export function toUtcDate(local) {
  if (!(local instanceof Date)) {
    local = new Date(local);
  }

  if (isNaN(local.getTime())) {
    throw new TypeError("Input must be a valid date or convertible to a valid Date");
  }
  const utcDate = new Date(local.getTime() - local.getTimezoneOffset() * 60000);
  return utcDate.toISOString();
}

