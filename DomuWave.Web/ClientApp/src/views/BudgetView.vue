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
                            @click="approveBudget(b)">Approva</button>
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
        <input class="form-input" type="date" v-model="expFrom" style="width:160px" />
        <input class="form-input" type="date" v-model="expTo"   style="width:160px" />
        <select class="form-select" v-model="expFilter" style="width:140px">
          <option value="">Tutte</option>
          <option value="unpaid">Non pagate</option>
        </select>
        <button class="btn btn-ghost btn-sm" @click="loadExpenses">Aggiorna</button>
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
                <td><span class="badge" :class="payBadge(e.paymentStatus)">{{ e.paymentStatus }}</span></td>
                <td>
                  <div class="row-actions">
                    <button v-if="canEdit && e.paymentStatus !== 'Paid'" class="btn btn-sm btn-ghost"
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
         MODAL — Crea / Modifica Budget
    ══════════════════════════════════════════════════ -->
    <div class="modal-overlay" v-if="showBudgetModal" @click.self="showBudgetModal = false">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ editingBudget ? 'Modifica budget' : 'Nuovo budget' }}</h2>
          <button class="btn-icon" @click="showBudgetModal = false">✕</button>
        </div>
        <div class="modal-body">
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
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showBudgetModal = false">Annulla</button>
          <button class="btn btn-primary" @click="saveBudget" :disabled="savingBudget">
            <span v-if="savingBudget" class="spinner" style="width:14px;height:14px"></span>
            {{ editingBudget ? 'Salva' : 'Crea' }}
          </button>
        </div>
      </div>
    </div>

    <!-- ══════════════════════════════════════════════════
         MODAL — Voci di Budget
    ══════════════════════════════════════════════════ -->
    <div class="modal-overlay" v-if="showItemsModal" @click.self="closeItemsModal">
      <div class="modal modal-lg">
        <div class="modal-header">
          <div>
            <h2>Voci di budget</h2>
            <p class="modal-subtitle" v-if="selectedBudget">
              {{ selectedBudget.type }}
              <span v-if="selectedBudget.fiscalYearCode"> · {{ selectedBudget.fiscalYearCode }}</span>
              <span class="badge ml-2" :class="statusBadge(selectedBudget.statusId)">
                {{ statusLabel(selectedBudget.statusId) }}
              </span>
            </p>
          </div>
          <button class="btn-icon" @click="closeItemsModal">✕</button>
        </div>

        <div class="modal-body">
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
        </div>

        <div class="modal-footer">
          <button class="btn btn-ghost" @click="closeItemsModal">Chiudi</button>
        </div>
      </div>
    </div>

    <!-- ══════════════════════════════════════════════════
         MODAL — Crea / Modifica Spesa
    ══════════════════════════════════════════════════ -->
    <div class="modal-overlay" v-if="showExpenseModal" @click.self="showExpenseModal = false">
      <div class="modal modal-lg">
        <div class="modal-header">
          <h2>{{ editingExp ? 'Modifica spesa' : 'Nuova spesa' }}</h2>
          <button class="btn-icon" @click="showExpenseModal = false">✕</button>
        </div>
        <div class="modal-body">
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
            <div class="form-group" :class="{ 'has-error': expErrors.expenseType }">
              <label class="form-label">Tipo spesa *</label>
              <input class="form-input" v-model="expForm.expenseType" placeholder="es. Manutenzione" @input="clearExpError('expenseType')" />
              <span v-if="expErrors.expenseType" class="field-error">{{ expErrors.expenseType }}</span>
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
                  {{ t.code }}{{ t.description ? ' – ' + t.description : '' }}
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
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showExpenseModal = false">Annulla</button>
          <button class="btn btn-primary" @click="saveExpense" :disabled="savingExp">
            <span v-if="savingExp" class="spinner" style="width:14px;height:14px"></span>
            {{ editingExp ? 'Salva' : 'Crea' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useAppStore } from '@/stores/app'
import { budgetApi, budgetItemApi, fiscalYearApi, chartOfAccountsApi, expenseApi, supplierApi, millesimalTableApi } from '@/services/api'
import { usePermissions } from '@/composables/usePermissions'

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

async function approveBudget(b) {
  const tipo = b.type === 1 ? 'preventivo' : 'consuntivo'
  if (!confirm(
    `Approvare il budget ${tipo}?\n\n` +
    `Una volta approvato, non sarà possibile approvare un altro budget dello stesso tipo ` +
    `per questo esercizio finché non verrà chiuso il corrente.`
  )) return
  try { await budgetApi.approve(b.id); await loadBudgets() } catch {}
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

function clearExpError(field) { delete expErrors.value[field] }

function validateExpForm() {
  const e = {}
  const f = expForm.value
  if (!f.name?.trim())          e.name             = 'Campo obbligatorio'
  if (!f.expenseType?.trim())   e.expenseType      = 'Campo obbligatorio'
  if (!f.documentDate)          e.documentDate     = 'Campo obbligatorio'
  if (!f.registrationDate)      e.registrationDate = 'Campo obbligatorio'
  if (!f.grossAmount || f.grossAmount <= 0) e.grossAmount = 'Inserire un importo maggiore di zero'
  if (!f.accountId)             e.accountId        = 'Selezionare un conto'
  if (!f.millesimalTableId)     e.millesimalTableId = 'Selezionare una tabella millesimale'
  expErrors.value = e
  return Object.keys(e).length === 0
}
const today           = new Date().toISOString().slice(0, 10)
const firstOfYear     = new Date(new Date().getFullYear(), 0, 1).toISOString().slice(0, 10)
const expFrom         = ref(firstOfYear)
const expTo           = ref(today)
const suppliers       = ref([])
const millesimalTables = ref([])

const emptyExpForm = () => ({
  name: '', documentNumber: '', documentDate: today, registrationDate: today,
  grossAmount: 0, vatAmount: 0, expenseType: '', paymentStatus: 'ToPay',
  paymentMethod: '', supplierId: null, accountId: null, millesimalTableId: null, description: ''
})
const expForm = ref(emptyExpForm())

async function loadExpenses() {
  if (!store.selectedCondominioId) return
  loadingExp.value = true
  try {
    const res = expFilter.value === 'unpaid'
      ? await expenseApi.getUnpaid(store.selectedCondominioId)
      : await expenseApi.getByDateRange(store.selectedCondominioId, expFrom.value, expTo.value)
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
    expenseType:        e.expenseType ?? '',
    paymentStatus:      e.paymentStatus ?? 'ToPay',
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
      expenseType:      expForm.value.expenseType,
      paymentStatus:    expForm.value.paymentStatus || 'ToPay',
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
const payBadge     = (s) => ({ ToPay: 'badge-amber', Paid: 'badge-green', Overdue: 'badge-red' }[s] ?? 'badge-muted')

// ─── Watchers / Init ──────────────────────────────────────────
watch(() => store.selectedCondominioId, async () => {
  accountsLoaded = false
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

/* Modal sizing */
.modal-lg    { max-width: 760px; width: 95vw; }
.modal-subtitle { margin: 0.15rem 0 0; font-size: 0.85rem; color: var(--text-muted); }

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

/* Form validation */
.has-error .form-input,
.has-error .form-select { border-color: var(--accent-red, #e53e3e); }
.field-error { font-size: 0.78rem; color: var(--accent-red, #e53e3e); margin-top: 0.2rem; display: block; }
</style>
