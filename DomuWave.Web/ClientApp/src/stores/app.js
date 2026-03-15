import { defineStore } from 'pinia'
import { ref, computed, watch } from 'vue'
import { condominiumApi, fiscalYearApi } from '@/services/api'

export const useAppStore = defineStore('app', () => {
  const condomini = ref([])
  const selectedCondominioId = ref(null)
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
      if (!selectedCondominioId.value && data.length > 0) {
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

  // ─── Esercizio fiscale selezionato (persistente tra le pagine) ────────────
  const fiscalYears          = ref([])
  const selectedFiscalYearId = ref(null)

  const selectedFiscalYear = computed(() =>
    fiscalYears.value.find(f => f.id === selectedFiscalYearId.value) || null
  )

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
        const active = fiscalYears.value.find(f => f.isActive) ?? fiscalYears.value[0]
        selectedFiscalYearId.value = active?.id ?? null
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
