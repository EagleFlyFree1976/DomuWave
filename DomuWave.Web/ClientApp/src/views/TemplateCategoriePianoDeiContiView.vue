<template>
  <div class="view-container">

    <!-- Header -->
    <div class="view-header">
      <div>
        <h1 class="view-title">Template Categorie Piano dei Conti</h1>
        <p class="view-subtitle">
          Queste categorie verranno copiate automaticamente per ogni nuovo tenant al momento della sua creazione.
        </p>
      </div>
      <button class="btn btn-primary" @click="openModal()">
        <i class="pi pi-plus"></i> Nuovo Template
      </button>
    </div>

    <!-- Info banner -->
    <div class="info-banner">
      <i class="pi pi-info-circle"></i>
      Solo i template <strong>attivi</strong> vengono copiati al nuovo tenant. I template disattivati sono mantenuti per storico.
    </div>

    <!-- Loading / empty -->
    <div v-if="loading" class="state-box">
      <i class="pi pi-spin pi-spinner"></i> Caricamento…
    </div>
    <div v-else-if="!items.length" class="state-box muted">
      <i class="pi pi-tags"></i> Nessun template definito. Aggiungine uno con il pulsante in alto.
    </div>

    <!-- Tabella -->
    <div v-else class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th>Nome</th>
            <th>Descrizione</th>
            <th>Stato</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in items" :key="item.id" :class="{ 'row-inactive': !item.isActive }">
            <td class="font-medium">{{ item.name }}</td>
            <td class="text-secondary">{{ item.description ?? '—' }}</td>
            <td>
              <span class="badge" :class="item.isActive ? 'badge-active' : 'badge-inactive'">
                {{ item.isActive ? 'Attivo' : 'Inattivo' }}
              </span>
            </td>
            <td class="actions">
              <button class="icon-btn" title="Modifica" @click="openModal(item)">
                <i class="pi pi-pencil"></i>
              </button>
              <button class="icon-btn danger" title="Elimina" @click="confirmDelete(item)">
                <i class="pi pi-trash"></i>
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal Create/Edit -->
    <Teleport to="body">
      <div v-if="showModal" class="modal-overlay" @mousedown.self="closeModal">
        <div class="modal modal-sm">
          <div class="modal-header">
            <h2>{{ editing ? 'Modifica Template' : 'Nuovo Template' }}</h2>
            <button class="icon-btn" @click="closeModal"><i class="pi pi-times"></i></button>
          </div>
          <div class="modal-body">
            <div class="form-stack">

              <div class="form-group" :class="{ 'has-error': errors.name }">
                <label class="form-label">Nome *</label>
                <input class="form-input" v-model="form.name" @input="clearError('name')"
                       placeholder="es. Ordinaria, Straordinaria, Manutenzione…" />
                <span class="field-error" v-if="errors.name">{{ errors.name }}</span>
              </div>

              <div class="form-group">
                <label class="form-label">Descrizione</label>
                <textarea class="form-input" v-model="form.description" rows="2"
                          placeholder="Descrizione opzionale…" />
              </div>

              <div class="form-group" v-if="editing">
                <label class="form-label checkbox-label">
                  <input type="checkbox" v-model="form.isActive" />
                  Template attivo (verrà usato per i nuovi tenant)
                </label>
              </div>

            </div>
          </div>
          <div class="modal-footer">
            <button class="btn btn-ghost" @click="closeModal">Annulla</button>
            <button class="btn btn-primary" @click="save" :disabled="saving">
              <i class="pi pi-spin pi-spinner" v-if="saving"></i>
              {{ saving ? 'Salvataggio…' : 'Salva' }}
            </button>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- Confirm delete -->
    <Teleport to="body">
      <div v-if="deleteTarget" class="modal-overlay" @mousedown.self="deleteTarget = null">
        <div class="modal modal-sm">
          <div class="modal-header">
            <h2>Elimina template</h2>
            <button class="icon-btn" @click="deleteTarget = null"><i class="pi pi-times"></i></button>
          </div>
          <div class="modal-body">
            <p>Eliminare il template <strong>{{ deleteTarget.name }}</strong>?</p>
            <p class="text-secondary hint">
              L'eliminazione non influisce sulle categorie già create per i tenant esistenti.
            </p>
          </div>
          <div class="modal-footer">
            <button class="btn btn-ghost" @click="deleteTarget = null">Annulla</button>
            <button class="btn btn-danger" @click="doDelete" :disabled="deleting">
              <i class="pi pi-spin pi-spinner" v-if="deleting"></i>
              {{ deleting ? 'Eliminazione…' : 'Elimina' }}
            </button>
          </div>
        </div>
      </div>
    </Teleport>

  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { chartOfAccountsCategoryTemplateApi } from '@/services/api'

