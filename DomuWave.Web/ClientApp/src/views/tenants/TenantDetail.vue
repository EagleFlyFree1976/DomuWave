<template>
  <div class="tenant-detail-page">

    <!-- ── BREADCRUMB ─────────────────────────────────────────────────── -->
    <div class="breadcrumb">
      <span class="breadcrumb__link" @click="goBack">
        <i class="pi pi-arrow-left" /> Tenant
      </span>
      <span class="breadcrumb__sep">›</span>
      <span class="breadcrumb__current">{{ pageTitle }}</span>
    </div>

    <!-- ── HEADER ─────────────────────────────────────────────────────── -->
    <div class="page-header">
      <div>
        <h1 class="page-title">{{ pageTitle }}</h1>
        <span v-if="!isNew" class="page-id">ID: {{ store.currentTenant?.id }}</span>
      </div>
      <div class="page-header__actions">
        <Button
          label="Annulla"
          icon="pi pi-times"
          class="btn-ghost"
          :disabled="store.saving"
          @click="goBack"
        />
        <Button
          :label="store.saving ? 'Salvataggio...' : 'Salva'"
          icon="pi pi-check"
          class="btn-primary"
          :loading="store.saving"
          :disabled="store.saving || store.loading"
          @click="handleSave"
        />
      </div>
    </div>

    <!-- ── SPINNER CARICAMENTO ─────────────────────────────────────────── -->
    <div v-if="store.loading" class="loading-state">
      <i class="pi pi-spinner pi-spin loading-state__icon" />
      <span>Caricamento in corso...</span>
    </div>

    <!-- ── FORM ──────────────────────────────────────────────────────────── -->
    <template v-else-if="store.currentTenant">
      <div class="form-grid">

        <!-- ── CARD: Informazioni principali ─────────────────────────── -->
        <div class="form-card form-card--main">
          <div class="form-card__header">
            <i class="pi pi-info-circle" />
            <span>Informazioni principali</span>
          </div>
          <div class="form-card__body">

            <!-- Nome -->
            <div class="field" :class="{ 'field--error': v$.name.$error }">
              <label class="field__label" for="name">
                Nome <span class="required">*</span>
              </label>
              <InputText
                id="name"
                v-model="store.currentTenant.name"
                class="field__input"
                placeholder="Nome del tenant"
                maxlength="200"
                @blur="v$.name.$touch()"
              />
              <small v-if="v$.name.$error" class="field__error">
                {{ v$.name.$errors[0].$message }}
              </small>
            </div>

            <!-- Codice -->
            <div class="field" :class="{ 'field--error': v$.code.$error }">
              <label class="field__label" for="code">
                Codice <span class="required">*</span>
              </label>
              <InputText
                id="code"
                v-model="store.currentTenant.code"
                class="field__input field__input--mono"
                placeholder="Codice univoco (es. COND_ROMA_01)"
                maxlength="50"
                @blur="v$.code.$touch()"
                @input="store.currentTenant.code = store.currentTenant.code?.toUpperCase()"
              />
              <small class="field__hint">
                Identificativo univoco. Usato internamente per riferimento rapido.
              </small>
              <small v-if="v$.code.$error" class="field__error">
                {{ v$.code.$errors[0].$message }}
              </small>
            </div>

          </div>
        </div>

        <!-- ── CARD: Configurazione ───────────────────────────────────── -->
        <div class="form-card form-card--settings">
          <div class="form-card__header">
            <i class="pi pi-cog" />
            <span>Configurazione</span>
          </div>
          <div class="form-card__body">

            <!-- Stato -->
            <div class="field field--inline">
              <label class="field__label" for="isActive">Stato attivo</label>
              <div class="toggle-wrapper">
                <ToggleSwitch
                  id="isActive"
                  v-model="store.currentTenant.isActive"
                  class="tenant-toggle"
                />
                <span class="toggle-label" :class="store.currentTenant.isActive ? 'toggle-label--on' : 'toggle-label--off'">
                  {{ store.currentTenant.isActive ? 'Attivo' : 'Inattivo' }}
                </span>
              </div>
              <small class="field__hint">
                I tenant inattivi non possono accedere alla piattaforma.
              </small>
            </div>

          </div>
        </div>

        <!-- ── CARD: Audit trail (solo in modifica) ───────────────────── -->
        <div v-if="!isNew" class="form-card form-card--audit">
          <div class="form-card__header">
            <i class="pi pi-history" />
            <span>Audit</span>
          </div>
          <div class="form-card__body">
            <div class="audit-grid">
              <div class="audit-item">
                <span class="audit-item__label">Creato da</span>
                <span class="audit-item__value">{{ store.currentTenant.createdByFullName || '—' }}</span>
                <span class="audit-item__label">Il</span>
                <span class="audit-item__value">{{ store.currentTenant.creationDate || '—' }}</span>
              </div>
              <div class="audit-item">
                <span class="audit-item__label">Ultima modifica</span>
                <span class="audit-item__value">{{ store.currentTenant.lastUpdatedByFullName || '—' }}</span>
                <span class="audit-item__label">Il</span>
                <span class="audit-item__value">{{ store.currentTenant.lastUpdateDate || '—' }}</span>
              </div>
            </div>
          </div>
        </div>

      </div>

      <!-- ── CARD: Utenti del Tenant ─────────────────────────────────────── -->
      <div v-if="!isNew" class="form-card form-card--users">
        <div class="form-card__header">
          <i class="pi pi-users" />
          <span>Utenti del Tenant</span>
          <Button icon="pi pi-plus"
                  label="Aggiungi Utente"
                  class="btn-ghost btn-ghost--small ml-auto"
                  @click="openAddUserDialog" />
        </div>
        <div class="form-card__body">

          <div v-if="loadingUsers" class="tu-loading">
            <i class="pi pi-spinner pi-spin" /> Caricamento utenti...
          </div>

          <div v-else-if="tenantUsers.length === 0" class="tu-empty">
            <i class="pi pi-users" />
            <span>Nessun utente associato a questo tenant.</span>
          </div>

          <div v-else class="tu-list">
            <div v-for="ut in tenantUsers" :key="ut.userTenantId"
                 class="tu-item"
                 :class="{ 'tu-item--default': ut.isDefault }">

              <div class="tu-avatar">
                {{ userInitials(resolvedUser(ut.userId)) }}
              </div>

              <div class="tu-info">
                <span class="tu-name">{{ resolvedUserName(ut.userId) }}</span>
                <span class="tu-email">{{ resolvedUserEmail(ut.userId) }}</span>
              </div>

              <div class="tu-tags">
                <Tag v-if="ut.isDefault"
                     value="Amministratore di Condominio"
                     severity="success"
                     class="tu-default-tag" />
                <Tag v-if="!ut.isActive"
                     value="Inattivo"
                     severity="secondary"
                     class="tu-inactive-tag" />
              </div>

              <div class="tu-role">
                <Tag :value="formatRole(resolvedUser(ut.userId)?.roleCode)"
                     :severity="roleSeverity(resolvedUser(ut.userId)?.roleCode)"
                     class="tu-role-tag" />
              </div>

              <div class="tu-actions">
                <Button v-if="!ut.isDefault"
                        icon="pi pi-star"
                        class="btn-icon-sm"
                        v-tooltip="'Imposta come Amministratore di Condominio'"
                        @click="setDefaultUser(ut)" />
                <Button v-if="!ut.isDefault"
                        icon="pi pi-trash"
                        class="btn-icon-sm btn-icon-sm--danger"
                        v-tooltip="'Rimuovi dal tenant'"
                        @click="confirmRemoveUser(ut)" />
              </div>

            </div>
          </div>

        </div>
      </div>

    </template>

    <!-- ── ERRORE CARICAMENTO ─────────────────────────────────────────── -->
    <div v-else class="error-state">
      <i class="pi pi-exclamation-circle error-state__icon" />
      <span>Impossibile caricare il tenant</span>
      <Button label="Riprova" icon="pi pi-refresh" class="btn-ghost" size="small" @click="loadData" />
    </div>

    <!-- ── DIALOG: Aggiungi Utente al Tenant ──────────────────────────── -->
    <Dialog v-model:visible="showAddUserDialog"
            header="Associa Utente al Tenant"
            :modal="true"
            :draggable="false"
            style="width: 480px">

      <div class="dialog-body">
        <div class="field">
          <label class="field__label">Cerca Utente</label>
          <InputText v-model="userSearch"
                     class="field__input"
                     placeholder="Nome, cognome o email..."
                     @input="filterUsers" />
        </div>

        <div class="user-search-results">
          <div v-for="u in filteredAvailableUsers" :key="u.id"
               class="user-search-item"
               :class="{ 'user-search-item--selected': selectedUserToAdd?.id === u.id }"
               @click="selectedUserToAdd = u">
            <div class="user-search-avatar">{{ userInitials(u) }}</div>
            <div class="user-search-item__info">
              <span class="user-search-item__name">{{ fullName(u) }}</span>
              <span class="user-search-item__email">{{ u.email }}</span>
            </div>
            <i v-if="selectedUserToAdd?.id === u.id" class="pi pi-check check-icon" />
          </div>
          <div v-if="filteredAvailableUsers.length === 0" class="user-search-empty">
            Nessun utente disponibile
          </div>
        </div>

        <div class="field field--inline">
          <label class="field__label" for="addAsAdmin">Amministratore di Condominio</label>
          <div class="toggle-wrapper">
            <ToggleSwitch id="addAsAdmin" v-model="addAsDefault" />
            <span class="toggle-label" :class="addAsDefault ? 'toggle-label--on' : 'toggle-label--off'">
              {{ addAsDefault ? 'Sì' : 'No' }}
            </span>
          </div>
          <small class="field__hint">
            Imposta questo utente come amministratore predefinito per questo tenant.
          </small>
        </div>
      </div>

      <template #footer>
        <Button label="Annulla" icon="pi pi-times" class="btn-ghost"
                @click="showAddUserDialog = false" />
        <Button label="Associa" icon="pi pi-check" class="btn-primary"
                :disabled="!selectedUserToAdd"
                :loading="addingUser"
                @click="doAddUser" />
      </template>
    </Dialog>

    <ConfirmDialog class="domu-confirm" />

  </div>
