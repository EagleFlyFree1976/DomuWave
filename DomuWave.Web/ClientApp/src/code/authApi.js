// api.js o axios.js
import axios from 'axios'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_AUTH_URL || 'http://localhost:601032/api',
  timeout: 10000,
})
api.interceptors.request.use(config => {
  const token = localStorage.getItem('auth_token');
 
  if (token) {
    config.headers['X-Auth-Token'] = token
  }
  
  return config
})
export default api
