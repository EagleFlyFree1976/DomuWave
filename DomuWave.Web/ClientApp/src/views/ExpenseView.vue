<template>
  <div>
    <!-- ── Header ──────────────────────────────────────── -->
    <div class="page-header">
      <h1>Spese</h1>
      <button v-if="canCreate" class="btn btn-primary" @click="openExpenseModal()">+ Nuova spesa</button>
    </div>


    <!-- ── Toolbar filtri ──────────────────────────────── -->
    <div class="tab-toolbar">
      <!-- Esercizio fiscale -->
      <select class="form-select" v-model.number="selectedFiscalYearId" style="min-width:180px"
              :disabled="!fiscalYears.length">
        <option :value="null">Tutti gli esercizi</option>
        <option v-for="fy in fiscalYears" :key="fy.id" :value="fy.id">
          {{ fy.code }}{{ fy.description ? ' – ' + fy.description : '' }}
        </option>
      </select>

      <!-- Ricerca fornitore -->
      <input class="form-input" v-model="supplierSearch" placeholder="Cerca fornitore…"
             style="min-width:180px" @input="onSupplierSearch" />

      <!-- Tipo spesa -->
      <select class="form-select" v-model.number="expTypeFilter" style="width:160px" @change="loadExpenses">
        <option :value="0">Tutti i tipi</option>
        <option :value="1">Manutenzione</option>
        <option :value="2">Pulizie</option>
        <option :value="3">Sicurezza</option>
        <option :value="4">Utenze</option>
        <option :value="5">Professionale</option>
        <option :value="6">Altro</option>
      </select>

      <!-- Stato pagamento -->
      <select class="form-select" v-model="expFilter" style="width:140px" @change="loadExpenses">
        <option value="">Tutte</option>
        <option value="unpaid">Non pagate</option>
      </select>
    </div>

    <!-- ── Tabella ──────────────────────────────────────── -->
    <div class="card">
      <div v-if="loadingExp" class="loading-state"><div class="spinner"></div></div>
      <div v-else-if="!filteredExpenses.length" class="empty-state">
        <div class="empty-icon">◎</div><div>Nessuna spesa trovata</div>
      </div>
      <div v-else class="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Data</th>
              <th>Descrizione</th>
              <th>Fornitore</th>
              <th>Esercizio</th>
              <th class="text-right">Importo</th>
              <th class="text-right">IVA</th>
              <th>Pagamento</th>
              <th>Stato</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="e in filteredExpenses" :key="e.id">
              <td class="mono text-secondary">{{ fmtDate(e.documentDate) }}</td>
              <td>{{ e.name }}</td>
              <td>{{ e.supplierName || '—' }}</td>
              <td class="text-secondary">{{ e.fiscalYearCode || '—' }}</td>
              <td class="mono text-right">{{ fmt(e.grossAmount) }}</td>
              <td class="mono text-right text-secondary">{{ fmt(e.vatAmount) }}</td>
              <td class="text-secondary">{{ e.paymentMethodName || '—' }}</td>
              <td><span class="badge" :class="payBadge(e.paymentStatusId)">{{ e.paymentStatusName }}</span></td>
              <td>
                <div class="row-actions">
                  <button v-if="canEdit && e.paymentStatusId !== 2" class="btn btn-sm btn-ghost"
                          :disabled="actionInProgress[`pay-${e.id}`]"
                          @click="markPaid(e.id)">
                    <span v-if="actionInProgress[`pay-${e.id}`]" class="spinner" style="width:12px;height:12px"></span>
                    <span v-else>Paga</span>
                  </button>
                  <button v-if="canEdit && e.paymentStatusId === 2" class="btn btn-sm btn-ghost"
                          :disabled="actionInProgress[`pay-${e.id}`]"
                          @click="markUnpaid(e.id)">
                    <span v-if="actionInProgress[`pay-${e.id}`]" class="spinner" style="width:12px;height:12px"></span>
                    <span v-else>Segna non pagata</span>
                  </button>
                  <button v-if="canEdit" class="btn-icon"
                          :disabled="actionInProgress[`pay-${e.id}`] || actionInProgress[`del-${e.id}`]"
                          @click="openExpenseModal(e)">✎</button>
                  <button v-if="canDelete" class="btn-icon" style="color:var(--accent-red)"
                          :disabled="actionInProgress[`del-${e.id}`]"
                          @click="deleteExpense(e.id)">
                    <span v-if="actionInProgress[`del-${e.id}`]" class="spinner" style="width:12px;height:12px"></span>
                    <span v-else>✕</span>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- ══════════════════════════════════════════════════
         MODAL — Crea / Modifica Spesa
    ══════════════════════════════════════════════════ -->
    <BaseModal
      :show="showExpenseModal"
      @close="showExpenseModal = false"
      :title="editingExp ? 'Modifica spesa' : 'Nuova spesa'"
      :subtitle="store.selectedCondominio?.name"
      size="lg"
    >
      <!-- ── Blocco: nessun esercizio fiscale disponibile ─────────── -->
      <div v-if="noFiscalYearAvailable && !editingExp" class="no-fiscal-year-banner">
        <div class="no-fiscal-year-icon">📅</div>
        <div>
          <strong>Nessun esercizio fiscale disponibile</strong>
          <div class="no-fiscal-year-desc">
            Per inserire una spesa è necessario avere almeno un esercizio fiscale in stato
            <strong>Aperto</strong> o <strong>In chiusura</strong>.<br/>
            Vai alla sezione <em>Esercizi fiscali</em> e apri un esercizio prima di procedere.
          </div>
        </div>
      </div>

      <template v-if="!noFiscalYearAvailable || editingExp">
      <!-- ── Esercizio fiscale ──────────────────────────────────── -->
      <div class="form-group form-group--full" :class="{ 'has-error': expErrors.fiscalYearId }">
        <label class="form-label">Esercizio fiscale *</label>
        <select class="form-select" v-model.number="expForm.fiscalYearId"
                @change="clearExpError('fiscalYearId'); checkRegistrationDateWarning()">
          <option :value="null" disabled>Seleziona esercizio…</option>
          <option v-for="fy in selectableFiscalYears" :key="fy.id" :value="fy.id">
            {{ fy.code }}{{ fy.description ? ' – ' + fy.description : '' }}
            ({{ fy.startDate?.slice(0,10) }} / {{ fy.endDate?.slice(0,10) }})
            {{ fy.statusId === 3 ? '· In chiusura' : '' }}
          </option>
        </select>
        <span v-if="expErrors.fiscalYearId" class="field-error">{{ expErrors.fiscalYearId }}</span>
      </div>

      <!-- ── Warning data fuori periodo (solo Closing) ─────────── -->
      <div v-if="registrationDateWarning" class="warning-banner">
        ⚠ {{ registrationDateWarning }}
      </div>

      <div class="form-grid">
        <div class="form-group" style="grid-column: span 2" :class="{ 'has-error': expErrors.name }">
          <label class="form-label">Descrizione *</label>
          <input class="form-input" v-model="expForm.name" placeholder="Descrizione spesa…" @input="clearExpError('name')" />
          <span v-if="expErrors.name" class="field-error">{{ expErrors.name }}</span>
        </div>
        <div class="form-group">
          <label class="form-label">N° documento</label>
          <input class="form-input" v-model="expForm.documentNumber" />
        </div>
        <div class="form-group" :class="{ 'has-error': expErrors.expenseTypeId }">
          <label class="form-label">Tipo spesa *</label>
          <select class="form-select" v-model.number="expForm.expenseTypeId" @change="clearExpError('expenseTypeId')">
            <option :value="0" disabled>Seleziona tipo…</option>
            <option :value="1">Manutenzione</option>
            <option :value="2">Pulizie</option>
            <option :value="3">Sicurezza</option>
            <option :value="4">Utenze</option>
            <option :value="5">Professionale</option>
            <option :value="6">Altro</option>
          </select>
          <span v-if="expErrors.expenseTypeId" class="field-error">{{ expErrors.expenseTypeId }}</span>
        </div>
        <div class="form-group" :class="{ 'has-error': expErrors.documentDate }">
          <label class="form-label">Data documento *</label>
          <input class="form-input" type="date" v-model="expForm.documentDate" @input="clearExpError('documentDate')" />
          <span v-if="expErrors.documentDate" class="field-error">{{ expErrors.documentDate }}</span>
        </div>
        <div class="form-group" :class="{ 'has-error': expErrors.registrationDate }">
          <label class="form-label">Data registrazione *</label>
          <input class="form-input" type="date" v-model="expForm.registrationDate" @input="clearExpError('registrationDate')" />
          <span v-if="expErrors.registrationDate" class="field-error">{{ expErrors.registrationDate }}</span>
        </div>
        <!-- Errore base imponibile -->
        <div v-if="expErrors.taxableBase" class="field-error" style="grid-column:span 2;margin-bottom:0.25rem">
          ⚠ {{ expErrors.taxableBase }}
        </div>
        <!-- Imponibile / Imponibile esente IVA — almeno uno obbligatorio -->
        <div class="form-group" :class="{ 'has-error': expErrors.taxableAmount }">
          <label class="form-label">
            Imponibile (€)
            <span class="label-hint">obbligatorio se non esente IVA</span>
          </label>
          <input class="form-input" type="number" step="0.01" min="0"
                 v-model.number="expForm.taxableAmount"
                 @input="clearExpError('taxableAmount'); clearExpError('taxableBase'); autoCalcGross()" />
          <span v-if="expErrors.taxableAmount" class="field-error">{{ expErrors.taxableAmount }}</span>
        </div>
        <div class="form-group" :class="{ 'has-error': expErrors.taxableAmountVatExempt }">
          <label class="form-label">
            Imponibile esente IVA (€)
            <span class="label-hint">obbligatorio se non imponibile</span>
          </label>
          <input class="form-input" type="number" step="0.01" min="0"
                 v-model.number="expForm.taxableAmountVatExempt"
                 @input="clearExpError('taxableAmountVatExempt'); clearExpError('taxableBase'); autoCalcGross()" />
          <span v-if="expErrors.taxableAmountVatExempt" class="field-error">{{ expErrors.taxableAmountVatExempt }}</span>
        </div>
        <div class="form-group" :class="{ 'has-error': expErrors.vatAmount }">
          <label class="form-label">
            IVA / Imposta (€)
            <span v-if="expForm.taxableAmount > 0" class="label-required">*</span>
          </label>
          <input class="form-input" type="number" step="0.01" min="0"
                 v-model.number="expForm.vatAmount"
                 @input="clearExpError('vatAmount'); autoCalcGross()" />
          <span v-if="expErrors.vatAmount" class="field-error">{{ expErrors.vatAmount }}</span>
        </div>
        <div class="form-group">
          <label class="form-label">Cassa previdenziale (€)</label>
          <input class="form-input" type="number" step="0.01" min="0"
                 v-model.number="expForm.pensionFund"
                 @input="autoCalcGross()" />
        </div>
        <div class="form-group">
          <label class="form-label">Ritenuta d'acconto (€)</label>
          <input class="form-input" type="number" step="0.01" min="0"
                 v-model.number="expForm.withholdingTax"
                 @input="autoCalcGross()" />
        </div>
        <div class="form-group">
          <label class="form-label">Bollo (€)</label>
          <input class="form-input" type="number" step="0.01" min="0"
                 v-model.number="expForm.stampDuty"
                 @input="autoCalcGross()" />
        </div>
        <div class="form-group" style="grid-column: span 2">
          <label class="form-label">Importo lordo (€) <span class="label-hint">calcolato automaticamente</span></label>
          <input class="form-input" type="number" step="0.01"
                 v-model.number="expForm.grossAmount" readonly
                 style="background:var(--bg-base);cursor:default" />
        </div>
        <div class="form-group" style="grid-column: span 2" :class="{ 'has-error': expErrors.accountId }">
          <label class="form-label">Conto *</label>
          <select class="form-select" v-model.number="expForm.accountId" @change="clearExpError('accountId')">
            <option :value="null" disabled>Seleziona conto…</option>
            <optgroup v-for="grp in expenseAccountGroups" :key="grp.typeLabel" :label="grp.typeLabel">
              <option v-for="r in grp.rows" :key="r.accountId" :value="r.accountId">{{ r.label }}</option>
            </optgroup>
          </select>
          <span v-if="expErrors.accountId" class="field-error">{{ expErrors.accountId }}</span>
        </div>
        <div class="form-group" :class="{ 'has-error': expErrors.millesimalTableId }">
          <label class="form-label">Tabella millesimale *</label>
          <select class="form-select" v-model.number="expForm.millesimalTableId" @change="clearExpError('millesimalTableId')">
            <option :value="null" disabled>Seleziona tabella…</option>
            <option v-for="t in enabledMillesimalTables" :key="t.id" :value="t.id">
              {{ t.code }}{{ t.name ? ' – ' + t.name : '' }}{{ !t.isEnabled ? ' (disabilitata)' : '' }}
            </option>
          </select>
          <span v-if="expErrors.millesimalTableId" class="field-error">{{ expErrors.millesimalTableId }}</span>
        </div>
        <div class="form-group">
          <label class="form-label">Fornitore</label>
          <select class="form-select" v-model.number="expForm.supplierId">
            <option :value="null">—</option>
            <option v-for="s in suppliers" :key="s.id" :value="s.id">
              {{ s.name || s.companyName }}
            </option>
          </select>
        </div>
        <div class="form-group">
          <label class="form-label">Metodo pagamento</label>
          <select class="form-select" v-model.number="expForm.paymentMethodId">
            <option :value="null">—</option>
            <option v-for="m in paymentMethods" :key="m.id" :value="m.id">{{ m.name }}</option>
          </select>
        </div>
      </div>
      <div class="form-group">
        <label class="form-label">Note</label>
        <textarea class="form-textarea" v-model="expForm.description" rows="2"></textarea>
      </div>
      <div class="form-group" style="margin-top:0.5rem">
        <label class="form-label">A carico di</label>
        <div class="radio-group">
          <label class="radio-label">
            <input type="radio" v-model.number="expForm.chargeabilityTypeId" :value="1" />
            Proprietario
          </label>
          <label class="radio-label">
            <input type="radio" v-model.number="expForm.chargeabilityTypeId" :value="2" />
            Inquilino (se presente)
          </label>
          <label class="radio-label">
            <input type="radio" v-model.number="expForm.chargeabilityTypeId" :value="3" />
            Automatico
          </label>
        </div>
      </div>
      </template><!-- fine v-if !noFiscalYearAvailable -->

      <template #footer>
        <button class="btn btn-ghost" @click="showExpenseModal = false">Annulla</button>
        <button class="btn btn-primary" @click="saveExpense"
                :disabled="savingExp || (noFiscalYearAvailable && !editingExp)">
          <span v-if="savingExp" class="spinner" style="width:14px;height:14px"></span>
          {{ editingExp ? 'Salva' : 'Crea' }}
        </button>
      </template>
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useAppStore } from '@/stores/app'
import { expenseApi, chartOfAccountsApi, supplierApi, millesimalTableApi } from '@/services/api'
import { usePermissions } from '@/composables/usePermissions'
import BaseModal from '@/components/BaseModal.vue'

