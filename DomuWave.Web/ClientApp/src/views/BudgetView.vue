<template>
  <div>
    <!-- ── Header ──────────────────────────────────────── -->
    <div class="page-header">
      <h1>Budget</h1>
    </div>

    <div>
      <div class="tab-toolbar">
        <!-- FiscalYear selector -->
        <select class="form-select" v-model="selectedFiscalYearId" style="min-width:180px"
                :disabled="!fiscalYears.length">
          <option :value="null" disabled>Seleziona esercizio…</option>
          <option v-for="fy in fiscalYears" :key="fy.id" :value="fy.id">
            {{ fy.code }}{{ fy.description ? ' – ' + fy.description : '' }}
          </option>
        </select>

        <button v-if="canCreate && canCreateBudget" class="btn btn-primary" style="margin-left:auto"
                @click="openBudgetModal()">
          + Nuovo budget
        </button>
      </div>

      <div class="card">
        <div v-if="loadingBudget" class="loading-state"><div class="spinner"></div></div>
        <div v-else-if="!selectedFiscalYearId" class="empty-state">
          <div class="empty-icon">◎</div>
          <div>Seleziona un esercizio fiscale per vedere i budget</div>
        </div>
        <div v-else-if="!budgets.length" class="empty-state">
          <div class="empty-icon">◎</div>
          <div>Nessun budget per questo esercizio</div>
        </div>
        <div v-else class="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Tipo</th>
                <th class="text-right">Entrate</th>
                <th class="text-right">Uscite</th>
                <th>Approvazione</th>
                <th>Stato</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="b in budgets" :key="b.id">
                <td><span class="text-muted mono">#{{ b.id }}</span> {{ b.type === 1 ? 'Preventivo' : 'Consuntivo' }}</td>
                <td class="mono text-right text-green">{{ fmt(b.totalIncome) }}</td>
                <td class="mono text-right text-red">{{ fmt(b.totalExpenses) }}</td>
                <td class="text-secondary">{{ fmtDate(b.approvalDate) }}</td>
                <td><span class="badge" :class="statusBadge(b.statusId)">{{ statusLabel(b.statusId) }}</span></td>
                <td>
                  <div class="row-actions">
                    <button class="btn btn-sm btn-ghost" @click="openItemsModal(b)">Voci</button>
                    <button v-if="b.type === 1 && (b.statusId === 2 || b.statusId === 3)"
                            class="btn btn-sm btn-ghost" @click="goToRate(b)" title="Vai alle rate generate">Rate ↗</button>
                    <button v-if="canEdit && b.statusId === 1 && !blockedTypes.has(b.type)"
                            class="btn btn-sm btn-ghost"
                            @click="openApproveModal(b)">Approva</button>
                    <span v-if="canEdit && b.statusId === 1 && blockedTypes.has(b.type)"
                          class="badge badge-muted" title="Esiste già un budget di questo tipo approvato o chiuso">
                      Non approvabile
                    </span>
                    <button v-if="canEdit && b.statusId === 2" class="btn btn-sm btn-ghost"
                            @click="closeBudget(b)">Chiudi</button>
                    <button v-if="session.isSuperAdmin && b.statusId === 2" class="btn btn-sm btn-ghost btn-reopen"
                            @click="reopenBudget(b)" title="Riapri in bozza (SuperAdmin)">Riapri</button>
                    <button v-if="canEdit && b.statusId === 2 && b.type === 1"
                            class="btn btn-sm btn-ghost"
                            @click="openGenerateModal(b)">Genera rate</button>
                    <button v-if="canEdit && b.type === 2 && b.statusId !== 3"
                            class="btn btn-sm btn-ghost"
                            @click="recalculateConsuntivoItems(b)" title="Ricalcola voci da spese registrate">Ricalcola voci</button>
                    <button v-if="canEdit && b.statusId === 1"
                            class="btn btn-sm btn-ghost"
                            @click="fixOrphanItems(b)" title="Aggiunge gli antenati mancanti alle voci orfane">Ripara gerarchia</button>
                    <button v-if="canEdit && b.statusId === 1" class="btn-icon"
                            @click="openBudgetModal(b)" title="Modifica">✎</button>
                    <button v-if="canDelete && b.statusId === 1" class="btn-icon"
                            style="color:var(--accent-red)"
                            @click="deleteBudget(b.id)" title="Elimina">✕</button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- ══════════════════════════════════════════════════
         MODAL — Approvazione Budget + Rate
    ══════════════════════════════════════════════════ -->
    <div class="modal-overlay" v-if="showApproveModal" @click.self="showApproveModal = false">
      <div class="modal">
        <div class="modal-header">
          <h2>Approva budget</h2>
          <button class="btn-icon" @click="showApproveModal = false">✕</button>
        </div>
        <div class="modal-body">
          <p class="approve-info">
            Approvando il budget verranno generate automaticamente le rate e le relative quote per ogni unità
            in base alla tabella millesimale attiva.
          </p>
          <div class="form-grid">
            <div class="form-group">
              <label class="form-label">Numero di rate *</label>
              <input class="form-input" type="number" min="1" max="24"
                     v-model.number="approveForm.numberOfInstallments" />
            </div>
            <div class="form-group">
              <label class="form-label">Prima scadenza *</label>
              <input class="form-input" type="date" v-model="approveForm.firstDueDate" />
            </div>
          </div>
          <p class="approve-hint">
            Le scadenze successive saranno distanziate di un mese l'una dall'altra.
          </p>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showApproveModal = false">Annulla</button>
          <button class="btn btn-primary" @click="confirmApprove" :disabled="savingApprove">
            <span v-if="savingApprove" class="spinner" style="width:14px;height:14px"></span>
            Approva e genera rate
          </button>
        </div>
      </div>
    </div>

    <!-- ══════════════════════════════════════════════════
         MODAL — Genera rate da budget
    ══════════════════════════════════════════════════ -->
    <div class="modal-overlay" v-if="showGenerateModal" @click.self="showGenerateModal = false">
      <div class="modal">
        <div class="modal-header">
          <h2>Genera rate</h2>
          <button class="btn-icon" @click="showGenerateModal = false">✕</button>
        </div>
        <div class="modal-body">
          <div v-if="generateHasExisting" class="generate-warning">
            <span class="generate-warning-icon">⚠</span>
            <div>
              <strong>Attenzione: rate già generate</strong><br>
              Le rate esistenti per questo budget verranno eliminate e rigenerate.
              Questa operazione non può essere annullata.
            </div>
          </div>
          <p v-else class="approve-info">
            Verranno generate le rate e le relative quote per ogni unità
            in base alla tabella millesimale attiva.
          </p>
          <div class="form-grid">
            <div class="form-group">
              <label class="form-label">Numero di rate *</label>
              <input class="form-input" type="number" min="1" max="24"
                     v-model.number="generateForm.numberOfInstallments" />
            </div>
            <div class="form-group">
              <label class="form-label">Prima scadenza *</label>
              <input class="form-input" type="date" v-model="generateForm.firstDueDate" />
            </div>
          </div>
          <p class="approve-hint">
            Le scadenze successive saranno distanziate di un mese l'una dall'altra.
          </p>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showGenerateModal = false">Annulla</button>
          <button class="btn btn-primary" :class="{ 'btn-danger': generateHasExisting }"
                  @click="confirmGenerate" :disabled="savingGenerate">
            <span v-if="savingGenerate" class="spinner" style="width:14px;height:14px"></span>
            {{ generateHasExisting ? 'Rigenera rate' : 'Genera rate' }}
          </button>
        </div>
      </div>
    </div>

    <!-- ══════════════════════════════════════════════════
         MODAL — Crea / Modifica Budget
    ══════════════════════════════════════════════════ -->
    <BaseModal
      :show="showBudgetModal"
      @close="showBudgetModal = false"
      :title="editingBudget ? 'Modifica budget' : 'Nuovo budget'"
      :subtitle="store.selectedCondominio?.name"
    >
      <div class="form-grid">
        <div class="form-group" v-if="!editingBudget">
          <label class="form-label">Tipo *</label>
          <select class="form-select" v-model.number="budgetForm.type">
            <option :value="1" :disabled="existingTypes.has(1)">
              Preventivo{{ existingTypes.has(1) ? ' (già presente)' : '' }}
            </option>
            <option :value="2" :disabled="existingTypes.has(2)">
              Consuntivo{{ existingTypes.has(2) ? ' (già presente)' : '' }}
            </option>
          </select>
        </div>
      </div>

      <!-- ── Sezione base da consuntivo (solo Preventivo in creazione) ── -->
      <template v-if="!editingBudget && budgetForm.type === 1">
        <fieldset class="form-fieldset" style="margin-top:12px">
          <legend class="form-fieldset-legend">Importa da consuntivo precedente (opzionale)</legend>
          <div class="form-grid">
            <div class="form-group form-group--full">
              <label class="form-label">Budget consuntivo di base</label>
              <select class="form-select" v-model.number="budgetForm.sourceConsuntivoId">
                <option :value="null">— Nessuno (voci vuote) —</option>
                <option v-for="c in availableConsuntivi" :key="c.id" :value="c.id">
                  {{ c.fiscalYearCode || c.fiscalYearId }} · {{ c.statusName }}
                  {{ c.totalExpenses != null ? ' · €' + c.totalExpenses.toLocaleString('it-IT', {minimumFractionDigits:2}) : '' }}
                </option>
              </select>
            </div>
            <div class="form-group" v-if="budgetForm.sourceConsuntivoId">
              <label class="form-label">Maggiorazione %</label>
              <input class="form-input" type="number" step="0.1" min="-100" max="999"
                     v-model.number="budgetForm.increasePercentage"
                     placeholder="es. 5 = +5%" />
            </div>
          </div>
        </fieldset>
      </template>

      <div class="form-group" style="margin-top:12px">
        <label class="form-label">Note</label>
        <textarea class="form-textarea" v-model="budgetForm.notes" rows="2"></textarea>
      </div>
      <template #footer>
        <button class="btn btn-ghost" @click="showBudgetModal = false">Annulla</button>
        <button class="btn btn-primary" @click="saveBudget" :disabled="savingBudget">
          <span v-if="savingBudget" class="spinner" style="width:14px;height:14px"></span>
          {{ editingBudget ? 'Salva' : 'Crea' }}
        </button>
      </template>
    </BaseModal>

    <!-- ══════════════════════════════════════════════════
         MODAL — Voci di Budget (Piano dei conti completo)
    ══════════════════════════════════════════════════ -->
    <BaseModal :show="showItemsModal" @close="closeItemsModal" size="lg">
      <template #title>Voci di budget</template>
      <template #subtitle>
        <span v-if="store.selectedCondominio" class="modal-condominium">{{ store.selectedCondominio.name }}</span>
        <span v-if="store.selectedCondominio && selectedBudget"> · </span>
        <span v-if="selectedBudget">{{ selectedBudget.type === 1 ? 'Preventivo' : 'Consuntivo' }}</span>
        <span v-if="selectedBudget?.fiscalYearCode"> · {{ selectedBudget.fiscalYearCode }}</span>
        <span v-if="selectedBudget" class="badge ml-2" :class="statusBadge(selectedBudget.statusId)">
          {{ statusLabel(selectedBudget.statusId) }}
        </span>
      </template>

      <div v-if="loadingItems" class="loading-state"><div class="spinner"></div></div>
      <div v-else-if="budgetTabs.length">

        <!-- ── Riepilogo totali (fisso in cima) ── -->
        <div class="budget-summary-top">
          <div class="summary-pill summary-pill-green">
            <span class="summary-pill-label">Entrate + Patrim.</span>
            <span class="summary-pill-value mono">{{ fmt(totalIncome) }}</span>
          </div>
          <div class="summary-pill summary-pill-red">
            <span class="summary-pill-label">Uscite</span>
            <span class="summary-pill-value mono">{{ fmt(totalExpenses) }}</span>
          </div>
          <div class="summary-pill" :class="totalBalance >= 0 ? 'summary-pill-green' : 'summary-pill-red'">
            <span class="summary-pill-label">Saldo</span>
            <span class="summary-pill-value mono">{{ fmt(totalBalance) }}</span>
          </div>
          <!-- Salva bar inline -->
          <div v-if="hasDirtyRows && canEdit && selectedBudget?.statusId === 1" class="summary-save-actions">
            <button class="btn btn-ghost btn-sm" @click="discardChanges">Annulla</button>
            <button class="btn btn-primary btn-sm" @click="saveAllItems" :disabled="savingItem">
              <span v-if="savingItem" class="spinner" style="width:12px;height:12px"></span>
              Salva
            </button>
          </div>
          <div v-if="canEdit && selectedBudget?.statusId === 1"
               :class="hasDirtyRows ? '' : 'summary-add-action'">
            <button class="btn btn-sm btn-ghost" @click="openAddAccountsModal">+ Aggiungi voci</button>
          </div>
        </div>

        <!-- ── Tab: Entrate / Uscite / Patrimoniale ── -->
        <div class="budget-tabs">
          <button
            v-for="tab in budgetTabs" :key="tab.id"
            class="budget-tab-btn"
            :class="{ active: activeItemTab === tab.id }"
            @click="activeItemTab = tab.id"
          >
            <span class="tab-label">{{ tab.label }}</span>
            <span class="tab-total mono" :class="tab.id === 'Uscita' ? 'text-red' : 'text-green'">
              {{ fmt(tab.total) }}
            </span>
          </button>
        </div>

        <!-- ── Tabella righe del tab attivo (gerarchica) ── -->
        <div class="budget-tab-content">
          <table class="budget-accounts-table" v-if="activeTabRows.length">
            <thead>
              <tr>
                <th style="width:90px">Codice</th>
                <th>Voce</th>
                <th style="width:140px" class="text-right">Competenza (€)</th>
                <th v-if="selectedBudget?.type === 2" style="width:140px" class="text-right">Pagato (€)</th>
                <th v-if="selectedBudget?.type === 2" style="width:120px" class="text-right">Delta (€)</th>
                <th v-if="canEdit && selectedBudget?.statusId === 1" style="width:36px"></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in activeTabRows" :key="row.accountId"
                  :class="{
                    'row-edited':  row.dirty,
                    'row-header':  row.isHeader,
                    'row-leaf':    !row.isHeader,
                  }">
                <td class="mono text-secondary" style="font-size:0.82em">{{ row.accountCode }}</td>
                <td :style="{ paddingLeft: (0.65 + (row.level - 1) * 1.25) + 'rem' }">
                  {{ row.accountName }}
                </td>
                <td class="text-right">
                  <input
                    v-if="!row.isHeader && canEdit && selectedBudget?.statusId === 1"
                    class="form-input budget-amount-input"
                    type="number" step="0.01" min="0"
                    :value="row.amount"
                    @change="onAmountChange(row, $event)"
                    @focus="$event.target.select()"
                  />
                  <span v-else class="mono" :class="row.isHeader ? 'row-header-amount' : ''">
                    {{ fmt(row.amount) }}
                  </span>
                </td>
                <td v-if="selectedBudget?.type === 2" class="text-right">
                  <span class="mono" :class="row.isHeader ? 'row-header-amount' : ''">
                    {{ fmt(row.amountPaid) }}
                  </span>
                </td>
                <td v-if="selectedBudget?.type === 2" class="text-right">
                  <span class="mono delta-cell"
                        :class="(row.amount - row.amountPaid) > 0 ? 'delta-positive' : (row.amount - row.amountPaid) < 0 ? 'delta-negative' : 'delta-zero'">
                    {{ fmt(row.amount - row.amountPaid) }}
                  </span>
                </td>
                <td v-if="canEdit && selectedBudget?.statusId === 1">
                  <button
                    v-if="!row.isHeader && row.itemId"
                    class="btn-icon btn-delete-row"
                    title="Rimuovi voce"
                    @click="deleteItem(row)"
                  >✕</button>
                </td>
              </tr>
            </tbody>
            <tfoot>
              <tr class="section-total-row">
                <td colspan="2" class="text-secondary">Totale</td>
                <td class="mono text-right">{{ fmt(activeTabRows.filter(r => !r.isHeader).reduce((s, r) => s + (r.amount || 0), 0)) }}</td>
                <td v-if="selectedBudget?.type === 2" class="mono text-right">
                  {{ fmt(activeTabRows.filter(r => !r.isHeader).reduce((s, r) => s + (r.amountPaid || 0), 0)) }}
                </td>
                <td v-if="selectedBudget?.type === 2" class="mono text-right">
                  {{ fmt(activeTabRows.filter(r => !r.isHeader).reduce((s, r) => s + ((r.amount || 0) - (r.amountPaid || 0)), 0)) }}
                </td>
              </tr>
            </tfoot>
          </table>
          <div v-else class="empty-state" style="padding:1rem">
            <div class="empty-icon">◎</div>
            <div>Nessuna voce in questa sezione</div>
          </div>
        </div>
      </div>
      <div v-else class="empty-state" style="padding:2rem">
        <div class="empty-icon">◎</div>
        <div>Nessuna voce di budget</div>
      </div>

      <template #footer>
        <button class="btn btn-ghost" @click="closeItemsModal">Chiudi</button>
      </template>
    </BaseModal>

    <!-- ══════════════════════════════════════════════════
         MODAL — Seleziona conti da aggiungere al budget
    ══════════════════════════════════════════════════ -->
    <Teleport to="body">
    <div class="modal-overlay add-accounts-overlay" v-if="showAddAccountsModal" @click.self="showAddAccountsModal = false">
      <div class="modal modal-lg">
        <div class="modal-header">
          <h2>Aggiungi voci dal piano dei conti</h2>
          <button class="btn-icon" @click="showAddAccountsModal = false">✕</button>
        </div>
        <div class="modal-body">
          <div v-if="loadingAccounts" class="loading-state"><div class="spinner"></div></div>
          <div v-else-if="!addAccountsTree.length" class="empty-state">
            <div class="empty-icon">◎</div>
            <div>Nessun conto disponibile da aggiungere</div>
          </div>
          <template v-else>
            <p class="add-accounts-hint">Seleziona i conti da aggiungere. I conti già presenti nel budget non sono selezionabili.</p>
            <div class="add-accounts-tree">
              <div
                v-for="node in addAccountsTree"
                :key="node.id"
                class="coa-node"
                :class="{ 'coa-node-header': node.hasChildren, 'coa-node-disabled': node.alreadyPresent }"
                :style="{ paddingLeft: (0.5 + (node.level - 1) * 1.5) + 'rem' }"
              >
                <label v-if="!node.hasChildren" class="coa-checkbox-label">
                  <input
                    type="checkbox"
                    :disabled="node.alreadyPresent"
                    v-model="addAccountsSelected"
                    :value="node.id"
                  />
                  <span class="coa-code mono">{{ node.code }}</span>
                  <span class="coa-name">{{ node.name }}</span>
                  <span v-if="node.alreadyPresent" class="badge badge-muted" style="margin-left:auto;font-size:0.72rem">già presente</span>
                </label>
                <div v-else class="coa-group-label">
                  <span class="coa-code mono">{{ node.code }}</span>
                  <span class="coa-name">{{ node.name }}</span>
                </div>
              </div>
            </div>
          </template>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showAddAccountsModal = false">Annulla</button>
          <button class="btn btn-primary" @click="confirmAddAccounts"
                  :disabled="!addAccountsSelected.length || savingAddAccounts">
            <span v-if="savingAddAccounts" class="spinner" style="width:14px;height:14px"></span>
            Aggiungi ({{ addAccountsSelected.length }})
          </button>
        </div>
      </div>
    </div>
    </Teleport>

  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAppStore } from '@/stores/app'
