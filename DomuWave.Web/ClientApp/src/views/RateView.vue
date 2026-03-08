<template>
  <div>
    <div class="page-header">
      <h1>Rate</h1>
      <button v-if="canCreate" class="btn btn-primary" @click="openInstModal()">+ Nuova rata</button>
    </div>

    <!-- ── Toolbar ─────────────────────────────────── -->
    <div class="tab-toolbar">
      <select class="form-select" v-model.number="selectedFiscalYearId" style="width:280px">
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
    </div>

    <!-- ── Tabella rate ────────────────────────────── -->
    <div class="card">
      <div v-if="loadingInst" class="loading-state"><div class="spinner"></div></div>
      <div v-else-if="!installments.length" class="empty-state">
        <div class="empty-icon">◷</div>
        <div>Nessuna rata trovata</div>
      </div>
      <div v-else class="table-wrap">
        <table>
          <thead>
            <tr>
              <th style="width:2rem"></th>
              <th>N°</th>
              <th>Esercizio</th>
              <th>Scadenza</th>
              <th class="text-right">Totale rata</th>
              <th class="text-right">Pagate</th>
              <th class="text-right">Scadute</th>
              <th>Stato</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <template v-for="inst in installments" :key="inst.id">

              <!-- Riga rata -->
              <tr
                class="inst-row"
                :class="{ 'inst-row--expanded': expandedInstId === inst.id }"
                @click="toggleInstExpand(inst)"
              >
                <td class="expand-cell">
                  <span class="expand-icon">{{ expandedInstId === inst.id ? '▾' : '▸' }}</span>
                </td>
                <td class="mono text-muted">
                  {{ inst.installmentNumber }}
                  <span class="origin-badge" :class="inst.budgetId ? 'origin-auto' : 'origin-manual'" :title="inst.budgetId ? 'Generata automaticamente da budget' : 'Inserita manualmente'">
                    {{ inst.budgetId ? 'AUTO' : 'MAN' }}
                  </span>
                </td>
                <td class="text-secondary">{{ inst.fiscalYearCode ?? '—' }}</td>
                <td class="mono" :class="isOverdue(inst.dueDate) && inst.statusId !== 3 ? 'text-red' : 'text-secondary'">
                  {{ fmtDate(inst.dueDate) }}
                </td>
                <td class="mono text-right">{{ fmt(inst.totalAmount) }}</td>
                <td class="mono text-right text-green">{{ fmt(instSummary(inst.id)?.paid) }}</td>
                <td class="mono text-right text-amber">{{ fmt(instSummary(inst.id)?.overdue) }}</td>
                <td><span class="badge" :class="instBadge(inst.statusId)">{{ inst.statusName }}</span></td>
                <td @click.stop>
                  <div class="row-actions">
                    <button v-if="canEdit" class="btn-icon" @click="openInstModal(inst)" title="Modifica">✎</button>
                    <button v-if="canDelete" class="btn-icon" @click="deleteInst(inst.id)" style="color:var(--accent-red)" title="Elimina">✕</button>
                  </div>
                </td>
              </tr>

              <!-- Pannello quote inline -->
              <tr v-if="expandedInstId === inst.id" class="fees-row">
                <td colspan="9" class="fees-body">

                  <!-- Header pannello -->
                  <div class="fees-header">
                    <span class="fees-title">Quote – Rata {{ inst.installmentNumber }} ({{ fmtDate(inst.dueDate) }})</span>
                    <button v-if="canCreate" class="btn btn-sm btn-primary" @click.stop="openFeeModal(null, inst)">
                      + Nuova quota
                    </button>
                  </div>

                  <!-- Contenuto quote -->
                  <div v-if="loadingFees" class="loading-state" style="padding:0.75rem">
                    <div class="spinner"></div>
                  </div>
                  <div v-else-if="!fees.length" class="empty-state" style="padding:1rem">
                    <div class="empty-icon" style="font-size:1.2rem">◷</div>
                    <div>Nessuna quota per questa rata</div>
                  </div>
                  <table v-else class="inner-table">
                    <thead>
                      <tr>
                        <th>Unità</th>
                        <th class="text-right">Dovuto</th>
                        <th class="text-right">Pagato</th>
                        <th class="text-right">Saldo</th>
                        <th>Stato</th>
                        <th>Data pagamento</th>
                        <th></th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="f in fees" :key="f.id">
                        <td>
                          <span class="unit-number">{{ f.unitInternalNumber || f.unitId }}</span>
                          <span v-if="f.unitDisplayName" class="unit-display-name">{{ f.unitDisplayName }}</span>
                          <span class="origin-badge" :class="f.isAutoGenerated ? 'origin-auto' : 'origin-manual'" :title="f.isAutoGenerated ? 'Generata automaticamente da budget' : 'Inserita manualmente'">
                            {{ f.isAutoGenerated ? 'AUTO' : 'MAN' }}
                          </span>
                        </td>
                        <td class="mono text-right">{{ fmt(f.amountDue) }}</td>
                        <td class="mono text-right text-green">{{ fmt(f.amountPaid) }}</td>
                        <td class="mono text-right" :class="f.balance > 0 ? 'text-amber' : 'text-green'">{{ fmt(f.balance) }}</td>
                        <td><span class="badge" :class="feeBadge(f.paymentStatus)">{{ feeStatusLabel(f.paymentStatus) }}</span></td>
                        <td class="text-secondary mono" style="font-size:0.82rem">{{ fmtDate(f.paymentDate) }}</td>
                        <td>
                          <div class="row-actions">
                            <button v-if="canEdit" class="btn-icon" @click="openPayModal(f, inst)" title="Registra pagamento" style="color:var(--accent-green)">€</button>
                            <button v-if="canEdit" class="btn-icon" @click="openFeeModal(f, inst)" title="Modifica">✎</button>
                            <button v-if="canDelete" class="btn-icon" @click="deleteFee(f.id)" style="color:var(--accent-red)">✕</button>
                          </div>
                        </td>
                      </tr>
                    </tbody>
                    <!-- Totali -->
                    <tfoot>
                      <tr class="fees-total-row">
                        <td class="text-muted" style="font-size:0.8rem">Totale</td>
                        <td class="mono text-right">{{ fmt(fees.reduce((s,f) => s + f.amountDue, 0)) }}</td>
                        <td class="mono text-right text-green">{{ fmt(fees.reduce((s,f) => s + f.amountPaid, 0)) }}</td>
                        <td class="mono text-right text-amber">{{ fmt(fees.reduce((s,f) => s + f.balance, 0)) }}</td>
                        <td colspan="3"></td>
                      </tr>
                    </tfoot>
                  </table>
                </td>
              </tr>

            </template>
          </tbody>
        </table>
      </div>
    </div>

    <!-- ══ Modal Rata ══════════════════════════════════ -->
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

    <!-- ══ Modal Quota ═════════════════════════════════ -->
    <div class="modal-overlay" v-if="showFeeModal" @click.self="showFeeModal=false">
      <div class="modal">
        <div class="modal-header">
          <h2>
            {{ feeModalMode === 'create' ? 'Nuova quota' : feeModalMode === 'edit' ? 'Modifica quota' : 'Registra pagamento' }}
          </h2>
          <button class="btn-icon" @click="showFeeModal=false">✕</button>
        </div>
        <div class="modal-body">

          <!-- CREA: unità + importo dovuto + note -->
          <template v-if="feeModalMode === 'create'">
            <div class="form-group">
              <label class="form-label">Unità *</label>
              <select class="form-select" v-model.number="feeForm.unitId">
                <option :value="null">— Seleziona unità —</option>
                <option v-for="u in units" :key="u.id" :value="u.id">
                  {{ u.internalNumber }}{{ u.displayName ? ` – ${u.displayName}` : '' }}
                </option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Importo dovuto (€) *</label>
              <input class="form-input" type="number" step="0.01" v-model.number="feeForm.amountDue" />
            </div>
            <div class="form-group">
              <label class="form-label">Note</label>
              <textarea class="form-textarea" v-model="feeForm.notes" rows="2"></textarea>
            </div>
          </template>

          <!-- MODIFICA: importo dovuto + note -->
          <template v-if="feeModalMode === 'edit'">
            <div class="form-group">
              <label class="form-label">Importo dovuto (€) *</label>
              <input class="form-input" type="number" step="0.01" v-model.number="feeForm.amountDue" />
            </div>
            <div class="form-group">
              <label class="form-label">Note</label>
              <textarea class="form-textarea" v-model="feeForm.notes" rows="2"></textarea>
            </div>
          </template>

          <!-- PAGAMENTO: riepilogo + importo da versare + data + metodo -->
          <template v-if="feeModalMode === 'pay'">
            <div class="fee-pay-summary">
              <div class="fee-pay-row">
                <span class="text-muted">Importo dovuto</span>
                <span class="mono">{{ fmt(feeForm.amountDue) }}</span>
              </div>
              <div class="fee-pay-row">
                <span class="text-muted">Già pagato</span>
                <span class="mono text-green">{{ fmt(feeForm.amountPaid) }}</span>
              </div>
            </div>
            <div class="form-grid">
              <div class="form-group">
                <label class="form-label">Importo versamento (€) *</label>
                <input class="form-input" type="number" step="0.01" min="0" v-model.number="feeForm.paymentAmount" />
              </div>
              <div class="form-group">
                <label class="form-label">Data pagamento *</label>
                <input class="form-input" type="date" v-model="feeForm.paymentDate" />
              </div>
              <div class="form-group">
                <label class="form-label">Metodo</label>
                <select class="form-select" v-model="feeForm.paymentMethod">
                  <option value="BankTransfer">Bonifico</option>
                  <option value="Cash">Contanti</option>
                  <option value="Check">Assegno</option>
                </select>
              </div>
            </div>
          </template>

        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showFeeModal=false">Annulla</button>
          <button class="btn btn-primary" @click="saveFee" :disabled="savingFee">
            <span v-if="savingFee" class="spinner" style="width:14px;height:14px"></span>
            {{ feeModalMode === 'pay' ? 'Registra' : feeModalMode === 'edit' ? 'Salva' : 'Crea' }}
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, watch, onMounted, computed } from 'vue'
import { useAppStore } from '@/stores/app'
import { installmentApi, feeApi, fiscalYearApi, unitApi } from '@/services/api'
import { usePermissions } from '@/composables/usePermissions'

