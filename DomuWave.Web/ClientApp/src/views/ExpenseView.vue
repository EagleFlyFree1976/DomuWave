<template>
  <div>
    <!-- ── Header ──────────────────────────────────────── -->
    <div class="page-header">
      <h1>Spese</h1>
      <button v-if="canCreate" class="btn btn-primary" @click="openExpenseModal()">+ Nuova spesa</button>
    </div>


    <!-- ── Toolbar filtri ──────────────────────────────── -->
    <div class="tab-toolbar">
      <input class="form-input" v-model="search" placeholder="Cerca descrizione, n° doc…"
             style="min-width:200px" @input="onSearchInput" />

      <select class="form-select" v-model.number="selectedFiscalYearId" style="min-width:160px">
        <option :value="null">Tutti gli esercizi</option>
        <option v-for="fy in fiscalYears" :key="fy.id" :value="fy.id">
          {{ fy.code }}{{ fy.description ? ' – ' + fy.description : '' }}
        </option>
      </select>

      <select class="form-select" v-model.number="expTypeFilter" style="width:150px" @change="resetAndLoad">
        <option :value="null">Tutti i tipi</option>
        <option :value="1">Manutenzione</option>
        <option :value="2">Pulizie</option>
        <option :value="3">Sicurezza</option>
        <option :value="4">Utenze</option>
        <option :value="5">Professionale</option>
        <option :value="6">Altro</option>
      </select>

      <select class="form-select" v-model.number="paymentStatusFilter" style="width:140px" @change="resetAndLoad">
        <option :value="null">Tutti gli stati</option>
        <option :value="1">Da pagare</option>
        <option :value="2">Pagata</option>
      </select>
    </div>

    <!-- ── Tabella ──────────────────────────────────────── -->
    <div class="card">
      <div v-if="loadingExp" class="loading-state"><div class="spinner"></div></div>
      <div v-else-if="!expenses.length" class="empty-state">
        <div class="empty-icon">◎</div><div>Nessuna spesa trovata</div>
      </div>
      <div v-else class="table-wrap">
        <AppPaginator v-model="currentPage" v-model:pageSize="pageSize" :total-count="totalCount" />
        <table>
          <thead>
            <tr>
              <th class="sortable" @click="setSort('documentdate')">
                Data <span class="sort-icon">{{ sortIcon('documentdate') }}</span>
              </th>
              <th>Descrizione</th>
              <th class="sortable" @click="setSort('suppliername')">
                Fornitore <span class="sort-icon">{{ sortIcon('suppliername') }}</span>
              </th>
              <th>Esercizio</th>
              <th class="text-right sortable" @click="setSort('grossamount')">
                Importo <span class="sort-icon">{{ sortIcon('grossamount') }}</span>
              </th>
              <th class="text-right">IVA</th>
              <th>Pagamento</th>
              <th>Stato</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="e in expenses" :key="e.id">
              <td class="mono text-secondary">{{ fmtDate(e.documentDate) }}</td>
              <td>{{ e.name }}</td>
              <td>{{ e.supplierName || '—' }}</td>
              <td class="text-secondary">{{ e.fiscalYearCode || '—' }}</td>
              <td class="mono text-right">{{ fmt(e.grossAmount) }}</td>
              <td class="mono text-right text-secondary">{{ fmt(e.vatAmount) }}</td>
              <td class="text-secondary">{{ e.paymentMethodName || '—' }}</td>
              <td><span class="badge" :class="payBadge(e.paymentStatusId)">{{ e.paymentStatusName }}</span></td>
              <td>
                <div class="row-actions">
                  <button v-if="canEdit && e.paymentStatusId !== 2" class="btn btn-sm btn-ghost"
                          :disabled="actionInProgress[`pay-${e.id}`]"
                          @click="markPaid(e.id)">
                    <span v-if="actionInProgress[`pay-${e.id}`]" class="spinner" style="width:12px;height:12px"></span>
                    <span v-else>Paga</span>
                  </button>
                  <button v-if="canEdit && e.paymentStatusId === 2" class="btn btn-sm btn-ghost"
                          :disabled="actionInProgress[`pay-${e.id}`]"
                          @click="markUnpaid(e.id)">
                    <span v-if="actionInProgress[`pay-${e.id}`]" class="spinner" style="width:12px;height:12px"></span>
                    <span v-else>Segna non pagata</span>
                  </button>
                  <button v-if="canEdit" class="btn-icon"
                          :disabled="actionInProgress[`pay-${e.id}`] || actionInProgress[`del-${e.id}`]"
                          @click="openExpenseModal(e)">✎</button>
                  <button v-if="canDelete" class="btn-icon" style="color:var(--accent-red)"
                          :disabled="actionInProgress[`del-${e.id}`]"
                          @click="deleteExpense(e.id)">
                    <span v-if="actionInProgress[`del-${e.id}`]" class="spinner" style="width:12px;height:12px"></span>
                    <span v-else>✕</span>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        <AppPaginator v-model="currentPage" v-model:pageSize="pageSize" :total-count="totalCount" />
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
      <!-- ── Blocco: nessun esercizio fiscale disponibile ─────────── -->
      <div v-if="noFiscalYearAvailable && !editingExp" class="no-fiscal-year-banner">
        <div class="no-fiscal-year-icon">📅</div>
        <div>
          <strong>Nessun esercizio fiscale disponibile</strong>
          <div class="no-fiscal-year-desc">
            Per inserire una spesa è necessario avere almeno un esercizio fiscale in stato
            <strong>Aperto</strong> o <strong>In chiusura</strong>.<br/>
            Vai alla sezione <em>Esercizi fiscali</em> e apri un esercizio prima di procedere.
          </div>
        </div>
      </div>

      <template v-if="!noFiscalYearAvailable || editingExp">
      <!-- ── Esercizio fiscale ──────────────────────────────────── -->
      <div class="form-group form-group--full" :class="{ 'has-error': expErrors.fiscalYearId }">
        <label class="form-label">Esercizio fiscale *</label>
        <select class="form-select" v-model.number="expForm.fiscalYearId"
                @change="clearExpError('fiscalYearId')">
          <option :value="null" disabled>Seleziona esercizio…</option>
          <option v-for="fy in selectableFiscalYears" :key="fy.id" :value="fy.id">
            {{ fy.code }}{{ fy.description ? ' – ' + fy.description : '' }}
            ({{ fy.startDate?.slice(0,10) }} / {{ fy.endDate?.slice(0,10) }})
            {{ fy.statusId === 3 ? '· In chiusura' : '' }}
          </option>
        </select>
        <span v-if="expErrors.fiscalYearId" class="field-error">{{ expErrors.fiscalYearId }}</span>
      </div>

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
        <div class="form-group">
          <label class="form-label">Data documento</label>
          <input class="form-input" type="date" v-model="expForm.documentDate" />
        </div>
        <div class="form-group">
          <label class="form-label">Data registrazione</label>
          <input class="form-input" type="date" v-model="expForm.registrationDate" />
        </div>
        <!-- Errore base imponibile -->
        <div v-if="expErrors.taxableBase" class="field-error" style="grid-column:span 2;margin-bottom:0.25rem">
          ⚠ {{ expErrors.taxableBase }}
        </div>
        <!-- Imponibile / Imponibile esente IVA — almeno uno obbligatorio -->
        <div class="form-group" :class="{ 'has-error': expErrors.taxableAmount }">
          <label class="form-label">
            Imponibile (€)
            <span class="label-hint">obbligatorio se non esente IVA</span>
          </label>
          <input class="form-input" type="number" step="0.01" min="0"
                 v-model.number="expForm.taxableAmount"
                 @input="clearExpError('taxableAmount'); clearExpError('taxableBase'); autoCalcGross()" />
          <span v-if="expErrors.taxableAmount" class="field-error">{{ expErrors.taxableAmount }}</span>
        </div>
        <div class="form-group" :class="{ 'has-error': expErrors.taxableAmountVatExempt }">
          <label class="form-label">
            Imp. es. IVA (€)
            <span class="label-hint">obbl. se non imponibile</span>
          </label>
          <input class="form-input" type="number" step="0.01" min="0"
                 v-model.number="expForm.taxableAmountVatExempt"
                 @input="clearExpError('taxableAmountVatExempt'); clearExpError('taxableBase'); autoCalcGross()" />
          <span v-if="expErrors.taxableAmountVatExempt" class="field-error">{{ expErrors.taxableAmountVatExempt }}</span>
        </div>
        <div class="form-group" :class="{ 'has-error': expErrors.vatAmount }">
          <label class="form-label">
            IVA / Imposta (€)
            <span v-if="expForm.taxableAmount > 0" class="label-required">*</span>
          </label>
          <input class="form-input" type="number" step="0.01" min="0"
                 v-model.number="expForm.vatAmount"
                 @input="clearExpError('vatAmount'); autoCalcGross()" />
          <span v-if="expErrors.vatAmount" class="field-error">{{ expErrors.vatAmount }}</span>
        </div>
        <div class="form-group">
          <label class="form-label">Cassa previdenziale (€)</label>
          <input class="form-input" type="number" step="0.01" min="0"
                 v-model.number="expForm.pensionFund"
                 @input="autoCalcGross()" />
        </div>
        <div class="form-group">
          <label class="form-label">Ritenuta d'acconto (€)</label>
          <input class="form-input" type="number" step="0.01" min="0"
                 v-model.number="expForm.withholdingTax"
                 @input="autoCalcGross()" />
        </div>
        <div class="form-group">
          <label class="form-label">Bollo (€)</label>
          <input class="form-input" type="number" step="0.01" min="0"
                 v-model.number="expForm.stampDuty"
                 @input="autoCalcGross()" />
        </div>
        <div class="form-group" style="grid-column: span 2">
          <label class="form-label">Importo lordo (€) <span class="label-hint">calcolato automaticamente</span></label>
          <input class="form-input" type="number" step="0.01"
                 v-model.number="expForm.grossAmount" readonly
                 style="background:var(--bg-base);cursor:default" />
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
        <div class="form-group" style="grid-column:span 2" :class="{ 'has-error': expErrors.allocation }">
          <label class="form-label">Imputazione spesa *</label>
          <div class="radio-group" style="margin-bottom:0.4rem">
            <label class="radio-label">
              <input type="radio" value="table" v-model="expForm.allocationMode" @change="onAllocationModeChange" />
              Tabella millesimale
            </label>
            <label class="radio-label">
              <input type="radio" value="unit" v-model="expForm.allocationMode" @change="onAllocationModeChange" />
              Immobile specifico
            </label>
          </div>
          <select v-if="expForm.allocationMode === 'table'" class="form-select"
                  v-model.number="expForm.millesimalTableId" @change="clearExpError('allocation')">
            <option :value="null" disabled>Seleziona tabella…</option>
            <option v-for="t in enabledMillesimalTables" :key="t.id" :value="t.id">
              {{ t.code }}{{ t.name ? ' – ' + t.name : '' }}{{ !t.isEnabled ? ' (disabilitata)' : '' }}
            </option>
          </select>
          <select v-else class="form-select"
                  v-model.number="expForm.unitId" @change="clearExpError('allocation')">
            <option :value="null" disabled>Seleziona immobile…</option>
            <option v-for="u in units" :key="u.id" :value="u.id">
              {{ u.displayName || u.name || u.internalNumber }}
            </option>
          </select>
          <span v-if="expErrors.allocation" class="field-error">{{ expErrors.allocation }}</span>
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
          <select class="form-select" v-model.number="expForm.paymentMethodId">
            <option :value="null">—</option>
            <option v-for="m in paymentMethods" :key="m.id" :value="m.id">{{ m.name }}</option>
          </select>
        </div>
      </div>
      <div class="form-group form-group--full" style="margin-top:0.5rem">
        <label class="form-label">A carico di</label>
        <div class="charge-row">
          <label class="radio-label">
            <input type="radio" v-model.number="expForm.chargeabilityTypeId" :value="1" />
            Proprietario
          </label>
          <label class="radio-label">
            <input type="radio" v-model.number="expForm.chargeabilityTypeId" :value="2" />
            Inquilino (se presente)
          </label>
          <label class="radio-label">
            <input type="radio" v-model.number="expForm.chargeabilityTypeId" :value="3" />
            Automatico
          </label>
          <span class="charge-divider"></span>
          <label class="checkbox-label">
            <input type="checkbox" v-model="expForm.send770" />
            Includi nel modello 770
          </label>
        </div>
      </div>
      <div class="form-group">
        <label class="form-label">Note</label>
        <textarea class="form-textarea" v-model="expForm.description" rows="2"></textarea>
      </div>
      </template><!-- fine v-if !noFiscalYearAvailable -->

      <!-- ── Documenti allegati ─────────────────────────────────── -->
      <div v-if="editingExp || savedExpenseId" class="doc-section">
        <div class="doc-section-header">
          <span>Documenti allegati</span>
          <label class="btn btn-sm btn-ghost doc-upload-btn" :class="{ 'btn-disabled': uploadingDoc }">
            <span v-if="uploadingDoc" class="spinner" style="width:12px;height:12px;margin-right:4px"></span>
            <span v-else>+ Allega file</span>
            <input type="file" multiple style="display:none" @change="attachFiles" :disabled="uploadingDoc" />
          </label>
        </div>
        <div v-if="loadingDocs" class="doc-loading"><div class="spinner" style="width:16px;height:16px"></div></div>
        <div v-else-if="!attachedDocs.length" class="doc-empty">Nessun documento allegato</div>
        <div v-else class="doc-list">
          <div v-for="doc in attachedDocs" :key="doc.id" class="doc-item">
            <span class="doc-icon">{{ fileIcon(doc.mimeType) }}</span>
            <span class="doc-name" :title="doc.fileName">{{ doc.fileName }}</span>
            <span class="doc-size text-muted">{{ fmtSize(doc.fileSize) }}</span>
            <button class="btn-icon doc-dl" title="Scarica" @click="downloadDoc(doc)">↓</button>
            <button v-if="canDelete" class="btn-icon doc-del" title="Elimina" @click="deleteDoc(doc.id)">✕</button>
          </div>
        </div>
      </div>

      <template #footer>
        <button class="btn btn-ghost" @click="showExpenseModal = false">Annulla</button>
        <button class="btn btn-primary" @click="saveExpense"
                :disabled="savingExp || (noFiscalYearAvailable && !editingExp)">
          <span v-if="savingExp" class="spinner" style="width:14px;height:14px"></span>
          {{ editingExp ? 'Salva' : 'Crea' }}
        </button>
      </template>
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAppStore } from '@/stores/app'
import { expenseApi, chartOfAccountsApi, supplierApi, millesimalTableApi, documentApi, unitApi } from '@/services/api'
import { usePermissions } from '@/composables/usePermissions'
import BaseModal from '@/components/BaseModal.vue'
import AppPaginator from '@/components/AppPaginator.vue'