</template>

<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useVuelidate } from '@vuelidate/core'
import { required, minLength, maxLength, helpers } from '@vuelidate/validators'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import { useTenantStore } from '@/stores/tenantStore'
import { useSessionStore } from '@/stores/sessionStore'
import { userTenantApi, userApi } from '@/services/userService'

import InputText from 'primevue/inputtext'
import Button from 'primevue/button'
import ToggleSwitch from 'primevue/toggleswitch'
import Tag from 'primevue/tag'
import Dialog from 'primevue/dialog'
import ConfirmDialog from 'primevue/confirmdialog'

// ─── PROPS (route props: true) ────────────────────────────────────────────────
// Con props: true il router inietta i params come props del componente.
// Dichiariamo 'id' come prop opzionale (undefined = nuovo tenant).
const props = defineProps({
  id: { type: String, default: null }
})

// ─── COMPOSABLES ────────────────────────────────────────────────────────────
const router  = useRouter()
const route   = useRoute()
const toast   = useToast()
const confirm = useConfirm()
const store   = useTenantStore()
const session = useSessionStore()

// ─── COMPUTED ────────────────────────────────────────────────────────────────
// Legge prima dalla prop (props: true), poi da route.params come fallback
const tenantId  = computed(() => props.id || route.params.id || null)
const isNew     = computed(() => !tenantId.value)
const pageTitle = computed(() => (isNew.value ? 'Nuovo Tenant' : 'Modifica Tenant'))

