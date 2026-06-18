<template>
  <div>
    <div class="toolbar">
      <h1>Attività</h1>
      <div class="toolbar-spacer"></div>
      <select class="form-select" v-model="assigneeFilter" style="min-width:180px">
        <option :value="null">Tutti gli assegnatari</option>
        <option v-for="u in assignees" :key="u.id" :value="u.id">{{ fullName(u) }}</option>
      </select>
      <select class="form-select" v-model.number="statusFilter" style="min-width:150px">
        <option :value="null">Tutti gli stati</option>
        <option v-for="s in STATUSES" :key="s.id" :value="s.id">{{ s.name }}</option>
      </select>
      <button class="btn btn-primary" @click="openModal()">+ Nuova attività</button>
    </div>

    <div v-if="loading" class="card loading-state"><div class="spinner"></div></div>

    <div v-else-if="!items.length" class="card empty-state">
      Nessuna attività. Crea la prima con <strong>+ Nuova attività</strong>.
    </div>

    <div v-else class="card table-wrap">
      <table>
        <thead>
          <tr>
            <th>Titolo</th>
            <th>Assegnatario</th>
            <th>Scadenza</th>
            <th>Priorità</th>
            <th>Stato</th>
            <th>Condomìni</th>
            <th class="text-right">Azioni</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="t in items" :key="t.id">
            <td>
              <div>{{ t.title }}</div>
              <div v-if="t.description" class="text-muted" style="font-size:0.8rem">{{ t.description }}</div>
            </td>
            <td>{{ t.assignedToFullName || '—' }}</td>
            <td :class="isOverdue(t) ? 'text-red' : ''" class="mono">{{ fmtDate(t.dueDate) }}</td>
            <td><span class="badge" :class="priorityBadge(t.priorityId)">{{ t.priorityName }}</span></td>
            <td><span class="badge" :class="statusBadge(t.statusId)">{{ t.statusName }}</span></td>
            <td class="text-muted" style="font-size:0.82rem">
              {{ t.condominiums?.length ? t.condominiums.map(c => c.condominiumName).join(', ') : '—' }}
            </td>
            <td class="text-right row-actions">
              <button v-if="t.statusId !== 3" class="btn btn-sm btn-ghost" title="Completa" @click="completeItem(t.id)">✓</button>
              <button class="btn btn-sm btn-ghost" title="Modifica" @click="openModal(t)">✎</button>
              <button class="btn btn-sm btn-ghost" title="Elimina" @click="deleteItem(t.id)">✕</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- ── Modale crea/modifica ── -->
    <div class="modal-overlay" v-if="showModal" @click.self="showModal = false">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ editing ? 'Modifica' : 'Nuova' }} attività</h2>
          <button class="btn-icon" @click="showModal = false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-group" :class="{ 'has-error': errors.title }">
            <label class="form-label">Titolo *</label>
            <input class="form-input" v-model="form.title" @input="delete errors.title" />
            <span v-if="errors.title" class="field-error">{{ errors.title }}</span>
          </div>

          <div class="form-group">
            <label class="form-label">Descrizione</label>
            <textarea class="form-textarea" rows="2" v-model="form.description"></textarea>
          </div>

          <div class="form-grid">
            <div class="form-group">
              <label class="form-label">Priorità</label>
              <select class="form-select" v-model.number="form.priorityId">
                <option v-for="p in PRIORITIES" :key="p.id" :value="p.id">{{ p.name }}</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Stato</label>
              <select class="form-select" v-model.number="form.statusId">
                <option v-for="s in STATUSES" :key="s.id" :value="s.id">{{ s.name }}</option>
              </select>
            </div>
          </div>

          <div class="form-grid">
            <div class="form-group">
              <label class="form-label">Scadenza</label>
              <input class="form-input" type="date" v-model="form.dueDate" />
            </div>
            <div class="form-group">
              <label class="form-label">Assegnatario</label>
              <select class="form-select" v-model.number="form.assignedToUserId">
                <option :value="null">— Nessuno —</option>
                <option v-for="u in assignees" :key="u.id" :value="u.id">{{ fullName(u) }}</option>
              </select>
            </div>
          </div>

          <div class="form-group">
            <label class="form-label">Condomìni collegati (facoltativo)</label>
            <div class="cond-checklist">
              <label v-for="c in store.condomini" :key="c.id" class="cond-item">
                <input type="checkbox" :value="c.id" v-model="form.condominiumIds" />
                <span>{{ c.name }}</span>
              </label>
              <p v-if="!store.condomini.length" class="text-muted" style="font-size:0.82rem;margin:0">Nessun condominio disponibile.</p>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showModal = false">Annulla</button>
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
import { useAuthStore } from '@/stores/authStore'
import { taskApi } from '@/services/api'
import { userApi } from '@/services/userService'

const store = useAppStore()
const authStore = useAuthStore()

