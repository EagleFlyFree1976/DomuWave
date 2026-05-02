<template>
  <div v-if="assembly" class="assembly-detail">

    <!-- Header card -->
    <div class="card detail-header">
      <div class="detail-header-main">
        <div>
          <h2 class="detail-title">{{ assembly.title }}</h2>
          <div class="detail-meta">
            <span class="badge" :class="statusClass(assembly.statusId)">{{ assembly.statusName }}</span>
            <span class="text-secondary">{{ assembly.assemblyTypeName }}</span>
            <span v-if="assembly.fiscalYearName" class="text-secondary">· {{ assembly.fiscalYearName }}</span>
          </div>
        </div>
        <div class="detail-actions">
          <button v-if="canEdit && assembly.statusId === 1" class="btn btn-primary btn-sm" @click="planAssembly">
            <i class="pi pi-send"></i> Pianifica
          </button>
          <button v-if="canEdit && assembly.statusId === 3" class="btn btn-primary btn-sm" @click="openCloseModal">
            <i class="pi pi-check"></i> Segna come svolta
          </button>
          <button v-if="canEdit && [1,2,3].includes(assembly.statusId)" class="btn btn-ghost btn-sm" style="color:var(--accent-red)" @click="cancelAssembly">
            <i class="pi pi-ban"></i> Annulla
          </button>
          <button v-if="canEdit && [1,2].includes(assembly.statusId)" class="btn btn-ghost btn-sm" @click="openEditModal">
            <i class="pi pi-pencil"></i> Modifica
          </button>
          <button v-if="canDelete && assembly.statusId === 1" class="btn btn-ghost btn-sm" style="color:var(--accent-red)" @click="deleteAssembly">
            <i class="pi pi-trash"></i> Elimina
          </button>
        </div>
      </div>
      <div class="detail-info-grid">
        <div class="info-item">
          <span class="info-label">Data convocata</span>
          <span class="info-value">{{ fmtDate(assembly.scheduledDate) }}</span>
        </div>
        <div class="info-item" v-if="assembly.actualDate">
          <span class="info-label">Data effettiva</span>
          <span class="info-value">{{ fmtDate(assembly.actualDate) }}</span>
        </div>
        <div class="info-item" v-if="assembly.location">
          <span class="info-label">Luogo</span>
          <span class="info-value">{{ assembly.location }}</span>
        </div>
        <div class="info-item" v-if="assembly.notes">
          <span class="info-label">Note</span>
          <span class="info-value">{{ assembly.notes }}</span>
        </div>
        <div class="info-item" v-if="assembly.communicationId">
          <span class="info-label">Comunicazione</span>
          <router-link class="info-link" :to="communicationPath">
            <i class="pi pi-envelope"></i> {{ assembly.communicationTitle || 'Vai alla comunicazione' }}
          </router-link>
        </div>
      </div>
    </div>

    <!-- Tabs -->
    <div class="tab-pills">
      <button class="tab-pill" :class="{ active: tab === 'odg' }" @click="tab = 'odg'">
        <i class="pi pi-list"></i> Ordine del Giorno ({{ agendaItems.length }})
      </button>
      <button class="tab-pill" :class="{ active: tab === 'presenze' }" @click="tab = 'presenze'">
        <i class="pi pi-users"></i> Presenze ({{ attendances.length }})
      </button>
      <button v-if="assembly.statusId >= 3" class="tab-pill" :class="{ active: tab === 'verbale' }" @click="tab = 'verbale'">
        <i class="pi pi-file-edit"></i> Verbale
      </button>
    </div>

    <!-- OdG Tab -->
    <div v-if="tab === 'odg'">
      <div class="toolbar">
        <span class="text-secondary" style="font-size:.875rem">Punti all'ordine del giorno</span>
        <button v-if="canCreate && assembly.statusId !== 2" class="btn btn-primary btn-sm" style="margin-left:auto" @click="openAgendaModal()">
          + Aggiungi punto
        </button>
      </div>
      <div class="card">
        <div v-if="loadingAgenda" class="loading-state"><div class="spinner"></div> Caricamento…</div>
        <div v-else-if="!agendaItems.length" class="empty-state">
          <div>Nessun punto OdG inserito</div>
        </div>
        <div v-else class="agenda-list">
          <div v-for="item in agendaItems" :key="item.id" class="agenda-item">
            <div class="agenda-number">{{ item.orderIndex }}</div>
            <div class="agenda-content">
              <div class="agenda-title">{{ item.title }}</div>
              <div v-if="item.description" class="agenda-desc text-secondary">{{ item.description }}</div>
              <div v-if="item.resolution" class="agenda-resolution">
                <span class="resolution-label">Delibera:</span> {{ item.resolution }}
              </div>
            </div>
            <div class="agenda-vote">
              <span class="badge" :class="voteClass(item.voteResultId)">{{ item.voteResultName }}</span>
            </div>
            <div class="row-actions" v-if="canEdit">
              <button class="btn-icon" @click="openAgendaModal(item)" title="Modifica">✎</button>
              <button class="btn-icon" style="color:var(--accent-red)" @click="deleteAgendaItem(item.id)" title="Elimina">✕</button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Presenze Tab -->
    <div v-if="tab === 'presenze'">
      <div class="toolbar">
        <div class="quorum-info text-secondary" style="font-size:.875rem">
          Millesimi presenti: <strong>{{ totMillesimi.toFixed(4) }}</strong>
        </div>
        <button v-if="canEdit && assembly.statusId !== 5" class="btn btn-ghost btn-sm" style="margin-left:auto" @click="prepopulateAttendances">
          <i class="pi pi-refresh"></i> Prepopola presenze
        </button>
        <button v-if="canCreate && assembly.statusId !== 2" class="btn btn-primary btn-sm" @click="openAttendanceModal()">
          + Aggiungi presenza
        </button>
      </div>
      <div class="card">
        <div v-if="loadingAttendances" class="loading-state"><div class="spinner"></div> Caricamento…</div>
        <div v-else-if="!attendances.length" class="empty-state">
          <div>Nessuna presenza registrata</div>
        </div>
        <div v-else class="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Proprietario</th>
                <th>Unità</th>
                <th>Presenza</th>
                <th>Delegato</th>
                <th class="text-right">Millesimi</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="a in attendances" :key="a.id">
                <td>{{ a.ownerFirstName }} {{ a.ownerLastName }}</td>
                <td class="mono text-secondary">{{ a.unitInternalNumber || '—' }}</td>
                <td><span class="badge" :class="attendanceClass(a.attendanceTypeId)">{{ a.attendanceTypeName }}</span></td>
                <td class="text-secondary">{{ a.delegateName || '—' }}</td>
                <td class="text-right mono">{{ a.millesimalValue?.toFixed(4) ?? '—' }}</td>
                <td v-if="canEdit">
                  <div class="row-actions">
                    <button class="btn-icon" @click="openAttendanceModal(a)" title="Modifica">✎</button>
                    <button class="btn-icon" style="color:var(--accent-red)" @click="deleteAttendance(a.id)" title="Elimina">✕</button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Verbale Tab -->
    <div v-if="tab === 'verbale'">
      <div class="card" style="padding:1.25rem 1.5rem">
        <div class="form-fieldset">
          <span class="form-fieldset-legend">Consiglieri / Ufficio di presidenza</span>
          <div class="form-group form-group--full">
            <label class="form-label">Nomi dei consiglieri</label>
            <textarea
              class="form-textarea"
              v-model="minutesForm.boardMembers"
              rows="3"
              placeholder="Es. Presidente: Mario Rossi — Segretario: Anna Bianchi — Scrutatori: …"
              :disabled="assembly.statusId === 5"
            ></textarea>
          </div>
        </div>
        <div class="form-fieldset" style="margin-top:1rem">
          <span class="form-fieldset-legend">Trascrizione del verbale</span>
          <div class="form-group form-group--full">
            <label class="form-label">Verbale assemblea</label>
            <textarea
              class="form-textarea minutes-editor"
              v-model="minutesForm.minutes"
              rows="18"
              placeholder="Testo integrale del verbale…"
              :disabled="assembly.statusId === 5"
            ></textarea>
          </div>
        </div>
        <div class="toolbar" style="margin-top:1rem; justify-content:flex-end; border-top:1px solid var(--border); padding-top:1rem" v-if="canEdit && assembly.statusId !== 5">
          <button class="btn btn-primary" @click="saveMinutes" :disabled="savingMinutes">
            <span v-if="savingMinutes" class="spinner" style="width:14px;height:14px"></span>
            Salva verbale
          </button>
        </div>
      </div>
    </div>

    <!-- Modal modifica assemblea -->
    <div class="modal-overlay" v-if="showEditModal" @click.self="showEditModal=false">
      <div class="modal">
        <div class="modal-header">
          <h2>Modifica assemblea</h2>
          <button class="btn-icon" @click="showEditModal=false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-grid">
            <div class="form-group form-group--full" :class="{ 'has-error': editErrors.title }">
              <label class="form-label">Titolo *</label>
              <input class="form-input" v-model="editForm.title" @input="delete editErrors.title" />
              <span v-if="editErrors.title" class="field-error">{{ editErrors.title }}</span>
            </div>
            <div class="form-group">
              <label class="form-label">Tipo *</label>
              <select class="form-select" v-model.number="editForm.assemblyTypeId">
                <option :value="1">Ordinaria</option>
                <option :value="2">Straordinaria</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Esercizio fiscale</label>
              <select class="form-select" v-model.number="editForm.fiscalYearId">
                <option :value="null">— Nessuno —</option>
                <option v-for="fy in fiscalYears" :key="fy.id" :value="fy.id">{{ fy.code }}{{ fy.description ? ' – ' + fy.description : '' }}</option>
              </select>
            </div>
            <div class="form-group" :class="{ 'has-error': editErrors.scheduledDate }">
              <label class="form-label">Data convocata *</label>
              <input class="form-input" type="datetime-local" v-model="editForm.scheduledDate" @input="delete editErrors.scheduledDate" />
              <span v-if="editErrors.scheduledDate" class="field-error">{{ editErrors.scheduledDate }}</span>
            </div>
            <div class="form-group">
              <label class="form-label">Luogo</label>
              <input class="form-input" v-model="editForm.location" placeholder="es. Sala riunioni condominio" />
            </div>
            <div class="form-group form-group--full">
              <label class="form-label">Note</label>
              <textarea class="form-textarea" v-model="editForm.notes" rows="3"></textarea>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showEditModal=false">Annulla</button>
          <button class="btn btn-primary" @click="saveEdit" :disabled="savingEdit">
            <span v-if="savingEdit" class="spinner" style="width:14px;height:14px"></span>
            Salva
          </button>
        </div>
      </div>
    </div>

    <!-- Modal OdG -->
    <div class="modal-overlay" v-if="showAgendaModal" @click.self="showAgendaModal=false">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ editingAgenda ? 'Modifica punto OdG' : 'Nuovo punto OdG' }}</h2>
          <button class="btn-icon" @click="showAgendaModal=false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-grid">
            <div class="form-group" style="max-width:100px">
              <label class="form-label">Ordine</label>
              <input class="form-input" type="number" min="1" v-model.number="agendaForm.orderIndex" />
            </div>
            <div class="form-group form-group--full" :class="{ 'has-error': agendaErrors.title }">
              <label class="form-label">Titolo *</label>
              <input class="form-input" v-model="agendaForm.title" @input="delete agendaErrors.title" />
              <span v-if="agendaErrors.title" class="field-error">{{ agendaErrors.title }}</span>
            </div>
            <div class="form-group form-group--full">
              <label class="form-label">Descrizione</label>
              <textarea class="form-textarea" v-model="agendaForm.description" rows="2"></textarea>
            </div>
            <div v-if="editingAgenda" class="form-group">
              <label class="form-label">Esito votazione</label>
              <select class="form-select" v-model.number="agendaForm.voteResultId">
                <option :value="1">Non Votato</option>
                <option :value="2">Approvato</option>
                <option :value="3">Respinto</option>
                <option :value="4">Rinviato</option>
              </select>
            </div>
            <div v-if="editingAgenda" class="form-group form-group--full">
              <label class="form-label">Delibera / Esito</label>
              <textarea class="form-textarea" v-model="agendaForm.resolution" rows="3" placeholder="Testo della delibera approvata…"></textarea>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showAgendaModal=false">Annulla</button>
          <button class="btn btn-primary" @click="saveAgendaItem" :disabled="savingAgenda">
            <span v-if="savingAgenda" class="spinner" style="width:14px;height:14px"></span>
            {{ editingAgenda ? 'Salva' : 'Aggiungi' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Modal Presenza -->
    <div class="modal-overlay" v-if="showAttendanceModal" @click.self="showAttendanceModal=false">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ editingAttendance ? 'Modifica presenza' : 'Registra presenza' }}</h2>
          <button class="btn-icon" @click="showAttendanceModal=false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-grid">
            <div v-if="!editingAttendance" class="form-group form-group--full" :class="{ 'has-error': attendanceErrors.unitOwnerId }">
              <label class="form-label">Proprietario *</label>
              <select class="form-select" v-model.number="attendanceForm.unitOwnerId" @change="delete attendanceErrors.unitOwnerId">
                <option :value="null">— Seleziona —</option>
                <option v-for="o in availableOwners" :key="o.id" :value="o.id">
                  {{ o.firstName }} {{ o.lastName }} ({{ o.unitInternalNumber || 'unità n/d' }})
                </option>
              </select>
              <span v-if="attendanceErrors.unitOwnerId" class="field-error">{{ attendanceErrors.unitOwnerId }}</span>
            </div>
            <div v-else class="form-group form-group--full">
              <label class="form-label">Proprietario</label>
              <input class="form-input" :value="`${editingAttendanceData?.ownerFirstName} ${editingAttendanceData?.ownerLastName}`" disabled />
            </div>
            <div class="form-group">
              <label class="form-label">Tipo presenza</label>
              <select class="form-select" v-model.number="attendanceForm.attendanceTypeId">
                <option :value="1">Presente</option>
                <option :value="2">Delegato</option>
                <option :value="3">Assente</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Millesimi</label>
              <input class="form-input" type="number" step="0.0001" v-model.number="attendanceForm.millesimalValue" />
            </div>
            <div v-if="attendanceForm.attendanceTypeId === 2" class="form-group form-group--full">
              <label class="form-label">Nome delegato</label>
              <input class="form-input" v-model="attendanceForm.delegateName" />
            </div>
            <div class="form-group form-group--full">
              <label class="form-label">Note</label>
              <input class="form-input" v-model="attendanceForm.notes" />
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showAttendanceModal=false">Annulla</button>
          <button class="btn btn-primary" @click="saveAttendance" :disabled="savingAttendance">
            <span v-if="savingAttendance" class="spinner" style="width:14px;height:14px"></span>
            {{ editingAttendance ? 'Salva' : 'Registra' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Modal chiusura -->
    <div class="modal-overlay" v-if="showCloseModal" @click.self="showCloseModal=false">
      <div class="modal" style="max-width:420px">
        <div class="modal-header">
          <h2>Segna assemblea come svolta</h2>
          <button class="btn-icon" @click="showCloseModal=false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-group">
            <label class="form-label">Data effettiva *</label>
            <input class="form-input" type="datetime-local" v-model="closeForm.actualDate" />
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showCloseModal=false">Annulla</button>
          <button class="btn btn-primary" @click="confirmClose" :disabled="savingClose">
            <span v-if="savingClose" class="spinner" style="width:14px;height:14px"></span>
            Conferma
          </button>
        </div>
      </div>
    </div>

  </div>
  <div v-else-if="loading" class="loading-state"><div class="spinner"></div> Caricamento assemblea…</div>
  <div v-else class="empty-state">Assemblea non trovata.</div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAppStore } from '@/stores/app'
import { usePermissions } from '@/composables/usePermissions'
import { assemblyApi, unitOwnerApi, fiscalYearApi } from '@/services/api'

const route  = useRoute()
const router = useRouter()
const store  = useAppStore()
const { canCreate, canEdit, canDelete } = usePermissions()

const assemblyId       = computed(() => Number(route.params.assemblyId || route.params.id))
const communicationPath = computed(() => `/condomini/${route.params.id}/comunicazioni`)

// ── State ──────────────────────────────────────────────────────────────────
const loading            = ref(false)
const loadingAgenda      = ref(false)
const loadingAttendances = ref(false)
const assembly           = ref(null)
const agendaItems        = ref([])
const attendances        = ref([])
const allOwners          = ref([])
const tab                = ref('odg')

// Agenda modal
const showAgendaModal  = ref(false)
const editingAgenda    = ref(null)
const savingAgenda     = ref(false)
const agendaErrors     = ref({})
const defaultAgendaForm = () => ({ title: '', description: '', orderIndex: agendaItems.value.length + 1, voteResultId: 1, resolution: '' })
const agendaForm       = ref(defaultAgendaForm())

// Attendance modal
const showAttendanceModal    = ref(false)
const editingAttendance      = ref(null)
const editingAttendanceData  = ref(null)
const savingAttendance       = ref(false)
const attendanceErrors       = ref({})
const defaultAttendanceForm  = () => ({ unitOwnerId: null, attendanceTypeId: 1, millesimalValue: 0, delegateName: '', notes: '' })
const attendanceForm         = ref(defaultAttendanceForm())

// Close modal
const showCloseModal = ref(false)
const savingClose    = ref(false)
const closeForm      = ref({ actualDate: '' })

// Minutes
const savingMinutes = ref(false)
const minutesForm   = ref({ boardMembers: '', minutes: '' })

// Edit modal
const showEditModal = ref(false)
const savingEdit    = ref(false)
const editErrors    = ref({})
const fiscalYears   = ref([])
const defaultEditForm = () => ({ title: '', assemblyTypeId: 1, fiscalYearId: null, scheduledDate: '', location: '', notes: '' })
const editForm      = ref(defaultEditForm())

// ── Computed ───────────────────────────────────────────────────────────────
const totMillesimi = computed(() =>
  attendances.value.filter(a => a.attendanceTypeId !== 3).reduce((s, a) => s + (a.millesimalValue ?? 0), 0)
)

const registeredOwnerIds = computed(() => new Set(attendances.value.map(a => a.unitOwnerId)))

const availableOwners = computed(() =>
  allOwners.value.filter(o => !registeredOwnerIds.value.has(o.id))
)

// ── Helpers ────────────────────────────────────────────────────────────────
function fmtDate(d) {
  if (!d) return '—'
  return new Date(d).toLocaleString('it-IT', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' })
}
function toLocalDatetimeInput(d) {
  if (!d) return ''
  const dt = new Date(d)
  const pad = n => String(n).padStart(2, '0')
  return `${dt.getFullYear()}-${pad(dt.getMonth()+1)}-${pad(dt.getDate())}T${pad(dt.getHours())}:${pad(dt.getMinutes())}`
}
function statusClass(id) {
  if (id === 4) return 'badge-green'
  if (id === 5) return 'badge-muted'
  if (id === 3) return 'badge-blue'
  if (id === 2) return 'badge-purple'
  return 'badge-gray'  // Bozza = 1
}
function voteClass(id) {
  if (id === 2) return 'badge-green'   // Approvato
  if (id === 3) return 'badge-muted'   // Respinto
  if (id === 4) return 'badge-yellow'  // Rinviato
  return 'badge-gray'                  // NonVotato = 1
}
function attendanceClass(id) {
  if (id === 1) return 'badge-green'  // Presente
  if (id === 2) return 'badge-blue'   // Delegato
  return 'badge-muted'                // Assente = 3
}

// ── Load ───────────────────────────────────────────────────────────────────
async function loadAssembly() {
  loading.value = true
  try {
    const { data } = await assemblyApi.getById(assemblyId.value)
    assembly.value = data
    minutesForm.value = { boardMembers: data.boardMembers ?? '', minutes: data.minutes ?? '' }
    await Promise.all([loadAgendaItems(), loadAttendances()])
    if (assembly.value?.condominiumId) {
      loadOwners(assembly.value.condominiumId)
      loadFiscalYears(assembly.value.condominiumId)
    }
  } catch { /* handled globally */ }
  finally { loading.value = false }
}

async function loadFiscalYears(condominiumId) {
  try {
    const { data } = await fiscalYearApi.getByCondominium(condominiumId)
    fiscalYears.value = data ?? []
  } catch { /* */ }
}

async function loadAgendaItems() {
  loadingAgenda.value = true
  try {
    const { data } = await assemblyApi.getAgendaItems(assemblyId.value)
    agendaItems.value = data ?? []
  } catch { /* */ }
  finally { loadingAgenda.value = false }
}

async function loadAttendances() {
  loadingAttendances.value = true
  try {
    const { data } = await assemblyApi.getAttendances(assemblyId.value)
    attendances.value = data ?? []
  } catch { /* */ }
  finally { loadingAttendances.value = false }
}

async function prepopulateAttendances() {
  loadingAttendances.value = true
  try {
    const { data } = await assemblyApi.prepopulateAttendances(assemblyId.value)
    attendances.value = data ?? []
    store.toast('Presenze aggiornate', 'success')
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    loadingAttendances.value = false
  }
}

async function loadOwners(condominiumId) {
  try {
    const { data } = await unitOwnerApi.getByCondominium(condominiumId)
    allOwners.value = (data ?? []).map(o => ({
      ...o,
      unitInternalNumber: o.unitInternalNumber ?? o.unit?.internalNumber,
    }))
  } catch { /* */ }
}

// ── Agenda ─────────────────────────────────────────────────────────────────
function openAgendaModal(item = null) {
  editingAgenda.value = item?.id ?? null
  agendaErrors.value  = {}
  agendaForm.value = item ? {
    title:        item.title,
    description:  item.description ?? '',
    orderIndex:   item.orderIndex,
    voteResultId: item.voteResultId,
    resolution:   item.resolution ?? '',
  } : defaultAgendaForm()
  showAgendaModal.value = true
}

async function saveAgendaItem() {
  if (!agendaForm.value.title?.trim()) {
    agendaErrors.value = { title: 'Il titolo è obbligatorio' }
    return
  }
  savingAgenda.value = true
  try {
    if (editingAgenda.value) {
      await assemblyApi.updateAgendaItem(editingAgenda.value, agendaForm.value)
      store.toast('Punto aggiornato', 'success')
    } else {
      await assemblyApi.createAgendaItem({ ...agendaForm.value, assemblyId: assemblyId.value })
      store.toast('Punto aggiunto', 'success')
    }
    showAgendaModal.value = false
    await loadAgendaItems()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    savingAgenda.value = false
  }
}

async function deleteAgendaItem(id) {
  if (!confirm('Eliminare questo punto OdG?')) return
  try {
    await assemblyApi.deleteAgendaItem(id)
    store.toast('Punto eliminato', 'success')
    await loadAgendaItems()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

// ── Attendances ────────────────────────────────────────────────────────────
function openAttendanceModal(item = null) {
  editingAttendance.value     = item?.id ?? null
  editingAttendanceData.value = item ?? null
  attendanceErrors.value      = {}
  attendanceForm.value = item ? {
    unitOwnerId:      item.unitOwnerId,
    attendanceTypeId: item.attendanceTypeId,
    millesimalValue:  item.millesimalValue,
    delegateName:     item.delegateName ?? '',
    notes:            item.notes        ?? '',
  } : defaultAttendanceForm()
  showAttendanceModal.value = true
}

async function saveAttendance() {
  if (!editingAttendance.value && !attendanceForm.value.unitOwnerId) {
    attendanceErrors.value = { unitOwnerId: 'Seleziona un proprietario' }
    return
  }
  savingAttendance.value = true
  try {
    if (editingAttendance.value) {
      await assemblyApi.updateAttendance(editingAttendance.value, attendanceForm.value)
      store.toast('Presenza aggiornata', 'success')
    } else {
      await assemblyApi.createAttendance({ ...attendanceForm.value, assemblyId: assemblyId.value })
      store.toast('Presenza registrata', 'success')
    }
    showAttendanceModal.value = false
    await loadAttendances()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    savingAttendance.value = false
  }
}

async function deleteAttendance(id) {
  if (!confirm('Rimuovere questa presenza?')) return
  try {
    await assemblyApi.deleteAttendance(id)
    store.toast('Presenza rimossa', 'success')
    await loadAttendances()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

// ── Plan ───────────────────────────────────────────────────────────────────
async function planAssembly() {
  if (!confirm(`Spostare l'assemblea in Pianificata? Verrà creata la comunicazione di convocazione e generate le notifiche.`)) return
  try {
    const { data } = await assemblyApi.plan(assemblyId.value)
    assembly.value = data
    store.toast('Assemblea spostata in Pianificata', 'success')
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

// ── Edit ───────────────────────────────────────────────────────────────────
function openEditModal() {
  const a = assembly.value
  editForm.value = {
    title:          a.title,
    assemblyTypeId: a.assemblyTypeId,
    fiscalYearId:   a.fiscalYearId ?? null,
    scheduledDate:  toLocalDatetimeInput(a.scheduledDate),
    location:       a.location ?? '',
    notes:          a.notes    ?? '',
  }
  editErrors.value = {}
  showEditModal.value = true
}

async function saveEdit() {
  const e = {}
  if (!editForm.value.title?.trim()) e.title = 'Il titolo è obbligatorio'
  if (!editForm.value.scheduledDate)  e.scheduledDate = 'La data è obbligatoria'
  editErrors.value = e
  if (Object.keys(e).length) return

  savingEdit.value = true
  try {
    const { data } = await assemblyApi.update(assemblyId.value, editForm.value)
    assembly.value = data
    showEditModal.value = false
    store.toast('Assemblea aggiornata', 'success')
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    savingEdit.value = false
  }
}

// ── Delete ─────────────────────────────────────────────────────────────────
async function deleteAssembly() {
  if (!confirm('Eliminare questa assemblea? L\'operazione è irreversibile.')) return
  try {
    await assemblyApi.delete(assemblyId.value)
    store.toast('Assemblea eliminata', 'success')
    router.push(`/condomini/${route.params.id}/assemblee`)
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

// ── Close / Cancel ─────────────────────────────────────────────────────────
function openCloseModal() {
  closeForm.value = { actualDate: toLocalDatetimeInput(assembly.value?.scheduledDate) }
  showCloseModal.value = true
}

async function confirmClose() {
  if (!closeForm.value.actualDate) return
  savingClose.value = true
  try {
    const { data } = await assemblyApi.close(assemblyId.value, { actualDate: closeForm.value.actualDate })
    assembly.value = data
    minutesForm.value = { boardMembers: data.boardMembers ?? '', minutes: data.minutes ?? '' }
    store.toast('Assemblea segnata come svolta', 'success')
    showCloseModal.value = false
    tab.value = 'verbale'
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    savingClose.value = false
  }
}

async function cancelAssembly() {
  if (!confirm('Annullare questa assemblea?')) return
  try {
    const { data } = await assemblyApi.cancel(assemblyId.value)
    assembly.value = data
    store.toast('Assemblea annullata', 'success')
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

// ── Minutes ────────────────────────────────────────────────────────────────
async function saveMinutes() {
  savingMinutes.value = true
  try {
    const { data } = await assemblyApi.saveMinutes(assemblyId.value, minutesForm.value)
    assembly.value = data
    store.toast('Verbale salvato', 'success')
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    savingMinutes.value = false
  }
}

onMounted(loadAssembly)
</script>

<style scoped>
.assembly-detail { display: flex; flex-direction: column; gap: 1.25rem; }

.detail-header { padding: 1.25rem 1.5rem; }
.detail-header-main { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; margin-bottom: 1rem; }
.detail-title { font-size: 1.25rem; font-weight: 600; margin: 0 0 .4rem; }
.detail-meta { display: flex; align-items: center; gap: .5rem; flex-wrap: wrap; }
.detail-actions { display: flex; gap: .5rem; flex-shrink: 0; }

.detail-info-grid { display: flex; gap: 2rem; flex-wrap: wrap; }
.info-item { display: flex; flex-direction: column; gap: .15rem; }
.info-label { font-size: .75rem; color: var(--text-muted); text-transform: uppercase; letter-spacing: .04em; }
.info-value { font-size: .9rem; }

.tab-pills { display: flex; gap: 4px; }
.tab-pill { padding: 6px 14px; border-radius: 6px; border: 1px solid var(--border); background: transparent; color: var(--text-secondary); cursor: pointer; font-size: .875rem; display: flex; align-items: center; gap: .4rem; }
.tab-pill.active { background: var(--accent); color: #fff; border-color: var(--accent); }

.agenda-list { display: flex; flex-direction: column; }
.agenda-item { display: flex; align-items: flex-start; gap: 1rem; padding: .875rem 1rem; border-bottom: 1px solid var(--border); }
.agenda-item:last-child { border-bottom: none; }
.agenda-number { min-width: 28px; height: 28px; border-radius: 50%; background: var(--bg-surface); border: 1px solid var(--border); display: flex; align-items: center; justify-content: center; font-size: .8rem; font-weight: 600; color: var(--text-secondary); flex-shrink: 0; }
.agenda-content { flex: 1; }
.agenda-title { font-weight: 500; }
.agenda-desc { font-size: .875rem; margin-top: .2rem; }
.agenda-resolution { margin-top: .4rem; font-size: .875rem; background: var(--bg-surface); border-left: 3px solid var(--accent-green); padding: .4rem .75rem; border-radius: 0 4px 4px 0; }
.resolution-label { font-weight: 600; color: var(--accent-green); }
.agenda-vote { flex-shrink: 0; }

.badge-blue   { background: rgba(99,102,241,.12);  color: #6366f1; }
.badge-purple { background: rgba(168,85,247,.12);  color: #9333ea; }
.badge-gray   { background: rgba(107,114,128,.12); color: #6b7280; }
.badge-yellow { background: rgba(234,179,8,.12);   color: #b45309; }

.quorum-info strong { color: var(--text); }

.info-link { color: var(--accent); text-decoration: none; font-size: .9rem; display: flex; align-items: center; gap: .3rem; }
.info-link:hover { text-decoration: underline; }

.minutes-editor { font-family: var(--font-mono, monospace); font-size: .875rem; line-height: 1.6; resize: vertical; }
</style>
