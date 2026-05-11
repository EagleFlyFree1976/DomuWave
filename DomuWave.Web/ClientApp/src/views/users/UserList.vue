<template>
  <div class="user-list-page">

    <!-- ── HEADER ─────────────────────────────────────────────────────── -->
    <div class="page-header">
      <div class="page-header__left">
        <h1 class="page-title">
          <i class="pi pi-users page-title__icon" />
          Gestione Utenti
        </h1>
        <span class="page-subtitle">
          {{ session.isTenantAdmin
              ? 'Utenti del tuo condominio'
              : 'Amministrazione degli utenti della piattaforma' }}
        </span>
      </div>
      <div class="page-header__actions">
        <Button label="Nuovo Utente"
                icon="pi pi-plus"
                class="btn-primary"
                @click="goToNew" />
      </div>
    </div>

    <!-- ── FILTRI ──────────────────────────────────────────────────────── -->
    <div class="filter-bar">
      <!-- Ricerca testo -->
      <div class="filter-bar__search">
        <span class="p-input-icon-left w-full">
          <i class="pi pi-search" />
          <InputText v-model="searchText"
                     placeholder="Cerca per nome, email o username..."
                     class="filter-input"
                     @input="onSearchInput" />
        </span>
      </div>

      <!-- Stato -->
      <div class="filter-bar__item">
        <label class="filter-label">Stato</label>
        <Select v-model="selectedStatus"
                :options="statusOptions"
                option-label="label"
                option-value="value"
                placeholder="Tutti"
                class="filter-select"
                @change="onStatusChange" />
      </div>

      <!-- Tenant (solo SuperAdmin) -->
      <div v-if="session.isSuperAdmin" class="filter-bar__item">
        <label class="filter-label">Tenant</label>
        <Select v-model="selectedTenantId"
                :options="tenantOptions"
                option-label="name"
                option-value="id"
                placeholder="Tutti i tenant"
                class="filter-select filter-select--wide"
                :loading="session.loadingTenants"
                show-clear
                @change="onTenantChange" />
      </div>

      <!-- Condominio (solo SuperAdmin, dopo aver scelto un tenant) -->
      <div v-if="session.isSuperAdmin && selectedTenantId" class="filter-bar__item">
        <label class="filter-label">Condominio</label>
        <Select v-model="selectedCondominiumId"
                :options="condominiumOptions"
                option-label="name"
                option-value="id"
                placeholder="Tutti i condomini"
                class="filter-select filter-select--wide"
                :loading="loadingCondominiums"
                show-clear
                @change="onCondominiumChange" />
      </div>

      <Button icon="pi pi-filter-slash"
              class="btn-ghost"
              v-tooltip="'Reimposta filtri'"
              :disabled="!hasActiveFilters"
              @click="resetFilters" />
    </div>

    <!-- ── TABELLA ─────────────────────────────────────────────────────── -->
    <div class="table-wrapper">
      <DataTable :value="store.users"
                 :loading="store.loading"
                 data-key="id"
                 class="domu-table"
                 :sort-field="sortField"
                 :sort-order="sortOrder"
                 removable-sort
                 :expandedRows="expandedRows"
                 @sort="onSort"
                 @row-expand="onRowExpand"
                 @row-collapse="onRowCollapse">

        <!-- Expand toggle (solo SuperAdmin) -->
        <Column v-if="session.isSuperAdmin"
                expander
                style="width: 3rem" />

        <!-- Colonna: Nome -->
        <Column field="firstName" header="Nome" sortable>
          <template #body="{ data }">
            <div class="user-name-cell">
              <div class="user-avatar-sm">{{ initials(data) }}</div>
              <div class="user-name-info">
                <span class="user-fullname">{{ fullName(data) }}</span>
                <span class="user-login">{{ data.name }}</span>
              </div>
            </div>
          </template>
        </Column>

        <!-- Colonna: Email -->
        <Column field="email" header="Email" sortable>
          <template #body="{ data }">
            <span class="meta-text">{{ data.email || '—' }}</span>
          </template>
        </Column>

        <!-- Colonna: Ruolo -->
        <Column field="role" header="Ruolo" sortable style="width: 160px">
          <template #body="{ data }">
            <Tag :value="data.role"
                 :severity="roleSeverity(data.roleCode)"
                 class="role-tag" />
          </template>
        </Column>

        <!-- Colonna: Stato -->
        <Column field="isActive" header="Stato" sortable style="width: 100px">
          <template #body="{ data }">
            <Tag :value="data.isActive ? 'Attivo' : 'Inattivo'"
                 :severity="data.isActive ? 'success' : 'secondary'"
                 class="status-tag" />
          </template>
        </Column>

        <!-- Colonna: Condomini (preview pillole, solo SuperAdmin) -->
        <Column v-if="session.isSuperAdmin" header="Condomini" style="min-width:200px">
          <template #body="{ data }">
            <div v-if="condominiumsCache[data.id]" class="condo-pills">
              <template v-if="condominiumsCache[data.id].length === 0">
                <span class="meta-text">—</span>
              </template>
              <template v-else>
                <span v-for="c in condominiumsCache[data.id].slice(0, 3)"
                      :key="c.condominiumId"
                      class="condo-pill"
                      :title="c.tenantName">
                  {{ c.condominiumName }}
                </span>
                <span v-if="condominiumsCache[data.id].length > 3"
                      class="condo-pill condo-pill--more">
                  +{{ condominiumsCache[data.id].length - 3 }}
                </span>
              </template>
            </div>
            <span v-else class="meta-text">—</span>
          </template>
        </Column>

        <!-- Colonna: Azioni -->
        <Column header="" style="width: 120px; text-align: right">
          <template #body="{ data }">
            <div class="row-actions">
              <Button v-if="canManage(data)"
                      icon="pi pi-pencil"
                      class="btn-row-action"
                      v-tooltip="'Modifica'"
                      @click="goToDetail(data.id)" />
              <Button v-else
                      icon="pi pi-eye"
                      class="btn-row-action"
                      v-tooltip="'Visualizza (sola lettura)'"
                      @click="goToDetail(data.id)" />

              <Button icon="pi pi-key"
                      class="btn-row-action btn-row-action--warning"
                      v-tooltip="'Reset password'"
                      :disabled="!canManage(data)"
                      @click="confirmResetPassword(data)" />

              <Button v-if="session.isSuperAdmin && !isCurrentUser(data)"
                      icon="pi pi-user-edit"
                      class="btn-row-action btn-row-action--impersonate"
                      v-tooltip="'Impersona'"
                      @click="doImpersonate(data)" />

              <Button v-if="!isCurrentUser(data) && canManage(data)"
                      icon="pi pi-trash"
                      class="btn-row-action btn-row-action--danger"
                      v-tooltip="'Elimina'"
                      @click="confirmDelete(data)" />
            </div>
          </template>
        </Column>

        <!-- ── RIGA ESPANSA: dettaglio condomini ─────────────────────── -->
        <template #expansion="{ data }">
          <div class="expansion-panel">
            <div v-if="loadingExpansion[data.id]" class="expansion-loading">
              <i class="pi pi-spinner pi-spin" /> Caricamento condomini...
            </div>
            <div v-else-if="!condominiumsCache[data.id]?.length" class="expansion-empty">
              <i class="pi pi-building" />
              <span>Nessun condominio associato a questo utente.</span>
            </div>
            <div v-else class="expansion-content">
              <p class="expansion-title">
                Condomini associati ({{ condominiumsCache[data.id].length }})
              </p>
              <!-- Raggruppa per tenant -->
              <div v-for="(group, tenantName) in groupByTenant(condominiumsCache[data.id])"
                   :key="tenantName"
                   class="tenant-group">
                <div class="tenant-group__header">
                  <i class="pi pi-briefcase" />
                  {{ tenantName }}
                </div>
                <div class="tenant-group__condos">
                  <div v-for="c in group" :key="c.condominiumId" class="condo-row">
                    <i class="pi pi-building condo-icon" />
                    <span class="condo-name">{{ c.condominiumName }}</span>
                    <span class="condo-code">{{ c.condominiumCode }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </template>

        <!-- Stato vuoto -->
        <template #empty>
          <div class="empty-state">
            <i class="pi pi-users empty-state__icon" />
            <span>Nessun utente trovato</span>
          </div>
        </template>

        <template #loadingicon>
          <i class="pi pi-spinner pi-spin loading-spinner" />
        </template>
      </DataTable>

      <!-- ── INFO RISULTATI ───────────────────────────────────────────── -->
      <div class="pagination-bar">
        <span class="pagination-info">{{ paginationInfo }}</span>
        <span v-if="session.isTenantAdmin" class="tenant-scope-note">
          <i class="pi pi-info-circle" />
          Visualizzi solo gli utenti associati al tuo tenant
        </span>
      </div>
    </div>

    <ConfirmDialog class="domu-confirm" />

  </div>
</template>

<script setup>
import { ref, computed, onMounted, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { useUserStore } from '@/stores/userStore'
import { useAuthStore } from '@/stores/authStore'
import { useSessionStore } from '@/stores/sessionStore'
import { userTenantApi } from '@/services/userService'

import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import Tag from 'primevue/tag'
import ConfirmDialog from 'primevue/confirmdialog'

// ─── COMPOSABLES ────────────────────────────────────────────────────────────
const router  = useRouter()
const confirm = useConfirm()
const toast   = useToast()
const store   = useUserStore()
const auth    = useAuthStore()
const session = useSessionStore()

// Ruoli gestibili da TenantAdmin
const TENANT_ADMIN_ROLES = ['CLB', 'Condomino']

function canManage(userData) {
  if (session.isSuperAdmin) return true
  const role = userData.roleCode ?? userData.role ?? ''
  return TENANT_ADMIN_ROLES.includes(role)
}

function isCurrentUser(userData) {
  if (!auth.user || !userData) return false
  if (auth.user.username && userData.email) return auth.user.username === userData.email
  return String(auth.user.id) === String(userData.id)
}

// ─── FILTRI STATE ─────────────────────────────────────────────────────────────
// Tutti i filtri sono inizializzati dallo store e vi vengono riscritti ad ogni
// modifica, così sopravvivono alla navigazione dettaglio → lista.
let searchDebounce = null

const searchText            = ref(store.query.search)
const selectedStatus        = ref(store.query.isActive)
const selectedTenantId      = ref(store.query.tenantId)
const selectedCondominiumId = ref(store.query.condominiumId)
const sortField             = ref('firstName')
const sortOrder             = ref(1)

// Opzioni condomini per il filtro (caricate quando si sceglie un tenant)
const condominiumOptions  = ref([])
const loadingCondominiums = ref(false)

const statusOptions = [
  { label: 'Tutti',    value: null  },
  { label: 'Attivi',   value: true  },
  { label: 'Inattivi', value: false },
]

// Tenant disponibili (per SuperAdmin — dalla sessionStore)
const tenantOptions = computed(() => session.availableTenants ?? [])

// ─── EXPAND STATE ────────────────────────────────────────────────────────────
const expandedRows        = ref([])
// cache: userId → UserCondominiumDto[]
const condominiumsCache   = reactive({})
// loading flag per userId
const loadingExpansion    = reactive({})

async function onRowExpand(event) {
  const userId = event.data.id
  if (condominiumsCache[userId] !== undefined) return  // già caricato
  loadingExpansion[userId] = true
  try {
    const { data } = await userTenantApi.getCondominiumsByUser(userId)
    condominiumsCache[userId] = Array.isArray(data) ? data : []
  } catch {
    condominiumsCache[userId] = []
  } finally {
    loadingExpansion[userId] = false
  }
}

function onRowCollapse() { /* noop */ }

function groupByTenant(condominiums) {
  return condominiums.reduce((acc, c) => {
    const key = c.tenantName || 'Sconosciuto'
    if (!acc[key]) acc[key] = []
    acc[key].push(c)
    return acc
  }, {})
}

// ─── COMPUTED ────────────────────────────────────────────────────────────────

const hasActiveFilters = computed(
  () => !!searchText.value
     || selectedStatus.value !== null
     || !!selectedTenantId.value
     || !!selectedCondominiumId.value
)

const paginationInfo = computed(() => {
  const total = store.totalCount
  if (total === 0) return 'Nessun risultato'
  return `${total} utent${total === 1 ? 'e' : 'i'} trovat${total === 1 ? 'o' : 'i'}`
})

// ─── LIFECYCLE ───────────────────────────────────────────────────────────────
onMounted(async () => {
  // Azzera i filtri se non si arriva dal dettaglio utente
  const prevRoute = window.history.state?.back ?? ''
  const comingFromDetail = prevRoute.startsWith('/users/')
  if (!comingFromDetail) {
    store.setQueryParams({ search: '', isActive: null, tenantId: null, condominiumId: null })
    searchText.value            = ''
    selectedStatus.value        = null
    selectedTenantId.value      = null
    selectedCondominiumId.value = null
    condominiumOptions.value    = []
  }

  if (session.isSuperAdmin && !session.availableTenants.length) {
    await session.loadTenants()
  }
  await loadList()
  // Ricostruisce le opzioni condominio se il filtro tenant era già attivo
  if (selectedTenantId.value) {
    buildCondominiumOptionsFromCache()
  }
})

// ─── METHODS ────────────────────────────────────────────────────────────────

async function loadList() {
  if (session.isTenantAdmin) {
    // activeTenant è null dopo un refresh di pagina (initFromAuth è chiamato solo al login)
    const tenantId = session.activeTenant?.id ?? localStorage.getItem('tenantId')
    if (!tenantId) { store.users = []; store.totalCount = 0; return }
    await store.fetchListByTenant(tenantId, TENANT_ADMIN_ROLES)
  } else {
    // SuperAdmin: filtra per condominio (userId list) o per tenant o globale
    if (selectedCondominiumId.value) {
      await loadByCondominium()
    } else if (selectedTenantId.value) {
      await store.fetchListByTenant(selectedTenantId.value, null)
    } else {
      await store.fetchList()
    }
  }

  // Pre-carica i condomini per tutte le righe visibili (colonna preview)
  if (session.isSuperAdmin) {
    preloadCondominiums(store.users)
  }
}

/**
 * Carica gli utenti filtrati per condominio:
 * recupera gli unitOwner del condominio e filtra gli user con quell'userId.
 * Dato che non abbiamo un endpoint "users by condominium", carichiamo prima
 * tutti gli utenti del tenant e poi filtriamo lato client per condominio
 * usando la cache dei condomini.
 */
async function loadByCondominium() {
  // Carica utenti del tenant selezionato
  await store.fetchListByTenant(selectedTenantId.value, null)
  // Poi filtra lato client: tieni solo chi ha il condominio selezionato
  // (la cache viene popolata da preloadCondominiums)
  await preloadCondominiums(store.users)
  store.users = store.users.filter(u => {
    const condos = condominiumsCache[u.id]
    return condos?.some(c => c.condominiumId === selectedCondominiumId.value)
  })
  store.totalCount = store.users.length
}

/**
 * Pre-carica i condomini per una lista di utenti in parallelo (batch di 5).
 */
async function preloadCondominiums(users) {
  const toLoad = users.filter(u => condominiumsCache[u.id] === undefined)
  if (!toLoad.length) return

  // Batch da 5 richieste parallele
  const BATCH = 5
  for (let i = 0; i < toLoad.length; i += BATCH) {
    const batch = toLoad.slice(i, i + BATCH)
    await Promise.all(batch.map(async u => {
      if (condominiumsCache[u.id] !== undefined) return
      try {
        const { data } = await userTenantApi.getCondominiumsByUser(u.id)
        condominiumsCache[u.id] = Array.isArray(data) ? data : []
      } catch {
        condominiumsCache[u.id] = []
      }
    }))
  }
}

async function onTenantChange() {
  selectedCondominiumId.value = null
  condominiumOptions.value    = []

  store.setQueryParams({ tenantId: selectedTenantId.value, condominiumId: null })
  await loadList()
  buildCondominiumOptionsFromCache()
}

/** Estrae le opzioni condominio uniche dalla cache per il tenant selezionato */
function buildCondominiumOptionsFromCache() {
  if (!selectedTenantId.value) { condominiumOptions.value = []; return }
  const seen = new Map()
  for (const condos of Object.values(condominiumsCache)) {
    for (const c of condos) {
      if (c.tenantId === selectedTenantId.value && !seen.has(c.condominiumId)) {
        seen.set(c.condominiumId, { id: c.condominiumId, name: `${c.condominiumCode} – ${c.condominiumName}` })
      }
    }
  }
  condominiumOptions.value = [...seen.values()]
    .sort((a, b) => a.name.localeCompare(b.name))
}

async function onCondominiumChange() {
  store.setQueryParams({ condominiumId: selectedCondominiumId.value })
  await loadList()
}

function onSearchInput() {
  clearTimeout(searchDebounce)
  searchDebounce = setTimeout(() => {
    store.setQueryParams({ search: searchText.value })
    loadList()
  }, 400)
}

function onStatusChange() {
  store.setQueryParams({ isActive: selectedStatus.value })
  loadList()
}

function onSort(event) {
  sortField.value = event.sortField
  sortOrder.value = event.sortOrder
}

function resetFilters() {
  searchText.value            = ''
  selectedStatus.value        = null
  selectedTenantId.value      = null
  selectedCondominiumId.value = null
  condominiumOptions.value    = []
  store.setQueryParams({ search: '', isActive: null, tenantId: null, condominiumId: null })
  loadList()
}

function goToNew() { router.push({ name: 'user-new' }) }
function goToDetail(id) { router.push({ name: 'user-detail', params: { id } }) }

function confirmDelete(user) {
  confirm.require({
    message: `Vuoi eliminare l'utente "${fullName(user)}"? L'operazione non è reversibile.`,
    header: 'Conferma eliminazione',
    icon: 'pi pi-exclamation-triangle',
    acceptLabel: 'Elimina',
    rejectLabel: 'Annulla',
    acceptClass: 'p-button-danger',
    accept: () => doDelete(user),
  })
}

async function doDelete(user) {
  try {
    await store.remove(user.id)
    toast.add({ severity: 'success', summary: 'Eliminato',
      detail: `Utente "${fullName(user)}" eliminato.`, life: 3000 })
  } catch (_) { /* gestito dall'interceptor */ }
}

function confirmResetPassword(user) {
  confirm.require({
    message: `Vuoi inviare un'email di reset password a "${user.email}"?`,
    header: 'Reset Password',
    icon: 'pi pi-key',
    acceptLabel: 'Invia',
    rejectLabel: 'Annulla',
    accept: () => doResetPassword(user),
  })
}

async function doResetPassword(user) {
  try {
    await store.resetPassword(user.id)
    toast.add({ severity: 'success', summary: 'Email inviata',
      detail: `Email di reset password inviata a ${user.email}.`, life: 4000 })
  } catch (_) { /* gestito dall'interceptor */ }
}

async function doImpersonate(userData) {
  confirm.require({
    message: `Vuoi impersonare "${fullName(userData)}"? Vedrai l'app come la vede questo utente.`,
    header: 'Impersonificazione',
    icon: 'pi pi-user-edit',
    acceptLabel: 'Impersona',
    rejectLabel: 'Annulla',
    accept: async () => {
      const result = await auth.impersonate(userData.id)
      if (result.success) {
        toast.add({ severity: 'warn', summary: 'Impersonificazione attiva',
          detail: `Stai navigando come ${fullName(userData)}`, life: 5000 })
        await router.push('/dashboard')
        window.location.reload()
      } else {
        toast.add({ severity: 'error', summary: 'Errore', detail: result.message, life: 4000 })
      }
    },
  })
}

// ─── HELPERS ────────────────────────────────────────────────────────────────

function fullName(user) {
  return [user.firstName, user.lastName].filter(Boolean).join(' ') || user.name || '—'
}

function initials(user) {
  const fn = user.firstName?.[0] ?? ''
  const ln = user.lastName?.[0] ?? ''
  return (fn + ln).toUpperCase() || (user.name?.[0] ?? '?').toUpperCase()
}

function formatRole(roleCode) {
  const map = {
    SuperAdmin:          'Super Admin',
    TenantAdministrator: 'Amministratore',
    Collaboratore:       'Collaboratore',
    Condomino:           'Condomino',
    Admin:               'Admin',
    User:                'Utente',
  }
  return map[roleCode] ?? roleCode ?? '—'
}

function roleSeverity(roleCode) {
  if (roleCode === 'SuperAdmin') return 'danger'
  if (roleCode === 'TenantAdministrator' || roleCode === 'Admin') return 'warn'
  if (roleCode === 'Collaboratore') return 'info'
  return 'secondary'
}
</script>

<style scoped>
/* ── PAGE LAYOUT ─────────────────────────────────────────────────────────── */
.user-list-page {
  display: flex;
  flex-direction: column;
  gap: 20px;
  padding: 28px 32px;
  min-height: 100%;
}

/* ── HEADER ──────────────────────────────────────────────────────────────── */
.page-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
}
.page-header__left { display: flex; flex-direction: column; gap: 4px; }
.page-title {
  display: flex; align-items: center; gap: 10px;
  font-size: 22px; font-weight: 700; color: var(--text); margin: 0;
}
.page-title__icon { color: var(--accent); font-size: 20px; }
.page-subtitle    { font-size: 13px; color: var(--text-dim); }

/* ── FILTER BAR ──────────────────────────────────────────────────────────── */
.filter-bar {
  display: flex;
  align-items: flex-end;
  gap: 12px;
  flex-wrap: wrap;
  background: var(--surface2);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 14px 16px;
}
.filter-bar__search { flex: 1; min-width: 200px; }
.filter-bar__item   { display: flex; flex-direction: column; gap: 4px; }
.filter-label {
  display: block;
  font-size: 11px; color: var(--text-faint);
  text-transform: uppercase; letter-spacing: 0.8px;
}
.filter-input {
  width: 100%;
  background: var(--surface) !important;
  border-color: var(--border) !important;
  color: var(--text) !important;
  font-size: 13px !important;
}
.filter-select       { width: 160px; font-size: 13px; }
.filter-select--wide { width: 220px; }

/* ── BUTTONS ─────────────────────────────────────────────────────────────── */
.btn-primary {
  background: var(--accent) !important; border-color: var(--accent) !important;
  color: #000 !important; font-weight: 600 !important; font-size: 13px !important;
}
.btn-ghost {
  background: transparent !important; border-color: var(--border) !important;
  color: var(--text-dim) !important;
}
.btn-ghost:disabled { opacity: 0.4 !important; }

.row-actions { display: flex; justify-content: flex-end; gap: 4px; }
.btn-row-action {
  background: transparent !important; border: none !important;
  color: var(--text-dim) !important;
  width: 32px !important; height: 32px !important;
  padding: 0 !important; border-radius: 6px !important;
}
.btn-row-action:hover {
  background: var(--surface2) !important; color: var(--accent) !important;
}
.btn-row-action--warning:hover {
  background: rgba(251,191,36,.1) !important; color: #f59e0b !important;
}
.btn-row-action--danger:hover {
  background: rgba(255,80,80,.1) !important; color: #ff5555 !important;
}
.btn-row-action--impersonate:hover {
  background: rgba(99,102,241,.15) !important; color: #6366f1 !important;
}

/* ── TABLE ───────────────────────────────────────────────────────────────── */
.table-wrapper {
  background: var(--surface2); border: 1px solid var(--border);
  border-radius: 10px; overflow: hidden;
}
.domu-table { font-size: 13px; }

.user-name-cell { display: flex; align-items: center; gap: 10px; }
.user-avatar-sm {
  width: 30px; height: 30px; border-radius: 50%;
  background: linear-gradient(135deg, var(--accent), #059669);
  color: #000; font-size: 11px; font-weight: 700;
  display: flex; align-items: center; justify-content: center; flex-shrink: 0;
}
.user-name-info { display: flex; flex-direction: column; gap: 1px; }
.user-fullname  { font-weight: 500; color: var(--text); font-size: 13px; }
.user-login     { font-size: 11px; color: var(--text-faint); font-family: 'JetBrains Mono', monospace; }

.role-tag   { font-size: 11px !important; padding: 2px 8px !important; }
.status-tag { font-size: 11px !important; padding: 2px 8px !important; }
.meta-text  { color: var(--text-dim); font-size: 12px; }

/* ── CONDOMINI PILLS (preview colonna) ───────────────────────────────────── */
.condo-pills { display: flex; flex-wrap: wrap; gap: 4px; }
.condo-pill {
  display: inline-block;
  padding: 2px 8px;
  background: rgba(52, 211, 153, 0.1);
  border: 1px solid rgba(52, 211, 153, 0.3);
  border-radius: 20px;
  font-size: 11px;
  color: var(--text);
  white-space: nowrap;
  max-width: 130px;
  overflow: hidden;
  text-overflow: ellipsis;
}
.condo-pill--more {
  background: var(--surface);
  border-color: var(--border);
  color: var(--text-muted);
}

/* ── EXPANSION PANEL ─────────────────────────────────────────────────────── */
.expansion-panel {
  padding: 16px 24px;
  background: color-mix(in srgb, var(--accent) 3%, var(--surface));
}
.expansion-loading, .expansion-empty {
  display: flex; align-items: center; gap: 8px;
  font-size: 13px; color: var(--text-faint); padding: 8px 0;
}
.expansion-title {
  font-size: 12px; font-weight: 600;
  text-transform: uppercase; letter-spacing: 0.6px;
  color: var(--text-dim); margin-bottom: 12px;
}
.expansion-content { display: flex; flex-direction: column; gap: 12px; }

.tenant-group { }
.tenant-group__header {
  display: flex; align-items: center; gap: 6px;
  font-size: 12px; font-weight: 600; color: var(--accent);
  margin-bottom: 6px;
}
.tenant-group__condos {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 6px;
  padding-left: 18px;
}
.condo-row {
  display: flex; align-items: center; gap: 8px;
  padding: 6px 10px;
  background: var(--surface2);
  border: 1px solid var(--border);
  border-radius: 6px;
}
.condo-icon { color: var(--text-muted); font-size: 12px; flex-shrink: 0; }
.condo-name { font-size: 13px; font-weight: 500; color: var(--text); flex: 1; }
.condo-code { font-size: 11px; color: var(--text-faint); font-family: 'JetBrains Mono', monospace; }

/* ── EMPTY STATE ─────────────────────────────────────────────────────────── */
.empty-state {
  display: flex; flex-direction: column; align-items: center;
  gap: 10px; padding: 48px 0; color: var(--text-faint);
}
.empty-state__icon { font-size: 36px; opacity: 0.4; }

/* ── PAGINATION BAR ──────────────────────────────────────────────────────── */
.pagination-bar {
  display: flex; align-items: center; justify-content: space-between;
  padding: 10px 16px; border-top: 1px solid var(--border);
}
.pagination-info  { font-size: 12px; color: var(--text-faint); }
.tenant-scope-note {
  display: flex; align-items: center; gap: 5px;
  font-size: 11px; color: var(--text-faint); font-style: italic;
}

/* ── LOADING ─────────────────────────────────────────────────────────────── */
.loading-spinner { font-size: 28px; color: var(--accent); }

/* ── RESPONSIVE ──────────────────────────────────────────────────────────── */
@media (max-width: 768px) {
  .user-list-page { padding: 16px; }
  .page-header    { flex-direction: column; }
  .filter-bar     { flex-direction: column; align-items: stretch; }
  .filter-select, .filter-select--wide { width: 100%; }
}
</style>
