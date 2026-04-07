<template>
  <div>
    <div class="page-header">
      <h1>Consumi</h1>
    </div>

    <div v-if="!store.selectedCondominioId" class="card">
      <div class="empty-state">
        <div class="empty-icon">◎</div>
        <div>Seleziona un condominio per gestire i consumi</div>
      </div>
    </div>

    <template v-else>
      <!-- Tab bar -->
      <div class="tab-bar">
        <button class="tab-btn" :class="{ active: tab === 'types' }"    @click="tab = 'types'">Tipi consumo</button>
        <button class="tab-btn" :class="{ active: tab === 'meters' }"   @click="tab = 'meters'">Contatori</button>
        <button class="tab-btn" :class="{ active: tab === 'readings' }" @click="tab = 'readings'" :disabled="!selectedTypeId">Letture</button>
        <button class="tab-btn" :class="{ active: tab === 'charges' }"  @click="tab = 'charges'">Ripartizioni</button>
      </div>

      <!-- ── TAB: Tipi consumo ──────────────────────────────────────────── -->
      <div v-if="tab === 'types'" class="card">
        <div class="toolbar">
          <span class="text-secondary">{{ consumptionTypes.length }} tipo/i</span>
          <button v-if="canCreate" class="btn btn-primary btn-sm" @click="openTypeModal()">+ Nuovo tipo</button>
        </div>
        <div v-if="loadingTypes" class="loading-state"><div class="spinner"></div></div>
        <div v-else-if="!consumptionTypes.length" class="empty-state">
          <div class="empty-icon">◎</div>
          <div>Nessun tipo di consumo. Crea il primo.</div>
        </div>
        <div v-else class="table-wrap">
          <table>
            <thead><tr><th>Nome</th><th>U.M.</th><th>Stato</th><th></th></tr></thead>
            <tbody>
              <tr v-for="t in consumptionTypes" :key="t.id">
                <td>
                  <button class="link-btn" @click="selectType(t)">{{ t.name }}</button>
                </td>
                <td class="text-secondary mono">{{ t.unitOfMeasure }}</td>
                <td><span class="badge" :class="t.isActive ? 'badge-green' : 'badge-muted'">{{ t.isActive ? 'Attivo' : 'Inattivo' }}</span></td>
                <td>
                  <div class="row-actions">
                    <button v-if="canEdit" class="btn-icon" @click="openTypeModal(t)">✎</button>
                    <button v-if="canDelete" class="btn-icon btn-icon--danger" @click="deleteType(t.id)">🗑</button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- ── TAB: Contatori ─────────────────────────────────────────────── -->
      <div v-if="tab === 'meters'" class="card">
        <div class="toolbar">
          <span class="text-secondary">{{ meters.length }} contatore/i</span>
          <button v-if="canCreate" class="btn btn-primary btn-sm" @click="openMeterModal()">+ Nuovo contatore</button>
        </div>
        <div v-if="loadingMeters" class="loading-state"><div class="spinner"></div></div>
        <div v-else-if="!meters.length" class="empty-state">
          <div class="empty-icon">◎</div>
          <div>Nessun contatore. Crea il primo.</div>
        </div>
        <div v-else class="table-wrap">
          <table>
            <thead><tr><th>Tipo</th><th>Unità</th><th>Matricola</th><th>Stato</th><th></th></tr></thead>
          <tbody>
            <tr v-for="m in meters" :key="m.id">
              <td>{{ m.consumptionTypeName }}</td>
              <td class="text-secondary">{{ m.unitName }}</td>
              <td class="mono">{{ m.code }}</td>
              <td><span class="badge" :class="m.isActive ? 'badge-green' : 'badge-muted'">{{ m.isActive ? 'Attivo' : 'Inattivo' }}</span></td>
              <td>
                <div class="row-actions">
                  <button v-if="canEdit" class="btn-icon" @click="openMeterModal(m)">✎</button>
                  <button v-if="canDelete" class="btn-icon btn-icon--danger" @click="deleteMeter(m.id)">🗑</button>
                </div>
              </td>
            </tr>
          </tbody>
          </table>
        </div>
      </div>

      <!-- ── TAB: Letture ───────────────────────────────────────────────── -->
      <div v-if="tab === 'readings'">
        <!-- Selettore tipo + esercizio -->
        <div class="card readings-filter">
          <div class="form-grid">
            <div class="form-group">
              <label class="form-label">Tipo consumo</label>
              <select class="form-select" v-model="selectedTypeId" @change="onTypeOrFyChange">
                <option :value="null" disabled>— Seleziona —</option>
                <option v-for="t in consumptionTypes" :key="t.id" :value="t.id">{{ t.name }} ({{ t.unitOfMeasure }})</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Esercizio fiscale</label>
              <select class="form-select" v-model="selectedFiscalYearId" @change="onTypeOrFyChange">
                <option :value="null" disabled>— Seleziona —</option>
                <option v-for="fy in fiscalYears" :key="fy.id" :value="fy.id">{{ fy.code }}{{ fy.description ? ' – ' + fy.description : '' }}</option>
              </select>
            </div>
          </div>
        </div>

        <div class="card" v-if="selectedTypeId && selectedFiscalYearId">
          <div class="toolbar">
            <span class="text-secondary">Letture contatori — {{ selectedTypeName }}</span>
            <button v-if="canEdit && readingsModified" class="btn btn-primary btn-sm" @click="saveReadings" :disabled="savingReadings">
              <span v-if="savingReadings" class="spinner" style="width:14px;height:14px"></span>
              Salva letture
            </button>
          </div>
          <div v-if="loadingReadings" class="loading-state"><div class="spinner"></div></div>
          <div v-else-if="!readingRows.length" class="empty-state">
            <div class="empty-icon">◎</div>
            <div>Nessun contatore per questo tipo di consumo. Aggiungi prima i contatori.</div>
          </div>
          <div v-else class="table-wrap">
            <table class="readings-table">
              <thead>
                <tr>
                  <th>Unità</th>
                  <th>Matricola</th>
                  <th class="col-num">Lettura iniziale</th>
                  <th class="col-date">Data inizio</th>
                  <th class="col-num">Lettura finale</th>
                  <th class="col-date">Data fine</th>
                  <th class="col-num">Consumo</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="row in readingRows" :key="row.meterId" :class="{ 'row-warning': row.hasWarning }">
                  <td>{{ row.unitName }}</td>
                  <td class="mono text-secondary">{{ row.meterCode }}</td>
                  <td class="col-num">
                    <input class="inline-input" type="number" step="0.001" v-model.number="row.initialValue" @input="readingsModified = true" />
                  </td>
                  <td class="col-date">
                    <input class="inline-input" type="date" v-model="row.initialDate" @input="readingsModified = true" />
                  </td>
                  <td class="col-num">
                    <input class="inline-input" type="number" step="0.001" v-model.number="row.finalValue" @input="readingsModified = true" />
                  </td>
                  <td class="col-date">
                    <input class="inline-input" type="date" v-model="row.finalDate" @input="readingsModified = true" />
                  </td>
                  <td class="col-num" :class="consumptionClass(row)">
                    {{ fmtNum(row.finalValue - row.initialValue) }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- ── TAB: Ripartizioni ──────────────────────────────────────────── -->
      <div v-if="tab === 'charges'" class="card">
        <div class="toolbar">
          <div class="form-group" style="margin:0; min-width:200px">
            <select class="form-select" v-model="chargesFiscalYearId" @change="loadCharges">
              <option :value="null" disabled>— Esercizio —</option>
              <option v-for="fy in fiscalYears" :key="fy.id" :value="fy.id">{{ fy.code }}</option>
            </select>
          </div>
          <button v-if="canCreate && chargesFiscalYearId" class="btn btn-primary btn-sm" @click="openChargeModal()">+ Nuova ripartizione</button>
        </div>

        <div v-if="loadingCharges" class="loading-state"><div class="spinner"></div></div>
        <div v-else-if="!charges.length && chargesFiscalYearId" class="empty-state">
          <div class="empty-icon">◎</div>
          <div>Nessuna ripartizione per questo esercizio.</div>
        </div>
        <template v-else>
          <div v-for="charge in charges" :key="charge.id" class="charge-card">
            <div class="charge-header">
              <div class="charge-title">
                <span class="charge-type">{{ charge.consumptionTypeName }}</span>
                <span class="text-secondary mono">{{ charge.unitOfMeasure }}</span>
                <span class="badge" :class="charge.statusId === 2 ? 'badge-green' : 'badge-draft'">{{ charge.statusName }}</span>
                <span v-if="charge.hasWarnings" class="badge badge-amber" title="Alcune unità non hanno letture">⚠ warning</span>
              </div>
              <div class="charge-amount">{{ fmt(charge.totalAmount) }}</div>
              <div class="row-actions" v-if="charge.statusId === 1">
                <button v-if="canEdit" class="btn btn-sm btn-ghost" @click="recalculate(charge.id)">↺ Ricalcola</button>
                <button v-if="canEdit" class="btn btn-sm btn-ghost btn-green" @click="approveCharge(charge.id)">✓ Approva</button>
                <button v-if="canEdit" class="btn-icon" @click="openChargeModal(charge)">✎</button>
                <button v-if="canDelete" class="btn-icon btn-icon--danger" @click="deleteCharge(charge.id)">🗑</button>
              </div>
            </div>

            <!-- Tabella ripartizione per unità -->
            <div v-if="charge.items?.length" class="charge-items">
              <table class="items-table">
                <thead>
                  <tr>
                    <th>Unità</th>
                    <th class="col-num">Consumo</th>
                    <th class="col-num">% sul totale</th>
                    <th class="col-num">Importo</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="item in charge.items" :key="item.id" :class="{ 'row-warning': item.hasWarning }">
                    <td>{{ item.unitName }}</td>
                    <td class="col-num mono">{{ fmtNum(item.consumption) }}</td>
                    <td class="col-num">{{ fmtPct(item.percentage) }}</td>
                    <td class="col-num" :class="item.amount > 0 ? '' : 'text-muted'">{{ fmt(item.amount) }}</td>
                    <td>
                      <span v-if="item.hasWarning" class="warn-icon" title="Nessuna lettura per questa unità">⚠</span>
                    </td>
                  </tr>
                </tbody>
                <tfoot>
                  <tr class="totals-row">
                    <td>Totale</td>
                    <td class="col-num mono">{{ fmtNum(charge.items.reduce((s,i) => s + i.consumption, 0)) }}</td>
                    <td class="col-num">{{ fmtPct(charge.items.reduce((s,i) => s + i.percentage, 0)) }}</td>
                    <td class="col-num">{{ fmt(charge.items.reduce((s,i) => s + i.amount, 0)) }}</td>
                    <td></td>
                  </tr>
                </tfoot>
              </table>
            </div>
          </div>
        </template>
      </div>
    </template>

    <!-- ══ MODAL: Tipo consumo ════════════════════════════════════════════ -->
    <BaseModal :show="showTypeModal" @close="showTypeModal = false"
               :title="editingTypeId ? 'Modifica tipo consumo' : 'Nuovo tipo consumo'"
               size="sm">
      <div class="form-grid">
        <div class="form-group" :class="{ 'has-error': typeErrors.name }">
          <label class="form-label">Nome *</label>
          <input class="form-input" v-model="typeForm.name" @input="delete typeErrors.name" />
          <span v-if="typeErrors.name" class="field-error">{{ typeErrors.name }}</span>
        </div>
        <div class="form-group" :class="{ 'has-error': typeErrors.unitOfMeasure }">
          <label class="form-label">Unità di misura *</label>
          <input class="form-input" v-model="typeForm.unitOfMeasure" placeholder="mc, kWh, litri…" @input="delete typeErrors.unitOfMeasure" />
          <span v-if="typeErrors.unitOfMeasure" class="field-error">{{ typeErrors.unitOfMeasure }}</span>
        </div>
        <div v-if="editingTypeId" class="form-group form-group--full">
          <label class="form-label">
            <input type="checkbox" v-model="typeForm.isActive" style="margin-right:6px" />
            Attivo
          </label>
        </div>
        <div class="form-group form-group--full">
          <label class="form-label">Note</label>
          <textarea class="form-textarea" v-model="typeForm.notes" rows="2"></textarea>
        </div>
      </div>
      <template #footer>
        <button class="btn btn-ghost" @click="showTypeModal = false">Annulla</button>
        <button class="btn btn-primary" @click="saveType" :disabled="savingType">
          <span v-if="savingType" class="spinner" style="width:14px;height:14px"></span>
          {{ editingTypeId ? 'Salva' : 'Crea' }}
        </button>
      </template>
    </BaseModal>

    <!-- ══ MODAL: Contatore ═══════════════════════════════════════════════ -->
    <BaseModal :show="showMeterModal" @close="showMeterModal = false"
               :title="editingMeterId ? 'Modifica contatore' : 'Nuovo contatore'"
               size="sm">
      <div class="form-grid">
        <div v-if="!editingMeterId" class="form-group" :class="{ 'has-error': meterErrors.consumptionTypeId }">
          <label class="form-label">Tipo consumo *</label>
          <select class="form-select" v-model="meterForm.consumptionTypeId" @change="delete meterErrors.consumptionTypeId">
            <option :value="null" disabled>— Seleziona —</option>
            <option v-for="t in consumptionTypes" :key="t.id" :value="t.id">{{ t.name }}</option>
          </select>
          <span v-if="meterErrors.consumptionTypeId" class="field-error">{{ meterErrors.consumptionTypeId }}</span>
        </div>
        <div v-if="!editingMeterId" class="form-group" :class="{ 'has-error': meterErrors.unitId }">
          <label class="form-label">Unità immobiliare *</label>
          <select class="form-select" v-model="meterForm.unitId" @change="delete meterErrors.unitId">
            <option :value="null" disabled>— Seleziona —</option>
            <option v-for="u in units" :key="u.id" :value="u.id">{{ u.internalNumber }}{{ u.displayName ? ' — ' + u.displayName : '' }}</option>
          </select>
          <span v-if="meterErrors.unitId" class="field-error">{{ meterErrors.unitId }}</span>
        </div>
        <div class="form-group form-group--full" :class="{ 'has-error': meterErrors.code }">
          <label class="form-label">Matricola contatore *</label>
          <input class="form-input" v-model="meterForm.code" @input="delete meterErrors.code" />
          <span v-if="meterErrors.code" class="field-error">{{ meterErrors.code }}</span>
        </div>
        <div v-if="editingMeterId" class="form-group form-group--full">
          <label class="form-label">
            <input type="checkbox" v-model="meterForm.isActive" style="margin-right:6px" />
            Attivo
          </label>
        </div>
        <div class="form-group form-group--full">
          <label class="form-label">Note</label>
          <textarea class="form-textarea" v-model="meterForm.notes" rows="2"></textarea>
        </div>
      </div>
      <template #footer>
        <button class="btn btn-ghost" @click="showMeterModal = false">Annulla</button>
        <button class="btn btn-primary" @click="saveMeter" :disabled="savingMeter">
          <span v-if="savingMeter" class="spinner" style="width:14px;height:14px"></span>
          {{ editingMeterId ? 'Salva' : 'Crea' }}
        </button>
      </template>
    </BaseModal>

    <!-- ══ MODAL: Ripartizione ════════════════════════════════════════════ -->
    <BaseModal :show="showChargeModal" @close="showChargeModal = false"
               :title="editingChargeId ? 'Modifica ripartizione' : 'Nuova ripartizione'"
               size="sm">
      <div class="form-grid">
        <div v-if="!editingChargeId" class="form-group form-group--full" :class="{ 'has-error': chargeErrors.consumptionTypeId }">
          <label class="form-label">Tipo consumo *</label>
          <select class="form-select" v-model="chargeForm.consumptionTypeId" @change="delete chargeErrors.consumptionTypeId">
            <option :value="null" disabled>— Seleziona —</option>
            <option v-for="t in consumptionTypes" :key="t.id" :value="t.id">{{ t.name }}</option>
          </select>
          <span v-if="chargeErrors.consumptionTypeId" class="field-error">{{ chargeErrors.consumptionTypeId }}</span>
        </div>
        <div class="form-group form-group--full" :class="{ 'has-error': chargeErrors.budgetId }">
          <label class="form-label">Budget preventivo *</label>
          <select class="form-select" v-model="chargeForm.budgetId" @change="delete chargeErrors.budgetId">
            <option :value="null" disabled>— Seleziona —</option>
            <option v-for="b in preventiviForCharge" :key="b.id" :value="b.id">{{ b.code }}{{ b.description ? ' – ' + b.description : '' }}</option>
          </select>
          <span v-if="chargeErrors.budgetId" class="field-error">{{ chargeErrors.budgetId }}</span>
        </div>
        <div class="form-group form-group--full" :class="{ 'has-error': chargeErrors.totalAmount }">
          <label class="form-label">Importo totale spesa (€) *</label>
          <input class="form-input" type="number" step="0.01" v-model.number="chargeForm.totalAmount" @input="delete chargeErrors.totalAmount" />
          <span v-if="chargeErrors.totalAmount" class="field-error">{{ chargeErrors.totalAmount }}</span>
        </div>
        <div class="form-group form-group--full">
          <label class="form-label">Note</label>
          <textarea class="form-textarea" v-model="chargeForm.notes" rows="2"></textarea>
        </div>
      </div>
      <template #footer>
        <button class="btn btn-ghost" @click="showChargeModal = false">Annulla</button>
        <button class="btn btn-primary" @click="saveCharge" :disabled="savingCharge">
          <span v-if="savingCharge" class="spinner" style="width:14px;height:14px"></span>
          {{ editingChargeId ? 'Salva' : 'Crea e calcola' }}
        </button>
      </template>
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useAppStore } from '@/stores/app'
import { usePermissions } from '@/composables/usePermissions'
import {
  consumptionTypeApi, meterApi, consumptionReadingApi, consumptionChargeApi,
  fiscalYearApi, unitApi, budgetApi,
} from '@/services/api'
import BaseModal from '@/components/BaseModal.vue'