const store = useAppStore()
const { canCreate, canEdit, canDelete } = usePermissions()

// ── Esercizi fiscali ──────────────────────────────────────────
const fiscalYears          = ref([])
const selectedFiscalYearId = ref(null)

async function loadFiscalYears() {
  if (!store.selectedCondominioId) { fiscalYears.value = []; return }
  try {
    const { data } = await fiscalYearApi.getByCondominium(store.selectedCondominioId)
    fiscalYears.value = data ?? []
    if (!selectedFiscalYearId.value) {
      const active = fiscalYears.value.find(fy => fy.isActive)
      selectedFiscalYearId.value = active?.id ?? null
    }
  } catch { fiscalYears.value = [] }
}

// ── Rate ──────────────────────────────────────────────────────
const instFilter    = ref('all')
const installments  = ref([])
const loadingInst   = ref(false)
const showInstModal = ref(false)
const editingInst   = ref(null)
const savingInst    = ref(false)
const instForm      = ref({})
const instErrors    = ref({})

// Expand rata → quote
const expandedInstId = ref(null)
const fees           = ref([])
const loadingFees    = ref(false)

// Riepilogo importi per rata (calcolati dalle fee caricate)
// mappa instId → { paid, overdue }
const feeSummaries = ref({})

function instSummary(instId) {
  return feeSummaries.value[instId] ?? null
}

