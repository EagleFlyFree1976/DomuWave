<template>
  <div class="user-detail-page">

    <!-- ── BREADCRUMB ─────────────────────────────────────────────────── -->
    <div class="breadcrumb">
      <span class="breadcrumb__link" @click="goBack">
        <i class="pi pi-arrow-left" /> Utenti
      </span>
      <span class="breadcrumb__sep">›</span>
      <span class="breadcrumb__current">{{ pageTitle }}</span>
    </div>

    <!-- ── HEADER ─────────────────────────────────────────────────────── -->
    <div class="page-header">
      <div>
        <h1 class="page-title">{{ pageTitle }}</h1>
        <span v-if="!isNew" class="page-id">ID: {{ store.currentUser?.id }}</span>
      </div>
      <div class="page-header__actions">
        <Button label="Annulla" icon="pi pi-times" class="btn-ghost"
                :disabled="store.saving" @click="goBack" />
        <Button :label="store.saving ? 'Salvataggio...' : 'Salva'"
                icon="pi pi-check" class="btn-primary"
                :loading="store.saving"
                :disabled="store.saving || store.loading || isReadOnly"
                @click="handleSave" />
      </div>
    </div>

    <!-- ── SPINNER ─────────────────────────────────────────────────────── -->
    <div v-if="store.loading" class="loading-state">
      <i class="pi pi-spinner pi-spin loading-state__icon" />
      <span>Caricamento in corso...</span>
    </div>

    <!-- ── FORM ──────────────────────────────────────────────────────────── -->
    <template v-else-if="store.currentUser">

      <!-- Banner sola lettura per TenantAdmin su ruoli non gestibili -->
      <div v-if="isReadOnly" class="readonly-banner">
        <i class="pi pi-lock" />
        <span>Questo utente ha un ruolo che non puoi modificare. Visualizzazione in sola lettura.</span>
      </div>

      <!-- ── CARD: Informazioni principali ──────────────────────────────── -->
      <div class="form-grid">

        <div class="form-card form-card--main">
          <div class="form-card__header">
            <i class="pi pi-user" />
            <span>Informazioni principali</span>
          </div>
          <div class="form-card__body">

            <!-- Nome -->
            <div class="field" :class="{ 'field--error': v$.firstName.$error }">
              <label class="field__label" for="firstName">
                Nome <span class="required">*</span>
              </label>
              <InputText id="firstName" v-model="store.currentUser.firstName"
                         class="field__input" placeholder="Nome"
                         maxlength="100"
                         :disabled="isReadOnly"
                         @blur="v$.firstName.$touch()" />
              <small v-if="v$.firstName.$error" class="field__error">
                {{ v$.firstName.$errors[0].$message }}
              </small>
            </div>

            <!-- Cognome -->
            <div class="field" :class="{ 'field--error': v$.lastName.$error }">
              <label class="field__label" for="lastName">
                Cognome <span class="required">*</span>
              </label>
              <InputText id="lastName" v-model="store.currentUser.lastName"
                         class="field__input" placeholder="Cognome"
                         maxlength="100"
                         :disabled="isReadOnly"
                         @blur="v$.lastName.$touch()" />
              <small v-if="v$.lastName.$error" class="field__error">
                {{ v$.lastName.$errors[0].$message }}
              </small>
            </div>

            <!-- Email -->
            <div class="field" :class="{ 'field--error': v$.email.$error }">
              <label class="field__label" for="email">
                Email <span class="required">*</span>
              </label>
              <InputText id="email" v-model="store.currentUser.email"
                         type="email"
                         class="field__input" placeholder="utente@esempio.it"
                         maxlength="200"
                         :disabled="isReadOnly"
                         @blur="v$.email.$touch()" />
              <small v-if="v$.email.$error" class="field__error">
                {{ v$.email.$errors[0].$message }}
              </small>
            </div>

            <!-- Password (solo in creazione) -->
            <div v-if="isNew" class="field" :class="{ 'field--error': v$.password.$error }">
              <label class="field__label" for="password">
                Password <span class="required">*</span>
              </label>
              <Password id="password"
                        v-model="store.currentUser.password"
                        class="field__password"
                        placeholder="Password iniziale"
                        :feedback="true"
                        toggleMask
                        @blur="v$.password.$touch()" />
              <small class="field__hint">
                L'utente potrà cambiarla tramite reset password.
              </small>
              <small v-if="v$.password.$error" class="field__error">
                {{ v$.password.$errors[0].$message }}
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

            <!-- Ruolo -->
            <div class="field" :class="{ 'field--error': v$.roleCode.$error }">
              <label class="field__label" for="roleCode">
                Ruolo <span class="required">*</span>

             

              </label>
              <Select id="roleCode"
                      v-model="store.currentUser.roleCode"
                      :options="availableRoles"
                      optionLabel="description"
                      optionValue="code"
                      placeholder="Seleziona un ruolo..."
                      class="field__input"
                      :loading="loadingRoles"
                      :disabled="isReadOnly"
                      @blur="v$.roleCode.$touch()" />
              <small v-if="v$.roleCode.$error" class="field__error">
                {{ v$.roleCode.$errors[0].$message }}
              </small>
            </div>

            <!-- Stato (sola lettura in creazione, editabile successivamente) -->
            <div v-if="!isNew" class="field field--inline">
              <label class="field__label" for="isActive">Stato attivo</label>
              <div class="toggle-wrapper">
                <ToggleSwitch id="isActive" v-model="store.currentUser.isActive"
                              :disabled="isSelf"
                              v-tooltip="isSelf ? 'Non puoi disabilitare il tuo account' : null" />
                <span class="toggle-label"
                      :class="store.currentUser.isActive ? 'toggle-label--on' : 'toggle-label--off'">
                  {{ store.currentUser.isActive ? 'Attivo' : 'Inattivo' }}
                </span>
              </div>
            </div>

            <!-- Reset password (solo in modifica) -->
            <div v-if="!isNew" class="field">
              <label class="field__label">Password</label>
              <Button label="Invia reset password"
                      icon="pi pi-key"
                      class="btn-ghost btn-ghost--small"
                      :loading="resettingPassword"
                      @click="handleResetPassword" />
              <small class="field__hint">
                Invia un'email all'utente con il link per impostare una nuova password.
              </small>
            </div>

          </div>
        </div>

      </div>

      <!-- ── SEZIONE TENANT (solo SuperAdmin su utenti esistenti) ─────── -->
      <div v-if="!isNew && session.isSuperAdmin" class="form-card form-card--tenants">
        <div class="form-card__header">
          <i class="pi pi-building" />
          <span>Tenant associati</span>
          <Button icon="pi pi-plus"
                  label="Aggiungi Tenant"
                  class="btn-ghost btn-ghost--small ml-auto"
                  @click="openAddTenantDialog" />
        </div>
        <div class="form-card__body">

          <div v-if="loadingTenants" class="tenant-loading">
            <i class="pi pi-spinner pi-spin" /> Caricamento tenant...
          </div>

          <div v-else-if="userTenants.length === 0" class="tenant-empty">
            <i class="pi pi-inbox" />
            <span>Nessun tenant associato a questo utente.</span>
          </div>

          <div v-else class="tenant-list">
            <div v-for="ut in userTenants" :key="ut.userTenantId" class="tenant-item"
                 :class="{ 'tenant-item--default': ut.isDefault }">

              <div class="tenant-item__badge">
                <i class="pi pi-building" />
              </div>

              <div class="tenant-item__info">
                <span class="tenant-item__name">{{ ut.tenantName }}</span>
                <span class="tenant-item__code">{{ ut.tenantCode }}</span>
              </div>

              <div class="tenant-item__tags">
                <Tag v-if="ut.isDefault"
                     value="Amministratore di Condominio"
                     severity="success"
                     class="default-tag" />
                <Tag v-if="!ut.isActive"
                     value="Inattivo"
                     severity="secondary"
                     class="inactive-tag" />
              </div>

              <div class="tenant-item__actions">
                <Button v-if="!ut.isDefault"
                        icon="pi pi-star"
                        class="btn-icon-sm"
                        v-tooltip="'Imposta come Amministratore di Condominio'"
                        @click="setDefaultTenant(ut)" />
                <Button icon="pi pi-trash"
                        class="btn-icon-sm btn-icon-sm--danger"
                        v-tooltip="'Rimuovi dal tenant'"
                        @click="confirmRemoveTenant(ut)" />
              </div>

            </div>
          </div>

        </div>
      </div>

    </template>

    <!-- ── ERRORE ──────────────────────────────────────────────────────── -->
    <div v-else-if="!store.loading" class="error-state">
      <i class="pi pi-exclamation-circle error-state__icon" />
      <span>Impossibile caricare l'utente</span>
      <Button label="Riprova" icon="pi pi-refresh" class="btn-ghost" size="small" @click="loadData" />
    </div>

    <!-- ── DIALOG: Aggiungi Tenant ──────────────────────────────────────── -->
    <Dialog v-model:visible="showAddTenantDialog"
            header="Associa Tenant"
            :modal="true"
            :draggable="false"
            class="add-tenant-dialog"
            style="width: 480px">

      <div class="dialog-body">
        <div class="field">
          <label class="field__label">Cerca Tenant</label>
          <InputText v-model="tenantSearch"
                     class="field__input"
                     placeholder="Nome o codice..."
                     @input="filterTenants" />
        </div>

        <div class="tenant-search-results">
          <div v-for="t in filteredAvailableTenants" :key="t.id"
               class="tenant-search-item"
               :class="{ 'tenant-search-item--selected': selectedTenantToAdd?.id === t.id }"
               @click="selectedTenantToAdd = t">
            <i class="pi pi-building" />
            <div class="tenant-search-item__info">
              <span class="tenant-search-item__name">{{ t.name }}</span>
              <span class="tenant-search-item__code">{{ t.code }}</span>
            </div>
            <i v-if="selectedTenantToAdd?.id === t.id" class="pi pi-check check-icon" />
          </div>
          <div v-if="filteredAvailableTenants.length === 0" class="tenant-search-empty">
            Nessun tenant disponibile
          </div>
        </div>

        <div class="field field--inline">
          <label class="field__label" for="addAsDefault">Amministratore di Condominio</label>
          <div class="toggle-wrapper">
            <ToggleSwitch id="addAsDefault" v-model="addAsDefault" />
            <span class="toggle-label" :class="addAsDefault ? 'toggle-label--on' : 'toggle-label--off'">
              {{ addAsDefault ? 'Sì' : 'No' }}
            </span>
          </div>
          <small class="field__hint">
            Imposta questo utente come amministratore predefinito del tenant selezionato.
          </small>
        </div>
      </div>

      <template #footer>
        <Button label="Annulla" icon="pi pi-times" class="btn-ghost"
                @click="showAddTenantDialog = false" />
        <Button label="Associa" icon="pi pi-check" class="btn-primary"
                :disabled="!selectedTenantToAdd"
                :loading="addingTenant"
                @click="doAddTenant" />
      </template>
    </Dialog>

    <ConfirmDialog class="domu-confirm" />

  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useVuelidate } from '@vuelidate/core'
