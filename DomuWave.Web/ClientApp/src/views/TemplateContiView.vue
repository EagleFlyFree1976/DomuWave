<template>
  <div class="view-container">

    <!-- Header -->
    <div class="view-header">
      <div>
        <h1 class="view-title">Template Piano dei Conti</h1>
        <p class="view-subtitle">Gestione dei template riutilizzabili per il piano dei conti</p>
      </div>
      <div class="header-actions">
        <router-link to="/piano-dei-conti" class="btn btn-ghost">
          <i class="pi pi-arrow-left"></i> Piano dei Conti
        </router-link>
        <button class="btn btn-primary" @click="openTemplateModal()">
          <i class="pi pi-plus"></i> Nuovo Template
        </button>
      </div>
    </div>

    <!-- Loading / empty -->
    <div v-if="loading" class="state-box"><i class="pi pi-spin pi-spinner"></i> Caricamento…</div>
    <div v-else-if="!templates.length" class="state-box muted">
      <i class="pi pi-th-large"></i> Nessun template. Creane uno con il pulsante in alto.
    </div>

    <!-- Two-pane layout -->
    <div v-else class="pane-layout">

      <!-- Left: template list -->
      <div class="template-list">
        <div
          v-for="t in templates"
          :key="t.id"
          class="template-card"
          :class="{ 'template-card--selected': selectedTemplate?.id === t.id, 'template-card--inactive': !t.isActive }"
          @click="selectTemplate(t)"
        >
          <div class="template-card__header">
            <span class="template-card__name">{{ t.name }}</span>
            <div class="template-card__badges">
              <span v-if="t.isDefault" class="badge badge-default">Predefinito</span>
              <span class="badge" :class="t.isActive ? 'badge-active' : 'badge-inactive'">
                {{ t.isActive ? 'Attivo' : 'Inattivo' }}
              </span>
            </div>
          </div>
          <div class="template-card__meta">
            <span>{{ t.itemCount }} voci</span>
            <span v-if="t.description" class="template-card__desc">{{ t.description }}</span>
          </div>
          <div class="template-card__actions" @click.stop>
            <button
              v-if="!t.isDefault"
              class="icon-btn"
              title="Imposta come predefinito"
              @click="doSetDefault(t)"
            >
              <i class="pi pi-star"></i>
            </button>
            <button class="icon-btn" title="Modifica template" @click="openTemplateModal(t)">
              <i class="pi pi-pencil"></i>
            </button>
            <button class="icon-btn danger" title="Elimina template" @click="confirmDeleteTemplate(t)">
              <i class="pi pi-trash"></i>
            </button>
          </div>
        </div>
      </div>

      <!-- Right: items for selected template -->
      <div class="items-pane" v-if="selectedTemplate">
        <div class="items-pane__header">
          <h3>Voci — {{ selectedTemplate.name }}</h3>
          <button class="btn btn-primary btn-sm" @click="openItemModal()">
            <i class="pi pi-plus"></i> Nuova Voce
          </button>
        </div>

        <div v-if="loadingItems" class="state-box"><i class="pi pi-spin pi-spinner"></i> Caricamento…</div>
        <div v-else-if="!items.length" class="state-box muted">
          <i class="pi pi-list"></i> Nessuna voce in questo template.
        </div>
        <div v-else class="table-wrapper">
          <table class="data-table">
            <thead>
              <tr>
                <th>Ord.</th>
                <th>Codice</th>
                <th>Nome</th>
                <th>Tipo</th>
                <th>Padre</th>
                <th>Stato</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in sortedItems" :key="item.id" :class="{ 'row-inactive': !item.isActive }">
                <td class="text-center text-muted">{{ item.sortOrder }}</td>
                <td class="mono">{{ item.code }}</td>
                <td :style="{ paddingLeft: itemDepth(item) + 'rem' }">{{ item.name }}</td>
                <td><span class="badge badge-type">{{ item.typeLabel }}</span></td>
                <td class="mono text-muted">{{ parentCode(item.parentItemId) }}</td>
                <td>
                  <span class="badge" :class="item.isActive ? 'badge-active' : 'badge-inactive'">
                    {{ item.isActive ? 'Attiva' : 'Inattiva' }}
                  </span>
                </td>
                <td class="actions">
                  <button class="icon-btn" title="Modifica" @click="openItemModal(item)">
                    <i class="pi pi-pencil"></i>
                  </button>
                  <button class="icon-btn danger" title="Elimina" @click="confirmDeleteItem(item)">
                    <i class="pi pi-trash"></i>
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div v-else class="items-pane items-pane--empty">
        <i class="pi pi-arrow-left"></i> Seleziona un template per gestirne le voci.
      </div>
    </div>

    <!-- ── Template create/edit modal ────────────────────────── -->
    <Teleport to="body">
      <div v-if="showTemplateModal" class="modal-overlay" @click.self="showTemplateModal = false">
        <div class="modal modal-sm">
          <div class="modal-header">
            <h2>{{ editingTemplate ? 'Modifica Template' : 'Nuovo Template' }}</h2>
            <button class="icon-btn" @click="showTemplateModal = false"><i class="pi pi-times"></i></button>
          </div>
          <div class="modal-body">
            <div class="form-stack">
              <div class="form-group" :class="{ 'has-error': errors.name }">
                <label class="form-label">Nome *</label>
                <input class="form-input" v-model="tForm.name" @input="clearError('name')" />
                <span class="field-error" v-if="errors.name">{{ errors.name }}</span>
              </div>
              <div class="form-group">
                <label class="form-label">Descrizione</label>
                <textarea class="form-input" v-model="tForm.description" rows="2" />
              </div>
              <div class="form-group">
                <label class="form-label checkbox-label">
                  <input type="checkbox" v-model="tForm.isActive" />
                  Template attivo
                </label>
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn btn-ghost" @click="showTemplateModal = false">Annulla</button>
            <button class="btn btn-primary" @click="saveTemplate" :disabled="savingTemplate">
              <i class="pi pi-spin pi-spinner" v-if="savingTemplate"></i>
              {{ savingTemplate ? 'Salvataggio…' : 'Salva' }}
            </button>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- ── Item create/edit modal ─────────────────────────────── -->
    <Teleport to="body">
      <div v-if="showItemModal" class="modal-overlay" @click.self="showItemModal = false">
        <div class="modal modal-md">
          <div class="modal-header">
            <h2>{{ editingItem ? 'Modifica Voce' : 'Nuova Voce' }}</h2>
            <button class="icon-btn" @click="showItemModal = false"><i class="pi pi-times"></i></button>
          </div>
          <div class="modal-body">
            <div class="form-grid">

              <div class="form-group" :class="{ 'has-error': errors.code }">
                <label class="form-label">Codice *</label>
                <input class="form-input" v-model="iForm.code" @input="clearError('code')" />
                <span class="field-error" v-if="errors.code">{{ errors.code }}</span>
              </div>

              <div class="form-group" :class="{ 'has-error': errors.name }">
                <label class="form-label">Nome *</label>
                <input class="form-input" v-model="iForm.name" @input="clearError('name')" />
                <span class="field-error" v-if="errors.name">{{ errors.name }}</span>
              </div>

              <div class="form-group" style="grid-column: 1 / -1">
                <label class="form-label">Tipo *</label>
                <div class="radio-group">
                  <label v-for="t in typeOptions" :key="t.value" class="radio-label">
                    <input type="radio" v-model.number="iForm.type" :value="t.value" />
                    {{ t.label }}
                  </label>
                </div>
              </div>

              <div class="form-group">
                <label class="form-label">Voce padre</label>
                <select class="form-select" v-model.number="iForm.parentItemId">
                  <option :value="null">— Nessuna (radice) —</option>
                  <option v-for="i in parentCandidates" :key="i.id" :value="i.id">
                    {{ i.code }} – {{ i.name }}
                  </option>
                </select>
              </div>

              <div class="form-group">
                <label class="form-label">Ordinamento</label>
                <input class="form-input" type="number" min="0" v-model.number="iForm.sortOrder" />
              </div>

              <div class="form-group" style="grid-column: 1 / -1">
                <label class="form-label">Descrizione</label>
                <textarea class="form-input" v-model="iForm.description" rows="2" />
              </div>

              <div class="form-group" style="grid-column: 1 / -1">
                <label class="form-label checkbox-label">
                  <input type="checkbox" v-model="iForm.isActive" />
                  Voce attiva
                </label>
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn btn-ghost" @click="showItemModal = false">Annulla</button>
            <button class="btn btn-primary" @click="saveItem" :disabled="savingItem">
              <i class="pi pi-spin pi-spinner" v-if="savingItem"></i>
              {{ savingItem ? 'Salvataggio…' : 'Salva' }}
            </button>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- Delete template confirm -->
    <Teleport to="body">
      <div v-if="deleteTemplateTarget" class="modal-overlay" @click.self="deleteTemplateTarget = null">
        <div class="modal modal-sm">
          <div class="modal-header">
            <h2>Elimina Template</h2>
            <button class="icon-btn" @click="deleteTemplateTarget = null"><i class="pi pi-times"></i></button>
          </div>
          <div class="modal-body">
            <p>Eliminare il template <strong>{{ deleteTemplateTarget.name }}</strong> e tutte le sue voci?</p>
          </div>
          <div class="modal-footer">
            <button class="btn btn-ghost" @click="deleteTemplateTarget = null">Annulla</button>
            <button class="btn btn-danger" @click="doDeleteTemplate" :disabled="deletingTemplate">
              <i class="pi pi-spin pi-spinner" v-if="deletingTemplate"></i>
              {{ deletingTemplate ? 'Eliminazione…' : 'Elimina' }}
            </button>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- Delete item confirm -->
    <Teleport to="body">
      <div v-if="deleteItemTarget" class="modal-overlay" @click.self="deleteItemTarget = null">
        <div class="modal modal-sm">
          <div class="modal-header">
            <h2>Elimina Voce</h2>
            <button class="icon-btn" @click="deleteItemTarget = null"><i class="pi pi-times"></i></button>
          </div>
          <div class="modal-body">
            <p>Eliminare la voce <strong>{{ deleteItemTarget.code }} – {{ deleteItemTarget.name }}</strong>?</p>
          </div>
          <div class="modal-footer">
            <button class="btn btn-ghost" @click="deleteItemTarget = null">Annulla</button>
            <button class="btn btn-danger" @click="doDeleteItem" :disabled="deletingItem">
              <i class="pi pi-spin pi-spinner" v-if="deletingItem"></i>
              {{ deletingItem ? 'Eliminazione…' : 'Elimina' }}
            </button>
          </div>
        </div>
      </div>
    </Teleport>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { chartOfAccountsTemplateApi } from '@/services/api'

