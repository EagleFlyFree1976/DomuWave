<template>
  <div>
    <div class="page-header">
      <h1>Rate</h1>
      <button v-if="canCreate" class="btn btn-primary" @click="openInstModal()">+ Nuova rata</button>
    </div>

    <!-- ── Banner filtro budget ────────────────────── -->
    <div v-if="budgetIdFilter" class="budget-filter-banner">
      <span>Stai visualizzando le rate del budget #{{ budgetIdFilter }}</span>
      <div class="budget-filter-actions">
        <router-link to="/budget" class="btn btn-sm btn-ghost">← Torna al budget</router-link>
        <button class="btn btn-sm btn-ghost" @click="clearBudgetFilter">Mostra tutte le rate</button>
      </div>
    </div>

    <!-- ── Toolbar ─────────────────────────────────── -->
    <div v-if="!budgetIdFilter" class="tab-toolbar">
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
      <div class="view-toggle">
        <button class="btn btn-sm" :class="viewMode === 'by-installment' ? 'btn-primary' : 'btn-ghost'" @click="setViewMode('by-installment')" title="Per rata">Per rata</button>
        <button class="btn btn-sm" :class="viewMode === 'by-unit' ? 'btn-primary' : 'btn-ghost'" @click="setViewMode('by-unit')" title="Per unità">Per unità</button>
      </div>
    </div>

    <!-- ══ Vista: Per rata ══════════════════════════════════════ -->
    <div v-if="viewMode === 'by-installment'" class="card">
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
                  <router-link v-if="inst.budgetId"
                    :to="{ path: '/rate', query: { budgetId: inst.budgetId } }"
                    class="origin-badge origin-auto"
                    title="Generata automaticamente da budget — clicca per filtrare"
                    @click.stop>
                    AUTO
                  </router-link>
                  <span v-else class="origin-badge origin-manual" title="Inserita manualmente">MAN</span>
                </td>
                <td class="text-secondary">{{ inst.fiscalYearCode ?? '—' }}</td>
                <td class="mono" :class="isOverdue(inst.dueDate) && inst.statusId !== 3 ? 'text-red' : 'text-secondary'">
                  {{ fmtDate(inst.dueDate) }}
                </td>
                <td class="mono text-right">{{ fmt(inst.totalAmount) }}</td>
                <td class="mono text-right text-green">{{ fmt(inst.totalPaid) }}</td>
                <td class="mono text-right text-amber">{{ fmt(inst.totalOverdue) }}</td>
                <td><span class="badge" :class="instBadge(inst.statusId)">{{ inst.statusName }}</span></td>
                <td @click.stop>
                  <div class="row-actions">
                    <button class="btn-icon" @click="downloadInstallmentNoticePdf(inst)" title="Scarica avviso PDF" :disabled="downloadingPdf === inst.id">
                      <span v-if="downloadingPdf === inst.id" class="spinner" style="width:12px;height:12px"></span>
                      <span v-else>⬇</span>
                    </button>
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

    <!-- ══ Vista: Per unità ════════════════════════════════════ -->
    <div v-else class="card">
      <div v-if="loadingByUnit" class="loading-state"><div class="spinner"></div></div>
      <div v-else-if="!unitGroups.length" class="empty-state">
        <div class="empty-icon">◷</div>
        <div>Nessuna quota trovata</div>
      </div>
      <div v-else class="table-wrap">
        <table>
          <thead>
            <tr>
              <th style="width:2rem"></th>
              <th>Unità</th>
              <th class="text-right">Dovuto</th>
              <th class="text-right">Pagato</th>
              <th class="text-right">Saldo</th>
              <th>Stato</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <template v-for="ug in unitGroups" :key="ug.unitId">

              <!-- Riga unità -->
              <tr
                class="inst-row"
                :class="{ 'inst-row--expanded': expandedUnitId === ug.unitId }"
                @click="expandedUnitId = expandedUnitId === ug.unitId ? null : ug.unitId"
              >
                <td class="expand-cell">
                  <span class="expand-icon">{{ expandedUnitId === ug.unitId ? '▾' : '▸' }}</span>
                </td>
                <td>
                  <span class="unit-number">{{ ug.unitInternalNumber }}</span>
                  <span v-if="ug.unitDisplayName" class="unit-display-name">{{ ug.unitDisplayName }}</span>
                </td>
                <td class="mono text-right">{{ fmt(ug.totalDue) }}</td>
                <td class="mono text-right text-green">{{ fmt(ug.totalPaid) }}</td>
                <td class="mono text-right" :class="ug.totalBalance > 0 ? 'text-amber' : 'text-green'">{{ fmt(ug.totalBalance) }}</td>
                <td>
                  <span v-if="ug.totalBalance <= 0" class="badge badge-green">Saldata</span>
                  <span v-else-if="ug.hasOverdue" class="badge badge-red">In ritardo</span>
                  <span v-else class="badge badge-amber">Aperta</span>
                </td>
                <td @click.stop>
                  <div class="row-actions">
                    <button class="btn-icon" @click="downloadUnitNoticePdf(ug)" title="Scarica avviso PDF" :disabled="downloadingPdf === ug.unitId">
                      <span v-if="downloadingPdf === ug.unitId" class="spinner" style="width:12px;height:12px"></span>
                      <span v-else>⬇</span>
                    </button>
                  </div>
                </td>
              </tr>

              <!-- Dettaglio per rata -->
              <tr v-if="expandedUnitId === ug.unitId" class="fees-row">
                <td colspan="7" class="fees-body">
                  <div class="fees-header">
                    <span class="fees-title">Quote per rata – {{ ug.unitInternalNumber }}{{ ug.unitDisplayName ? ` – ${ug.unitDisplayName}` : '' }}</span>
                  </div>
                  <table class="inner-table">
                    <thead>
                      <tr>
                        <th>N° rata</th>
                        <th>Scadenza</th>
                        <th class="text-right">Dovuto</th>
                        <th class="text-right">Pagato</th>
                        <th class="text-right">Saldo</th>
                        <th>Stato</th>
                        <th>Data pagamento</th>
                        <th></th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="f in ug.fees" :key="f.id">
                        <td class="mono text-muted">{{ f.installmentNumber }}</td>
                        <td class="mono text-secondary" style="font-size:0.82rem">{{ fmtDate(f.dueDate) }}</td>
                        <td class="mono text-right">{{ fmt(f.amountDue) }}</td>
                        <td class="mono text-right text-green">{{ fmt(f.amountPaid) }}</td>
                        <td class="mono text-right" :class="f.balance > 0 ? 'text-amber' : 'text-green'">{{ fmt(f.balance) }}</td>
                        <td><span class="badge" :class="feeBadge(f.paymentStatus)">{{ feeStatusLabel(f.paymentStatus) }}</span></td>
                        <td class="text-secondary mono" style="font-size:0.82rem">{{ fmtDate(f.paymentDate) }}</td>
                        <td>
                          <div class="row-actions">
                            <button v-if="canEdit" class="btn-icon" @click="openPayModal(f, f._installment)" title="Registra pagamento" style="color:var(--accent-green)">€</button>
                            <button v-if="canEdit" class="btn-icon" @click="openFeeModal(f, f._installment)" title="Modifica">✎</button>
                            <button v-if="canDelete" class="btn-icon" @click="deleteFeeFromUnitView(f.id, ug.unitId)" style="color:var(--accent-red)">✕</button>
                          </div>
                        </td>
                      </tr>
                    </tbody>
                    <tfoot>
                      <tr class="fees-total-row">
                        <td colspan="2" class="text-muted" style="font-size:0.8rem">Totale</td>
                        <td class="mono text-right">{{ fmt(ug.totalDue) }}</td>
                        <td class="mono text-right text-green">{{ fmt(ug.totalPaid) }}</td>
                        <td class="mono text-right text-amber">{{ fmt(ug.totalBalance) }}</td>
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
                <button type="button" class="btn btn-ghost btn-sm" style="margin-top:6px" @click="feeForm.paymentAmount = feeForm.amountDue - feeForm.amountPaid">
                  Salda l'intero importo
                </button>
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
import { useRoute, useRouter } from 'vue-router'
import { useAppStore } from '@/stores/app'
import { installmentApi, feeApi, unitApi } from '@/services/api'
import { usePermissions } from '@/composables/usePermissions'