// ─── VALIDATION ──────────────────────────────────────────────────────────────
const rules = computed(() => ({
  name: {
    required: helpers.withMessage('Il nome è obbligatorio', required),
    minLength: helpers.withMessage('Minimo 2 caratteri', minLength(2)),
    maxLength: helpers.withMessage('Massimo 200 caratteri', maxLength(200)),
  },
  code: {
    required: helpers.withMessage('Il codice è obbligatorio', required),
    minLength: helpers.withMessage('Minimo 2 caratteri', minLength(2)),
    maxLength: helpers.withMessage('Massimo 50 caratteri', maxLength(50)),
    alphaNumUnderscore: helpers.withMessage(
      'Solo lettere, numeri e underscore',
      helpers.regex(/^[A-Z0-9_]+$/)
    ),
  },
}))

const formState = computed(() => ({
  name: store.currentTenant?.name ?? '',
  code: store.currentTenant?.code ?? '',
}))

const v$ = useVuelidate(rules, formState)

// ─── TENANT USERS STATE ───────────────────────────────────────────────────────
/** Associazioni utente-tenant per il tenant corrente */
const tenantUsers          = ref([])
/** Mappa userId → utente (AuthFlatUser) per la visualizzazione */
const usersMap             = ref({})
const loadingUsers         = ref(false)

