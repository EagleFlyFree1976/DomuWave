<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal modal-xl">
      <div class="modal-header">
        <h2>Occupanti · {{ unitLabel }}</h2>
        <button class="btn-icon" @click="$emit('close')">✕</button>
      </div>

      <!-- Tabs -->
      <div class="tabs">
        <button class="tab-btn" :class="{ active: tab === 'owners' }" @click="tab = 'owners'">Proprietari</button>
        <button class="tab-btn" :class="{ active: tab === 'tenants' }" @click="tab = 'tenants'">Inquilini</button>
      </div>

      <div class="modal-body">

        <!-- ═══════════ PROPRIETARI ═══════════ -->
        <template v-if="tab === 'owners'">
          <div class="section-toolbar">
            <span class="section-count">{{ owners.length }} proprietari</span>
            <button v-if="canCreate" class="btn btn-primary btn-sm" @click="openAddOwner">+ Aggiungi</button>
          </div>

          <div v-if="loadingOwners" class="loading-state"><div class="spinner"></div></div>
          <div v-else-if="!owners.length" class="empty-state">
            <div>Nessun proprietario registrato</div>
          </div>
          <div v-else class="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Nome</th>
                  <th>Email</th>
                  <th>Tipo</th>
                  <th>Quota</th>
                  <th>Dal</th>
                  <th>Al</th>
                  <th>Residente</th>
                  <th>Stato</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="o in owners" :key="o.id">
                  <td>{{ [o.firstName, o.lastName].filter(Boolean).join(' ') || '—' }}</td>
                  <td class="text-secondary">{{ o.email || '—' }}</td>
                  <td>{{ o.ownerType || '—' }}</td>
                  <td>{{ o.ownershipQuota }}%</td>
                  <td>{{ formatDate(o.startDate) }}</td>
                  <td>{{ o.endDate ? formatDate(o.endDate) : '—' }}</td>
                  <td>{{ o.isResident ? 'Sì' : 'No' }}</td>
                  <td><span class="badge" :class="o.isActive ? 'badge-green' : 'badge-muted'">{{ o.isActive ? 'Attivo' : 'Inattivo' }}</span></td>
                  <td>
                    <div v-if="canEdit || canDelete" class="row-actions">
                      <button v-if="canEdit" class="btn-icon" @click="openEditOwner(o)" title="Modifica">✎</button>
                      <button v-if="canDelete" class="btn-icon" @click="deleteOwner(o.id)" title="Elimina" style="color:var(--accent-red)">✕</button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </template>

        <!-- ═══════════ INQUILINI ═══════════ -->
        <template v-if="tab === 'tenants'">
          <div class="section-toolbar">
            <span class="section-count">{{ tenants.length }} inquilini</span>
            <button v-if="canCreate" class="btn btn-primary btn-sm" @click="openAddTenant">+ Aggiungi</button>
          </div>

          <div v-if="loadingTenants" class="loading-state"><div class="spinner"></div></div>
          <div v-else-if="!tenants.length" class="empty-state">
            <div>Nessun inquilino registrato</div>
          </div>
          <div v-else class="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Nome</th>
                  <th>Email</th>
                  <th>Inizio contratto</th>
                  <th>Fine contratto</th>
                  <th>Stato</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="t in tenants" :key="t.id">
                  <td>{{ t.firstName }} {{ t.lastName }}</td>
                  <td class="text-secondary">{{ t.email || '—' }}</td>
                  <td>{{ formatDate(t.leaseStartDate) }}</td>
                  <td>{{ t.leaseEndDate ? formatDate(t.leaseEndDate) : '—' }}</td>
                  <td><span class="badge" :class="t.isActive ? 'badge-green' : 'badge-muted'">{{ t.isActive ? 'Attivo' : 'Inattivo' }}</span></td>
                  <td>
                    <div v-if="canEdit || canDelete" class="row-actions">
                      <button v-if="canEdit" class="btn-icon" @click="openEditTenant(t)" title="Modifica">✎</button>
                      <button v-if="canDelete" class="btn-icon" @click="deleteTenant(t.id)" title="Elimina" style="color:var(--accent-red)">✕</button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </template>

      </div>
    </div>
  </div>

  <!-- ══════════ SUB-MODAL: Aggiungi/Modifica Proprietario ══════════ -->
  <div class="modal-overlay" v-if="ownerModal.show" @click.self="ownerModal.show = false">
    <div class="modal modal-lg">
      <div class="modal-header">
        <h2>{{ ownerModal.editing ? 'Modifica proprietario' : 'Aggiungi proprietario' }}</h2>
        <button class="btn-icon" @click="ownerModal.show = false">✕</button>
      </div>
      <div class="modal-body">

        <!-- Step 1: selezione utente (solo su creazione) -->
        <template v-if="!ownerModal.editing">
          <fieldset class="form-fieldset">
            <legend class="form-fieldset-legend">1 · Seleziona condomino</legend>
            <div class="user-search-row">
              <input class="form-input" v-model="userSearch" placeholder="Cerca per nome o email…" @input="searchUsers" />
              <button class="btn btn-ghost btn-sm" @click="openCreateCondomino('owner')">+ Nuovo</button>
            </div>
            <div v-if="loadingUsers" class="loading-state small"><div class="spinner"></div></div>
            <div v-else-if="userSearch && !userResults.length" class="empty-state small">Nessun utente trovato</div>
            <div v-else-if="userResults.length" class="user-list">
              <div
                v-for="u in userResults" :key="u.id"
                class="user-item"
                :class="{ selected: ownerModal.form.userId === u.id }"
                @click="selectOwnerUser(u)"
              >
                <span class="user-name">{{ u.firstName || u.name }} {{ u.lastName }}</span>
                <span class="user-email text-secondary">{{ u.email }}</span>
              </div>
            </div>
            <div v-if="ownerModal.form.userId" class="selected-user-badge">
              ✓ {{ ownerModal.selectedUserName }}
            </div>
          </fieldset>
        </template>

        <!-- Dati anagrafici (sempre visibili, pre-popolati dalla selezione utente) -->
        <fieldset class="form-fieldset" style="margin-top:1rem">
          <legend class="form-fieldset-legend">{{ ownerModal.editing ? 'Dati anagrafici' : '2 · Dati anagrafici' }}</legend>
          <div class="form-grid">
            <div class="form-group">
              <label class="form-label">Nome</label>
              <input class="form-input" v-model="ownerModal.form.firstName" placeholder="Es. Mario" />
            </div>
            <div class="form-group">
              <label class="form-label">Cognome</label>
              <input class="form-input" v-model="ownerModal.form.lastName" placeholder="Es. Rossi" />
            </div>
            <div class="form-group" style="grid-column:span 2">
              <label class="form-label">Email</label>
              <input class="form-input" type="email" v-model="ownerModal.form.email" placeholder="Es. mario.rossi@email.it" />
            </div>
          </div>
        </fieldset>

        <!-- Step 2/3: dettagli proprietà -->
        <fieldset class="form-fieldset" style="margin-top:1rem">
          <legend class="form-fieldset-legend">{{ ownerModal.editing ? 'Dettagli proprietà' : '3 · Dettagli proprietà' }}</legend>
          <div class="form-grid">
            <div class="form-group">
              <label class="form-label">Tipo proprietà</label>
              <select class="form-select" v-model="ownerModal.form.ownerType">
                <option value="">— Seleziona —</option>
                <option v-for="t in ownerTypes" :key="t" :value="t">{{ t }}</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Quota (%)</label>
              <input class="form-input" type="number" min="0" max="100" step="0.01" v-model.number="ownerModal.form.ownershipQuota" />
            </div>
            <div class="form-group">
              <label class="form-label">Data inizio *</label>
              <input class="form-input" type="date" v-model="ownerModal.form.startDate" />
            </div>
            <div class="form-group">
              <label class="form-label">Data fine</label>
              <input class="form-input" type="date" v-model="ownerModal.form.endDate" />
            </div>
          </div>
          <div class="form-group" style="flex-direction:row;align-items:center;gap:0.5rem;margin-top:0.5rem">
            <input type="checkbox" id="ownerIsResident" v-model="ownerModal.form.isResident" />
            <label for="ownerIsResident" style="font-size:0.875rem;cursor:pointer">Residente nell'unità</label>
          </div>
          <div class="form-group" style="flex-direction:row;align-items:center;gap:0.5rem;margin-top:0.25rem">
            <input type="checkbox" id="ownerIsActive" v-model="ownerModal.form.isActive" />
            <label for="ownerIsActive" style="font-size:0.875rem;cursor:pointer">Attivo</label>
          </div>
          <div class="form-group" style="margin-top:0.75rem">
            <label class="form-label">Note</label>
            <textarea class="form-textarea" v-model="ownerModal.form.notes" rows="2"></textarea>
          </div>
        </fieldset>

      </div>
      <div class="modal-footer">
        <button class="btn btn-ghost" @click="ownerModal.show = false">Annulla</button>
        <button class="btn btn-primary" @click="saveOwner" :disabled="ownerModal.saving">
          <span v-if="ownerModal.saving" class="spinner" style="width:14px;height:14px"></span>
          {{ ownerModal.editing ? 'Salva' : 'Aggiungi' }}
        </button>
      </div>
    </div>
  </div>

  <!-- ══════════ SUB-MODAL: Aggiungi/Modifica Inquilino ══════════ -->
  <div class="modal-overlay" v-if="tenantModal.show" @click.self="tenantModal.show = false">
    <div class="modal modal-lg">
      <div class="modal-header">
        <h2>{{ tenantModal.editing ? 'Modifica inquilino' : 'Aggiungi inquilino' }}</h2>
        <button class="btn-icon" @click="tenantModal.show = false">✕</button>
      </div>
      <div class="modal-body">

        <!-- Step 1: selezione utente (solo su creazione) -->
        <template v-if="!tenantModal.editing">
          <fieldset class="form-fieldset">
            <legend class="form-fieldset-legend">1 · Seleziona condomino</legend>
            <div class="user-search-row">
              <input class="form-input" v-model="tenantUserSearch" placeholder="Cerca per nome o email…" @input="searchTenantUsers" />
              <button class="btn btn-ghost btn-sm" @click="openCreateCondomino('tenant')">+ Nuovo</button>
            </div>
            <div v-if="loadingTenantUsers" class="loading-state small"><div class="spinner"></div></div>
            <div v-else-if="tenantUserSearch && !tenantUserResults.length" class="empty-state small">Nessun utente trovato</div>
            <div v-else-if="tenantUserResults.length" class="user-list">
              <div
                v-for="u in tenantUserResults" :key="u.id"
                class="user-item"
                :class="{ selected: tenantModal.form.userId === u.id }"
                @click="selectTenantUser(u)"
              >
                <span class="user-name">{{ u.firstName || u.name }} {{ u.lastName }}</span>
                <span class="user-email text-secondary">{{ u.email }}</span>
              </div>
            </div>
            <div v-if="tenantModal.form.userId" class="selected-user-badge">
              ✓ {{ tenantModal.selectedUserName }}
            </div>
          </fieldset>
        </template>

        <!-- Step 2: dettagli contratto -->
        <fieldset class="form-fieldset" style="margin-top:1rem">
          <legend class="form-fieldset-legend">{{ tenantModal.editing ? 'Dettagli' : '2 · Dettagli contratto' }}</legend>
          <div class="form-grid">
            <div class="form-group">
              <label class="form-label">Inizio contratto *</label>
              <input class="form-input" type="date" v-model="tenantModal.form.leaseStartDate" />
            </div>
            <div class="form-group">
              <label class="form-label">Fine contratto</label>
              <input class="form-input" type="date" v-model="tenantModal.form.leaseEndDate" />
            </div>
            <div class="form-group">
              <label class="form-label">Pagatore spese</label>
              <select class="form-select" v-model="tenantModal.form.expensePayer">
                <option value="">— Seleziona —</option>
                <option value="Proprietario">Proprietario</option>
                <option value="Inquilino">Inquilino</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Codice fiscale</label>
              <input class="form-input" v-model="tenantModal.form.taxCode" maxlength="16" style="text-transform:uppercase" />
            </div>
          </div>
          <div class="form-group" style="flex-direction:row;align-items:center;gap:0.5rem;margin-top:0.5rem">
            <input type="checkbox" id="tenantIsActive" v-model="tenantModal.form.isActive" />
            <label for="tenantIsActive" style="font-size:0.875rem;cursor:pointer">Attivo</label>
          </div>
          <div class="form-group" style="margin-top:0.75rem">
            <label class="form-label">Note</label>
            <textarea class="form-textarea" v-model="tenantModal.form.notes" rows="2"></textarea>
          </div>
        </fieldset>

      </div>
      <div class="modal-footer">
        <button class="btn btn-ghost" @click="tenantModal.show = false">Annulla</button>
        <button class="btn btn-primary" @click="saveTenant" :disabled="tenantModal.saving">
          <span v-if="tenantModal.saving" class="spinner" style="width:14px;height:14px"></span>
          {{ tenantModal.editing ? 'Salva' : 'Aggiungi' }}
        </button>
      </div>
    </div>
  </div>

  <!-- ══════════ SUB-MODAL: Nuovo condomino ══════════ -->
  <CondominoModal
    v-if="condominoModal.show"
    :user="null"
    @saved="onCondominoCreated"
    @close="condominoModal.show = false"
  />
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useAppStore } from '@/stores/app'
import { unitOwnerApi, unitTenantApi } from '@/services/api'
import { userApi } from '@/services/userService'
import CondominoModal from './CondominoModal.vue'
import { usePermissions } from '@/composables/usePermissions'

