import authApiClient from './authApiClient'

// ─── Autorizzazioni (risorse autorizzabili) ──────────────────────────────────
export const authorizationsApi = {
  /** Lista tutte le risorse: [{ id, code, description, areaId, areaCode, areaDescription, moduleCode }] */
  getAll: () => authApiClient.get('/authorization/authorizations'),

  /** Lista delle aree attive (per la select): [{ id, code, description, moduleCode }] */
  getAreas: () => authApiClient.get('/authorization/authorizations/areas'),

  /** Crea una nuova risorsa. Body: { code, description, areaId } */
  create: (data) => authApiClient.post('/authorization/authorization', data),

  /** Aggiorna una risorsa esistente (code immutabile). Body: { description, areaId } */
  update: (id, data) => authApiClient.put(`/authorization/authorizations/${id}`, data),

  /** Elimina una risorsa (fallisce se assegnata a ruoli/gruppi/utenti). */
  remove: (id) => authApiClient.delete(`/authorization/authorizations/${id}`),
}

// ─── Ruoli ───────────────────────────────────────────────────────────────────
export const rolesAuthApi = {
  /** Lista ruoli per modulo: [{ id, code, description }] */
  getByModule: (moduleCode) =>
    authApiClient.get('/authorization/roles/bymodule', { params: { moduleCode } }),

  /** Lista tutti i ruoli: [{ id, code, description }] */
  getAll: (params) =>
    authApiClient.get('/authorization/roles', { params }),

  /** Lista dei moduli disponibili: [{ id, code, description }] */
  getModules: () =>
    authApiClient.get('/authorization/modules'),

  /** Crea un nuovo ruolo (vuoto). Body: { code, description, moduleCode } */
  create: (data) =>
    authApiClient.post('/authorization/roles/new', data),

  /** Aggiorna codice/descrizione di un ruolo esistente. Body: { code, description } */
  updateDetails: (id, data) =>
    authApiClient.put(`/authorization/roles/${id}/details`, data),

  /** Clona un ruolo (con i permessi) con codice/descrizione nuovi. Body: { code, description, moduleCode } */
  clone: (id, data) =>
    authApiClient.post(`/authorization/roles/${id}/clone`, data),

  /** Copia (merge) i permessi dal ruolo sorgente in questo ruolo. Body: { sourceId } */
  copyPermissions: (id, sourceId) =>
    authApiClient.post(`/authorization/roles/${id}/copy-permissions`, { sourceId }),

  /**
   * Crea un'assegnazione permesso sul ruolo.
   * Restituisce l'ID del record GroupAuthorization creato (stringa).
   * Body: { entityId, authCode, can: { canView, canCreate, canModify, canDelete, canAction } }
   */
  addPermission: (entityId, authCode, can) =>
    authApiClient.post('/authorization/role', { entityId, authCode, can }),

  /**
   * Aggiorna un'assegnazione esistente (id = GroupAuthorization record ID).
   * Body: { entityId, authCode, can }
   */
  updatePermission: (id, entityId, authCode, can) =>
    authApiClient.put(`/authorization/roles/${id}`, { entityId, authCode, can }),

  /** Rimuove un'assegnazione permesso dal ruolo (id = GroupAuthorization record ID). */
  deletePermission: (id) =>
    authApiClient.delete(`/authorization/roles/${id}`),
}

// ─── Gruppi ──────────────────────────────────────────────────────────────────
export const groupsAuthApi = {
  /**
   * Lista tutti i gruppi attivi (include anche i ruoli con isRole=true).
   * Filtrare lato client con !item.isRole per avere solo i gruppi.
   */
  getAll: (q) =>
    authApiClient.get('/authorization/groups', { params: { q: q ?? '' } }),

  /**
   * Recupera le autorizzazioni assegnate a un gruppo o ruolo (GroupBase).
   * Funziona sia per i Gruppi che per i Ruoli, poiché entrambi estendono GroupBase.
   * Restituisce: [{ id?, authCode, can: { canView, canCreate, canModify, canDelete, canAction } }]
   */
  getAuthorizations: (groupId) =>
    authApiClient.get(`/authorization/groups/${groupId}/authorizations`),

  /**
   * Crea un'assegnazione permesso sul gruppo.
   * Restituisce l'ID del record GroupAuthorization creato (stringa).
   */
  addPermission: (entityId, authCode, can) =>
    authApiClient.post('/authorization/group', { entityId, authCode, can }),

  /** Aggiorna un'assegnazione esistente (id = GroupAuthorization record ID). */
  updatePermission: (id, entityId, authCode, can) =>
    authApiClient.put(`/authorization/groups/${id}`, { entityId, authCode, can }),

  /** Rimuove un'assegnazione permesso dal gruppo o dal ruolo. */
  deletePermission: (id) =>
    authApiClient.delete(`/authorization/groups/${id}`),
}

// ─── Utenti (autorizzazioni dirette) ─────────────────────────────────────────
export const usersAuthApi = {
  /**
   * Recupera le autorizzazioni di un utente specifico.
   * Restituisce: [{ authCode, can: { canView, canCreate, canModify, canDelete, canAction } }]
   */
  getAuthorizations: (userId) =>
    authApiClient.get(`/authorization/users/${userId}/authorizations`),

  /** Aggiorna un'autorizzazione diretta dell'utente (id = UserAuthorization record ID). */
  updatePermission: (id, entityId, authCode, can) =>
    authApiClient.put(`/authorization/users/${id}`, { entityId, authCode, can }),

  /** Rimuove un'autorizzazione diretta dell'utente. */
  deletePermission: (id) =>
    authApiClient.delete(`/authorization/users/${id}`),
}