// Dialog: Aggiungi Utente
const showAddUserDialog    = ref(false)
const userSearch           = ref('')
const allAvailableUsers    = ref([])
const filteredAvailableUsers = ref([])
const selectedUserToAdd    = ref(null)
const addAsDefault         = ref(false)
const addingUser           = ref(false)

// ─── LIFECYCLE ───────────────────────────────────────────────────────────────
onMounted(() => loadData())

// Osserva cambi del param id: gestisce navigazione tra dettagli
// senza smontare/rimontare il componente (es. da /tenants/1 a /tenants/2)
watch(tenantId, (newId, oldId) => {
  if (newId !== oldId) loadData()
})

// ─── METHODS ────────────────────────────────────────────────────────────────

async function loadData() {
  if (isNew.value) {
    store.initNew()
  } else {
    await store.fetchById(tenantId.value)
    await loadTenantUsers()
  }
}

async function loadTenantUsers() {
  loadingUsers.value = true
  try {
    const { data } = await userTenantApi.getByTenantId(tenantId.value)
    tenantUsers.value = Array.isArray(data) ? data : []

    // Recupera i dettagli utente per ogni userId trovato
    if (tenantUsers.value.length > 0) {
      const { data: allUsers } = await userApi.search({})
      const users = Array.isArray(allUsers) ? allUsers : (allUsers?.items ?? [])
      usersMap.value = Object.fromEntries(users.map(u => [String(u.id), u]))
    }
  } catch {
    tenantUsers.value = []
  } finally {
    loadingUsers.value = false
  }
}

async function handleSave() {
  // Valida
  const valid = await v$.value.$validate()
  if (!valid) return

  try {
    const saved = await store.save()
    toast.add({
      severity: 'success',
      summary: 'Salvato',
      detail: `Tenant "${saved.name}" salvato con successo.`,
      life: 3000,
    })

    // Aggiorna la lista dei tenant disponibili nel selector (visibile ai superadmin)
    // in modo che il widget TenantSelectorWidget mostri subito il nome aggiornato
    await session.loadTenants()

    // Dopo la creazione, naviga al dettaglio (aggiorna URL con il nuovo ID)
    if (isNew.value) {
      router.replace({ name: 'tenant-detail', params: { id: saved.id } })
    }
  } catch {
    // Il toast di errore è già mostrato dall'interceptor API
  }
}

function goBack() {
  router.push({ name: 'tenants' })
}

// ── Helpers utente ───────────────────────────────────────────────────────────