import { required, minLength, maxLength, email as emailValidator, helpers } from '@vuelidate/validators'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import { useUserStore } from '@/stores/userStore'
import { useAuthStore } from '@/stores/authStore'
import { useSessionStore } from '@/stores/sessionStore'
import { userTenantApi, rolesApi } from '@/services/userService'
import { tenantApi } from '@/services/tenantService'

import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Button from 'primevue/button'
import Select from 'primevue/select'
import ToggleSwitch from 'primevue/toggleswitch'
import Tag from 'primevue/tag'
import Dialog from 'primevue/dialog'
import ConfirmDialog from 'primevue/confirmdialog'

// ─── PROPS ──────────────────────────────────────────────────────────────────
const props = defineProps({
  id: { type: String, default: null }
})

// ─── COMPOSABLES ────────────────────────────────────────────────────────────
const router  = useRouter()
const route   = useRoute()
const toast   = useToast()
const confirm = useConfirm()
const store   = useUserStore()
const auth    = useAuthStore()
const session = useSessionStore()

// Ruoli gestibili da TenantAdmin
const TENANT_ADMIN_ROLES = ['CLB', 'Condomino']

const isSelf = computed(() => {
  if (!auth.user || !store.currentUser) return false
  if (auth.user.username && store.currentUser.email)
    return auth.user.username === store.currentUser.email
  return String(auth.user.id) === String(store.currentUser.id)
})

