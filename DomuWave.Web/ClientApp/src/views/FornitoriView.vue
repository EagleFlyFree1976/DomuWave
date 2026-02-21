<template>
  <div>
    <div class="page-header">
      <h1>Fornitori</h1>
      <div class="flex gap-2">
        <button class="btn btn-ghost" @click="activeTab='fornitori'" :class="activeTab==='fornitori'?'btn-active':''">Fornitori</button>
        <button class="btn btn-ghost" @click="activeTab='contratti'" :class="activeTab==='contratti'?'btn-active':''">Contratti</button>
      </div>
    </div>

    <!-- ── Fornitori ──────────────────────────────── -->
    <div v-if="activeTab === 'fornitori'">
      <div class="toolbar">
        <input class="form-input search-input" v-model="search" placeholder="Cerca per nome, P.IVA…" @input="onSearch" />
        <select class="form-select" v-model="filterType" style="width:160px">
          <option value="">Tutti i tipi</option>
          <option value="Maintenance">Manutenzione</option>
          <option value="Cleaning">Pulizie</option>
          <option value="Security">Sicurezza</option>
          <option value="Utilities">Utenze</option>
          <option value="Professional">Professionale</option>
        </select>
        <button class="btn btn-primary" @click="openModal()">+ Nuovo fornitore</button>
      </div>

      <div class="card">
        <div v-if="loading" class="loading-state"><div class="spinner"></div></div>
        <div v-else-if="!filtered.length" class="empty-state">
          <div class="empty-icon">◈</div><div>Nessun fornitore trovato</div>
        </div>
        <div v-else class="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Ragione sociale</th>
                <th>P.IVA</th>
                <th>Tipo</th>
                <th>Email</th>
                <th>Telefono</th>
                <th>Stato</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="s in filtered" :key="s.id">
                <td style="font-weight:500">{{ s.companyName }}</td>
                <td class="mono text-muted">{{ s.vatNumber || '—' }}</td>
                <td>{{ s.supplierType || '—' }}</td>
                <td class="text-secondary">{{ s.email || '—' }}</td>
                <td class="text-secondary">{{ s.phone || '—' }}</td>
                <td><span class="badge" :class="s.isActive ? 'badge-green' : 'badge-muted'">{{ s.isActive ? 'Attivo' : 'Inattivo' }}</span></td>
                <td>
                  <div class="row-actions">
                    <button class="btn-icon" @click="openModal(s)">✎</button>
                    <button class="btn-icon" @click="deleteItem(s.id)" style="color:var(--accent-red)">✕</button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- ── Contratti ──────────────────────────────── -->
    <div v-if="activeTab === 'contratti'">
      <div class="toolbar">
        <select class="form-select" v-model="contractFilter" style="width:160px">
          <option value="all">Tutti</option>
          <option value="active">Attivi</option>
          <option value="expiring">In scadenza (30gg)</option>
        </select>
        <button class="btn btn-primary" @click="openContractModal()" style="margin-left:auto">+ Nuovo contratto</button>
      </div>

      <div class="card">
        <div v-if="loadingContracts" class="loading-state"><div class="spinner"></div></div>
        <div v-else-if="!contracts.length" class="empty-state">
          <div class="empty-icon">◈</div><div>Nessun contratto trovato</div>
        </div>
        <div v-else class="table-wrap">
          <table>
            <thead>
              <tr>
                <th>N° contratto</th>
                <th>Oggetto</th>
                <th>Fornitore</th>
                <th>Inizio</th>
                <th>Fine</th>
                <th>Importo annuo</th>
                <th>Stato</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="c in contracts" :key="c.id">
                <td class="mono text-muted">{{ c.contractNumber || '—' }}</td>
                <td style="font-weight:500">{{ c.subject }}</td>
                <td class="text-secondary">{{ c.supplier?.companyName || '—' }}</td>
                <td class="mono text-secondary">{{ fmtDate(c.startDate) }}</td>
                <td class="mono" :class="isExpiring(c.endDate) ? 'text-amber' : 'text-secondary'">{{ fmtDate(c.endDate) }}</td>
                <td class="mono">{{ c.annualAmount ? fmt(c.annualAmount) : '—' }}</td>
                <td><span class="badge" :class="contractBadge(c.status)">{{ c.status }}</span></td>
                <td>
                  <div class="row-actions">
                    <button class="btn-icon" @click="openContractModal(c)">✎</button>
                    <button class="btn-icon" @click="deleteContract(c.id)" style="color:var(--accent-red)">✕</button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Fornitore Modal -->
    <div class="modal-overlay" v-if="showModal" @click.self="showModal=false">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ editing ? 'Modifica' : 'Nuovo' }} fornitore</h2>
          <button class="btn-icon" @click="showModal=false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-grid">
            <div class="form-group" style="grid-column:1/-1">
              <label class="form-label">Ragione sociale *</label>
              <input class="form-input" v-model="form.companyName" />
            </div>
            <div class="form-group">
              <label class="form-label">P.IVA</label>
              <input class="form-input" v-model="form.vatNumber" maxlength="11" />
            </div>
            <div class="form-group">
              <label class="form-label">Cod. fiscale</label>
              <input class="form-input" v-model="form.taxCode" maxlength="16" />
            </div>
            <div class="form-group">
              <label class="form-label">Email</label>
              <input class="form-input" type="email" v-model="form.email" />
            </div>
            <div class="form-group">
              <label class="form-label">Telefono</label>
              <input class="form-input" v-model="form.phone" />
            </div>
            <div class="form-group">
              <label class="form-label">PEC</label>
              <input class="form-input" v-model="form.pec" />
            </div>
            <div class="form-group">
              <label class="form-label">Tipo fornitore</label>
              <select class="form-select" v-model="form.supplierType">
                <option value="">—</option>
                <option value="Maintenance">Manutenzione</option>
                <option value="Cleaning">Pulizie</option>
                <option value="Security">Sicurezza</option>
                <option value="Utilities">Utenze</option>
                <option value="Professional">Professionale</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">IBAN</label>
              <input class="form-input" v-model="form.ibanAccount" />
            </div>
            <div class="form-group">
              <label class="form-label">Indirizzo</label>
              <input class="form-input" v-model="form.address" />
            </div>
            <div class="form-group">
              <label class="form-label">Città</label>
              <input class="form-input" v-model="form.city" />
            </div>
          </div>
          <div class="form-group">
            <label class="form-label">Note</label>
            <textarea class="form-textarea" v-model="form.notes" rows="2"></textarea>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showModal=false">Annulla</button>
          <button class="btn btn-primary" @click="save" :disabled="saving">
            <span v-if="saving" class="spinner" style="width:14px;height:14px"></span>
            {{ editing ? 'Salva' : 'Crea' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Contratto Modal -->
    <div class="modal-overlay" v-if="showContractModal" @click.self="showContractModal=false">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ editingContract ? 'Modifica' : 'Nuovo' }} contratto</h2>
          <button class="btn-icon" @click="showContractModal=false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-grid">
            <div class="form-group" style="grid-column:1/-1">
              <label class="form-label">Oggetto *</label>
              <input class="form-input" v-model="contractForm.subject" />
            </div>
            <div class="form-group">
              <label class="form-label">N° contratto</label>
              <input class="form-input" v-model="contractForm.contractNumber" />
            </div>
            <div class="form-group">
              <label class="form-label">Fornitore</label>
              <select class="form-select" v-model.number="contractForm.supplierId">
                <option value="">— Scegli —</option>
                <option v-for="s in suppliers" :key="s.id" :value="s.id">{{ s.companyName }}</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Data inizio</label>
              <input class="form-input" type="date" v-model="contractForm.startDate" />
            </div>
            <div class="form-group">
              <label class="form-label">Data fine</label>
              <input class="form-input" type="date" v-model="contractForm.endDate" />
            </div>
            <div class="form-group">
              <label class="form-label">Importo annuo (€)</label>
              <input class="form-input" type="number" step="0.01" v-model.number="contractForm.annualAmount" />
            </div>
            <div class="form-group">
              <label class="form-label">Stato</label>
              <select class="form-select" v-model="contractForm.status">
                <option value="Active">Attivo</option>
                <option value="Expired">Scaduto</option>
                <option value="Cancelled">Annullato</option>
              </select>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showContractModal=false">Annulla</button>
          <button class="btn btn-primary" @click="saveContract" :disabled="savingContract">
            <span v-if="savingContract" class="spinner" style="width:14px;height:14px"></span>
            {{ editingContract ? 'Salva' : 'Crea' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useAppStore } from '@/stores/app'
import { supplierApi } from '@/services/api'

const store = useAppStore()
const activeTab = ref('fornitori')
const suppliers = ref([])
const loading = ref(false)
const saving = ref(false)
const showModal = ref(false)
const editing = ref(null)
const search = ref('')
const filterType = ref('')
const contracts = ref([])
const loadingContracts = ref(false)
const contractFilter = ref('all')
const showContractModal = ref(false)
const editingContract = ref(null)
const savingContract = ref(false)

const fmt = (v) => v != null ? '€ ' + Number(v).toLocaleString('it-IT', { minimumFractionDigits: 2 }) : '—'
const fmtDate = (d) => d ? new Date(d).toLocaleDateString('it-IT') : '—'
const isExpiring = (d) => { if (!d) return false; const diff = (new Date(d) - new Date()) / 86400000; return diff >= 0 && diff <= 30 }
const contractBadge = (s) => ({ Active: 'badge-green', Expired: 'badge-red', Cancelled: 'badge-muted' }[s] || 'badge-muted')

const defaultForm = () => ({ companyName: '', vatNumber: '', taxCode: '', email: '', phone: '', pec: '', supplierType: '', ibanAccount: '', address: '', city: '', province: '', postalCode: '', notes: '', isActive: true })
const form = ref(defaultForm())
const contractForm = ref({ subject: '', contractNumber: '', supplierId: '', startDate: '', endDate: '', annualAmount: null, status: 'Active', autoRenewal: false })

const filtered = computed(() => {
  let list = suppliers.value
  if (filterType.value) list = list.filter(s => s.supplierType === filterType.value)
  if (search.value) {
    const q = search.value.toLowerCase()
    list = list.filter(s => s.companyName?.toLowerCase().includes(q) || s.vatNumber?.includes(q))
  }
  return list
})

async function loadData() {
  loading.value = true
  try {
    const { data } = await supplierApi.getAll()
    suppliers.value = data
  } catch { suppliers.value = [] } finally { loading.value = false }
}

async function loadContracts() {
  if (!store.selectedCondominioId) return
  loadingContracts.value = true
  try {
    const res = contractFilter.value === 'active'
      ? await supplierApi.getActiveContracts(store.selectedCondominioId)
      : contractFilter.value === 'expiring'
        ? await supplierApi.getExpiringContracts(store.selectedCondominioId, 30)
        : await supplierApi.getActiveContracts(store.selectedCondominioId)
    contracts.value = res.data
  } catch { contracts.value = [] } finally { loadingContracts.value = false }
}

function onSearch() { /* debounce optionally */ }

function openModal(s = null) {
  editing.value = s?.id ?? null
  form.value = s ? { ...s } : defaultForm()
  showModal.value = true
}

async function save() {
  if (!form.value.companyName) return store.toast('La ragione sociale è obbligatoria', 'error')
  saving.value = true
  try {
    if (editing.value) await supplierApi.update(editing.value, form.value)
    else await supplierApi.create(form.value)
    store.toast('Fornitore salvato', 'success')
    showModal.value = false
    loadData()
  } catch { store.toast('Errore', 'error') } finally { saving.value = false }
}

async function deleteItem(id) {
  if (!confirm('Eliminare il fornitore?')) return
  try { await supplierApi.delete(id); store.toast('Fornitore eliminato', 'success'); loadData() }
  catch { store.toast('Errore', 'error') }
}

function openContractModal(c = null) {
  editingContract.value = c?.id ?? null
  contractForm.value = c ? { ...c } : { subject: '', contractNumber: '', supplierId: '', startDate: '', endDate: '', annualAmount: null, status: 'Active', autoRenewal: false, condominiumId: store.selectedCondominioId }
  showContractModal.value = true
}

async function saveContract() {
  savingContract.value = true
  try {
    if (editingContract.value) await supplierApi.updateContract(editingContract.value, contractForm.value)
    else await supplierApi.createContract({ ...contractForm.value, condominiumId: store.selectedCondominioId })
    store.toast('Contratto salvato', 'success')
    showContractModal.value = false
    loadContracts()
  } catch { store.toast('Errore', 'error') } finally { savingContract.value = false }
}

async function deleteContract(id) {
  if (!confirm('Eliminare il contratto?')) return
  try { await supplierApi.deleteContract(id); store.toast('Contratto eliminato', 'success'); loadContracts() }
  catch { store.toast('Errore', 'error') }
}

watch(() => store.selectedCondominioId, loadContracts)
watch(contractFilter, loadContracts)
watch(activeTab, (t) => { if (t === 'contratti') loadContracts() })
onMounted(() => { loadData(); loadContracts() })
</script>

<style scoped>
.toolbar { display: flex; gap: 0.75rem; margin-bottom: 1rem; flex-wrap: wrap; align-items: center; }
.search-input { flex: 1; min-width: 200px; max-width: 320px; }
.row-actions { display: flex; gap: 0.4rem; justify-content: flex-end; }
.btn-active { background: var(--accent-glow) !important; color: var(--accent) !important; border-color: var(--border-active) !important; }
</style>