const EXPENSE_ENTITY_FULL_NAME = 'DomuWave.Services.Models.Expense'

const store  = useAppStore()
const route  = useRoute()
const router = useRouter()
const { canCreate, canEdit, canDelete } = usePermissions()

// ─── Fiscal Years ─────────────────────────────────────────────
const fiscalYears          = computed(() => store.fiscalYears)
const selectedFiscalYearId = ref(store.selectedFiscalYearId ?? null)

// Esercizi selezionabili nella form: solo Open (2) e Closing (3) — la Bozza non è utilizzabile
const FY_OPEN    = 2
const FY_CLOSING = 3
const selectableFiscalYears = computed(() =>
  fiscalYears.value.filter(f => f.statusId === FY_OPEN || f.statusId === FY_CLOSING)
)
const noFiscalYearAvailable = computed(() => selectableFiscalYears.value.length === 0)

// ─── Filtri e paginazione ─────────────────────────────────────
// Inizializzati dai query param per supportare link condivisibili
function qInt(key, fallback = null) {
  const v = route.query[key]
  return v != null && v !== '' ? parseInt(v, 10) : fallback
}
function qStr(key, fallback = '') {
  return route.query[key] ?? fallback
}

const pageSize            = ref(qInt('pageSize', 20))
const currentPage         = ref(qInt('page',     1))
const totalCount          = ref(0)
const search              = ref(qStr('search'))
const expTypeFilter       = ref(qInt('type'))
const paymentStatusFilter = ref(qInt('status'))
const sortField           = ref(qStr('sortField', 'documentdate'))
const sortAsc             = ref(route.query.sortAsc === 'true')