const store = useAppStore()
const { canCreate, canEdit, canDelete } = usePermissions()

// ─── Fiscal Years ─────────────────────────────────────────────
const fiscalYears          = computed(() => store.fiscalYears)
const selectedFiscalYearId = ref(store.selectedFiscalYearId ?? null)

// Esercizi selezionabili nella form: solo Open (2) e Closing (3) — la Bozza non è utilizzabile
const FY_OPEN    = 2
const FY_CLOSING = 3
const selectableFiscalYears = computed(() =>
  fiscalYears.value.filter(f => f.statusId === FY_OPEN || f.statusId === FY_CLOSING)
)
const noFiscalYearAvailable = computed(() => selectableFiscalYears.value.length === 0)

// Warning non bloccante: data registrazione fuori dal periodo, solo se esercizio in Closing
const registrationDateWarning = ref('')

function checkRegistrationDateWarning() {
  registrationDateWarning.value = ''
  const fyId = expForm.value.fiscalYearId
  const regDate = expForm.value.registrationDate
  if (!fyId || !regDate) return

  const fy = fiscalYears.value.find(f => f.id === fyId)
  if (!fy || fy.statusId !== FY_CLOSING) return

  const parseLocal = s => { const [y,m,d] = s.slice(0,10).split('-').map(Number); return new Date(y, m-1, d) }
  const reg   = parseLocal(regDate)
  const start = parseLocal(fy.startDate)
  const end   = parseLocal(fy.endDate)
  if (reg < start || reg > end) {
    const fmt = d => parseLocal(d).toLocaleDateString('it-IT')
    registrationDateWarning.value =
      `La data di registrazione (${fmt(regDate)}) è fuori dal periodo dell'esercizio ` +
      `(${fmt(fy.startDate)} – ${fmt(fy.endDate)}). Il movimento verrà registrato ugualmente ` +
      `perché l'esercizio è in fase di chiusura.`
  }
}

