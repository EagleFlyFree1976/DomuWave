// api/aiApi.js — chiamate al modulo AI Assistant
import api from '@/services/api'

/**
 * Esegue una query in linguaggio naturale sui dati condominiali.
 * @param {{ question: string, condominiumId?: number|null, fiscalYear?: number|null, history?: Array }} payload
 * @returns {Promise<{ answer: string, toolUsed: string, success: boolean, errorMessage: string }>}
 */
export const queryAi = (payload) =>
  api.post('/ai/query', payload).then(r => r.data)
