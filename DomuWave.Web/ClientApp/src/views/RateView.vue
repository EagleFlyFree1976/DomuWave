<template>
  <div>
    <div class="page-header">
      <h1>Rate & Quote</h1>
      <div class="flex gap-2">
        <button class="btn btn-ghost" @click="activeTab='rate'" :class="activeTab==='rate'?'btn-active':''">Rate</button>
        <button class="btn btn-ghost" @click="activeTab='quote'" :class="activeTab==='quote'?'btn-active':''">Quote</button>
      </div>
    </div>

    <!-- ── Rate ─────────────────────────────────── -->
    <div v-if="activeTab === 'rate'">
      <div class="tab-toolbar">
        <select class="form-select" v-model.number="selectedFiscalYearId" style="width:260px">
          <option :value="null">— Tutti gli esercizi —</option>
          <option v-for="fy in fiscalYears" :key="fy.id" :value="fy.id">
            {{ fy.code }}{{ fy.description ? ` – ${fy.description}` : '' }}
          </option>
        </select>
        <select class="form-select" v-model="instFilter" style="width:140px">
          <option value="all">Tutte</option>
          <option value="open">Aperte</option>
          <option value="overdue">Scadute</option>
        </select>
        <button v-if="canCreate" class="btn btn-primary" @click="openInstModal()" style="margin-left:auto">+ Nuova rata</button>
      </div>

      <div class="card">
        <div v-if="loadingInst" class="loading-state"><div class="spinner"></div></div>
        <div v-else-if="!installments.length" class="empty-state">
          <div class="empty-icon">◷</div><div>Nessuna rata trovata</div>
        </div>
        <div v-else class="table-wrap">
          <table>
            <thead>
              <tr>
                <th>N°</th>
                <th>Esercizio</th>
                <th>Scadenza</th>
                <th>Importo</th>
                <th>Stato</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="i in installments" :key="i.id">
                <td class="mono text-muted">{{ i.installmentNumber }}</td>
                <td class="text-secondary">{{ i.fiscalYearCode ?? '—' }}</td>
                <td class="mono" :class="isOverdue(i.dueDate) ? 'text-red' : 'text-secondary'">{{ fmtDate(i.dueDate) }}</td>
                <td class="mono">{{ fmt(i.totalAmount) }}</td>
                <td><span class="badge" :class="instBadge(i.statusId)">{{ i.statusName }}</span></td>
                <td>
                  <div v-if="canEdit || canDelete" class="row-actions">
                    <button v-if="canEdit" class="btn-icon" @click="openInstModal(i)">✎</button>
                    <button v-if="canDelete" class="btn-icon" @click="deleteInst(i.id)" style="color:var(--accent-red)">✕</button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- ── Quote ─────────────────────────────────── -->
    <div v-if="activeTab === 'quote'">
      <div class="tab-toolbar">
        <span class="text-muted" style="font-size:0.875rem">Seleziona una rata per vedere le quote</span>
        <select class="form-select" v-model="selectedInstId" style="width:300px" v-if="allInstallments.length">
          <option value="">— Scegli rata —</option>
          <option v-for="i in allInstallments" :key="i.id" :value="i.id">
            {{ i.installmentNumber }} – {{ i.fiscalYearCode ?? '?' }} ({{ fmtDate(i.dueDate) }})
          </option>
        </select>
        <button v-if="canCreate" class="btn btn-primary" @click="openFeeModal()" :disabled="!selectedInstId" style="margin-left:auto">+ Nuova quota</button>
      </div>

      <div class="card" v-if="selectedInstId">
        <div v-if="loadingFees" class="loading-state"><div class="spinner"></div></div>
        <div v-else-if="!fees.length" class="empty-state">
          <div class="empty-icon">◷</div><div>Nessuna quota per questa rata</div>
        </div>
        <div v-else class="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Unità</th>
                <th>Dovuto</th>
                <th>Pagato</th>
                <th>Saldo</th>
                <th>Stato</th>
                <th>Data pagamento</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="f in fees" :key="f.id">
                <td class="mono text-secondary">{{ f.unit?.internalNumber || f.unitId }}</td>
                <td class="mono">{{ fmt(f.amountDue) }}</td>
                <td class="mono text-green">{{ fmt(f.amountPaid) }}</td>
                <td class="mono" :class="f.balance > 0 ? 'text-amber' : 'text-green'">{{ fmt(f.balance) }}</td>
                <td><span class="badge" :class="feeBadge(f.paymentStatus)">{{ f.paymentStatus }}</span></td>
                <td class="text-secondary">{{ fmtDate(f.paymentDate) }}</td>
                <td>
                  <div v-if="canEdit || canDelete" class="row-actions">
                    <button v-if="canEdit" class="btn-icon" @click="openFeeModal(f)">✎</button>
                    <button v-if="canDelete" class="btn-icon" @click="deleteFee(f.id)" style="color:var(--accent-red)">✕</button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
      <div v-else class="empty-state" style="margin-top:2rem">
        <div class="empty-icon">◷</div>
        <div>Seleziona una rata per vedere le quote dei condomini</div>
      </div>
    </div>

    <!-- Inst Modal -->
    <div class="modal-overlay" v-if="showInstModal" @click.self="showInstModal=false">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ editingInst ? 'Modifica' : 'Nuova' }} rata</h2>
          <button class="btn-icon" @click="showInstModal=false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-grid">
            <div class="form-group" :class="{ 'has-error': instErrors.fiscalYearId }">
              <label class="form-label">Esercizio fiscale *</label>
              <select class="form-select" v-model.number="instForm.fiscalYearId">
                <option :value="null">— Seleziona —</option>
                <option v-for="fy in fiscalYears" :key="fy.id" :value="fy.id">
                  {{ fy.code }}{{ fy.description ? ` – ${fy.description}` : '' }}
                </option>
              </select>
              <span v-if="instErrors.fiscalYearId" class="field-error">{{ instErrors.fiscalYearId }}</span>
            </div>
            <div class="form-group" :class="{ 'has-error': instErrors.installmentNumber }">
              <label class="form-label">N° rata *</label>
              <input class="form-input" type="number" min="1" v-model.number="instForm.installmentNumber" />
              <span v-if="instErrors.installmentNumber" class="field-error">{{ instErrors.installmentNumber }}</span>
            </div>
            <div class="form-group" :class="{ 'has-error': instErrors.dueDate }">
              <label class="form-label">Scadenza *</label>
              <input class="form-input" type="date" v-model="instForm.dueDate" />
              <span v-if="instErrors.dueDate" class="field-error">{{ instErrors.dueDate }}</span>
            </div>
            <div class="form-group" :class="{ 'has-error': instErrors.totalAmount }">
              <label class="form-label">Importo (€) *</label>
              <input class="form-input" type="number" step="0.01" v-model.number="instForm.totalAmount" />
              <span v-if="instErrors.totalAmount" class="field-error">{{ instErrors.totalAmount }}</span>
            </div>
          </div>
          <div class="form-group">
            <label class="form-label">Stato *</label>
            <select class="form-select" v-model.number="instForm.statusId">
              <option :value="1">Bozza</option>
              <option :value="2">Aperta</option>
              <option :value="3">Pagata</option>
              <option :value="4">Scaduta</option>
              <option :value="5">Annullata</option>
            </select>
          </div>
          <div class="form-group">
            <label class="form-label">Note</label>
            <textarea class="form-textarea" v-model="instForm.notes" rows="2"></textarea>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showInstModal=false">Annulla</button>
          <button class="btn btn-primary" @click="saveInst" :disabled="savingInst">
            <span v-if="savingInst" class="spinner" style="width:14px;height:14px"></span>
            {{ editingInst ? 'Salva' : 'Crea' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue'
import { useAppStore } from '@/stores/app'
import { installmentApi, feeApi, fiscalYearApi } from '@/services/api'
import { usePermissions } from '@/composables/usePermissions'

const store = useAppStore()
const { canCreate, canEdit, canDelete } = usePermissions()
const activeTab = ref('rate')

// ── Esercizi fiscali ──────────────────────────────────────────
const fiscalYears          = ref([])
const selectedFiscalYearId = ref(null)

async function loadFiscalYears() {
  if (!store.selectedCondominioId) { fiscalYears.value = []; return }
  try {
    const { data } = await fiscalYearApi.getByCondominium(store.selectedCondominioId)
    fiscalYears.value = data ?? []
    // Preseleziona l'esercizio attivo se nessuno è selezionato
    if (!selectedFiscalYearId.value) {
      const active = fiscalYears.value.find(fy => fy.isActive)
      selectedFiscalYearId.value = active?.id ?? null
    }
  } catch { fiscalYears.value = [] }
}

// ── Rate ──────────────────────────────────────────────────────
const instFilter   = ref('all')
const installments = ref([])
const allInstallments = ref([])
const loadingInst  = ref(false)
const showInstModal = ref(false)
const editingInst  = ref(null)
const savingInst   = ref(false)
const instForm     = ref({})
const instErrors   = ref({})

// ── Quote ─────────────────────────────────────────────────────
const fees          = ref([])
const loadingFees   = ref(false)
const selectedInstId = ref('')
const editingFee    = ref(null)

const fmt     = (v) => v != null ? '€ ' + Number(v).toLocaleString('it-IT', { minimumFractionDigits: 2 }) : '—'
const fmtDate = (d) => d ? new Date(d).toLocaleDateString('it-IT') : '—'
const isOverdue = (d) => d && new Date(d) < new Date()
const instBadge = (id) => ({ 1: 'badge-muted', 2: 'badge-blue', 3: 'badge-green', 4: 'badge-red', 5: 'badge-muted' }[id] || 'badge-muted')
const feeBadge  = (s)  => ({ ToPay: 'badge-amber', Paid: 'badge-green', Overdue: 'badge-red', PartiallyPaid: 'badge-purple' }[s] || 'badge-muted')

async function loadInstallments() {
  if (!store.selectedCondominioId) return
  loadingInst.value = true
  try {
    let data
    if (instFilter.value === 'open') {
      ({ data } = await installmentApi.getOpen(store.selectedCondominioId))
    } else if (instFilter.value === 'overdue') {
      ({ data } = await installmentApi.getOverdue(store.selectedCondominioId))
    } else if (selectedFiscalYearId.value) {
      ({ data } = await installmentApi.getByFiscalYear(store.selectedCondominioId, selectedFiscalYearId.value))
    } else {
      ({ data } = await installmentApi.getByCondominium(store.selectedCondominioId))
    }
    installments.value = data ?? []
  } catch { installments.value = [] } finally { loadingInst.value = false }
}

async function loadAllInstallments() {
  if (!store.selectedCondominioId) return
  try {
    const { data } = await installmentApi.getByCondominium(store.selectedCondominioId)
    allInstallments.value = data ?? []
  } catch { allInstallments.value = [] }
}

async function loadFees() {
  if (!selectedInstId.value) return
  loadingFees.value = true
  try {
    const { data } = await feeApi.getByInstallment(selectedInstId.value)
    fees.value = data ?? []
  } catch { fees.value = [] } finally { loadingFees.value = false }
}

function validateInst() {
  const e = {}
  if (!instForm.value.fiscalYearId) e.fiscalYearId = 'Esercizio fiscale obbligatorio'
  if (!instForm.value.installmentNumber) e.installmentNumber = 'N° rata obbligatorio'
  if (!instForm.value.dueDate) e.dueDate = 'Scadenza obbligatoria'
  if (instForm.value.totalAmount == null || instForm.value.totalAmount === '') e.totalAmount = 'Importo obbligatorio'
  instErrors.value = e
  return Object.keys(e).length === 0
}

function openInstModal(i = null) {
  editingInst.value = i?.id ?? null
  instErrors.value  = {}
  instForm.value = i
    ? { ...i }
    : {
        fiscalYearId:      selectedFiscalYearId.value,
        installmentNumber: 1,
        dueDate:           '',
        totalAmount:       0,
        statusId:          2,
        notes:             '',
      }
  showInstModal.value = true
}

async function saveInst() {
  if (!validateInst()) return
  savingInst.value = true
  try {
    if (editingInst.value) {
      await installmentApi.update(editingInst.value, instForm.value)
    } else {
      await installmentApi.create({ ...instForm.value, condominiumId: store.selectedCondominioId })
    }
    store.toast('Rata salvata', 'success')
    showInstModal.value = false
    loadInstallments()
    loadAllInstallments()
  } catch { store.toast('Errore', 'error') } finally { savingInst.value = false }
}

async function deleteInst(id) {
  if (!confirm('Eliminare la rata?')) return
  try { await installmentApi.delete(id); store.toast('Rata eliminata', 'success'); loadInstallments(); loadAllInstallments() }
  catch { store.toast('Errore', 'error') }
}

function openFeeModal(f = null) {
  editingFee.value = f?.id ?? null
}

async function deleteFee(id) {
  if (!confirm('Eliminare questa quota?')) return
  try { await feeApi.delete(id); store.toast('Quota eliminata', 'success'); loadFees() }
  catch { store.toast('Errore', 'error') }
}

watch(() => store.selectedCondominioId, async () => {
  selectedFiscalYearId.value = null
  await loadFiscalYears()
  loadInstallments()
  loadAllInstallments()
})
watch([selectedFiscalYearId, instFilter], loadInstallments)
watch(selectedInstId, loadFees)
watch(activeTab, (t) => { if (t === 'quote') loadAllInstallments() })
onMounted(async () => {
  await loadFiscalYears()
  loadInstallments()
  loadAllInstallments()
})
</script>

<style scoped>
.tab-toolbar { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1rem; flex-wrap: wrap; }
.btn-active { background: var(--accent-glow) !important; color: var(--accent) !important; border-color: var(--border-active) !important; }
.row-actions { display: flex; gap: 0.4rem; justify-content: flex-end; }
.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; margin-bottom: 0.75rem; }
.form-group { display: flex; flex-direction: column; gap: 0.3rem; }
.form-label { font-size: 0.8rem; font-weight: 500; color: var(--text-muted); }
.has-error .form-input, .has-error .form-select { border-color: var(--accent-red, #e53e3e); }
.field-error { font-size: 0.75rem; color: var(--accent-red, #f87171); }
</style>
