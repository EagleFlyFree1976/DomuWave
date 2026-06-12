import { ref } from 'vue'
import { tenantDisplaySettingsApi } from '@/services/api'
import defaultLogo from '@/assets/domuwave-logo.svg'

// Logo di default DomuWave, mostrato quando il tenant non ha un logo personalizzato
// (e nella maschera di login, dove il tenant non è ancora noto).
export const DEFAULT_LOGO = defaultLogo

// Stato singleton condiviso tra tutti i consumatori (sidebar, pagina impostazioni…).
// `logoUrl` è null finché non si conosce un logo personalizzato → il client usa DEFAULT_LOGO.
const logoUrl = ref(null)
const loaded  = ref(false)
let   loadingPromise = null
let   currentObjectUrl = null   // object URL del blob corrente, da revocare quando cambia

function setObjectUrl(url) {
  if (currentObjectUrl) {
    URL.revokeObjectURL(currentObjectUrl)
    currentObjectUrl = null
  }
  currentObjectUrl = url
  logoUrl.value = url
}

function clearLogo() {
  if (currentObjectUrl) {
    URL.revokeObjectURL(currentObjectUrl)
    currentObjectUrl = null
  }
  logoUrl.value = null
}

/**
 * Carica il logo del tenant corrente.
 * 1) legge le impostazioni (hasLogo); 2) se presente, scarica il blob autenticato
 *    e ne crea un object URL utilizzabile in <img src>. Fail-safe: in errore → default.
 */
async function load(force = false) {
  if (loaded.value && !force) return
  if (loadingPromise) return loadingPromise

  loadingPromise = (async () => {
    try {
      const { data } = await tenantDisplaySettingsApi.get()
      if (data?.hasLogo) {
        const res  = await tenantDisplaySettingsApi.getLogoBlob()
        const blob = res?.data
        if (blob && blob.size > 0) {
          setObjectUrl(URL.createObjectURL(blob))
        } else {
          clearLogo()
        }
      } else {
        clearLogo()
      }
      loaded.value = true
    } catch {
      // fallback silenzioso al logo di default
      clearLogo()
    } finally {
      loadingPromise = null
    }
  })()

  return loadingPromise
}

/** Forza il ricaricamento (dopo upload/rimozione del logo o cambio tenant). */
async function reload() {
  return load(true)
}

export function useTenantBranding() {
  return { logoUrl, loaded, load, reload, DEFAULT_LOGO }
}