// ── State ──────────────────────────────────────────────────────
const loading          = ref(false)
const templates        = ref([])
const selectedTemplate = ref(null)

const loadingItems  = ref(false)
const items         = ref([])

const errors = ref({})

// ── Template CRUD ───────────────────────────────────────────────
const showTemplateModal  = ref(false)
const editingTemplate    = ref(null)
const savingTemplate     = ref(false)
const deleteTemplateTarget = ref(null)
const deletingTemplate   = ref(false)

const tForm = ref({ name: '', description: '', isActive: true })

const typeOptions = [
  { value: 1, label: 'Entrata' },
  { value: 2, label: 'Uscita' },
  { value: 3, label: 'Patrimoniale' },
]

// ── Item CRUD ───────────────────────────────────────────────────
const showItemModal  = ref(false)
const editingItem    = ref(null)
const savingItem     = ref(false)
const deleteItemTarget = ref(null)
const deletingItem   = ref(false)

const emptyItemForm = () => ({
  templateId: 0, code: '', name: '', description: '',
  type: 2, parentItemId: null, sortOrder: 0, isActive: true,
})
const iForm = ref(emptyItemForm())

// ── Computed ────────────────────────────────────────────────────
const sortedItems = computed(() =>
  items.value.slice().sort((a, b) => a.sortOrder - b.sortOrder || a.code.localeCompare(b.code))
)

