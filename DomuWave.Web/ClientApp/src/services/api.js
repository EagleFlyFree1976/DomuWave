import axios from 'axios'
const STORAGE_KEY = 'tenantId'
const api = axios.create({
  baseURL: import.meta.env.VITE_API_DOMUAPP_URL + "/api",
  headers: { 'Content-Type': 'application/json' }
})

// Inject auth token from localStorage
api.interceptors.request.use(config => {
  const token = localStorage.getItem('domuwave_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
    config.headers['X-Auth-Token'] = token;
  }
  const tenantId = localStorage.getItem(STORAGE_KEY);
  if (tenantId) config.headers['X-Tenant-Id'] = tenantId;
  return config
})

/**
 * Estrae un messaggio leggibile dall'errore axios.
 * Priorità: messaggio del backend → messaggio HTTP → fallback generico.
 */
function extractErrorMessage(err) {
  const data = err.response?.data
  if (typeof data === 'string' && data.length > 0) return data
  // formato { Errors: ["..."] } — ValidatorException, NotFoundException, ecc.
  const errs = data?.Errors ?? data?.errors
  if (Array.isArray(errs) && errs.length > 0) return errs.join('\n')
  if (data?.message) return data.message
  if (data?.title) return data.title
  if (data?.detail) return data.detail
  if (err.message) return err.message
  return 'Si è verificato un errore imprevisto'
}

api.interceptors.response.use(
  res => res,
  err => {
    const status  = err.response?.status
    const message = extractErrorMessage(err)

    // Errori di rete / timeout (nessuna risposta dal server)
    if (!err.response) {
      window.dispatchEvent(new CustomEvent('api:error', {
        detail: { status: null, message: 'Impossibile raggiungere il server. Controlla la connessione.' }
      }))
      return Promise.reject(err)
    }

    // 401 — non autorizzato
    if (status === 401) {
      window.dispatchEvent(new CustomEvent('api:error', {
        detail: { status: 401, message: 'Operazione non autorizzata' }
      }))
      return Promise.reject(err)
    }

    // 403 — accesso vietato
    if (status === 403) {
      window.dispatchEvent(new CustomEvent('api:error', {
        detail: { status: 403, message: 'Non hai i permessi per eseguire questa operazione' }
      }))
      return Promise.reject(err)
    }

    // 404 — risorsa non trovata (spesso non va mostrato come toast, si lascia gestire al chiamante)
    // Se vuoi mostrarlo comunque, decommenta:
    // if (status === 404) {
    //   window.dispatchEvent(new CustomEvent('api:error', {
    //     detail: { status: 404, message: message || 'Risorsa non trovata' }
    //   }))
    //   return Promise.reject(err)
    // }

    // 422 / 400 — errori di validazione o bad request
    if (status === 422 || status === 400) {
      window.dispatchEvent(new CustomEvent('api:error', {
        detail: { status, message }
      }))
      return Promise.reject(err)
    }

    // 5xx — errori server
    if (status >= 500) {
      window.dispatchEvent(new CustomEvent('api:error', {
        detail: { status, message: 'Errore del server. Riprova più tardi.' }
      }))
      return Promise.reject(err)
    }

    // Tutti gli altri errori HTTP
    window.dispatchEvent(new CustomEvent('api:error', {
      detail: { status, message }
    }))

    return Promise.reject(err)
  }
)

// ─── Dashboard ────────────────────────────────────────────────
export const dashboardApi = {
  getSummary: () => api.get('/dashboard/summary'),
}

// ─── Condomini ────────────────────────────────────────────────
export const condominiumApi = {
  getAll:         ()         => api.get('/condominiums'),
  getById:        (id)       => api.get(`/condominiums/${id}`),
  getActive:      (tenantId) => api.get(`/condominiums/active`, { params: { tenantId } }),
  create:         (data)     => api.post('/condominiums', data),
  update:         (id, data) => api.put(`/condominiums/${id}`, data),
  delete:         (id)       => api.delete(`/condominiums/${id}`),
  getSetupStatus: (id)       => api.get(`/condominiums/${id}/setup-status`),
}