// ─── Filtri ───────────────────────────────────────────────────
const supplierSearch = ref('')
const expTypeFilter  = ref(0)
const expFilter      = ref('')

let supplierSearchTimer = null
function onSupplierSearch() {
  clearTimeout(supplierSearchTimer)
  supplierSearchTimer = setTimeout(applyFilters, 300)
}

// ─── Metodi di pagamento ──────────────────────────────────────
const paymentMethods = ref([])

// ─── Spese ────────────────────────────────────────────────────
const expenses         = ref([])
const loadingExp       = ref(false)
const showExpenseModal = ref(false)
const editingExp       = ref(null)
const savingExp        = ref(false)
const expErrors        = ref({})
const actionInProgress = ref({})

// Filtro lato client: esercizio fiscale + testo (fornitore e descrizione)
const filteredExpenses = computed(() => {
  const q  = supplierSearch.value.trim().toLowerCase()
  const fy = selectedFiscalYearId.value
  return expenses.value.filter(e => {
    if (fy && e.fiscalYearId !== fy) return false
    if (q && !(
      (e.supplierName ?? '').toLowerCase().includes(q) ||
      (e.name         ?? '').toLowerCase().includes(q)
    )) return false
    return true
  })
})

function applyFilters() {
  loadExpenses()
}

