import axios from 'axios'

const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' }
})

// Inject auth token from localStorage
api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  const tenantId = localStorage.getItem('tenantId')
  if (tenantId) config.headers['X-Tenant-Id'] = tenantId
  return config
})

api.interceptors.response.use(
  res => res,
  err => {
    if (err.response?.status === 401) {
      localStorage.removeItem('token')
      window.location.href = '/login'
    }
    return Promise.reject(err)
  }
)

// ─── Condomini ────────────────────────────────────────────────
export const condominiumApi = {
  getAll:       ()         => api.get('/condominiums'),
  getById:      (id)       => api.get(`/condominiums/${id}`),
  getActive:    (tenantId) => api.get(`/condominiums/active`, { params: { tenantId } }),
  create:       (data)     => api.post('/condominiums', data),
  update:       (id, data) => api.put(`/condominiums/${id}`, data),
  delete:       (id)       => api.delete(`/condominiums/${id}`),
}

// ─── Unità immobiliari ────────────────────────────────────────
export const unitApi = {
  getAll:             ()              => api.get('/realestate-units'),
  getById:            (id)            => api.get(`/realestate-units/${id}`),
  getByCondominium:   (condId)        => api.get(`/realestate-units/condominium/${condId}`),
  getByType:          (condId, type)  => api.get(`/realestate-units/condominium/${condId}/type/${type}`),
  create:             (data)          => api.post('/realestate-units', data),
  update:             (id, data)      => api.put(`/realestate-units/${id}`, data),
  delete:             (id)            => api.delete(`/realestate-units/${id}`),
}

// ─── Budget ───────────────────────────────────────────────────
export const budgetApi = {
  getAll:             ()              => api.get('/budgets'),
  getById:            (id)            => api.get(`/budgets/${id}`),
  getByCondominium:   (condId)        => api.get(`/budgets/condominium/${condId}`),
  getByYear:          (condId, year)  => api.get(`/budgets/condominium/${condId}/year/${year}`),
  getCurrent:         (condId)        => api.get(`/budgets/condominium/${condId}/current`),
  create:             (data)          => api.post('/budgets', data),
  update:             (id, data)      => api.put(`/budgets/${id}`, data),
  approve:            (id)            => api.post(`/budgets/${id}/approve`),
  close:              (id)            => api.post(`/budgets/${id}/close`),
  delete:             (id)            => api.delete(`/budgets/${id}`),
}

// ─── Spese ────────────────────────────────────────────────────
export const expenseApi = {
  getAll:             ()                          => api.get('/expenses'),
  getById:            (id)                        => api.get(`/expenses/${id}`),
  getByCondominium:   (condId)                    => api.get(`/expenses/condominium/${condId}`),
  getByDateRange:     (condId, from, to)          => api.get(`/expenses/condominium/${condId}/range`, { params: { from, to } }),
  getUnpaid:          (condId)                    => api.get(`/expenses/condominium/${condId}/unpaid`),
  getTotal:           (condId, from, to)          => api.get(`/expenses/condominium/${condId}/total`, { params: { from, to } }),
  create:             (data)                      => api.post('/expenses', data),
  update:             (id, data)                  => api.put(`/expenses/${id}`, data),
  markAsPaid:         (id, paymentDate, method)   => api.post(`/expenses/${id}/pay`, { paymentDate, method }),
  delete:             (id)                        => api.delete(`/expenses/${id}`),
}

// ─── Rate ─────────────────────────────────────────────────────
export const installmentApi = {
  getAll:             ()              => api.get('/installments'),
  getById:            (id)            => api.get(`/installments/${id}`),
  getByCondominium:   (condId)        => api.get(`/installments/condominium/${condId}`),
  getByYear:          (condId, year)  => api.get(`/installments/condominium/${condId}/year/${year}`),
  getOpen:            (condId)        => api.get(`/installments/condominium/${condId}/open`),
  getOverdue:         (condId)        => api.get(`/installments/condominium/${condId}/overdue`),
  generate:           (condId, year, budgetId) => api.post(`/installments/condominium/${condId}/generate`, { year, budgetId }),
  create:             (data)          => api.post('/installments', data),
  update:             (id, data)      => api.put(`/installments/${id}`, data),
  delete:             (id)            => api.delete(`/installments/${id}`),
}

// ─── Quote ────────────────────────────────────────────────────
export const feeApi = {
  getAll:             ()              => api.get('/fees'),
  getById:            (id)            => api.get(`/fees/${id}`),
  getByInstallment:   (instId)        => api.get(`/fees/installment/${instId}`),
  create:             (data)          => api.post('/fees', data),
  update:             (id, data)      => api.put(`/fees/${id}`, data),
  delete:             (id)            => api.delete(`/fees/${id}`),
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
  getContracts:       (suppId)        => api.get(`/supplier-contracts/supplier/${suppId}`),
  getActiveContracts: (condId)        => api.get(`/supplier-contracts/condominium/${condId}/active`),
  getExpiringContracts:(condId, days) => api.get(`/supplier-contracts/condominium/${condId}/expiring`, { params: { days } }),
  createContract:     (data)          => api.post('/supplier-contracts', data),
  updateContract:     (id, data)      => api.put(`/supplier-contracts/${id}`, data),
  deleteContract:     (id)            => api.delete(`/supplier-contracts/${id}`),
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
  getUnread:          (condId, userId)=> api.get(`/communications/condominium/${condId}/unread`, { params: { userId } }),
  publish:            (id)            => api.post(`/communications/${id}/publish`),
  create:             (data)          => api.post('/communications', data),
  update:             (id, data)      => api.put(`/communications/${id}`, data),
  delete:             (id)            => api.delete(`/communications/${id}`),
}

export default api
