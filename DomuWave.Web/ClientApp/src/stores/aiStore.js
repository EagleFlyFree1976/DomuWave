// stores/aiStore.js — stato del pannello AI Assistant
import { defineStore } from 'pinia'
import { queryAi } from '@/api/aiApi'

export const useAiStore = defineStore('ai', {
  state: () => ({
    messages: [],   // { role: 'user' | 'assistant', content: string }
    loading: false,
    error: null,
    isOpen: false,
  }),
  actions: {
    async sendQuestion(question, condominiumId = null) {
      const text = (question ?? '').trim()
      if (!text || this.loading) return

      this.error = null
      this.messages.push({ role: 'user', content: text })
      this.loading = true

      try {
        // Storico = tutti i messaggi tranne l'ultimo (la domanda appena inviata)
        const history = this.messages.slice(0, -1)
        const res = await queryAi({ question: text, condominiumId, history })

        if (res?.success === false) {
          this.error = res.errorMessage || 'Errore nella risposta AI'
          this.messages.push({ role: 'assistant', content: this.error })
        } else {
          this.messages.push({ role: 'assistant', content: res.answer })
        }
      } catch (e) {
        this.error = 'Errore nella risposta AI'
        this.messages.push({ role: 'assistant', content: this.error })
      } finally {
        this.loading = false
      }
    },
    clearChat() {
      this.messages = []
      this.error = null
    },
    open() { this.isOpen = true },
    close() { this.isOpen = false },
    toggle() { this.isOpen = !this.isOpen },
  },
})
