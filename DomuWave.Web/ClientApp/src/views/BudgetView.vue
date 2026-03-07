<template>
  <div>
    <!-- ── Header ──────────────────────────────────────── -->
    <div class="page-header">
      <h1>Budget</h1>
      <div class="flex gap-2">
        <button class="btn btn-ghost" @click="activeTab='budget'"
                :class="activeTab==='budget' ? 'btn-active' : ''">Budget</button>
        <button class="btn btn-ghost" @click="activeTab='spese'"
                :class="activeTab==='spese' ? 'btn-active' : ''">Spese</button>
      </div>
    </div>

    <!-- ══════════════════════════════════════════════════
         TAB BUDGET
    ══════════════════════════════════════════════════ -->
    <div v-if="activeTab === 'budget'">
      <div class="tab-toolbar">
        <!-- FiscalYear selector -->
        <select class="form-select" v-model="selectedFiscalYearId" style="min-width:180px"
                :disabled="!fiscalYears.length">
          <option :value="null" disabled>Seleziona esercizio…</option>
          <option v-for="fy in fiscalYears" :key="fy.id" :value="fy.id">
            {{ fy.code }}{{ fy.description ? ' – ' + fy.description : '' }}
          </option>
        </select>

        <button v-if="canCreate" class="btn btn-primary" style="margin-left:auto"
                :disabled="!selectedFiscalYearId"
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
                <td>{{ b.type === 1 ? 'Preventivo' : 'Consuntivo' }}</td>
                <td class="mono text-right text-green">{{ fmt(b.totalIncome) }}</td>
                <td class="mono text-right text-red">{{ fmt(b.totalExpenses) }}</td>
                <td class="text-secondary">{{ fmtDate(b.approvalDate) }}</td>
                <td><span class="badge" :class="statusBadge(b.statusId)">{{ statusLabel(b.statusId) }}</span></td>
                <td>
                  <div class="row-actions">
                    <button class="btn btn-sm btn-ghost" @click="openItemsModal(b)">Voci</button>
                    <button v-if="canEdit && b.statusId === 1 && !blockedTypes.has(b.type)"
                            class="btn btn-sm btn-ghost"
                            @click="openApproveModal(b)">Approva</button>
                    <span v-if="canEdit && b.statusId === 1 && blockedTypes.has(b.type)"
                          class="badge badge-muted" title="Esiste già un budget di questo tipo approvato o chiuso">
                      Non approvabile
                    </span>
                    <button v-if="canEdit && b.statusId === 2" class="btn btn-sm btn-ghost"
                            @click="closeBudget(b)">Chiudi</button>
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
         TAB SPESE
    ══════════════════════════════════════════════════ -->
    <div v-if="activeTab === 'spese'">
      <div class="tab-toolbar">
        <select class="form-select" v-model.number="expTypeFilter" style="width:160px" @change="loadExpenses">
          <option :value="0">Tutti i tipi</option>
          <option :value="1">Manutenzione</option>
          <option :value="2">Pulizie</option>
          <option :value="3">Sicurezza</option>
          <option :value="4">Utenze</option>
          <option :value="5">Professionale</option>
          <option :value="6">Altro</option>
        </select>
        <select class="form-select" v-model="expFilter" style="width:140px" @change="loadExpenses">
          <option value="">Tutte</option>
          <option value="unpaid">Non pagate</option>
        </select>
        <button v-if="canCreate" class="btn btn-primary" style="margin-left:auto"
                @click="openExpenseModal()">+ Nuova spesa</button>
      </div>

      <div class="card">
        <div v-if="loadingExp" class="loading-state"><div class="spinner"></div></div>
        <div v-else-if="!expenses.length" class="empty-state">
          <div class="empty-icon">◎</div><div>Nessuna spesa trovata</div>
        </div>
        <div v-else class="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Data</th>
                <th>Fornitore</th>
                <th class="text-right">Importo</th>
                <th class="text-right">IVA</th>
                <th>Pagamento</th>
                <th>Stato</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="e in expenses" :key="e.id">
                <td class="mono text-secondary">{{ fmtDate(e.documentDate) }}</td>
                <td>{{ e.supplierName || '—' }}</td>
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
            <option value="1">Preventivo</option>
            <option value="2">Consuntivo</option>
          </select>
        </div>
        <div class="form-group">
          <label class="form-label">Totale entrate (€)</label>
          <input class="form-input" type="number" step="0.01" v-model.number="budgetForm.totalIncome" />
        </div>
      </div>
      <div class="form-group">
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
         MODAL — Voci di Budget
    ══════════════════════════════════════════════════ -->
    <BaseModal :show="showItemsModal" @close="closeItemsModal" size="lg">
      <template #title>Voci di budget</template>
      <template #subtitle>
        <span v-if="store.selectedCondominio" class="modal-condominium">{{ store.selectedCondominio.name }}</span>
        <span v-if="store.selectedCondominio && selectedBudget"> · </span>
        <span v-if="selectedBudget">{{ selectedBudget.type }}</span>
        <span v-if="selectedBudget?.fiscalYearCode"> · {{ selectedBudget.fiscalYearCode }}</span>
        <span v-if="selectedBudget" class="badge ml-2" :class="statusBadge(selectedBudget.statusId)">
          {{ statusLabel(selectedBudget.statusId) }}
        </span>
      </template>

      <!-- Add new item button -->
      <div class="items-toolbar" v-if="canCreate && !showItemForm">
        <button class="btn btn-primary btn-sm" @click="openItemForm()">+ Aggiungi voce</button>
      </div>

      <!-- Item form (inline) -->
      <div class="item-form-panel" v-if="showItemForm">
        <div class="form-grid form-grid-3">
          <div class="form-group" style="grid-column: span 2">
            <label class="form-label">Conto *</label>
            <select class="form-select" v-model.number="itemForm.accountId">
              <option :value="null" disabled>Seleziona conto…</option>
              <optgroup v-for="grp in accountGroups" :key="grp.type" :label="grp.type">
                <option v-for="a in grp.accounts" :key="a.id" :value="a.id">
                  {{ a.code }} – {{ a.name }}
                </option>
              </optgroup>
            </select>
          </div>
          <div class="form-group">
            <label class="form-label">Importo (€) *</label>
            <input class="form-input" type="number" step="0.01" v-model.number="itemForm.amount" />
          </div>
          <div class="form-group" style="grid-column: span 2">
            <label class="form-label">Descrizione</label>
            <input class="form-input" v-model="itemForm.description" placeholder="Descrizione voce…" />
          </div>
          <div class="form-group">
            <label class="form-label">Note</label>
            <input class="form-input" v-model="itemForm.notes" />
          </div>
        </div>
        <div class="item-form-actions">
          <button class="btn btn-ghost btn-sm" @click="cancelItemForm">Annulla</button>
          <button class="btn btn-primary btn-sm" @click="saveItem" :disabled="savingItem">
            <span v-if="savingItem" class="spinner" style="width:12px;height:12px"></span>
            {{ editingItem ? 'Salva' : 'Aggiungi' }}
          </button>
        </div>
      </div>

      <!-- Items table -->
      <div v-if="loadingItems" class="loading-state"><div class="spinner"></div></div>
      <div v-else-if="!budgetItems.length && !showItemForm" class="empty-state" style="padding:1.5rem">
        <div class="empty-icon">◎</div>
        <div>Nessuna voce di budget. Clicca "+ Aggiungi voce" per iniziare.</div>
      </div>
      <div v-else-if="budgetItems.length" class="table-wrap" style="margin-top:0.5rem">
        <table>
          <thead>
            <tr>
              <th>Conto</th>
              <th>Descrizione</th>
              <th class="text-right">Importo</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in budgetItems" :key="item.id">
              <td>
                <span class="mono text-secondary" style="font-size:0.8em">{{ item.accountCode }}</span>
                {{ item.accountName }}
              </td>
              <td class="text-secondary">{{ item.description || '—' }}</td>
              <td class="mono text-right">{{ fmt(item.amount) }}</td>
              <td>
                <div class="row-actions">
                  <button v-if="canEdit" class="btn-icon" @click="openItemForm(item)" title="Modifica">✎</button>
                  <button v-if="canDelete" class="btn-icon" style="color:var(--accent-red)"
                          @click="deleteItem(item.id)" title="Elimina">✕</button>
                </div>
              </td>
            </tr>
          </tbody>
          <tfoot>
            <tr class="items-total-row">
              <td colspan="2" class="text-secondary">Totale voci</td>
              <td class="mono text-right text-red">{{ fmt(totalItems) }}</td>
              <td></td>
            </tr>
          </tfoot>
        </table>
      </div>

      <template #footer>
        <button class="btn btn-ghost" @click="closeItemsModal">Chiudi</button>
      </template>
    </BaseModal>

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
            <optgroup v-for="grp in accountGroups" :key="grp.type" :label="grp.type">
              <option v-for="a in grp.accounts" :key="a.id" :value="a.id">{{ a.code }} – {{ a.name }}</option>
            </optgroup>
          </select>
          <span v-if="expErrors.accountId" class="field-error">{{ expErrors.accountId }}</span>
        </div>
        <div class="form-group" :class="{ 'has-error': expErrors.millesimalTableId }">
          <label class="form-label">Tabella millesimale *</label>
          <select class="form-select" v-model.number="expForm.millesimalTableId" @change="clearExpError('millesimalTableId')">
            <option :value="null" disabled>Seleziona tabella…</option>
            <option v-for="t in millesimalTables" :key="t.id" :value="t.id">
              {{ t.code }}{{ t.name ? ' – ' + t.name : '' }}
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
import { budgetApi, budgetItemApi, fiscalYearApi, chartOfAccountsApi, expenseApi, supplierApi, millesimalTableApi } from '@/services/api'
import { usePermissions } from '@/composables/usePermissions'
import BaseModal from '@/components/BaseModal.vue'

