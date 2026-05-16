<template>
  <div>
    <!-- Search / filter toolbar -->
    <ToolbarRow>
      <template #left>
        <input class="form-input search-input" v-model="search" placeholder="Cerca per numero interno…" />
        <select v-if="buildings.length" class="form-select filter-select" v-model="filterBuilding">
          <option value="">Tutti gli edifici</option>
          <option v-for="b in buildings" :key="b.id" :value="String(b.id)">{{ b.name }}</option>
        </select>
        <select v-if="staircases.length" class="form-select filter-select" v-model="filterStaircase">
          <option value="">Tutte le scale</option>
          <option v-for="s in toolbarStaircaseOptions" :key="s.id" :value="String(s.id)">{{ s.name }}</option>
        </select>
        <select class="form-select filter-select" v-model="filterType">
          <option value="">Tutti i tipi</option>
          <option v-for="t in unitTypes" :key="t" :value="t">{{ t }}</option>
        </select>
        <select class="form-select filter-select" v-model="filterActive">
          <option value="">Tutte</option>
          <option value="true">Attive</option>
          <option value="false">Inattive</option>
        </select>
      </template>
      <template #right>
        <button v-if="canCreate" class="btn btn-primary" @click="openModal()">
          <i class="pi pi-plus" style="font-size:0.8rem;margin-right:0.35rem"></i>Nuova unità
        </button>
      </template>
    </ToolbarRow>

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
              <th>Denominazione</th>
              <th v-if="buildings.length">Edificio</th>
              <th>Scala</th>
              <th>Piano</th>
              <th>Tipo</th>
              <th>Occupazione</th>
              <th>Mq</th>
              <th>Stato</th>
              <th class="text-center">Prop.</th>
              <th class="text-center">Inq.</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="u in filtered" :key="u.id">
              <td class="mono" style="font-weight:500">{{ u.internalNumber || '—' }}</td>
              <td class="display-name-cell">{{ u.displayName || '—' }}</td>
              <td v-if="buildings.length" class="text-secondary">{{ u.buildingName || '—' }}</td>
              <td>{{ u.staircaseName || '—' }}</td>
              <td>{{ u.floor }}</td>
              <td class="text-secondary">{{ u.unitType || '—' }}</td>
              <td class="text-secondary">{{ u.occupancyStatus || '—' }}</td>
              <td class="text-secondary">{{ u.areaSqm != null ? `${u.areaSqm} m²` : '—' }}</td>
              <td><span class="badge" :class="u.isActive ? 'badge-green' : 'badge-muted'">{{ u.isActive ? 'Attiva' : 'Inattiva' }}</span></td>
              <td class="text-center text-secondary count-cell">{{ u.ownersCount }}</td>
              <td class="text-center text-secondary count-cell">{{ u.tenantsCount }}</td>
              <td>
                <div class="row-actions">
                  <button class="btn-icon" @click="openOccupanti(u)" title="Occupanti">👤</button>
                  <button v-if="canEdit" class="btn-icon" @click="openBalanceModal(u)" title="Bilancio iniziale">₿</button>
                  <button v-if="canCreate" class="btn-icon" @click="cloneUnit(u)" title="Clona unità">⧉</button>
                  <button v-if="canEdit" class="btn-icon" @click="openModal(u)" title="Modifica">✎</button>
                  <button v-if="canDelete" class="btn-icon" @click="deleteItem(u.id)" title="Elimina" style="color:var(--accent-red)">✕</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Modal -->
    <div class="modal-overlay" v-if="showModal" @mousedown.self="showModal=false">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ editing ? 'Modifica' : isCloning ? 'Clona' : 'Nuova' }} unità</h2>
          <button class="btn-icon" @click="showModal=false">✕</button>
        </div>
        <div class="modal-body">

          <fieldset class="form-fieldset">
            <legend class="form-fieldset-legend">Identificazione</legend>
            <div class="form-grid">
              <div v-if="editing" class="form-group">
                <label class="form-label">ID</label>
                <input class="form-input" :value="editing" readonly />
              </div>
              <div v-if="editing || isCloning" class="form-group form-group--full">
                <label class="form-label">Denominazione</label>
                <input class="form-input" :value="form.displayName || '—'" readonly />
              </div>
              <div class="form-group">
                <label class="form-label">N° interno</label>
                <input class="form-input" v-model="form.internalNumber" placeholder="Es. 1" />
              </div>
              <div v-if="buildings.length" class="form-group">
                <label class="form-label">Edificio</label>
                <select class="form-select" v-model.number="form.buildingId">
                  <option :value="null">— Nessuno —</option>
                  <option v-for="b in buildings" :key="b.id" :value="b.id">{{ b.name }}</option>
                </select>
              </div>
              <div class="form-group">
                <label class="form-label">Scala</label>
                <select class="form-select" v-model.number="form.staircaseId">
                  <option :value="null">— Nessuna —</option>
                  <option v-for="s in modalStaircases" :key="s.id" :value="s.id">{{ s.name }}</option>
                </select>
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
                <label class="form-label">Foglio</label>
                <input class="form-input" v-model="form.sheet" placeholder="Es. 12" maxlength="20" />
              </div>
              <div class="form-group">
                <label class="form-label">Particella</label>
                <input class="form-input" v-model="form.parcel" placeholder="Es. 345" maxlength="20" />
              </div>
              <div class="form-group">
                <label class="form-label">Subalterno</label>
                <input class="form-input" v-model="form.subordinate" placeholder="Es. A" maxlength="20" />
              </div>
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
              <div class="form-group">
                <label class="form-label">Numero abitanti</label>
                <input class="form-input" type="number" min="0" step="1" v-model.number="form.numeroAbitanti" />
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
    @close="onOccupantiClose"
  />

  <!-- Bilancio esercizio modal -->
  <div class="modal-overlay" v-if="showBalanceModal" @mousedown.self="showBalanceModal=false">
    <div class="modal modal--wide">
      <div class="modal-header">
        <h2>Bilancio esercizio — {{ balanceUnit?.displayName || balanceUnit?.internalNumber }}</h2>
        <button class="btn-icon" @click="showBalanceModal=false">✕</button>
      </div>
      <div class="modal-body">

        <div class="form-group">
          <label class="form-label">Esercizio *</label>
          <select class="form-select" v-model.number="balanceForm.fiscalYearId" @change="onBalanceFiscalYearChange">
            <option :value="null">— Seleziona —</option>
            <option v-for="fy in fiscalYears" :key="fy.id" :value="fy.id">
              {{ fy.code }}{{ fy.isActive ? ' (attivo)' : '' }}
            </option>
          </select>
        </div>

        <div v-if="balanceLoading" class="loading-state" style="padding:1rem 0"><div class="spinner"></div></div>

        <template v-else-if="balanceForm.fiscalYearId">

          <!-- Info: non modificabile (propagato da esercizio precedente o già chiuso) -->
          <div v-if="balanceData && !balanceData.isEditable" class="info-banner">
            <span>&#9432;</span>
            <span v-if="balanceData.isClosed">
              L'esercizio è chiuso. Il bilancio è definitivo e non modificabile.
            </span>
            <span v-else>
              Il saldo di apertura è propagato automaticamente dal saldo di chiusura dell'esercizio precedente e non è modificabile.
            </span>
          </div>

          <!-- Riepilogo saldi (read-only se esercizio chiuso) -->
          <div v-if="balanceData?.isClosed" class="balance-summary">
            <div class="balance-row">
              <span class="balance-label">Saldo apertura</span>
              <span class="balance-value" :class="balanceData.openingBalance >= 0 ? 'text-green' : 'text-red'">
                {{ fmtCurrency(balanceData.openingBalance) }}
              </span>
            </div>
            <div class="balance-row">
              <span class="balance-label">Totale movimenti (quote - pagamenti)</span>
              <span class="balance-value" :class="balanceData.totalMovements >= 0 ? 'text-red' : 'text-green'">
                {{ fmtCurrency(balanceData.totalMovements) }}
              </span>
            </div>
            <div class="balance-row balance-row--total">
              <span class="balance-label">Saldo chiusura</span>
              <span class="balance-value" :class="balanceData.closingBalance >= 0 ? 'text-green' : 'text-red'">
                {{ fmtCurrency(balanceData.closingBalance) }}
              </span>
            </div>
          </div>

          <!-- Form modifica (solo primo esercizio aperto) -->
          <template v-else>
            <div class="form-group" style="margin-top:1rem">
              <label class="form-label">Saldo di apertura (€)</label>
              <input class="form-input" type="number" step="0.01"
                v-model.number="balanceForm.openingBalance"
                :disabled="balanceData && !balanceData.isEditable" />
              <span class="field-hint">Positivo = credito verso il condominio; negativo = debito.</span>
            </div>
            <div class="form-group">
              <label class="form-label">Note</label>
              <textarea class="form-textarea" v-model="balanceForm.notes" rows="2"
                :disabled="balanceData && !balanceData.isEditable"></textarea>
            </div>
          </template>

        </template>

      </div>
      <div class="modal-footer">
        <button class="btn btn-ghost" @click="showBalanceModal=false">Chiudi</button>
        <button v-if="balanceData && !balanceData.isClosed && balanceData.isEditable"
          class="btn btn-primary"
          @click="saveBalance"
          :disabled="balanceSaving || !balanceForm.fiscalYearId">
          <span v-if="balanceSaving" class="spinner" style="width:14px;height:14px"></span>
          Salva
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import { useAppStore } from '@/stores/app'
import { unitApi, fiscalYearApi, buildingApi, staircaseApi } from '@/services/api'
import OccupantiModal from '@/views/condomini/OccupantiModal.vue'
import { usePermissions } from '@/composables/usePermissions'
import ToolbarRow from '@/components/ToolbarRow.vue'

