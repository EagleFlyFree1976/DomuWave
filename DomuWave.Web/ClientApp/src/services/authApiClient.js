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

// ── Response interceptor: dispatch api:error events (come api.js) ──────────
function extractErrorMessage(err) {
  const data = err.response?.data
  if (typeof data === 'string' && data.length > 0) return data
  if (data?.message) return data.message
  if (data?.title)   return data.title
  if (data?.detail)  return data.detail
  if (err.message)   return err.message
  return 'Si è verificato un errore imprevisto'
}

authApiClient.interceptors.response.use(
  res => res,
  err => {
    const status  = err.response?.status
    const message = extractErrorMessage(err)

    if (!err.response) {
      window.dispatchEvent(new CustomEvent('api:error', {
        detail: { status: null, message: 'Impossibile raggiungere il server. Controlla la connessione.' }
      }))
      return Promise.reject(err)
    }

    if (status === 401) {
      window.dispatchEvent(new CustomEvent('api:error', {
        detail: { status: 401, message: 'Operazione non autorizzata' }
      }))
      return Promise.reject(err)
    }

    if (status === 403) {
      window.dispatchEvent(new CustomEvent('api:error', {
        detail: { status: 403, message: 'Non hai i permessi per eseguire questa operazione' }
      }))
      return Promise.reject(err)
    }

    if (status === 422 || status === 400) {
      window.dispatchEvent(new CustomEvent('api:error', {
        detail: { status, message }
      }))
      return Promise.reject(err)
    }

    if (status >= 500) {
      window.dispatchEvent(new CustomEvent('api:error', {
        detail: { status, message: 'Errore del server. Riprova più tardi.' }
      }))
      return Promise.reject(err)
    }

    window.dispatchEvent(new CustomEvent('api:error', {
      detail: { status, message }
    }))
    return Promise.reject(err)
  }
)

export default authApiClient
