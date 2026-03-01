<template>
  <div>
    <div class="page-header">
      <h1>Comunicazioni</h1>
      <button v-if="canCreate" class="btn btn-primary" @click="openModal()">+ Nuova comunicazione</button>
    </div>

    <div class="toolbar">
      <input class="form-input search-input" v-model="search" placeholder="Cerca per titolo…" />
      <select class="form-select" v-model="filterType" style="width:160px">
        <option value="">Tutti i tipi</option>
        <option value="Notice">Avviso</option>
        <option value="Meeting">Assemblea</option>
        <option value="Maintenance">Manutenzione</option>
        <option value="Emergency">Urgente</option>
        <option value="Info">Informazione</option>
      </select>
      <select class="form-select" v-model="filterPriority" style="width:140px">
        <option value="">Tutte le priorità</option>
        <option value="High">Alta</option>
        <option value="Normal">Normale</option>
        <option value="Low">Bassa</option>
      </select>
    </div>

    <!-- Cards layout -->
    <div v-if="loading" class="loading-state"><div class="spinner"></div> Caricamento…</div>
    <div v-else-if="!filtered.length" class="empty-state">
      <div class="empty-icon">◉</div><div>Nessuna comunicazione trovata</div>
    </div>
    <div v-else class="comms-list">
      <div v-for="c in filtered" :key="c.id" class="comm-card" :class="{ 'comm-urgent': c.priority === 'High' }">
        <div class="comm-left">
          <div class="comm-priority-bar" :class="priorityBar(c.priority)"></div>
        </div>
        <div class="comm-body">
          <div class="comm-header">
            <span class="comm-title">{{ c.title }}</span>
            <div class="comm-badges">
              <span class="badge" :class="typeBadge(c.communicationType)">{{ c.communicationType }}</span>
              <span class="badge" :class="priorityBadge(c.priority)">{{ c.priority }}</span>
              <span class="badge badge-muted" v-if="!c.isVisible">Nascosta</span>
            </div>
          </div>
          <div class="comm-preview">{{ preview(c.content) }}</div>
          <div class="comm-meta">
            <span class="text-muted mono" style="font-size:0.75rem">{{ fmtDate(c.publicationDate) }}</span>
            <span v-if="c.expirationDate" class="text-muted" style="font-size:0.75rem">
              Scade: {{ fmtDate(c.expirationDate) }}
            </span>
            <span v-if="c.sendEmail" class="badge badge-blue" style="font-size:0.7rem">📧 Email inviata</span>
          </div>
        </div>
        <div class="comm-actions">
          <button v-if="canEdit && !c.isVisible" class="btn btn-sm btn-ghost" @click="publishComm(c.id)">Pubblica</button>
          <button v-if="canEdit" class="btn-icon" @click="openModal(c)">✎</button>
          <button v-if="canDelete" class="btn-icon" @click="deleteItem(c.id)" style="color:var(--accent-red)">✕</button>
        </div>
      </div>
    </div>

    <!-- Modal -->
    <div class="modal-overlay" v-if="showModal" @click.self="showModal=false">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ editing ? 'Modifica' : 'Nuova' }} comunicazione</h2>
          <button class="btn-icon" @click="showModal=false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-group">
            <label class="form-label">Titolo *</label>
            <input class="form-input" v-model="form.title" />
          </div>
          <div class="form-grid">
            <div class="form-group">
              <label class="form-label">Tipo</label>
              <select class="form-select" v-model="form.communicationType">
                <option value="Notice">Avviso</option>
                <option value="Meeting">Assemblea</option>
                <option value="Maintenance">Manutenzione</option>
                <option value="Emergency">Urgente</option>
                <option value="Info">Informazione</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Priorità</label>
              <select class="form-select" v-model="form.priority">
                <option value="High">Alta</option>
                <option value="Normal">Normale</option>
                <option value="Low">Bassa</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Data pubblicazione</label>
              <input class="form-input" type="datetime-local" v-model="form.publicationDate" />
            </div>
            <div class="form-group">
              <label class="form-label">Data scadenza</label>
              <input class="form-input" type="datetime-local" v-model="form.expirationDate" />
            </div>
          </div>
          <div class="form-group">
            <label class="form-label">Contenuto *</label>
            <textarea class="form-textarea" v-model="form.content" rows="5"></textarea>
          </div>
          <div style="display:flex;gap:1.5rem">
            <label style="display:flex;align-items:center;gap:0.4rem;font-size:0.875rem;cursor:pointer">
              <input type="checkbox" v-model="form.isVisible" />
              Visibile ai condòmini
            </label>
            <label style="display:flex;align-items:center;gap:0.4rem;font-size:0.875rem;cursor:pointer">
              <input type="checkbox" v-model="form.sendEmail" />
              Invia email notifica
            </label>
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
import { ref, computed, onMounted, watch } from 'vue'
import { useAppStore } from '@/stores/app'
import { communicationApi } from '@/services/api'
import { usePermissions } from '@/composables/usePermissions'