const items        = ref([])
const loading      = ref(false)
const showModal    = ref(false)
const editing      = ref(null)
const saving       = ref(false)
const deleteTarget = ref(null)
const deleting     = ref(false)
const errors       = ref({})

const emptyForm = () => ({ name: '', description: '', isActive: true })
const form = ref(emptyForm())

async function load() {
  loading.value = true
  try {
    const res = await chartOfAccountsCategoryTemplateApi.getAll()
    items.value = (res.data ?? []).slice().sort((a, b) => a.name.localeCompare(b.name))
  } catch { items.value = [] } finally { loading.value = false }
}

onMounted(load)

function clearError(f) { delete errors.value[f] }

function validate() {
  errors.value = {}
  if (!form.value.name?.trim()) errors.value.name = 'Nome obbligatorio'
  return Object.keys(errors.value).length === 0
}

function openModal(item = null) {
  errors.value = {}
  if (item) {
    editing.value = item.id
    form.value = { name: item.name, description: item.description ?? '', isActive: item.isActive }
  } else {
    editing.value = null
    form.value = emptyForm()
  }
  showModal.value = true
}

function closeModal() {
  showModal.value = false
  errors.value = {}
}

async function save() {
  if (!validate()) return
  saving.value = true
  try {
    if (editing.value) {
      await chartOfAccountsCategoryTemplateApi.update(editing.value, {
        name:        form.value.name,
        description: form.value.description || null,
        isActive:    form.value.isActive,
      })
    } else {
      await chartOfAccountsCategoryTemplateApi.create({
        name:        form.value.name,
        description: form.value.description || null,
        isActive:    true,
      })
    }
    closeModal()
    await load()
  } catch (err) {
    if (!err?.response) throw err
  } finally { saving.value = false }
}

function confirmDelete(item) { deleteTarget.value = item }

async function doDelete() {
  deleting.value = true
  try {
    await chartOfAccountsCategoryTemplateApi.delete(deleteTarget.value.id)
    deleteTarget.value = null
    await load()
  } catch (err) {
    if (!err?.response) throw err
  } finally { deleting.value = false }
}
</script>

<style scoped>
.view-container { padding: 1.5rem 2rem; max-width: 860px; }

.view-header {
  display: flex; align-items: flex-start;
  justify-content: space-between; gap: 1rem;
  margin-bottom: 1rem;
}
.view-title    { font-size: 1.4rem; font-weight: 700; color: var(--text-primary); }
.view-subtitle { font-size: .85rem; color: var(--text-muted); margin-top: .2rem; max-width: 540px; }

.info-banner {
  display: flex; align-items: center; gap: .6rem;
  background: rgba(99,102,241,.08); border: 1px solid rgba(99,102,241,.2);
  border-radius: 8px; padding: .65rem 1rem;
  font-size: .82rem; color: #a5b4fc;
  margin-bottom: 1.25rem;
}
.info-banner .pi { font-size: 1rem; flex-shrink: 0; }

.state-box {
  display: flex; align-items: center; gap: .6rem;
  padding: 3rem; justify-content: center;
  color: var(--text-muted); font-size: .9rem;
}
.state-box .pi { font-size: 1.2rem; }