const store = useAppStore()
const { canCreate, canEdit, canDelete } = usePermissions()

const activeTab = ref('budget')

// ─── Fiscal Years ─────────────────────────────────────────────
const fiscalYears        = ref([])
const selectedFiscalYearId = ref(null)

async function loadFiscalYears() {
  if (!store.selectedCondominioId) return
  try {
    const { data } = await fiscalYearApi.getByCondominium(store.selectedCondominioId)
    fiscalYears.value = data ?? []
    // Pre-select active or first
    const active = fiscalYears.value.find(f => f.isActive) ?? fiscalYears.value[0]
    selectedFiscalYearId.value = active?.id ?? null
  } catch {
    fiscalYears.value = []
  }
}

// ─── Budget ───────────────────────────────────────────────────
const budgets       = ref([])
const loadingBudget = ref(false)
const showBudgetModal = ref(false)
const editingBudget  = ref(null)
const savingBudget   = ref(false)
const budgetForm     = ref({ type: 1, totalIncome: 0, notes: '' })

async function loadBudgets() {
  if (!selectedFiscalYearId.value) { budgets.value = []; return }
  loadingBudget.value = true
  try {
    const { data } = await budgetApi.getByFiscalYear(selectedFiscalYearId.value)
    budgets.value = data ?? []
  } catch { budgets.value = [] } finally { loadingBudget.value = false }
}