/**
 * TenantAdmin può modificare solo utenti con ruolo in TENANT_ADMIN_ROLES.
 * SuperAdmin può sempre modificare.
 */
const isReadOnly = computed(() => {
  if (session.isSuperAdmin) return false
  if (isNew.value) return false   // creazione: sempre editabile
  const rc = store.currentUser?.roleCode ?? store.currentUser?.role ?? ''
  return !TENANT_ADMIN_ROLES.includes(rc)
})

// ─── COMPUTED ────────────────────────────────────────────────────────────────
const userId    = computed(() => props.id || route.params.id || null)
const isNew     = computed(() => !userId.value)
const pageTitle = computed(() => (isNew.value ? 'Nuovo Utente' : 'Modifica Utente'))

// ─── VALIDATION ──────────────────────────────────────────────────────────────
const rules = computed(() => ({
  firstName: {
    required:  helpers.withMessage('Il nome è obbligatorio', required),
    maxLength: helpers.withMessage('Massimo 100 caratteri', maxLength(100)),
  },
  lastName: {
    required:  helpers.withMessage('Il cognome è obbligatorio', required),
    maxLength: helpers.withMessage('Massimo 100 caratteri', maxLength(100)),
  },
  email: {
    required:  helpers.withMessage("L'email è obbligatoria", required),
    email:     helpers.withMessage('Formato email non valido', emailValidator),
    maxLength: helpers.withMessage('Massimo 200 caratteri', maxLength(200)),
  },
  roleCode: {
    required: helpers.withMessage('Il ruolo è obbligatorio', required),
  },
  // Password obbligatoria solo in creazione
  ...(isNew.value ? {
    password: {
      required:  helpers.withMessage('La password è obbligatoria', required),
      minLength: helpers.withMessage('Minimo 8 caratteri', minLength(8)),
    },
  } : {}),
}))

