import { toUtcDate } from './dateUtils'
export function processApiResponse(response) {
  // Esempio di elaborazione standard della risposta
  if (response.ok) {
    return response.json();
  } else {
    throw new Error(`API Error: ${response.status} ${response.statusText}`);
  }
}

// Altre funzioni utili
export function handleApiError(error) {
  console.error('API call failed:', error);
  // Gestione comune degli errori
}

export function filterToDateString(targetDate) {
  return targetDate ? toUtcDate(targetDate) : "";
}
export function filterToString(inputString) {
  return inputString ? inputString : "";
}