function openBudgetModal(b = null) {
  editingBudget.value = b?.id ?? null
  budgetForm.value = b
    ? { totalIncome: b.totalIncome, notes: b.notes ?? '' }
    : { type: 1, totalIncome: 0, notes: '' }
  showBudgetModal.value = true
}

async function saveBudget() {
  savingBudget.value = true
  try {
    if (editingBudget.value) {
      await budgetApi.update(editingBudget.value, {
        totalIncome: budgetForm.value.totalIncome,
        notes: budgetForm.value.notes,
      })
    } else {
      await budgetApi.create({
        condominiumId:  store.selectedCondominioId,
        fiscalYearId:   selectedFiscalYearId.value,
        type:           budgetForm.value.type,
        totalIncome:    budgetForm.value.totalIncome,
        notes:          budgetForm.value.notes,
      })
    }
    showBudgetModal.value = false
    await loadBudgets()
  } catch { /* toast handled globally */ } finally { savingBudget.value = false }
}

// Tipi di budget per cui esiste già un approvato o chiuso (non approvabili)
const blockedTypes = computed(() => {
  const s = new Set()
  for (const b of budgets.value) {
    if (b.statusId === 2 || b.statusId === 3) s.add(b.type)
  }
  return s
})

// ─── Approvazione con parametri rate ──────────────────────────
const showApproveModal    = ref(false)
const approvingBudget     = ref(null)
const savingApprove       = ref(false)
const approveForm         = ref({ numberOfInstallments: 4, firstDueDate: '' })

function openApproveModal(b) {
  approvingBudget.value = b
  const today = new Date()
  // prima scadenza: primo giorno del mese successivo
  const next = new Date(today.getFullYear(), today.getMonth() + 1, 1)
  approveForm.value = {
    numberOfInstallments: 4,
    firstDueDate: next.toISOString().slice(0, 10),
  }
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
  if (!confirm(
    `Chiudere il budget ${tipo}?\n\n` +
    `Dopo la chiusura sarà possibile approvare un nuovo budget dello stesso tipo.`
  )) return
  try { await budgetApi.close(b.id); await loadBudgets() } catch {}
}

async function deleteBudget(id) {
  if (!confirm('Eliminare il budget?')) return
  try { await budgetApi.delete(id); await loadBudgets() } catch {}
}