import { useSessionStore } from '@/stores/sessionStore'
import { budgetApi, budgetItemApi, chartOfAccountsApi, installmentApi } from '@/services/api'
import { usePermissions } from '@/composables/usePermissions'
import { useDeepLink }   from '@/composables/useDeepLink'
import BaseModal from '@/components/BaseModal.vue'

const router  = useRouter()
const route   = useRoute()
const { resolveContext } = useDeepLink()
const store   = useAppStore()
const session = useSessionStore()
const { canCreate, canEdit, canDelete } = usePermissions()

function goToRate(b) {
  router.push({ path: '/rate', query: { budgetId: b.id } })
}

// ─── Fiscal Years — dallo store globale ───────────────────────
const fiscalYears          = computed(() => store.fiscalYears)
const selectedFiscalYearId = computed({
  get: () => store.selectedFiscalYearId,
  set: (v) => { store.selectedFiscalYearId = v },
})

// ─── Budget ───────────────────────────────────────────────────
const budgets        = ref([])
const loadingBudget  = ref(false)
const showBudgetModal     = ref(false)
const editingBudget       = ref(null)
const savingBudget        = ref(false)
const budgetForm          = ref({ type: 1, notes: '', sourceConsuntivoId: null, increasePercentage: 0 })
const availableConsuntivi = ref([])

