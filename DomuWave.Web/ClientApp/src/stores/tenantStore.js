/**
 * tenantStore.js
 * Pinia store per la gestione dei Tenant.
 *
 * Gli errori provenienti dalle chiamate API vengono gestiti centralmente
 * dall'interceptor in api.js che emette un CustomEvent 'api:error',
 * ascoltato da App.vue che mostra il toast all'utente.
 *
 * Lo store si occupa quindi solo di:
 *   - mantenere lo stato (lista, elemento corrente, flag di caricamento)
 *   - eseguire le operazioni CRUD
 *   - ri-lanciare l'eccezione se il chiamante deve reagire (es. non chiudere il dialog)
 */

import { defineStore } from 'pinia'
import { ref, reactive } from 'vue'
import { tenantApi } from '@/services/tenantService'

export const useTenantStore = defineStore('tenant', () => {
  // ─── STATE ───────────────────────────────────────────────────────────────

  const tenants      = ref([])
  const currentTenant = ref(null)
  const totalCount   = ref(0)
  const loading      = ref(false)
  const saving       = ref(false)

  const query = reactive({
    page:      1,
    pageSize:  20,
    sortField: 'name',
    sortOrder: 'asc',
    search:    '',
    isActive:  null,   // null = tutti, true = attivi, false = inattivi
  })

  // ─── ACTIONS ─────────────────────────────────────────────────────────────

  async function fetchList() {
    loading.value = true
    try {
      const params = {
        page:      query.page,
        pageSize:  query.pageSize,
        sortOrder: query.sortOrder === 'asc',
      }
      if (query.sortField) params.sortField = query.sortField
      if (query.search)    params.search    = query.search
      if (query.isActive !== null && query.isActive !== undefined) {
        params.isActive = query.isActive
      }

      const { data } = await tenantApi.getPaged(params)
      tenants.value    = data.items    ?? []
      totalCount.value = data.totalCount ?? 0
    } catch {
      // Il toast è già mostrato dall'interceptor; azzeriamo lo stato locale
      tenants.value    = []
      totalCount.value = 0
    } finally {
      loading.value = false
    }
  }

  async function fetchById(id) {
    loading.value = true
    currentTenant.value = null
    try {
      const { data } = await tenantApi.getById(id)
      currentTenant.value = data
    } catch {
      // Errore già gestito dall'interceptor
    } finally {
      loading.value = false
    }
  }

  function initNew() {
    currentTenant.value = { id: null, name: '', code: '', isActive: true }
  }

  async function save() {
    saving.value = true
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
      // Ri-lancia così il componente (es. dialog) sa che il salvataggio è fallito
      throw err
    } finally {
      saving.value = false
    }
  }

  async function remove(id) {
    loading.value = true
    try {
      await tenantApi.delete(id)
      tenants.value    = tenants.value.filter(t => t.id !== id)
      totalCount.value = Math.max(0, totalCount.value - 1)
    } catch (err) {
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
    Object.assign(query, {
      page: 1, pageSize: 20, sortField: 'name', sortOrder: 'asc', search: '', isActive: null,
    })
  }

  return {
    tenants, currentTenant, totalCount, loading, saving, query,
    fetchList, fetchById, initNew, save, remove, setQueryParams, reset,
  }
})