const route  = useRoute()
const store  = useAppStore()
const { canCreate, canEdit, canDelete } = usePermissions()

// condominiumId: from route param when inside CondominioLayout, otherwise from selected condominium in store
const condominiumId = computed(() => Number(route.params.id) || store.selectedCondominioId)

const loading         = ref(false)
const saving          = ref(false)
const showModal       = ref(false)
const editing         = ref(null)
const isCloning       = ref(false)
const occupantiUnit   = ref(null)
const search          = ref('')
const filterStaircase = ref('')
const filterFloor     = ref(null)
const filterType      = ref('')
const filterActive    = ref('')
const filterBuilding  = ref('')
const errors          = ref({})
const units           = ref([])
const buildings       = ref([])

// ── Bilancio esercizio ─────────────────────────────────────────────────────
const showBalanceModal  = ref(false)
const balanceUnit       = ref(null)
const fiscalYears       = ref([])
const balanceForm       = ref({ fiscalYearId: null, openingBalance: 0, notes: '' })
const balanceData       = ref(null)   // UnitOpeningBalanceReadDto
const balanceSaving     = ref(false)
const balanceLoading    = ref(false)

function fmtCurrency(v) {
  if (v == null) return '—'
  return new Intl.NumberFormat('it-IT', { style: 'currency', currency: 'EUR' }).format(v)
}

