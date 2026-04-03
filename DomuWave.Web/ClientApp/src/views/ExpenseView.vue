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
              <td class="text-secondary">{{ e.paymentMethod || '—' }}</td>
              <td><span class="badge" :class="payBadge(e.paymentStatusId)">{{ e.paymentStatusName }}</span></td>
              <td>
                <div class="row-actions">
                  <button v-if="canEdit && e.paymentStatusId !== 2" class="btn btn-sm btn-ghost"
                          @click="markPaid(e.id)">Paga</button>
                  <button v-if="canEdit" class="btn-icon" @click="openExpenseModal(e)">✎</button>
                  <button v-if="canDelete" class="btn-icon" style="color:var(--accent-red)"
                          @click="deleteExpense(e.id)">✕</button>
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
        <div class="form-group" :class="{ 'has-error': expErrors.grossAmount }">
          <label class="form-label">Importo lordo (€) *</label>
          <input class="form-input" type="number" step="0.01" v-model.number="expForm.grossAmount" @input="clearExpError('grossAmount')" />
          <span v-if="expErrors.grossAmount" class="field-error">{{ expErrors.grossAmount }}</span>
        </div>
        <div class="form-group">
          <label class="form-label">IVA (€)</label>
          <input class="form-input" type="number" step="0.01" v-model.number="expForm.vatAmount" />
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
          <select class="form-select" v-model="expForm.paymentMethod">
            <option value="">—</option>
            <option value="BankTransfer">Bonifico</option>
            <option value="Cash">Contanti</option>
            <option value="Check">Assegno</option>
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
            <input type="radio" v-model.number="expForm.chargeabilityTypeId" :value="0" />
            Proprietario
          </label>
          <label class="radio-label">
            <input type="radio" v-model.number="expForm.chargeabilityTypeId" :value="1" />
            Inquilino (se presente)
          </label>
          <label class="radio-label">
            <input type="radio" v-model.number="expForm.chargeabilityTypeId" :value="2" />
            Automatico
          </label>
        </div>
      </div>
      <template #footer>
        <button class="btn btn-ghost" @click="showExpenseModal = false">Annulla</button>
        <button class="btn btn-primary" @click="saveExpense" :disabled="savingExp">
          <span v-if="savingExp" class="spinner" style="width:14px;height:14px"></span>
          {{ editingExp ? 'Salva' : 'Crea' }}
        </button>
      </template>
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useAppStore } from '@/stores/app'
import { expenseApi, chartOfAccountsApi, supplierApi, millesimalTableApi } from '@/services/api'
import { usePermissions } from '@/composables/usePermissions'
import BaseModal from '@/components/BaseModal.vue'

const store = useAppStore()
const { canCreate, canEdit, canDelete } = usePermissions()

// ─── Fiscal Years ─────────────────────────────────────────────
const fiscalYears          = computed(() => store.fiscalYears)
const selectedFiscalYearId = ref(store.selectedFiscalYearId ?? null)

// ─── Filtri ───────────────────────────────────────────────────
const supplierSearch = ref('')
const expTypeFilter  = ref(0)
const expFilter      = ref('')

let supplierSearchTimer = null
function onSupplierSearch() {
  clearTimeout(supplierSearchTimer)
  supplierSearchTimer = setTimeout(applyFilters, 300)
}

// ─── Spese ────────────────────────────────────────────────────
const expenses         = ref([])
const loadingExp       = ref(false)
const showExpenseModal = ref(false)
const editingExp       = ref(null)
const savingExp        = ref(false)
const expErrors        = ref({})

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
  if (!f.name?.trim())          e.name             = 'Campo obbligatorio'
  if (!f.expenseTypeId)         e.expenseTypeId    = 'Selezionare un tipo spesa'
  if (!f.documentDate)          e.documentDate     = 'Campo obbligatorio'
  if (!f.registrationDate)      e.registrationDate = 'Campo obbligatorio'
  if (!f.grossAmount || f.grossAmount <= 0) e.grossAmount = 'Inserire un importo maggiore di zero'
  if (!f.accountId)             e.accountId        = 'Selezionare un conto'
  if (!f.millesimalTableId)     e.millesimalTableId = 'Selezionare una tabella millesimale'
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
  name: '', documentNumber: '', documentDate: today, registrationDate: today,
  grossAmount: 0, vatAmount: 0, expenseTypeId: 0, paymentStatusId: 1,
  paymentMethod: '', supplierId: null, accountId: null, millesimalTableId: null, description: '',
  chargeabilityTypeId: 0,
})
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
  editingExp.value = e?.id ?? null
  expForm.value = e ? {
    name:               e.name ?? '',
    documentNumber:     e.documentNumber ?? '',
    documentDate:       e.documentDate?.slice(0, 10) ?? today,
    registrationDate:   e.registrationDate?.slice(0, 10) ?? today,
    grossAmount:        e.grossAmount ?? 0,
    vatAmount:          e.vatAmount ?? 0,
    expenseTypeId:      e.expenseTypeId ?? 0,
    paymentStatusId:    e.paymentStatusId ?? 1,
    paymentMethod:      e.paymentMethod ?? '',
    supplierId:         e.supplierId ?? null,
    accountId:          e.accountId ?? null,
    millesimalTableId:  e.millesimalTableId ?? null,
    description:        e.description ?? '',
    chargeabilityTypeId: e.chargeabilityTypeId ?? 0,
  } : emptyExpForm()

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
      grossAmount:        expForm.value.grossAmount,
      vatAmount:          expForm.value.vatAmount,
      netAmount:          expForm.value.grossAmount - expForm.value.vatAmount,
      expenseTypeId:      expForm.value.expenseTypeId,
      paymentStatusId:    expForm.value.paymentStatusId || 1,
      paymentMethod:      expForm.value.paymentMethod || null,
      description:        expForm.value.description || null,
      condominiumId:      store.selectedCondominioId,
      fiscalYearId:       selectedFiscalYearId.value ?? null,
      accountId:          expForm.value.accountId         ?? null,
      millesimalTableId:  expForm.value.millesimalTableId ?? null,
      supplierId:         expForm.value.supplierId        ?? null,
      chargeabilityTypeId: expForm.value.chargeabilityTypeId ?? 0,
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
  try { await expenseApi.markAsPaid(id, today, 'BankTransfer'); await loadExpenses() } catch {}
}

async function deleteExpense(id) {
  if (!confirm('Eliminare questa spesa?')) return
  try { await expenseApi.delete(id); await loadExpenses() } catch {}
}

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
  await loadExpenses()
})
</script>

<style scoped>
.tab-toolbar { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1rem; flex-wrap: wrap; }
</style>
