import axios from 'axios'

const authApiClient = axios.create({
  baseURL: import.meta.env.VITE_API_DOMUAPP_URL ?? 'https://localhost:5001',
  timeout: 15000,
  headers: {
    'Content-Type': 'application/json',
  },
})

// ── Request interceptor: attach JWT token ──────────────────────────────────
authApiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('domuwave_token')
    if (token) {
      config.headers['Authorization'] = `Bearer ${token}`
    }
    return config
  },
  (error) => Promise.reject(error)
)

// ── Response interceptor: handle 401 globally ─────────────────────────────
authApiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Token expired or invalid → clear storage and redirect to login
      localStorage.removeItem('domuwave_token')
      localStorage.removeItem('domuwave_user')
      // Avoid circular import: use window.location for hard redirect
      if (window.location.pathname !== '/login') {
        window.location.href = '/login'
      }
    }
    return Promise.reject(error)
  }
)

export default authApiClient