const formState = computed(() => ({
  firstName: store.currentUser?.firstName ?? '',
  lastName:  store.currentUser?.lastName  ?? '',
  email:     store.currentUser?.email     ?? '',
  password:  store.currentUser?.password  ?? '',
  roleCode:  store.currentUser?.roleCode  ?? '',
}))

const v$ = useVuelidate(rules, formState)

// ─── ROLES STATE ─────────────────────────────────────────────────────────────
const availableRoles  = ref([])
const loadingRoles    = ref(false)

// ─── TENANT STATE ────────────────────────────────────────────────────────────
const userTenants           = ref([])
const loadingTenants        = ref(false)
const resettingPassword     = ref(false)

// Dialog: Aggiungi Tenant
const showAddTenantDialog   = ref(false)
const tenantSearch          = ref('')
const allAvailableTenants   = ref([])
const filteredAvailableTenants = ref([])
const selectedTenantToAdd   = ref(null)
const addAsDefault          = ref(false)
const addingTenant          = ref(false)

// ─── LIFECYCLE ───────────────────────────────────────────────────────────────
onMounted(() => loadData())
watch(userId, (newId, oldId) => { if (newId !== oldId) loadData() })

// ─── METHODS ────────────────────────────────────────────────────────────────

async function loadData() {
  await loadRoles()
  if (isNew.value) {
    store.initNew()
  } else {
    await store.fetchById(userId.value)
    await loadUserTenants()
  }
}

async function loadRoles() {
  loadingRoles.value = true
  try {
    const { data } = await rolesApi.getByModule('DomuWeb')
    const allRoles = Array.isArray(data) ? data : []
    // TenantAdmin può assegnare solo Collaboratore e Condomino
    availableRoles.value = session.isTenantAdmin
      ? allRoles.filter(r => TENANT_ADMIN_ROLES.includes(r.code))
      : allRoles
  } catch {
    availableRoles.value = []
  } finally {
    loadingRoles.value = false
  }
}

async function loadUserTenants() {
  loadingTenants.value = true
  try {
    const { data } = await userTenantApi.getByUserId(userId.value)
    userTenants.value = Array.isArray(data) ? data : []
  } catch {
    userTenants.value = []
  } finally {
    loadingTenants.value = false
  }
}

async function handleSave() {
  const valid = await v$.value.$validate()
  if (!valid) return

  const wasNew = isNew.value
  try {
    const saved = await store.save()
    toast.add({
      severity: 'success',
      summary: 'Salvato',
      detail: `Utente "${saved?.firstName ?? ''} ${saved?.lastName ?? ''}" salvato con successo.`,
      life: 3000,
    })
    if (wasNew) {
      router.replace({ name: 'user-detail', params: { id: saved.id } })
    }
  } catch {
    // Il toast di errore è già mostrato dall'interceptor API
  }
}

async function handleResetPassword() {
  resettingPassword.value = true
  try {
    await store.resetPassword(userId.value)
    toast.add({
      severity: 'success',
      summary: 'Email inviata',
      detail: 'Email di reset password inviata con successo.',
      life: 4000,
    })
  } catch {
    // Gestito dall'interceptor
  } finally {
    resettingPassword.value = false
  }
}

// ── Tenant management ────────────────────────────────────────────────────────