function clearExpError(field) { delete expErrors.value[field] }

function validateExpForm() {
  const e = {}
  const f = expForm.value
  if (!f.fiscalYearId)          e.fiscalYearId     = 'Selezionare un esercizio fiscale'
  if (!f.name?.trim())          e.name             = 'Campo obbligatorio'
  if (!f.expenseTypeId)         e.expenseTypeId    = 'Selezionare un tipo spesa'
  if (!f.documentDate)          e.documentDate     = 'Campo obbligatorio'
  if (!f.registrationDate)      e.registrationDate = 'Campo obbligatorio'
  const hasTaxable    = f.taxableAmount          > 0
  const hasTaxExempt  = f.taxableAmountVatExempt > 0
  if (!hasTaxable && !hasTaxExempt)
    e.taxableBase = 'Inserire almeno un importo tra Imponibile e Imponibile esente IVA'
  if (hasTaxable && !(f.vatAmount > 0))
    e.vatAmount = "L'IVA è obbligatoria quando è presente l'imponibile"
  if (!f.accountId)             e.accountId        = 'Selezionare un conto'
  if (!f.millesimalTableId)     e.millesimalTableId = 'Selezionare una tabella millesimale'

  // Data registrazione fuori periodo: bloccante solo se l'esercizio è Open (non Closing)
  if (f.fiscalYearId && f.registrationDate) {
    const fy = fiscalYears.value.find(fy => fy.id === f.fiscalYearId)
    if (fy && fy.statusId === FY_OPEN) {
      const parseLocal = s => { const [y,m,d] = s.slice(0,10).split('-').map(Number); return new Date(y, m-1, d) }
      const reg   = parseLocal(f.registrationDate)
      const start = parseLocal(fy.startDate)
      const end   = parseLocal(fy.endDate)
      if (reg < start || reg > end) {
        const fmt = d => parseLocal(d).toLocaleDateString('it-IT')
        e.registrationDate =
          `La data deve essere compresa nel periodo dell'esercizio (${fmt(fy.startDate)} – ${fmt(fy.endDate)})`
      }
    }
  }

  expErrors.value = e
  return Object.keys(e).length === 0
}

