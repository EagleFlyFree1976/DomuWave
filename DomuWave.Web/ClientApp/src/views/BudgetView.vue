<template>
  <div>
    <div class="page-header">
      <h1>Budget & Spese</h1>
      <div class="flex gap-2">
        <button class="btn btn-ghost" @click="activeTab='budget'" :class="activeTab==='budget'?'btn-active':''">Budget</button>
        <button class="btn btn-ghost" @click="activeTab='spese'" :class="activeTab==='spese'?'btn-active':''">Spese</button>
      </div>
    </div>

    <!-- ── Budget tab ─────────────────────────────── -->
    <div v-if="activeTab === 'budget'">
      <div class="tab-toolbar">
        <select class="form-select" v-model="budgetYear" style="width:120px">
          <option v-for="y in years" :key="y" :value="y">{{ y }}</option>
        </select>
        <button class="btn btn-primary" @click="openBudgetModal()">+ Nuovo budget</button>
      </div>

      <div class="card">
        <div v-if="loadingBudget" class="loading-state"><div class="spinner"></div></div>
        <div v-else-if="!budgets.length" class="empty-state">
          <div class="empty-icon">◎</div>
          <div>Nessun budget per {{ budgetYear }}</div>
        </div>
        <div v-else class="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Anno</th>
                <th>Tipo</th>
                <th>Entrate</th>
                <th>Uscite</th>
                <th>Approvazione</th>
                <th>Stato</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="b in budgets" :key="b.id">
                <td class="mono">{{ b.year }}</td>
                <td>{{ b.type }}</td>
                <td class="mono text-green">{{ fmt(b.totalIncome) }}</td>
                <td class="mono text-red">{{ fmt(b.totalExpenses) }}</td>
                <td class="text-secondary">{{ fmtDate(b.approvalDate) }}</td>
                <td><span class="badge" :class="statusBadge(b.status)">{{ b.status }}</span></td>
                <td>
                  <div class="row-actions">
                    <button v-if="b.status==='Draft'" class="btn btn-sm btn-ghost" @click="approveBudget(b.id)">Approva</button>
                    <button class="btn-icon" @click="openBudgetModal(b)">✎</button>
                    <button class="btn-icon" @click="deleteBudget(b.id)" style="color:var(--accent-red)">✕</button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- ── Spese tab ───────────────────────────────── -->
    <div v-if="activeTab === 'spese'">
      <div class="tab-toolbar">
        <input class="form-input" type="date" v-model="expFrom" style="width:160px" />
        <input class="form-input" type="date" v-model="expTo"   style="width:160px" />
        <select class="form-select" v-model="expFilter" style="width:140px">
          <option value="">Tutte</option>
          <option value="unpaid">Non pagate</option>
        </select>
        <button class="btn btn-ghost btn-sm" @click="loadExpenses">Aggiorna</button>
        <button class="btn btn-primary" @click="openExpenseModal()" style="margin-left:auto">+ Nuova spesa</button>
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
                <th>Importo</th>
                <th>IVA</th>
                <th>Pagamento</th>
                <th>Stato</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="e in expenses" :key="e.id">
                <td class="mono text-secondary">{{ fmtDate(e.expenseDate) }}</td>
                <td>{{ e.supplier?.companyName || '—' }}</td>
                <td class="mono">{{ fmt(e.amount) }}</td>
                <td class="mono text-secondary">{{ fmt(e.vatAmount) }}</td>
                <td class="text-secondary">{{ e.paymentMethod || '—' }}</td>
                <td><span class="badge" :class="payBadge(e.paymentStatus)">{{ e.paymentStatus }}</span></td>
                <td>
                  <div class="row-actions">
                    <button v-if="e.paymentStatus!=='Paid'" class="btn btn-sm btn-ghost" @click="markPaid(e.id)">Paga</button>
                    <button class="btn-icon" @click="openExpenseModal(e)">✎</button>
                    <button class="btn-icon" @click="deleteExpense(e.id)" style="color:var(--accent-red)">✕</button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Budget Modal -->
    <div class="modal-overlay" v-if="showBudgetModal" @click.self="showBudgetModal=false">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ editingBudget ? 'Modifica' : 'Nuovo' }} budget</h2>
          <button class="btn-icon" @click="showBudgetModal=false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-grid">
            <div class="form-group">
              <label class="form-label">Anno *</label>
              <input class="form-input" type="number" v-model.number="budgetForm.year" />
            </div>
            <div class="form-group">
              <label class="form-label">Tipo</label>
              <select class="form-select" v-model="budgetForm.type">
                <option value="Preventivo">Preventivo</option>
                <option value="Consuntivo">Consuntivo</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Totale entrate (€)</label>
              <input class="form-input" type="number" step="0.01" v-model.number="budgetForm.totalIncome" />
            </div>
            <div class="form-group">
              <label class="form-label">Totale spese (€)</label>
              <input class="form-input" type="number" step="0.01" v-model.number="budgetForm.totalExpenses" />
            </div>
          </div>
          <div class="form-group">
            <label class="form-label">Note</label>
            <textarea class="form-textarea" v-model="budgetForm.notes" rows="2"></textarea>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showBudgetModal=false">Annulla</button>
          <button class="btn btn-primary" @click="saveBudget" :disabled="savingBudget">
            <span v-if="savingBudget" class="spinner" style="width:14px;height:14px"></span>
            {{ editingBudget ? 'Salva' : 'Crea' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Expense Modal -->
    <div class="modal-overlay" v-if="showExpenseModal" @click.self="showExpenseModal=false">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ editingExp ? 'Modifica' : 'Nuova' }} spesa</h2>
          <button class="btn-icon" @click="showExpenseModal=false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-grid">
            <div class="form-group">
              <label class="form-label">Data spesa *</label>
              <input class="form-input" type="date" v-model="expForm.expenseDate" />
            </div>
            <div class="form-group">
              <label class="form-label">Importo (€) *</label>
              <input class="form-input" type="number" step="0.01" v-model.number="expForm.amount" />
            </div>
            <div class="form-group">
              <label class="form-label">IVA (€)</label>
              <input class="form-input" type="number" step="0.01" v-model.number="expForm.vatAmount" />
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
            <div class="form-group">
              <label class="form-label">Tipo spesa</label>
              <input class="form-input" v-model="expForm.expenseType" />
            </div>
            <div class="form-group">
              <label class="form-label">N° fattura</label>
              <input class="form-input" v-model="expForm.invoiceNumber" />
            </div>
          </div>
          <div class="form-group">
            <label class="form-label">Descrizione</label>
            <textarea class="form-textarea" v-model="expForm.description" rows="2"></textarea>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showExpenseModal=false">Annulla</button>
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
import { ref, onMounted, watch } from 'vue'
import { useAppStore } from '@/store/app'
import { budgetApi, expenseApi } from '@/services/api'

