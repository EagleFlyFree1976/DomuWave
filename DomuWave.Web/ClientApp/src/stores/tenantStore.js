/**
 * tenantStore.js
 * Pinia store per la gestione dei Tenant.
 */

import { defineStore } from 'pinia'
import { ref, reactive } from 'vue'
import { tenantApi } from '@/services/tenantService'

export const useTenantStore = defineStore('tenant', () => {
  // ─── STATE ───────────────────────────────────────────────────────────────

  const tenants    = ref([])
  const currentTenant = ref(null)
  const totalCount = ref(0)
  const loading    = ref(false)
  const saving     = ref(false)
  const error      = ref(null)

  const query = reactive({
    page:       1,
    pageSize:   20,
    sortField:  'name',
    sortOrder:  'asc',
    search:     '',
    isActive:   null,   // null = tutti, true = attivi, false = inattivi
  })

  // ─── ACTIONS ─────────────────────────────────────────────────────────────

  async function fetchList() {
    loading.value = true
    error.value   = null
    try {
      // Rimuove i parametri null/vuoti prima di inviare
      const params = Object.fromEntries(
        Object.entries(query).filter(([, v]) => v !== null && v !== '')
      )
      const { data } = await tenantApi.getPaged(params);
      console.log("Data", data);

      tenants.value    = data.items      ?? []
      totalCount.value = data.totalCount ?? 0
    } catch (err) {
      error.value   = err.response?.data?.message || err.message || 'Errore nel caricamento dei tenant'
      tenants.value = []
      totalCount.value = 0
    } finally {
      loading.value = false
    }
  }

  async function fetchById(id) {
    loading.value       = true
    error.value         = null
    currentTenant.value = null
    try {
      const { data } = await tenantApi.getById(id)
      currentTenant.value = data
    } catch (err) {
      error.value = err.response?.data?.message || err.message || `Tenant ${id} non trovato`
    } finally {
      loading.value = false
    }
  }

  function initNew() {
    currentTenant.value = { id: null, name: '', code: '', isActive: true }
    error.value = null
  }

  async function save() {
    saving.value = true
    error.value  = null
    try {
      const payload = {
        name:     currentTenant.value.name,
        code:     currentTenant.value.code,
        isActive: currentTenant.value.isActive,
      }
      const { data } = currentTenant.value.id
        ? await tenantApi.update(currentTenant.value.id, payload)
        : await tenantApi.create(payload)

      currentTenant.value = data
      return data
    } catch (err) {
      error.value = err.response?.data?.message || err.message || 'Errore durante il salvataggio'
      throw err
    } finally {
      saving.value = false
    }
  }

  async function remove(id) {
    loading.value = true
    error.value   = null
    try {
      await tenantApi.delete(id)
      tenants.value    = tenants.value.filter(t => t.id !== id)
      totalCount.value = Math.max(0, totalCount.value - 1)
    } catch (err) {
      error.value = err.response?.data?.message || err.message || `Errore nell'eliminazione`
      throw err
    } finally {
      loading.value = false
    }
  }

  function setQueryParams(params) {
    Object.assign(query, params)
    if (!('page' in params)) query.page = 1
  }

  function reset() {
    tenants.value       = []
    currentTenant.value = null
    totalCount.value    = 0
    loading.value       = false
    saving.value        = false
    error.value         = null
    Object.assign(query, { page: 1, pageSize: 20, sortField: 'name', sortOrder: 'asc', search: '', isActive: null })
  }

  return {
    tenants, currentTenant, totalCount, loading, saving, error, query,
    fetchList, fetchById, initNew, save, remove, setQueryParams, reset,
  }
})
