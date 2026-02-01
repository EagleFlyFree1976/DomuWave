import { ref, computed } from 'vue'
import DomuWaveApi from '@/code/DomuWaveApi'
import { defineStore } from 'pinia'
 
export const useDomuWaveStore  = defineStore('DomuWaveStore', {
    state: () => ({
    currentUser: null, // { id, name, email, token, ... }
    options: {
      dateFormat: 'dd/mm/yy'
    },
    itemMenues: [],
    loading:false
     
    }),
  actions: {
    startLoading() {
      this.loading = true;
    },
    stopLoading() {
      this.loading = false;
    },
    async loadMenu() {
      const token = localStorage.getItem('auth_token')
      if (token != null && token != "") {
        const res = await DomuWaveApi.get('/Menues');
        this.itemMenues = res.data;
      }
      else {
        this.itemMenues = [];
      }
    },
    async loadDefaultBook() {
      const res = await DomuWaveApi.get('/books/primary');

      if (res != null) {
        localStorage.setItem('bookId', res.data.id);
      }

    },
    async loadCurrencies(q) {
      const params = new URLSearchParams({ q });
      const res = await DomuWaveApi.get(`/currencies/lookups?${params}`);
      return res.data;
    },
    async loadAccountTypes() {
      
      const res = await DomuWaveApi.get(`/accounttype`);
      return res.data;
    },
      async setUser(userData) {
        this.currentUser = userData;
        //
        if (userData != null) {
          await this.loadDefaultBook();
        }
        await this.loadMenu();
      },
      clearUser() {
        this.currentUser = null
      },
      isAuthenticated() {
        return !!this.currentUser
      }
    },
    persist: true // opzionale, vedi sotto per dettagli
  })
