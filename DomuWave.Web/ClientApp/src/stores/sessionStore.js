/**
 * sessionStore.js
 *
 * Gestisce il tenant attivo nella sessione corrente del superamministratore.
 *
 * Integrazione con api.js:
 *   L'interceptor di api.js legge già `localStorage.getItem('tenantId')` e lo
 *   manda come header `X-Tenant-Id` su ogni richiesta. Questo store si occupa
 *   di mantenere la lista dei tenant disponibili, di impostare/ripulire il
 *   valore in localStorage e di esporre lo stato reattivo al resto dell'app.
 *
 * Utilizzo tipico:
 *   const session = useSessionStore()
 *   await session.loadTenants()        // carica la lista (da chiamare al mount del layout)
 *   session.selectTenant(tenant)       // imposta il tenant attivo
 *   session.activeTenant               // tenant attualmente selezionato
 *   session.isSuperAdmin               // true se l'utente è superamministratore
 */

import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { tenantApi } from '@/services/tenantService'

const STORAGE_KEY = 'tenantId'
const STORAGE_TENANT_NAME_KEY = 'tenantName'
const UserProfile = 'domuwave_userprofile'

export const useSessionStore = defineStore('session', () => {
  // ─── STATE ─────────────────────────────────────────────────────────────────

  /** Lista completa dei tenant attivi (per la combo del superadmin) */
  const availableTenants = ref([])

  /** Tenant attualmente selezionato per la sessione */
  const savedTenantId   = localStorage.getItem(STORAGE_KEY)
  const savedTenantName = localStorage.getItem(STORAGE_TENANT_NAME_KEY)
  const activeTenant = ref(
    savedTenantId && savedTenantName ? { id: savedTenantId, name: savedTenantName } : null
  )

  /** Loading della lista tenant */
  const loadingTenants = ref(false)

  /** Lista condomini per utenti Condomino (profile == 3), popolata dalla risposta di login */
  const condominoCondominiums = ref([])

  /**
   * Tenant dove l'utente è AMMINISTRATORE (RoleCode != Condomino), dalla risposta di login.
   * Coesiste con condominoCondominiums: lo stesso utente può avere entrambi i ruoli.
   */
  const adminTenants = ref([])

  /**
   * Ruolo dell'utente corrente.
   * Adattare alla struttura del proprio authStore / oggetto utente.
   * Questo campo viene impostato da initFromAuth() al login.
   */
  const currentUserRole = ref(localStorage.getItem(UserProfile))

  // ─── GETTERS ───────────────────────────────────────────────────────────────

  const isSuperAdmin  = computed(() => currentUserRole.value == 1)
  const isTenantAdmin = computed(() => currentUserRole.value == 2)
  const isCondomino   = computed(() => currentUserRole.value == 3)

  const hasTenantSelected = computed(() => !!activeTenant.value)

  /** True se l'utente ha SIA tenant-admin SIA condomìni → selettore unificato a sezioni */
  const hasMixedRoles = computed(() =>
    adminTenants.value.length > 0 && condominoCondominiums.value.length > 0)

  // ─── INIT ──────────────────────────────────────────────────────────────────

  /**
   * Inizializza lo store al login o al mount del layout.
   * Ripristina il tenant salvato in localStorage se esiste ancora tra
   * quelli disponibili (es. dopo un refresh di pagina).
   *
   * @param {string} role - Ruolo dell'utente ('SuperAdmin', 'Admin', ecc.)
   */
  function initFromAuth(user) {
    console.log("initFromAuth", user.value);
    currentUserRole.value = user.value.profile;

    // Ripristina il tenant salvato in localStorage
    const savedId   = localStorage.getItem(STORAGE_KEY)
    const savedName = localStorage.getItem(STORAGE_TENANT_NAME_KEY)
    if (savedId && savedName) {
      activeTenant.value = { id: savedId, name: savedName }
    }

    // Ripristina i condomini del Condomino (se presenti)
    const storedTenants = localStorage.getItem('domuwave_condomino_condominiums')
    if (storedTenants) {
      try { condominoCondominiums.value = JSON.parse(storedTenants) } catch { /* ignore */ }
    }

    // Ripristina i tenant-admin (se presenti)
    const storedAdmin = localStorage.getItem('domuwave_admin_tenants')
    if (storedAdmin) {
      try { adminTenants.value = JSON.parse(storedAdmin) } catch { /* ignore */ }
    }
  }

  // ─── ACTIONS ───────────────────────────────────────────────────────────────

  /**
   * Carica tutti i tenant attivi dal backend.
   * Va chiamato una sola volta al mount del layout principale (es. AppSidebar).
   * Per i non-superadmin il tenant è già fisso nell'interceptor, quindi
   * questo metodo è no-op.
   */
  async function loadTenants() {
    console.log("loadTenants", isSuperAdmin.value);
    if (!isSuperAdmin.value) return

    loadingTenants.value = true
    try
    {
      const { data } = await tenantApi.getAll();
      // Filtra solo gli attivi e ordina per nome
      availableTenants.value = (data ?? [])
        .filter(t => t.isActive)
        .sort((a, b) => a.name.localeCompare(b.name))

      // Se c'è un tenant salvato in localStorage, verifica che sia ancora valido
      const savedId = localStorage.getItem(STORAGE_KEY)
      if (savedId) {
        const stillValid = availableTenants.value.find(t => t.id === savedId)
        if (stillValid) {
          activeTenant.value = stillValid
        } else {
          // Tenant rimosso o disattivato: pulisce lo stato
          clearTenant()
        }
      }
    }
    catch
    {
      // Errore già gestito dall'interceptor in api.js → toast in App.vue
      availableTenants.value = []
    } finally {
      loadingTenants.value = false
    }
  }

  /**
   * Imposta il tenant attivo.
   * Salva l'ID in localStorage (letto dall'interceptor di api.js come X-Tenant-Id)
   * e aggiorna lo stato reattivo.
   *
   * @param {{ id: string, name: string }} tenant
   */
  function selectTenant(tenant) {
    activeTenant.value = tenant
    localStorage.setItem(STORAGE_KEY, tenant.id)
    localStorage.setItem(STORAGE_TENANT_NAME_KEY, tenant.name)
  }

  /**
   * Salva la lista dei condomini del Condomino (profile==3).
   * @param {Array<{ id: string, name: string, code: string }>} tenants
   */
  function setCondominoCondominiums(tenants) {
    condominoCondominiums.value = tenants ?? []
    localStorage.setItem('domuwave_condomino_condominiums', JSON.stringify(condominoCondominiums.value))
  }

  /** Salva i tenant dove l'utente è amministratore (dalla risposta di login). */
  function setAdminTenants(tenants) {
    adminTenants.value = tenants ?? []
    localStorage.setItem('domuwave_admin_tenants', JSON.stringify(adminTenants.value))
  }

  /**
   * Aggiorna il profilo dell'utente a runtime (ruolo per-tenant).
   * Usato al cambio tenant quando lo stesso utente ha ruoli diversi su tenant diversi
   * (es. admin nel proprio tenant, condòmino nel tenant di un altro amministratore).
   * @param {number} profile  1=SuperAdmin, 2=TenantAdmin, 3=Condomino
   */
  function setProfile(profile) {
    currentUserRole.value = profile
    localStorage.setItem(UserProfile, profile)
  }

  /**
   * Rimuove il tenant di sessione (es. alla disconnessione o cambio utente).
   */
  function clearTenant() {
    activeTenant.value = null
    localStorage.removeItem(STORAGE_KEY)
    localStorage.removeItem(STORAGE_TENANT_NAME_KEY)
  }

  /**
   * Reset completo (da chiamare al logout).
   */
  function reset() {
    clearTenant()
    availableTenants.value = []
    condominoCondominiums.value = []
    adminTenants.value = []
    localStorage.removeItem('domuwave_condomino_condominiums')
    localStorage.removeItem('domuwave_admin_tenants')
    currentUserRole.value  = null
  }

  return {
    // state
    availableTenants,
    condominoCondominiums,
    adminTenants,
    activeTenant,
    loadingTenants,
    currentUserRole,
    // getters
    isSuperAdmin,
    isTenantAdmin,
    isCondomino,
    hasTenantSelected,
    hasMixedRoles,
    // actions
    initFromAuth,
    setCondominoCondominiums,
    setAdminTenants,
    setProfile,
    loadTenants,
    selectTenant,
    clearTenant,
    reset,
  }
})
