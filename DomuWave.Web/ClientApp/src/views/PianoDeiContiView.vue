<template>
  <div class="view-container">

    <!-- Header -->
    <div class="view-header">
      <div>
        <h1 class="view-title">Piano dei Conti</h1>
        <p class="view-subtitle" v-if="condominioName">{{ condominioName }}</p>
      </div>
      <button class="btn btn-primary" @click="openModal()" :disabled="!store.selectedCondominioId">
        <i class="pi pi-plus"></i> Nuovo Conto
      </button>
    </div>

    <!-- Filtri -->
    <div class="filter-bar" v-if="accounts.length">
      <input class="form-input filter-input" v-model="search" placeholder="Cerca per codice o nome…" />
      <select class="form-select filter-select" v-model.number="filterType">
        <option value="">Tutti i tipi</option>
        <option v-for="t in typeOptions" :key="t.value" :value="t.value">{{ t.label }}</option>
      </select>
    </div>

    <!-- Loading / empty -->
    <div v-if="loading" class="state-box"><i class="pi pi-spin pi-spinner"></i> Caricamento…</div>
    <div v-else-if="!store.selectedCondominioId" class="state-box muted">
      <i class="pi pi-building"></i> Seleziona un condominio per visualizzare il piano dei conti.
    </div>
    <div v-else-if="!filtered.length" class="state-box muted">
      <i class="pi pi-list"></i> Nessun conto trovato.
    </div>

    <!-- Tabella -->
    <div v-else class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th>Codice</th>
            <th>Nome</th>
            <th>Tipo</th>
            <th>Categoria</th>
            <th>Livello</th>
            <th>Conto padre</th>
            <th>Stato</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="a in filtered" :key="a.id" :class="{ 'row-inactive': !a.isActive }">
            <td class="mono">{{ a.code }}</td>
            <td :style="{ paddingLeft: `${(a.level - 1) * 1.2 + 0.75}rem` }">{{ a.name }}</td>
            <td><span class="badge badge-type">{{ typeLabel(a.type) }}</span></td>
            <td class="text-secondary">{{ a.category ?? '—' }}</td>
            <td class="text-center">{{ a.level }}</td>
            <td class="text-secondary mono">{{ parentCode(a.parentAccountId) }}</td>
            <td>
              <span class="badge" :class="a.isActive ? 'badge-open' : 'badge-muted'">
                {{ a.isActive ? 'Attivo' : 'Inattivo' }}
              </span>
            </td>
            <td class="actions">
              <button class="icon-btn" title="Modifica" @click="openModal(a)">
                <i class="pi pi-pencil"></i>
              </button>
              <button class="icon-btn danger" title="Elimina" @click="confirmDelete(a)">
                <i class="pi pi-trash"></i>
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal Create/Edit -->
    <Teleport to="body">
      <div v-if="showModal" class="modal-overlay" @click.self="showModal = false">
        <div class="modal modal-md">
          <div class="modal-header">
            <h2>{{ editing ? 'Modifica Conto' : 'Nuovo Conto' }}</h2>
            <button class="icon-btn" @click="showModal = false"><i class="pi pi-times"></i></button>
          </div>
          <div class="modal-body">
            <div class="form-grid">

              <div class="form-group" :class="{ 'has-error': errors.code }">
                <label class="form-label">Codice *</label>
                <input class="form-input" v-model="form.code" @input="clearError('code')" />
                <span class="field-error" v-if="errors.code">{{ errors.code }}</span>
              </div>

              <div class="form-group" :class="{ 'has-error': errors.name }">
                <label class="form-label">Nome *</label>
                <input class="form-input" v-model="form.name" @input="clearError('name')" />
                <span class="field-error" v-if="errors.name">{{ errors.name }}</span>
              </div>

              <div class="form-group" style="grid-column: 1 / -1" :class="{ 'has-error': errors.type }">
                <label class="form-label">Tipo *</label>
                <div class="radio-group">
                  <label v-for="t in typeOptions" :key="t.value" class="radio-label">
                    <input type="radio" v-model.number="form.type" :value="t.value" @change="clearError('type')" />
                    {{ t.label }}
                  </label>
                </div>
                <span class="field-error" v-if="errors.type">{{ errors.type }}</span>
              </div>

              <div class="form-group">
                <label class="form-label">Categoria</label>
                <input class="form-input" v-model="form.category" placeholder="es. Ordinaria, Straordinaria…" />
              </div>

              <div class="form-group" v-if="!editing">
                <label class="form-label">Conto padre</label>
                <select class="form-select" v-model.number="form.parentAccountId">
                  <option :value="null">— Nessuno (radice) —</option>
                  <option v-for="a in accounts" :key="a.id" :value="a.id">
                    {{ a.code }} – {{ a.name }}
                  </option>
                </select>
              </div>

              <div class="form-group" style="grid-column: 1 / -1">
                <label class="form-label">Descrizione</label>
                <textarea class="form-input" v-model="form.description" rows="2" />
              </div>

              <div class="form-group" style="grid-column: 1 / -1">
                <label class="form-label checkbox-label">
                  <input type="checkbox" v-model="form.isActive" />
                  Conto attivo
                </label>
              </div>

            </div>
          </div>
          <div class="modal-footer">
            <button class="btn btn-ghost" @click="showModal = false">Annulla</button>
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
      <div v-if="deleteTarget" class="modal-overlay" @click.self="deleteTarget = null">
        <div class="modal modal-sm">
          <div class="modal-header">
            <h2>Elimina conto</h2>
            <button class="icon-btn" @click="deleteTarget = null"><i class="pi pi-times"></i></button>
          </div>
          <div class="modal-body">
            <p>Eliminare il conto <strong>{{ deleteTarget.code }} – {{ deleteTarget.name }}</strong>?</p>
            <p class="text-secondary" style="margin-top:.5rem;font-size:.85rem">
              L'operazione non è consentita se il conto ha sotto-conti o voci di budget associate.
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
import { ref, computed, watch, onMounted } from 'vue'
import { useAppStore } from '@/stores/app'
import { chartOfAccountsApi } from '@/services/api'