const unitTypes = [
  'Residenziale', 'Commerciale', 'Artigianale', 'Direzionale',
  'Autorimessa', 'Cantina', 'Deposito', 'Altro',
]
const occupancyStatuses = ['Occupata proprietario', 'Occupata inquilino', 'Libera', 'Non abitabile']

const defaultForm = () => ({
  condominiumId:   condominiumId.value,
  buildingId:      null,
  staircaseId:     null,
  internalNumber:  '',
  sheet:           '',
  parcel:          '',
  subordinate:     '',
  floor:           0,
  unitType:        '',
  category:        '',
  occupancyStatus: '',
  areaSqm:         null,
  rooms:           null,
  cadastralIncome: null,
  numeroAbitanti:  1,
  notes:           '',
  isActive:        true,
})
const form = ref(defaultForm())

const staircases = ref([])

async function loadStaircases() {
  if (!condominiumId.value) { staircases.value = []; return }
  try {
    const { data } = await staircaseApi.getByCondominium(condominiumId.value)
    staircases.value = (data ?? []).filter(s => s.isActive)
  } catch {
    staircases.value = []
  }
}

const toolbarStaircaseOptions = computed(() =>
  staircases.value
)

// Scale filtrate per l'edificio selezionato nella modale
const modalStaircases = computed(() => {
  if (!form.value.buildingId) return staircases.value
  return staircases.value.filter(s => s.buildingId === form.value.buildingId)
})

const floorOptions = computed(() =>
  [...new Set(units.value.map(u => u.floor).filter(v => v != null))].sort((a, b) => a - b)
)

const filtered = computed(() => {
  let list = units.value
  if (search.value) {
    const q = search.value.toLowerCase()
    list = list.filter(u => u.internalNumber?.toLowerCase().includes(q))
  }
  if (filterBuilding.value) list = list.filter(u => String(u.buildingId) === filterBuilding.value)
  if (filterStaircase.value) list = list.filter(u => String(u.staircaseId) === filterStaircase.value)
  if (filterFloor.value != null) list = list.filter(u => u.floor === filterFloor.value)
  if (filterType.value)   list = list.filter(u => u.unitType === filterType.value)
  if (filterActive.value !== '') list = list.filter(u => String(u.isActive) === filterActive.value)
  return list
})

async function loadData() {
  if (!condominiumId.value) {
    units.value = []
    buildings.value = []
    return
  }
  loading.value = true
  try {
    const [unitsRes, buildingsRes] = await Promise.all([
      unitApi.getByCondominium(condominiumId.value),
      buildingApi.getByCondominium(condominiumId.value),
      loadStaircases(),
    ])
    units.value     = unitsRes.data ?? []
    buildings.value = buildingsRes.data ?? []
  } catch {
    // error handled by interceptor
  } finally {
    loading.value = false
  }
}

function openOccupanti(unit) {
  occupantiUnit.value = unit
}

function onOccupantiClose() {
  occupantiUnit.value = null
  loadData()
}

function openModal(item = null) {
  editing.value  = item?.id ?? null
  isCloning.value = false
  form.value = item ? { ...item, condominiumId: condominiumId.value } : defaultForm()
  errors.value = {}
  showModal.value = true
}