const today = new Date().toISOString().slice(0, 10)
const suppliers        = ref([])
const millesimalTables = ref([])
const enabledMillesimalTables = computed(() => {
  const currentId = editingExp.value ? expForm.value.millesimalTableId : null
  return millesimalTables.value.filter(t => t.isEnabled || t.id === currentId)
})

const emptyExpForm = () => ({
  fiscalYearId: null,
  name: '', documentNumber: '', documentDate: today, registrationDate: today,
  taxableAmount: 0, taxableAmountVatExempt: 0,
  grossAmount: 0, vatAmount: 0, netAmount: 0,
  pensionFund: 0, withholdingTax: 0, stampDuty: 0,
  expenseTypeId: 0, paymentStatusId: 1,
  paymentMethodId: null, supplierId: null, accountId: null, millesimalTableId: null, description: '',
  chargeabilityTypeId: 1,
})

function autoCalcGross() {
  const f = expForm.value
  const taxable    = Number(f.taxableAmount)          || 0
  const taxExempt  = Number(f.taxableAmountVatExempt) || 0
  const vat        = Number(f.vatAmount)              || 0
  const pension    = Number(f.pensionFund)            || 0
  const withholding= Number(f.withholdingTax)         || 0
  const stamp      = Number(f.stampDuty)              || 0
  // Lordo = imponibile + esente + IVA + cassa + bollo - ritenuta
  f.grossAmount = Math.round((taxable + taxExempt + vat + pension + stamp - withholding) * 100) / 100
}
const expForm = ref(emptyExpForm())

