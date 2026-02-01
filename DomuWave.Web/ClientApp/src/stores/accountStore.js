import { defineStore } from 'pinia'
import accountApi from '@/code/accountApi'
import { toUtcDate } from '@/code/utils/dateUtils'
import axios from 'axios'
//import { processApiResponse, handleApiError } from '@/code/utils/apiUtils'
import { useMessageStore } from './messageStore'
import { useDomuWaveStore } from './domuWaveStore'
import { MESSAGES, TYPES } from '@/code/messages'
import router from '@/router'
export const useAccountStore = defineStore('accountStore', {
  state: () => ({
    accounts: null,
    
    createmode: true,
    loading: false,
    error: null,
    dashboarddata: null,
    edititem: null,
    createitem:null
  }),
  actions: {

    async loadDashboard(accountId) {
      const messageStore = useMessageStore();
      this.loading = true
      this.error = null
      try {
        const response = await accountApi.get(`/Accounts/${accountId}/dashboard`);
        this.dashboarddata = response.data
        console.log("this.dashboarddata", this.dashboarddata);
      } catch (err) {
        console.log("Error", err);
        messageStore.setMessages("Non è stato possibile caricare la dashboard dell\'account specificato", TYPES.error);
        this.error = err.response?.data?.message || 'Errore durante il caricamento dell\'account specificato';
      }
      finally {
        this.loading = false
      }
    },
    async createEntity(account) {
      await this.updateEntity(account);
    },
    async updateEntity(account) {
      const messageStore = useMessageStore();
      const DomuWaveStore = useDomuWaveStore();
      this.loading = true
      this.error = null


      try {
        let apiAction = "/Accounts";
        let response = null;
        if (account.id != 0) {
          apiAction = `${apiAction}/${account.id}`
            response = await accountApi.put(apiAction, {

              currencyId: account.currency.id,
              name: account.name,
              description: account.description,
              initialBalance: account.initialBalance,
              openDate: toUtcDate(account.openDate),
              closedDate: account.closedDate != null && account.closedDate != "" ? toUtcDate(account.closedDate) : null

            });
        }
        else {
          response = await accountApi.post(apiAction, {
            accountTypeId:account.accountType.id,
            currencyId: account.currency.id,
            name: account.name,
            description: account.description,
            initialBalance: account.initialBalance,
            openDate: toUtcDate(account.openDate)

          });
          this.edititem = { ...account }
          this.edititem.id = response.headers['x-key']; 
          account.id = response.headers['x-key']; 
          this.createmode = false;
        }
        
        await DomuWaveStore.loadMenu();

        this.edititem = { ...account }
        this.createmode = false;
        messageStore.addMessage(MESSAGES.SUCCESS.SAVE, TYPES.success);
        console.log("this.edititem", this.edititem);
      } catch (err) {
        
        if (err.response != null) {
          messageStore.setMessages(err.response.data.Errors, TYPES.error);
        }
        else {
          console.log("Error", err);
          messageStore.setMessages(MESSAGES.ERROR.SAVE, TYPES.error);
        }
        this.error = err.response?.data?.message || MESSAGES.ERROR.SAVE;
        
      } finally {
        this.loading = false
      }
    },

    
    async newEntity() {
     
      
      this.createitem =
      {
        "book": null,
        "accountType": null,
        "currency": null,
        "isActive": true,
        "balance": null,
        "openDate": null,
        "closedDate": null,
        "name": "[New}",
        "description": "",
        "id": 0
      }
      ;
 
    },
    async edit(accountId) {
      const messageStore = useMessageStore();
      this.loading = true
      this.error = null
      try {
        const response = await accountApi.get(`/Accounts/${accountId}`);

        this.edititem = response.data;
        this.createmode = false;
        console.log("this.edititem", this.edititem);
      } catch (err) {
        console.log("Error", err);
        messageStore.setMessages("Non è stato possibile caricare i dati dell'account", TYPES.error);
        this.error = err.response?.data?.message || 'Errore durante il caricamento dei dati dell\'account';


      } finally {
        this.loading = false
      }

    },
    async deleteAccount(accountId) {
      const messageStore = useMessageStore();
      const DomuWaveStore = useDomuWaveStore();
      this.loading = true
      this.error = null
      try {
        const response = await accountApi.delete(`/Accounts/${accountId}`);

        await this.loadAllAccounts();

        await DomuWaveStore.loadMenu();
      } catch (err) {
        console.log("Error", err);
        messageStore.setMessages(err.response.data.Errors, TYPES.error);
        if (err.response != null) {
          messageStore.setMessages(err.response.data.Errors, TYPES.error);
        }
        else {
          console.log("Error", err);
          messageStore.setMessages(MESSAGES.ERROR.DELETE, TYPES.error);
        }
        this.error = err.response?.data?.message || 'Errore durante il caricamento degli accounts'

      } finally {
        this.loading = false
      }
    },
    async loadAllAccounts() {
      const messageStore = useMessageStore();
      this.loading = true
      this.error = null
      try {
        const response = await accountApi.get('/Accounts');

        this.accounts = response.data
        
        
      } catch (err) {
        console.log("Error", err);
        messageStore.setMessages("Non è stato possibile caricare gli accounts", TYPES.error);
        this.error = err.response?.data?.message || 'Errore durante il caricamento degli accounts'    
        
      } finally {
        this.loading = false
      }
    },
    async loadLookups() {
      await this.loadAllAccounts();

    }

    
    
  },
  computed: {
    
  }
  ,
})
