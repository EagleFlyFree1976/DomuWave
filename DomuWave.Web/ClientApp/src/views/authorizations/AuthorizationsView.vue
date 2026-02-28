<template>
  <div class="auth-page">

    <!-- ── HEADER ─────────────────────────────────────────────────────────── -->
    <div class="page-header">
      <div class="page-header__left">
        <h1 class="page-title">
          <i class="pi pi-shield page-title__icon" />
          Autorizzazioni
        </h1>
        <span class="page-subtitle">Gestione permessi per ruoli e gruppi del modulo DomuWeb</span>
      </div>
    </div>

    <!-- ── TAB BAR ────────────────────────────────────────────────────────── -->
    <div class="tab-bar">
      <button class="tab-btn" :class="{ 'tab-btn--active': activeTab === 'roles' }"
              @click="activeTab = 'roles'">
        <i class="pi pi-id-card" /> Ruoli
      </button>
      <button class="tab-btn" :class="{ 'tab-btn--active': activeTab === 'groups' }"
              @click="activeTab = 'groups'">
        <i class="pi pi-users" /> Gruppi
      </button>
    </div>

    <!-- ── TAB: RUOLI ─────────────────────────────────────────────────────── -->
    <div v-show="activeTab === 'roles'" class="tab-content">
      <div class="filter-bar">
        <div class="filter-bar__search">
          <span class="p-input-icon-left w-full">
            <i class="pi pi-search" />
            <InputText v-model="roleSearch"
                       placeholder="Cerca codice o descrizione..."
                       class="filter-input" />
          </span>
        </div>
        <Button icon="pi pi-refresh" class="btn-ghost" v-tooltip="'Ricarica'"
                :loading="loadingRoles" @click="loadRoles" />
      </div>

      <div class="table-wrapper">
        <DataTable :value="filteredRoles" :loading="loadingRoles"
                   data-key="id" class="domu-table">
          <Column field="code" header="Codice" style="width: 180px">
            <template #body="{ data }">
              <span class="code-badge">{{ data.code }}</span>
            </template>
          </Column>
          <Column field="description" header="Descrizione" />
          <Column header="" style="width: 130px; text-align: right">
            <template #body="{ data }">
              <Button icon="pi pi-key"
                      label="Gestisci"
                      class="btn-manage"
                      @click="openDialog(data, 'role')" />
            </template>
          </Column>
          <template #empty>
            <div class="empty-state">
              <i class="pi pi-id-card empty-state__icon" />
              <span>Nessun ruolo trovato per il modulo DomuWeb</span>
            </div>
          </template>
          <template #loadingicon>
            <i class="pi pi-spinner pi-spin loading-spinner" />
          </template>
        </DataTable>
      </div>
    </div>

    <!-- ── TAB: GRUPPI ────────────────────────────────────────────────────── -->
    <div v-show="activeTab === 'groups'" class="tab-content">
      <div class="filter-bar">
        <div class="filter-bar__search">
          <span class="p-input-icon-left w-full">
            <i class="pi pi-search" />
            <InputText v-model="groupSearch"
                       placeholder="Cerca codice o descrizione..."
                       class="filter-input" />
          </span>
        </div>
        <Button icon="pi pi-refresh" class="btn-ghost" v-tooltip="'Ricarica'"
                :loading="loadingGroups" @click="loadGroups" />
      </div>

      <div class="table-wrapper">
        <DataTable :value="filteredGroups" :loading="loadingGroups"
                   data-key="id" class="domu-table">
          <Column field="code" header="Codice" style="width: 180px">
            <template #body="{ data }">
              <span class="code-badge">{{ data.code }}</span>
            </template>
          </Column>
          <Column field="description" header="Descrizione" />
          <Column header="" style="width: 130px; text-align: right">
            <template #body="{ data }">
              <Button icon="pi pi-key"
                      label="Gestisci"
                      class="btn-manage"
                      @click="openDialog(data, 'group')" />
            </template>
          </Column>
          <template #empty>
            <div class="empty-state">
              <i class="pi pi-users empty-state__icon" />
              <span>Nessun gruppo trovato</span>
            </div>
          </template>
          <template #loadingicon>
            <i class="pi pi-spinner pi-spin loading-spinner" />
          </template>
        </DataTable>
      </div>
    </div>

    <!-- ── DIALOG: Gestione Permessi ──────────────────────────────────────── -->
    <Dialog v-model:visible="dialogVisible"
            :header="`Permessi — ${dialogItem?.description ?? dialogItem?.code ?? ''}`"
            :modal="true"
            :draggable="false"
            style="width: min(920px, 95vw)"
            class="perm-dialog">

      <div class="dialog-body">

        <!-- ── Permessi esistenti ────────────────────────────────────────── -->
        <div class="existing-section">
          <span class="existing-title">
            <i class="pi pi-list-check" />
            Permessi esistenti
          </span>

          <div v-if="loadingExisting" class="existing-loading">
            <i class="pi pi-spinner pi-spin" />
            <span>Caricamento in corso...</span>
          </div>

          <div v-else-if="existingPermissions.length === 0" class="existing-empty">
            <i class="pi pi-info-circle" />
            <span>Nessun permesso assegnato a questo {{ dialogType === 'role' ? 'ruolo' : 'gruppo' }}</span>
          </div>

          <div v-else class="perm-list">
            <div v-for="perm in existingPermissions" :key="perm.id ?? perm.authCode" class="exist-item">

              <!-- Header: codice + azioni -->
              <div class="exist-item__header">
                <div class="session-item__left">
                  <span class="session-item__code">{{ perm.authCode }}</span>
                  <span v-if="perm.id" class="session-item__id">record #{{ perm.id }}</span>
                </div>
                <div class="exist-item__actions">
                  <button class="btn-all-toggle"
                          :disabled="!!savingPerm[perm.id]"
                          @click="toggleAll(perm)">
                    {{ isAllSelected(perm.can) ? 'Deseleziona tutto' : 'Seleziona tutto' }}
                  </button>
                  <i v-if="savingPerm[perm.id]" class="pi pi-spinner pi-spin exist-item__saving" />
                  <Button v-if="perm.id"
                          icon="pi pi-trash"
                          class="btn-icon-sm btn-icon-sm--danger"
                          v-tooltip="'Rimuovi permesso'"
                          :loading="!!deleting[perm.id]"
                          @click="deletePermission(perm, existingPermissions)" />
                </div>
              </div>

              <!-- Toggle permessi inline -->
              <div class="exist-item__perms">
                <label class="exist-perm-toggle">
                  <ToggleSwitch v-model="perm.can.canView"
                                :disabled="!!savingPerm[perm.id]"
                                @change="updateExistingPermission(perm)" />
                  <span><i class="pi pi-eye" /> Visualizza</span>
                </label>
                <label class="exist-perm-toggle">
                  <ToggleSwitch v-model="perm.can.canCreate"
                                :disabled="!!savingPerm[perm.id]"
                                @change="updateExistingPermission(perm)" />
                  <span><i class="pi pi-plus-circle" /> Crea</span>
                </label>
                <label class="exist-perm-toggle">
                  <ToggleSwitch v-model="perm.can.canModify"
                                :disabled="!!savingPerm[perm.id]"
                                @change="updateExistingPermission(perm)" />
                  <span><i class="pi pi-pencil" /> Modifica</span>
                </label>
                <label class="exist-perm-toggle">
                  <ToggleSwitch v-model="perm.can.canDelete"
                                :disabled="!!savingPerm[perm.id]"
                                @change="updateExistingPermission(perm)" />
                  <span><i class="pi pi-trash" /> Elimina</span>
                </label>
                <label class="exist-perm-toggle">
                  <ToggleSwitch v-model="perm.can.canAction"
                                :disabled="!!savingPerm[perm.id]"
                                @change="updateExistingPermission(perm)" />
                  <span><i class="pi pi-bolt" /> Azione</span>
                </label>
              </div>

            </div>
          </div>
        </div>

        <Divider />

        <!-- ── Sezione: AuthCode ─────────────────────────────────────────── -->
        <div class="form-section">
          <div class="field">
            <label class="field__label">
              Risorsa / Codice Autorizzazione <span class="required">*</span>
            </label>
            <Select v-model="form.authCode"
                    :options="availableAuthCodes"
                    optionLabel="label"
                    optionValue="code"
                    placeholder="Seleziona una risorsa..."
                    class="field__input"
                    showClear
                    filter />
            <small class="field__hint">Solo i codici non ancora assegnati.</small>
          </div>
        </div>

        <!-- ── Sezione: Permessi ─────────────────────────────────────────── -->
        <div class="perm-section">
          <div class="perm-section__header">
            <span class="perm-section__label">Permessi da assegnare</span>
            <button class="btn-all-toggle" @click="toggleAllForm">
              {{ isAllFormSelected ? 'Deseleziona tutto' : 'Seleziona tutto' }}
            </button>
          </div>
          <div class="perm-grid">

            <div class="perm-item">
              <ToggleSwitch v-model="form.can.canView" inputId="canView" />
              <label for="canView" class="perm-label">
                <i class="pi pi-eye" /> Visualizza
              </label>
            </div>

            <div class="perm-item">
              <ToggleSwitch v-model="form.can.canCreate" inputId="canCreate" />
              <label for="canCreate" class="perm-label">
                <i class="pi pi-plus-circle" /> Crea
              </label>
            </div>

            <div class="perm-item">
              <ToggleSwitch v-model="form.can.canModify" inputId="canModify" />
              <label for="canModify" class="perm-label">
                <i class="pi pi-pencil" /> Modifica
              </label>
            </div>

            <div class="perm-item">
              <ToggleSwitch v-model="form.can.canDelete" inputId="canDelete" />
              <label for="canDelete" class="perm-label">
                <i class="pi pi-trash" /> Elimina
              </label>
            </div>

            <div class="perm-item">
              <ToggleSwitch v-model="form.can.canAction" inputId="canAction" />
              <label for="canAction" class="perm-label">
                <i class="pi pi-bolt" /> Azione
              </label>
            </div>

          </div>
        </div>

        <!-- ── Pulsante aggiungi ─────────────────────────────────────────── -->
        <Button label="Aggiungi Permesso"
                icon="pi pi-plus"
                class="btn-primary btn-add"
                :loading="adding"
                :disabled="!isFormValid || adding"
                @click="addPermission" />

      </div>

      <template #footer>
        <Button label="Chiudi" icon="pi pi-times" class="btn-ghost" @click="closeDialog" />
      </template>

    </Dialog>

  </div>
