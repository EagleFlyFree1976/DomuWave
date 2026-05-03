<template>
  <div>
    <div class="page-header">
      <h1>Manutenzioni</h1>
      <button v-if="canCreate && store.selectedCondominioId" class="btn btn-primary" @click="openModal()">+ Nuova manutenzione</button>
    </div>

    <div class="toolbar">
      <select class="form-select" v-model="statusFilter" style="width:180px">
        <option value="all">Tutti gli stati</option>
        <option value="open">Aperte</option>
        <option value="Open">In attesa</option>
        <option value="InProgress">In corso</option>
        <option value="Completed">Completate</option>
        <option value="Cancelled">Annullate</option>
      </select>
      <select class="form-select" v-model="priorityFilter" style="width:160px">
        <option value="">Tutte le priorità</option>
        <option value="Low">Bassa</option>
        <option value="Medium">Media</option>
        <option value="High">Alta</option>
        <option value="Critical">Critica</option>
      </select>
    </div>

    <div class="card">
      <div v-if="loading" class="loading-state"><div class="spinner"></div></div>
      <div v-else-if="!items.length" class="empty-state">
        <div class="empty-icon">◈</div><div>Nessuna manutenzione trovata</div>
      </div>
      <div v-else class="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Titolo</th>
              <th>Categoria</th>
              <th>Priorità</th>
              <th>Stato</th>
              <th>Fornitore</th>
              <th>Data segnalazione</th>
              <th>Data prevista</th>
              <th>Costo stimato</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="m in items" :key="m.id">
              <td style="font-weight:500">{{ m.title }}</td>
              <td class="text-secondary">{{ m.category || '—' }}</td>
              <td><span class="badge" :class="priorityBadge(m.priority)">{{ priorityLabel(m.priority) }}</span></td>
              <td><span class="badge" :class="statusBadge(m.status)">{{ statusLabel(m.status) }}</span></td>
              <td class="text-secondary">{{ m.supplierName || '—' }}</td>
              <td class="mono text-secondary">{{ fmtDate(m.reportedDate) }}</td>
              <td class="mono text-secondary">{{ fmtDate(m.scheduledDate) }}</td>
              <td class="mono">{{ m.estimatedCost ? fmt(m.estimatedCost) : '—' }}</td>
              <td>
                <div v-if="canEdit || canDelete" class="row-actions">
                  <button v-if="canEdit" class="btn-icon" @click="openModal(m)">✎</button>
                  <button v-if="canDelete" class="btn-icon" @click="deleteItem(m.id)" style="color:var(--accent-red)">✕</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Modal -->
    <div class="modal-overlay" v-if="showModal" @click.self="showModal=false">
      <div class="modal modal-lg">
        <div class="modal-header">
          <h2>{{ editing ? 'Modifica' : 'Nuova' }} manutenzione</h2>
          <button class="btn-icon" @click="showModal=false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-grid">
            <div class="form-group" style="grid-column:1/-1" :class="{ 'has-error': errors.title }">
              <label class="form-label">Titolo *</label>
              <input class="form-input" v-model="form.title" @input="errors.title = null" />
              <span v-if="errors.title" class="field-error">{{ errors.title }}</span>
            </div>
            <div class="form-group">
              <label class="form-label">Categoria</label>
              <select class="form-select" v-model="form.category">
                <option value="">—</option>
                <option value="Elettrico">Elettrico</option>
                <option value="Idraulico">Idraulico</option>
                <option value="Ascensore">Ascensore</option>
                <option value="Riscaldamento">Riscaldamento</option>
                <option value="Strutturale">Strutturale</option>
                <option value="Verde">Verde / Giardino</option>
                <option value="Pulizie">Pulizie</option>
                <option value="Sicurezza">Sicurezza</option>
                <option value="Altro">Altro</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Priorità</label>
              <select class="form-select" v-model="form.priority">
                <option value="Low">Bassa</option>
                <option value="Medium">Media</option>
                <option value="High">Alta</option>
                <option value="Critical">Critica</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Stato</label>
              <select class="form-select" v-model="form.status">
                <option value="Open">In attesa</option>
                <option value="InProgress">In corso</option>
                <option value="Completed">Completata</option>
                <option value="Cancelled">Annullata</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Fornitore</label>
              <select class="form-select" v-model.number="form.supplierId">
                <option :value="null">— Nessuno —</option>
                <option v-for="s in suppliers" :key="s.id" :value="s.id">{{ s.companyName }}</option>
              </select>
            </div>
            <div class="form-group" :class="{ 'has-error': errors.reportedDate }">
              <label class="form-label">Data segnalazione *</label>
              <input class="form-input" type="date" v-model="form.reportedDate" @change="errors.reportedDate = null" />
              <span v-if="errors.reportedDate" class="field-error">{{ errors.reportedDate }}</span>
            </div>
            <div class="form-group">
              <label class="form-label">Data prevista</label>
              <input class="form-input" type="date" v-model="form.scheduledDate" />
            </div>
            <div class="form-group">
              <label class="form-label">Data completamento</label>
              <input class="form-input" type="date" v-model="form.completedDate" />
            </div>
            <div class="form-group">
              <label class="form-label">Costo stimato (€)</label>
              <input class="form-input" type="number" step="0.01" v-model.number="form.estimatedCost" />
            </div>
            <div class="form-group">
              <label class="form-label">Costo effettivo (€)</label>
              <input class="form-input" type="number" step="0.01" v-model.number="form.actualCost" />
            </div>
            <div class="form-group" style="grid-column:1/-1">
              <label class="form-label">Descrizione</label>
              <textarea class="form-textarea" v-model="form.description" rows="3"></textarea>
            </div>
            <div class="form-group" style="grid-column:1/-1">
              <label class="form-label">Note</label>
              <textarea class="form-textarea" v-model="form.notes" rows="2"></textarea>
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
  </div>