const store = useAppStore()
const { canCreate, canEdit, canDelete } = usePermissions()
const communications = ref([])
const loading = ref(false)
const saving = ref(false)
const showModal = ref(false)
const editing = ref(null)
const search = ref('')
const filterType = ref('')
const filterPriority = ref('')

const defaultForm = () => ({
  title: '', content: '', communicationType: 'Notice', priority: 'Normal',
  publicationDate: new Date().toISOString().slice(0,16),
  expirationDate: '', sendEmail: false, isVisible: true, attachmentPath: ''
})
const form = ref(defaultForm())

const fmtDate = (d) => d ? new Date(d).toLocaleDateString('it-IT', { day:'2-digit', month:'short', year:'numeric' }) : '—'
const preview = (t) => t?.length > 120 ? t.slice(0, 120) + '…' : t

const typeBadge = (t) => ({ Notice: 'badge-blue', Meeting: 'badge-purple', Maintenance: 'badge-amber', Emergency: 'badge-red', Info: 'badge-muted' }[t] || 'badge-muted')
const priorityBadge = (p) => ({ High: 'badge-red', Normal: 'badge-blue', Low: 'badge-muted' }[p] || 'badge-muted')
const priorityBar = (p) => ({ High: 'bar-red', Normal: 'bar-blue', Low: 'bar-muted' }[p] || 'bar-muted')

const filtered = computed(() => {
  let list = communications.value
  if (filterType.value) list = list.filter(c => c.communicationType === filterType.value)
  if (filterPriority.value) list = list.filter(c => c.priority === filterPriority.value)
  if (search.value) {
    const q = search.value.toLowerCase()
    list = list.filter(c => c.title?.toLowerCase().includes(q))
  }
  return list
})

async function loadData() {
  if (!store.selectedCondominioId) return
  loading.value = true
  try {
    const { data } = await communicationApi.getByCondominium(store.selectedCondominioId)
    communications.value = data
  } catch { communications.value = [] } finally { loading.value = false }
}

function openModal(c = null) {
  editing.value = c?.id ?? null
  form.value = c ? { ...c, publicationDate: c.publicationDate?.slice(0,16), expirationDate: c.expirationDate?.slice(0,16) } : defaultForm()
  showModal.value = true
}

async function save() {
  if (!form.value.title || !form.value.content) return store.toast('Titolo e contenuto sono obbligatori', 'error')
  saving.value = true
  try {
    if (editing.value) await communicationApi.update(editing.value, form.value)
    else await communicationApi.create({ ...form.value, condominiumId: store.selectedCondominioId })
    store.toast('Comunicazione salvata', 'success')
    showModal.value = false
    loadData()
  } catch { store.toast('Errore', 'error') } finally { saving.value = false }
}

async function publishComm(id) {
  try { await communicationApi.publish(id); store.toast('Comunicazione pubblicata', 'success'); loadData() }
  catch { store.toast('Errore', 'error') }
}

async function deleteItem(id) {
  if (!confirm('Eliminare questa comunicazione?')) return
  try { await communicationApi.delete(id); store.toast('Comunicazione eliminata', 'success'); loadData() }
  catch { store.toast('Errore', 'error') }
}

watch(() => store.selectedCondominioId, loadData)
onMounted(loadData)
</script>

<style scoped>
.toolbar { display: flex; gap: 0.75rem; margin-bottom: 1rem; flex-wrap: wrap; align-items: center; }
.search-input { flex: 1; min-width: 200px; max-width: 320px; }

.comms-list { display: flex; flex-direction: column; gap: 0.5rem; }
.comm-card {
  display: flex; align-items: flex-start; gap: 0;
  background: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  overflow: hidden;
  transition: border-color 0.15s;
}
.comm-card:hover { border-color: rgba(255,255,255,0.12); }
.comm-urgent { border-color: rgba(252,129,129,0.2); }

.comm-left { flex-shrink: 0; width: 4px; }
.comm-priority-bar { width: 100%; height: 100%; min-height: 60px; }
.bar-red   { background: var(--accent-red); }
.bar-blue  { background: var(--accent); }
.bar-muted { background: var(--text-muted); }

.comm-body { flex: 1; min-width: 0; padding: 0.9rem 1rem; display: flex; flex-direction: column; gap: 0.4rem; }
.comm-header { display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap; }
.comm-title { font-weight: 500; font-size: 0.95rem; }
.comm-badges { display: flex; gap: 0.35rem; flex-wrap: wrap; }
.comm-preview { font-size: 0.83rem; color: var(--text-secondary); line-height: 1.5; }
.comm-meta { display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap; }

.comm-actions { display: flex; align-items: center; gap: 0.4rem; padding: 0.9rem 0.75rem; flex-shrink: 0; }
</style>