// ─── Unità immobiliari ────────────────────────────────────────
export const unitApi = {
  getById:            (id)            => api.get(`/real-estate-units/${id}`),
  getByCondominium:   (condId)        => api.get(`/real-estate-units/by-condominium/${condId}`),
  getByType:          (condId, type)  => api.get(`/real-estate-units/by-condominium/${condId}/type/${type}`),
  create:             (data)          => api.post('/real-estate-units', data),
  update:             (id, data)      => api.put(`/real-estate-units/${id}`, data),
  delete:             (id)            => api.delete(`/real-estate-units/${id}`),
  getOpeningBalance:  (unitId, fiscalYearId) => api.get(`/real-estate-units/${unitId}/opening-balance`, { params: { fiscalYearId } }),
  setOpeningBalance:  (unitId, data)         => api.put(`/real-estate-units/${unitId}/opening-balance`, data),
}

// ─── Proprietari unità ────────────────────────────────────────
export const unitOwnerApi = {
  search:     (q)           => api.get(`/unit-owners/search`, { params: { q } }),
  getByUnit:  (unitId)      => api.get(`/unit-owners/by-unit/${unitId}`),
  getByUser:  (userId)      => api.get(`/unit-owners/by-user/${userId}`),
  create:     (data)        => api.post('/unit-owners', data),
  update:     (id, data)    => api.put(`/unit-owners/${id}`, data),
  delete:     (id)          => api.delete(`/unit-owners/${id}`),
}

// ─── Inquilini unità ──────────────────────────────────────────
export const unitTenantApi = {
  search:     (q)           => api.get(`/unit-tenants/search`, { params: { q } }),
  getByUnit:  (unitId)      => api.get(`/unit-tenants/by-unit/${unitId}`),
  create:     (data)        => api.post('/unit-tenants', data),
  update:     (id, data)    => api.put(`/unit-tenants/${id}`, data),
  delete:     (id)          => api.delete(`/unit-tenants/${id}`),
}

// ─── Esercizi fiscali ─────────────────────────────────────────
export const fiscalYearApi = {
  getByCondominium:   (condId)        => api.get(`/fiscal-years/by-condominium/${condId}`),
  getActive:          (condId)        => api.get(`/fiscal-years/active/${condId}`),
  getById:            (id)            => api.get(`/fiscal-years/${id}`),
  create:             (data)          => api.post('/fiscal-years', data),
  update:             (id, data)      => api.put(`/fiscal-years/${id}`, data),
  open:               (id)            => api.post(`/fiscal-years/${id}/open`),
  startClosing:       (id, notes)     => api.post(`/fiscal-years/${id}/start-closing`, { notes }),
  close:              (id, notes)     => api.post(`/fiscal-years/${id}/close`, { notes }),
  lock:               (id, notes)     => api.post(`/fiscal-years/${id}/lock`, { notes }),
}

// ─── Saldi contabili (AccountBalance) ─────────────────────────
export const accountBalanceApi = {
  getByFiscalYear: (fiscalYearId)      => api.get(`/account-balances/by-fiscal-year/${fiscalYearId}`),
  updateOpening:   (id, data)          => api.put(`/account-balances/${id}`, data),
}

// ─── Piano dei conti ──────────────────────────────────────────
export const chartOfAccountsApi = {
  getById:             (id)       => api.get(`/chart-of-accounts/${id}`),
  getByCondominium:    (condId)   => api.get(`/chart-of-accounts/by-condominium/${condId}`),
  create:              (data)     => api.post('/chart-of-accounts', data),
  update:              (id, data) => api.put(`/chart-of-accounts/${id}`, data),
  // Restituisce { requiresReplacement, hasDraftUsages, hasLockedUsages }
  checkDelete:         (id)       => api.get(`/chart-of-accounts/${id}/check-delete`),
  // replacementAccountId obbligatorio se il conto è usato in budget in bozza
  delete:              (id, replacementAccountId) => api.delete(
    `/chart-of-accounts/${id}`,
    { params: replacementAccountId ? { replacementAccountId } : {} }
  ),
  copyFromCondominium: (data)     => api.post('/chart-of-accounts/copy-from-condominium', data),
  applyTemplate:       (condId)   => api.post(`/chart-of-accounts/apply-template/${condId}`),
}