async function openAddTenantDialog() {
  tenantSearch.value        = ''
  selectedTenantToAdd.value = null
  addAsDefault.value        = false

  try {
    const { data } = await tenantApi.getAll()
    const assigned = new Set(userTenants.value.map(ut => ut.tenantId?.toString()))
    allAvailableTenants.value = (Array.isArray(data) ? data : [])
      .filter(t => t.isActive && !assigned.has(t.id?.toString()))
    filteredAvailableTenants.value = allAvailableTenants.value
  } catch {
    allAvailableTenants.value      = []
    filteredAvailableTenants.value = []
  }

  showAddTenantDialog.value = true
}

function filterTenants() {
  const q = tenantSearch.value.toLowerCase()
  filteredAvailableTenants.value = allAvailableTenants.value.filter(
    t => t.name?.toLowerCase().includes(q) || t.code?.toLowerCase().includes(q)
  )
}

async function doAddTenant() {
  if (!selectedTenantToAdd.value) return
  addingTenant.value = true
  try {
    await userTenantApi.create({
      userId:    Number(userId.value),
      tenantId:  selectedTenantToAdd.value.id,
      isDefault: addAsDefault.value,
      isActive:  true,
    })
    toast.add({
      severity: 'success',
      summary: 'Tenant associato',
      detail: `Tenant "${selectedTenantToAdd.value.name}" associato all'utente.`,
      life: 3000,
    })
    showAddTenantDialog.value = false
    await loadUserTenants()
  } catch {
    // Gestito dall'interceptor
  } finally {
    addingTenant.value = false
  }
}

async function setDefaultTenant(ut) {
  try {
    await userTenantApi.setDefault(userId.value, ut.userTenantId)
    toast.add({
      severity: 'success',
      summary: 'Amministratore impostato',
      detail: `"${ut.tenantName}" impostato come tenant di default.`,
      life: 3000,
    })
    await loadUserTenants()
  } catch {
    // Gestito dall'interceptor
  }
}

function confirmRemoveTenant(ut) {
  confirm.require({
    message: `Rimuovere l'utente dal tenant "${ut.tenantName}"?`,
    header: 'Conferma rimozione',
    icon: 'pi pi-exclamation-triangle',
    acceptLabel: 'Rimuovi',
    rejectLabel: 'Annulla',
    acceptClass: 'p-button-danger',
    accept: () => doRemoveTenant(ut),
  })
}

async function doRemoveTenant(ut) {
  try {
    await userTenantApi.delete(ut.userTenantId)
    toast.add({
      severity: 'success',
      summary: 'Rimosso',
      detail: `Utente rimosso dal tenant "${ut.tenantName}".`,
      life: 3000,
    })
    await loadUserTenants()
  } catch {
    // Gestito dall'interceptor
  }
}

function goBack() {
  router.push({ name: 'users' })
}

// ─── HELPERS ────────────────────────────────────────────────────────────────

