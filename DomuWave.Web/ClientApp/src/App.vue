<template>
  <div>
    <Toast position="top-right" :pt="{
      root:    { style: 'z-index: 9999' },
      summary: { style: 'white-space: pre-line; word-break: break-word' }
    }" />
    <RouterView />
  </div>
</template>

<script setup>
import { onMounted, onUnmounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useRouter } from 'vue-router'

const router = useRouter()

const toast = useToast()

/**
 * Mappa lo status HTTP a severity e titolo del toast.
 */
function resolveToastSeverity(status) {
  if (status === 401 || status === 403) return 'warn'
  if (status === 400 || status === 422) return 'warn'
  if (status === 404)                   return 'info'
  if (status >= 500)                    return 'error'
  if (status === null)                  return 'error'
  return 'error'
}

function onApiError(event) {
  const { status, message } = event.detail
  console.log('[api:error]', status, message)
  const severity = resolveToastSeverity(status)
  const text = Array.isArray(message) ? message.join('\n') : (message ?? 'Si è verificato un errore imprevisto')

  toast.add({
    severity,
    summary: text,
    life: 8000,
  })
}

function onLicenseNotActivated() {
  router.push({ name: 'servizio-non-attivato' })
}

onMounted(() => {
  window.addEventListener('api:error', onApiError)
  window.addEventListener('license:not-activated', onLicenseNotActivated)
})
onUnmounted(() => {
  window.removeEventListener('api:error', onApiError)
  window.removeEventListener('license:not-activated', onLicenseNotActivated)
})
</script>