async function loadBudgets() {
  if (!selectedFiscalYearId.value) { budgets.value = []; return }
  loadingBudget.value = true
  try {
    const { data } = await budgetApi.getByFiscalYear(selectedFiscalYearId.value)
    budgets.value = data ?? []
  } catch { budgets.value = [] } finally { loadingBudget.value = false }
}

async function openBudgetModal(b = null) {
  editingBudget.value = b?.id ?? null
  if (b) {
    budgetForm.value = { notes: b.notes ?? '', sourceConsuntivoId: null, increasePercentage: 0 }
  } else {
    const defaultType = !existingTypes.value.has(1) ? 1 : 2
    budgetForm.value = { type: defaultType, notes: '', sourceConsuntivoId: null, increasePercentage: 0 }
    // Carica i consuntivi disponibili come base (tutti gli esercizi del condominio)
    try {
      const { data } = await budgetApi.getByCondominium(store.selectedCondominioId)
      availableConsuntivi.value = (data ?? []).filter(b => b.type === 2)
    } catch { availableConsuntivi.value = [] }
  }
  showBudgetModal.value = true
}

async function saveBudget() {
  savingBudget.value = true
  try {
    if (editingBudget.value) {
      await budgetApi.update(editingBudget.value, { notes: budgetForm.value.notes })
    } else {
      await budgetApi.create({
        condominiumId:       store.selectedCondominioId,
        fiscalYearId:        selectedFiscalYearId.value,
        type:                budgetForm.value.type,
        notes:               budgetForm.value.notes,
        sourceConsuntivoId:  budgetForm.value.type === 1 ? (budgetForm.value.sourceConsuntivoId || null) : null,
        increasePercentage:  budgetForm.value.type === 1 ? (budgetForm.value.increasePercentage || 0) : 0,
      })
    }
    showBudgetModal.value = false
    await loadBudgets()
  } catch { /* toast handled globally */ } finally { savingBudget.value = false }
}