// ─── Template Piano dei conti ──────────────────────────────────
export const chartOfAccountsTemplateApi = {
  getAll:       ()                           => api.get('/chart-of-accounts-templates'),
  create:       (data)                       => api.post('/chart-of-accounts-templates', data),
  update:       (id, data)                   => api.put(`/chart-of-accounts-templates/${id}`, data),
  delete:       (id)                         => api.delete(`/chart-of-accounts-templates/${id}`),
  getItems:     (templateId)                 => api.get(`/chart-of-accounts-templates/${templateId}/items`),
  createItem:   (templateId, data)           => api.post(`/chart-of-accounts-templates/${templateId}/items`, data),
  updateItem:   (templateId, itemId, data)   => api.put(`/chart-of-accounts-templates/${templateId}/items/${itemId}`, data),
  deleteItem:   (templateId, itemId)         => api.delete(`/chart-of-accounts-templates/${templateId}/items/${itemId}`),
  apply:        (templateId, condominiumId)  => api.post(`/chart-of-accounts-templates/${templateId}/apply/${condominiumId}`),
  setDefault:   (id)                        => api.patch(`/chart-of-accounts-templates/${id}/set-default`),
}

// ─── Categorie piano dei conti ────────────────────────────────
export const chartOfAccountsCategoryApi = {
  getAll:           ()              => api.get('/chart-of-accounts-categories'),
  create:           (data)          => api.post('/chart-of-accounts-categories', data),
  update:           (id, data)      => api.put(`/chart-of-accounts-categories/${id}`, data),
  delete:           (id)            => api.delete(`/chart-of-accounts-categories/${id}`),
  importFromTemplate: (templateIds) => api.post('/chart-of-accounts-categories/import-from-template', templateIds),
}

// ─── Template categorie piano dei conti (SuperAdmin) ─────────
export const chartOfAccountsCategoryTemplateApi = {
  getAll:  ()           => api.get('/chart-of-accounts-category-templates'),
  create:  (data)       => api.post('/chart-of-accounts-category-templates', data),
  update:  (id, data)   => api.put(`/chart-of-accounts-category-templates/${id}`, data),
  delete:  (id)         => api.delete(`/chart-of-accounts-category-templates/${id}`),
}

// ─── Budget ───────────────────────────────────────────────────
export const budgetApi = {
  getByCondominium:   (condId)  => api.get(`/budgets/by-condominium/${condId}`),
  getByFiscalYear:    (fyId)    => api.get(`/budgets/by-fiscal-year/${fyId}`),
  getById:            (id)      => api.get(`/budgets/${id}`),
  create:             (data)    => api.post('/budgets', data),
  update:             (id, data)=> api.put(`/budgets/${id}`, data),
  approve:              (id, opts) => api.post(`/budgets/${id}/approve`, opts ?? {}),
  reopen:               (id)      => api.post(`/budgets/${id}/reopen`),
  close:                (id)      => api.post(`/budgets/${id}/close`),
  generateInstallments: (id, opts) => api.post(`/budgets/${id}/generate-installments`, opts ?? {}),
  recalculateItems:     (id)      => api.post(`/budgets/${id}/recalculate-items`),
  fixOrphanItems:       (id)      => api.post(`/budgets/${id}/fix-orphan-items`),
  delete:               (id)      => api.delete(`/budgets/${id}`),
}

// ─── Voci di budget ───────────────────────────────────────────
export const budgetItemApi = {
  getByBudget:        (budgetId)        => api.get(`/budget-items/by-budget/${budgetId}`),
  create:             (data)            => api.post('/budget-items', data),
  update:             (id, data)        => api.put(`/budget-items/${id}`, data),
  delete:             (id)              => api.delete(`/budget-items/${id}`),
}

