import { defineStore } from 'pinia'
import { ref, computed, watch } from 'vue'
import { condominiumApi, fiscalYearApi } from '@/services/api'

const SELECTED_CONDO_KEY = 'domuwave_selected_condo_id'

export const useAppStore = defineStore('app', () => {
  const condomini = ref([])
  const selectedCondominioId = ref(Number(localStorage.getItem(SELECTED_CONDO_KEY)) || null)
  const loading = ref(false)
  const toasts = ref([])

  const selectedCondominio = computed(() =>
    condomini.value.find(c => c.id === selectedCondominioId.value) || null
  )

  async function loadCondomini() {
    loading.value = true
    try {
      const { data } = await condominiumApi.getAll()
      condomini.value = data
      // Ripristina il condominio salvato se è ancora valido, altrimenti prendi il primo
      const saved = data.find(c => c.id === selectedCondominioId.value)
      if (!saved && data.length > 0) {
        selectedCondominioId.value = data[0].id
      }
    } catch (e) {
      console.error(e)
    } finally {
      loading.value = false
    }
  }

  function selectCondominio(id) {
    selectedCondominioId.value = id
  }

  // Persisti in localStorage ad ogni cambio
  watch(selectedCondominioId, (id) => {
    if (id) localStorage.setItem(SELECTED_CONDO_KEY, id)
    else localStorage.removeItem(SELECTED_CONDO_KEY)
  })

  // ─── Esercizio fiscale selezionato (persistente tra le pagine) ────────────
  const fiscalYears          = ref([])
  const selectedFiscalYearId = ref(null)

  const selectedFiscalYear = computed(() =>
    fiscalYears.value.find(f => f.id === selectedFiscalYearId.value) || null
  )

  function pickDefaultFiscalYear(years) {
    if (!years.length) return null
    // 1. Esercizio con isActive = true
    const active = years.find(f => f.isActive)
    if (active) return active.id
    // 2. Esercizio il cui periodo contiene oggi
    const today = new Date()
    today.setHours(0, 0, 0, 0)
    const covering = years.find(f => {
      const start = new Date(f.startDate)
      const end   = new Date(f.endDate)
      return start <= today && today <= end
    })
    if (covering) return covering.id
    // 3. Fallback: il più recente per data di inizio
    const sorted = [...years].sort((a, b) => new Date(b.startDate) - new Date(a.startDate))
    return sorted[0]?.id ?? null
  }

  async function loadFiscalYears() {
    if (!selectedCondominioId.value) {
      fiscalYears.value = []
      selectedFiscalYearId.value = null
      return
    }
    try {
      const { data } = await fiscalYearApi.getByCondominium(selectedCondominioId.value)
      fiscalYears.value = data ?? []
      // Mantieni la selezione corrente se è ancora valida per il nuovo condominio
      const stillValid = fiscalYears.value.find(f => f.id === selectedFiscalYearId.value)
      if (!stillValid) {
        selectedFiscalYearId.value = pickDefaultFiscalYear(fiscalYears.value)
      }
    } catch {
      fiscalYears.value = []
      selectedFiscalYearId.value = null
    }
  }

  // Al cambio condominio ricarica automaticamente gli esercizi
  watch(selectedCondominioId, () => {
    loadFiscalYears()
  })

  // ─── Toast ────────────────────────────────────────────────────────────────
  let toastId = 0
  function toast(message, type = 'info', duration = 3500) {
    const id = ++toastId
    toasts.value.push({ id, message, type })
    setTimeout(() => { toasts.value = toasts.value.filter(t => t.id !== id) }, duration)
  }

  return {
    condomini, selectedCondominioId, selectedCondominio, loading, toasts,
    loadCondomini, selectCondominio, toast,
    fiscalYears, selectedFiscalYearId, selectedFiscalYear, loadFiscalYears,
  }
})
