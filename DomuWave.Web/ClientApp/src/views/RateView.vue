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
        <select class="form-select" v-model="instYear" style="width:110px">
          <option v-for="y in years" :key="y" :value="y">{{ y }}</option>
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
                <th>Descrizione</th>
                <th>Scadenza</th>
                <th>Importo</th>
                <th>Stato</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="i in installments" :key="i.id">
                <td class="mono text-muted">{{ i.installmentNumber }}</td>
                <td>{{ i.description }}</td>
                <td class="mono" :class="isOverdue(i.dueDate) ? 'text-red' : 'text-secondary'">{{ fmtDate(i.dueDate) }}</td>
                <td class="mono">{{ fmt(i.totalAmount) }}</td>
                <td><span class="badge" :class="instBadge(i.status)">{{ i.status }}</span></td>
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
        <select class="form-select" v-model="selectedInstId" style="width:260px" v-if="allInstallments.length">
          <option value="">— Scegli rata —</option>
          <option v-for="i in allInstallments" :key="i.id" :value="i.id">
            {{ i.installmentNumber }} - {{ i.description }} ({{ fmtDate(i.dueDate) }})
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
            <div class="form-group">
              <label class="form-label">Anno *</label>
              <input class="form-input" type="number" v-model.number="instForm.year" />
            </div>
            <div class="form-group">
              <label class="form-label">N° rata *</label>
              <input class="form-input" type="number" v-model.number="instForm.installmentNumber" />
            </div>
            <div class="form-group">
              <label class="form-label">Scadenza *</label>
              <input class="form-input" type="date" v-model="instForm.dueDate" />
            </div>
            <div class="form-group">
              <label class="form-label">Importo (€) *</label>
              <input class="form-input" type="number" step="0.01" v-model.number="instForm.totalAmount" />
            </div>
          </div>
          <div class="form-group">
            <label class="form-label">Descrizione *</label>
            <input class="form-input" v-model="instForm.description" />
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
import { installmentApi, feeApi } from '@/services/api'
import { usePermissions } from '@/composables/usePermissions'

const store = useAppStore()
const { canCreate, canEdit, canDelete } = usePermissions()
const activeTab = ref('rate')
const instYear = ref(new Date().getFullYear())
const instFilter = ref('all')
const years = Array.from({ length: 5 }, (_, i) => new Date().getFullYear() - 2 + i)

const installments = ref([])
const allInstallments = ref([])
const loadingInst = ref(false)
const showInstModal = ref(false)
const editingInst = ref(null)
const savingInst = ref(false)
const instForm = ref({})

const fees = ref([])
const loadingFees = ref(false)
const selectedInstId = ref('')
const showFeeModal = ref(false)
const editingFee = ref(null)

const fmt = (v) => v != null ? '€ ' + Number(v).toLocaleString('it-IT', { minimumFractionDigits: 2 }) : '—'
const fmtDate = (d) => d ? new Date(d).toLocaleDateString('it-IT') : '—'
const isOverdue = (d) => d && new Date(d) < new Date()
const instBadge = (s) => ({ Open: 'badge-blue', Closed: 'badge-green', Cancelled: 'badge-red' }[s] || 'badge-muted')
const feeBadge = (s) => ({ ToPay: 'badge-amber', Paid: 'badge-green', Overdue: 'badge-red', PartiallyPaid: 'badge-purple' }[s] || 'badge-muted')

async function loadInstallments() {
  if (!store.selectedCondominioId) return
  loadingInst.value = true
  try {
    const fetchFn = instFilter.value === 'open' ? installmentApi.getOpen
      : instFilter.value === 'overdue' ? installmentApi.getOverdue
      : null
    const { data } = fetchFn
      ? await fetchFn(store.selectedCondominioId)
      : await installmentApi.getByYear(store.selectedCondominioId, instYear.value)
    installments.value = data
  } catch { installments.value = [] } finally { loadingInst.value = false }
}

async function loadAllInstallments() {
  if (!store.selectedCondominioId) return
  try {
    const { data } = await installmentApi.getByCondominium(store.selectedCondominioId)
    allInstallments.value = data
  } catch { allInstallments.value = [] }
}

async function loadFees() {
  if (!selectedInstId.value) return
  loadingFees.value = true
  try {
    const { data } = await feeApi.getByInstallment(selectedInstId.value)
    fees.value = data
  } catch { fees.value = [] } finally { loadingFees.value = false }
}

function openInstModal(i = null) {
  editingInst.value = i?.id ?? null
  instForm.value = i ? { ...i } : { year: instYear.value, installmentNumber: 1, description: '', dueDate: '', totalAmount: 0, notes: '', status: 'Open' }
  showInstModal.value = true
}

async function saveInst() {
  savingInst.value = true
  try {
    if (editingInst.value) await installmentApi.update(editingInst.value, instForm.value)
    else await installmentApi.create({ ...instForm.value, condominiumId: store.selectedCondominioId })
    store.toast('Rata salvata', 'success')
    showInstModal.value = false
    loadInstallments()
  } catch { store.toast('Errore', 'error') } finally { savingInst.value = false }
}

async function deleteInst(id) {
  if (!confirm('Eliminare la rata?')) return
  try { await installmentApi.delete(id); store.toast('Rata eliminata', 'success'); loadInstallments() }
  catch { store.toast('Errore', 'error') }
}

function openFeeModal(f = null) {
  editingFee.value = f?.id ?? null
  showFeeModal.value = true
}

async function deleteFee(id) {
  if (!confirm('Eliminare questa quota?')) return
  try { await feeApi.delete(id); store.toast('Quota eliminata', 'success'); loadFees() }
  catch { store.toast('Errore', 'error') }
}

watch(() => store.selectedCondominioId, () => { loadInstallments(); loadAllInstallments() })
watch([instYear, instFilter], loadInstallments)
watch(selectedInstId, loadFees)
watch(activeTab, (t) => { if (t === 'quote') loadAllInstallments() })
onMounted(() => { loadInstallments(); loadAllInstallments() })
</script>

<style scoped>
.tab-toolbar { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1rem; flex-wrap: wrap; }
.btn-active { background: var(--accent-glow) !important; color: var(--accent) !important; border-color: var(--border-active) !important; }
.row-actions { display: flex; gap: 0.4rem; justify-content: flex-end; }
</style>
