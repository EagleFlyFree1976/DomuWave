/**
 * userStore.js
 * Pinia store per la gestione degli Utenti.
 *
 * Gli errori API sono gestiti centralmente dall'interceptor in api.js
 * (CustomEvent 'api:error'). Lo store ri-lancia solo le eccezioni che
 * richiedono una reazione nel componente chiamante (es. chiusura dialog).
 */

import { defineStore } from 'pinia'
import { ref, reactive } from 'vue'
import { userApi } from '@/services/userService'
import { userTenantApi } from '@/services/userService'

export const useUserStore = defineStore('user', () => {

  // ─── STATE ─────────────────────────────────────────────────────────────────

  const users        = ref([])
  const currentUser  = ref(null)
  const totalCount   = ref(0)
  const loading      = ref(false)
  const saving       = ref(false)

  const query = reactive({
    search:          '',
    isActive:        null,   // null = tutti, true = attivi, false = inattivi
    tenantId:        null,   // filtro tenant (SuperAdmin)
    condominiumId:   null,   // filtro condominio (SuperAdmin)
  })

  // ─── ACTIONS ───────────────────────────────────────────────────────────────

  /**
   * Carica tutti gli utenti (SuperAdmin): nessun filtro per tenant.
   */
  async function fetchList() {
    loading.value = true
    try {
      const params = {}
      if (query.search)  params.search   = query.search
      if (query.isActive !== null && query.isActive !== undefined)
        params.isActive = query.isActive

      const { data } = await userApi.search(params)
      users.value      = Array.isArray(data) ? data : (data.items ?? [])
      totalCount.value = users.value.length
    } catch {
      users.value     = []
      totalCount.value = 0
    } finally {
      loading.value = false
    }
  }

  /**
   * Carica gli utenti del tenant corrente (TenantAdmin).
   * Recupera prima la lista degli userId associati al tenant,
   * poi carica i dettagli via search filtrando solo quelli del tenant.
   *
   * @param {string|number} tenantId  - ID del tenant attivo
   * @param {string[]} allowedRoles   - solo questi roleCode vengono mostrati
   */
  async function fetchListByTenant(tenantId, allowedRoles) {
    loading.value = true
    try {
      // 1. Recupera le associazioni utente-tenant per questo tenant
      const { data: tenantUsers } = await userTenantApi.getByTenantId(tenantId)
      const tenantUserIds = new Set(
        (Array.isArray(tenantUsers) ? tenantUsers : []).map(tu => String(tu.userId))
      )

      if (tenantUserIds.size === 0) {
        users.value      = []
        totalCount.value = 0
        return
      }

      // 2. Cerca gli utenti (con filtri testo/stato) limitandosi ai ruoli consentiti
      const params = {}
      if (query.search)   params.search   = query.search
      if (query.isActive !== null && query.isActive !== undefined)
        params.isActive = query.isActive
      if (allowedRoles?.length)
        params.roles = allowedRoles.join(',')

      const { data } = await userApi.search(params)
      const allUsers = Array.isArray(data) ? data : (data.items ?? [])

      // 3. Filtra solo quelli associati al tenant
      users.value      = allUsers.filter(u => tenantUserIds.has(String(u.id)))
      totalCount.value = users.value.length
    } catch {
      users.value     = []
      totalCount.value = 0
    } finally {
      loading.value = false
    }
  }

  async function fetchById(id) {
    loading.value = true
    currentUser.value = null
    try {
      const { data } = await userApi.getById(id)
      // Normalizza i campi del DTO verso il modello interno
      currentUser.value = data ? {
        id:        data.id,
        firstName: data.name,
        lastName:  data.lastName,
        email:     data.email,
        fullName:  data.fullName,
        roleCode: data.roleCode,
        role:data.role,
        isActive:  data.isActive,
        login:     data.login,
      } : null
    } catch {
      // Errore già gestito dall'interceptor
    } finally {
      loading.value = false
    }
  }

  function initNew() {
    currentUser.value = {
      id:        null,
      firstName: '',
      lastName:  '',
      name:      '',
      email:     '',
      password:  '',
      roleCode: '',
      role:'',
      isActive:  true,
    }
  }

  async function save() {
    saving.value = true
    try {
      const u = currentUser.value
      const payload = {
        email:    u.email,
        name:     u.firstName,
        surName:  u.lastName,
        roleCode: u.roleCode,
        role: u.role,
        isActive: u.isActive,
        ...(u.id
          ? {}
          : { password: u.password, moduleCode: 'DomuWeb' }),
      }

      if (u.id) {
        await userApi.update(u.id, payload)
      } else {
        const { data } = await userApi.create(payload)
        console.log("Data", data);
        currentUser.value = data ? {
          id:        data.id,
          firstName: data.name,
          lastName:  data.lastName,
          email:     data.email,
          fullName:  data.fullName,
          roleCode:  data.roleCode,
          role:  data.role,
          isActive:  data.isActive ?? true,
          login:     data.login,
        } : null
      }

      return currentUser.value
    } catch (err) {
      throw err
    } finally {
      saving.value = false
    }
  }

  async function remove(id) {
    loading.value = true
    try {
      await userApi.delete(id)
      users.value      = users.value.filter(u => u.id !== id)
      totalCount.value = Math.max(0, totalCount.value - 1)
    } catch (err) {
      throw err
    } finally {
      loading.value = false
    }
  }

  async function resetPassword(id) {
    try {
      await userApi.resetPassword(id)
    } catch (err) {
      throw err
    }
  }

  function setQueryParams(params) {
    Object.assign(query, params)
  }

  function reset() {
    users.value       = []
    currentUser.value = null
    totalCount.value  = 0
    loading.value     = false
    saving.value      = false
    Object.assign(query, { search: '', isActive: null, tenantId: null, condominiumId: null })
  }

  return {
    users, currentUser, totalCount, loading, saving, query,
    fetchList, fetchListByTenant, fetchById, initNew, save, remove, resetPassword,
    setQueryParams, reset,
  }
})