// ─── Spese ────────────────────────────────────────────────────
// Backend: api/expenses  (ExpensesController - [Route("api/[controller]")])
export const expenseApi = {
  getAll:             ()                          => api.get('/expenses'),
  getById:            (id)                        => api.get(`/expenses/${id}`),
  getByCondominium:   (condId)                    => api.get(`/expenses/by-condominium/${condId}`),
  getByDateRange:     (condId, from, to)          => api.get(`/expenses/by-condominium/${condId}/date-range`, { params: { from, to } }),
  getByType:          (condId, typeId)            => api.get(`/expenses/by-condominium/${condId}/type/${typeId}`),
  getUnpaid:          (condId)                    => api.get(`/expenses/by-condominium/${condId}/unpaid`),
  getTotal:           (condId, from, to)          => api.get(`/expenses/by-condominium/${condId}/total`, { params: { from, to } }),
  create:             (data)                      => api.post('/expenses', data),
  update:             (id, data)                  => api.put(`/expenses/${id}`, data),
  markAsPaid:         (id, paymentDate, method)   => api.patch(`/expenses/${id}/pay`, { paymentDate, paymentMethod: method }),
  delete:             (id)                        => api.delete(`/expenses/${id}`),
}

// ─── Rate ─────────────────────────────────────────────────────
// Backend: api/condominium-installments  (CondominiumInstallmentsController)
export const installmentApi = {
  getByCondominium:   (condId)                      => api.get(`/condominium-installments/by-condominium/${condId}`),
  getByBudget:        (budgetId)                     => api.get(`/condominium-installments/by-budget/${budgetId}`),
  getByFiscalYear:    (condId, fiscalYearId)         => api.get(`/condominium-installments/by-fiscal-year/${fiscalYearId}`, { params: { condominiumId: condId } }),
  getByYearAndNumber: (condId, year, number)         => api.get(`/condominium-installments/by-condominium/${condId}/year/${year}/number/${number}`),
  getOpen:            (condId)                       => api.get(`/condominium-installments/by-condominium/${condId}/open`),
  getOverdue:         (condId)                       => api.get(`/condominium-installments/by-condominium/${condId}/overdue`),
  generate:           (condId, fiscalYearId, budgetId) => api.post(`/condominium-installments/by-condominium/${condId}/generate`, { fiscalYearId, budgetId }),
  create:             (data)                         => api.post('/condominium-installments', data),
  update:             (id, data)                     => api.put(`/condominium-installments/${id}`, data),
  delete:             (id)                           => api.delete(`/condominium-installments/${id}`),
}

// ─── Quote ────────────────────────────────────────────────────
// Backend: api/condominium-fees  (CondominiumFeesController)
export const feeApi = {
  getByInstallment:   (instId)        => api.get(`/condominium-fees/by-installment/${instId}`),
  getByUnit:          (unitId)        => api.get(`/condominium-fees/by-unit/${unitId}`),
  getByUser:          (userId)        => api.get(`/condominium-fees/by-user/${userId}`),
  getUnpaid:          (condId)        => api.get(`/condominium-fees/by-condominium/${condId}/unpaid`),
  getOverdue:         (condId)        => api.get(`/condominium-fees/by-condominium/${condId}/overdue`),
  getTotalDue:        (userId)        => api.get(`/condominium-fees/total-due/${userId}`),
  getBalance:         (userId)        => api.get(`/condominium-fees/balance/${userId}`),
  update:             (id, data)      => api.put(`/condominium-fees/${id}`, data),
  recordPayment:      (feeId, amount, paymentDate, paymentMethod) => api.patch(`/condominium-fees/${feeId}/pay`, { amount, paymentDate, paymentMethod }),
  create:             (data)          => api.post('/condominium-fees', data),
  delete:             (id)            => api.delete(`/condominium-fees/${id}`),
}

