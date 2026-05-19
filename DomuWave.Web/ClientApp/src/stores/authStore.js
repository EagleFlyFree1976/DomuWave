import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

import api from '@/services/api'

import { useSessionStore } from '@/stores/sessionStore'
const STORAGE_KEY = 'tenantId'
const STORAGE_TENANT_NAME_KEY = 'tenantName'
const UserProfile = 'domuwave_userprofile'
const IMPERSONATION_KEY = 'domuwave_impersonation_original'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('domuwave_token') || null)
  const user = ref(JSON.parse(localStorage.getItem('domuwave_user') || 'null'))
  const loading = ref(false)
  const error = ref(null)

  const isAuthenticated = computed(() => !!token.value)
  const currentUser = computed(() => user.value)
  const isImpersonating = computed(() => !!localStorage.getItem(IMPERSONATION_KEY))

  async function requestReset(email) {
    console.log("[AuthStore] request-reset-password:", email);
    await api.post('auth/reset-password', {
      email: email,
    })
  }
  async function login(username, password) {
    loading.value = true
    error.value = null
    console.log("[AuthStore] Attempting login for user:", username);
    try {
      const response = await api.post('PublicUser/login', {
        "Email": username,
        "Password": password
      });

      
      


      const data = response.data

      if (data != null && !data.isActive) {
        const msg =
                    'Utenza disabilitata'
        error.value = msg
        return { success: false, message: msg }
      }

      if (data.tenant == null) {
                const msg =
                    'Nessun tenant associato all\'utenza'
        error.value = msg
        return { success: false, message: msg }
      
      }


      token.value = data.token ?? data.accessToken ?? data.Token
      user.value = {
        id: data.userId ?? data.UserId,
        username: data.username ?? data.Username ?? username,
        displayName: data.displayName ?? data.DisplayName ?? username,
        role: data.role ?? data.Role,
        profile: data.profile
      }
      console.log("DATA", data);

      const session = useSessionStore()
      session.initFromAuth(user)
      localStorage.setItem('domuwave_token', token.value)
      localStorage.setItem(STORAGE_KEY, data.tenant.id)
      localStorage.setItem(STORAGE_TENANT_NAME_KEY, data.tenant.Name)
      localStorage.setItem('domuwave_user', JSON.stringify(user.value))
      localStorage.setItem('domuwave_userprofile', user.value.profile)

      // ── Flusso Condomino: salva la lista dei condomini nello store ──────────
      // Profilo 3 = User/Condomino; la lista arriva già dalla risposta di login
      if (user.value.profile == 3) {
        console.log('[Condomino] availableCondominiums:', data.availableCondominiums)
        if (Array.isArray(data.availableCondominiums) && data.availableCondominiums.length > 0) {
          session.setCondominoCondominiums(data.availableCondominiums)
        } else {
          console.warn('[Condomino] Nessun condominio ricevuto dal login — controlla GetCondominiumsByCondominoUserIdAsync')
        }
      }
      // ────────────────────────────────────────────────────────────────────────

      return { success: true }
    } catch (err) {
      console.log("Errr", err);
      const msg =
        err.response?.data?.message ??
        err.response?.data?.Message ??
        err.response?.data?.title ??
        'Credenziali non valide o errore di connessione.'
      error.value = msg
      return { success: false, message: msg }
    } finally {
      loading.value = false
    }
  }

  function logout() {
    token.value = null
    user.value = null

    localStorage.removeItem('domuwave_token')
    localStorage.removeItem(STORAGE_KEY)
    localStorage.removeItem(STORAGE_TENANT_NAME_KEY)
    localStorage.removeItem('domuwave_user')
    localStorage.removeItem('domuwave_userprofile')
    localStorage.removeItem('domuwave_selected_condo_id')
    localStorage.removeItem(IMPERSONATION_KEY)
  }

  async function impersonate(userId) {
    loading.value = true
    error.value = null
    try {
      const response = await api.post(`users/${userId}/impersonate`)
      const data = response.data

      // Salva la sessione originale prima di sovrascriverla
      localStorage.setItem(IMPERSONATION_KEY, JSON.stringify({
        token:      token.value,
        user:       user.value,
        tenantId:   localStorage.getItem(STORAGE_KEY),
        tenantName: localStorage.getItem(STORAGE_TENANT_NAME_KEY),
        profile:    localStorage.getItem(UserProfile),
      }))

      // Imposta la sessione dell'utente impersonato
      token.value = data.token
      user.value = {
        id:          data.id ?? data.userId,
        username:    data.name,
        displayName: data.fullName ?? data.name,
        role:        data.role,
        profile:     data.profile,
      }
      localStorage.setItem('domuwave_token', token.value)
      localStorage.setItem('domuwave_user', JSON.stringify(user.value))
      localStorage.setItem(UserProfile, user.value.profile)

      const session = useSessionStore()
      session.initFromAuth(user)

      if (data.tenant) {
        localStorage.setItem(STORAGE_KEY, data.tenant.id)
        localStorage.setItem(STORAGE_TENANT_NAME_KEY, data.tenant.name)
        session.selectTenant(data.tenant)
      }

      return { success: true, data }
    } catch (err) {
      const msg = err.response?.data?.message ?? err.response?.data?.title ?? 'Impossibile impersonare l\'utente'
      error.value = msg
      return { success: false, message: msg }
    } finally {
      loading.value = false
    }
  }

  function exitImpersonation() {
    const original = JSON.parse(localStorage.getItem(IMPERSONATION_KEY) || 'null')
    if (!original) return

    token.value = original.token
    user.value  = original.user
    localStorage.setItem('domuwave_token', original.token)
    localStorage.setItem('domuwave_user', JSON.stringify(original.user))
    localStorage.setItem(UserProfile, original.profile)

    if (original.tenantId) {
      localStorage.setItem(STORAGE_KEY, original.tenantId)
      localStorage.setItem(STORAGE_TENANT_NAME_KEY, original.tenantName)
    } else {
      localStorage.removeItem(STORAGE_KEY)
      localStorage.removeItem(STORAGE_TENANT_NAME_KEY)
    }

    localStorage.removeItem(IMPERSONATION_KEY)

    const session = useSessionStore()
    session.initFromAuth(user)
    if (original.tenantId) session.selectTenant({ id: original.tenantId, name: original.tenantName })
  }

  return { token, user, loading, error, isAuthenticated, currentUser, isImpersonating, login, logout, requestReset, impersonate, exitImpersonation }
})
