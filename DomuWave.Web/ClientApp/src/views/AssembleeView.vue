<template>
  <div>
    <div class="toolbar">
      <select class="form-select filter-select" v-model="filterStatus">
        <option value="">Tutti gli stati</option>
        <option value="1">Bozza</option>
        <option value="2">Pianificata</option>
        <option value="3">Convocata</option>
        <option value="10">In corso</option>
        <option value="4">Svolta</option>
        <option value="5">Annullata</option>
      </select>
      <select class="form-select filter-select" v-model="filterType">
        <option value="">Tutti i tipi</option>
        <option value="1">Ordinaria</option>
        <option value="2">Straordinaria</option>
      </select>
      <button v-if="canCreate" class="btn btn-primary" style="margin-left:auto" @click="openModal()">+ Nuova assemblea</button>
    </div>

    <div v-if="loading" class="loading-state"><div class="spinner"></div> Caricamento…</div>
    <div v-else-if="!filtered.length" class="empty-state card">
      <div class="empty-icon"><i class="pi pi-microphone"></i></div>
      <div>Nessuna assemblea trovata</div>
    </div>
    <div v-else class="assembly-list">
      <div v-for="a in filtered" :key="a.id" class="assembly-card card" @click="goToDetail(a.id)">
        <div class="assembly-card-main">
          <div class="assembly-card-info">
            <div class="assembly-card-title">{{ a.title }}</div>
            <div class="assembly-card-meta">
              <span class="badge" :class="statusClass(a.statusId)">{{ a.statusName }}</span>
              <span class="meta-chip">{{ a.assemblyTypeName }}</span>
              <span v-if="a.fiscalYearName" class="meta-chip">{{ a.fiscalYearName }}</span>
            </div>
          </div>
          <div class="assembly-card-dates">
            <div class="date-item">
              <span class="date-label">Convocata</span>
              <span class="date-value">{{ fmtDate(a.scheduledDate) }}</span>
            </div>
            <div v-if="a.actualDate" class="date-item">
              <span class="date-label">Svolta</span>
              <span class="date-value">{{ fmtDate(a.actualDate) }}</span>
            </div>
            <div v-if="a.location" class="date-item">
              <span class="date-label">Luogo</span>
              <span class="date-value">{{ a.location }}</span>
            </div>
          </div>
          <div class="assembly-card-counts">
            <div class="count-chip"><i class="pi pi-list"></i> {{ a.agendaItemCount }} punti OdG</div>
            <div class="count-chip"><i class="pi pi-users"></i> {{ a.attendanceCount }} presenze</div>
          </div>
          <div class="assembly-card-actions" @click.stop>
            <button v-if="canEdit && a.statusId === 1" class="btn-icon" @click="planAssembly(a)" title="Sposta in Pianificata"><i class="pi pi-send"></i></button>
            <button v-if="canEdit && a.statusId === 10" class="btn-icon" @click="openCloseModal(a)" title="Segna come svolta"><i class="pi pi-check"></i></button>
            <button v-if="canEdit && [1,2,3].includes(a.statusId)" class="btn-icon" style="color:var(--accent-red)" @click="cancelAssembly(a)" title="Annulla assemblea"><i class="pi pi-ban"></i></button>
            <button v-if="canEdit" class="btn-icon" @click="openModal(a)" title="Modifica">✎</button>
            <button v-if="canDelete" class="btn-icon" style="color:var(--accent-red)" @click="deleteItem(a.id)" title="Elimina">✕</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal crea/modifica assemblea -->
    <div class="modal-overlay" v-if="showModal" @click.self="showModal=false">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ editing ? 'Modifica' : 'Nuova' }} assemblea</h2>
          <button class="btn-icon" @click="showModal=false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-grid">
            <div class="form-group form-group--full" :class="{ 'has-error': errors.title }">
              <label class="form-label">Titolo *</label>
              <input class="form-input" v-model="form.title" @input="clearError('title')" placeholder="es. Assemblea ordinaria 2025" />
              <span v-if="errors.title" class="field-error">{{ errors.title }}</span>
            </div>
            <div class="form-group">
              <label class="form-label">Tipo *</label>
              <select class="form-select" v-model.number="form.assemblyTypeId">
                <option :value="1">Ordinaria</option>
                <option :value="2">Straordinaria</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Esercizio fiscale</label>
              <select class="form-select" v-model.number="form.fiscalYearId">
                <option :value="null">— Nessuno —</option>
                <option v-for="fy in fiscalYears" :key="fy.id" :value="fy.id">{{ fy.code }}{{ fy.description ? ' – ' + fy.description : '' }}</option>
              </select>
            </div>
            <div class="form-group" :class="{ 'has-error': errors.scheduledDate }">
              <label class="form-label">Data convocata *</label>
              <input class="form-input" type="datetime-local" v-model="form.scheduledDate" @input="clearError('scheduledDate')" />
              <span v-if="errors.scheduledDate" class="field-error">{{ errors.scheduledDate }}</span>
            </div>
            <div class="form-group">
              <label class="form-label">Luogo</label>
              <input class="form-input" v-model="form.location" placeholder="es. Sala riunioni condominio" />
            </div>
            <div class="form-group form-group--full">
              <label class="form-label">Note</label>
              <textarea class="form-textarea" v-model="form.notes" rows="3"></textarea>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showModal=false">Annulla</button>
          <button class="btn btn-primary" @click="save" :disabled="saving">
            <span v-if="saving" class="spinner" style="width:14px;height:14px"></span>
            {{ editing ? 'Salva' : 'Crea' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Modal chiusura assemblea -->
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
          <button class="btn btn-primary" @click="confirmClose" :disabled="saving">
            <span v-if="saving" class="spinner" style="width:14px;height:14px"></span>
            Conferma
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAppStore } from '@/stores/app'
import { usePermissions } from '@/composables/usePermissions'
import { assemblyApi, fiscalYearApi } from '@/services/api'

const route  = useRoute()
const router = useRouter()
const store  = useAppStore()
const { canCreate, canEdit, canDelete } = usePermissions()

const condominiumId = computed(() => Number(route.params.id) || store.selectedCondominioId)

// ── State ──────────────────────────────────────────────────────────────────
const loading       = ref(false)
const saving        = ref(false)
const assemblies    = ref([])
const fiscalYears   = ref([])
const filterStatus  = ref('')
const filterType    = ref('')
const showModal     = ref(false)
const showCloseModal = ref(false)
const editing       = ref(null)
const errors        = ref({})
const closingAssembly = ref(null)

const defaultForm = () => ({
  title:          '',
  assemblyTypeId: 1,
  fiscalYearId:   null,
  scheduledDate:  '',
  location:       '',
  notes:          '',
})
const form      = ref(defaultForm())
const closeForm = ref({ actualDate: '' })

// ── Computed ───────────────────────────────────────────────────────────────
const filtered = computed(() => {
  let list = assemblies.value
  if (filterStatus.value !== '') list = list.filter(a => String(a.statusId) === filterStatus.value)
  if (filterType.value   !== '') list = list.filter(a => String(a.assemblyTypeId) === filterType.value)
  return list
})

// ── Helpers ────────────────────────────────────────────────────────────────
function fmtDate(d) {
  if (!d) return '—'
  return new Date(d).toLocaleString('it-IT', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' })
}

function statusClass(statusId) {
  if (statusId === 4)  return 'badge-green'
  if (statusId === 5)  return 'badge-muted'
  if (statusId === 10) return 'badge-orange'
  if (statusId === 3)  return 'badge-blue'
  if (statusId === 2)  return 'badge-purple'
  return 'badge-gray'   // Bozza = 1
}

function toLocalDatetimeInput(d) {
  if (!d) return ''
  const dt = new Date(d)
  const pad = n => String(n).padStart(2, '0')
  return `${dt.getFullYear()}-${pad(dt.getMonth()+1)}-${pad(dt.getDate())}T${pad(dt.getHours())}:${pad(dt.getMinutes())}`
}

// ── Load ───────────────────────────────────────────────────────────────────
async function loadData() {
  if (!condominiumId.value) return
  loading.value = true
  try {
    const [asRes, fyRes] = await Promise.allSettled([
      assemblyApi.getByCondominium(condominiumId.value),
      fiscalYearApi.getByCondominium(condominiumId.value),
    ])
    assemblies.value  = asRes.status  === 'fulfilled' ? (asRes.value.data  ?? []) : []
    fiscalYears.value = fyRes.status  === 'fulfilled' ? (fyRes.value.data  ?? []) : []
  } finally {
    loading.value = false
  }
}

// ── Modal ──────────────────────────────────────────────────────────────────
function openModal(item = null) {
  editing.value = item?.id ?? null
  errors.value  = {}
  form.value    = item ? {
    title:          item.title,
    assemblyTypeId: item.assemblyTypeId,
    fiscalYearId:   item.fiscalYearId ?? null,
    scheduledDate:  toLocalDatetimeInput(item.scheduledDate),
    location:       item.location ?? '',
    notes:          item.notes    ?? '',
  } : defaultForm()
  showModal.value = true
}

function openCloseModal(assembly) {
  closingAssembly.value = assembly
  closeForm.value = { actualDate: toLocalDatetimeInput(assembly.scheduledDate) }
  showCloseModal.value = true
}

function clearError(field) { delete errors.value[field] }

function validate() {
  const e = {}
  if (!form.value.title?.trim()) e.title = 'Il titolo è obbligatorio'
  if (!form.value.scheduledDate) e.scheduledDate = 'La data è obbligatoria'
  errors.value = e
  return Object.keys(e).length === 0
}

// ── Save ───────────────────────────────────────────────────────────────────
async function save() {
  if (!validate()) return
  saving.value = true
  try {
    const payload = { ...form.value, condominiumId: condominiumId.value }
    if (editing.value) {
      await assemblyApi.update(editing.value, payload)
      store.toast('Assemblea aggiornata', 'success')
    } else {
      await assemblyApi.create(payload)
      store.toast('Assemblea creata', 'success')
    }
    showModal.value = false
    await loadData()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    saving.value = false
  }
}

async function confirmClose() {
  if (!closeForm.value.actualDate) return
  saving.value = true
  try {
    await assemblyApi.close(closingAssembly.value.id, { actualDate: closeForm.value.actualDate })
    store.toast('Assemblea segnata come svolta', 'success')
    showCloseModal.value = false
    await loadData()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    saving.value = false
  }
}

async function planAssembly(assembly) {
  if (!confirm(`Spostare l'assemblea "${assembly.title}" in Pianificata?`)) return
  try {
    await assemblyApi.plan(assembly.id)
    store.toast('Assemblea spostata in Pianificata', 'success')
    await loadData()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

async function cancelAssembly(assembly) {
  if (!confirm(`Annullare l'assemblea "${assembly.title}"?`)) return
  try {
    await assemblyApi.cancel(assembly.id)
    store.toast('Assemblea annullata', 'success')
    await loadData()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

async function deleteItem(id) {
  if (!confirm('Eliminare questa assemblea?')) return
  try {
    await assemblyApi.delete(id)
    store.toast('Assemblea eliminata', 'success')
    await loadData()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

function goToDetail(id) {
  if (route.params.id) {
    router.push(`/condomini/${route.params.id}/assemblee/${id}`)
  } else {
    router.push(`/assemblee/${id}`)
  }
}

onMounted(async () => {
  await loadData()
  // Apertura modale precompilato da query string (?createOrdinaria=1&fiscalYearId=X)
  if (route.query.createOrdinaria && canCreate.value) {
    const fyId = Number(route.query.fiscalYearId) || null
    form.value = { ...defaultForm(), assemblyTypeId: 1, fiscalYearId: fyId }
    showModal.value = true
    // Pulisci la query string senza ricaricare
    router.replace({ query: {} })
  }
})
onUnmounted(() => window.removeEventListener('app:refresh', loadData))
window.addEventListener('app:refresh', loadData)
watch(condominiumId, loadData)
</script>

<style scoped>
.toolbar { display: flex; gap: .75rem; align-items: center; flex-wrap: wrap; margin-bottom: 1.25rem; }
.filter-select { max-width: 160px; }

.badge-blue   { background: rgba(99,102,241,.12);  color: #6366f1; }
.badge-purple { background: rgba(168,85,247,.12);  color: #a855f7; }
.badge-gray   { background: rgba(107,114,128,.12); color: #6b7280; }
.badge-orange { background: rgba(249,115,22,.15);  color: #ea580c; }

.assembly-list { display: flex; flex-direction: column; gap: .75rem; }

.assembly-card {
  padding: 1rem 1.25rem;
  cursor: pointer;
  transition: border-color .15s, box-shadow .15s;
}
.assembly-card:hover { border-color: var(--border-active); box-shadow: 0 2px 8px rgba(0,0,0,.08); }

.assembly-card-main {
  display: flex;
  align-items: center;
  gap: 1.5rem;
}

.assembly-card-info { flex: 1; min-width: 0; }
.assembly-card-title { font-size: 1rem; font-weight: 600; margin-bottom: .35rem; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.assembly-card-meta { display: flex; align-items: center; gap: .4rem; flex-wrap: wrap; }
.meta-chip { font-size: .78rem; color: var(--text-secondary); background: var(--bg-surface); border: 1px solid var(--border); border-radius: 4px; padding: 1px 7px; }

.assembly-card-dates { display: flex; gap: 1.5rem; flex-shrink: 0; }
.date-item { display: flex; flex-direction: column; gap: .1rem; }
.date-label { font-size: .7rem; text-transform: uppercase; letter-spacing: .05em; color: var(--text-muted); }
.date-value { font-size: .85rem; color: var(--text); white-space: nowrap; }

.assembly-card-counts { display: flex; flex-direction: column; gap: .3rem; flex-shrink: 0; }
.count-chip { font-size: .78rem; color: var(--text-secondary); display: flex; align-items: center; gap: .35rem; }

.assembly-card-actions { display: flex; align-items: center; gap: .15rem; flex-shrink: 0; }
</style>
