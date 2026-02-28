<template>
  <div>
    <div class="page-header">
      <div class="header-left">
        <router-link :to="`/condomini/${condominiumId}`" class="btn btn-ghost btn-sm">← Indietro</router-link>
        <h1>Unità · {{ condominiumName }}</h1>
      </div>
      <button class="btn btn-primary" @click="openModal()">+ Nuova unità</button>
    </div>

    <!-- Search / filter -->
    <div class="toolbar">
      <input class="form-input search-input" v-model="search" placeholder="Cerca per numero interno, scala…" />
      <select class="form-select" v-model="filterType" style="width:180px">
        <option value="">Tutti i tipi</option>
        <option v-for="t in unitTypes" :key="t" :value="t">{{ t }}</option>
      </select>
      <select class="form-select" v-model="filterActive" style="width:140px">
        <option value="">Tutte</option>
        <option value="true">Attive</option>
        <option value="false">Inattive</option>
      </select>
    </div>

    <div class="card">
      <div v-if="loading" class="loading-state"><div class="spinner"></div> Caricamento…</div>
      <div v-else-if="!filtered.length" class="empty-state">
        <div class="empty-icon">⊞</div>
        <div>Nessuna unità trovata</div>
      </div>
      <div v-else class="table-wrap">
        <table>
          <thead>
            <tr>
              <th>N° Interno</th>
              <th>Scala</th>
              <th>Piano</th>
              <th>Tipo</th>
              <th>Occupazione</th>
              <th>Mq</th>
              <th>Stato</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="u in filtered" :key="u.id">
              <td class="mono" style="font-weight:500">{{ u.internalNumber || '—' }}</td>
              <td>{{ u.staircase || '—' }}</td>
              <td>{{ u.floor }}</td>
              <td class="text-secondary">{{ u.unitType || '—' }}</td>
              <td class="text-secondary">{{ u.occupancyStatus || '—' }}</td>
              <td class="text-secondary">{{ u.areaSqm != null ? `${u.areaSqm} m²` : '—' }}</td>
              <td><span class="badge" :class="u.isActive ? 'badge-green' : 'badge-muted'">{{ u.isActive ? 'Attiva' : 'Inattiva' }}</span></td>
              <td>
                <div class="row-actions">
                  <button class="btn-icon" @click="openOccupanti(u)" title="Occupanti">👤</button>
                  <button class="btn-icon" @click="openModal(u)" title="Modifica">✎</button>
                  <button class="btn-icon" @click="deleteItem(u.id)" title="Elimina" style="color:var(--accent-red)">✕</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Modal -->
    <div class="modal-overlay" v-if="showModal" @click.self="showModal=false">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ editing ? 'Modifica' : 'Nuova' }} unità</h2>
          <button class="btn-icon" @click="showModal=false">✕</button>
        </div>
        <div class="modal-body">

          <fieldset class="form-fieldset">
            <legend class="form-fieldset-legend">Identificazione</legend>
            <div class="form-grid">
              <div class="form-group" :class="{ 'has-error': errors.internalNumber }">
                <label class="form-label">N° interno *</label>
                <input class="form-input" v-model="form.internalNumber" placeholder="Es. 1" @input="clearError('internalNumber')" />
                <span v-if="errors.internalNumber" class="field-error">{{ errors.internalNumber }}</span>
              </div>
              <div class="form-group">
                <label class="form-label">Subordinato</label>
                <input class="form-input" v-model="form.subordinate" placeholder="Es. A" />
              </div>
              <div class="form-group">
                <label class="form-label">Scala</label>
                <input class="form-input" v-model="form.staircase" placeholder="Es. A" maxlength="10" />
              </div>
              <div class="form-group" :class="{ 'has-error': errors.floor }">
                <label class="form-label">Piano *</label>
                <input class="form-input" type="number" v-model.number="form.floor" @input="clearError('floor')" />
                <span v-if="errors.floor" class="field-error">{{ errors.floor }}</span>
              </div>
            </div>
          </fieldset>

          <fieldset class="form-fieldset">
            <legend class="form-fieldset-legend">Classificazione</legend>
            <div class="form-grid">
              <div class="form-group">
                <label class="form-label">Tipo unità</label>
                <select class="form-select" v-model="form.unitType">
                  <option value="">— Seleziona —</option>
                  <option v-for="t in unitTypes" :key="t" :value="t">{{ t }}</option>
                </select>
              </div>
              <div class="form-group">
                <label class="form-label">Categoria catastale</label>
                <input class="form-input" v-model="form.category" placeholder="Es. A/2" maxlength="10" />
              </div>
              <div class="form-group">
                <label class="form-label">Stato occupazione</label>
                <select class="form-select" v-model="form.occupancyStatus">
                  <option value="">— Seleziona —</option>
                  <option v-for="s in occupancyStatuses" :key="s" :value="s">{{ s }}</option>
                </select>
              </div>
            </div>
          </fieldset>

          <fieldset class="form-fieldset">
            <legend class="form-fieldset-legend">Dati catastali e superfici</legend>
            <div class="form-grid">
              <div class="form-group">
                <label class="form-label">Superficie (mq)</label>
                <input class="form-input" type="number" min="0" step="0.01" v-model.number="form.areaSqm" />
              </div>
              <div class="form-group">
                <label class="form-label">Vani</label>
                <input class="form-input" type="number" min="0" step="0.5" v-model.number="form.rooms" />
              </div>
              <div class="form-group">
                <label class="form-label">Rendita catastale (€)</label>
                <input class="form-input" type="number" min="0" step="0.01" v-model.number="form.cadastralIncome" />
              </div>
            </div>
          </fieldset>

          <div class="form-group" style="margin-top:1rem">
            <label class="form-label">Note</label>
            <textarea class="form-textarea" v-model="form.notes" rows="2"></textarea>
          </div>
          <div class="form-group" style="flex-direction:row;align-items:center;gap:0.5rem;margin-top:0.5rem">
            <input type="checkbox" id="unitIsActive" v-model="form.isActive" />
            <label for="unitIsActive" style="font-size:0.875rem;cursor:pointer">Unità attiva</label>
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

  <!-- Occupanti modal -->
  <OccupantiModal
    v-if="occupantiUnit"
    :unit-id="occupantiUnit.id"
    :unit-label="occupantiUnit.internalNumber || `#${occupantiUnit.id}`"
    @close="occupantiUnit = null"
  />
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useAppStore } from '@/stores/app'
import { unitApi, condominiumApi } from '@/services/api'
import OccupantiModal from '@/views/condomini/OccupantiModal.vue'