// ─── Budget Items ─────────────────────────────────────────────
const showItemsModal  = ref(false)
const selectedBudget  = ref(null)
const budgetItems     = ref([])
const loadingItems    = ref(false)

const showItemForm  = ref(false)
const editingItem   = ref(null)
const savingItem    = ref(false)
const itemForm      = ref({ accountId: null, description: '', amount: 0, notes: '' })

// ChartOfAccounts (loaded once per condominium on first items open)
const chartOfAccounts = ref([])
let   accountsLoaded  = false

const accountGroups = computed(() => {
  const map = {}
  for (const a of chartOfAccounts.value) {
    const t = a.type || 'Altro'
    if (!map[t]) map[t] = []
    map[t].push(a)
  }
  return Object.entries(map).map(([type, accounts]) => ({ type, accounts }))
})

const totalItems = computed(() =>
  budgetItems.value.reduce((s, i) => s + (i.amount ?? 0), 0)
)

async function openItemsModal(budget) {
  selectedBudget.value = budget
  showItemsModal.value = true
  showItemForm.value   = false
  editingItem.value    = null
  budgetItems.value    = []

  // Load chart of accounts (once)
  if (!accountsLoaded && store.selectedCondominioId) {
    try {
      const { data } = await chartOfAccountsApi.getByCondominium(store.selectedCondominioId)
      chartOfAccounts.value = data ?? []
      accountsLoaded = true
    } catch { chartOfAccounts.value = [] }
  }

  await loadBudgetItems()
}

function closeItemsModal() {
  showItemsModal.value = false
  selectedBudget.value = null
  budgetItems.value    = []
  showItemForm.value   = false
}

async function loadBudgetItems() {
  if (!selectedBudget.value) return
  loadingItems.value = true
  try {
    const { data } = await budgetItemApi.getByBudget(selectedBudget.value.id)
    budgetItems.value = data ?? []
  } catch { budgetItems.value = [] } finally { loadingItems.value = false }
}

function openItemForm(item = null) {
  editingItem.value = item?.id ?? null
  itemForm.value = item
    ? { accountId: item.accountId, description: item.description ?? '', amount: item.amount, notes: item.notes ?? '' }
    : { accountId: null, description: '', amount: 0, notes: '' }
  showItemForm.value = true
}

function cancelItemForm() {
  showItemForm.value = false
  editingItem.value  = null
}

async function saveItem() {
  if (!itemForm.value.accountId || !itemForm.value.amount) return
  savingItem.value = true
  try {
    if (editingItem.value) {
      await budgetItemApi.update(editingItem.value, {
        accountId:   itemForm.value.accountId,
        description: itemForm.value.description,
        amount:      itemForm.value.amount,
        notes:       itemForm.value.notes,
      })
    } else {
      await budgetItemApi.create({
        budgetId:    selectedBudget.value.id,
        accountId:   itemForm.value.accountId,
        description: itemForm.value.description,
        amount:      itemForm.value.amount,
        notes:       itemForm.value.notes,
      })
    }
    showItemForm.value = false
    editingItem.value  = null
    await loadBudgetItems()
    // Refresh budgets list so TotalExpenses is updated
    await loadBudgets()
  } catch { /* global */ } finally { savingItem.value = false }
}

async function deleteItem(id) {
  if (!confirm('Eliminare questa voce?')) return
  try {
    await budgetItemApi.delete(id)
    await loadBudgetItems()
    await loadBudgets()
  } catch {}
}

// ─── Spese ────────────────────────────────────────────────────
const expenses        = ref([])
const loadingExp      = ref(false)
const showExpenseModal = ref(false)
const editingExp      = ref(null)
const savingExp       = ref(false)
const expErrors       = ref({})
const expFilter       = ref('')
const expTypeFilter   = ref(0)

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
const today           = new Date().toISOString().slice(0, 10)
const suppliers       = ref([])
const millesimalTables = ref([])

const emptyExpForm = () => ({
  name: '', documentNumber: '', documentDate: today, registrationDate: today,
  grossAmount: 0, vatAmount: 0, expenseTypeId: 0, paymentStatusId: 1,
  paymentMethod: '', supplierId: null, accountId: null, millesimalTableId: null, description: ''
})
const expForm = ref(emptyExpForm())

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
    expenses.value = res.data
  } catch { expenses.value = [] } finally { loadingExp.value = false }
}