function cloneUnit(item) {
  editing.value   = null
  isCloning.value = true
  form.value = {
    ...item,
    condominiumId: condominiumId.value,
    internalNumber: '',
    displayName:    '',
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

// ── Bilancio esercizio: logica ─────────────────────────────────────────────
async function openBalanceModal(unit) {
  balanceUnit.value  = unit
  balanceData.value  = null
  balanceForm.value  = { fiscalYearId: null, openingBalance: 0, notes: '' }
  showBalanceModal.value = true

  try {
    const { data } = await fiscalYearApi.getByCondominium(condominiumId.value)
    fiscalYears.value = (data ?? []).filter(f => !f.isDeleted)
    const active = fiscalYears.value.find(f => f.isActive) ?? fiscalYears.value[0]
    if (active) {
      balanceForm.value.fiscalYearId = active.id
      await loadBalance(unit.id, active.id)
    }
  } catch { /* handled by interceptor */ }
}

async function loadBalance(unitId, fiscalYearId) {
  if (!fiscalYearId) return
  balanceLoading.value = true
  try {
    const { data } = await unitApi.getOpeningBalance(unitId, fiscalYearId)
    balanceData.value                = data
    balanceForm.value.openingBalance = data.openingBalance ?? 0
    balanceForm.value.notes          = data.notes  ?? ''
  } catch {
    balanceData.value = null
  } finally {
    balanceLoading.value = false
  }
}

async function onBalanceFiscalYearChange() {
  if (balanceUnit.value && balanceForm.value.fiscalYearId) {
    await loadBalance(balanceUnit.value.id, balanceForm.value.fiscalYearId)
  }
}

async function saveBalance() {
  if (!balanceUnit.value || !balanceForm.value.fiscalYearId) return
  balanceSaving.value = true
  try {
    const { data } = await unitApi.setOpeningBalance(balanceUnit.value.id, {
      fiscalYearId:   balanceForm.value.fiscalYearId,
      openingBalance: balanceForm.value.openingBalance,
      notes:          balanceForm.value.notes,
    })
    balanceData.value = data
    store.toast('Bilancio di apertura salvato', 'success')
    showBalanceModal.value = false
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    balanceSaving.value = false
  }
}

onMounted(loadData)
onUnmounted(() => window.removeEventListener('app:refresh', loadData))
window.addEventListener('app:refresh', loadData)
watch(condominiumId, loadData)
watch(filterBuilding, () => { filterStaircase.value = '' })

// Quando si cambia edificio nella modale, resetta la scala se non appartiene al nuovo edificio
watch(() => form.value.buildingId, (newBuildingId) => {
  if (!form.value.staircaseId) return
  const currentStaircase = staircases.value.find(s => s.id === form.value.staircaseId)
  if (currentStaircase && currentStaircase.buildingId !== newBuildingId) {
    form.value.staircaseId = null
  }
})
</script>

<style scoped>
.search-input { flex: 1; min-width: 160px; max-width: 280px; }
.filter-select { width: 140px; }
.row-actions { display: flex; gap: 0.4rem; justify-content: flex-end; }
.count-cell { width: 48px; font-size: 0.8125rem; }
.display-name-cell { max-width: 200px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; color: var(--text-secondary); font-size: 0.875rem; }

.form-group--full { grid-column: 1 / -1; }
.form-fieldset { border: 1px solid var(--border); border-radius: 6px; padding: 1rem; margin-top: 1rem; }
.form-fieldset-legend { font-size: 0.8125rem; font-weight: 600; color: var(--text-secondary); padding: 0 0.4rem; }

.has-error .form-input,
.has-error .form-select { border-color: var(--accent-red, #e53e3e); }
.field-error { font-size: 0.78rem; color: var(--accent-red, #e53e3e); margin-top: 0.2rem; }

.info-banner { display:flex; align-items:flex-start; gap:0.5rem; background:var(--bg-surface); border:1px solid var(--border); border-radius:6px; padding:0.75rem 1rem; font-size:0.875rem; color:var(--text-muted); margin-top:0.5rem; }
.field-hint  { font-size:0.78rem; color:var(--text-muted); margin-top:0.2rem; }

.modal--wide { min-width: 520px; }


.balance-summary { margin-top: 1.25rem; border: 1px solid var(--border); border-radius: 8px; overflow: hidden; }
.balance-row { display: flex; justify-content: space-between; align-items: center; padding: 0.65rem 1rem; border-bottom: 1px solid var(--border); font-size: 0.9rem; }
.balance-row:last-child { border-bottom: none; }
.balance-row--total { background: var(--bg-surface); font-weight: 600; }
.balance-label { color: var(--text-secondary); }
.balance-value { font-family: monospace; font-size: 1rem; }
.text-green { color: var(--accent-green, #22c55e); }
.text-red   { color: var(--accent-red,   #ef4444); }
</style>