function resolvedUser(userId) {
  return usersMap.value[String(userId)] ?? null
}
function resolvedUserName(userId) {
  const u = resolvedUser(userId)
  if (!u) return `Utente #${userId}`
  return [u.firstName, u.lastName].filter(Boolean).join(' ') || u.name || `#${userId}`
}
function resolvedUserEmail(userId) {
  return resolvedUser(userId)?.email ?? ''
}
function userInitials(user) {
  if (!user) return '?'
  const fn = user.firstName?.[0] ?? ''
  const ln = user.lastName?.[0] ?? ''
  return (fn + ln).toUpperCase() || (user.name?.[0] ?? '?').toUpperCase()
}
function fullName(user) {
  return [user?.firstName, user?.lastName].filter(Boolean).join(' ') || user?.name || '—'
}
function formatRole(roleCode) {
  const map = { SuperAdmin: 'Super Admin', TenantAdministrator: 'Amministratore', Admin: 'Admin', User: 'Utente' }
  return map[roleCode] ?? roleCode ?? '—'
}
function roleSeverity(roleCode) {
  if (roleCode === 'SuperAdmin') return 'danger'
  if (roleCode === 'TenantAdministrator' || roleCode === 'Admin') return 'warn'
  return 'secondary'
}

// ── Dialog Aggiungi Utente ───────────────────────────────────────────────────

async function openAddUserDialog() {
  userSearch.value        = ''
  selectedUserToAdd.value = null
  addAsDefault.value      = false

  try {
    const { data } = await userApi.search({ isActive: true })
    const users = Array.isArray(data) ? data : (data?.items ?? [])
    const assigned = new Set(tenantUsers.value.map(ut => String(ut.userId)))
    allAvailableUsers.value     = users.filter(u => !assigned.has(String(u.id)))
    filteredAvailableUsers.value = allAvailableUsers.value
  } catch {
    allAvailableUsers.value      = []
    filteredAvailableUsers.value = []
  }

  showAddUserDialog.value = true
}

function filterUsers() {
  const q = userSearch.value.toLowerCase()
  filteredAvailableUsers.value = allAvailableUsers.value.filter(u =>
    fullName(u).toLowerCase().includes(q) || u.email?.toLowerCase().includes(q)
  )
}

async function doAddUser() {
  if (!selectedUserToAdd.value) return
  addingUser.value = true
  try {
    await userTenantApi.create({
      userId:    selectedUserToAdd.value.id,
      tenantId:  store.currentTenant.id,
      isDefault: addAsDefault.value,
      isActive:  true,
    })
    toast.add({
      severity: 'success',
      summary: 'Utente associato',
      detail: `"${fullName(selectedUserToAdd.value)}" associato al tenant.`,
      life: 3000,
    })
    showAddUserDialog.value = false
    await loadTenantUsers()
  } catch {
    // Gestito dall'interceptor
  } finally {
    addingUser.value = false
  }
}

async function setDefaultUser(ut) {
  try {
    await userTenantApi.setDefault(ut.userId, ut.userTenantId)
    toast.add({
      severity: 'success',
      summary: 'Amministratore impostato',
      detail: `"${resolvedUserName(ut.userId)}" impostato come Amministratore di Condominio.`,
      life: 3000,
    })
    await loadTenantUsers()
  } catch {
    // Gestito dall'interceptor
  }
}

function confirmRemoveUser(ut) {
  confirm.require({
    message: `Rimuovere "${resolvedUserName(ut.userId)}" dal tenant?`,
    header: 'Conferma rimozione',
    icon: 'pi pi-exclamation-triangle',
    acceptLabel: 'Rimuovi',
    rejectLabel: 'Annulla',
    acceptClass: 'p-button-danger',
    accept: () => doRemoveUser(ut),
  })
}

async function doRemoveUser(ut) {
  try {
    await userTenantApi.delete(ut.userTenantId)
    toast.add({
      severity: 'success',
      summary: 'Rimosso',
      detail: `Utente rimosso dal tenant.`,
      life: 3000,
    })
    await loadTenantUsers()
  } catch {
    // Gestito dall'interceptor
  }
}
</script>