// Sovrascrive selectedFiscalYearId se presente in query
if (qInt('fy')) selectedFiscalYearId.value = qInt('fy')

// Mantiene la URL allineata allo stato — senza aggiungere entry nello history
function syncUrl() {
  const q = {}
  if (currentPage.value  > 1)                 q.page      = currentPage.value
  if (pageSize.value     !== 20)               q.pageSize  = pageSize.value
  if (search.value)                            q.search    = search.value
  if (expTypeFilter.value)                     q.type      = expTypeFilter.value
  if (paymentStatusFilter.value)               q.status    = paymentStatusFilter.value
  if (selectedFiscalYearId.value)              q.fy        = selectedFiscalYearId.value
  if (sortField.value !== 'documentdate')      q.sortField = sortField.value
  if (sortAsc.value)                           q.sortAsc   = 'true'
  router.replace({ query: q })
}

function setSort(field) {
  if (sortField.value === field) {
    sortAsc.value = !sortAsc.value
  } else {
    sortField.value = field
    sortAsc.value   = false
  }
  currentPage.value = 1
  loadExpenses()
}

function sortIcon(field) {
  if (sortField.value !== field) return '↕'
  return sortAsc.value ? '↑' : '↓'
}

let searchTimer = null
function onSearchInput() {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(resetAndLoad, 300)
}

