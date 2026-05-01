<template>
  <div>
    <div class="toolbar">
      <select class="form-select filter-select" v-model="filterStatus">
        <option value="">Tutti gli stati</option>
        <option value="0">Convocata</option>
        <option value="1">Svolta</option>
        <option value="2">Annullata</option>
      </select>
      <select class="form-select filter-select" v-model="filterType">
        <option value="">Tutti i tipi</option>
        <option value="0">Ordinaria</option>
        <option value="1">Straordinaria</option>
      </select>
      <button v-if="canCreate" class="btn btn-primary" style="margin-left:auto" @click="openModal()">+ Nuova assemblea</button>
    </div>

    <div class="card">
      <div v-if="loading" class="loading-state"><div class="spinner"></div> Caricamento…</div>
      <div v-else-if="!filtered.length" class="empty-state">
        <div class="empty-icon"><i class="pi pi-microphone"></i></div>
        <div>Nessuna assemblea trovata</div>
      </div>
      <div v-else class="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Titolo</th>
              <th>Tipo</th>
              <th>Stato</th>
              <th>Data convocata</th>
              <th>Data effettiva</th>
              <th>Luogo</th>
              <th class="text-center">OdG</th>
              <th class="text-center">Presenze</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="a in filtered" :key="a.id" class="clickable-row" @click="goToDetail(a.id)">
              <td style="font-weight:500">{{ a.title }}</td>
              <td class="text-secondary">{{ a.assemblyTypeName }}</td>
              <td><span class="badge" :class="statusClass(a.statusId)">{{ a.statusName }}</span></td>
              <td>{{ fmtDate(a.scheduledDate) }}</td>
              <td>{{ a.actualDate ? fmtDate(a.actualDate) : '—' }}</td>
              <td class="text-secondary">{{ a.location || '—' }}</td>
              <td class="text-center text-secondary">{{ a.agendaItemCount }}</td>
              <td class="text-center text-secondary">{{ a.attendanceCount }}</td>
              <td @click.stop>
                <div class="row-actions">
                  <button v-if="canEdit && a.statusId === 0" class="btn-icon" @click="openCloseModal(a)" title="Segna come svolta">✓</button>
                  <button v-if="canEdit && a.statusId === 0" class="btn-icon" style="color:var(--accent-red)" @click="cancelAssembly(a)" title="Annulla assemblea"><i class="pi pi-ban"></i></button>
                  <button v-if="canEdit" class="btn-icon" @click="openModal(a)" title="Modifica">✎</button>
                  <button v-if="canDelete" class="btn-icon" style="color:var(--accent-red)" @click="deleteItem(a.id)" title="Elimina">✕</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
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
                <option :value="0">Ordinaria</option>
                <option :value="1">Straordinaria</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Esercizio fiscale</label>
              <select class="form-select" v-model.number="form.fiscalYearId">
                <option :value="null">— Nessuno —</option>
                <option v-for="fy in fiscalYears" :key="fy.id" :value="fy.id">{{ fy.name }}</option>
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
  assemblyTypeId: 0,
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
  if (statusId === 1) return 'badge-green'
  if (statusId === 2) return 'badge-muted'
  return 'badge-blue'
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

onMounted(loadData)
onUnmounted(() => window.removeEventListener('app:refresh', loadData))
window.addEventListener('app:refresh', loadData)
watch(condominiumId, loadData)
</script>

<style scoped>
.clickable-row { cursor: pointer; }
.clickable-row:hover td { background: var(--bg-surface); }

.badge-blue { background: rgba(99,102,241,.12); color: #6366f1; }

.filter-select { max-width: 160px; }
</style>