<style scoped>
/* ── PAGE LAYOUT ─────────────────────────────────────────────────────────── */
.tenant-detail-page {
  display: flex;
  flex-direction: column;
  gap: 20px;
  padding: 28px 32px;
  max-width: 960px;
}

/* ── BREADCRUMB ──────────────────────────────────────────────────────────── */
.breadcrumb {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 12px;
  color: var(--text-faint);
}
.breadcrumb__link {
  cursor: pointer;
  color: var(--accent);
  display: flex;
  align-items: center;
  gap: 5px;
  transition: opacity 0.15s;
}
.breadcrumb__link:hover {
  opacity: 0.75;
}
.breadcrumb__sep {
  color: var(--border);
}
.breadcrumb__current {
  color: var(--text-dim);
}

/* ── HEADER ──────────────────────────────────────────────────────────────── */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
}
.page-title {
  font-size: 22px;
  font-weight: 700;
  color: var(--text);
  margin: 0;
}
.page-id {
  font-size: 11px;
  color: var(--text-faint);
  font-family: 'JetBrains Mono', monospace;
}
.page-header__actions {
  display: flex;
  gap: 8px;
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

/* ── FORM GRID ───────────────────────────────────────────────────────────── */
.form-grid {
  display: grid;
  grid-template-columns: 2fr 1fr;
  grid-template-rows: auto auto;
  gap: 16px;
}
.form-card--main {
  grid-column: 1;
  grid-row: 1;
}
.form-card--settings {
  grid-column: 2;
  grid-row: 1;
}
.form-card--audit {
  grid-column: 1 / -1;
  grid-row: 2;
}

/* ── FORM CARD ───────────────────────────────────────────────────────────── */
.form-card {
  background: var(--surface2);
  border: 1px solid var(--border);
  border-radius: 10px;
  overflow: hidden;
}
.form-card__header {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 16px;
  border-bottom: 1px solid var(--border);
  font-size: 13px;
  font-weight: 600;
  color: var(--text-dim);
  background: var(--surface);
}
.form-card__header i {
  color: var(--accent);
  font-size: 14px;
}
.form-card__body {
  padding: 20px 16px;
  display: flex;
  flex-direction: column;
  gap: 18px;
}

/* ── FIELDS ──────────────────────────────────────────────────────────────── */
.field {
  display: flex;
  flex-direction: column;
  gap: 5px;
}
.field--inline {
  gap: 8px;
}
.field__label {
  font-size: 12px;
  font-weight: 600;
  color: var(--text-dim);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
.required {
  color: #ff6b6b;
  margin-left: 2px;
}
.field__input {
  width: 100% !important;
  background: var(--surface) !important;
  border-color: var(--border) !important;
  color: var(--text) !important;
  font-size: 13px !important;
}
.field__input--mono {
  font-family: 'JetBrains Mono', monospace !important;
  letter-spacing: 0.5px !important;
}
.field--error .field__input {
  border-color: #ff6b6b !important;
}
.field__error {
  color: #ff6b6b;
  font-size: 11px;
}
.field__hint {
  color: var(--text-faint);
  font-size: 11px;
  line-height: 1.4;
}

/* ── TOGGLE ──────────────────────────────────────────────────────────────── */
.toggle-wrapper {
  display: flex;
  align-items: center;
  gap: 10px;
}
.toggle-label {
  font-size: 13px;
  font-weight: 500;
}
.toggle-label--on  { color: #5dca7e; }
.toggle-label--off { color: var(--text-faint); }

/* ── AUDIT ───────────────────────────────────────────────────────────────── */
.audit-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
}
.audit-item {
  display: flex;
  flex-direction: column;
  gap: 3px;
}
.audit-item__label {
  font-size: 11px;
  color: var(--text-faint);
  text-transform: uppercase;
  letter-spacing: 0.6px;
}
.audit-item__value {
  font-size: 13px;
  color: var(--text-dim);
}

/* ── LOADING / ERROR STATES ──────────────────────────────────────────────── */
.loading-state,
.error-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  padding: 64px 0;
  color: var(--text-faint);
}
.loading-state__icon {
  font-size: 36px;
  color: var(--accent);
}
.error-state__icon {
  font-size: 36px;
  color: #ff6b6b;
}

/* ── USERS SECTION ───────────────────────────────────────────────────────── */
.form-card--users { grid-column: 1 / -1; }

.btn-ghost--small {
  font-size: 12px !important;
  padding: 5px 10px !important;
}
.ml-auto { margin-left: auto; }

.tu-loading {
  display: flex;
  align-items: center;
  gap: 8px;
  color: var(--text-faint);
  font-size: 13px;
}
.tu-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 24px 0;
  color: var(--text-faint);
  font-size: 13px;
}
.tu-empty .pi { font-size: 28px; opacity: 0.4; }