const store = useAppStore()
const activeTab = ref('budget')

// Budget
const budgets = ref([])
const loadingBudget = ref(false)
const showBudgetModal = ref(false)
const editingBudget = ref(null)
const savingBudget = ref(false)
const budgetYear = ref(new Date().getFullYear())
const years = Array.from({ length: 5 }, (_, i) => new Date().getFullYear() - 2 + i)
const budgetForm = ref({ year: budgetYear.value, type: 'Preventivo', totalIncome: 0, totalExpenses: 0, notes: '', status: 'Draft' })

// Spese
const expenses = ref([])
const loadingExp = ref(false)
const showExpenseModal = ref(false)
const editingExp = ref(null)
const savingExp = ref(false)
const expFilter = ref('')
const today = new Date().toISOString().slice(0, 10)
const firstOfYear = new Date(new Date().getFullYear(), 0, 1).toISOString().slice(0, 10)
const expFrom = ref(firstOfYear)
const expTo = ref(today)
const expForm = ref({ expenseDate: today, amount: 0, vatAmount: 0, paymentMethod: '', expenseType: '', invoiceNumber: '', description: '' })

const fmt = (v) => v != null ? '€ ' + Number(v).toLocaleString('it-IT', { minimumFractionDigits: 2 }) : '—'
const fmtDate = (d) => d ? new Date(d).toLocaleDateString('it-IT') : '—'
const statusBadge = (s) => ({ Draft: 'badge-muted', Approved: 'badge-green', Closed: 'badge-purple' }[s] || 'badge-muted')
const payBadge = (s) => ({ ToPay: 'badge-amber', Paid: 'badge-green', Overdue: 'badge-red' }[s] || 'badge-muted')

