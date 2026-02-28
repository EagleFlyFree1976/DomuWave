<template>
  <div class="user-list-page">

    <!-- ── HEADER ─────────────────────────────────────────────────────── -->
    <div class="page-header">
      <div class="page-header__left">
        <h1 class="page-title">
          <i class="pi pi-users page-title__icon" />
          Gestione Utenti
        </h1>
        <span class="page-subtitle">Amministrazione degli utenti della piattaforma</span>
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
      <div class="filter-bar__search">
        <span class="p-input-icon-left w-full">
          <i class="pi pi-search" />
          <InputText v-model="searchText"
                     placeholder="Cerca per nome, email o username..."
                     class="filter-input"
                     @input="onSearchInput" />
        </span>
      </div>

      <div class="filter-bar__status">
        <label class="filter-label">Stato</label>
        <Select v-model="selectedStatus"
                :options="statusOptions"
                option-label="label"
                option-value="value"
                placeholder="Tutti"
                class="filter-select"
                @change="onStatusChange" />
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
                 @sort="onSort">

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
        <Column field="roleCode" header="Ruolo" sortable style="width: 160px">
          <template #body="{ data }">
            <Tag :value="formatRole(data.roleCode)"
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

        <!-- Colonna: Azioni -->
        <Column header="" style="width: 120px; text-align: right">
          <template #body="{ data }">
            <div class="row-actions">
              <Button icon="pi pi-pencil"
                      class="btn-row-action"
                      v-tooltip="'Modifica'"
                      @click="goToDetail(data.id)" />
              <Button icon="pi pi-key"
                      class="btn-row-action btn-row-action--warning"
                      v-tooltip="'Reset password'"
                      @click="confirmResetPassword(data)" />
              <Button v-if="!isCurrentUser(data)"
                      icon="pi pi-trash"
                      class="btn-row-action btn-row-action--danger"
                      v-tooltip="'Elimina'"
                      @click="confirmDelete(data)" />
            </div>
          </template>
        </Column>

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
        <span class="pagination-info">
          {{ paginationInfo }}
        </span>
      </div>
    </div>

    <!-- ── DIALOG CONFERMA ELIMINAZIONE ───────────────────────────────── -->
    <ConfirmDialog class="domu-confirm" />

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { useUserStore } from '@/stores/userStore'
import { useAuthStore } from '@/stores/authStore'

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

// Confronta per email (username = email di login) oppure per id come fallback
function isCurrentUser(userData) {
  if (!auth.user || !userData) return false
  if (auth.user.username && userData.email) return auth.user.username === userData.email
  return String(auth.user.id) === String(userData.id)
}

// ─── LOCAL STATE ────────────────────────────────────────────────────────────
let searchDebounce = null

const searchText     = ref(store.query.search)
const selectedStatus = ref(store.query.isActive)
const sortField      = ref('firstName')
const sortOrder      = ref(1)

const statusOptions = [
  { label: 'Tutti',    value: null  },
  { label: 'Attivi',   value: true  },
  { label: 'Inattivi', value: false },
]

// ─── COMPUTED ────────────────────────────────────────────────────────────────

const hasActiveFilters = computed(
  () => !!searchText.value || selectedStatus.value !== null
)

const paginationInfo = computed(() => {
  const total = store.totalCount
  if (total === 0) return 'Nessun risultato'
  return `${total} utent${total === 1 ? 'e' : 'i'} trovat${total === 1 ? 'o' : 'i'}`
})

// ─── LIFECYCLE ───────────────────────────────────────────────────────────────
onMounted(() => loadList())

// ─── METHODS ────────────────────────────────────────────────────────────────

async function loadList() {
  await store.fetchList()
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
  searchText.value     = ''
  selectedStatus.value = null
  store.setQueryParams({ search: '', isActive: null })
  loadList()
}

function goToNew() {
  router.push({ name: 'user-new' })
}

function goToDetail(id) {
  router.push({ name: 'user-detail', params: { id } })
}

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
    toast.add({
      severity: 'success',
      summary: 'Eliminato',
      detail: `Utente "${fullName(user)}" eliminato.`,
      life: 3000,
    })
  } catch (_) {
    // Il toast di errore è già mostrato dall'interceptor API
  }
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
    toast.add({
      severity: 'success',
      summary: 'Email inviata',
      detail: `Email di reset password inviata a ${user.email}.`,
      life: 4000,
    })
  } catch (_) {
    // Il toast di errore è già mostrato dall'interceptor API
  }
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
    SuperAdmin:           'Super Admin',
    TenantAdministrator:  'Amministratore',
    Admin:                'Admin',
    User:                 'Utente',
  }
  return map[roleCode] ?? roleCode ?? '—'
}