.tu-list { display: flex; flex-direction: column; gap: 8px; }

.tu-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px 14px;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 8px;
}
.tu-item--default {
  border-color: rgba(52, 211, 153, 0.4);
  background: rgba(52, 211, 153, 0.04);
}
.tu-avatar {
  width: 32px;
  height: 32px;
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
.tu-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
  flex: 1;
  min-width: 0;
}
.tu-name {
  font-size: 13px;
  font-weight: 500;
  color: var(--text);
}
.tu-email {
  font-size: 11px;
  color: var(--text-faint);
}
.tu-tags { display: flex; gap: 4px; flex-wrap: wrap; }
.tu-default-tag { font-size: 10px !important; }
.tu-inactive-tag { font-size: 10px !important; }
.tu-role { display: flex; }
.tu-role-tag { font-size: 10px !important; }
.tu-actions { display: flex; gap: 4px; }

.btn-icon-sm {
  background: transparent !important;
  border: none !important;
  color: var(--text-dim) !important;
  width: 28px !important;
  height: 28px !important;
  padding: 0 !important;
  border-radius: 5px !important;
}
  .btn-icon-sm:hover {
    background: var(--surface2) !important;
    color: var(--accent) !important;
  }
.btn-icon-sm--danger:hover {
  background: rgba(255, 80, 80, 0.1) !important;
  color: #ff5555 !important;
}

/* ── DIALOG ──────────────────────────────────────────────────────────────── */
.dialog-body { display: flex; flex-direction: column; gap: 16px; padding: 4px 0; }

.user-search-results {
  display: flex;
  flex-direction: column;
  gap: 4px;
  max-height: 220px;
  overflow-y: auto;
  border: 1px solid var(--border);
  border-radius: 8px;
  padding: 4px;
}
.user-search-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 10px;
  border-radius: 6px;
  cursor: pointer;
  transition: background 0.12s;
  color: var(--text);
}
  .user-search-item:hover { background: var(--surface); }
  .user-search-item--selected {
    background: rgba(52, 211, 153, 0.08);
    border: 1px solid rgba(52, 211, 153, 0.3);
  }
.user-search-avatar {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background: linear-gradient(135deg, var(--accent), #059669);
  color: #000;
  font-size: 10px;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.user-search-item__info { display: flex; flex-direction: column; flex: 1; }
.user-search-item__name { font-size: 13px; font-weight: 500; }
.user-search-item__email { font-size: 11px; color: var(--text-faint); }
.check-icon { color: var(--accent); margin-left: auto; }
.user-search-empty { padding: 20px; text-align: center; color: var(--text-faint); font-size: 13px; }

/* ── RESPONSIVE ──────────────────────────────────────────────────────────── */
@media (max-width: 768px) {
  .tenant-detail-page {
    padding: 16px;
  }
  .form-grid {
    grid-template-columns: 1fr;
  }
  .form-card--main,
  .form-card--settings,
  .form-card--audit {
    grid-column: 1;
    grid-row: auto;
  }
  .page-header {
    flex-direction: column;
    align-items: flex-start;
  }
  .audit-grid {
    grid-template-columns: 1fr;
  }
}
</style>