.table-wrapper { overflow-x: auto; border-radius: 8px; border: 1px solid var(--border); }
.data-table { width: 100%; border-collapse: collapse; font-size: .875rem; }
.data-table th {
  background: var(--bg-surface); color: var(--text-muted);
  font-size: .75rem; font-weight: 600; text-transform: uppercase;
  letter-spacing: .5px; padding: .65rem .85rem; text-align: left;
  border-bottom: 1px solid var(--border);
}
.data-table td { padding: .75rem .85rem; border-bottom: 1px solid var(--border); vertical-align: middle; }
.data-table tbody tr:last-child td { border-bottom: none; }
.data-table tbody tr:hover { background: var(--bg-surface); }
.row-inactive { opacity: .5; }

.font-medium  { font-weight: 500; }
.text-secondary { color: var(--text-muted); }

.actions { display: flex; gap: .35rem; justify-content: flex-end; }

.badge {
  display: inline-block; padding: .2rem .6rem;
  border-radius: 4px; font-size: .72rem; font-weight: 600;
}
.badge-active   { background: rgba(52,211,153,.12); color: #34d399; }
.badge-inactive { background: rgba(100,116,139,.12); color: #64748b; }

.icon-btn {
  background: transparent; border: none; cursor: pointer;
  color: var(--text-muted); width: 30px; height: 30px;
  border-radius: 6px; display: flex; align-items: center; justify-content: center;
  transition: background .12s, color .12s;
}
.icon-btn:hover { background: var(--bg-surface); color: var(--text-primary); }
.icon-btn.danger:hover { color: #f87171; background: rgba(248,113,113,.08); }

/* Modal */
.modal-overlay {
  position: fixed; inset: 0; background: rgba(0,0,0,.55);
  display: flex; align-items: center; justify-content: center; z-index: 1000;
}
.modal {
  background: var(--bg-card); border-radius: 12px; width: 100%;
  box-shadow: 0 20px 60px rgba(0,0,0,.4);
}
.modal-sm { max-width: 420px; }
.modal-header {
  display: flex; align-items: center; justify-content: space-between;
  padding: 1rem 1.25rem; border-bottom: 1px solid var(--border);
}
.modal-header h2 { font-size: 1rem; font-weight: 600; color: var(--text-primary); }
.modal-body   { padding: 1.25rem; }
.modal-footer {
  display: flex; justify-content: flex-end; gap: .75rem;
  padding: 1rem 1.25rem; border-top: 1px solid var(--border);
}

.form-stack { display: flex; flex-direction: column; gap: .9rem; }
.form-group { display: flex; flex-direction: column; gap: .35rem; }
.form-label { font-size: .8rem; font-weight: 500; color: var(--text-muted); }
.form-input {
  background: var(--bg-surface); border: 1px solid var(--border);
  color: var(--text-primary); border-radius: 6px; padding: .5rem .75rem;
  font-size: .875rem; font-family: inherit; width: 100%; box-sizing: border-box;
}
.form-input:focus { outline: none; border-color: #34d399; }
textarea.form-input { resize: vertical; min-height: 60px; }
.has-error .form-input { border-color: var(--accent-red, #f87171); }
.field-error { font-size: .75rem; color: var(--accent-red, #f87171); }
.checkbox-label { display: flex; align-items: center; gap: .5rem; cursor: pointer; font-size: .875rem; color: var(--text-primary); }

.hint { margin-top: .5rem; font-size: .82rem; }

.btn {
  display: inline-flex; align-items: center; gap: .4rem;
  padding: .5rem 1rem; border-radius: 7px; font-size: .875rem;
  font-weight: 500; cursor: pointer; border: none; transition: background .12s;
}
.btn-primary { background: #34d399; color: #0f172a; }
.btn-primary:hover:not(:disabled) { background: #2ac488; }
.btn-ghost { background: transparent; color: var(--text-muted); border: 1px solid var(--border); }
.btn-ghost:hover { background: var(--bg-surface); }
.btn-danger { background: #f87171; color: #fff; }
.btn-danger:hover:not(:disabled) { background: #ef4444; }
.btn:disabled { opacity: .55; cursor: not-allowed; }
</style>