function resetAndLoad() {
  currentPage.value = 1
  loadExpenses()
}

// ─── Metodi di pagamento ──────────────────────────────────────
const paymentMethods = ref([])

// ─── Spese ────────────────────────────────────────────────────
const expenses         = ref([])
const loadingExp       = ref(false)
const showExpenseModal = ref(false)
const editingExp       = ref(null)
const savingExp        = ref(false)
const expErrors        = ref({})
const actionInProgress = ref({})

// ─── Documenti allegati ───────────────────────────────────────
const attachedDocs   = ref([])
const loadingDocs    = ref(false)
const uploadingDoc   = ref(false)
const savedExpenseId = ref(null)   // id della spesa appena creata (per allegare subito)

async function loadDocs(expenseId) {
  if (!expenseId) return
  loadingDocs.value = true
  try {
    const { data } = await documentApi.getByEntity(expenseId, EXPENSE_ENTITY_FULL_NAME)
    attachedDocs.value = data ?? []
  } catch { attachedDocs.value = [] } finally { loadingDocs.value = false }
}

async function attachFiles(event) {
  const files = Array.from(event.target.files)
  event.target.value = ''
  if (!files.length) return
  const expenseId = editingExp.value ?? savedExpenseId.value
  if (!expenseId) return
  uploadingDoc.value = true
  try {
    for (const file of files) {
      const fileContent = await toBase64(file)
      await documentApi.upload({
        condominiumId:    store.selectedCondominioId,
        entityId:         expenseId,
        entityFullName:   EXPENSE_ENTITY_FULL_NAME,
        title:            file.name,
        fileName:         file.name,
        mimeType:         file.type || 'application/octet-stream',
        fileSize:         file.size,
        isVisibleToOwners: false,
        fileContent,
      })
    }
    await loadDocs(expenseId)
    store.toast(files.length === 1 ? 'Documento allegato' : `${files.length} documenti allegati`, 'success')
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { uploadingDoc.value = false }
}

async function downloadDoc(doc) {
  try {
    const response = await documentApi.download(doc.id)
    const url = URL.createObjectURL(response.data)
    const a   = Object.assign(document.createElement('a'), { href: url, download: doc.fileName })
    a.click()
    URL.revokeObjectURL(url)
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

async function deleteDoc(id) {
  if (!confirm('Eliminare questo documento?')) return
  try {
    await documentApi.delete(id)
    const expenseId = editingExp.value ?? savedExpenseId.value
    await loadDocs(expenseId)
    store.toast('Documento eliminato', 'success')
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

function toBase64(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onload  = () => resolve(reader.result.split(',')[1])
    reader.onerror = reject
    reader.readAsDataURL(file)
  })
}

function fileIcon(mimeType) {
  const m = (mimeType ?? '').toLowerCase()
  if (m.includes('pdf'))   return '📄'
  if (m.includes('image')) return '🖼'
  if (m.includes('word') || m.includes('msword')) return '📝'
  if (m.includes('excel') || m.includes('spreadsheet')) return '📊'
  if (m.includes('zip') || m.includes('compressed')) return '🗜'
  return '📎'
}
function fmtSize(bytes) {
  if (!bytes) return ''
  if (bytes < 1024) return bytes + ' B'
  if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB'
  return (bytes / (1024 * 1024)).toFixed(1) + ' MB'
}


function clearExpError(field) { delete expErrors.value[field] }

// Cambio modalità imputazione: azzera il campo non più usato (esclusività)
function onAllocationModeChange() {
  if (expForm.value.allocationMode === 'unit') expForm.value.millesimalTableId = null
  else                                          expForm.value.unitId = null
  clearExpError('allocation')
}

function validateExpForm() {
  const e = {}
  const f = expForm.value
  if (!f.fiscalYearId)          e.fiscalYearId     = 'Selezionare un esercizio fiscale'
  if (!f.name?.trim())          e.name             = 'Campo obbligatorio'
  if (!f.expenseTypeId)         e.expenseTypeId    = 'Selezionare un tipo spesa'
  const hasTaxable    = f.taxableAmount          > 0
  const hasTaxExempt  = f.taxableAmountVatExempt > 0
  if (!hasTaxable && !hasTaxExempt)
    e.taxableBase = 'Inserire almeno un importo tra Imponibile e Imponibile esente IVA'
  if (hasTaxable && !(f.vatAmount > 0))
    e.vatAmount = "L'IVA è obbligatoria quando è presente l'imponibile"
  if (!f.accountId)             e.accountId        = 'Selezionare un conto'
  if (f.allocationMode === 'unit') {
    if (!f.unitId)              e.allocation = 'Selezionare un immobile'
  } else {
    if (!f.millesimalTableId)   e.allocation = 'Selezionare una tabella millesimale'
  }

  expErrors.value = e
  return Object.keys(e).length === 0
}

const today = new Date().toISOString().slice(0, 10)
const suppliers        = ref([])
const millesimalTables = ref([])
const units            = ref([])
const enabledMillesimalTables = computed(() => {
  const currentId = editingExp.value ? expForm.value.millesimalTableId : null
  return millesimalTables.value.filter(t => t.isEnabled || t.id === currentId)
})

const emptyExpForm = () => ({
  fiscalYearId: null,
  name: '', documentNumber: '', documentDate: today, registrationDate: today,
  taxableAmount: 0, taxableAmountVatExempt: 0,
  grossAmount: 0, vatAmount: 0, netAmount: 0,
  pensionFund: 0, withholdingTax: 0, stampDuty: 0,
  expenseTypeId: 0, paymentStatusId: 1,
  paymentMethodId: null, supplierId: null, accountId: null, millesimalTableId: null, description: '',
  chargeabilityTypeId: 1,
  send770: false,
  allocationMode: 'table', unitId: null,
})

function autoCalcGross() {
  const f = expForm.value
  const taxable    = Number(f.taxableAmount)          || 0
  const taxExempt  = Number(f.taxableAmountVatExempt) || 0
  const vat        = Number(f.vatAmount)              || 0
  const pension    = Number(f.pensionFund)            || 0
  const withholding= Number(f.withholdingTax)         || 0
  const stamp      = Number(f.stampDuty)              || 0
  // Lordo = imponibile + esente + IVA + cassa + bollo - ritenuta
  f.grossAmount = Math.round((taxable + taxExempt + vat + pension + stamp - withholding) * 100) / 100
}
const expForm = ref(emptyExpForm())

// ChartOfAccounts
const chartOfAccounts      = ref([])
const expenseAccountGroups = ref([])
let accountsLoaded = false

async function loadExpenses() {
  if (!store.selectedCondominioId) return
  syncUrl()
  loadingExp.value = true
  try {
    const fy = fiscalYears.value.find(f => f.id === selectedFiscalYearId.value)
    const { data } = await expenseApi.getByCondominium(store.selectedCondominioId, {
      page:            currentPage.value,
      pageSize:        pageSize.value,
      sortField:       sortField.value,
      sortAsc:         sortAsc.value,
      search:          search.value || undefined,
      expenseTypeId:   expTypeFilter.value  || undefined,
      paymentStatusId: paymentStatusFilter.value || undefined,
      dateFrom:        fy?.startDate ? fy.startDate.slice(0, 10) : undefined,
      dateTo:          fy?.endDate   ? fy.endDate.slice(0, 10)   : undefined,
    })
    expenses.value   = data?.items      ?? []
    totalCount.value = data?.totalCount ?? 0
  } catch { expenses.value = []; totalCount.value = 0 } finally { loadingExp.value = false }
}

async function openExpenseModal(e = null) {
  expErrors.value  = {}
editingExp.value = e?.id ?? null
  savedExpenseId.value = null
  attachedDocs.value   = []

  // Auto-seleziona l'esercizio attivo per le nuove spese
  const defaultFyId = selectableFiscalYears.value.find(f => f.isActive)?.id
    ?? selectableFiscalYears.value[0]?.id
    ?? null

  expForm.value = e ? {
    fiscalYearId:       e.fiscalYearId ?? defaultFyId,
    name:               e.name ?? '',
    documentNumber:     e.documentNumber ?? '',
    documentDate:       e.documentDate?.slice(0, 10) ?? '',
    registrationDate:   e.registrationDate?.slice(0, 10) ?? '',
    taxableAmount:          e.taxableAmount          ?? 0,
    taxableAmountVatExempt: e.taxableAmountVatExempt ?? 0,
    grossAmount:            e.grossAmount            ?? 0,
    vatAmount:              e.vatAmount              ?? 0,
    netAmount:              e.netAmount              ?? 0,
    pensionFund:            e.pensionFund            ?? 0,
    withholdingTax:         e.withholdingTax         ?? 0,
    stampDuty:              e.stampDuty              ?? 0,
    expenseTypeId:          e.expenseTypeId          ?? 0,
    paymentStatusId:    e.paymentStatusId ?? 1,
    paymentMethodId:    e.paymentMethodId ?? null,
    supplierId:         e.supplierId ?? null,
    accountId:          e.accountId ?? null,
    millesimalTableId:  e.millesimalTableId ?? null,
    unitId:             e.unitId ?? null,
    allocationMode:     e.unitId ? 'unit' : 'table',
    description:        e.description ?? '',
    chargeabilityTypeId: e.chargeabilityTypeId ?? 1,
    send770:            e.send770 ?? false,
  } : { ...emptyExpForm(), fiscalYearId: defaultFyId }

  if (!store.fiscalYears.length) await store.loadFiscalYears()

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
      const [supRes, mtRes, unitRes] = await Promise.all([
        supplierApi.getAll(),
        millesimalTableApi.getByCondominium(store.selectedCondominioId),
        unitApi.getByCondominium(store.selectedCondominioId),
      ])
      suppliers.value = supRes.data ?? []
      millesimalTables.value = mtRes.data ?? []
      units.value = unitRes.data ?? []
    } catch {}
  }
  if (e) await loadDocs(e.id)

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
      documentDate:       expForm.value.documentDate || null,
      registrationDate:   expForm.value.registrationDate || null,
      taxableAmount:          expForm.value.taxableAmount          || 0,
      taxableAmountVatExempt: expForm.value.taxableAmountVatExempt || 0,
      vatAmount:              expForm.value.vatAmount              || 0,
      pensionFund:            expForm.value.pensionFund            || 0,
      withholdingTax:         expForm.value.withholdingTax         || 0,
      stampDuty:              expForm.value.stampDuty              || 0,
      expenseTypeId:      expForm.value.expenseTypeId,
      paymentStatusId:    expForm.value.paymentStatusId || 1,
      paymentMethodId:    expForm.value.paymentMethodId || null,
      description:        expForm.value.description || null,
      condominiumId:      store.selectedCondominioId,
      fiscalYearId:       expForm.value.fiscalYearId ?? null,
      accountId:          expForm.value.accountId         ?? null,
      millesimalTableId:  expForm.value.allocationMode === 'unit' ? null : (expForm.value.millesimalTableId ?? null),
      unitId:             expForm.value.allocationMode === 'unit' ? (expForm.value.unitId ?? null) : null,
      supplierId:         expForm.value.supplierId        ?? null,
      chargeabilityTypeId: expForm.value.chargeabilityTypeId ?? 1,
      send770:            expForm.value.send770 ?? false,
    }
    if (editingExp.value) {
      await expenseApi.update(editingExp.value, payload)
      store.toast('Spesa aggiornata', 'success')
      showExpenseModal.value = false
      await loadExpenses()
    } else {
      const { data } = await expenseApi.create(payload)
      store.toast('Spesa creata', 'success')
      savedExpenseId.value = data?.id ?? null
      editingExp.value     = savedExpenseId.value
      await loadExpenses()
      if (savedExpenseId.value) await loadDocs(savedExpenseId.value)
    }
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { savingExp.value = false }
}

