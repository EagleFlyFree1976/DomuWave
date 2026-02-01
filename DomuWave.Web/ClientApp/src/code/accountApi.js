// api.js o axios.js
import axios from 'axios'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BLIZIO_URL || 'http://localhost:60101/api',
  timeout: 10000,
})
api.interceptors.request.use(config => {
  const token = localStorage.getItem('auth_token');
  const bookId = localStorage.getItem('bookId');

  if (token) {
    config.headers['X-Auth-Token'] = token
  }
  console.log("BOOK G", bookId);
  if (bookId != null) {
    console.log("Aggiungo book come header: id:", bookId);
    config.headers['X-Book-Id'] = bookId;

  }
  return config
})
export default api
