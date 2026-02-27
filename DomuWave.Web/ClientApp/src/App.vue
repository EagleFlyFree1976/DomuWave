<template>
  <Toast position="top-right" :pt="{ root: { style: 'z-index: 9999' } }" />
  <RouterView />
</template>

<script setup>
import { onMounted, onUnmounted } from 'vue'
import { useToast } from 'primevue/usetoast'

const toast = useToast()

/**
 * Mappa lo status HTTP a severity e titolo del toast.
 */
function resolveToastConfig(status) {
  if (status === 401 || status === 403) {
    return { severity: 'warn', summary: 'Accesso negato' }
  }
  if (status === 400 || status === 422) {
    return { severity: 'warn', summary: 'Dati non validi' }
  }
  if (status === 404) {
    return { severity: 'info', summary: 'Non trovato' }
  }
  if (status >= 500) {
    return { severity: 'error', summary: 'Errore del server' }
  }
  if (status === null) {
    return { severity: 'error', summary: 'Connessione assente' }
  }
  return { severity: 'error', summary: 'Errore' }
}

function onApiError(event) {
  const { status, message } = event.detail
  const { severity, summary } = resolveToastConfig(status)

  toast.add({
    severity,
    summary,
    detail: message,
    life: 6000,
  })
}

onMounted(() => window.addEventListener('api:error', onApiError))
onUnmounted(() => window.removeEventListener('api:error', onApiError))
</script>