async function markPaid(id) {
  actionInProgress.value[`pay-${id}`] = true
  try { await expenseApi.markAsPaid(id, today, null); await loadExpenses() }
  catch {} finally { actionInProgress.value[`pay-${id}`] = false }
}

async function markUnpaid(id) {
  actionInProgress.value[`pay-${id}`] = true
  try { await expenseApi.markAsUnpaid(id); await loadExpenses() }
  catch {} finally { actionInProgress.value[`pay-${id}`] = false }
}

async function deleteExpense(id) {
  if (!confirm('Eliminare questa spesa?')) return
  actionInProgress.value[`del-${id}`] = true
  try { await expenseApi.delete(id); await loadExpenses() }
  catch {} finally { actionInProgress.value[`del-${id}`] = false }
}

// Ricalcola warning quando cambia la data di registrazione
// Auto-select DefaultMillesimalTable when account changes
watch(() => expForm.value.accountId, (accountId) => {
  if (!accountId) return
  if (expForm.value.allocationMode === 'unit') return   // in modalità immobile non tocca la tabella
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
  search.value              = ''
  expTypeFilter.value       = null
  paymentStatusFilter.value = null
  selectedFiscalYearId.value = null
  currentPage.value         = 1
  loadExpenses()
})

// I filtri che cambiano pagina resettano a 1 via resetAndLoad; currentPage chiama direttamente
watch(currentPage,         loadExpenses)
watch(pageSize,            () => { currentPage.value = 1; loadExpenses() })
watch(selectedFiscalYearId, () => { currentPage.value = 1; loadExpenses() })