</template>

<script setup>
import { ref, watch, onMounted, onUnmounted } from 'vue'
import { useAppStore } from '@/stores/app'
import { maintenanceApi, supplierApi } from '@/services/api'
import { usePermissions } from '@/composables/usePermissions'

const store = useAppStore()
const { canCreate, canEdit, canDelete } = usePermissions()

const items         = ref([])
const suppliers     = ref([])
const loading       = ref(false)
const saving        = ref(false)
const showModal     = ref(false)
const editing       = ref(null)
const statusFilter  = ref('all')
const priorityFilter = ref('')
const errors        = ref({})

const fmt     = (v) => v != null ? '€ ' + Number(v).toLocaleString('it-IT', { minimumFractionDigits: 2 }) : '—'
const fmtDate = (d) => d ? new Date(d).toLocaleDateString('it-IT') : '—'

const priorityLabel = (p) => ({ Low: 'Bassa', Medium: 'Media', High: 'Alta', Critical: 'Critica' }[p] || p || '—')
const priorityBadge = (p) => ({ Low: 'badge-muted', Medium: 'badge-blue', High: 'badge-amber', Critical: 'badge-red' }[p] || 'badge-muted')
const statusLabel   = (s) => ({ Open: 'In attesa', InProgress: 'In corso', Completed: 'Completata', Cancelled: 'Annullata' }[s] || s || '—')
const statusBadge   = (s) => ({ Open: 'badge-blue', InProgress: 'badge-amber', Completed: 'badge-green', Cancelled: 'badge-muted' }[s] || 'badge-muted')

const defaultForm = () => ({
  title: '', description: '', category: '', priority: 'Medium', status: 'Open',
  supplierId: null, reportedDate: '', scheduledDate: '', completedDate: '',
  estimatedCost: null, actualCost: null, notes: '', documentPath: '',
})
const form = ref(defaultForm())

