import { ref } from 'vue'
import { tenantDisplaySettingsApi } from '@/services/api'
import DEFAULT_LOGO from '@/assets/logostudiogalli.png'

/**
 * Branding per tenant: logo configurabile usato nella sidebar e nei report esportati.
 *
 * L'endpoint del logo è AUTENTICATO (richiede Bearer + X-Tenant-Id), quindi un
 * <img src> diretto non funziona: scarichiamo il blob autenticato e creiamo:
 *   - un objectURL (per <img>)
 *   - un data-URL base64 (per jsPDF.addImage nei report PDF)
 *
 * Stato singleton condiviso (come useAccountingFormat).
 */

// ── Stato singleton ──────────────────────────────────────────────────────────
const hasLogo      = ref(false)          // true se il tenant ha un logo caricato
const logoUrl      = ref(null)           // objectURL per <img>, null → usa DEFAULT_LOGO
const logoDataUrl  = ref(null)           // data-URL base64 per jsPDF
const logoMime     = ref(null)           // content-type del logo
const loaded       = ref(false)
let   loadingPromise = null
let   currentObjectUrl = null

function revokeObjectUrl() {
  if (currentObjectUrl) {
    URL.revokeObjectURL(currentObjectUrl)
    currentObjectUrl = null
  }
}

function blobToDataUrl(blob) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onload  = () => resolve(reader.result)
    reader.onerror = reject
    reader.readAsDataURL(blob)
  })
}

async function fetchLogo() {
  // Prima verifica dal readDto se esiste un logo (evita 404 rumorosi)
  const { data: settings } = await tenantDisplaySettingsApi.get()
  if (!settings?.hasLogo) {
    revokeObjectUrl()
    hasLogo.value     = false
    logoUrl.value     = null
    logoDataUrl.value = null
    logoMime.value    = null
    return
  }

  const { data: blob } = await tenantDisplaySettingsApi.getLogoBlob()
  revokeObjectUrl()
  currentObjectUrl  = URL.createObjectURL(blob)
  logoUrl.value     = currentObjectUrl
  logoMime.value    = settings.logoContentType || blob.type || 'image/png'
  logoDataUrl.value = await blobToDataUrl(blob)
  hasLogo.value     = true
}

/**
 * Carica il logo del tenant (una sola volta salvo force). Fail-safe: in errore
 * mantiene lo stato "nessun logo" (i consumatori useranno DEFAULT_LOGO).
 */
async function load(force = false) {
  if (loaded.value && !force) return
  if (loadingPromise) return loadingPromise

  loadingPromise = fetchLogo()
    .then(() => { loaded.value = true })
    .catch(() => { /* nessun logo / errore: stato default già impostato */ })
    .finally(() => { loadingPromise = null })

  return loadingPromise
}

/** Ricarica forzata (dopo upload/rimozione o cambio tenant). */
async function reload() {
  loaded.value = false
  return load(true)
}

/**
 * Restituisce il logo pronto per i report (data-URL base64), caricandolo se serve.
 * @returns {Promise<{dataUrl:string, mime:string}|null>} null se il tenant non ha logo.
 */
async function getReportLogo() {
  await load()
  if (!hasLogo.value || !logoDataUrl.value) return null
  return { dataUrl: logoDataUrl.value, mime: logoMime.value }
}

export function useTenantBranding() {
  return {
    hasLogo,
    logoUrl,
    logoDataUrl,
    logoMime,
    loaded,
    load,
    reload,
    getReportLogo,
    DEFAULT_LOGO,
  }
}
