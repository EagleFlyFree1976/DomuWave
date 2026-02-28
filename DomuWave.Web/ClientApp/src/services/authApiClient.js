import axios from 'axios'

const authApiClient = axios.create({
  baseURL: import.meta.env.VITE_API_AUTH_URL ?? 'https://localhost:5001',
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
      config.headers.Authorization = `Bearer ${token}`
      config.headers['X-Auth-Token'] = token;
    }
    return config
  },
  (error) => Promise.reject(error)
)



export default authApiClient