const parentCandidates = computed(() =>
  items.value.filter(i => !editingItem.value || i.id !== editingItem.value)
)

function parentCode(parentId) {
  if (!parentId) return '—'
  return items.value.find(i => i.id === parentId)?.code ?? '—'
}

function itemDepth(item) {
  // count ancestors for indentation
  let depth = 0
  let cur = item
  while (cur.parentItemId) {
    depth++
    cur = items.value.find(i => i.id === cur.parentItemId) ?? {}
    if (!cur.parentItemId) break
  }
  return 0.75 + depth * 1.2
}

// ── Load ────────────────────────────────────────────────────────
async function load() {
  loading.value = true
  try {
    const { data } = await chartOfAccountsTemplateApi.getAll()
    templates.value = data ?? []
    if (selectedTemplate.value) {
      selectedTemplate.value = templates.value.find(t => t.id === selectedTemplate.value.id) ?? null
    }
  } catch { templates.value = [] } finally { loading.value = false }
}

async function selectTemplate(t) {
  selectedTemplate.value = t
  loadingItems.value     = true
  items.value            = []
  try {
    const { data } = await chartOfAccountsTemplateApi.getItems(t.id)
    items.value = data ?? []
  } catch { items.value = [] } finally { loadingItems.value = false }
}

onMounted(load)

