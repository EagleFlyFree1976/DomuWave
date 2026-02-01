import { defineStore } from 'pinia'
import systemApi from '@/code/systemApi'
import { toUtcDate } from '@/code/utils/dateUtils'
import axios from 'axios'
//import { processApiResponse, handleApiError } from '@/code/utils/apiUtils'
import { useMessageStore } from './messageStore'
import { useDomuWaveStore } from './domuWaveStore'
import { MESSAGES, TYPES } from '@/code/messages'
import router from '@/router'
export const useExchangeRateHistoryStore = defineStore('exchangeRateHistoryStore', {
  state: () => ({
    exchangeRateHistory: null,
 
    pageSize: 20,
    page: 1,
    totalRecords: null,
    totalPages:null,
    createmode: false,
    loading: false,
    error: null,
    edititem: null,
    createitem: null,
    targetDate: null,
    toCurrencyId: null,
    sortBy: null,
    sortAscending: null,
  }),
  actions: {

 
    async createEntity(account) {
      return await this.updateEntity(account);
    },
    async updateEntity(exchangeRateHistory) {
      const messageStore = useMessageStore();
      const DomuWaveStore = useDomuWaveStore();
      this.loading = true
      this.error = null


      try {
        let apiAction = "/currencies/exchange";
        let response = null;
        if (exchangeRateHistory.id != 0) {
          apiAction = `${apiAction}/${exchangeRateHistory.id}`
          response = await systemApi.put(apiAction, {
            toCurrencyId: exchangeRateHistory.toCurrency.id,
            rate: exchangeRateHistory.rate,
            targetDate: toUtcDate(exchangeRateHistory.validFrom), 
            });
        }
        else {
          response = await systemApi.post(apiAction, {
            toCurrencyId: exchangeRateHistory.toCurrency.id,
            rate: exchangeRateHistory.rate,
            targetDate: toUtcDate(exchangeRateHistory.validFrom)
          });

          this.edititem.id = response.headers['x-key']; 
          exchangeRateHistory.id = response.headers['x-key']; 
          this.createmode = false;
        }
        
        

        this.edititem = { ...exchangeRateHistory }
        this.createmode = false;
        messageStore.addMessage(MESSAGES.SUCCESS.SAVE, TYPES.success);
        console.log("this.edititem", this.edititem);
        return true;
      } catch (err) {
        console.log("Error", err);
        messageStore.setMessages(err.response.data.Errors, TYPES.error);
        this.error = err.response?.data?.message || MESSAGES.ERROR.SAVE;
        return false;
        
      } finally {
        this.loading = false
      }
    },

    
    async newEntity() {
     
      this.createmode = true;
      this.edititem =
      {
        
        "toCurrencyId": {"id":null, "description":null},
        "rate": null,
        "validFrom": new Date(),
        "id": 0
      }
      ;
 
    },
    async edit(exchangeRateHistoryId) {
      const messageStore = useMessageStore();
      this.loading = true
      this.error = null
      try {
        const response = await systemApi.get(`/currencies/exchange/${exchangeRateHistoryId}`);

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
    async deleteExchangeRateHistory(exchangeRateHistoryId) {
      const messageStore = useMessageStore();
      this.loading = true
      this.error = null
      try {
        const response = await systemApi.delete(`/currencies/exchange/${exchangeRateHistoryId}`);

        await this.loadAllexchangeRateHistory(this.targetDate, this.toCurrencyId, this.sortBy, this.sortAscending);


      } catch (err) {
        console.log("Error", err);
        messageStore.setMessages(err.response.data.Errors, TYPES.error);
        
        this.error = err.response?.data?.message || 'Errore durante il caricamento dei metodi di pagamento'

      } finally {
        this.loading = false
      }
    },

    async onPageChange(_page) {
      this.page = _page;
      await this.loadAllexchangeRateHistory(this.targetDate, this.toCurrencyId, this.sortBy, this.sortAscending);

    },
   
    async filterAllexchangeRateHistory(_targetDate, _toCurrencyId, _sortBy, _sortAsc) {
      this.page = 1;
      this.pageSize = 20;
      await this.loadAllexchangeRateHistory(_targetDate, _toCurrencyId, _sortBy, _sortAsc);
    },
    async loadAllexchangeRateHistory(_targetDate,_toCurrencyId, _sortBy, _sortAsc) {
      const messageStore = useMessageStore();
      this.loading = true
      this.error = null
      try {
        this.targetDate = _targetDate;
        this.toCurrencyId = _toCurrencyId;
        this.sortBy = _sortBy;
        this.sortAscending = _sortAsc;
        var strTargetDate = this.targetDate ? toUtcDate(this.targetDate) : "";
        var strToCurrencyId = this.toCurrencyId ? this.toCurrencyId : "";
        var strPage = this.page ? this.page : 1;
        var strPageSize = this.pageSize ? this.pageSize : 20;
        var strSortBy = this.sortBy ? this.sortBy : "";
        var strSortAscending = this.sortAscending ? this.sortAscending=='asc' : true;
        const response = await systemApi.get(`/currencies/exchange?targetDate=${strTargetDate}&toCurrencyId=${strToCurrencyId}&page=${strPage}&pageSize=${strPageSize}&sortBy=${strSortBy}&asc=${strSortAscending}`);

        this.exchangeRateHistory = response.data.items;
        this.pageSize = response.data.pageSize;
        this.page = response.data.pageNumber;
        this.totalRecords = response.data.totalCount;
        this.totalPages = response.data.totalPages;
        
        
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