onMounted(async () => {
  if (!store.fiscalYears.length) await store.loadFiscalYears()
  const { data } = await expenseApi.getPaymentMethods()
  paymentMethods.value = data ?? []
  await loadExpenses()
})
onUnmounted(() => window.removeEventListener('app:refresh', loadExpenses))
window.addEventListener('app:refresh', loadExpenses)
</script>

<style scoped>
.tab-toolbar { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1rem; flex-wrap: wrap; }

/* Riga "A carico di" + flag 770: tutto in linea, va a capo solo se manca spazio */
.charge-row {
  display: flex; align-items: center; flex-wrap: wrap;
  gap: 0.5rem 1.25rem;
}
.charge-row .radio-label,
.charge-row .checkbox-label {
  display: flex; align-items: center; gap: 0.4rem;
  font-size: 0.875rem; cursor: pointer; user-select: none;
  white-space: nowrap; margin: 0;
}
.charge-row input { cursor: pointer; }
.charge-divider {
  width: 1px; align-self: stretch; min-height: 20px;
  background: var(--border); margin: 0 0.25rem;
}

th.sortable {
  cursor: pointer;
  user-select: none;
  white-space: nowrap;
}
th.sortable:hover { color: var(--accent); }
.sort-icon {
  font-size: 0.75rem;
  color: var(--text-muted);
  margin-left: 0.2rem;
}
th.sortable:hover .sort-icon { color: var(--accent); }

