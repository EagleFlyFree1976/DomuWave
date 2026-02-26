import api from './api'

// ─── Tenant ───────────────────────────────────────────────────
export const tenantApi = {
  getAll: () => api.get('/tenants'),
  getById: (id) => api.get(`/tenants/${id}`),
  getPaged: (params) => api.get('/tenants/paged', { params }),   // { page, pageSize, sortField, sortOrder, search, isActive }
  checkCode: (code, excl) => api.get('/tenants/check-code', { params: { code, excludeId: excl } }),
  create: (data) => api.post('/tenants', data),
  update: (id, data) => api.put(`/tenants/${id}`, data),
  delete: (id) => api.delete(`/tenants/${id}`),
}