// ChartOfAccounts
const chartOfAccounts      = ref([])
const expenseAccountGroups = ref([])
let accountsLoaded = false

async function loadExpenses() {
  if (!store.selectedCondominioId) return
  loadingExp.value = true
  try {
    let res
    if (expFilter.value === 'unpaid') {
      res = await expenseApi.getUnpaid(store.selectedCondominioId)
    } else if (expTypeFilter.value) {
      res = await expenseApi.getByType(store.selectedCondominioId, expTypeFilter.value)
    } else {
      res = await expenseApi.getByCondominium(store.selectedCondominioId)
    }
    expenses.value = res.data ?? []
  } catch { expenses.value = [] } finally { loadingExp.value = false }
}

async function openExpenseModal(e = null) {
  expErrors.value  = {}
  registrationDateWarning.value = ''
  editingExp.value = e?.id ?? null

  // Auto-seleziona l'esercizio attivo per le nuove spese
  const defaultFyId = selectableFiscalYears.value.find(f => f.isActive)?.id
    ?? selectableFiscalYears.value[0]?.id
    ?? null

  expForm.value = e ? {
    fiscalYearId:       e.fiscalYearId ?? defaultFyId,
    name:               e.name ?? '',
    documentNumber:     e.documentNumber ?? '',
    documentDate:       e.documentDate?.slice(0, 10) ?? today,
    registrationDate:   e.registrationDate?.slice(0, 10) ?? today,
    taxableAmount:          e.taxableAmount          ?? 0,
    taxableAmountVatExempt: e.taxableAmountVatExempt ?? 0,
    grossAmount:            e.grossAmount            ?? 0,
    vatAmount:              e.vatAmount              ?? 0,
    netAmount:              e.netAmount              ?? 0,
    pensionFund:            e.pensionFund            ?? 0,
    withholdingTax:         e.withholdingTax         ?? 0,
    stampDuty:              e.stampDuty              ?? 0,
    expenseTypeId:          e.expenseTypeId          ?? 0,
    paymentStatusId:    e.paymentStatusId ?? 1,
    paymentMethodId:    e.paymentMethodId ?? null,
    supplierId:         e.supplierId ?? null,
    accountId:          e.accountId ?? null,
    millesimalTableId:  e.millesimalTableId ?? null,
    description:        e.description ?? '',
    chargeabilityTypeId: e.chargeabilityTypeId ?? 1,
  } : { ...emptyExpForm(), fiscalYearId: defaultFyId }

  if (!store.fiscalYears.length) await store.loadFiscalYears()

  if (!accountsLoaded && store.selectedCondominioId) {
    try {
      const { data } = await chartOfAccountsApi.getByCondominium(store.selectedCondominioId)
      chartOfAccounts.value = data ?? []
      accountsLoaded = true
    } catch { chartOfAccounts.value = [] }
  }
  buildExpenseAccountGroups()

  if (store.selectedCondominioId) {
    try {
      const [supRes, mtRes] = await Promise.all([
        supplierApi.getAll(),
        millesimalTableApi.getByCondominium(store.selectedCondominioId),
      ])
      suppliers.value = supRes.data ?? []
      millesimalTables.value = mtRes.data ?? []
    } catch {}
  }
  // Controlla subito il warning se stiamo modificando una spesa esistente
  if (e) checkRegistrationDateWarning()

  showExpenseModal.value = true
}