// ── Quote ─────────────────────────────────────────────────────
const showFeeModal   = ref(false)
const feeModalMode   = ref('create') // 'create' | 'edit' | 'pay'
const editingFee     = ref(null)
const savingFee      = ref(false)
const feeForm        = ref({})
const feeInstallment = ref(null) // rata corrente nel modal quota
const units          = ref([])   // unità del condominio (caricate all'apertura modal nuova quota)

// ── Formatters ────────────────────────────────────────────────
const fmt     = (v) => v != null ? '€ ' + Number(v).toLocaleString('it-IT', { minimumFractionDigits: 2 }) : '—'
const fmtDate = (d) => d ? new Date(d).toLocaleDateString('it-IT') : '—'
const isOverdue = (d) => d && new Date(d) < new Date()
const instBadge = (id) => ({ 1: 'badge-muted', 2: 'badge-blue', 3: 'badge-green', 4: 'badge-red', 5: 'badge-muted' }[id] || 'badge-muted')
const feeBadge  = (s)  => ({ ToPay: 'badge-amber', Paid: 'badge-green', Overdue: 'badge-red', PartiallyPaid: 'badge-purple' }[s] || 'badge-muted')
const feeStatusLabel = (s) => ({ ToPay: 'Da pagare', Paid: 'Pagata', Overdue: 'Scaduta', PartiallyPaid: 'Parz. pagata' }[s] || s)

