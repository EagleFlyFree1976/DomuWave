import { ref, computed } from 'vue'
import bizlioApi from '@/code/bizlioApi'
import { defineStore } from 'pinia'
 
export const useBizlioStore  = defineStore('BizlioStore', {
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
        const res = await bizlioApi.get('/Menues');
        this.itemMenues = res.data;
      }
      else {
        this.itemMenues = [];
      }
    },
    async loadDefaultBook() {
      const res = await bizlioApi.get('/books/primary');

      if (res != null) {
        localStorage.setItem('bookId', res.data.id);
      }

    },
    async loadCurrencies(q) {
      const params = new URLSearchParams({ q });
      const res = await bizlioApi.get(`/currencies/lookups?${params}`);
      return res.data;
    },
    async loadAccountTypes() {
      
      const res = await bizlioApi.get(`/accounttype`);
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