</template>

<script setup>
import { ref, computed, reactive, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'

import DataTable   from 'primevue/datatable'
import Column      from 'primevue/column'
import Button      from 'primevue/button'
import Select      from 'primevue/select'
import ToggleSwitch from 'primevue/toggleswitch'
import Dialog      from 'primevue/dialog'
import Divider     from 'primevue/divider'

import { rolesAuthApi, groupsAuthApi, authorizationsApi } from '@/services/authorizationService'

const toast = useToast()

// ─── STATE ───────────────────────────────────────────────────────────────────

const activeTab     = ref('roles')

const roles         = ref([])
const groups        = ref([])
const authCodes     = ref([])
const loadingRoles  = ref(false)
const loadingGroups = ref(false)
const roleSearch    = ref('')
const groupSearch   = ref('')

// Dialog
const dialogVisible      = ref(false)
const dialogItem         = ref(null)
const dialogType         = ref('role')       // 'role' | 'group'
const adding             = ref(false)
const deleting            = reactive({})     // { [assignmentId]: true }
const savingPerm          = reactive({})     // { [permId]: true } — salvataggio inline
const existingPermissions = ref([])          // permessi assegnati (caricati dall'API + aggiunti)
const loadingExisting     = ref(false)

// Form
const form = reactive({
  authCode: null,
  can: {
    canView:   false,
    canCreate: false,
    canModify: false,
    canDelete: false,
    canAction: false,
  },
})

// ─── COMPUTED ────────────────────────────────────────────────────────────────

const filteredRoles = computed(() => {
  if (!roleSearch.value) return roles.value
  const q = roleSearch.value.toLowerCase()
  return roles.value.filter(r =>
    r.code?.toLowerCase().includes(q) || r.description?.toLowerCase().includes(q)
  )
})

const filteredGroups = computed(() => {
  if (!groupSearch.value) return groups.value
  const q = groupSearch.value.toLowerCase()
  return groups.value.filter(g =>
    g.code?.toLowerCase().includes(q) || g.description?.toLowerCase().includes(q)
  )
})

const availableAuthCodes = computed(() => {
  const assigned = new Set(existingPermissions.value.map(p => p.authCode))
  return authCodes.value.filter(a => !assigned.has(a.code))
})

const isFormValid = computed(() => {
  if (!form.authCode) return false
  const c = form.can
  return c.canView || c.canCreate || c.canModify || c.canDelete || c.canAction
})

const isAllFormSelected = computed(() =>
  form.can.canView && form.can.canCreate && form.can.canModify && form.can.canDelete && form.can.canAction
)

// ─── LIFECYCLE ───────────────────────────────────────────────────────────────

onMounted(() => {
  loadRoles()
  loadGroups()
  loadAuthCodes()
})

// ─── METHODS ─────────────────────────────────────────────────────────────────

async function loadRoles() {
  loadingRoles.value = true
  try {
    const { data } = await rolesAuthApi.getByModule('DomuWeb')
    roles.value = Array.isArray(data) ? data : []
  } catch {
    roles.value = []
  } finally {
    loadingRoles.value = false
  }
}

async function loadGroups() {
  loadingGroups.value = true
  try {
    const { data } = await groupsAuthApi.getAll()
    // L'API restituisce sia gruppi che ruoli (GroupBase). Filtra solo i gruppi reali.
    groups.value = Array.isArray(data) ? data.filter(g => !g.isRole) : []
  } catch {
    groups.value = []
  } finally {
    loadingGroups.value = false
  }
}

async function loadAuthCodes() {
  try {
    const { data } = await authorizationsApi.getAll()
    // Normalizza: accetta { code, description } o { code, label } o stringhe
    authCodes.value = Array.isArray(data)
      ? data.map(a => typeof a === 'string'
          ? { code: a, label: a }
          : { code: a.code, label: a.description ?? a.label ?? a.code })
      : []
  } catch {
    authCodes.value = []
  }
}

function openDialog(item, type) {
  dialogItem.value          = item
  dialogType.value          = type
  existingPermissions.value = []
  resetForm()
  dialogVisible.value = true
  loadExistingPermissions(item)
}

function closeDialog() {
  dialogVisible.value       = false
  dialogItem.value          = null
  existingPermissions.value = []
  resetForm()
}

async function loadExistingPermissions(item) {
  loadingExisting.value = true
  try {
    // getAuthorizations funziona sia per ruoli che per gruppi (entrambi GroupBase)
    const { data } = await groupsAuthApi.getAuthorizations(item.id)
    existingPermissions.value = Array.isArray(data)
      ? data.map(p => ({
          ...p,
          can: p.can != null ? p.can : {
            canView:   p.canView   ?? false,
            canCreate: p.canCreate ?? false,
            canModify: p.canModify ?? false,
            canDelete: p.canDelete ?? false,
            canAction: p.canAction ?? false,
          },
        }))
      : []
  } catch {
    existingPermissions.value = []
  } finally {
    loadingExisting.value = false
  }
}

async function updateExistingPermission(perm) {
  if (!perm.id) return
  savingPerm[perm.id] = true
  try {
    const api = dialogType.value === 'role' ? rolesAuthApi : groupsAuthApi
    await api.updatePermission(perm.id, dialogItem.value.id, perm.authCode, perm.can)
  } catch {
    // Errore gestito dall'interceptor; ricarica per resettare i toggle allo stato reale
    await loadExistingPermissions(dialogItem.value)
  } finally {
    delete savingPerm[perm.id]
  }
}

function isAllSelected(can) {
  return can.canView && can.canCreate && can.canModify && can.canDelete && can.canAction
}

async function toggleAll(perm) {
  const selectAll = !isAllSelected(perm.can)
  perm.can.canView   = selectAll
  perm.can.canCreate = selectAll
  perm.can.canModify = selectAll
  perm.can.canDelete = selectAll
  perm.can.canAction = selectAll
  await updateExistingPermission(perm)
}

function toggleAllForm() {
  const selectAll = !isAllFormSelected.value
  form.can.canView   = selectAll
  form.can.canCreate = selectAll
  form.can.canModify = selectAll
  form.can.canDelete = selectAll
  form.can.canAction = selectAll
}

function resetForm() {
  form.authCode      = null
  form.can.canView   = false
  form.can.canCreate = false
  form.can.canModify = false
  form.can.canDelete = false
  form.can.canAction = false
}

async function addPermission() {
  const authCode = form.authCode
  if (!authCode || !isFormValid.value) return

  adding.value = true
  try {
    const api = dialogType.value === 'role' ? rolesAuthApi : groupsAuthApi
    await api.addPermission(
      dialogItem.value.id,
      authCode,
      { ...form.can }
    )

    // Ricarica la lista dall'API per avere dati certi e id corretti
    await loadExistingPermissions(dialogItem.value)

    toast.add({
      severity: 'success',
      summary:  'Permesso aggiunto',
      detail:   `"${authCode}" assegnato a ${dialogType.value === 'role' ? 'ruolo' : 'gruppo'} "${dialogItem.value.code}".`,
      life:     4000,
    })

    resetForm()
  } catch {
    // Gestito dall'interceptor in authApiClient
  } finally {
    adding.value = false
  }
}

async function deletePermission(perm, list) {
  deleting[perm.id] = true
  try {
    const api = dialogType.value === 'role' ? rolesAuthApi : groupsAuthApi
    await api.deletePermission(perm.id)

    const idx = list.findIndex(a => a.id === perm.id)
    if (idx > -1) list.splice(idx, 1)

    toast.add({
      severity: 'success',
      summary:  'Permesso rimosso',
      detail:   `Autorizzazione "${perm.authCode}" rimossa.`,
      life:     3000,
    })
  } catch {
    // Gestito dall'interceptor
  } finally {
    delete deleting[perm.id]
  }
}
</script>

<style scoped>
/* ── PAGE ─────────────────────────────────────────────────────────────────── */
.auth-page {
  display: flex;
  flex-direction: column;
  gap: 20px;
  padding: 28px 32px;
  min-height: 100%;
}

/* ── HEADER ──────────────────────────────────────────────────────────────── */
.page-header { display: flex; align-items: flex-start; }
.page-header__left { display: flex; flex-direction: column; gap: 4px; }
.page-title {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 22px;
  font-weight: 700;
  color: var(--text);
  margin: 0;
}
.page-title__icon { color: var(--accent); font-size: 20px; }
.page-subtitle { font-size: 13px; color: var(--text-dim); }

/* ── TAB BAR ─────────────────────────────────────────────────────────────── */
.tab-bar {
  display: flex;
  gap: 4px;
  border-bottom: 1px solid var(--border);
  padding-bottom: 0;
}
.tab-btn {
  display: flex;
  align-items: center;
  gap: 7px;
  padding: 9px 18px;
  border: none;
  background: transparent;
  color: var(--text-dim);
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  border-bottom: 2px solid transparent;
  transition: color 0.15s, border-color 0.15s;
  font-family: inherit;
  border-radius: 6px 6px 0 0;
}
.tab-btn:hover { color: var(--text); background: var(--surface2); }
.tab-btn--active {
  color: var(--accent);
  border-bottom-color: var(--accent);
  font-weight: 600;
}
.tab-btn .pi { font-size: 13px; }

/* ── TAB CONTENT ─────────────────────────────────────────────────────────── */
.tab-content { display: flex; flex-direction: column; gap: 16px; }

/* ── FILTER BAR ──────────────────────────────────────────────────────────── */
.filter-bar {
  display: flex;
  align-items: center;
  gap: 10px;
  background: var(--surface2);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 12px 14px;
}
.filter-bar__search { flex: 1; }
.filter-input {
  width: 100%;
  background: var(--surface) !important;
  border-color: var(--border) !important;
  color: var(--text) !important;
  font-size: 13px !important;
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
.btn-manage {
  background: transparent !important;
  border-color: var(--border) !important;
  color: var(--text-dim) !important;
  font-size: 12px !important;
  padding: 5px 10px !important;
}
.btn-manage:hover { color: var(--accent) !important; border-color: var(--accent) !important; }

.btn-icon-sm {
  background: transparent !important;
  border: none !important;
  color: var(--text-dim) !important;
  width: 28px !important;
  height: 28px !important;
  padding: 0 !important;
  border-radius: 5px !important;
}
.btn-icon-sm:hover { background: var(--surface2) !important; }
.btn-icon-sm--danger:hover {
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
.code-badge {
  display: inline-block;
  padding: 2px 8px;
  background: rgba(52,211,153,0.08);
  border: 1px solid rgba(52,211,153,0.25);
  border-radius: 5px;
  font-family: 'JetBrains Mono', monospace;
  font-size: 12px;
  color: var(--accent);
}
.loading-spinner { font-size: 28px; color: var(--accent); }
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
  padding: 48px 0;
  color: var(--text-faint);
}
.empty-state__icon { font-size: 36px; opacity: 0.4; }

/* ── DIALOG ──────────────────────────────────────────────────────────────── */
.dialog-body {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

/* Existing permissions section */
.existing-section {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.existing-title {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  font-weight: 600;
  color: var(--text-dim);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
.existing-loading {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 12px;
  font-size: 12px;
  color: var(--text-faint);
}
.existing-loading .pi-spin { color: var(--accent); }
.existing-empty {
  display: flex;
  align-items: center;
  gap: 7px;
  padding: 10px 12px;
  font-size: 12px;
  color: var(--text-faint);
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 8px;
}
.perm-list {
  display: flex;
  flex-direction: column;
  gap: 5px;
}
.exist-item {
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding: 10px 12px;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 8px;
}
.exist-item__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.exist-item__actions {
  display: flex;
  align-items: center;
  gap: 4px;
}
.exist-item__saving {
  font-size: 13px;
  color: var(--accent);
}
.exist-item__perms {
  display: flex;
  flex-wrap: wrap;
  gap: 10px 20px;
}
.exist-perm-toggle {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  color: var(--text-dim);
  cursor: pointer;
  user-select: none;
}
.exist-perm-toggle span {
  display: flex;
  align-items: center;
  gap: 4px;
}
.exist-perm-toggle .pi { font-size: 11px; color: var(--accent); }

/* Form section */
.form-section { display: flex; flex-direction: column; gap: 12px; }
.field { display: flex; flex-direction: column; gap: 5px; }
.field__label {
  font-size: 11px;
  font-weight: 600;
  color: var(--text-dim);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
.field__input {
  width: 100% !important;
  background: var(--surface) !important;
  border-color: var(--border) !important;
  color: var(--text) !important;
  font-size: 13px !important;
}
.field__hint { font-size: 11px; color: var(--text-faint); }
.required { color: #ff6b6b; margin-left: 2px; }

/* Select/deselect all */
.btn-all-toggle {
  background: transparent;
  border: none;
  cursor: pointer;
  font-size: 11px;
  color: var(--accent);
  padding: 2px 7px;
  border-radius: 4px;
  font-family: inherit;
  white-space: nowrap;
  transition: background 0.12s;
}
.btn-all-toggle:hover { background: rgba(52, 211, 153, 0.08); }
.btn-all-toggle:disabled { opacity: 0.4; cursor: not-allowed; }

/* Permission toggles */
.perm-section { display: flex; flex-direction: column; gap: 10px; }
.perm-section__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.perm-section__label {
  font-size: 11px;
  font-weight: 600;
  color: var(--text-dim);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
.perm-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 14px;
  padding: 14px;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 8px;
}
.perm-item {
  display: flex;
  align-items: center;
  gap: 8px;
  min-width: 120px;
}
.perm-label {
  display: flex;
  align-items: center;
  gap: 5px;
  font-size: 13px;
  color: var(--text-dim);
  cursor: pointer;
}
.perm-label .pi { font-size: 13px; color: var(--accent); }

/* Add button */
.btn-add { align-self: flex-start; }

.session-item__left {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 130px;
}
.session-item__code {
  font-family: 'JetBrains Mono', monospace;
  font-size: 12px;
  font-weight: 600;
  color: var(--accent);
}
.session-item__id {
  font-size: 10px;
  color: var(--text-faint);
  font-family: 'JetBrains Mono', monospace;
}
.session-item__perms {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
  flex: 1;
}
.perm-tag { font-size: 10px !important; padding: 1px 6px !important; }

/* ── RESPONSIVE ──────────────────────────────────────────────────────────── */
@media (max-width: 768px) {
  .auth-page { padding: 16px; }
  .perm-grid { flex-direction: column; }
}
</style>