// Tipi già approvati o chiusi (non approvabili di nuovo)
const blockedTypes = computed(() => {
  const s = new Set()
  for (const b of budgets.value) {
    if (b.statusId === 2 || b.statusId === 3) s.add(b.type)
  }
  return s
})

// Tipi già esistenti in qualsiasi stato (non creabili di nuovo)
const existingTypes = computed(() => {
  const s = new Set()
  for (const b of budgets.value) s.add(b.type)
  return s
})

// "Nuovo budget" ha senso solo se manca almeno un tipo
const canCreateBudget = computed(() =>
  !!selectedFiscalYearId.value && existingTypes.value.size < 2
)

// ─── Approvazione ─────────────────────────────────────────────
const showApproveModal = ref(false)
const approvingBudget  = ref(null)
const savingApprove    = ref(false)
const approveForm      = ref({ numberOfInstallments: 4, firstDueDate: '' })

function openApproveModal(b) {
  approvingBudget.value = b
  const next = new Date()
  next.setDate(1)
  next.setMonth(next.getMonth() + 1)
  approveForm.value = { numberOfInstallments: 4, firstDueDate: next.toISOString().slice(0, 10) }
  showApproveModal.value = true
}

async function confirmApprove() {
  if (!approvingBudget.value) return
  savingApprove.value = true
  try {
    await budgetApi.approve(approvingBudget.value.id, {
      numberOfInstallments: approveForm.value.numberOfInstallments,
      firstDueDate: approveForm.value.firstDueDate,
    })
    showApproveModal.value = false
    approvingBudget.value  = null
    await loadBudgets()
  } catch { /* global */ } finally { savingApprove.value = false }
}