// ─── Tabelle Millesimali ───────────────────────────────────────
export const millesimalTableApi = {
  getByCondominium:   (condId)        => api.get(`/millesimal-tables/by-condominium/${condId}`),
  getActive:          (condId)        => api.get(`/millesimal-tables/by-condominium/${condId}/active`),
  getById:            (id)            => api.get(`/millesimal-tables/${id}`),
  create:             (data)          => api.post('/millesimal-tables', data),
  update:             (id, data)      => api.put(`/millesimal-tables/${id}`, data),
  setEnabled:         (id, isEnabled) => api.patch(`/millesimal-tables/${id}/enabled`, isEnabled),
  delete:             (id)            => api.delete(`/millesimal-tables/${id}`),
}

// ─── Voci Millesimali (UnitMillesimal) ────────────────────────
export const unitMillesimalApi = {
  getByTable:  (tableId)      => api.get(`/unit-millesimals/by-table/${tableId}`),
  create:      (data)         => api.post('/unit-millesimals', data),
  update:      (id, data)     => api.put(`/unit-millesimals/${id}`, data),
  delete:      (id)           => api.delete(`/unit-millesimals/${id}`),
}

// ─── Fornitori ────────────────────────────────────────────────
export const supplierApi = {
  getAll:         ()              => api.get('/suppliers'),
  getById:        (id)            => api.get(`/suppliers/${id}`),
  search:         (tenantId, q)   => api.get(`/suppliers/search`, { params: { tenantId, q } }),
  getByType:      (tenantId, t)   => api.get(`/suppliers/type/${t}`, { params: { tenantId } }),
  create:         (data)          => api.post('/suppliers', data),
  update:         (id, data)      => api.put(`/suppliers/${id}`, data),
  delete:         (id)            => api.delete(`/suppliers/${id}`),
  // Contratti
  getContracts:              (suppId)        => api.get(`/supplier-contracts/supplier/${suppId}`),
  getContractsByCondominium: (condId)        => api.get(`/supplier-contracts/condominium/${condId}`),
  getActiveContracts:        (condId)        => api.get(`/supplier-contracts/condominium/${condId}/active`),
  getExpiringContracts:(condId, days) => api.get(`/supplier-contracts/condominium/${condId}/expiring`, { params: { days } }),
  createContract:     (data)          => api.post('/supplier-contracts', data),
  updateContract:     (id, data)      => api.put(`/supplier-contracts/${id}`, data),
  deleteContract:     (id)            => api.delete(`/supplier-contracts/${id}`),
}

// ─── Lavori Straordinari ──────────────────────────────────────
export const extraordinaryWorkApi = {
  getByCondominium: (condId)            => api.get(`/extraordinary-works/condominium/${condId}`),
  getByStatus:      (condId, status)    => api.get(`/extraordinary-works/condominium/${condId}/status/${status}`),
  getById:          (id)                => api.get(`/extraordinary-works/${id}`),
  create:           (data)              => api.post('/extraordinary-works', data),
  update:           (id, data)          => api.put(`/extraordinary-works/${id}`, data),
  delete:           (id)                => api.delete(`/extraordinary-works/${id}`),
  // Preventivi
  getQuotes:        (workId)            => api.get(`/extraordinary-works/${workId}/quotes`),
  createQuote:      (workId, data)      => api.post(`/extraordinary-works/${workId}/quotes`, data),
  updateQuote:      (workId, qId, data) => api.put(`/extraordinary-works/${workId}/quotes/${qId}`, data),
  deleteQuote:      (workId, qId)       => api.delete(`/extraordinary-works/${workId}/quotes/${qId}`),
  approveQuote:     (workId, qId)       => api.post(`/extraordinary-works/${workId}/quotes/${qId}/approve`),
  rejectQuote:      (workId, qId)       => api.post(`/extraordinary-works/${workId}/quotes/${qId}/reject`),
  // Documenti preventivo
  addDocument:      (workId, qId, data) => api.post(`/extraordinary-works/${workId}/quotes/${qId}/documents`, data),
  deleteDocument:   (workId, qId, docId)=> api.delete(`/extraordinary-works/${workId}/quotes/${qId}/documents/${docId}`),
}