// ── Caricamento rate ──────────────────────────────────────────
async function loadInstallments() {
  if (!store.selectedCondominioId) return
  loadingInst.value = true
  expandedInstId.value = null
  fees.value = []
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

// ── Expand rata → carica quote ────────────────────────────────
async function toggleInstExpand(inst) {
  if (expandedInstId.value === inst.id) {
    expandedInstId.value = null
    fees.value = []
    return
  }
  expandedInstId.value = inst.id
  fees.value = []
  loadingFees.value = true
  try {
    const { data } = await feeApi.getByInstallment(inst.id)
    fees.value = data ?? []
    // Calcola e memorizza i totali per questa rata
    const paid    = fees.value.reduce((s, f) => s + (f.amountPaid ?? 0), 0)
    const overdue = fees.value.filter(f => f.paymentStatus === 'Overdue').reduce((s, f) => s + (f.balance ?? 0), 0)
    feeSummaries.value = { ...feeSummaries.value, [inst.id]: { paid, overdue } }
  } catch { fees.value = [] } finally { loadingFees.value = false }
}

// ── Rata CRUD ─────────────────────────────────────────────────
function validateInst() {
  const e = {}
  if (!instForm.value.fiscalYearId)  e.fiscalYearId = 'Esercizio fiscale obbligatorio'
  if (!instForm.value.installmentNumber) e.installmentNumber = 'N° rata obbligatorio'
  if (!instForm.value.dueDate)        e.dueDate = 'Scadenza obbligatoria'
  if (instForm.value.totalAmount == null || instForm.value.totalAmount === '') e.totalAmount = 'Importo obbligatorio'
  instErrors.value = e
  return Object.keys(e).length === 0
}

function openInstModal(i = null) {
  editingInst.value = i?.id ?? null
  instErrors.value  = {}
  instForm.value = i
    ? { ...i, dueDate: i.dueDate?.slice(0, 10) ?? '' }
    : { fiscalYearId: selectedFiscalYearId.value, installmentNumber: 1, dueDate: '', totalAmount: 0, statusId: 2, notes: '' }
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
    await loadInstallments()
  } catch { store.toast('Errore nel salvataggio', 'error') } finally { savingInst.value = false }
}

async function deleteInst(id) {
  if (!confirm('Eliminare la rata?')) return
  try {
    await installmentApi.delete(id)
    store.toast('Rata eliminata', 'success')
    await loadInstallments()
  } catch { store.toast('Errore', 'error') }
}

// ── Quota CRUD ────────────────────────────────────────────────
async function openFeeModal(f = null, inst = null) {
  editingFee.value     = f?.id ?? null
  feeInstallment.value = inst
  feeModalMode.value   = f ? 'edit' : 'create'
  feeForm.value = f
    ? { amountDue: f.amountDue, notes: f.notes ?? '' }
    : { unitId: null, amountDue: 0, notes: '' }
  // Carica unità solo in fase di creazione
  if (!f && store.selectedCondominioId) {
    try {
      const { data } = await unitApi.getByCondominium(store.selectedCondominioId)
      units.value = data ?? []
    } catch { units.value = [] }
  }
  showFeeModal.value = true
}

function openPayModal(f, inst) {
  editingFee.value     = f.id
  feeInstallment.value = inst
  feeModalMode.value   = 'pay'
  feeForm.value = {
    amountDue:     f.amountDue,
    amountPaid:    f.amountPaid ?? 0,
    paymentAmount: 0,
    paymentDate:   new Date().toISOString().slice(0, 10),
    paymentMethod: f.paymentMethod ?? 'BankTransfer',
    notes:         f.notes ?? '',
  }
  showFeeModal.value = true
}

async function saveFee() {
  savingFee.value = true
  try {
    if (feeModalMode.value === 'pay' && editingFee.value) {
      await feeApi.recordPayment(
        editingFee.value,
        feeForm.value.paymentAmount,
        feeForm.value.paymentDate || new Date().toISOString().slice(0, 10),
        feeForm.value.paymentMethod || 'BankTransfer'
      )
    } else if (feeModalMode.value === 'edit' && editingFee.value) {
      await feeApi.update(editingFee.value, {
        amountDue: feeForm.value.amountDue,
        notes:     feeForm.value.notes,
      })
    } else if (feeModalMode.value === 'create' && feeInstallment.value) {
      await feeApi.create({
        installmentId: feeInstallment.value.id,
        condominiumId: store.selectedCondominioId,
        unitId:        feeForm.value.unitId,
        amountDue:     feeForm.value.amountDue,
        notes:         feeForm.value.notes,
      })
    }
    store.toast('Quota salvata', 'success')
    showFeeModal.value = false
    // Ricarica le quote della rata aperta
    const openId = expandedInstId.value
    if (openId) {
      expandedInstId.value = null
      fees.value = []
      const inst = installments.value.find(i => i.id === openId)
      if (inst) await toggleInstExpand(inst)
    }
  } catch { store.toast('Errore nel salvataggio', 'error') } finally { savingFee.value = false }
}