const props = defineProps({
  unitId:    { type: Number, required: true },
  unitLabel: { type: String, default: '' },
})
const emit = defineEmits(['close'])
const { canCreate, canEdit, canDelete } = usePermissions()

const store = useAppStore()

// ─── Tabs ────────────────────────────────────────────────────
const tab = ref('owners')

// ─── Data ────────────────────────────────────────────────────
const owners         = ref([])
const tenants        = ref([])
const loadingOwners  = ref(false)
const loadingTenants = ref(false)

const ownerTypes = ['Proprietario', 'Comproprietario', 'Nudo Proprietario', 'Usufruttuario']

// ─── User search (for owners) ─────────────────────────────────
const userSearch      = ref('')
const userResults     = ref([])
const loadingUsers    = ref(false)
let   userSearchTimer = null

// ─── User search (for tenants) ────────────────────────────────
const tenantUserSearch   = ref('')
const tenantUserResults  = ref([])
const loadingTenantUsers = ref(false)
let   tenantSearchTimer  = null

// ─── Owner modal state ────────────────────────────────────────
const ownerModal = reactive({
  show:             false,
  editing:          null,
  saving:           false,
  selectedUserName: '',
  form: {
    userId:         0,
    firstName:      '',
    lastName:       '',
    email:          '',
    ownerType:      '',
    ownershipQuota: 100,
    startDate:      '',
    endDate:        '',
    isResident:     false,
    isActive:       true,
    notes:          '',
  },
})