async function openExpenseModal(e = null) {
  expErrors.value = {}
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
  } : emptyExpForm()

  // Load dropdowns (accounts already loaded when opening items modal; reload suppliers & millesimal tables)
  if (!accountsLoaded && store.selectedCondominioId) {
    try {
      const { data } = await chartOfAccountsApi.getByCondominium(store.selectedCondominioId)
      chartOfAccounts.value = data ?? []
      accountsLoaded = true
    } catch { chartOfAccounts.value = [] }
  }
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

async function saveExpense() {
  if (!validateExpForm()) return
  savingExp.value = true
  try {
    const payload = {
      name:             expForm.value.name,
      documentNumber:   expForm.value.documentNumber || null,
      documentDate:     expForm.value.documentDate,
      registrationDate: expForm.value.registrationDate,
      grossAmount:      expForm.value.grossAmount,
      vatAmount:        expForm.value.vatAmount,
      netAmount:        expForm.value.grossAmount - expForm.value.vatAmount,
      expenseTypeId:    expForm.value.expenseTypeId,
      paymentStatusId:  expForm.value.paymentStatusId || 1,
      paymentMethod:    expForm.value.paymentMethod || null,
      description:      expForm.value.description || null,
      condominiumId:    store.selectedCondominioId,
      accountId:        expForm.value.accountId         ?? null,
      millesimalTableId: expForm.value.millesimalTableId ?? null,
      supplierId:       expForm.value.supplierId        ?? null,
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
  try {
    await expenseApi.markAsPaid(id, today, 'BankTransfer')
    await loadExpenses()
  } catch {}
}

async function deleteExpense(id) {
  if (!confirm('Eliminare questa spesa?')) return
  try { await expenseApi.delete(id); await loadExpenses() } catch {}
}

// ─── Formatters ───────────────────────────────────────────────
const fmt     = (v) => v != null ? '€ ' + Number(v).toLocaleString('it-IT', { minimumFractionDigits: 2 }) : '—'
const fmtDate = (d) => d ? new Date(d).toLocaleDateString('it-IT') : '—'

const statusBadge  = (id) => ({ 1: 'badge-muted', 2: 'badge-green', 3: 'badge-purple' }[id] ?? 'badge-muted')
const statusLabel  = (id) => ({ 1: 'Bozza', 2: 'Approvato', 3: 'Chiuso' }[id] ?? String(id ?? ''))
const payBadge     = (id) => ({ 1: 'badge-amber', 2: 'badge-green', 3: 'badge-red' }[id] ?? 'badge-muted')

// Auto-select DefaultMillesimalTable when account changes
watch(() => expForm.value.accountId, (accountId) => {
  if (!accountId) return
  const account = chartOfAccounts.value.find(a => a.id === accountId)
  if (account?.defaultMillesimalTableId) {
    expForm.value.millesimalTableId = account.defaultMillesimalTableId
  }
})

// ─── Watchers / Init ──────────────────────────────────────────
watch(() => store.selectedCondominioId, async () => {
  accountsLoaded = false
  expTypeFilter.value = 0
  expFilter.value = ''
  await loadFiscalYears()
  await loadExpenses()
})
watch(selectedFiscalYearId, loadBudgets)

onMounted(async () => {
  await loadFiscalYears()
  await loadExpenses()
})
</script>

<style scoped>
.tab-toolbar { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1rem; flex-wrap: wrap; }
.btn-active  { background: var(--accent-glow) !important; color: var(--accent) !important; border-color: var(--border-active) !important; }
.row-actions { display: flex; gap: 0.4rem; justify-content: flex-end; }
.text-right  { text-align: right; }
.text-green  { color: var(--accent-green, #22c55e); }
.text-red    { color: var(--accent-red,   #ef4444); }
.ml-2        { margin-left: 0.5rem; }

/* Items modal toolbar */
.items-toolbar { margin-bottom: 0.75rem; }

/* Item inline form */
.item-form-panel {
  background: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: 8px;
  padding: 1rem;
  margin-bottom: 1rem;
}
.form-grid-3 { grid-template-columns: 1fr 1fr 1fr; }
.item-form-actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 0.5rem; }

/* Items total row */
.items-total-row td { font-weight: 600; border-top: 2px solid var(--border); }

/* Approve modal */
.approve-info { font-size: .875rem; color: var(--text-secondary); margin-bottom: 1rem; }
.approve-hint { font-size: .78rem; color: var(--text-muted); margin-top: .5rem; }

/* Form validation */
.has-error .form-input,
.has-error .form-select { border-color: var(--accent-red, #e53e3e); }
.field-error { font-size: 0.78rem; color: var(--accent-red, #e53e3e); margin-top: 0.2rem; display: block; }
</style>
