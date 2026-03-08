<template>
  <div class="view-container">

    <!-- Header -->
    <div class="view-header">
      <div>
        <h1 class="view-title">Piano dei Conti</h1>
        <p class="view-subtitle" v-if="condominioName">{{ condominioName }}</p>
      </div>
      <div class="header-actions">
        <router-link to="/categorie-piano-dei-conti" class="btn btn-ghost">
          <i class="pi pi-tags"></i> Gestisci Categorie
        </router-link>
        <button class="btn btn-ghost" @click="openCopy" :disabled="!store.selectedCondominioId">
          <i class="pi pi-copy"></i> Copia da…
        </button>
        <button class="btn btn-primary" @click="openModal()" :disabled="!store.selectedCondominioId">
          <i class="pi pi-plus"></i> Nuovo Conto
        </button>
      </div>
    </div>

    <!-- Filtri -->
    <div class="filter-bar" v-if="accounts.length">
      <input class="form-input filter-input" v-model="search" placeholder="Cerca per codice o nome…" />
      <select class="form-select filter-select" v-model.number="filterType">
        <option value="">Tutti i tipi</option>
        <option v-for="t in typeOptions" :key="t.value" :value="t.value">{{ t.label }}</option>
      </select>
      <select class="form-select filter-select" v-model.number="filterCategory">
        <option value="">Tutte le categorie</option>
        <option v-for="c in categories" :key="c.id" :value="c.id">{{ c.name }}</option>
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
            <th>Tab. millesimale</th>
            <th>Ripartizione</th>
            <th>Stato</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="a in filtered" :key="a.id" :class="{ 'row-inactive': !a.isActive }">
            <td class="mono">{{ a.code }}</td>
            <td :style="{ paddingLeft: `${(a.level - 1) * 1.2 + 0.75}rem` }">{{ a.name }}</td>
            <td><span class="badge badge-type">{{ typeLabel(a.type) }}</span></td>
            <td class="text-secondary">{{ a.categoryName ?? '—' }}</td>
            <td class="text-center">{{ a.level }}</td>
            <td class="text-secondary mono">{{ parentCode(a.parentAccountId) }}</td>
            <td class="text-secondary">{{ a.defaultMillesimalTableName ?? '—' }}</td>
            <td>
              <span v-if="a.allocationMethod === 1" class="badge badge-mixed" :title="`${a.millesimalPercentage}% millesimale + ${100 - a.millesimalPercentage}% piano/abitanti`">
                Misto {{ a.millesimalPercentage }}%
              </span>
              <span v-else class="badge badge-standard">Standard</span>
            </td>
            <td>
              <span class="badge" :class="a.isActive ? 'badge-open' : 'badge-muted'">
                {{ a.isActive ? 'Attivo' : 'Inattivo' }}
              </span>
            </td>
            <td class="actions">
              <button class="icon-btn" title="Modifica" @click="openModal(a)">
                <i class="pi pi-pencil"></i>
              </button>
              <button class="icon-btn" title="Clona" @click="cloneAccount(a)">
                <i class="pi pi-copy"></i>
              </button>
              <button class="icon-btn danger" title="Elimina" @click="confirmDelete(a)">
                <i class="pi pi-trash"></i>
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal Create/Edit Conto -->
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
                <select class="form-select" v-model.number="form.categoryId">
                  <option :value="null">— Nessuna —</option>
                  <option v-for="c in activeCategories" :key="c.id" :value="c.id">{{ c.name }}</option>
                </select>
              </div>

              <div class="form-group">
                <label class="form-label">Tabella millesimale default</label>
                <select class="form-select" v-model.number="form.defaultMillesimalTableId">
                  <option :value="null">— Nessuna —</option>
                  <option v-for="t in millesimalTables" :key="t.id" :value="t.id">{{ t.name }}</option>
                </select>
              </div>

              <div class="form-group" style="grid-column: 1 / -1">
                <label class="form-label">Metodo di calcolo ripartizione</label>
                <div class="radio-group">
                  <label class="radio-label">
                    <input type="radio" v-model.number="form.allocationMethod" :value="0" />
                    Standard (100% millesimale)
                  </label>
                  <label class="radio-label">
                    <input type="radio" v-model.number="form.allocationMethod" :value="1" />
                    Misto (millesimale + piano/abitanti)
                  </label>
                </div>
              </div>

              <template v-if="form.allocationMethod === 1">
                <div class="form-group" style="grid-column: 1 / -1">
                  <div class="mixed-hint">
                    <i class="pi pi-info-circle"></i>
                    L'importo viene diviso in due parti: Y% ripartito via tabella millesimale,
                    il restante (100-Y)% via fattore = <strong>PesoFloor × Piano + PesoAbitanti × N.Abitanti</strong>.
                  </div>
                </div>
                <div class="form-group" :class="{ 'has-error': errors.millesimalPercentage }">
                  <label class="form-label">% Millesimale (Y) *</label>
                  <input class="form-input" type="number" min="0" max="100" step="0.01"
                    v-model.number="form.millesimalPercentage" @input="clearError('millesimalPercentage')" />
                  <span class="field-error" v-if="errors.millesimalPercentage">{{ errors.millesimalPercentage }}</span>
                </div>
                <div></div>
                <div class="form-group" :class="{ 'has-error': errors.floorWeight }">
                  <label class="form-label">Peso Piano (FloorWeight) *</label>
                  <input class="form-input" type="number" min="0" step="0.0001"
                    v-model.number="form.floorWeight" @input="clearError('floorWeight')" />
                  <span class="field-error" v-if="errors.floorWeight">{{ errors.floorWeight }}</span>
                </div>
                <div class="form-group" :class="{ 'has-error': errors.inhabitantsWeight }">
                  <label class="form-label">Peso Abitanti (InhabitantsWeight) *</label>
                  <input class="form-input" type="number" min="0" step="0.0001"
                    v-model.number="form.inhabitantsWeight" @input="clearError('inhabitantsWeight')" />
                  <span class="field-error" v-if="errors.inhabitantsWeight">{{ errors.inhabitantsWeight }}</span>
                </div>
              </template>

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

    <!-- Copy from existing condominium -->
    <Teleport to="body">
      <div v-if="showCopy" class="modal-overlay" @click.self="showCopy = false">
        <div class="modal modal-sm">
          <div class="modal-header">
            <h2>Copia Piano dei Conti</h2>
            <button class="icon-btn" @click="showCopy = false"><i class="pi pi-times"></i></button>
          </div>
          <div class="modal-body">
            <div class="form-stack">
              <div class="form-group" :class="{ 'has-error': copyError }">
                <label class="form-label">Condominio sorgente *</label>
                <select class="form-select" v-model.number="copySourceId">
                  <option :value="null">— Seleziona —</option>
                  <option
                    v-for="c in otherCondomini"
                    :key="c.id"
                    :value="c.id"
                  >{{ c.name }}</option>
                </select>
                <span class="field-error" v-if="copyError">{{ copyError }}</span>
              </div>
              <p class="copy-hint">
                <i class="pi pi-info-circle"></i>
                Verranno copiati tutti i conti del condominio selezionato. I conti con codice già presente nel condominio corrente verranno ignorati. Le categorie non vengono copiate.
              </p>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn btn-ghost" @click="showCopy = false">Annulla</button>
            <button class="btn btn-primary" @click="doCopy" :disabled="copying || !copySourceId">
              <i class="pi pi-spin pi-spinner" v-if="copying"></i>
              {{ copying ? 'Copia in corso…' : 'Copia' }}
            </button>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- Confirm delete conto -->
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
import { chartOfAccountsApi, chartOfAccountsCategoryApi, millesimalTableApi } from '@/services/api'