const store = useAppStore()
const { canCreate, canEdit, canDelete } = usePermissions()

// ── Tab ────────────────────────────────────────────────────────────────────
const tab = ref('types')

// ── Tipi consumo ──────────────────────────────────────────────────────────
const consumptionTypes = ref([])
const loadingTypes     = ref(false)

async function loadTypes() {
  if (!store.selectedCondominioId) { consumptionTypes.value = []; return }
  loadingTypes.value = true
  try {
    const { data } = await consumptionTypeApi.getByCondominium(store.selectedCondominioId)
    consumptionTypes.value = data ?? []
  } catch { consumptionTypes.value = [] } finally { loadingTypes.value = false }
}

// ── Contatori ─────────────────────────────────────────────────────────────
const meters        = ref([])
const loadingMeters = ref(false)

async function loadMeters() {
  if (!store.selectedCondominioId) { meters.value = []; return }
  loadingMeters.value = true
  try {
    const { data } = await meterApi.getByCondominium(store.selectedCondominioId)
    meters.value = data ?? []
  } catch { meters.value = [] } finally { loadingMeters.value = false }
}

// ── Unità (per modal contatore) ───────────────────────────────────────────
const units = ref([])
async function loadUnits() {
  if (!store.selectedCondominioId) return
  try {
    const { data } = await unitApi.getByCondominium(store.selectedCondominioId)
    units.value = data ?? []
  } catch { units.value = [] }
}

