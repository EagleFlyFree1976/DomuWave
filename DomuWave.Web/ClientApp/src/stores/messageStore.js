import { defineStore } from 'pinia'
import { processApiResponse, handleApiError } from '@/code/utils/apiUtils'
import { MESSAGES, TYPES } from '@/code/messages'
// Tipo per ogni errore
class AppMessage {
  constructor(message, type) {
      this.id = crypto.randomUUID(),
      this.visible = true,
      this.message = message,
      this.timestamp = new Date(),
      this.type = type;
  }
}

export const useMessageStore = defineStore('messageStore', {
  
  state: () => ({
    _messages: [],
   
    loading: false,
  }),
  getters: {
    visibleMessages(state) {
      return state._messages.filter(msg => msg.visible).sort((a, b) => b.timestamp - a.timestamp);
    }
  },
  actions: {
    setMessages(messages, type) {
      if (typeof messages === 'string') {
        this._messages.push(new AppMessage(messages, type));
      } else if (Array.isArray(messages)) {
        this._messages = messages.map(msg => new AppMessage(msg, type));
      }
    },

    addMessage(message, type) {
      this._messages.push(new AppMessage(message, type))
    },
    hideMessage(id) {
      const message = this._messages.find(msg => msg.id === id)
      if (message) {
        message.visible = false
      }
    },
    removeMessage(id) {
      this._messages = this._messages.filter(err => err.id !== id)
    },
    showSuccess(msg) {
      if (msg == null)
        msg = MESSAGES.SUCCESS.SAVE;

      this.addMessage(msg, TYPES.success);
    },
    showError(msg) {
      if (msg == null)
        msg = MESSAGES.ERROR.SAVE;

      this.addMessage(msg, TYPES.error);
    },
    clear() {
      console.log("mstore cancello i messaggi: pre:", this._messages);
      this._messages = [];
      console.log("mstore cancello i messaggi: post:", this._messages);
    },

    generateId(){
      // Se vuoi più robustezza: usa crypto.randomUUID()
      //return Date.now().toString(36) + Math.random().toString(36).substr(2, 5)
      return crypto.randomUUID();
    }
  },
})