const otherCondomini = computed(() =>
  (store.condomini ?? []).filter(c => c.id !== store.selectedCondominioId)
)

const store = useAppStore()

// ── Conti ──────────────────────────────────────────────────────
const accounts       = ref([])
const loading        = ref(false)
const search         = ref('')
const filterType     = ref('')
const filterCategory = ref('')
const showModal      = ref(false)
const editing        = ref(null)
const saving         = ref(false)
const deleteTarget   = ref(null)
const deleting       = ref(false)
const errors         = ref({})

// ── Categorie (solo per il dropdown) ──────────────────────────
const categories = ref([])

const activeCategories = computed(() => categories.value.filter(c => c.isActive))

// ── Tabelle millesimali (per il dropdown) ──────────────────────
const millesimalTables = ref([])

const typeOptions = [
  { value: 1, label: 'Entrata' },
  { value: 2, label: 'Uscita' },
  { value: 3, label: 'Patrimoniale' },
]
const typeLabel = (v) => typeOptions.find(o => o.value === v)?.label ?? v

const emptyForm = () => ({
  code: '', name: '', type: 2, categoryId: null, description: '',
  parentAccountId: null, isActive: true, defaultMillesimalTableId: null,
  allocationMethod: 0, millesimalPercentage: null, floorWeight: null, inhabitantsWeight: null,
})
const form = ref(emptyForm())