// ── Esercizi fiscali (per letture e ripartizioni) ─────────────────────────
const fiscalYears = ref([])
async function loadFiscalYears() {
  if (!store.selectedCondominioId) return
  try {
    const { data } = await fiscalYearApi.getByCondominium(store.selectedCondominioId)
    fiscalYears.value = data ?? []
  } catch { fiscalYears.value = [] }
}

// ── Letture ───────────────────────────────────────────────────────────────
const selectedTypeId      = ref(null)
const selectedFiscalYearId = ref(null)
const readingRows         = ref([])
const loadingReadings     = ref(false)
const savingReadings      = ref(false)
const readingsModified    = ref(false)

const selectedTypeName = computed(() =>
  consumptionTypes.value.find(t => t.id === selectedTypeId.value)?.name ?? '')

async function onTypeOrFyChange() {
  if (!selectedTypeId.value || !selectedFiscalYearId.value) return
  readingsModified.value = false
  loadingReadings.value  = true
  try {
    // Carica contatori del tipo selezionato
    const [metersRes, readingsRes] = await Promise.all([
      meterApi.getByType(selectedTypeId.value),
      consumptionReadingApi.getByTypeFiscalYear(selectedTypeId.value, selectedFiscalYearId.value),
    ])
    const typeMeters    = metersRes.data ?? []
    const savedReadings = readingsRes.data ?? []
    const readingByMeterId = Object.fromEntries(savedReadings.map(r => [r.meterId, r]))

    readingRows.value = typeMeters
      .filter(m => m.isActive)
      .sort((a, b) => (a.unitName ?? '').localeCompare(b.unitName ?? '', 'it', { sensitivity: 'base' }))
      .map(m => {
        const saved = readingByMeterId[m.id]
        return {
          meterId:      m.id,
          unitName:     m.unitName,
          meterCode:    m.code,
          initialValue: saved?.initialValue ?? 0,
          initialDate:  saved?.initialDate?.slice(0, 10) ?? '',
          finalValue:   saved?.finalValue ?? 0,
          finalDate:    saved?.finalDate?.slice(0, 10) ?? '',
          hasWarning:   false,
        }
      })
  } catch { readingRows.value = [] } finally { loadingReadings.value = false }
}

