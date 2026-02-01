import { defineStore } from 'pinia'
import accountApi from '@/code/accountApi'
import axios from 'axios'
//import { processApiResponse, handleApiError } from '@/code/utils/apiUtils'
import { useMessageStore } from './messageStore'
import { useDomuWaveStore } from './domuWaveStore'
import { MESSAGES, TYPES } from '@/code/messages'
import router from '@/router'
export const useCategoryStore = defineStore('categoryStore', {
  state: () => ({
    categories: null,
    createmode: false,
    q:null,
    loading: false,
    error: null,
    edititem: null,
    createitem:null
  }),
  actions: {

 
    async createEntity(account) {
      await this.updateEntity(account);
    },
    async updateEntity(category) {
      const messageStore = useMessageStore();
      const DomuWaveStore = useDomuWaveStore();
      this.loading = true
      this.error = null


      try {
        let apiAction = "/Categories";
        let response = null;
        if (category.id != 0) {
          apiAction = `${apiAction}/${category.id}`
          response = await accountApi.put(apiAction, {
            name: category.name,
            description: category.description,
            parentCategoryId: category.parent != null ? category.parent.id : null

            });
        }
        else {
          response = await accountApi.post(apiAction, {
           
            name: category.name,
            description: category.description,
            parentCategoryId: category.parent != null ? category.parent.id : null

          });

          this.edititem.id = response.headers['x-key']; 
          category.id = response.headers['x-key']; 
          this.createmode = false;
        }
        
         

        this.edititem = { ...category }
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

    
    async newEntity(parentCategoryId) {
     
      this.createmode = true;
      this.edititem =
      {

        "name": "[New]",
        "description": "",
        "parent": { id: parentCategoryId },
        "id": 0
      }
      ;
 
    },
    async edit(categoryId) {
      const messageStore = useMessageStore();
      this.loading = true
      this.error = null
      try {
        const response = await accountApi.get(`/Categories/${categoryId}`);

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
    async deleteCategory(categoryId) {
      const messageStore = useMessageStore();
      this.loading = true
      this.error = null
      try {
        const response = await accountApi.delete(`/Categories/${categoryId}`);

        await this.loadAllCategories();


      } catch (err) {
        console.log("Error", err);
        messageStore.setMessages(err.response.data.Errors, TYPES.error);
        
        this.error = err.response?.data?.message || 'Errore durante il caricamento delle categorie'

      } finally {
        this.loading = false
      }
    },

    
    async loadAllCategories() {
      const messageStore = useMessageStore();
      this.loading = true
      this.error = null
      try {
        var _q = "";
        if (this.q != null) {
          _q = this.q;
        }
        

        const response = await accountApi.get(`/Categories?q=${_q}`);

        this.categories = response.data
        
        
      } catch (err) {
        console.log("Error", err);
        messageStore.setMessages("Non è stato possibile caricare le categorie", TYPES.error);
        this.error = err.response?.data?.message || 'Errore durante il caricamento delle categorie' 
        
      } finally {
        this.loading = false
      }
    }

    
    
  },
  computed: {
    
  }
  ,
})