const condominioName = computed(() =>
  store.condomini?.find(c => c.id === store.selectedCondominioId)?.name ?? '')

const filtered = computed(() => {
  let list = accounts.value
  if (filterType.value !== '') list = list.filter(a => a.type === filterType.value)
  if (filterCategory.value !== '') list = list.filter(a => a.categoryId === filterCategory.value)
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

async function loadCategories() {
  try {
    const res = await chartOfAccountsCategoryApi.getAll()
    categories.value = res.data ?? []
  } catch { categories.value = [] }
}

async function loadMillesimalTables() {
  if (!store.selectedCondominioId) { millesimalTables.value = []; return }
  try {
    const res = await millesimalTableApi.getByCondominium(store.selectedCondominioId)
    millesimalTables.value = (res.data ?? []).filter(t => t.isActive)
  } catch { millesimalTables.value = [] }
}

watch(() => store.selectedCondominioId, () => { load(); loadMillesimalTables() })
onMounted(() => { load(); loadCategories(); loadMillesimalTables() })

// ── Conti CRUD ─────────────────────────────────────────────────
function clearError(f) { delete errors.value[f] }

function validate() {
  errors.value = {}
  if (!form.value.code?.trim()) errors.value.code = 'Codice obbligatorio'
  if (!form.value.name?.trim()) errors.value.name = 'Nome obbligatorio'
  if (form.value.allocationMethod === 1) {
    const pct = form.value.millesimalPercentage
    if (pct === null || pct === '' || pct === undefined)
      errors.value.millesimalPercentage = 'Percentuale obbligatoria'
    else if (pct < 0 || pct > 100)
      errors.value.millesimalPercentage = 'Deve essere tra 0 e 100'
    if (form.value.floorWeight === null || form.value.floorWeight === '')
      errors.value.floorWeight = 'Peso piano obbligatorio'
    if (form.value.inhabitantsWeight === null || form.value.inhabitantsWeight === '')
      errors.value.inhabitantsWeight = 'Peso abitanti obbligatorio'
  }
  return Object.keys(errors.value).length === 0
}

function openModal(a = null) {
  errors.value = {}
  if (a) {
    editing.value = a.id
    form.value = {
      code: a.code, name: a.name, type: a.type,
      categoryId: a.categoryId ?? null,
      description: a.description ?? '',
      parentAccountId: a.parentAccountId ?? null,
      isActive: a.isActive,
      defaultMillesimalTableId: a.defaultMillesimalTableId ?? null,
      allocationMethod: a.allocationMethod ?? 0,
      millesimalPercentage: a.millesimalPercentage ?? null,
      floorWeight: a.floorWeight ?? null,
      inhabitantsWeight: a.inhabitantsWeight ?? null,
    }
  } else {
    editing.value = null
    form.value = emptyForm()
  }
  showModal.value = true
}

function cloneAccount(a) {
  errors.value  = {}
  editing.value = null   // nuovo conto, non modifica
  form.value = {
    code:                     '',
    name:                     a.name,
    type:                     a.type,
    categoryId:               a.categoryId ?? null,
    description:              a.description ?? '',
    parentAccountId:          a.parentAccountId ?? null,
    isActive:                 a.isActive,
    allocationMethod:         a.allocationMethod ?? 0,
    millesimalPercentage:     a.millesimalPercentage ?? null,
    floorWeight:              a.floorWeight ?? null,
    inhabitantsWeight:        a.inhabitantsWeight ?? null,
  }
  showModal.value = true
}

async function save() {
  if (!validate()) return
  saving.value = true
  try {
    if (editing.value) {
      await chartOfAccountsApi.update(editing.value, {
        code:                     form.value.code,
        name:                     form.value.name,
        type:                     form.value.type,
        categoryId:               form.value.categoryId,
        description:              form.value.description || null,
        isActive:                 form.value.isActive,
        defaultMillesimalTableId: form.value.defaultMillesimalTableId,
        allocationMethod:         form.value.allocationMethod,
        millesimalPercentage:     form.value.allocationMethod === 1 ? form.value.millesimalPercentage : null,
        floorWeight:              form.value.allocationMethod === 1 ? form.value.floorWeight : null,
        inhabitantsWeight:        form.value.allocationMethod === 1 ? form.value.inhabitantsWeight : null,
      })
    } else {
      await chartOfAccountsApi.create({
        condominiumId:            store.selectedCondominioId,
        parentAccountId:          form.value.parentAccountId,
        code:                     form.value.code,
        name:                     form.value.name,
        type:                     form.value.type,
        categoryId:               form.value.categoryId,
        description:              form.value.description || null,
        isActive:                 form.value.isActive,
        defaultMillesimalTableId: form.value.defaultMillesimalTableId,
        allocationMethod:         form.value.allocationMethod,
        millesimalPercentage:     form.value.allocationMethod === 1 ? form.value.millesimalPercentage : null,
        floorWeight:              form.value.allocationMethod === 1 ? form.value.floorWeight : null,
        inhabitantsWeight:        form.value.allocationMethod === 1 ? form.value.inhabitantsWeight : null,
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

// ── Copia da condominio esistente ───────────────────────────────
const showCopy     = ref(false)
const copySourceId = ref(null)
const copying      = ref(false)
const copyError    = ref('')

function openCopy() {
  copySourceId.value = null
  copyError.value    = ''
  showCopy.value     = true
}

async function doCopy() {
  if (!copySourceId.value) { copyError.value = 'Seleziona un condominio sorgente'; return }
  copying.value = true
  try {
    await chartOfAccountsApi.copyFromCondominium({
      sourceCondominiumId: copySourceId.value,
      targetCondominiumId: store.selectedCondominioId,
    })
    showCopy.value = false
    await load()
  } catch (err) {
    if (!err?.response) throw err
  } finally { copying.value = false }
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

.header-actions { display: flex; gap: .5rem; align-items: center; }

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
.badge-type     { background: rgba(99,102,241,.12); color: #818cf8; }
.badge-open     { background: rgba(52,211,153,.12); color: #34d399; }
.badge-muted    { background: rgba(100,116,139,.12); color: #64748b; }
.badge-standard { background: rgba(100,116,139,.10); color: #64748b; }
.badge-mixed    { background: rgba(251,191,36,.12);  color: #f59e0b; cursor: help; }

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
  text-decoration: none;
}
.btn-primary { background: #34d399; color: #0f172a; }
.btn-primary:hover:not(:disabled) { background: #2ac488; }
.btn-ghost { background: transparent; color: var(--text-muted); border: 1px solid var(--border); }
.btn-ghost:hover { background: var(--bg-surface); }
.btn-danger { background: #f87171; color: #fff; }
.btn-danger:hover:not(:disabled) { background: #ef4444; }
.btn:disabled { opacity: .55; cursor: not-allowed; }

.form-stack { display: flex; flex-direction: column; gap: .9rem; }
.copy-hint, .mixed-hint {
  display: flex; align-items: flex-start; gap: .5rem;
  font-size: .8rem; color: var(--text-muted);
  background: var(--bg-surface); border-radius: 7px;
  padding: .65rem .85rem; line-height: 1.45;
}
.copy-hint .pi, .mixed-hint .pi { margin-top: .1rem; flex-shrink: 0; }

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