const route  = useRoute()
const router = useRouter()
const store  = useAppStore()
const { canCreate, canEdit, canDelete } = usePermissions()

// ── Esercizi fiscali — dallo store globale ────────────────────
const fiscalYears          = computed(() => store.fiscalYears)
const selectedFiscalYearId = computed({
  get: () => store.selectedFiscalYearId,
  set: (v) => { store.selectedFiscalYearId = v },
})

// ── Modalità visualizzazione ──────────────────────────────────
const viewMode = ref('by-installment') // 'by-installment' | 'by-unit'

function setViewMode(mode) {
  viewMode.value = mode
  if (mode === 'by-unit') loadByUnit()
}

// ── Filtro budget (da query param ?budgetId=) ─────────────────
const budgetIdFilter = computed(() => route.query.budgetId ? Number(route.query.budgetId) : null)

function clearBudgetFilter() {
  router.replace({ query: {} })
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
async function loadInstallments(preserveExpanded = false) {
  if (!store.selectedCondominioId) return
  loadingInst.value = true
  if (!preserveExpanded) {
    expandedInstId.value = null
    fees.value = []
  }
  try {
    let data
    if (budgetIdFilter.value) {
      ({ data } = await installmentApi.getByBudget(budgetIdFilter.value))
    } else if (instFilter.value === 'open') {
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
    if (viewMode.value === 'by-unit') {
      const openUnitId = expandedUnitId.value
      await loadByUnit()
      expandedUnitId.value = openUnitId
    } else {
      // Ricarica le rate (aggiorna totalPaid/totalOverdue in testata) preservando l'espansione
      const openId = expandedInstId.value
      await loadInstallments(true)
      if (openId) {
        expandedInstId.value = null
        fees.value = []
        const inst = installments.value.find(i => i.id === openId)
        if (inst) await toggleInstExpand(inst)
      }
    }
  } catch { store.toast('Errore nel salvataggio', 'error') } finally { savingFee.value = false }
}

async function deleteFee(id) {
  if (!confirm('Eliminare questa quota?')) return
  try {
    await feeApi.delete(id)
    store.toast('Quota eliminata', 'success')
    const openId = expandedInstId.value
    await loadInstallments(true)
    if (openId) {
      expandedInstId.value = null
      fees.value = []
      const inst = installments.value.find(i => i.id === openId)
      if (inst) await toggleInstExpand(inst)
    }
  } catch { store.toast('Errore', 'error') }
}

// ── Vista per unità ───────────────────────────────────────────
const allUnitFees    = ref([])   // tutte le fee di tutte le rate, arricchite con info rata
const loadingByUnit  = ref(false)
const expandedUnitId = ref(null)

const unitGroups = computed(() => {
  const map = new Map()
  for (const f of allUnitFees.value) {
    if (!map.has(f.unitId)) {
      map.set(f.unitId, {
        unitId:              f.unitId,
        unitInternalNumber:  f.unitInternalNumber ?? String(f.unitId),
        unitDisplayName:     f.unitDisplayName ?? '',
        totalDue:            0,
        totalPaid:           0,
        totalBalance:        0,
        hasOverdue:          false,
        fees:                [],
      })
    }
    const g = map.get(f.unitId)
    g.totalDue     += f.amountDue     ?? 0
    g.totalPaid    += f.amountPaid    ?? 0
    g.totalBalance += f.balance       ?? 0
    if (f.paymentStatus === 'Overdue') g.hasOverdue = true
    g.fees.push(f)
  }
  return [...map.values()].sort((a, b) =>
    a.unitInternalNumber.localeCompare(b.unitInternalNumber, 'it', { numeric: true })
  )
})

async function loadByUnit() {
  if (!store.selectedCondominioId) return
  loadingByUnit.value = true
  expandedUnitId.value = null
  allUnitFees.value = []
  try {
    // Prima carica tutte le rate (riutilizza la lista già caricata se disponibile)
    let insts = installments.value
    if (!insts.length) {
      let data
      if (selectedFiscalYearId.value) {
        ({ data } = await installmentApi.getByFiscalYear(store.selectedCondominioId, selectedFiscalYearId.value))
      } else {
        ({ data } = await installmentApi.getByCondominium(store.selectedCondominioId))
      }
      insts = data ?? []
    }

    // Carica le quote di ogni rata in parallelo
    const results = await Promise.all(
      insts.map(inst =>
        feeApi.getByInstallment(inst.id)
          .then(({ data }) => (data ?? []).map(f => ({
            ...f,
            installmentNumber: inst.installmentNumber,
            dueDate:           inst.dueDate,
            _installment:      inst,
          })))
          .catch(() => [])
      )
    )
    allUnitFees.value = results.flat()
  } finally {
    loadingByUnit.value = false
  }
}

async function deleteFeeFromUnitView(feeId, unitId) {
  if (!confirm('Eliminare questa quota?')) return
  try {
    await feeApi.delete(feeId)
    store.toast('Quota eliminata', 'success')
    allUnitFees.value = allUnitFees.value.filter(f => f.id !== feeId)
  } catch { store.toast('Errore', 'error') }
}

// ── Download PDF ──────────────────────────────────────────────
const downloadingPdf = ref(null)  // installmentId or unitId while downloading

function triggerBlobDownload(blob, filename) {
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  a.click()
  URL.revokeObjectURL(url)
}

async function downloadInstallmentNoticePdf(inst) {
  downloadingPdf.value = inst.id
  try {
    const { data } = await feeApi.getInstallmentBatchNoticePdf(inst.id)
    triggerBlobDownload(data, `avviso-rata-${inst.installmentNumber}.pdf`)
  } catch { store.toast('Errore nel download del PDF', 'error') }
  finally { downloadingPdf.value = null }
}

async function downloadUnitNoticePdf(ug) {
  if (!selectedFiscalYearId.value) {
    store.toast('Seleziona un esercizio fiscale per scaricare l\'avviso', 'error')
    return
  }
  downloadingPdf.value = ug.unitId
  try {
    const { data } = await feeApi.getUnitNoticePdf(ug.unitId, selectedFiscalYearId.value)
    triggerBlobDownload(data, `avviso-unita-${ug.unitInternalNumber}.pdf`)
  } catch { store.toast('Errore nel download del PDF', 'error') }
  finally { downloadingPdf.value = null }
}

// ── Watchers / Init ───────────────────────────────────────────
watch(() => store.selectedCondominioId, async () => {
  allUnitFees.value  = []
  // loadFiscalYears è gestito dal watch in appStore; attende che si aggiorni
  await loadInstallments()
  if (viewMode.value === 'by-unit') await loadByUnit()
})
watch(() => [store.selectedFiscalYearId, instFilter.value, route.query.budgetId], async () => {
  await loadInstallments()
  if (viewMode.value === 'by-unit') await loadByUnit()
})
onMounted(async () => {
  if (!store.fiscalYears.length) await store.loadFiscalYears()
  await loadInstallments()
})
</script>

<style scoped>
.budget-filter-banner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 0.5rem;
  padding: 0.6rem 1rem;
  margin-bottom: 0.75rem;
  background: color-mix(in srgb, var(--accent) 8%, transparent);
  border: 1px solid color-mix(in srgb, var(--accent) 30%, transparent);
  border-radius: 8px;
  font-size: 0.875rem;
  color: var(--text-secondary);
}
.budget-filter-actions { display: flex; gap: 0.4rem; }

.tab-toolbar {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1rem;
  flex-wrap: wrap;
}

.view-toggle {
  display: flex;
  gap: 0;
  margin-left: auto;
  border: 1px solid var(--border);
  border-radius: 6px;
  overflow: hidden;
}
.view-toggle .btn {
  border-radius: 0;
  border: none;
  padding: 0.3rem 0.75rem;
  font-size: 0.8rem;
}
.view-toggle .btn + .btn {
  border-left: 1px solid var(--border);
}

/* Riga rata cliccabile */
.inst-row { cursor: pointer; transition: background 0.1s; }
.inst-row:hover { background: var(--bg-hover, rgba(255,255,255,0.04)); }
.inst-row--expanded { background: var(--bg-surface-2, rgba(255,255,255,0.06)); }
.expand-cell { width: 2rem; text-align: center; }
.expand-icon { font-size: 0.72rem; color: var(--text-muted); }

/* Riga rata: font leggermente più grande e peso maggiore per distinguerla */
.inst-row td { font-size: 0.9rem; }

/* Pannello quote inline */
.fees-row > td { padding: 0; }
.fees-body {
  background: var(--bg-inset, rgba(0,0,0,0.22));
  border-left: 3px solid var(--accent-blue, #4299e1);
  border-bottom: 2px solid var(--border);
  padding: 0 0 0.5rem 0;
}

.fees-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.5rem 1rem 0.45rem 1.5rem;
  border-bottom: 1px solid var(--border);
  margin-bottom: 0;
  background: rgba(66, 153, 225, 0.06);
}
.fees-title {
  font-size: 0.78rem;
  font-weight: 600;
  color: var(--accent-blue, #4299e1);
  letter-spacing: 0.03em;
  text-transform: uppercase;
}

/* Tabella quote interna */
.inner-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.82rem;
}
.inner-table th {
  padding: 0.28rem 0.9rem 0.28rem 1.5rem;
  font-size: 0.68rem;
  font-weight: 600;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  border-bottom: 1px solid var(--border);
  background: transparent;
}
.inner-table th:first-child { padding-left: 1.5rem; }
.inner-table td {
  padding: 0.35rem 0.9rem;
  border-bottom: 1px solid var(--border-subtle, rgba(255,255,255,0.04));
  color: var(--text-secondary);
}
.inner-table td:first-child { padding-left: 1.5rem; }
.inner-table tbody tr:hover td { background: rgba(255,255,255,0.02); }
.inner-table tr:last-child td { border-bottom: none; }

/* Riga totali */
.fees-total-row td {
  font-size: 0.8rem;
  font-weight: 600;
  border-top: 1px solid var(--border);
  background: rgba(0,0,0,0.15);
  padding: 0.3rem 0.9rem;
}
.fees-total-row td:first-child { padding-left: 1.5rem; }

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