function buildExpenseAccountGroups() {
  const TYPE_MAP    = { 1: 'Entrata', 2: 'Uscita', 3: 'Patrimoniale' }
  const TYPE_ORDER  = ['Uscita', 'Entrata', 'Patrimoniale']
  const TYPE_LABELS = { Uscita: 'Uscite', Entrata: 'Entrate', Patrimoniale: 'Patrimoniale' }

  const accountById = Object.fromEntries(chartOfAccounts.value.map(a => [a.id, a]))
  const groups = {}
  for (const account of chartOfAccounts.value) {
    if (account.level === 1) continue
    const typeKey = TYPE_MAP[account.type] ?? 'Uscita'
    if (!groups[typeKey]) groups[typeKey] = []
    const parent = account.parentAccountId ? accountById[account.parentAccountId] : null
    const label = parent
      ? `${parent.code} ${parent.name}  ›  ${account.code} ${account.name}`
      : `${account.code} ${account.name}`
    groups[typeKey].push({ accountId: account.id, accountCode: account.code, accountName: account.name, label })
  }
  expenseAccountGroups.value = TYPE_ORDER.filter(t => groups[t]?.length).map(t => ({
    typeId: t, typeLabel: TYPE_LABELS[t] ?? t,
    rows: groups[t].sort((a, b) => (a.accountCode ?? '').localeCompare(b.accountCode ?? '')),
  }))
}