// ─── Manutenzioni ─────────────────────────────────────────────
export const maintenanceApi = {
  getByCondominium:  (condId)         => api.get(`/maintenance/condominium/${condId}`),
  getOpen:           (condId)         => api.get(`/maintenance/condominium/${condId}/open`),
  getByStatus:       (condId, status) => api.get(`/maintenance/condominium/${condId}/status/${status}`),
  getByPriority:     (condId, prio)   => api.get(`/maintenance/condominium/${condId}/priority/${prio}`),
  getById:           (id)             => api.get(`/maintenance/${id}`),
  create:            (data)           => api.post('/maintenance', data),
  update:            (id, data)       => api.put(`/maintenance/${id}`, data),
  delete:            (id)             => api.delete(`/maintenance/${id}`),
}

// ─── Documenti ────────────────────────────────────────────────
export const documentApi = {
  getAll:             ()              => api.get('/documents'),
  getById:            (id)            => api.get(`/documents/${id}`),
  getByCondominium:   (condId)        => api.get(`/documents/condominium/${condId}`),
  getByCategory:      (condId, cat)   => api.get(`/documents/condominium/${condId}/category/${cat}`),
  getVisibleToOwners: (condId)        => api.get(`/documents/condominium/${condId}/visible`),
  search:             (condId, q)     => api.get(`/documents/condominium/${condId}/search`, { params: { q } }),
  getRecent:          (condId, n)     => api.get(`/documents/condominium/${condId}/recent`, { params: { n } }),
  create:             (data)          => api.post('/documents', data),
  update:             (id, data)      => api.put(`/documents/${id}`, data),
  delete:             (id)            => api.delete(`/documents/${id}`),
}

// ─── Comunicazioni ────────────────────────────────────────────
export const communicationApi = {
  getAll:             ()              => api.get('/communications'),
  getById:            (id)            => api.get(`/communications/${id}`),
  getByCondominium:   (condId)        => api.get(`/communications/condominium/${condId}`),
  getVisible:         (condId)        => api.get(`/communications/condominium/${condId}/visible`),
  getByType:          (condId, type)  => api.get(`/communications/condominium/${condId}/type/${type}`),
  getUnread:          (condId, userId)=> api.get(`/communications/condominium/${condId}/unread/${userId}`),
  publish:            (id)            => api.post(`/communications/${id}/publish`),
  create:             (data)          => api.post('/communications', data),
  update:             (id, data)      => api.put(`/communications/${id}`, data),
  delete:             (id)            => api.delete(`/communications/${id}`),
}

// ─── Dynamic File Repository ──────────────────────────────────
export const dynamicFileApi = {
  /**
   * Lista file collegati a un'entita'.
   * @param {string} entityName       - Nome semplice (es. "Expense")
   * @param {number} entityId         - Id dell'entita'
   * @param {string} [entityFullName] - Nome completo opzionale per ricerca esatta
   */
  getByEntity:  (entityName, entityId, entityFullName) =>
    api.get('/dynamic-files/by-entity', { params: { entityName, entityId, entityFullName } }),

  /** Metadati del file (senza contenuto base64) */
  getById:      (id)       => api.get(`/dynamic-files/${id}`),

  /** Metadati + contenuto base64 (per download) */
  download:     (id)       => api.get(`/dynamic-files/${id}/download`),

  /** Carica un nuovo file. dto: { entityFullName, entityName, entityId, fileName, contentType, base64Data, description?, tags? } */
  upload:       (data)     => api.post('/dynamic-files', data),

  /** Aggiorna descrizione e tag */
  update:       (id, data) => api.put(`/dynamic-files/${id}`, data),

  delete:       (id)       => api.delete(`/dynamic-files/${id}`),
}

export default api