async function loadBudgets() {
  if (!store.selectedCondominioId) return
  loadingBudget.value = true
  try {
    const { data } = await budgetApi.getByYear(store.selectedCondominioId, budgetYear.value)
    budgets.value = data
  } catch { budgets.value = [] } finally { loadingBudget.value = false }
}

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

function openBudgetModal(b = null) {
  editingBudget.value = b?.id ?? null
  budgetForm.value = b ? { ...b } : { year: budgetYear.value, type: 'Preventivo', totalIncome: 0, totalExpenses: 0, notes: '', status: 'Draft' }
  showBudgetModal.value = true
}

async function saveBudget() {
  savingBudget.value = true
  try {
    if (editingBudget.value) {
      await budgetApi.update(editingBudget.value, budgetForm.value)
    } else {
      await budgetApi.create({ ...budgetForm.value, condominiumId: store.selectedCondominioId })
    }
    store.toast('Budget salvato', 'success')
    showBudgetModal.value = false
    loadBudgets()
  } catch { store.toast('Errore', 'error') } finally { savingBudget.value = false }
}

async function approveBudget(id) {
  try { await budgetApi.approve(id); store.toast('Budget approvato', 'success'); loadBudgets() }
  catch { store.toast('Errore', 'error') }
}

async function deleteBudget(id) {
  if (!confirm('Eliminare il budget?')) return
  try { await budgetApi.delete(id); store.toast('Budget eliminato', 'success'); loadBudgets() }
  catch { store.toast('Errore', 'error') }
}

function openExpenseModal(e = null) {
  editingExp.value = e?.id ?? null
  expForm.value = e ? { ...e } : { expenseDate: today, amount: 0, vatAmount: 0, paymentMethod: '', expenseType: '', invoiceNumber: '', description: '' }
  showExpenseModal.value = true
}

async function saveExpense() {
  savingExp.value = true
  try {
    if (editingExp.value) { await expenseApi.update(editingExp.value, expForm.value) }
    else { await expenseApi.create({ ...expForm.value, condominiumId: store.selectedCondominioId }) }
    store.toast('Spesa salvata', 'success')
    showExpenseModal.value = false
    loadExpenses()
  } catch { store.toast('Errore', 'error') } finally { savingExp.value = false }
}

async function markPaid(id) {
  try {
    await expenseApi.markAsPaid(id, today, 'BankTransfer')
    store.toast('Spesa segnata come pagata', 'success')
    loadExpenses()
  } catch { store.toast('Errore', 'error') }
}

async function deleteExpense(id) {
  if (!confirm('Eliminare questa spesa?')) return
  try { await expenseApi.delete(id); store.toast('Spesa eliminata', 'success'); loadExpenses() }
  catch { store.toast('Errore', 'error') }
}

watch(() => store.selectedCondominioId, () => { loadBudgets(); loadExpenses() })
watch(budgetYear, loadBudgets)
onMounted(() => { loadBudgets(); loadExpenses() })
</script>

<style scoped>
.tab-toolbar { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1rem; flex-wrap: wrap; }
.btn-active { background: var(--accent-glow) !important; color: var(--accent) !important; border-color: var(--border-active) !important; }
.row-actions { display: flex; gap: 0.4rem; justify-content: flex-end; }
</style>