const PRIORITIES = [
  { id: 1, name: 'Bassa' }, { id: 2, name: 'Media' }, { id: 3, name: 'Alta' }, { id: 4, name: 'Urgente' },
]
const STATUSES = [
  { id: 1, name: 'Da fare' }, { id: 2, name: 'In corso' }, { id: 3, name: 'Completata' }, { id: 4, name: 'Annullata' },
]

const items         = ref([])
const collaborators = ref([])
const loading       = ref(false)
const saving        = ref(false)
const showModal     = ref(false)
const editing       = ref(null)
const errors        = ref({})
const assigneeFilter = ref(null)
const statusFilter   = ref(null)

const fmtDate = (d) => d ? new Date(d).toLocaleDateString('it-IT') : '—'
const fullName = (u) => u.label || (u.fullName || `${u.name ?? ''} ${u.lastName ?? ''}`).trim() || u.email

// Assegnatari = me stesso (in cima) + collaboratori, senza duplicati.
const assignees = computed(() => {
  const me = authStore.currentUser
  const list = [...collaborators.value]
  if (me?.id != null && !list.some(u => u.id === me.id)) {
    list.unshift({ id: me.id, label: `${me.displayName || me.username || 'Io'} (io)` })
  }
  return list
})
const isOverdue = (t) => t.dueDate && t.statusId !== 3 && t.statusId !== 4 && new Date(t.dueDate) < new Date(new Date().toDateString())

const priorityBadge = (id) => ({ 1: 'badge-muted', 2: 'badge-blue', 3: 'badge-amber', 4: 'badge-red' }[id] || 'badge-muted')
const statusBadge   = (id) => ({ 1: 'badge-blue', 2: 'badge-amber', 3: 'badge-green', 4: 'badge-muted' }[id] || 'badge-muted')

const defaultForm = () => ({
  title: '', description: '', priorityId: 2, statusId: 1,
  dueDate: '', assignedToUserId: null, condominiumIds: [],
})
const form = ref(defaultForm())

async function loadData() {
  loading.value = true
  try {
    const params = {}
    if (assigneeFilter.value) params.assignedToUserId = assigneeFilter.value
    if (statusFilter.value)   params.status = statusFilter.value
    const { data } = await taskApi.getAll(params)
    items.value = data ?? []
  } catch (err) {
    items.value = []
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { loading.value = false }
}

async function loadCollaborators() {
  try {
    const { data } = await userApi.search({ roles: 'CLB', isActive: true })
    collaborators.value = data ?? []
  } catch { collaborators.value = [] }
}

function openModal(t = null) {
  editing.value = t?.id ?? null
  errors.value  = {}
  form.value = t ? {
    title:            t.title,
    description:      t.description || '',
    priorityId:       t.priorityId || 2,
    statusId:         t.statusId || 1,
    dueDate:          t.dueDate ? t.dueDate.substring(0, 10) : '',
    assignedToUserId: t.assignedToUserId ?? null,
    condominiumIds:   (t.condominiums || []).map(c => c.condominiumId),
  } : defaultForm()
  showModal.value = true
}

async function save() {
  errors.value = {}
  if (!form.value.title?.trim()) { errors.value.title = 'Il titolo è obbligatorio'; return }
  saving.value = true
  try {
    const payload = { ...form.value, dueDate: form.value.dueDate || null }
    if (editing.value) await taskApi.update(editing.value, payload)
    else               await taskApi.create(payload)
    store.toast('Attività salvata', 'success')
    showModal.value = false
    loadData()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { saving.value = false }
}

async function completeItem(id) {
  try {
    await taskApi.complete(id)
    store.toast('Attività completata', 'success')
    loadData()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

async function deleteItem(id) {
  if (!confirm('Eliminare questa attività?')) return
  try {
    await taskApi.delete(id)
    store.toast('Attività eliminata', 'success')
    loadData()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

watch([assigneeFilter, statusFilter], loadData)

onMounted(async () => {
  if (!store.condomini.length) await store.loadCondomini().catch(() => {})
  loadCollaborators()
  loadData()
})
</script>

<style scoped>
.toolbar { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1rem; flex-wrap: wrap; }
.toolbar h1 { margin: 0; }
.toolbar-spacer { flex: 1; }

.row-actions { display: flex; gap: 0.25rem; justify-content: flex-end; }

.cond-checklist {
  display: flex; flex-direction: column; gap: 6px;
  max-height: 180px; overflow-y: auto;
  border: 1px solid var(--border); border-radius: 8px; padding: 10px 12px;
}
.cond-item { display: flex; align-items: center; gap: 8px; font-size: 0.875rem; cursor: pointer; }
.cond-item input { accent-color: var(--accent); }

.text-red { color: var(--accent-red, #ef4444); }
.mono { font-family: monospace; }
</style>
