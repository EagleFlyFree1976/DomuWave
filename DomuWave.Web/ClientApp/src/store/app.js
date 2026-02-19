import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { condominiumApi } from '@/services/api'

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

  let toastId = 0
  function toast(message, type = 'info', duration = 3500) {
    const id = ++toastId
    toasts.value.push({ id, message, type })
    setTimeout(() => { toasts.value = toasts.value.filter(t => t.id !== id) }, duration)
  }

  return { condomini, selectedCondominioId, selectedCondominio, loading, toasts, loadCondomini, selectCondominio, toast }
})