const route           = useRoute()
const store           = useAppStore()
const condominiumId   = Number(route.params.condominiumId)
const condominiumName = ref('')

const loading        = ref(false)
const saving         = ref(false)
const showModal      = ref(false)
const editing        = ref(null)
const occupantiUnit  = ref(null)
const search         = ref('')
const filterType     = ref('')
const filterActive = ref('')
const errors      = ref({})
const units       = ref([])

const unitTypes = [
  'Residenziale', 'Commerciale', 'Artigianale', 'Direzionale',
  'Autorimessa', 'Cantina', 'Deposito', 'Altro',
]
const occupancyStatuses = ['Occupata proprietario', 'Occupata inquilino', 'Libera', 'Non abitabile']

const defaultForm = () => ({
  condominiumId,
  internalNumber:  '',
  subordinate:     '',
  staircase:       '',
  floor:           0,
  unitType:        '',
  category:        '',
  occupancyStatus: '',
  areaSqm:         null,
  rooms:           null,
  cadastralIncome: null,
  notes:           '',
  isActive:        true,
})
const form = ref(defaultForm())

const filtered = computed(() => {
  let list = units.value
  if (search.value) {
    const q = search.value.toLowerCase()
    list = list.filter(u =>
      u.internalNumber?.toLowerCase().includes(q) ||
      u.staircase?.toLowerCase().includes(q) ||
      String(u.floor).includes(q)
    )
  }
  if (filterType.value) list = list.filter(u => u.unitType === filterType.value)
  if (filterActive.value !== '') list = list.filter(u => String(u.isActive) === filterActive.value)
  return list
})

async function loadData() {
  loading.value = true
  try {
    const { data } = await unitApi.getByCondominium(condominiumId)
    units.value = data
  } catch {
    // error handled by interceptor
  } finally {
    loading.value = false
  }
}

function openOccupanti(unit) {
  occupantiUnit.value = unit
}

function openModal(item = null) {
  editing.value = item?.id ?? null
  if (item) {
    form.value = { ...item, condominiumId }
  } else {
    form.value = defaultForm()
  }
  errors.value = {}
  showModal.value = true
}

function clearError(field) {
  delete errors.value[field]
}

function validate() {
  const e = {}
  const f = form.value
  if (!f.internalNumber?.trim())
    e.internalNumber = 'Il numero interno è obbligatorio'
  if (f.floor == null || f.floor === '')
    e.floor = 'Il piano è obbligatorio'
  errors.value = e
  return Object.keys(e).length === 0
}

async function save() {
  if (!validate()) return
  saving.value = true
  try {
    if (editing.value) {
      await unitApi.update(editing.value, form.value)
      store.toast('Unità aggiornata', 'success')
    } else {
      await unitApi.create(form.value)
      store.toast('Unità creata', 'success')
    }
    showModal.value = false
    await loadData()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    saving.value = false
  }
}

async function deleteItem(id) {
  if (!confirm('Eliminare questa unità?')) return
  try {
    await unitApi.delete(id)
    store.toast('Unità eliminata', 'success')
    await loadData()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

onMounted(async () => {
  try {
    const { data } = await condominiumApi.getById(condominiumId)
    condominiumName.value = data.name
  } catch { /* ignored */ }
  await loadData()
})
</script>

<style scoped>
.header-left { display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap; }
.toolbar { display: flex; gap: 0.75rem; margin-bottom: 1rem; flex-wrap: wrap; }
.search-input { flex: 1; min-width: 200px; max-width: 360px; }
.row-actions { display: flex; gap: 0.4rem; justify-content: flex-end; }

.form-fieldset { border: 1px solid var(--border); border-radius: 6px; padding: 1rem; margin-top: 1rem; }
.form-fieldset-legend { font-size: 0.8125rem; font-weight: 600; color: var(--text-secondary); padding: 0 0.4rem; }

.has-error .form-input,
.has-error .form-select { border-color: var(--accent-red, #e53e3e); }
.field-error { font-size: 0.78rem; color: var(--accent-red, #e53e3e); margin-top: 0.2rem; }
</style>