// ─── Tenant modal state ───────────────────────────────────────
const tenantModal = reactive({
  show:             false,
  editing:          null,
  saving:           false,
  selectedUserName: '',
  form: {
    userId:         0,
    firstName:      '',
    lastName:       '',
    email:          '',
    phone:          '',
    leaseStartDate: '',
    leaseEndDate:   '',
    taxCode:        '',
    expensePayer:   '',
    isActive:       true,
    notes:          '',
  },
})

// ─── Condomino create modal ───────────────────────────────────
const condominoModal = reactive({ show: false, target: '' })

// ─── Helpers ─────────────────────────────────────────────────
function formatDate(d) {
  if (!d) return '—'
  return new Date(d).toLocaleDateString('it-IT', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

function todayIso() {
  return new Date().toISOString().split('T')[0]
}

// ─── Load data ────────────────────────────────────────────────
async function loadOwners() {
  loadingOwners.value = true
  try {
    const { data } = await unitOwnerApi.getByUnit(props.unitId)
    owners.value = data
  } catch { /* handled by interceptor */ }
  finally { loadingOwners.value = false }
}

async function loadTenants() {
  loadingTenants.value = true
  try {
    const { data } = await unitTenantApi.getByUnit(props.unitId)
    tenants.value = data
  } catch { /* handled by interceptor */ }
  finally { loadingTenants.value = false }
}

// ─── User search ──────────────────────────────────────────────
function searchUsers() {
  clearTimeout(userSearchTimer)
  if (!userSearch.value.trim()) { userResults.value = []; return }
  userSearchTimer = setTimeout(async () => {
    loadingUsers.value = true
    try {
      const { data } = await userApi.search({ roles: 'Condomino', search: userSearch.value })
      userResults.value = Array.isArray(data) ? data : (data.items ?? [])
    } catch { userResults.value = [] }
    finally { loadingUsers.value = false }
  }, 300)
}

function searchTenantUsers() {
  clearTimeout(tenantSearchTimer)
  if (!tenantUserSearch.value.trim()) { tenantUserResults.value = []; return }
  tenantSearchTimer = setTimeout(async () => {
    loadingTenantUsers.value = true
    try {
      const { data } = await userApi.search({ roles: 'Condomino', search: tenantUserSearch.value })
      tenantUserResults.value = Array.isArray(data) ? data : (data.items ?? [])
    } catch { tenantUserResults.value = [] }
    finally { loadingTenantUsers.value = false }
  }, 300)
}

// ─── Owner modal actions ──────────────────────────────────────
function openAddOwner() {
  ownerModal.editing          = null
  ownerModal.selectedUserName = ''
  ownerModal.form.userId         = 0
  ownerModal.form.firstName      = ''
  ownerModal.form.lastName       = ''
  ownerModal.form.email          = ''
  ownerModal.form.ownerType      = ''
  ownerModal.form.ownershipQuota = 100
  ownerModal.form.startDate      = todayIso()
  ownerModal.form.endDate        = ''
  ownerModal.form.isResident     = false
  ownerModal.form.isActive       = true
  ownerModal.form.notes          = ''
  userSearch.value    = ''
  userResults.value   = []
  ownerModal.show = true
}

function openEditOwner(owner) {
  ownerModal.editing          = owner.id
  ownerModal.selectedUserName = ''
  ownerModal.form.userId         = owner.userId
  ownerModal.form.firstName      = owner.firstName ?? ''
  ownerModal.form.lastName       = owner.lastName ?? ''
  ownerModal.form.email          = owner.email ?? ''
  ownerModal.form.ownerType      = owner.ownerType ?? ''
  ownerModal.form.ownershipQuota = owner.ownershipQuota
  ownerModal.form.startDate      = owner.startDate?.split('T')[0] ?? ''
  ownerModal.form.endDate        = owner.endDate?.split('T')[0] ?? ''
  ownerModal.form.isResident     = owner.isResident
  ownerModal.form.isActive       = owner.isActive
  ownerModal.form.notes          = owner.notes ?? ''
  ownerModal.show = true
}

function selectOwnerUser(u) {
  ownerModal.form.userId      = u.id
  ownerModal.form.firstName   = u.firstName || u.name || ''
  ownerModal.form.lastName    = u.lastName || ''
  ownerModal.form.email       = u.email || ''
  ownerModal.selectedUserName = `${ownerModal.form.firstName} ${ownerModal.form.lastName}`.trim()
  userResults.value           = []
  userSearch.value            = ''
}

async function saveOwner() {
  if (!ownerModal.editing && !ownerModal.form.userId)
    return store.toast('Seleziona un condomino', 'error')
  if (!ownerModal.form.startDate)
    return store.toast('Specificare la data di inizio', 'error')

  ownerModal.saving = true
  try {
    if (ownerModal.editing) {
      const dto = {
        firstName:      ownerModal.form.firstName || null,
        lastName:       ownerModal.form.lastName || null,
        email:          ownerModal.form.email || null,
        ownerType:      ownerModal.form.ownerType || null,
        ownershipQuota: ownerModal.form.ownershipQuota,
        startDate:      ownerModal.form.startDate,
        endDate:        ownerModal.form.endDate || null,
        isResident:     ownerModal.form.isResident,
        isActive:       ownerModal.form.isActive,
        notes:          ownerModal.form.notes || null,
      }
      await unitOwnerApi.update(ownerModal.editing, dto)
      store.toast('Proprietario aggiornato', 'success')
    } else {
      const dto = {
        unitId:         props.unitId,
        userId:         ownerModal.form.userId,
        firstName:      ownerModal.form.firstName || null,
        lastName:       ownerModal.form.lastName || null,
        email:          ownerModal.form.email || null,
        ownerType:      ownerModal.form.ownerType || null,
        ownershipQuota: ownerModal.form.ownershipQuota,
        startDate:      ownerModal.form.startDate,
        endDate:        ownerModal.form.endDate || null,
        isResident:     ownerModal.form.isResident,
        isActive:       ownerModal.form.isActive,
        notes:          ownerModal.form.notes || null,
      }
      await unitOwnerApi.create(dto)
      store.toast('Proprietario aggiunto', 'success')
    }
    ownerModal.show = false
    await loadOwners()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    ownerModal.saving = false
  }
}

async function deleteOwner(id) {
  if (!confirm('Rimuovere questo proprietario?')) return
  try {
    await unitOwnerApi.delete(id)
    store.toast('Proprietario rimosso', 'success')
    await loadOwners()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

// ─── Tenant modal actions ─────────────────────────────────────
function openAddTenant() {
  tenantModal.editing          = null
  tenantModal.selectedUserName = ''
  tenantModal.form.userId         = 0
  tenantModal.form.firstName      = ''
  tenantModal.form.lastName       = ''
  tenantModal.form.email          = ''
  tenantModal.form.phone          = ''
  tenantModal.form.leaseStartDate = todayIso()
  tenantModal.form.leaseEndDate   = ''
  tenantModal.form.taxCode        = ''
  tenantModal.form.expensePayer   = ''
  tenantModal.form.isActive       = true
  tenantModal.form.notes          = ''
  tenantUserSearch.value   = ''
  tenantUserResults.value  = []
  tenantModal.show = true
}

function openEditTenant(t) {
  tenantModal.editing          = t.id
  tenantModal.selectedUserName = ''
  tenantModal.form.userId         = t.userId
  tenantModal.form.firstName      = t.firstName ?? ''
  tenantModal.form.lastName       = t.lastName ?? ''
  tenantModal.form.email          = t.email ?? ''
  tenantModal.form.phone          = t.phone ?? ''
  tenantModal.form.leaseStartDate = t.leaseStartDate?.split('T')[0] ?? ''
  tenantModal.form.leaseEndDate   = t.leaseEndDate?.split('T')[0] ?? ''
  tenantModal.form.taxCode        = t.taxCode ?? ''
  tenantModal.form.expensePayer   = t.expensePayer ?? ''
  tenantModal.form.isActive       = t.isActive
  tenantModal.form.notes          = t.notes ?? ''
  tenantModal.show = true
}

function selectTenantUser(u) {
  tenantModal.form.userId         = u.id
  tenantModal.form.firstName      = u.firstName || u.name || ''
  tenantModal.form.lastName       = u.lastName || ''
  tenantModal.form.email          = u.email || ''
  tenantModal.selectedUserName    = `${tenantModal.form.firstName} ${tenantModal.form.lastName}`.trim()
  tenantUserResults.value         = []
  tenantUserSearch.value          = ''
}

async function saveTenant() {
  if (!tenantModal.editing && !tenantModal.form.userId)
    return store.toast('Seleziona un condomino', 'error')
  if (!tenantModal.form.leaseStartDate)
    return store.toast('Specificare la data di inizio contratto', 'error')

  tenantModal.saving = true
  try {
    if (tenantModal.editing) {
      const dto = {
        leaseStartDate: tenantModal.form.leaseStartDate,
        leaseEndDate:   tenantModal.form.leaseEndDate || null,
        taxCode:        tenantModal.form.taxCode || null,
        expensePayer:   tenantModal.form.expensePayer || null,
        isActive:       tenantModal.form.isActive,
        notes:          tenantModal.form.notes || null,
      }
      await unitTenantApi.update(tenantModal.editing, dto)
      store.toast('Inquilino aggiornato', 'success')
    } else {
      const dto = {
        unitId:         props.unitId,
        userId:         tenantModal.form.userId,
        firstName:      tenantModal.form.firstName || null,
        lastName:       tenantModal.form.lastName || null,
        email:          tenantModal.form.email || null,
        phone:          tenantModal.form.phone || null,
        leaseStartDate: tenantModal.form.leaseStartDate,
        leaseEndDate:   tenantModal.form.leaseEndDate || null,
        taxCode:        tenantModal.form.taxCode || null,
        expensePayer:   tenantModal.form.expensePayer || null,
        isActive:       tenantModal.form.isActive,
        notes:          tenantModal.form.notes || null,
      }
      await unitTenantApi.create(dto)
      store.toast('Inquilino aggiunto', 'success')
    }
    tenantModal.show = false
    await loadTenants()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    tenantModal.saving = false
  }
}

async function deleteTenant(id) {
  if (!confirm('Rimuovere questo inquilino?')) return
  try {
    await unitTenantApi.delete(id)
    store.toast('Inquilino rimosso', 'success')
    await loadTenants()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

// ─── Create new Condomino user ────────────────────────────────
function openCreateCondomino(target) {
  condominoModal.target = target
  condominoModal.show   = true
}

function onCondominoCreated(user) {
  condominoModal.show = false
  if (condominoModal.target === 'owner') {
    selectOwnerUser(user)
  } else {
    selectTenantUser(user)
  }
}

// ─── Init ─────────────────────────────────────────────────────
onMounted(() => {
  loadOwners()
  loadTenants()
})
</script>

<style scoped>
/* Sizes: override global max-width: 560px */
.modal-xl { width: min(1020px, 96vw); max-width: min(1020px, 96vw); min-height: min(640px, 80vh); }
.modal-lg { width: min(700px, 96vw);  max-width: min(700px, 96vw); }

.tabs { display: flex; border-bottom: 1px solid var(--border); padding: 0 1.25rem; gap: 0; }
.tab-btn {
  padding: 0.6rem 1.25rem;
  font-size: 0.875rem;
  font-weight: 500;
  border: none;
  background: none;
  cursor: pointer;
  color: var(--text-secondary);
  border-bottom: 2px solid transparent;
  margin-bottom: -1px;
}
.tab-btn.active { color: var(--primary); border-bottom-color: var(--primary); }

.section-toolbar { display: flex; align-items: center; justify-content: space-between; margin-bottom: 0.75rem; }
.section-count { font-size: 0.8125rem; color: var(--text-secondary); }

.user-search-row { display: flex; gap: 0.5rem; margin-bottom: 0.5rem; }
.user-list { border: 1px solid var(--border); border-radius: 6px; max-height: 180px; overflow-y: auto; margin-top: 0.25rem; }
.user-item {
  display: flex;
  flex-direction: column;
  padding: 0.5rem 0.75rem;
  cursor: pointer;
  border-bottom: 1px solid var(--border);
}
.user-item:last-child { border-bottom: none; }
.user-item:hover { background: var(--bg-hover, #f5f5f5); }
.user-item.selected { background: var(--primary-light, #e8f0fe); }
.user-name { font-size: 0.875rem; font-weight: 500; }
.user-email { font-size: 0.78rem; }

.selected-user-badge {
  margin-top: 0.5rem;
  font-size: 0.8125rem;
  color: var(--accent-green, #38a169);
  font-weight: 500;
}

.loading-state.small { padding: 0.5rem; }
.empty-state.small { padding: 0.5rem; font-size: 0.8125rem; }

.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; }
.form-fieldset { border: 1px solid var(--border); border-radius: 6px; padding: 1rem; margin-top: 1rem; }
.form-fieldset-legend { font-size: 0.8125rem; font-weight: 600; color: var(--text-secondary); padding: 0 0.4rem; }
.row-actions { display: flex; gap: 0.4rem; justify-content: flex-end; }
</style>