// ── Template modal ───────────────────────────────────────────────
function clearError(f) { delete errors.value[f] }

function openTemplateModal(t = null) {
  errors.value = {}
  if (t) {
    editingTemplate.value = t.id
    tForm.value = { name: t.name, description: t.description ?? '', isActive: t.isActive }
  } else {
    editingTemplate.value = null
    tForm.value = { name: '', description: '', isActive: true }
  }
  showTemplateModal.value = true
}

async function saveTemplate() {
  errors.value = {}
  if (!tForm.value.name?.trim()) { errors.value.name = 'Nome obbligatorio'; return }
  savingTemplate.value = true
  try {
    if (editingTemplate.value) {
      await chartOfAccountsTemplateApi.update(editingTemplate.value, tForm.value)
    } else {
      await chartOfAccountsTemplateApi.create(tForm.value)
    }
    showTemplateModal.value = false
    await load()
  } catch (err) {
    if (!err?.response) throw err
  } finally { savingTemplate.value = false }
}

async function doSetDefault(t) {
  try {
    await chartOfAccountsTemplateApi.setDefault(t.id)
    await load()
  } catch (err) {
    if (!err?.response) throw err
  }
}

function confirmDeleteTemplate(t) { deleteTemplateTarget.value = t }

async function doDeleteTemplate() {
  deletingTemplate.value = true
  try {
    await chartOfAccountsTemplateApi.delete(deleteTemplateTarget.value.id)
    if (selectedTemplate.value?.id === deleteTemplateTarget.value.id) {
      selectedTemplate.value = null
      items.value = []
    }
    deleteTemplateTarget.value = null
    await load()
  } catch (err) {
    if (!err?.response) throw err
  } finally { deletingTemplate.value = false }
}

// ── Item modal ───────────────────────────────────────────────────
function openItemModal(item = null) {
  errors.value = {}
  if (item) {
    editingItem.value = item.id
    iForm.value = {
      templateId:   item.templateId,
      code:         item.code,
      name:         item.name,
      description:  item.description ?? '',
      type:         item.type,
      parentItemId: item.parentItemId ?? null,
      sortOrder:    item.sortOrder,
      isActive:     item.isActive,
    }
  } else {
    editingItem.value = null
    iForm.value = { ...emptyItemForm(), templateId: selectedTemplate.value.id }
  }
  showItemModal.value = true
}

async function saveItem() {
  errors.value = {}
  if (!iForm.value.code?.trim()) { errors.value.code = 'Codice obbligatorio'; return }
  if (!iForm.value.name?.trim()) { errors.value.name = 'Nome obbligatorio'; return }
  savingItem.value = true
  try {
    if (editingItem.value) {
      await chartOfAccountsTemplateApi.updateItem(selectedTemplate.value.id, editingItem.value, iForm.value)
    } else {
      await chartOfAccountsTemplateApi.createItem(selectedTemplate.value.id, iForm.value)
    }
    showItemModal.value = false
    await selectTemplate(selectedTemplate.value)
    await load() // refresh itemCount
  } catch (err) {
    if (!err?.response) throw err
  } finally { savingItem.value = false }
}