.no-fiscal-year-banner {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  padding: 1.25rem;
  border-radius: 8px;
  border: 1px solid #f59e0b;
  background: color-mix(in srgb, #f59e0b 8%, transparent);
  margin-bottom: 0.5rem;
}
.no-fiscal-year-icon { font-size: 2rem; line-height: 1; flex-shrink: 0; }
.no-fiscal-year-desc { font-size: 0.875rem; color: var(--text-secondary); margin-top: 0.35rem; line-height: 1.5; }

.label-hint {
  font-size: 0.75rem;
  font-weight: 400;
  color: var(--text-muted);
  margin-left: 0.35rem;
}
.label-required {
  color: var(--accent-red);
  margin-left: 0.2rem;
  font-weight: 600;
}

.warning-banner {
  margin-bottom: 1rem;
  padding: .65rem 1rem;
  border-radius: 6px;
  border: 1px solid #f59e0b;
  background: color-mix(in srgb, #f59e0b 10%, transparent);
  color: #92400e;
  font-size: .85rem;
  line-height: 1.45;
}

/* ── Documenti allegati ── */
.doc-section {
  margin-top: 1.25rem;
  border-top: 1px solid var(--border);
  padding-top: 1rem;
}
.doc-section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-weight: 600;
  font-size: 0.875rem;
  margin-bottom: 0.6rem;
}
.doc-upload-btn { cursor: pointer; }
.doc-upload-btn.btn-disabled { opacity: 0.6; pointer-events: none; }
.doc-loading { display: flex; justify-content: center; padding: 0.75rem 0; }
.doc-empty { font-size: 0.85rem; color: var(--text-muted); padding: 0.5rem 0; }
.doc-list { display: flex; flex-direction: column; gap: 0.35rem; }
.doc-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.4rem 0.6rem;
  border-radius: 6px;
  background: var(--bg-base);
  border: 1px solid var(--border);
  font-size: 0.85rem;
}
.doc-icon { flex-shrink: 0; }
.doc-name { flex: 1; min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.doc-size { flex-shrink: 0; font-size: 0.78rem; color: var(--text-muted); }
.doc-dl, .doc-del { flex-shrink: 0; font-size: 0.85rem; }
.doc-del { color: var(--accent-red); }
</style>
