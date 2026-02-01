import { defineStore } from 'pinia'
import systemApi from '@/code/systemApi'
import { toUtcDate } from '@/code/utils/dateUtils'
import axios from 'axios'
//import { processApiResponse, handleApiError } from '@/code/utils/apiUtils'
import { useMessageStore } from './messageStore'
import { useDomuWaveStore } from './domuWaveStore'
import { MESSAGES, TYPES } from '@/code/messages'
import router from '@/router'
export const usePaymentMethodStore = defineStore('paymentMethodStore', {
  state: () => ({
    paymentmethods: null,
    createmode: false,
    loading: false,
    error: null,
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
    async updateEntity(paymentmethod) {
      const messageStore = useMessageStore();
      const DomuWaveStore = useDomuWaveStore();
      this.loading = true
      this.error = null


      try {
        let apiAction = "/Admin/PaymentMethods";
        let response = null;
        if (paymentmethod.id != 0) {
          apiAction = `${apiAction}/${paymentmethod.id}`
          response = await systemApi.put(apiAction, {
              name: paymentmethod.name,
              description: paymentmethod.description 

            });
        }
        else {
          response = await systemApi.post(apiAction, {
           
            name: paymentmethod.name,
            description: paymentmethod.description
           

          });

          this.edititem.id = response.headers['x-key']; 
          paymentmethod.id = response.headers['x-key']; 
          this.createmode = false;
        }
        
        DomuWaveStore.loadMenu();

        this.edititem = { ...paymentmethod }
        this.createmode = false;
        messageStore.addMessage(MESSAGES.SUCCESS.SAVE, TYPES.success);
        console.log("this.edititem", this.edititem);
      } catch (err) {
        console.log("Error", err);
        messageStore.setMessages(err.response.data.Errors, TYPES.error);
        this.error = err.response?.data?.message || MESSAGES.ERROR.SAVE;
        
      } finally {
        this.loading = false
      }
    },

    
    async newEntity() {
     
      this.createmode = true;
      this.edititem =
      {
        
        "name": "[New]",
        "description": "",
        "id": 0
      }
      ;
 
    },
    async edit(paymentmethodId) {
      const messageStore = useMessageStore();
      this.loading = true
      this.error = null
      try {
        const response = await systemApi.get(`/PaymentMethods/${paymentmethodId}`);

        this.edititem = response.data;
        this.createmode = false;
        console.log("this.edititem", this.edititem);
      } catch (err) {
        console.log("Error", err);
        messageStore.setMessages("Non è stato possibile caricare i dati", TYPES.error);
        this.error = err.response?.data?.message || 'Errore durante il caricamento dei dati';


      } finally {
        this.loading = false
      }

    },
    async deletePaymentMethod(paymentMethodId) {
      const messageStore = useMessageStore();
      this.loading = true
      this.error = null
      try {
        const response = await systemApi.delete(`/PaymentMethods/${paymentMethodId}`);

        await this.loadAllPaymentMethods();


      } catch (err) {
        console.log("Error", err);
        messageStore.setMessages(err.response.data.Errors, TYPES.error);
        
        this.error = err.response?.data?.message || 'Errore durante il caricamento dei metodi di pagamento'

      } finally {
        this.loading = false
      }
    },

    async disablePaymentMethod(paymentMethodId) {
      const messageStore = useMessageStore();
      this.loading = true
      this.error = null
      try {
        const response = await systemApi.patch(`/PaymentMethods/${paymentMethodId}/disable`);

        await this.loadAllPaymentMethods();


      } catch (err) {
        console.log("Error", err);
        messageStore.setMessages(err.response.data.Errors, TYPES.error);
        
        this.error = err.response?.data?.message || 'Errore durante il caricamento dei metodi di pagamento'

      } finally {
        this.loading = false
      }
    },
    async enablePaymentMethod(paymentMethodId) {
      const messageStore = useMessageStore();
      this.loading = true
      this.error = null
      try {
        const response = await systemApi.patch(`/PaymentMethods/${paymentMethodId}/enable`);

        await this.loadAllPaymentMethods();


      } catch (err) {
        console.log("Error", err);
        messageStore.setMessages(err.response.data.Errors, TYPES.error);
        
        this.error = err.response?.data?.message || 'Errore durante il caricamento dei metodi di pagamento'

      } finally {
        this.loading = false
      }
    },
    async loadAllPaymentMethods() {
      const messageStore = useMessageStore();
      this.loading = true
      this.error = null
      try {
        const response = await systemApi.get('/PaymentMethods');

        this.paymentmethods = response.data
        
        
      } catch (err) {
        console.log("Error", err);
        messageStore.setMessages("Non è stato possibile caricare i metodi di pagamento", TYPES.error);
        this.error = err.response?.data?.message || 'Errore durante il caricamento dei metodi di pagamento' 
        
      } finally {
        this.loading = false
      }
    }

    
    
  },
  computed: {
    
  }
  ,
})
