import { defineStore } from 'pinia'
import authApi from '@/code/authApi'
import axios from 'axios'
//import { processApiResponse, handleApiError } from '@/code/utils/apiUtils'
import { useMessageStore } from './messageStore'
import { useDomuWaveStore } from './domuWaveStore'
import { MESSAGES, TYPES } from '@/code/messages'
export const useAuthStore = defineStore('authStore', {
  state: () => ({
    user: null,
    token: null,
    loading: false,
    error: null
  }),
  actions: {
    async login(email, password) {
      const messageStore = useMessageStore();
      const DomuWaveStore = useDomuWaveStore();
      this.loading = true
      this.error = null
      try {
        const response = await authApi.post('/PublicUser/login', { email, password });
        
        this.token = response.data.token
        this.user = response.data;
        
        axios.defaults.headers.common['Authorization'] = `Bearer ${this.token}`;
        localStorage.setItem('auth_token', this.token);
        localStorage.setItem('auth_user', JSON.stringify(this.user));
 
        DomuWaveStore.setUser(this.user);
      } catch (err) {
        console.log("Error", err);
        messageStore.setMessages("Utente non trovato", TYPES.error)
        this.error = err.response?.data?.message || 'Errore durante il login'
        this.token = null
        this.user = null
        DomuWaveStore.setUser(this.user);
      } finally {
        this.loading = false
      }
    },
    async register(registeruser) {
      const messageStore = useMessageStore();
      this.loading = true
      this.error = null
      try {
        const response = await authApi.post('/PublicUser/register', registeruser);
        
        this.token = response.data.token
        this.user = response.data;
        
        axios.defaults.headers.common['Authorization'] = `Bearer ${this.token}`;
        localStorage.setItem('auth_token', this.token);
        localStorage.setItem('auth_user', JSON.stringify(this.user));
        messageStore.addMessage(MESSAGES.SUCCESS.SAVE, TYPES.success);

      } catch (err) {
        console.log("Error", err);
        messageStore.setMessages(err.response.data.Errors, TYPES.error);
        this.error = err.response?.data?.message || 'Errore durante il login'
        this.token = null
        this.user = null
      } finally {
        this.loading = false
      }
    },
    logout() {
      const DomuWaveStore = useDomuWaveStore();
      this.token = null
      this.user = null
      localStorage.removeItem('auth_token');
      localStorage.removeItem('auth_user');
      localStorage.removeItem('bookId');
      delete axios.defaults.headers.common['Authorization']
      DomuWaveStore.setUser(this.user);
    },
    loadFromStorage() {
      const token = localStorage.getItem('auth_token')
      const user = localStorage.getItem('auth_user')
      if (token && user) {
        this.token = token
        this.user = JSON.parse(user)
        axios.defaults.headers.common['Authorization'] = `Bearer ${token}`
      }
    },
    
  },
  compued: {
    isAuthenticated() {
      return this.user != null;
    }
  }
  ,
})
