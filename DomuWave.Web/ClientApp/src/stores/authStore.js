import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import authApiClient from '@/services/authApiClient'

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
      const response = await authApiClient.post('api/PublicUser/login', {
        "Email": username,
        "Password": password

      });

      


      const data = response.data
      token.value = data.token ?? data.accessToken ?? data.Token
      user.value = {
        id: data.userId ?? data.UserId,
        username: data.username ?? data.Username ?? username,
        displayName: data.displayName ?? data.DisplayName ?? username,
        role: data.role ?? data.Role,
      }
      localStorage.setItem('domuwave_token', token.value)
      localStorage.setItem('domuwave_user', JSON.stringify(user.value))
      return { success: true }
    } catch (err) {
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
    localStorage.removeItem('domuwave_user')
  }

  return { token, user, loading, error, isAuthenticated, currentUser, login, logout }
})