async function loadData() {
  if (!store.selectedCondominioId) return
  loading.value = true
  try {
    let res
    if (statusFilter.value === 'open') {
      res = await maintenanceApi.getOpen(store.selectedCondominioId)
    } else if (statusFilter.value !== 'all') {
      res = await maintenanceApi.getByStatus(store.selectedCondominioId, statusFilter.value)
    } else if (priorityFilter.value) {
      res = await maintenanceApi.getByPriority(store.selectedCondominioId, priorityFilter.value)
    } else {
      res = await maintenanceApi.getByCondominium(store.selectedCondominioId)
    }
    let list = res.data
    if (priorityFilter.value && statusFilter.value === 'all') {
      // already filtered by priority from API
    } else if (priorityFilter.value) {
      list = list.filter(m => m.priority === priorityFilter.value)
    }
    items.value = list
  } catch (err) {
    items.value = []
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { loading.value = false }
}

async function loadSuppliers() {
  try {
    const { data } = await supplierApi.getAll()
    suppliers.value = data
  } catch { suppliers.value = [] }
}

function sanitize(f) {
  return {
    ...f,
    supplierId:    f.supplierId    || null,
    reportedDate:  f.reportedDate  || null,
    scheduledDate: f.scheduledDate || null,
    completedDate: f.completedDate || null,
    estimatedCost: f.estimatedCost !== '' ? f.estimatedCost : null,
    actualCost:    f.actualCost    !== '' ? f.actualCost    : null,
  }
}

function openModal(m = null) {
  editing.value = m?.id ?? null
  errors.value  = {}
  form.value    = m ? {
    title:         m.title,
    description:   m.description   || '',
    category:      m.category      || '',
    priority:      m.priority      || 'Medium',
    status:        m.status        || 'Open',
    supplierId:    m.supplierId    || null,
    reportedDate:  m.reportedDate  ? m.reportedDate.substring(0, 10) : '',
    scheduledDate: m.scheduledDate ? m.scheduledDate.substring(0, 10) : '',
    completedDate: m.completedDate ? m.completedDate.substring(0, 10) : '',
    estimatedCost: m.estimatedCost ?? null,
    actualCost:    m.actualCost    ?? null,
    notes:         m.notes         || '',
    documentPath:  m.documentPath  || '',
  } : defaultForm()
  showModal.value = true
}

async function save() {
  errors.value = {}
  if (!form.value.title) {
    errors.value.title = 'Il titolo è obbligatorio'
    return
  }
  if (!form.value.reportedDate) {
    errors.value.reportedDate = 'La data di segnalazione è obbligatoria'
    return
  }
  saving.value = true
  try {
    const payload = sanitize(form.value)
    if (editing.value) {
      await maintenanceApi.update(editing.value, payload)
    } else {
      await maintenanceApi.create({ ...payload, condominiumId: store.selectedCondominioId })
    }
    store.toast('Manutenzione salvata', 'success')
    showModal.value = false
    loadData()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { saving.value = false }
}

async function deleteItem(id) {
  if (!confirm('Eliminare la manutenzione?')) return
  try {
    await maintenanceApi.delete(id)
    store.toast('Manutenzione eliminata', 'success')
    loadData()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

watch(() => store.selectedCondominioId, loadData)
watch(statusFilter,  loadData)
watch(priorityFilter, loadData)
function loadAll() { loadData(); loadSuppliers() }
onMounted(loadAll)
onUnmounted(() => window.removeEventListener('app:refresh', loadAll))
window.addEventListener('app:refresh', loadAll)
</script>

<style scoped>
.toolbar { display: flex; gap: 0.75rem; margin-bottom: 1rem; flex-wrap: wrap; align-items: center; }
.row-actions { display: flex; gap: 0.4rem; justify-content: flex-end; }
.modal-lg { max-width: 720px; width: 100%; }
</style>
