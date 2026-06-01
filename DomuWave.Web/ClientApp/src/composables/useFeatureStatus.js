import { ref } from 'vue'
import { licenseApi } from '@/services/api'

// Stato singleton — condiviso tra tutti i componenti che importano questo composable
const _status  = ref({})   // { [featureCode]: FeatureStatusDto }
const _loading = ref(false)

export function useFeatureStatus() {
  async function load() {
    if (_loading.value) return
    _loading.value = true
    try {
      const { data } = await licenseApi.getStatus()
      _status.value = Object.fromEntries(data.map(f => [f.code, f]))
    } catch {
      // fail-open: se LM non risponde non blocchiamo nulla
    } finally {
      _loading.value = false
    }
  }

  function reset() {
    _status.value = {}
  }

  /**
   * La feature è abilitata (ha licenza attiva).
   * Default: true (fail-open se lo status non è ancora caricato)
   */
  function isEnabled(code) {
    if (!Object.keys(_status.value).length) return true
    return code in _status.value
  }

  /**
   * La feature è a consumo e ha crediti esauriti.
   */
  function isExhausted(code) {
    return _status.value[code]?.isExhausted ?? false
  }

  /**
   * La feature è a consumo e si avvicina al limite (>=90% usato).
   */
  function isWarning(code) {
    return _status.value[code]?.isWarning ?? false
  }

  /**
   * Crediti rimanenti. Null = illimitati o feature non consumabile.
   */
  function remaining(code) {
    return _status.value[code]?.remaining ?? null
  }

  function limit(code) {
    return _status.value[code]?.limit ?? null
  }

  function used(code) {
    return _status.value[code]?.used ?? 0
  }

  return { load, reset, isEnabled, isExhausted, isWarning, remaining, limit, used }
}