function roleSeverity(roleCode) {
  if (roleCode === 'SuperAdmin') return 'danger'
  if (roleCode === 'TenantAdministrator' || roleCode === 'Admin') return 'warn'
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
.page-header__left {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.page-title {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 22px;
  font-weight: 700;
  color: var(--text);
  margin: 0;
}
.page-title__icon {
  color: var(--accent);
  font-size: 20px;
}
.page-subtitle {
  font-size: 13px;
  color: var(--text-dim);
}

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
.filter-bar__search {
  flex: 1;
  min-width: 200px;
}
.filter-label {
  display: block;
  font-size: 11px;
  color: var(--text-faint);
  text-transform: uppercase;
  letter-spacing: 0.8px;
  margin-bottom: 5px;
}
.filter-input {
  width: 100%;
  background: var(--surface) !important;
  border-color: var(--border) !important;
  color: var(--text) !important;
  font-size: 13px !important;
}
.filter-select {
  width: 160px;
  font-size: 13px;
}

/* ── BUTTONS ─────────────────────────────────────────────────────────────── */
.btn-primary {
  background: var(--accent) !important;
  border-color: var(--accent) !important;
  color: #000 !important;
  font-weight: 600 !important;
  font-size: 13px !important;
}
.btn-ghost {
  background: transparent !important;
  border-color: var(--border) !important;
  color: var(--text-dim) !important;
}
  .btn-ghost:disabled { opacity: 0.4 !important; }

.row-actions {
  display: flex;
  justify-content: flex-end;
  gap: 4px;
}
.btn-row-action {
  background: transparent !important;
  border: none !important;
  color: var(--text-dim) !important;
  width: 32px !important;
  height: 32px !important;
  padding: 0 !important;
  border-radius: 6px !important;
}
  .btn-row-action:hover {
    background: var(--surface2) !important;
    color: var(--accent) !important;
  }
.btn-row-action--warning:hover {
  background: rgba(251, 191, 36, 0.1) !important;
  color: #f59e0b !important;
}
.btn-row-action--danger:hover {
  background: rgba(255, 80, 80, 0.1) !important;
  color: #ff5555 !important;
}

/* ── TABLE ───────────────────────────────────────────────────────────────── */
.table-wrapper {
  background: var(--surface2);
  border: 1px solid var(--border);
  border-radius: 10px;
  overflow: hidden;
}
.domu-table { font-size: 13px; }

/* User name cell */
.user-name-cell {
  display: flex;
  align-items: center;
  gap: 10px;
}
.user-avatar-sm {
  width: 30px;
  height: 30px;
  border-radius: 50%;
  background: linear-gradient(135deg, var(--accent), #059669);
  color: #000;
  font-size: 11px;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.user-name-info {
  display: flex;
  flex-direction: column;
  gap: 1px;
}
.user-fullname {
  font-weight: 500;
  color: var(--text);
  font-size: 13px;
}
.user-login {
  font-size: 11px;
  color: var(--text-faint);
  font-family: 'JetBrains Mono', monospace;
}

.role-tag  { font-size: 11px !important; padding: 2px 8px !important; }
.status-tag { font-size: 11px !important; padding: 2px 8px !important; }
.meta-text { color: var(--text-dim); font-size: 12px; }

/* ── EMPTY STATE ─────────────────────────────────────────────────────────── */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
  padding: 48px 0;
  color: var(--text-faint);
}
.empty-state__icon {
  font-size: 36px;
  opacity: 0.4;
}

/* ── PAGINATION BAR ──────────────────────────────────────────────────────── */
.pagination-bar {
  display: flex;
  align-items: center;
  padding: 10px 16px;
  border-top: 1px solid var(--border);
}
.pagination-info {
  font-size: 12px;
  color: var(--text-faint);
}

/* ── LOADING ─────────────────────────────────────────────────────────────── */
.loading-spinner {
  font-size: 28px;
  color: var(--accent);
}

/* ── RESPONSIVE ──────────────────────────────────────────────────────────── */
@media (max-width: 768px) {
  .user-list-page { padding: 16px; }
  .page-header { flex-direction: column; }
  .filter-bar {
    flex-direction: column;
    align-items: stretch;
  }
  .filter-select { width: 100%; }
}
</style>