async function deleteFee(id) {
  if (!confirm('Eliminare questa quota?')) return
  try {
    await feeApi.delete(id)
    store.toast('Quota eliminata', 'success')
    // Ricarica le quote
    fees.value = fees.value.filter(f => f.id !== id)
  } catch { store.toast('Errore', 'error') }
}

// ── Watchers / Init ───────────────────────────────────────────
watch(() => store.selectedCondominioId, async () => {
  selectedFiscalYearId.value = null
  feeSummaries.value = {}
  await loadFiscalYears()
  await loadInstallments()
})
watch([selectedFiscalYearId, instFilter], loadInstallments)
onMounted(async () => {
  await loadFiscalYears()
  await loadInstallments()
})
</script>

<style scoped>
.tab-toolbar {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1rem;
  flex-wrap: wrap;
}

/* Riga rata cliccabile */
.inst-row { cursor: pointer; transition: background 0.1s; }
.inst-row:hover { background: var(--bg-hover, rgba(255,255,255,0.04)); }
.inst-row--expanded { background: var(--bg-surface-2, rgba(255,255,255,0.06)); }
.expand-cell { width: 2rem; text-align: center; }
.expand-icon { font-size: 0.72rem; color: var(--text-muted); }

/* Pannello quote inline */
.fees-row > td { padding: 0; }
.fees-body {
  background: var(--bg-inset, rgba(0,0,0,0.15));
  border-bottom: 2px solid var(--border);
  padding: 0 0 0.75rem 0;
}

.fees-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.6rem 1rem 0.5rem 2.5rem;
  border-bottom: 1px solid var(--border);
  margin-bottom: 0.25rem;
}
.fees-title {
  font-size: 0.82rem;
  font-weight: 600;
  color: var(--text-secondary);
  letter-spacing: 0.02em;
}

/* Tabella quote interna */
.inner-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}
.inner-table th {
  padding: 0.3rem 0.9rem;
  font-size: 0.73rem;
  font-weight: 600;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.04em;
  border-bottom: 1px solid var(--border);
  background: transparent;
}
.inner-table td {
  padding: 0.4rem 0.9rem;
  border-bottom: 1px solid var(--border-subtle, rgba(255,255,255,0.05));
}
.inner-table tr:last-child td { border-bottom: none; }

/* Riga totali */
.fees-total-row td {
  font-weight: 600;
  border-top: 1px solid var(--border);
  background: var(--bg-surface-2, rgba(255,255,255,0.04));
  padding: 0.35rem 0.9rem;
}

/* Cella unità */
.unit-number { font-family: var(--font-mono, monospace); font-size: 0.85rem; color: var(--text-secondary); margin-right: 0.4rem; }
.unit-display-name { font-size: 0.8rem; color: var(--text-muted); }

/* Colonne importo allineate a destra */
.text-right { text-align: right; }

.row-actions { display: flex; gap: 0.4rem; justify-content: flex-end; }

/* Form */
.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; margin-bottom: 0.75rem; }
.form-group { display: flex; flex-direction: column; gap: 0.3rem; }
.form-label { font-size: 0.8rem; font-weight: 500; color: var(--text-muted); }
.has-error .form-input, .has-error .form-select { border-color: var(--accent-red, #e53e3e); }
.field-error { font-size: 0.75rem; color: var(--accent-red, #f87171); }

/* Badge origine (auto / manuale) */
.origin-badge {
  display: inline-block;
  font-size: 0.62rem;
  font-weight: 700;
  letter-spacing: 0.04em;
  padding: 0.1rem 0.35rem;
  border-radius: 3px;
  margin-left: 0.4rem;
  vertical-align: middle;
  line-height: 1.4;
}
.origin-auto   { background: rgba(99, 179, 237, 0.15); color: #63b3ed; border: 1px solid rgba(99,179,237,0.3); }
.origin-manual { background: rgba(154, 154, 154, 0.1);  color: var(--text-muted); border: 1px solid rgba(154,154,154,0.25); }

/* Riepilogo pagamento nel modal */
.fee-pay-summary {
  background: var(--bg-surface-2, rgba(255,255,255,0.04));
  border: 1px solid var(--border);
  border-radius: 6px;
  padding: 0.75rem 1rem;
  margin-bottom: 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}
.fee-pay-row {
  display: flex;
  justify-content: space-between;
  font-size: 0.875rem;
}

/* Colori */
.text-green  { color: var(--accent-green, #22c55e); }
.text-amber  { color: var(--accent-amber, #f59e0b); }
.text-red    { color: var(--accent-red,   #ef4444); }
.text-secondary { color: var(--text-secondary); }
.text-muted  { color: var(--text-muted); }
.mono { font-family: var(--font-mono, monospace); }
</style>