async function closeBudget(b) {
  const tipo = b.type === 1 ? 'preventivo' : 'consuntivo'
  if (!confirm(`Chiudere il budget ${tipo}?\n\nDopo la chiusura sarà possibile approvare un nuovo budget dello stesso tipo.`)) return
  try { await budgetApi.close(b.id); await loadBudgets() } catch {}
}

// ─── Genera rate ──────────────────────────────────────────────
const showGenerateModal    = ref(false)
const generatingBudget     = ref(null)
const savingGenerate       = ref(false)
const generateHasExisting  = ref(false)
const generateForm         = ref({ numberOfInstallments: 4, firstDueDate: '' })

async function openGenerateModal(b) {
  generatingBudget.value   = b
  generateHasExisting.value = false
  const next = new Date()
  next.setDate(1)
  next.setMonth(next.getMonth() + 1)
  generateForm.value = { numberOfInstallments: 4, firstDueDate: next.toISOString().slice(0, 10) }
  try {
    const { data } = await installmentApi.getByBudget(b.id)
    generateHasExisting.value = Array.isArray(data) && data.length > 0
  } catch { /* ignore */ }
  showGenerateModal.value = true
}

async function confirmGenerate() {
  if (!generatingBudget.value) return
  savingGenerate.value = true
  try {
    await budgetApi.generateInstallments(generatingBudget.value.id, {
      numberOfInstallments: generateForm.value.numberOfInstallments,
      firstDueDate: generateForm.value.firstDueDate,
      force: generateHasExisting.value,
    })
    store.toast('Rate generate con successo', 'success')
    showGenerateModal.value = false
    generatingBudget.value  = null
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { savingGenerate.value = false }
}

async function deleteBudget(id) {
  if (!confirm('Eliminare il budget?')) return
  try { await budgetApi.delete(id); await loadBudgets() } catch {}
}

async function reopenBudget(b) {
  const tipo = b.type === 1 ? 'preventivo' : 'consuntivo'
  if (!confirm(`Riaprire il budget ${tipo}?\n\nVerrà riportato in stato Bozza e sarà nuovamente modificabile.`)) return
  try {
    await budgetApi.reopen(b.id)
    store.toast('Budget riaperto', 'success')
    await loadBudgets()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

async function fixOrphanItems(b) {
  try {
    const { data: created } = await budgetApi.fixOrphanItems(b.id)
    if (created > 0) {
      store.toast(`${created} voc${created === 1 ? 'e aggiunta' : 'i aggiunte'} alla gerarchia`, 'success')
      if (selectedBudget.value?.id === b.id) await loadBudgetItems(true)
      await loadBudgets()
    } else {
      store.toast('Nessuna voce mancante trovata', 'info')
    }
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

async function recalculateConsuntivoItems(b) {
  if (!confirm('Ricalcolare le voci del consuntivo?\n\n⚠️ ATTENZIONE: tutte le modifiche manuali alle voci verranno annullate e sostituite con i valori calcolati automaticamente dalle spese registrate e dai movimenti di entrata.\n\nContinuare?')) return
  try {
    await budgetApi.recalculateItems(b.id)
    store.toast('Voci ricalcolate', 'success')
    await loadBudgets()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

// ─── Budget Items — Piano dei conti gerarchico ───────────────
const showItemsModal = ref(false)
const selectedBudget = ref(null)
const loadingItems   = ref(false)
const savingItem     = ref(false)

// 3 tab fissi: Uscite / Entrate / Patrimoniale
const budgetTabs    = ref([])   // { id, label, rows, total }
const activeItemTab = ref(null)

// Righe del tab attivo (lista piatta ordinata gerarchicamente)
const activeTabRows = computed(() => {
  const tab = budgetTabs.value.find(t => t.id === activeItemTab.value)
  return tab?.rows ?? []
})

// Solo righe foglia (editabili) su tutti i tab
const allLeafRows = computed(() => budgetTabs.value.flatMap(t => t.rows.filter(r => !r.isHeader)))

const hasDirtyRows = computed(() => allLeafRows.value.some(r => r.dirty))

const totalIncome   = computed(() =>
  (budgetTabs.value.find(t => t.id === 'Entrata')?.total ?? 0) +
  (budgetTabs.value.find(t => t.id === 'Patrimoniale')?.total ?? 0)
)
const totalExpenses = computed(() =>
  budgetTabs.value.find(t => t.id === 'Uscita')?.total ?? 0
)
const totalBalance = computed(() => totalIncome.value - totalExpenses.value)

// snapshot per "discard"
let originalAmounts = {}

// ChartOfAccounts (per voci budget)
const chartOfAccounts = ref([])
let accountsLoaded = false

async function openItemsModal(budget) {
  selectedBudget.value = budget
  showItemsModal.value = true
  budgetTabs.value     = []
  activeItemTab.value  = null
  router.replace({ query: { ...route.query, voci: budget.id } })
  await loadBudgetItems()
}

function closeItemsModal() {
  showItemsModal.value = false
  selectedBudget.value = null
  const q = { ...route.query }
  delete q.voci
  router.replace({ query: q })
  budgetTabs.value     = []
  activeItemTab.value  = null
}

// Costruisce una lista piatta ordinata gerarchicamente a partire dai nodi radice del tipo dato.
// Ogni elemento ha: accountId, accountCode, accountName, accountType, level, isHeader,
//                   itemId, amount, dirty
function buildHierarchyRows(items, typeLabel) {
  const byParent  = {}
  for (const i of items) {
    const pid = i.parentAccountId ?? null
    if (!byParent[pid]) byParent[pid] = []
    byParent[pid].push(i)
  }
  for (const list of Object.values(byParent))
    list.sort((a, b) => (a.accountCode ?? '').localeCompare(b.accountCode ?? ''))

  const result = []
  function walk(parentId, level) {
    for (const i of (byParent[parentId] ?? [])) {
      const hasChildren = !!(byParent[i.accountId]?.length)
      result.push({
        accountId:   i.accountId,
        accountCode: i.accountCode,
        accountName: i.accountName,
        accountType: typeLabel,
        level,
        isHeader:    hasChildren,   // nodi con figli = sola lettura, importo calcolato
        itemId:      hasChildren ? null : (i.id ?? null),
        amount:      i.amount ?? 0,
        amountPaid:  i.amountPaid ?? 0,
        dirty:       false,
      })
      walk(i.accountId, level + 1)
    }
  }
  // Parti dai nodi senza padre
  walk(null, 1)
  // Se non ci sono nodi con parentAccountId=null, usa tutti come radici piatte
  if (result.length === 0) {
    for (const i of items.sort((a, b) => (a.accountCode ?? '').localeCompare(b.accountCode ?? ''))) {
      result.push({
        accountId: i.accountId, accountCode: i.accountCode,
        accountName: i.accountName, accountType: typeLabel,
        level: 1, isHeader: false,
        itemId: i.id ?? null, amount: i.amount ?? 0, amountPaid: i.amountPaid ?? 0, dirty: false,
      })
    }
  }
  return result
}

// Ricalcola l'importo di ogni nodo-header come somma dei figli foglia discendenti
function recalcHeaders(rows) {
  for (let i = rows.length - 1; i >= 0; i--) {
    const r = rows[i]
    if (!r.isHeader) continue
    let total = 0, totalPaid = 0
    for (let j = i + 1; j < rows.length; j++) {
      if (rows[j].level <= r.level) break
      if (!rows[j].isHeader) { total += rows[j].amount; totalPaid += rows[j].amountPaid }
    }
    r.amount     = total
    r.amountPaid = totalPaid
  }
}

async function loadBudgetItems(silent = false) {
  if (!selectedBudget.value) return
  if (!silent) loadingItems.value = true
  try {
    const { data } = await budgetItemApi.getByBudget(selectedBudget.value.id)
    const items = data ?? []

    // Mappa type numerico → label stringa
    const TYPE_LABEL = { 1: 'Entrata', 2: 'Uscita', 3: 'Patrimoniale' }

    budgetTabs.value = [
      { id: 'Uscita',       label: 'Uscite' },
      { id: 'Entrata',      label: 'Entrate' },
      { id: 'Patrimoniale', label: 'Patrimoniale' },
    ].map(def => {
      const typeItems = items.filter(i => (TYPE_LABEL[i.accountType] ?? i.accountType) === def.id)
      const rows = buildHierarchyRows(typeItems, def.id)
      recalcHeaders(rows)
      const total = rows.filter(r => !r.isHeader).reduce((s, r) => s + (r.amount || 0), 0)
      return { ...def, rows, total }
    })

    activeItemTab.value = budgetTabs.value[0].id

    // Snapshot per discard (solo foglie)
    originalAmounts = {}
    for (const row of allLeafRows.value) {
      originalAmounts[row.accountId] = row.amount
    }

  } catch {
    budgetTabs.value = []
  } finally {
    loadingItems.value = false
  }
}

function onAmountChange(row, event) {
  const val = parseFloat(event.target.value) || 0
  row.amount = val
  row.dirty  = val !== (originalAmounts[row.accountId] ?? 0)
  const tab = budgetTabs.value.find(t => t.id === activeItemTab.value)
  if (tab) {
    recalcHeaders(tab.rows)
    tab.total = tab.rows.filter(r => !r.isHeader).reduce((s, r) => s + (r.amount || 0), 0)
  }
}

function discardChanges() {
  for (const row of allLeafRows.value) {
    row.amount = originalAmounts[row.accountId] ?? 0
    row.dirty  = false
  }
  for (const tab of budgetTabs.value) {
    recalcHeaders(tab.rows)
    tab.total = tab.rows.filter(r => !r.isHeader).reduce((s, r) => s + (r.amount || 0), 0)
  }
}

async function saveAllItems() {
  if (!selectedBudget.value) return
  savingItem.value = true
  try {
    const dirtyRows = allLeafRows.value.filter(r => r.dirty)
    for (const row of dirtyRows) {
      if (row.itemId) {
        await budgetItemApi.update(row.itemId, { accountId: row.accountId, amount: row.amount })
      } else if (row.amount > 0) {
        const { data } = await budgetItemApi.create({
          budgetId:  selectedBudget.value.id,
          accountId: row.accountId,
          amount:    row.amount,
        })
        row.itemId = data?.id ?? null
      }
      row.dirty = false
      originalAmounts[row.accountId] = row.amount
    }
    await loadBudgets()
    const refreshed = budgets.value.find(b => b.id === selectedBudget.value?.id)
    if (refreshed) selectedBudget.value = refreshed
  } catch { /* global */ } finally { savingItem.value = false }
}

async function deleteItem(row) {
  if (!row.itemId) return
  if (!confirm(`Rimuovere la voce "${row.accountName}"?`)) return
  try {
    await budgetItemApi.delete(row.itemId)
    await loadBudgetItems(true)
    await loadBudgets()
    const refreshed = budgets.value.find(b => b.id === selectedBudget.value?.id)
    if (refreshed) selectedBudget.value = refreshed
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

// ─── Aggiungi voci dal piano dei conti ───────────────────────
const showAddAccountsModal  = ref(false)
const loadingAccounts       = ref(false)
const savingAddAccounts     = ref(false)
const addAccountsTree       = ref([])   // lista piatta ordinata per visualizzazione
const addAccountsSelected   = ref([])   // id dei conti foglia selezionati

async function openAddAccountsModal() {
  addAccountsSelected.value = []
  showAddAccountsModal.value = true
  loadingAccounts.value = true
  try {
    const { data } = await chartOfAccountsApi.getByCondominium(store.selectedCondominioId)
    const accounts = (data ?? []).filter(a => a.isActive)

    // Calcola set degli accountId già presenti nel budget (da tutte le tab)
    const presentIds = new Set(
      budgetTabs.value.flatMap(t => t.rows.map(r => r.accountId))
    )

    // Costruisce albero piatto ordinato per visualizzazione (DFS)
    const byParent = {}
    for (const a of accounts) {
      const pid = a.parentAccountId ?? null
      if (!byParent[pid]) byParent[pid] = []
      byParent[pid].push(a)
    }
    for (const list of Object.values(byParent))
      list.sort((a, b) => (a.code ?? '').localeCompare(b.code ?? ''))

    const childIds = new Set(accounts.filter(a => a.parentAccountId != null).map(a => a.parentAccountId))

    const tree = []
    function walkTree(parentId, level) {
      for (const a of (byParent[parentId] ?? [])) {
        const hasChildren = childIds.has(a.id)
        tree.push({
          id:            a.id,
          code:          a.code,
          name:          a.name,
          level,
          hasChildren,
          alreadyPresent: !hasChildren && presentIds.has(a.id),
        })
        walkTree(a.id, level + 1)
      }
    }
    walkTree(null, 1)
    addAccountsTree.value = tree
  } catch {
    addAccountsTree.value = []
  } finally {
    loadingAccounts.value = false
  }
}

async function confirmAddAccounts() {
  if (!addAccountsSelected.value.length || !selectedBudget.value) return
  savingAddAccounts.value = true
  try {
    for (const accountId of addAccountsSelected.value) {
      await budgetItemApi.create({
        budgetId:  selectedBudget.value.id,
        accountId,
        amount:    0,
      })
    }
    showAddAccountsModal.value = false
    store.toast(`${addAccountsSelected.value.length} voce/i aggiunta/e`, 'success')
    await loadBudgetItems(true)
    await loadBudgets()
    const refreshed = budgets.value.find(b => b.id === selectedBudget.value?.id)
    if (refreshed) selectedBudget.value = refreshed
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    savingAddAccounts.value = false
  }
}

// ─── Formatters ───────────────────────────────────────────────
const fmt     = (v) => v != null ? '€ ' + Number(v).toLocaleString('it-IT', { minimumFractionDigits: 2 }) : '—'
const fmtDate = (d) => d ? new Date(d).toLocaleDateString('it-IT') : '—'

const statusBadge  = (id) => ({ 1: 'badge-muted', 2: 'badge-green', 3: 'badge-purple' }[id] ?? 'badge-muted')
const statusLabel  = (id) => ({ 1: 'Bozza', 2: 'Approvato', 3: 'Chiuso' }[id] ?? String(id ?? ''))

const TYPE_CLASS = { Uscita: 'type-uscita', Entrata: 'type-entrata', Patrimoniale: 'type-patrimoniale' }
const typeClass  = (typeId) => ['type-badge', TYPE_CLASS[typeId] ?? ''].join(' ')

// ─── Watchers / Init ──────────────────────────────────────────
watch(() => store.selectedCondominioId, async () => {
  budgetTabs.value = []
  // loadFiscalYears è già gestito dal watch in appStore
})
watch(() => store.selectedFiscalYearId, loadBudgets)

onMounted(async () => {
  if (!store.fiscalYears.length) await store.loadFiscalYears()
  await loadBudgets()
  // Ripristina la modale voci se l'URL contiene ?voci=<id>
  const vociId = Number(route.query.voci)
  if (vociId) {
    let b = budgets.value.find(b => b.id === vociId)
    if (!b) {
      try {
        const { data: found } = await budgetApi.getById(vociId)
        if (found) {
          await resolveContext(found)
          store.selectedFiscalYearId = found.fiscalYearId
          await loadBudgets()
          b = budgets.value.find(b => b.id === vociId)
        }
      } catch {
        // Budget non trovato o accesso negato — ignora
      }
    }
    if (b) await openItemsModal(b)
  }
})
</script>

<style scoped>
.tab-toolbar { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1rem; flex-wrap: wrap; }
.btn-reopen  { color: var(--accent-amber, #f59e0b) !important; border-color: rgba(245,158,11,0.3) !important; }
.btn-active  { background: var(--accent-glow) !important; color: var(--accent) !important; border-color: var(--border-active) !important; }
.row-actions { display: flex; gap: 0.4rem; justify-content: flex-end; }
.text-right  { text-align: right; }
.text-green  { color: var(--accent-green, #22c55e); }
.text-red    { color: var(--accent-red,   #ef4444); }
.ml-2        { margin-left: 0.5rem; }

/* ── Riepilogo totali in cima al modal ── */
.budget-summary-top {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
  margin-bottom: 1rem;
  padding: 0.6rem 0.75rem;
  background: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: 8px;
}
.summary-pill {
  display: flex;
  align-items: center;
  gap: 0.35rem;
  padding: 0.3rem 0.65rem;
  border-radius: 20px;
  background: var(--bg-base, #1a1a2e);
  border: 1px solid var(--border);
  font-size: 0.82rem;
}
.summary-pill-green { border-color: rgba(34,197,94,0.4);  }
.summary-pill-red   { border-color: rgba(239,68,68,0.4); }
.summary-pill-label { color: var(--text-muted); }
.summary-pill-value { font-weight: 700; }
.summary-save-actions { margin-left: auto; display: flex; gap: 0.4rem; }

/* ── Tab capitoli ── */
.budget-tabs {
  display: flex;
  flex-wrap: wrap;
  gap: 0.25rem;
  margin-bottom: 0;
  border-bottom: 2px solid var(--border);
  padding-bottom: 0;
}
.budget-tab-btn {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  padding: 0.4rem 0.75rem;
  border: 1px solid transparent;
  border-bottom: none;
  border-radius: 6px 6px 0 0;
  background: transparent;
  cursor: pointer;
  font-size: 0.82rem;
  color: var(--text-secondary);
  transition: background 0.15s;
  position: relative;
  bottom: -2px;
}
.budget-tab-btn:hover { background: var(--bg-surface); }
.budget-tab-btn.active {
  background: var(--bg-surface);
  border-color: var(--border);
  border-bottom-color: var(--bg-surface);
  color: var(--text);
  font-weight: 600;
}
.tab-label  { line-height: 1.2; }
.tab-total  { font-size: 0.78rem; font-weight: 700; line-height: 1.2; }

/* ── Contenuto tab ── */
.budget-tab-content {
  border: 1px solid var(--border);
  border-top: none;
  border-radius: 0 0 8px 8px;
  overflow: hidden;
}

/* ── Tabella voci ── */
.budget-accounts-table { width: 100%; border-collapse: collapse; }
.budget-accounts-table th,
.budget-accounts-table td { padding: 0.45rem 0.65rem; border-bottom: 1px solid var(--border); font-size: 0.875rem; }
.budget-accounts-table thead th { background: var(--bg-surface); font-weight: 600; }
.budget-accounts-table tr.row-edited td { background: rgba(99,102,241,0.06); }
.budget-accounts-table tbody tr:last-child td { border-bottom: none; }
.budget-accounts-table tr.row-header td { background: var(--bg-surface); font-weight: 600; border-top: 1px solid var(--border); }
.budget-accounts-table tr.row-header td:first-child { color: var(--text-muted); }
.row-header-amount { font-weight: 600; color: var(--text); }

.budget-amount-input {
  width: 120px;
  padding: 0.25rem 0.4rem;
  text-align: right;
  font-family: monospace;
  font-size: 0.875rem;
}
.section-total-row td { font-weight: 600; background: var(--bg-surface); border-top: 2px solid var(--border); }
.btn-delete-row { color: var(--text-muted); font-size: 0.75rem; opacity: 0.5; transition: opacity 0.15s, color 0.15s; }
.btn-delete-row:hover { color: var(--accent-red); opacity: 1; }
.delta-cell { font-weight: 500; }
.delta-positive { color: var(--accent-red); }
.delta-negative { color: var(--accent-green); }
.delta-zero    { color: var(--text-muted); }

/* Approve modal */
.approve-info { font-size: .875rem; color: var(--text-secondary); margin-bottom: 1rem; }
.generate-warning {
  display: flex; gap: .75rem; align-items: flex-start;
  background: rgba(239,68,68,.08); border: 1px solid rgba(239,68,68,.3);
  border-radius: 6px; padding: .75rem 1rem; margin-bottom: 1rem;
  font-size: .875rem; color: var(--accent-red);
}
.generate-warning-icon { font-size: 1.25rem; flex-shrink: 0; margin-top: .05rem; }
.btn-danger { background: var(--accent-red) !important; color: #fff !important; border-color: var(--accent-red) !important; }
.approve-hint { font-size: .78rem;  color: var(--text-muted);     margin-top: .5rem; }

/* Form validation */
.has-error .form-input,
.has-error .form-select { border-color: var(--accent-red, #e53e3e); }
.field-error { font-size: 0.78rem; color: var(--accent-red, #e53e3e); margin-top: 0.2rem; display: block; }

/* ── Aggiungi voci ── */
.summary-add-action { margin-left: auto; }
.add-accounts-overlay { z-index: 1100; }
.modal-lg { max-width: 640px; width: 100%; }
.add-accounts-hint { font-size: 0.82rem; color: var(--text-secondary); margin-bottom: 0.75rem; }
.add-accounts-tree {
  max-height: 420px;
  overflow-y: auto;
  border: 1px solid var(--border);
  border-radius: 6px;
  background: var(--bg-base);
}
.coa-node {
  border-bottom: 1px solid var(--border);
  padding-top: 0.35rem;
  padding-bottom: 0.35rem;
  padding-right: 0.75rem;
}
.coa-node:last-child { border-bottom: none; }
.coa-node-header { background: var(--bg-surface); }
.coa-node-disabled { opacity: 0.55; }
.coa-checkbox-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
  font-size: 0.875rem;
  width: 100%;
}
.coa-checkbox-label input[type="checkbox"] { flex-shrink: 0; cursor: pointer; accent-color: var(--accent); }
.coa-group-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--text-secondary);
}
.coa-code { font-size: 0.78rem; color: var(--text-muted); min-width: 60px; }
.coa-name { flex: 1; }
</style>
