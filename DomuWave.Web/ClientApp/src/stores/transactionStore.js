import { defineStore } from 'pinia'
import systemApi from '@/code/systemApi'
import accountApi from '@/code/accountApi'
import { toUtcDate } from '@/code/utils/dateUtils'
import { filterToDateString, filterToString } from '../code/utils/apiUtils'
import axios from 'axios'
//import { processApiResponse, handleApiError } from '@/code/utils/apiUtils'
import { useMessageStore } from './messageStore'
import { useDomuWaveStore } from './domuWaveStore'
 
import { MESSAGES, TYPES } from '@/code/messages'
import router from '@/router'

export const useTransactionStore = defineStore('transactionStore', {
  state: () => ({
    transactions: null,
    selectedRows:[],
    pageSize: 20,
    page: 1,
    totalRecords: null,
    totalPages:null,
    createmode: false,
    loading: false,
    error: null,
    edititem: null,
    createitem: null,
    filterLookups: {
      accounts: [],
      categories: null,
      transactionTypes: [],
      paymentMethods: [],
      flowDirections: [],
      statuses: [],
      beneficiarys: [],
    },
    currentFilter: {
      targetAccountId:null,
      accountId: null,
      categoryId: null,
      fromDate: null,
      toDate: null,
      transactionType: null,
      flowDirection: null,
      status: null,
      note: null
    },
    massiveEditItem: {
      "updateAccountId": false,
      "updateDescription": false,
      "updateStatus": false,
      "updateAmount": false,
      "updateCurrencyId": false,
      "updateBeneficiary": false,
      "updatePaymentMethodId": false,
      "updateCategoryId": false,
      "updateTransactionDate": false,
      "updateTransactionType": false,
      "updateDestinationAccountId": false,
      "accountId": null,
      "destinationAccountId": null,
      "transactionType": null,
      "description": "",
      "status": null,
      "amount": null,
      "currencyId": null,
      "beneficiary": null,
      "paymentMethodId": null,
      "categoryId": null,
      "transactionDate": null,
      "TransactionIds": []
    },

    sortBy: null,
    sortAscending: null,
  }),
  actions: {

 
    async createEntity(account) {
      return await this.updateEntity(account);
    },
    async updateEntity(transaction) {
      const messageStore = useMessageStore();
      const DomuWaveStore = useDomuWaveStore();
      this.loading = true
      this.error = null


      try {
        let apiAction = "/Transactions";
        let response = null;
        if (transaction.id != 0) {
          apiAction = `${apiAction}/${transaction.id}`
          response = await systemApi.put(apiAction,

            {
              accountId: transaction.account.id,
              destinationAccountId: transaction.destinationAccount != null ? transaction.destinationAccount.id : null,
              transactionType: transaction.transactionType,
              description: transaction.description,
              status: transaction.status.id,
              amount: transaction.amount,
              currencyId: transaction.currency != null ? transaction.currency.id : null,
              beneficiary: transaction.beneficiary,
              paymentMethodId: transaction.paymentMethod.id,
              categoryId: transaction.category.id,
              //"tags": transaction.tags,
              
              transactionDate: toUtcDate(transaction.transactionDate),
                         

            });
        }
        else {
          response = await systemApi.post(apiAction, {
            accountId: transaction.account.id,
            destinationAccountId: transaction.destinationAccount != null ? transaction.destinationAccount.id : null,
            transactionType: transaction.transactionType,
            description: transaction.description,
            status: transaction.status.id,
            amount: transaction.amount,
            currencyId: transaction.currency != null ? transaction.currency.id : null,
            beneficiary: transaction.beneficiary,
            paymentMethodId: transaction.paymentMethod.id,
            categoryId: transaction.category.id,
            tags: transaction.tags,
            transactionDate: toUtcDate(transaction.transactionDate),


          });

          this.edititem.id = response.headers['x-key']; 
          transaction.id = response.headers['x-key']; 
          this.createmode = false;
        }
        
        

        this.edititem = { ...transaction }
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

    async saveMassive(messiveEdityEntity) {
      const messageStore = useMessageStore();
      this.loading = true
      this.error = null
      try {

        messiveEdityEntity.TransactionIds = this.selectedRows;

        var dtoMassiveEntity = {
          accountId: messiveEdityEntity.updateAccountId && messiveEdityEntity.account != null ? messiveEdityEntity.account.id : null,
          destinationAccountId: messiveEdityEntity.updateDestinationAccountId && messiveEdityEntity.destinationAccount != null ? messiveEdityEntity.destinationAccount.id : null,
          transactionType: messiveEdityEntity.updateTransactionType && messiveEdityEntity.transactionType != null ? messiveEdityEntity.transactionType : null,
          description: messiveEdityEntity.updateDescription && messiveEdityEntity.description != null ? messiveEdityEntity.description : null,
          status: messiveEdityEntity.updateStatus && messiveEdityEntity.status != null ? messiveEdityEntity.status.id : null,
          currencyId: messiveEdityEntity.updateCurrencyId && messiveEdityEntity.currency != null ? messiveEdityEntity.currency.id : null,
          amount: messiveEdityEntity.updateAmount && messiveEdityEntity.amount != null ? messiveEdityEntity.amount : null,
          beneficiary: messiveEdityEntity.updateBeneficiary && messiveEdityEntity.beneficiary != null ? messiveEdityEntity.beneficiary : null,
          beneficiaryId: messiveEdityEntity.updateBeneficiary && messiveEdityEntity.beneficiary != null ? messiveEdityEntity.beneficiary.id : null,
          paymentMethodId: messiveEdityEntity.updatePaymentMethodId && messiveEdityEntity.paymentMethod != null ? messiveEdityEntity.paymentMethod.id : null,
          categoryId: messiveEdityEntity.updateCategoryId && messiveEdityEntity.category != null ? messiveEdityEntity.category.id : null,
          transactionDate: messiveEdityEntity.updateTransactionDate && messiveEdityEntity.transactionDate != null ? toUtcDate(messiveEdityEntity.transactionDate) : null,
          
          updateAccountId: messiveEdityEntity.updateAccountId,
          updateDescription: messiveEdityEntity.updateDescription,
          updateStatus: messiveEdityEntity.updateStatus,
          updateAmount: messiveEdityEntity.updateAmount,
          updateCurrencyId: messiveEdityEntity.updateCurrencyId,
          updateBeneficiary: messiveEdityEntity.updateBeneficiary,
          updatePaymentMethodId: messiveEdityEntity.updatePaymentMethodId,
          updateCategoryId: messiveEdityEntity.updateCategoryId,
          updateTransactionDate: messiveEdityEntity.updateTransactionDate,
          updateTransactionType: messiveEdityEntity.updateTransactionType,
          updateDestinationAccountId: messiveEdityEntity.updateDestinationAccountId,
          TransactionIds: this.selectedRows

        };
        console.log("messiveEdityEntity", dtoMassiveEntity);
        const response = await systemApi.patch(`/Transactions/massive`, dtoMassiveEntity);
        
        
        
      } catch (err) {
        console.log("Error", err);
        messageStore.setMessages("Non è stato possibile caricare i dati", TYPES.error);
        this.error = err.response?.data?.message || 'Errore durante il caricamento dei dati';


      } finally {
        this.loading = false
      }
    },
    async newEntity() {
     
      this.createmode = true;
      this.edititem =
      {
        id:0,
        account: null,
        destinationAccount: null,
        transactionType: null,
        description: null,
        status: null,
        amount: null,
        currency: null,
        beneficiary: null,
        paymentMethod: null,
        category: null,
        tags: null,
        transactionDate: new Date() 


      }
      ;
 
    },
    async edit(transactionId) {
      const messageStore = useMessageStore();
      this.loading = true
      this.error = null
      try {
        const response = await systemApi.get(`/Transactions/${transactionId}`);
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
    async deleteTransaction(transactionId) {
      const messageStore = useMessageStore();
      this.loading = true
      this.error = null
      try {
        const response = await systemApi.delete(`/Transactions/${transactionId}`);

        


      } catch (err)
      {
        console.log("Error", err);
        messageStore.setMessages(err.response.data.Errors, TYPES.error);
        
        this.error = err.response?.data?.message || 'Errore durante il caricamento dei metodi di pagamento'

      } finally {
        this.loading = false
      }
    },

    async onPageChange(_page) {
      this.page = _page;
      await this.loadAlltransaction(this.currentFilter.targetAccountId,this.currentFilter.accountId, this.currentFilter.categoryId, this.currentFilter.fromDate, this.currentFilter.toDate, this.currentFilter.transactionType, this.currentFilter.flowDirection,
        this.currentFilter.status, this.currentFilter.note, this.sortBy, this.sortAscending);

    },
   
    async filterAlltransaction(_targetAccountId, _accountId, _categoryId, _fromDate, _toDate, _transactionType, _flowDirection, _status, _note, _sortBy, _sortAsc, _page) {
      console.log("filterAlltransaction", _page);
      this.page = _page != undefined ? _page: 1;
      this.pageSize = 20;
      await this.loadAlltransaction(_targetAccountId, _accountId, _categoryId, _fromDate, _toDate, _transactionType, _flowDirection, _status, _note, _sortBy, _sortAsc);
    },
    async loadAlltransaction(_targetAccountId, _accountId, _categoryId, _fromDate, _toDate, _transactionType, _flowDirection, _status,_note, _sortBy, _sortAsc) {

      //accountId & categoryId & fromDate & toDate & page=1 & pageSize=10 & sortBy=TransactionDate & asc=false & transactionType=2 & flowDirection & status & note
      const messageStore = useMessageStore();
      this.loading = true
      this.error = null
      try {
        this.currentFilter.targetAccountId = _targetAccountId;
        this.currentFilter.accountId = _accountId;
        this.currentFilter.categoryId = _categoryId;
        this.currentFilter.fromDate = _fromDate;
        this.currentFilter.toDate = _toDate;
        this.currentFilter.transactionType = _transactionType;
        this.currentFilter.flowDirection = _flowDirection;
        this.currentFilter.status = _status;
        this.currentFilter.note = _note;
        


        this.sortBy = _sortBy;
        this.sortAscending = _sortAsc;
        
        
        
        var strPage = this.page ? this.page : 1;
        var strPageSize = this.pageSize ? this.pageSize : 20;
        var strSortBy = this.sortBy ? this.sortBy : "";
        var strSortAscending = this.sortAscending ? this.sortAscending == 'asc' : true;
        const response = await systemApi.get(`/Transactions/find?note=${filterToString(this.currentFilter.note)}&status=${filterToString(this.currentFilter.status)}&flowDirection=${filterToString(this.currentFilter.flowDirection)}&transactionType=${filterToString(this.currentFilter.transactionType)}&targetAccountId=${filterToString(this.currentFilter.targetAccountId)}&accountId=${filterToString(this.currentFilter.accountId)}&categoryId=${filterToString(this.currentFilter.categoryId)}&fromDate=${filterToDateString(this.currentFilter.fromDate)}&toDate=${filterToDateString(this.currentFilter.toDate)}&page=${strPage}&pageSize=${strPageSize}&sortBy=${strSortBy}&asc=${strSortAscending}`);
        console.log("loadAlltransaction", this.transactions);
        if (this.transactions != null) {
          const currentIds = this.transactions.map(t => t.id);                       // tutti gli id della pagina corrente
          const currentSelectedIds = this.transactions.filter(t => t.selected).map(t => t.id); // id selezionati nella pagina corrente

          // 1) Aggiungi i selezionati correnti in modo idempotente
          const selectedSet = new Set(this.selectedRows); // set con le selezioni globali esistenti
          for (const id of currentSelectedIds) {
            selectedSet.add(id);
          }

          // 2) Rimuovi solo gli id visibili nella pagina corrente che NON sono selezionati
          //    (attenzione: NON rimuoviamo id che appartengono ad altre pagine)
          for (const id of currentIds) {
            if (!currentSelectedIds.includes(id)) {
              selectedSet.delete(id);
            }
          }

          // 3) Assegna di nuovo in forma di array (manteniamo ordine eventualmente)
          this.selectedRows = Array.from(selectedSet);
        }
        else {
          this.selectedRows = [];
        }

        console.log("loadAlltransaction this.selectedRows", this.selectedRows);
        this.transactions = response.data.items.map(t => ({
          ...t,
          selected: this.selectedRows.includes(t.id)
        }));
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

    
    ,async loadFilterLookups() {

      const messageStore = useMessageStore();
       
      this.loading = true
      this.error = null
      try {
        const response = await accountApi.get('/Accounts');

        this.filterLookups.accounts = response.data;

        const categoryResponse = await accountApi.get(`/Categories?q=`);
        this.filterLookups.categories = categoryResponse.data;

        const transactionStatusResponse = await accountApi.get(`/Transactions/status`);
        this.filterLookups.statuses = transactionStatusResponse.data;

        const transactionTypesResponse = await accountApi.get(`/Transactions/types`);
        this.filterLookups.transactionTypes = transactionTypesResponse.data;

      } catch (err) {
        console.log("Error", err);
        messageStore.setMessages("Non è stato possibile caricare gli accounts", TYPES.error);
        this.error = err.response?.data?.message || 'Errore durante il caricamento degli accounts'

      } finally {
        this.loading = false
      }
    },
    async findBeneficiaries(q) {
      const messageStore = useMessageStore();
      this.loading = true
      this.error = null
      try {
        const response = await systemApi.get(`Beneficiaries/lookups?q=${q}&add=true`);
        return response.data;
        //return response.data.map(item => ({
        //  id: item.id,
        //  description: item.name
        //}));
      } catch (err) {
        console.log("Error", err);
        messageStore.setMessages("Non è stato possibile caricare i dati", TYPES.error);
        this.error = err.response?.data?.message || 'Errore durante il caricamento dei dati';


      } finally {
        this.loading = false
      }
    },
    async findCategories(q) {
      const messageStore = useMessageStore();
      this.loading = true
      this.error = null
      try {
        const response = await systemApi.get(`Categories/lookups?q=${q}`);
        return response.data;
 
      } catch (err) {
        console.log("Error", err);
        messageStore.setMessages("Non è stato possibile caricare i dati", TYPES.error);
        this.error = err.response?.data?.message || 'Errore durante il caricamento dei dati';


      } finally {
        this.loading = false
      }
    },
    async getRate(fromCurrency, toCurrency, targetdate) {
     
        const messageStore = useMessageStore();
        this.loading = true
        this.error = null
      try {
        console.log("targetDate", targetdate);
        const response = await systemApi.get(`currencies/exchange/find?targetDate=${toUtcDate(targetdate)}&toCurrencyId=${toCurrency.id}&fromCurrencyId=${fromCurrency.id}`);

          console.log("rate", response.data);
          return response.data;
        } catch (err) {
          console.log("Error", err);
          messageStore.setMessages("Non è stato possibile caricare i dati", TYPES.error);
          this.error = err.response?.data?.message || 'Errore durante il caricamento dei dati';


        } finally {
          this.loading = false
        }

      
    },
    async getPaymentMethods(accountid) {
     
        const messageStore = useMessageStore();
        this.loading = true
        this.error = null
      try {
         
        const response = await systemApi.get(`/Accounts/${accountid}/paymentmethods`);

        console.log("paymentmethods", response.data);
          return response.data;
        } catch (err) {
          console.log("Error", err);
          messageStore.setMessages("Non è stato possibile caricare i dati", TYPES.error);
          this.error = err.response?.data?.message || 'Errore durante il caricamento dei dati';


        } finally {
          this.loading = false
        }

      
    }

  },
  computed: {
    
  }
  ,
})
