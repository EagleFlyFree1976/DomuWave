/**
 * userStore.js
 * Pinia store per la gestione degli Utenti (sezione SuperAdmin).
 *
 * Gli errori API sono gestiti centralmente dall'interceptor in api.js
 * (CustomEvent 'api:error'). Lo store ri-lancia solo le eccezioni che
 * richiedono una reazione nel componente chiamante (es. chiusura dialog).
 */

import { defineStore } from 'pinia'
import { ref, reactive } from 'vue'
import { userApi } from '@/services/userService'

export const useUserStore = defineStore('user', () => {

  // ─── STATE ─────────────────────────────────────────────────────────────────

  const users        = ref([])
  const currentUser  = ref(null)
  const totalCount   = ref(0)
  const loading      = ref(false)
  const saving       = ref(false)

  const query = reactive({
    search:   '',
    isActive: null,   // null = tutti, true = attivi, false = inattivi
  })

  // ─── ACTIONS ───────────────────────────────────────────────────────────────

  async function fetchList() {
    loading.value = true
    try {
      const params = {}
      if (query.search)  params.search   = query.search
      if (query.isActive !== null && query.isActive !== undefined)
        params.isActive = query.isActive

      const { data } = await userApi.search(params)
      users.value     = Array.isArray(data) ? data : (data.items ?? [])
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
        roleCode:  data.role,
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
      roleCode:  '',
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
        isActive: u.isActive,
        ...(u.id
          ? {}
          : { password: u.password, moduleCode: 'DomuWeb' }),
      }

      if (u.id) {
        // UPDATE: il PUT può restituire corpo vuoto (204) — non sovrascrivere
        // currentUser che è già in formato normalizzato
        await userApi.update(u.id, payload)
      } else {
        // CREATE: normalizza la risposta con gli stessi nomi campi di fetchById
        const { data } = await userApi.create(payload)
        currentUser.value = data ? {
          id:        data.id,
          firstName: data.name,
          lastName:  data.lastName,
          email:     data.email,
          fullName:  data.fullName,
          roleCode:  data.role,
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
      users.value     = users.value.filter(u => u.id !== id)
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
    Object.assign(query, { search: '', isActive: null })
  }

  return {
    users, currentUser, totalCount, loading, saving, query,
    fetchList, fetchById, initNew, save, remove, resetPassword,
    setQueryParams, reset,
  }
})