function formatRole(roleCode) {
  const map = {
    SuperAdmin:          'Super Admin',
    TenantAdministrator: 'Amministratore',
    Admin:               'Admin',
    User:                'Utente',
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
/* ── PASSWORD FIELD ──────────────────────────────────────────────────────── */
.field__password {
  width: 100% !important;
}
:deep(.field__password .p-inputtext) {
  background: var(--surface) !important;
  border-color: var(--border) !important;
  color: var(--text) !important;
  font-size: 13px !important;
  width: 100% !important;
}

/* ── PAGE LAYOUT ─────────────────────────────────────────────────────────── */
.user-detail-page {
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
.breadcrumb__link:hover { opacity: 0.75; }
.breadcrumb__sep { color: var(--border); }
.breadcrumb__current { color: var(--text-dim); }

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
.btn-ghost--small {
  font-size: 12px !important;
  padding: 5px 10px !important;
}
.ml-auto { margin-left: auto; }

/* ── FORM GRID ───────────────────────────────────────────────────────────── */
.form-grid {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: 16px;
}

/* ── FORM CARD ───────────────────────────────────────────────────────────── */
.form-card {
  background: var(--surface2);
  border: 1px solid var(--border);
  border-radius: 10px;
  overflow: hidden;
}
.form-card--tenants { margin-top: 0; }

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
.form-card__header i { color: var(--accent); font-size: 14px; }
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
.field--inline { gap: 8px; }
.field__label {
  font-size: 12px;
  font-weight: 600;
  color: var(--text-dim);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
.required { color: #ff6b6b; margin-left: 2px; }
.field__input {
  width: 100% !important;
  background: var(--surface) !important;
  border-color: var(--border) !important;
  color: var(--text) !important;
  font-size: 13px !important;
}
.field--error .field__input { border-color: #ff6b6b !important; }
.field__error { color: #ff6b6b; font-size: 11px; }
.field__hint { color: var(--text-faint); font-size: 11px; line-height: 1.4; }

/* ── TOGGLE ──────────────────────────────────────────────────────────────── */
.toggle-wrapper {
  display: flex;
  align-items: center;
  gap: 10px;
}
.toggle-label { font-size: 13px; font-weight: 500; }
.toggle-label--on  { color: #5dca7e; }
.toggle-label--off { color: var(--text-faint); }

/* ── ROLE DISPLAY ────────────────────────────────────────────────────────── */
.role-display { display: flex; align-items: center; }

/* ── TENANT LIST ─────────────────────────────────────────────────────────── */
.tenant-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.tenant-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px 14px;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 8px;
  transition: border-color 0.15s;
}
.tenant-item--default {
  border-color: rgba(52, 211, 153, 0.4);
  background: rgba(52, 211, 153, 0.04);
}
.tenant-item__badge {
  width: 32px;
  height: 32px;
  border-radius: 7px;
  background: var(--surface2);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  color: var(--accent);
}
.tenant-item__info {
  display: flex;
  flex-direction: column;
  gap: 2px;
  flex: 1;
  min-width: 0;
}
.tenant-item__name {
  font-size: 13px;
  font-weight: 500;
  color: var(--text);
}
.tenant-item__code {
  font-size: 11px;
  color: var(--text-faint);
  font-family: 'JetBrains Mono', monospace;
}
.tenant-item__tags {
  display: flex;
  gap: 4px;
  flex-wrap: wrap;
}
.default-tag { font-size: 10px !important; }
.inactive-tag { font-size: 10px !important; }
.tenant-item__actions {
  display: flex;
  gap: 4px;
}
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

.tenant-loading {
  display: flex;
  align-items: center;
  gap: 8px;
  color: var(--text-faint);
  font-size: 13px;
}
.tenant-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 24px 0;
  color: var(--text-faint);
  font-size: 13px;
}
.tenant-empty .pi { font-size: 28px; opacity: 0.4; }

/* ── DIALOG ──────────────────────────────────────────────────────────────── */
.dialog-body {
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 4px 0;
}
.tenant-search-results {
  display: flex;
  flex-direction: column;
  gap: 4px;
  max-height: 220px;
  overflow-y: auto;
  border: 1px solid var(--border);
  border-radius: 8px;
  padding: 4px;
}
.tenant-search-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 10px;
  border-radius: 6px;
  cursor: pointer;
  transition: background 0.12s;
  color: var(--text);
}
  .tenant-search-item:hover { background: var(--surface); }
  .tenant-search-item--selected {
    background: rgba(52, 211, 153, 0.08);
    border: 1px solid rgba(52, 211, 153, 0.3);
  }
.tenant-search-item__info {
  display: flex;
  flex-direction: column;
  flex: 1;
}
.tenant-search-item__name { font-size: 13px; font-weight: 500; }
.tenant-search-item__code {
  font-size: 11px;
  color: var(--text-faint);
  font-family: 'JetBrains Mono', monospace;
}
.check-icon { color: var(--accent); margin-left: auto; }
.tenant-search-empty {
  padding: 20px;
  text-align: center;
  color: var(--text-faint);
  font-size: 13px;
}

/* ── LOADING / ERROR STATES ──────────────────────────────────────────────── */
.loading-state, .error-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  padding: 64px 0;
  color: var(--text-faint);
}
.loading-state__icon { font-size: 36px; color: var(--accent); }
.error-state__icon   { font-size: 36px; color: #ff6b6b; }

/* ── READ-ONLY BANNER ────────────────────────────────────────────────────── */
.readonly-banner {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 16px;
  background: rgba(251, 191, 36, 0.08);
  border: 1px solid rgba(251, 191, 36, 0.35);
  border-radius: 8px;
  font-size: 13px;
  color: #b45309;
}
.readonly-banner .pi { font-size: 15px; flex-shrink: 0; }

/* ── RESPONSIVE ──────────────────────────────────────────────────────────── */
@media (max-width: 768px) {
  .user-detail-page { padding: 16px; }
  .form-grid {
    grid-template-columns: 1fr;
  }
  .page-header {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>