const store = useAppStore()

const accounts    = ref([])
const loading     = ref(false)
const search      = ref('')
const filterType  = ref('')
const showModal   = ref(false)
const editing     = ref(null)   // account id when editing, null when creating
const saving      = ref(false)
const deleteTarget = ref(null)
const deleting    = ref(false)
const errors      = ref({})

const typeOptions = [
  { value: 1, label: 'Entrata' },
  { value: 2, label: 'Uscita' },
  { value: 3, label: 'Patrimoniale' },
]
const typeLabel = (v) => typeOptions.find(o => o.value === v)?.label ?? v

const emptyForm = () => ({
  code: '', name: '', type: 2, category: '', description: '',
  parentAccountId: null, isActive: true,
})
const form = ref(emptyForm())

const condominioName = computed(() =>
  store.condomini?.find(c => c.id === store.selectedCondominioId)?.name ?? '')

const filtered = computed(() => {
  let list = accounts.value
  if (filterType.value !== '') list = list.filter(a => a.type === filterType.value)
  if (search.value) {
    const q = search.value.toLowerCase()
    list = list.filter(a =>
      a.code.toLowerCase().includes(q) || a.name.toLowerCase().includes(q))
  }
  return list.slice().sort((a, b) => a.code.localeCompare(b.code))
})

function parentCode(parentId) {
  if (!parentId) return '—'
  return accounts.value.find(a => a.id === parentId)?.code ?? '—'
}

async function load() {
  if (!store.selectedCondominioId) { accounts.value = []; return }
  loading.value = true
  try {
    const res = await chartOfAccountsApi.getByCondominium(store.selectedCondominioId)
    accounts.value = res.data ?? []
  } catch { accounts.value = [] } finally { loading.value = false }
}

watch(() => store.selectedCondominioId, load)
onMounted(load)

function clearError(f) { delete errors.value[f] }

function validate() {
  errors.value = {}
  if (!form.value.code?.trim()) errors.value.code = 'Codice obbligatorio'
  if (!form.value.name?.trim()) errors.value.name = 'Nome obbligatorio'
  return Object.keys(errors.value).length === 0
}