function confirmDeleteItem(item) { deleteItemTarget.value = item }

async function doDeleteItem() {
  deletingItem.value = true
  try {
    await chartOfAccountsTemplateApi.deleteItem(selectedTemplate.value.id, deleteItemTarget.value.id)
    deleteItemTarget.value = null
    await selectTemplate(selectedTemplate.value)
    await load()
  } catch (err) {
    if (!err?.response) throw err
  } finally { deletingItem.value = false }
}
</script>

<style scoped>
.view-container { padding: 1.5rem 2rem; max-width: 1200px; }

.view-header {
  display: flex; align-items: flex-start;
  justify-content: space-between; gap: 1rem; margin-bottom: 1.25rem;
}
.view-title   { font-size: 1.4rem; font-weight: 700; color: var(--text-primary); }
.view-subtitle { font-size: .85rem; color: var(--text-muted); margin-top: .2rem; }
.header-actions { display: flex; gap: .5rem; align-items: center; }

.pane-layout { display: grid; grid-template-columns: 280px 1fr; gap: 1.25rem; align-items: start; }

/* ── Template list ── */
.template-list { display: flex; flex-direction: column; gap: .5rem; }

.template-card {
  background: var(--bg-card); border: 1px solid var(--border);
  border-radius: 8px; padding: .75rem 1rem; cursor: pointer;
  transition: border-color .12s, box-shadow .12s;
  position: relative;
}
.template-card:hover { border-color: #34d399; }
.template-card--selected { border-color: #34d399; box-shadow: 0 0 0 2px rgba(52,211,153,.2); }
.template-card--inactive { opacity: .6; }

.template-card__header {
  display: flex; align-items: center; justify-content: space-between; gap: .5rem;
  margin-bottom: .3rem;
}
.template-card__name   { font-weight: 600; font-size: .9rem; color: var(--text-primary); }
.template-card__badges { display: flex; gap: .3rem; align-items: center; flex-wrap: wrap; }
.template-card__meta { display: flex; gap: .5rem; flex-wrap: wrap; font-size: .78rem; color: var(--text-muted); }
.template-card__desc { font-style: italic; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 160px; }
.template-card__actions {
  display: flex; gap: .3rem; margin-top: .5rem; justify-content: flex-end;
}

/* ── Items pane ── */
.items-pane {
  background: var(--bg-card); border: 1px solid var(--border);
  border-radius: 8px; overflow: hidden;
}
.items-pane--empty {
  display: flex; align-items: center; gap: .6rem;
  padding: 2.5rem; font-size: .9rem; color: var(--text-muted);
  justify-content: center;
}
.items-pane__header {
  display: flex; align-items: center; justify-content: space-between;
  padding: .75rem 1rem; border-bottom: 1px solid var(--border);
}
.items-pane__header h3 { font-size: .95rem; font-weight: 600; color: var(--text-primary); }

/* ── Table ── */
.table-wrapper { overflow-x: auto; }
.data-table { width: 100%; border-collapse: collapse; font-size: .875rem; }
.data-table th {
  background: var(--bg-surface); color: var(--text-muted);
  font-size: .75rem; font-weight: 600; text-transform: uppercase;
  letter-spacing: .5px; padding: .6rem .75rem; text-align: left;
  border-bottom: 1px solid var(--border);
}
.data-table td { padding: .6rem .75rem; border-bottom: 1px solid var(--border); }
.data-table tbody tr:last-child td { border-bottom: none; }
.data-table tbody tr:hover { background: var(--bg-surface); }
.row-inactive { opacity: .55; }

.mono { font-family: monospace; font-size: .83rem; }
.text-center { text-align: center; }
.text-muted { color: var(--text-muted); }
.actions { display: flex; gap: .4rem; justify-content: flex-end; }

/* ── State box ── */
.state-box {
  display: flex; align-items: center; gap: .6rem;
  padding: 2rem; justify-content: center;
  color: var(--text-muted); font-size: .9rem;
}

/* ── Badges ── */
.badge { display: inline-block; padding: .2rem .55rem; border-radius: 4px; font-size: .72rem; font-weight: 600; }
.badge-active   { background: rgba(52,211,153,.12); color: #34d399; }
.badge-inactive { background: rgba(100,116,139,.12); color: #64748b; }
.badge-default  { background: rgba(251,191,36,.15);  color: #f59e0b; }
.badge-type     { background: rgba(99,102,241,.12);  color: #818cf8; }

/* ── Icon buttons ── */
.icon-btn {
  background: transparent; border: none; cursor: pointer;
  color: var(--text-muted); width: 28px; height: 28px;
  border-radius: 6px; display: flex; align-items: center; justify-content: center;
  transition: background .12s, color .12s;
}
.icon-btn:hover { background: var(--bg-surface); color: var(--text-primary); }
.icon-btn.danger:hover { color: #f87171; background: rgba(248,113,113,.08); }

/* ── Buttons ── */
.btn {
  display: inline-flex; align-items: center; gap: .4rem;
  padding: .5rem 1rem; border-radius: 7px; font-size: .875rem;
  font-weight: 500; cursor: pointer; border: none; transition: background .12s;
  text-decoration: none;
}
.btn-sm { padding: .35rem .75rem; font-size: .8rem; }
.btn-primary { background: #34d399; color: #0f172a; }
.btn-primary:hover:not(:disabled) { background: #2ac488; }
.btn-ghost { background: transparent; color: var(--text-muted); border: 1px solid var(--border); }
.btn-ghost:hover { background: var(--bg-surface); }
.btn-danger { background: #f87171; color: #fff; }
.btn-danger:hover:not(:disabled) { background: #ef4444; }
.btn:disabled { opacity: .55; cursor: not-allowed; }

/* ── Modal ── */
.modal-overlay {
  position: fixed; inset: 0; background: rgba(0,0,0,.55);
  display: flex; align-items: center; justify-content: center; z-index: 1000;
}
.modal { background: var(--bg-card); border-radius: 12px; width: 100%; box-shadow: 0 20px 60px rgba(0,0,0,.4); }
.modal-md { max-width: 540px; }
.modal-sm { max-width: 400px; }
.modal-header {
  display: flex; align-items: center; justify-content: space-between;
  padding: 1rem 1.25rem; border-bottom: 1px solid var(--border);
}
.modal-header h2 { font-size: 1rem; font-weight: 600; color: var(--text-primary); }
.modal-body    { padding: 1.25rem; }
.modal-footer  {
  display: flex; justify-content: flex-end; gap: .75rem;
  padding: 1rem 1.25rem; border-top: 1px solid var(--border);
}

/* ── Form ── */
.form-stack { display: flex; flex-direction: column; gap: .9rem; }
.form-grid  { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
.form-group { display: flex; flex-direction: column; gap: .35rem; }
.form-label { font-size: .8rem; font-weight: 500; color: var(--text-muted); }
.form-input, .form-select {
  background: var(--bg-surface); border: 1px solid var(--border);
  color: var(--text-primary); border-radius: 6px; padding: .5rem .75rem;
  font-size: .875rem; font-family: inherit; width: 100%; box-sizing: border-box;
}
.form-input:focus, .form-select:focus { outline: none; border-color: #34d399; }
textarea.form-input { resize: vertical; min-height: 60px; }
.has-error .form-input { border-color: var(--accent-red, #f87171); }
.field-error { font-size: .75rem; color: var(--accent-red, #f87171); }
.checkbox-label { display: flex; align-items: center; gap: .5rem; cursor: pointer; font-size: .875rem; color: var(--text-primary); }

.radio-group { display: flex; gap: 1.25rem; flex-wrap: wrap; padding: .5rem 0; }
.radio-label {
  display: flex; align-items: center; gap: .45rem;
  font-size: .875rem; color: var(--text-primary); cursor: pointer;
}
.radio-label input[type="radio"] { accent-color: #34d399; width: 15px; height: 15px; cursor: pointer; }
</style>
