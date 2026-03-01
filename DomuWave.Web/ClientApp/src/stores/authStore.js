import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

import api from '@/services/api'
import { useSessionStore } from '@/stores/sessionStore'
const STORAGE_KEY = 'tenantId'
const STORAGE_TENANT_NAME_KEY = 'tenantName'
const UserProfile = 'domuwave_userprofile'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('domuwave_token') || null)
  const user = ref(JSON.parse(localStorage.getItem('domuwave_user') || 'null'))
  const loading = ref(false)
  const error = ref(null)

  const isAuthenticated = computed(() => !!token.value)
  const currentUser = computed(() => user.value)

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
      if (user.value.profile == 3 && Array.isArray(data.availableCondominiums)) {
        session.setCondominoCondominiums(data.availableCondominiums)
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
  }

  return { token, user, loading, error, isAuthenticated, currentUser, login, logout }
})