function openModal(a = null) {
  errors.value = {}
  if (a) {
    editing.value = a.id
    form.value = {
      code: a.code, name: a.name, type: a.type,
      category: a.category ?? '', description: a.description ?? '',
      parentAccountId: a.parentAccountId ?? null,
      isActive: a.isActive,
    }
  } else {
    editing.value = null
    form.value = emptyForm()
  }
  showModal.value = true
}

async function save() {
  if (!validate()) return
  saving.value = true
  try {
    if (editing.value) {
      await chartOfAccountsApi.update(editing.value, {
        code: form.value.code, name: form.value.name,
        type: form.value.type, category: form.value.category || null,
        description: form.value.description || null, isActive: form.value.isActive,
      })
    } else {
      await chartOfAccountsApi.create({
        condominiumId:   store.selectedCondominioId,
        parentAccountId: form.value.parentAccountId,
        code:            form.value.code,
        name:            form.value.name,
        type:            form.value.type,
        category:        form.value.category || null,
        description:     form.value.description || null,
        isActive:        form.value.isActive,
      })
    }
    showModal.value = false
    await load()
  } catch (err) {
    if (!err?.response) throw err
  } finally { saving.value = false }
}

function confirmDelete(a) { deleteTarget.value = a }

async function doDelete() {
  deleting.value = true
  try {
    await chartOfAccountsApi.delete(deleteTarget.value.id)
    deleteTarget.value = null
    await load()
  } catch (err) {
    if (!err?.response) throw err
  } finally { deleting.value = false }
}
</script>

<style scoped>
.view-container { padding: 1.5rem 2rem; max-width: 1100px; }

.view-header {
  display: flex; align-items: flex-start;
  justify-content: space-between; gap: 1rem;
  margin-bottom: 1.25rem;
}
.view-title  { font-size: 1.4rem; font-weight: 700; color: var(--text-primary); }
.view-subtitle { font-size: .85rem; color: var(--text-muted); margin-top: .2rem; }

.filter-bar {
  display: flex; gap: .75rem; margin-bottom: 1rem;
}
.filter-input  { max-width: 280px; }
.filter-select { max-width: 180px; }

.state-box {
  display: flex; align-items: center; gap: .6rem;
  padding: 2rem; justify-content: center;
  color: var(--text-muted); font-size: .9rem;
}
.state-box .pi { font-size: 1.1rem; }

.table-wrapper { overflow-x: auto; border-radius: 8px; border: 1px solid var(--border); }
.data-table { width: 100%; border-collapse: collapse; font-size: .875rem; }
.data-table th {
  background: var(--bg-surface); color: var(--text-muted);
  font-size: .75rem; font-weight: 600; text-transform: uppercase;
  letter-spacing: .5px; padding: .6rem .75rem; text-align: left;
  border-bottom: 1px solid var(--border);
}
.data-table td { padding: .65rem .75rem; border-bottom: 1px solid var(--border); }
.data-table tbody tr:last-child td { border-bottom: none; }
.data-table tbody tr:hover { background: var(--bg-surface); }
.row-inactive { opacity: .55; }

.mono { font-family: monospace; font-size: .83rem; }
.text-center { text-align: center; }
.text-secondary { color: var(--text-muted); }

.actions { display: flex; gap: .4rem; justify-content: flex-end; }

.badge { display: inline-block; padding: .2rem .55rem; border-radius: 4px; font-size: .72rem; font-weight: 600; }
.badge-type  { background: rgba(99,102,241,.12); color: #818cf8; }
.badge-open  { background: rgba(52,211,153,.12); color: #34d399; }
.badge-muted { background: rgba(100,116,139,.12); color: #64748b; }

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
.modal { background: var(--bg-card); border-radius: 12px; width: 100%; box-shadow: 0 20px 60px rgba(0,0,0,.4); }
.modal-md { max-width: 560px; }
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

.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
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

.radio-group {
  display: flex; gap: 1.25rem; flex-wrap: wrap;
  padding: .5rem 0;
}
.radio-label {
  display: flex; align-items: center; gap: .45rem;
  font-size: .875rem; color: var(--text-primary); cursor: pointer;
}
.radio-label input[type="radio"] { accent-color: #34d399; width: 15px; height: 15px; cursor: pointer; }
</style>
