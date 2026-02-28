import api from './api'
import authApiClient from '@/services/authApiClient'
// ─── Utenti ──────────────────────────────────────────────────────────────────
export const userApi = {
  /** Ricerca utenti con filtri: { search, isActive, roles, groups } */
  search: (params) => authApiClient.get('/users/search', { params }),
  getById: (id) => authApiClient.get(`/users/${id}`),
  /** Crea utente (admin): { email, password, name, surName } */
  create: (data) => authApiClient.post('/users/register', data),
  update: (id, data) => authApiClient.put(`/users/${id}`, data),
  delete: (id) => authApiClient.delete(`/users/${id}`),
  /** Invia email di reset password */
  resetPassword: (id) => authApiClient.post(`/users/${id}/reset-password`),
}

// ─── Ruoli per modulo ────────────────────────────────────────────────────────
export const rolesApi = {
  /** Ruoli afferenti a un modulo: [{ id, code, description }] */
  getByModule: (moduleCode) =>
    authApiClient.get('/authorization/roles/bymodule', { params: { moduleCode } }),
}

// ─── Associazioni Utente-Tenant ───────────────────────────────────────────────
export const userTenantApi = {
  /** Tutti i tenant di un utente */
  getByUserId:  (userId)   => api.get(`/user-tenants/user/${userId}`),
  /** Tenant di default di un utente */
  getDefault:   (userId)   => api.get(`/user-tenants/user/${userId}/default`),
  /** Tutti gli utenti associati a un tenant */
  getByTenantId: (tenantId) => api.get(`/user-tenants/tenant/${tenantId}`),
  getById:      (id)       => api.get(`/user-tenants/${id}`),
  /** Crea associazione: { userId, tenantId, isDefault, isActive } */
  create:       (data)     => api.post('/user-tenants', data),
  /** Aggiorna: { isDefault, isActive } */
  update:       (id, data) => api.put(`/user-tenants/${id}`, data),
  /** Imposta il tenant di default per un utente */
  setDefault:   (userId, userTenantId) =>
    api.patch(`/user-tenants/user/${userId}/set-default`, { userTenantId }),
  delete:       (id)       => api.delete(`/user-tenants/${id}`),
}