async function saveExpense() {
  if (!validateExpForm()) return
  savingExp.value = true
  try {
    const payload = {
      name:               expForm.value.name,
      documentNumber:     expForm.value.documentNumber || null,
      documentDate:       expForm.value.documentDate,
      registrationDate:   expForm.value.registrationDate,
      taxableAmount:          expForm.value.taxableAmount          || 0,
      taxableAmountVatExempt: expForm.value.taxableAmountVatExempt || 0,
      vatAmount:              expForm.value.vatAmount              || 0,
      pensionFund:            expForm.value.pensionFund            || 0,
      withholdingTax:         expForm.value.withholdingTax         || 0,
      stampDuty:              expForm.value.stampDuty              || 0,
      expenseTypeId:      expForm.value.expenseTypeId,
      paymentStatusId:    expForm.value.paymentStatusId || 1,
      paymentMethodId:    expForm.value.paymentMethodId || null,
      description:        expForm.value.description || null,
      condominiumId:      store.selectedCondominioId,
      fiscalYearId:       expForm.value.fiscalYearId ?? null,
      accountId:          expForm.value.accountId         ?? null,
      millesimalTableId:  expForm.value.millesimalTableId ?? null,
      supplierId:         expForm.value.supplierId        ?? null,
      chargeabilityTypeId: expForm.value.chargeabilityTypeId ?? 1,
    }
    if (editingExp.value) {
      await expenseApi.update(editingExp.value, payload)
    } else {
      await expenseApi.create(payload)
    }
    showExpenseModal.value = false
    await loadExpenses()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { savingExp.value = false }
}

async function markPaid(id) {
  actionInProgress.value[`pay-${id}`] = true
  try { await expenseApi.markAsPaid(id, today, null); await loadExpenses() }
  catch {} finally { actionInProgress.value[`pay-${id}`] = false }
}

async function markUnpaid(id) {
  actionInProgress.value[`pay-${id}`] = true
  try { await expenseApi.markAsUnpaid(id); await loadExpenses() }
  catch {} finally { actionInProgress.value[`pay-${id}`] = false }
}

async function deleteExpense(id) {
  if (!confirm('Eliminare questa spesa?')) return
  actionInProgress.value[`del-${id}`] = true
  try { await expenseApi.delete(id); await loadExpenses() }
  catch {} finally { actionInProgress.value[`del-${id}`] = false }
}

// Ricalcola warning quando cambia la data di registrazione
watch(() => expForm.value.registrationDate, checkRegistrationDateWarning)

// Auto-select DefaultMillesimalTable when account changes
watch(() => expForm.value.accountId, (accountId) => {
  if (!accountId) return
  const account = chartOfAccounts.value.find(a => a.id === accountId)
  if (account?.defaultMillesimalTableId) {
    expForm.value.millesimalTableId = account.defaultMillesimalTableId
  }
})

// ─── Formatters ───────────────────────────────────────────────
const fmt     = (v) => v != null ? '€ ' + Number(v).toLocaleString('it-IT', { minimumFractionDigits: 2 }) : '—'
const fmtDate = (d) => d ? new Date(d).toLocaleDateString('it-IT') : '—'
const payBadge = (id) => ({ 1: 'badge-amber', 2: 'badge-green', 3: 'badge-red' }[id] ?? 'badge-muted')

// ─── Watchers / Init ──────────────────────────────────────────
watch(() => store.selectedCondominioId, () => {
  accountsLoaded = false
  expenseAccountGroups.value = []
  expTypeFilter.value = 0
  expFilter.value = ''
  supplierSearch.value = ''
  loadExpenses()
})

onMounted(async () => {
  if (!store.fiscalYears.length) await store.loadFiscalYears()
  const { data } = await expenseApi.getPaymentMethods()
  paymentMethods.value = data ?? []
  await loadExpenses()
})
onUnmounted(() => window.removeEventListener('app:refresh', loadExpenses))
window.addEventListener('app:refresh', loadExpenses)
</script>

<style scoped>
.tab-toolbar { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1rem; flex-wrap: wrap; }

.no-fiscal-year-banner {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  padding: 1.25rem;
  border-radius: 8px;
  border: 1px solid #f59e0b;
  background: color-mix(in srgb, #f59e0b 8%, transparent);
  margin-bottom: 0.5rem;
}
.no-fiscal-year-icon { font-size: 2rem; line-height: 1; flex-shrink: 0; }
.no-fiscal-year-desc { font-size: 0.875rem; color: var(--text-secondary); margin-top: 0.35rem; line-height: 1.5; }

.label-hint {
  font-size: 0.75rem;
  font-weight: 400;
  color: var(--text-muted);
  margin-left: 0.35rem;
}
.label-required {
  color: var(--accent-red);
  margin-left: 0.2rem;
  font-weight: 600;
}

.warning-banner {
  margin-bottom: 1rem;
  padding: .65rem 1rem;
  border-radius: 6px;
  border: 1px solid #f59e0b;
  background: color-mix(in srgb, #f59e0b 10%, transparent);
  color: #92400e;
  font-size: .85rem;
  line-height: 1.45;
}
</style>