async function saveReadings() {
  if (!selectedTypeId.value || !selectedFiscalYearId.value) return
  savingReadings.value = true
  try {
    await consumptionReadingApi.saveBulk({
      consumptionTypeId: selectedTypeId.value,
      fiscalYearId:      selectedFiscalYearId.value,
      items: readingRows.value.map(r => ({
        meterId:      r.meterId,
        initialValue: r.initialValue,
        initialDate:  r.initialDate || null,
        finalValue:   r.finalValue,
        finalDate:    r.finalDate || null,
      })),
    })
    store.toast('Letture salvate', 'success')
    readingsModified.value = false
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { savingReadings.value = false }
}

function consumptionClass(row) {
  const c = row.finalValue - row.initialValue
  if (c < 0) return 'text-red'
  if (c > 0) return 'text-green'
  return 'text-muted'
}

function selectType(t) {
  selectedTypeId.value = t.id
  tab.value = 'readings'
}

// ── Ripartizioni ──────────────────────────────────────────────────────────
const charges             = ref([])
const loadingCharges      = ref(false)
const chargesFiscalYearId = ref(null)
const preventiviForCharge = ref([])

async function loadCharges() {
  if (!chargesFiscalYearId.value) return
  loadingCharges.value = true
  try {
    const [chargesRes, budgetsRes] = await Promise.all([
      consumptionChargeApi.getByFiscalYear(chargesFiscalYearId.value),
      budgetApi.getByCondominium(store.selectedCondominioId),
    ])
    charges.value = chargesRes.data ?? []
    preventiviForCharge.value = (budgetsRes.data ?? []).filter(b => b.typeId === 1)
  } catch { charges.value = [] } finally { loadingCharges.value = false }
}

async function recalculate(id) {
  try {
    const { data } = await consumptionChargeApi.recalculate(id)
    const idx = charges.value.findIndex(c => c.id === id)
    if (idx !== -1) charges.value[idx] = data
    store.toast('Ripartizione ricalcolata', 'success')
  } catch (err) { if (!err?.response) store.toast('Errore di rete', 'error') }
}

async function approveCharge(id) {
  if (!confirm('Approvare la ripartizione? Verranno generate le rate per ogni unità.')) return
  try {
    const { data } = await consumptionChargeApi.approve(id)
    const idx = charges.value.findIndex(c => c.id === id)
    if (idx !== -1) charges.value[idx] = data
    store.toast('Ripartizione approvata — rate generate', 'success')
  } catch (err) { if (!err?.response) store.toast('Errore di rete', 'error') }
}

async function deleteCharge(id) {
  if (!confirm('Eliminare questa ripartizione?')) return
  try {
    await consumptionChargeApi.delete(id)
    charges.value = charges.value.filter(c => c.id !== id)
    store.toast('Ripartizione eliminata', 'success')
  } catch (err) { if (!err?.response) store.toast('Errore di rete', 'error') }
}

// ── Modal tipo consumo ────────────────────────────────────────────────────
const showTypeModal  = ref(false)
const editingTypeId  = ref(null)
const savingType     = ref(false)
const typeErrors     = ref({})
const typeForm       = ref({ name: '', unitOfMeasure: '', notes: '', isActive: true })

function openTypeModal(t = null) {
  editingTypeId.value = t?.id ?? null
  typeForm.value = t
    ? { name: t.name, unitOfMeasure: t.unitOfMeasure, notes: t.notes ?? '', isActive: t.isActive }
    : { name: '', unitOfMeasure: '', notes: '', isActive: true }
  typeErrors.value = {}
  showTypeModal.value = true
}

async function saveType() {
  typeErrors.value = {}
  if (!typeForm.value.name?.trim()) typeErrors.value.name = 'Obbligatorio'
  if (!typeForm.value.unitOfMeasure?.trim()) typeErrors.value.unitOfMeasure = 'Obbligatorio'
  if (Object.keys(typeErrors.value).length) return
  savingType.value = true
  try {
    if (editingTypeId.value) {
      await consumptionTypeApi.update(editingTypeId.value, typeForm.value)
    } else {
      await consumptionTypeApi.create({ ...typeForm.value, condominiumId: store.selectedCondominioId })
    }
    store.toast(editingTypeId.value ? 'Tipo aggiornato' : 'Tipo creato', 'success')
    showTypeModal.value = false
    await loadTypes()
  } catch (err) { if (!err?.response) store.toast('Errore di rete', 'error') } finally { savingType.value = false }
}

async function deleteType(id) {
  if (!confirm('Eliminare questo tipo di consumo?')) return
  try {
    await consumptionTypeApi.delete(id)
    store.toast('Tipo eliminato', 'success')
    await loadTypes()
  } catch (err) { if (!err?.response) store.toast('Errore di rete', 'error') }
}

// ── Modal contatore ───────────────────────────────────────────────────────
const showMeterModal = ref(false)
const editingMeterId = ref(null)
const savingMeter    = ref(false)
const meterErrors    = ref({})
const meterForm      = ref({ consumptionTypeId: null, unitId: null, code: '', notes: '', isActive: true })

function openMeterModal(m = null) {
  editingMeterId.value = m?.id ?? null
  meterForm.value = m
    ? { consumptionTypeId: m.consumptionTypeId, unitId: m.unitId, code: m.code, notes: m.notes ?? '', isActive: m.isActive }
    : { consumptionTypeId: null, unitId: null, code: '', notes: '', isActive: true }
  meterErrors.value = {}
  showMeterModal.value = true
}

async function saveMeter() {
  meterErrors.value = {}
  if (!editingMeterId.value && !meterForm.value.consumptionTypeId) meterErrors.value.consumptionTypeId = 'Obbligatorio'
  if (!editingMeterId.value && !meterForm.value.unitId) meterErrors.value.unitId = 'Obbligatorio'
  if (!meterForm.value.code?.trim()) meterErrors.value.code = 'Obbligatorio'
  if (Object.keys(meterErrors.value).length) return
  savingMeter.value = true
  try {
    if (editingMeterId.value) {
      await meterApi.update(editingMeterId.value, { code: meterForm.value.code, notes: meterForm.value.notes, isActive: meterForm.value.isActive })
    } else {
      await meterApi.create(meterForm.value)
    }
    store.toast(editingMeterId.value ? 'Contatore aggiornato' : 'Contatore creato', 'success')
    showMeterModal.value = false
    await loadMeters()
  } catch (err) { if (!err?.response) store.toast('Errore di rete', 'error') } finally { savingMeter.value = false }
}

async function deleteMeter(id) {
  if (!confirm('Eliminare questo contatore?')) return
  try {
    await meterApi.delete(id)
    store.toast('Contatore eliminato', 'success')
    await loadMeters()
  } catch (err) { if (!err?.response) store.toast('Errore di rete', 'error') }
}

// ── Modal ripartizione ────────────────────────────────────────────────────
const showChargeModal  = ref(false)
const editingChargeId  = ref(null)
const savingCharge     = ref(false)
const chargeErrors     = ref({})
const chargeForm       = ref({ consumptionTypeId: null, budgetId: null, totalAmount: 0, notes: '' })

function openChargeModal(c = null) {
  editingChargeId.value = c?.id ?? null
  chargeForm.value = c
    ? { consumptionTypeId: c.consumptionTypeId, budgetId: c.budgetId, totalAmount: c.totalAmount, notes: c.notes ?? '' }
    : { consumptionTypeId: null, budgetId: null, totalAmount: 0, notes: '' }
  chargeErrors.value = {}
  showChargeModal.value = true
}

async function saveCharge() {
  chargeErrors.value = {}
  if (!editingChargeId.value && !chargeForm.value.consumptionTypeId) chargeErrors.value.consumptionTypeId = 'Obbligatorio'
  if (!chargeForm.value.budgetId) chargeErrors.value.budgetId = 'Obbligatorio'
  if (!chargeForm.value.totalAmount || chargeForm.value.totalAmount <= 0) chargeErrors.value.totalAmount = 'Deve essere maggiore di zero'
  if (Object.keys(chargeErrors.value).length) return
  savingCharge.value = true
  try {
    if (editingChargeId.value) {
      const { data } = await consumptionChargeApi.update(editingChargeId.value, {
        budgetId: chargeForm.value.budgetId,
        totalAmount: chargeForm.value.totalAmount,
        notes: chargeForm.value.notes,
      })
      const idx = charges.value.findIndex(c => c.id === editingChargeId.value)
      if (idx !== -1) charges.value[idx] = data
    } else {
      await consumptionChargeApi.create({
        consumptionTypeId: chargeForm.value.consumptionTypeId,
        fiscalYearId:      chargesFiscalYearId.value,
        budgetId:          chargeForm.value.budgetId,
        totalAmount:       chargeForm.value.totalAmount,
        notes:             chargeForm.value.notes,
      })
      await loadCharges()
    }
    store.toast(editingChargeId.value ? 'Ripartizione aggiornata' : 'Ripartizione creata', 'success')
    showChargeModal.value = false
  } catch (err) { if (!err?.response) store.toast('Errore di rete', 'error') } finally { savingCharge.value = false }
}

// ── Formatters ─────────────────────────────────────────────────────────────
const fmt    = (v) => v != null ? '€ ' + Number(v).toLocaleString('it-IT', { minimumFractionDigits: 2 }) : '—'
const fmtNum = (v) => v != null ? Number(v).toLocaleString('it-IT', { minimumFractionDigits: 3, maximumFractionDigits: 3 }) : '—'
const fmtPct = (v) => v != null ? (Number(v) * 100).toLocaleString('it-IT', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' %' : '—'

// ── Init ──────────────────────────────────────────────────────────────────
async function loadAll() {
  await Promise.all([loadTypes(), loadMeters(), loadUnits(), loadFiscalYears()])
}

onMounted(loadAll)
watch(() => store.selectedCondominioId, () => {
  selectedTypeId.value       = null
  selectedFiscalYearId.value = null
  chargesFiscalYearId.value  = null
  readingRows.value          = []
  charges.value              = []
  loadAll()
})
</script>

<style scoped>
.tab-bar { display: flex; gap: 0.25rem; margin-bottom: 1rem; border-bottom: 1px solid var(--border); }
.tab-btn {
  padding: 0.5rem 1.25rem;
  border: none; background: none; cursor: pointer;
  color: var(--text-secondary); font-size: 0.9rem;
  border-bottom: 2px solid transparent; margin-bottom: -1px;
  transition: color 0.15s, border-color 0.15s;
}
.tab-btn:hover:not(:disabled) { color: var(--text); }
.tab-btn.active { color: var(--accent); border-bottom-color: var(--accent); font-weight: 600; }
.tab-btn:disabled { opacity: 0.4; cursor: not-allowed; }

.readings-filter { margin-bottom: 0.75rem; padding: 1rem; }
.readings-table .col-num  { width: 110px; text-align: right; }
.readings-table .col-date { width: 130px; }
.readings-table .inline-input { width: 100%; }

.row-warning td { background: color-mix(in srgb, var(--accent-amber, #f59e0b) 8%, transparent); }
.warn-icon { color: var(--accent-amber, #f59e0b); cursor: help; }

.charge-card {
  border: 1px solid var(--border);
  border-radius: 8px;
  margin-bottom: 1rem;
  overflow: hidden;
}
.charge-header {
  display: flex; align-items: center; gap: 1rem;
  padding: 0.75rem 1rem;
  background: var(--bg-surface);
  border-bottom: 1px solid var(--border);
}
.charge-title { display: flex; align-items: center; gap: 0.5rem; flex: 1; flex-wrap: wrap; }
.charge-type  { font-weight: 600; }
.charge-amount { font-weight: 600; font-size: 1.05rem; white-space: nowrap; }
.charge-items { padding: 0.75rem 1rem; }

.items-table { width: 100%; border-collapse: collapse; font-size: 0.875rem; }
.items-table th, .items-table td { padding: 0.35rem 0.5rem; }
.items-table thead th { color: var(--text-secondary); font-weight: 500; border-bottom: 1px solid var(--border); }
.items-table tbody tr:hover td { background: var(--bg-surface); }
.items-table tfoot .totals-row td { border-top: 1px solid var(--border); font-weight: 600; }
.items-table .col-num { text-align: right; }

.link-btn { background: none; border: none; color: var(--accent); cursor: pointer; padding: 0; text-decoration: underline; font-size: inherit; }
</style>
