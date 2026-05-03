<template>
  <div>
    <div class="page-header">
      <h1>Lavori Straordinari</h1>
      <button v-if="canCreate && store.selectedCondominioId" class="btn btn-primary" @click="openWorkModal()">+ Nuovo lavoro</button>
    </div>

    <div class="toolbar">
      <select class="form-select" v-model="statusFilter" style="width:180px">
        <option value="">Tutti gli stati</option>
        <option value="Draft">Bozza</option>
        <option value="Planned">Pianificato</option>
        <option value="InProgress">In corso</option>
        <option value="Completed">Completato</option>
        <option value="Cancelled">Annullato</option>
      </select>
    </div>

    <!-- Works list -->
    <div v-if="loading" class="loading-state"><div class="spinner"></div></div>
    <div v-else-if="!works.length" class="card empty-state">
      <div class="empty-icon">◈</div><div>Nessun lavoro straordinario trovato</div>
    </div>

    <div v-else class="works-list">
      <div v-for="work in works" :key="work.id" class="work-card card">
        <!-- Work header -->
        <div class="work-header" @click="toggleWork(work.id)">
          <div class="work-header-left">
            <span class="expand-icon">{{ expandedWork === work.id ? '▼' : '▶' }}</span>
            <div>
              <div class="work-title">{{ work.title }}</div>
              <div class="work-meta">
                <span class="badge" :class="statusBadge(work.status)">{{ statusLabel(work.status) }}</span>
                <span class="badge" :class="priorityBadge(work.priority)">{{ priorityLabel(work.priority) }}</span>
                <span v-if="work.category" class="text-muted" style="font-size:0.8rem">{{ work.category }}</span>
                <span class="text-muted" style="font-size:0.8rem">{{ fmtDate(work.requestedDate) }}</span>
                <span v-if="work.quoteCount > 0" class="text-muted" style="font-size:0.8rem">{{ work.quoteCount }} preventiv{{ work.quoteCount === 1 ? 'o' : 'i' }}</span>
              </div>
            </div>
          </div>
          <div class="work-header-right">
            <span v-if="work.approvedAmount" class="work-amount">{{ fmt(work.approvedAmount) }}</span>
            <div class="row-actions" @click.stop>
              <button v-if="canEdit" class="btn-icon" @click="openWorkModal(work)">✎</button>
              <button v-if="canDelete" class="btn-icon" @click="deleteWork(work.id)" style="color:var(--accent-red)">✕</button>
            </div>
          </div>
        </div>

        <!-- Expanded: quotes section -->
        <div v-if="expandedWork === work.id" class="work-detail">
          <div v-if="work.description" class="work-description">{{ work.description }}</div>

          <div class="quotes-header">
            <span class="quotes-title">Preventivi</span>
            <button v-if="canCreate" class="btn btn-sm btn-ghost" @click="openQuoteModal(work.id)">+ Aggiungi preventivo</button>
          </div>

          <div v-if="loadingQuotes[work.id]" class="loading-state small"><div class="spinner" style="width:20px;height:20px"></div></div>
          <div v-else-if="!(quotes[work.id]?.length)" class="empty-state small">Nessun preventivo</div>

          <div v-else class="quotes-list">
            <div v-for="q in quotes[work.id]" :key="q.id" class="quote-row" :class="{ 'quote-approved': q.isApproved }">
              <div class="quote-main">
                <div class="quote-info">
                  <span v-if="q.isApproved" class="approved-mark">✔ Approvato</span>
                  <span class="quote-supplier">{{ q.supplierName || '— Fornitore non specificato —' }}</span>
                  <span v-if="q.quoteNumber" class="mono text-muted" style="font-size:0.8rem">{{ q.quoteNumber }}</span>
                  <span class="mono text-muted" style="font-size:0.8rem">{{ fmtDate(q.quoteDate) }}</span>
                  <span v-if="q.validUntil" class="mono text-muted" style="font-size:0.8rem">scad. {{ fmtDate(q.validUntil) }}</span>
                </div>
                <div class="quote-right">
                  <span class="quote-amount">{{ fmt(q.amount) }}</span>
                  <span class="badge" :class="quoteBadge(q.status)">{{ quoteStatusLabel(q.status) }}</span>
                  <div class="row-actions">
                    <button v-if="canEdit && !q.isApproved && q.status === 'Pending'" class="btn-icon btn-approve" @click="approveQuote(work.id, q.id)" title="Approva">✔</button>
                    <button v-if="canEdit && q.isApproved" class="btn-icon" @click="rejectQuote(work.id, q.id)" title="Revoca approvazione" style="color:var(--accent-amber)">↺</button>
                    <button v-if="canEdit" class="btn-icon" @click="openQuoteModal(work.id, q)">✎</button>
                    <button v-if="canDelete" class="btn-icon" @click="deleteQuote(work.id, q.id)" style="color:var(--accent-red)">✕</button>
                  </div>
                </div>
              </div>

              <!-- Documents for this quote -->
              <div class="quote-docs">
                <div v-for="doc in q.documents" :key="doc.id" class="doc-chip">
                  <span class="doc-icon">{{ fileIcon(doc.mimeType) }}</span>
                  <span class="doc-name">{{ doc.title }}</span>
                  <span class="text-muted" style="font-size:0.72rem">{{ fmtSize(doc.fileSize) }}</span>
                  <button v-if="canDelete" class="btn-icon" @click="deleteDocument(work.id, q.id, doc.id)" style="font-size:0.75rem;color:var(--accent-red)">✕</button>
                </div>
                <button v-if="canCreate" class="btn-icon doc-add-btn" @click="openDocumentModal(work.id, q.id)" title="Aggiungi documento">+ doc</button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Work Modal -->
    <div class="modal-overlay" v-if="showWorkModal" @click.self="showWorkModal=false">
      <div class="modal modal-lg">
        <div class="modal-header">
          <h2>{{ editingWork ? 'Modifica' : 'Nuovo' }} lavoro straordinario</h2>
          <button class="btn-icon" @click="showWorkModal=false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-grid">
            <div class="form-group" style="grid-column:1/-1" :class="{ 'has-error': workErrors.title }">
              <label class="form-label">Titolo *</label>
              <input class="form-input" v-model="workForm.title" @input="workErrors.title = null" />
              <span v-if="workErrors.title" class="field-error">{{ workErrors.title }}</span>
            </div>
            <div class="form-group">
              <label class="form-label">Categoria</label>
              <select class="form-select" v-model="workForm.category">
                <option value="">—</option>
                <option value="Facciata">Facciata</option>
                <option value="Tetto">Tetto / Copertura</option>
                <option value="Impianti">Impianti</option>
                <option value="Ascensore">Ascensore</option>
                <option value="Fondamenta">Fondamenta / Struttura</option>
                <option value="Cortile">Cortile / Aree comuni</option>
                <option value="Altro">Altro</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Priorità</label>
              <select class="form-select" v-model="workForm.priority">
                <option value="Low">Bassa</option>
                <option value="Medium">Media</option>
                <option value="High">Alta</option>
                <option value="Critical">Critica</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Stato</label>
              <select class="form-select" v-model="workForm.status">
                <option value="Draft">Bozza</option>
                <option value="Planned">Pianificato</option>
                <option value="InProgress">In corso</option>
                <option value="Completed">Completato</option>
                <option value="Cancelled">Annullato</option>
              </select>
            </div>
            <div class="form-group" :class="{ 'has-error': workErrors.requestedDate }">
              <label class="form-label">Data richiesta *</label>
              <input class="form-input" type="date" v-model="workForm.requestedDate" @change="workErrors.requestedDate = null" />
              <span v-if="workErrors.requestedDate" class="field-error">{{ workErrors.requestedDate }}</span>
            </div>
            <div class="form-group">
              <label class="form-label">Data approvazione</label>
              <input class="form-input" type="date" v-model="workForm.approvedDate" />
            </div>
            <div class="form-group">
              <label class="form-label">Data inizio lavori</label>
              <input class="form-input" type="date" v-model="workForm.startDate" />
            </div>
            <div class="form-group">
              <label class="form-label">Data completamento</label>
              <input class="form-input" type="date" v-model="workForm.completedDate" />
            </div>
            <div class="form-group">
              <label class="form-label">Importo approvato (€)</label>
              <input class="form-input" type="number" step="0.01" v-model.number="workForm.approvedAmount" />
            </div>
            <div class="form-group">
              <label class="form-label">Costo effettivo (€)</label>
              <input class="form-input" type="number" step="0.01" v-model.number="workForm.actualCost" />
            </div>
            <div class="form-group" style="grid-column:1/-1">
              <label class="form-label">Descrizione</label>
              <textarea class="form-textarea" v-model="workForm.description" rows="3"></textarea>
            </div>
            <div class="form-group" style="grid-column:1/-1">
              <label class="form-label">Note</label>
              <textarea class="form-textarea" v-model="workForm.notes" rows="2"></textarea>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showWorkModal=false">Annulla</button>
          <button class="btn btn-primary" @click="saveWork" :disabled="savingWork">
            <span v-if="savingWork" class="spinner" style="width:14px;height:14px"></span>
            {{ editingWork ? 'Salva' : 'Crea' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Quote Modal -->
    <div class="modal-overlay" v-if="showQuoteModal" @click.self="showQuoteModal=false">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ editingQuote ? 'Modifica' : 'Nuovo' }} preventivo</h2>
          <button class="btn-icon" @click="showQuoteModal=false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-grid">
            <div class="form-group">
              <label class="form-label">N° preventivo</label>
              <input class="form-input" v-model="quoteForm.quoteNumber" />
            </div>
            <div class="form-group">
              <label class="form-label">Fornitore</label>
              <select class="form-select" v-model.number="quoteForm.supplierId">
                <option :value="null">— Nessuno —</option>
                <option v-for="s in suppliers" :key="s.id" :value="s.id">{{ s.companyName }}</option>
              </select>
            </div>
            <div class="form-group" :class="{ 'has-error': quoteErrors.quoteDate }">
              <label class="form-label">Data preventivo *</label>
              <input class="form-input" type="date" v-model="quoteForm.quoteDate" @change="quoteErrors.quoteDate = null" />
              <span v-if="quoteErrors.quoteDate" class="field-error">{{ quoteErrors.quoteDate }}</span>
            </div>
            <div class="form-group">
              <label class="form-label">Valido fino al</label>
              <input class="form-input" type="date" v-model="quoteForm.validUntil" />
            </div>
            <div class="form-group" :class="{ 'has-error': quoteErrors.amount }">
              <label class="form-label">Importo (€) *</label>
              <input class="form-input" type="number" step="0.01" v-model.number="quoteForm.amount" @input="quoteErrors.amount = null" />
              <span v-if="quoteErrors.amount" class="field-error">{{ quoteErrors.amount }}</span>
            </div>
            <div class="form-group">
              <label class="form-label">Stato</label>
              <select class="form-select" v-model="quoteForm.status">
                <option value="Pending">In valutazione</option>
                <option value="Approved">Approvato</option>
                <option value="Rejected">Respinto</option>
              </select>
            </div>
          </div>
          <div class="form-group">
            <label class="form-label">Note</label>
            <textarea class="form-textarea" v-model="quoteForm.notes" rows="2"></textarea>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showQuoteModal=false">Annulla</button>
          <button class="btn btn-primary" @click="saveQuote" :disabled="savingQuote">
            <span v-if="savingQuote" class="spinner" style="width:14px;height:14px"></span>
            {{ editingQuote ? 'Salva' : 'Aggiungi' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Document Modal -->
    <div class="modal-overlay" v-if="showDocumentModal" @click.self="showDocumentModal=false">
      <div class="modal">
        <div class="modal-header">
          <h2>Aggiungi documento</h2>
          <button class="btn-icon" @click="showDocumentModal=false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-group" :class="{ 'has-error': docErrors.title }">
            <label class="form-label">Titolo documento *</label>
            <input class="form-input" v-model="docForm.title" @input="docErrors.title = null" />
            <span v-if="docErrors.title" class="field-error">{{ docErrors.title }}</span>
          </div>
          <div class="form-grid">
            <div class="form-group" :class="{ 'has-error': docErrors.fileName }">
              <label class="form-label">Nome file *</label>
              <input class="form-input" v-model="docForm.fileName" @input="docErrors.fileName = null" placeholder="preventivo.pdf" />
              <span v-if="docErrors.fileName" class="field-error">{{ docErrors.fileName }}</span>
            </div>
            <div class="form-group" :class="{ 'has-error': docErrors.filePath }">
              <label class="form-label">Percorso / URL *</label>
              <input class="form-input" v-model="docForm.filePath" @input="docErrors.filePath = null" placeholder="/uploads/..." />
              <span v-if="docErrors.filePath" class="field-error">{{ docErrors.filePath }}</span>
            </div>
            <div class="form-group">
              <label class="form-label">Tipo documento</label>
              <select class="form-select" v-model="docForm.documentType">
                <option value="">—</option>
                <option value="Preventivo">Preventivo</option>
                <option value="Contratto">Contratto</option>
                <option value="Planimetria">Planimetria</option>
                <option value="Perizia">Perizia tecnica</option>
                <option value="Fattura">Fattura</option>
                <option value="Altro">Altro</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Tipo MIME</label>
              <input class="form-input" v-model="docForm.mimeType" placeholder="application/pdf" />
            </div>
          </div>
          <div class="form-group">
            <label class="form-label">Note</label>
            <textarea class="form-textarea" v-model="docForm.notes" rows="2"></textarea>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showDocumentModal=false">Annulla</button>
          <button class="btn btn-primary" @click="saveDocument" :disabled="savingDoc">
            <span v-if="savingDoc" class="spinner" style="width:14px;height:14px"></span>
            Aggiungi
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, onMounted, onUnmounted } from 'vue'
import { useAppStore } from '@/stores/app'
import { extraordinaryWorkApi, supplierApi } from '@/services/api'
import { usePermissions } from '@/composables/usePermissions'

const store = useAppStore()
const { canCreate, canEdit, canDelete } = usePermissions()

const works         = ref([])
const quotes        = ref({})      // { [workId]: WorkQuoteReadDto[] }
const loadingQuotes = ref({})
const suppliers     = ref([])
const loading       = ref(false)
const statusFilter  = ref('')
const expandedWork  = ref(null)

// Work form
const showWorkModal = ref(false)
const editingWork   = ref(null)
const savingWork    = ref(false)
const workErrors    = ref({})
const workForm      = ref(defaultWorkForm())

// Quote form
const showQuoteModal = ref(false)
const editingQuote   = ref(null)
const quoteWorkId    = ref(null)
const savingQuote    = ref(false)
const quoteErrors    = ref({})
const quoteForm      = ref(defaultQuoteForm())

// Document form
const showDocumentModal = ref(false)
const docWorkId         = ref(null)
const docQuoteId        = ref(null)
const savingDoc         = ref(false)
const docErrors         = ref({})
const docForm           = ref(defaultDocForm())

// ── Formatters ──────────────────────────────────────────────────
const fmt     = (v) => v != null ? '€ ' + Number(v).toLocaleString('it-IT', { minimumFractionDigits: 2 }) : '—'
const fmtDate = (d) => d ? new Date(d).toLocaleDateString('it-IT') : '—'
const fmtSize = (s) => !s ? '' : s < 1024 ? s + ' B' : s < 1048576 ? (s/1024).toFixed(1) + ' KB' : (s/1048576).toFixed(1) + ' MB'
function fileIcon(mime) {
  if (!mime) return '📄'
  if (mime.includes('pdf')) return '📕'
  if (mime.includes('image')) return '🖼'
  if (mime.includes('word') || mime.includes('document')) return '📝'
  if (mime.includes('sheet') || mime.includes('excel')) return '📊'
  return '📄'
}

const statusLabel   = (s) => ({ Draft: 'Bozza', Planned: 'Pianificato', InProgress: 'In corso', Completed: 'Completato', Cancelled: 'Annullato' }[s] || s || '—')
const statusBadge   = (s) => ({ Draft: 'badge-muted', Planned: 'badge-blue', InProgress: 'badge-amber', Completed: 'badge-green', Cancelled: 'badge-muted' }[s] || 'badge-muted')
const priorityLabel = (p) => ({ Low: 'Bassa', Medium: 'Media', High: 'Alta', Critical: 'Critica' }[p] || p || '—')
const priorityBadge = (p) => ({ Low: 'badge-muted', Medium: 'badge-blue', High: 'badge-amber', Critical: 'badge-red' }[p] || 'badge-muted')
const quoteBadge    = (s) => ({ Pending: 'badge-blue', Approved: 'badge-green', Rejected: 'badge-muted' }[s] || 'badge-muted')
const quoteStatusLabel = (s) => ({ Pending: 'In valutazione', Approved: 'Approvato', Rejected: 'Respinto' }[s] || s || '—')

// ── Default forms ───────────────────────────────────────────────
function defaultWorkForm() {
  return { title: '', description: '', category: '', priority: 'Medium', status: 'Draft',
    requestedDate: '', approvedDate: '', startDate: '', completedDate: '',
    approvedAmount: null, actualCost: null, notes: '' }
}
function defaultQuoteForm() {
  return { quoteNumber: '', supplierId: null, quoteDate: '', validUntil: '', amount: null, status: 'Pending', notes: '' }
}
function defaultDocForm() {
  return { title: '', fileName: '', filePath: '', fileSize: 0, mimeType: 'application/pdf', documentType: '', notes: '' }
}

// ── Sanitize ────────────────────────────────────────────────────
function sanitizeWork(f) {
  return { ...f,
    requestedDate:  f.requestedDate  || null,
    approvedDate:   f.approvedDate   || null,
    startDate:      f.startDate      || null,
    completedDate:  f.completedDate  || null,
    approvedAmount: f.approvedAmount !== '' ? f.approvedAmount : null,
    actualCost:     f.actualCost     !== '' ? f.actualCost     : null,
  }
}
function sanitizeQuote(f) {
  return { ...f,
    supplierId: f.supplierId || null,
    quoteDate:  f.quoteDate  || null,
    validUntil: f.validUntil || null,
    amount:     f.amount     !== '' ? f.amount : 0,
  }
}

// ── Load data ───────────────────────────────────────────────────
async function loadWorks() {
  if (!store.selectedCondominioId) return
  loading.value = true
  try {
    const res = statusFilter.value
      ? await extraordinaryWorkApi.getByStatus(store.selectedCondominioId, statusFilter.value)
      : await extraordinaryWorkApi.getByCondominium(store.selectedCondominioId)
    works.value = res.data
  } catch (err) {
    works.value = []
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { loading.value = false }
}

async function loadSuppliers() {
  try { const { data } = await supplierApi.getAll(); suppliers.value = data }
  catch { suppliers.value = [] }
}

async function loadQuotes(workId) {
  loadingQuotes.value = { ...loadingQuotes.value, [workId]: true }
  try {
    const { data } = await extraordinaryWorkApi.getQuotes(workId)
    quotes.value = { ...quotes.value, [workId]: data }
  } catch (err) {
    quotes.value = { ...quotes.value, [workId]: [] }
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    loadingQuotes.value = { ...loadingQuotes.value, [workId]: false }
  }
}

// ── Toggle expand ───────────────────────────────────────────────
function toggleWork(id) {
  if (expandedWork.value === id) {
    expandedWork.value = null
  } else {
    expandedWork.value = id
    if (!quotes.value[id]) loadQuotes(id)
  }
}

// ── Work CRUD ───────────────────────────────────────────────────
function openWorkModal(w = null) {
  editingWork.value = w?.id ?? null
  workErrors.value  = {}
  workForm.value    = w ? {
    title:          w.title,
    description:    w.description    || '',
    category:       w.category       || '',
    priority:       w.priority       || 'Medium',
    status:         w.status         || 'Draft',
    requestedDate:  w.requestedDate  ? w.requestedDate.substring(0, 10) : '',
    approvedDate:   w.approvedDate   ? w.approvedDate.substring(0, 10) : '',
    startDate:      w.startDate      ? w.startDate.substring(0, 10) : '',
    completedDate:  w.completedDate  ? w.completedDate.substring(0, 10) : '',
    approvedAmount: w.approvedAmount ?? null,
    actualCost:     w.actualCost     ?? null,
    notes:          w.notes          || '',
  } : defaultWorkForm()
  showWorkModal.value = true
}

async function saveWork() {
  workErrors.value = {}
  if (!workForm.value.title) { workErrors.value.title = 'Il titolo è obbligatorio'; return }
  if (!workForm.value.requestedDate) { workErrors.value.requestedDate = 'La data è obbligatoria'; return }
  savingWork.value = true
  try {
    const payload = sanitizeWork(workForm.value)
    if (editingWork.value) {
      await extraordinaryWorkApi.update(editingWork.value, payload)
    } else {
      await extraordinaryWorkApi.create({ ...payload, condominiumId: store.selectedCondominioId })
    }
    store.toast('Lavoro salvato', 'success')
    showWorkModal.value = false
    loadWorks()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { savingWork.value = false }
}

async function deleteWork(id) {
  if (!confirm('Eliminare il lavoro e tutti i preventivi associati?')) return
  try { await extraordinaryWorkApi.delete(id); store.toast('Lavoro eliminato', 'success'); loadWorks() }
  catch (err) { if (!err?.response) store.toast('Impossibile raggiungere il server', 'error') }
}

// ── Quote CRUD ──────────────────────────────────────────────────
function openQuoteModal(workId, q = null) {
  quoteWorkId.value  = workId
  editingQuote.value = q?.id ?? null
  quoteErrors.value  = {}
  quoteForm.value    = q ? {
    quoteNumber: q.quoteNumber || '',
    supplierId:  q.supplierId  || null,
    quoteDate:   q.quoteDate   ? q.quoteDate.substring(0, 10) : '',
    validUntil:  q.validUntil  ? q.validUntil.substring(0, 10) : '',
    amount:      q.amount      ?? null,
    status:      q.status      || 'Pending',
    notes:       q.notes       || '',
  } : defaultQuoteForm()
  showQuoteModal.value = true
}

async function saveQuote() {
  quoteErrors.value = {}
  if (!quoteForm.value.quoteDate) { quoteErrors.value.quoteDate = 'La data è obbligatoria'; return }
  if (!quoteForm.value.amount && quoteForm.value.amount !== 0) { quoteErrors.value.amount = "L'importo è obbligatorio"; return }
  savingQuote.value = true
  try {
    const payload = sanitizeQuote(quoteForm.value)
    if (editingQuote.value) {
      await extraordinaryWorkApi.updateQuote(quoteWorkId.value, editingQuote.value, payload)
    } else {
      await extraordinaryWorkApi.createQuote(quoteWorkId.value, payload)
    }
    store.toast('Preventivo salvato', 'success')
    showQuoteModal.value = false
    loadQuotes(quoteWorkId.value)
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { savingQuote.value = false }
}

async function deleteQuote(workId, quoteId) {
  if (!confirm('Eliminare questo preventivo?')) return
  try { await extraordinaryWorkApi.deleteQuote(workId, quoteId); store.toast('Preventivo eliminato', 'success'); loadQuotes(workId) }
  catch (err) { if (!err?.response) store.toast('Impossibile raggiungere il server', 'error') }
}

async function approveQuote(workId, quoteId) {
  try { await extraordinaryWorkApi.approveQuote(workId, quoteId); store.toast('Preventivo approvato', 'success'); loadQuotes(workId) }
  catch (err) { if (!err?.response) store.toast('Impossibile raggiungere il server', 'error') }
}

async function rejectQuote(workId, quoteId) {
  try { await extraordinaryWorkApi.rejectQuote(workId, quoteId); store.toast('Approvazione revocata', 'success'); loadQuotes(workId) }
  catch (err) { if (!err?.response) store.toast('Impossibile raggiungere il server', 'error') }
}

// ── Document CRUD ───────────────────────────────────────────────
function openDocumentModal(workId, quoteId) {
  docWorkId.value  = workId
  docQuoteId.value = quoteId
  docErrors.value  = {}
  docForm.value    = defaultDocForm()
  showDocumentModal.value = true
}

async function saveDocument() {
  docErrors.value = {}
  if (!docForm.value.title)    { docErrors.value.title    = 'Il titolo è obbligatorio'; return }
  if (!docForm.value.fileName) { docErrors.value.fileName = 'Il nome file è obbligatorio'; return }
  if (!docForm.value.filePath) { docErrors.value.filePath = 'Il percorso è obbligatorio'; return }
  savingDoc.value = true
  try {
    await extraordinaryWorkApi.addDocument(docWorkId.value, docQuoteId.value, docForm.value)
    store.toast('Documento aggiunto', 'success')
    showDocumentModal.value = false
    loadQuotes(docWorkId.value)
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { savingDoc.value = false }
}

async function deleteDocument(workId, quoteId, docId) {
  if (!confirm('Eliminare questo documento?')) return
  try { await extraordinaryWorkApi.deleteDocument(workId, quoteId, docId); store.toast('Documento eliminato', 'success'); loadQuotes(workId) }
  catch (err) { if (!err?.response) store.toast('Impossibile raggiungere il server', 'error') }
}

watch(() => store.selectedCondominioId, loadWorks)
watch(statusFilter, loadWorks)
function loadAll() { loadWorks(); loadSuppliers() }
onMounted(loadAll)
onUnmounted(() => window.removeEventListener('app:refresh', loadAll))
window.addEventListener('app:refresh', loadAll)
</script>

<style scoped>
.toolbar { display: flex; gap: 0.75rem; margin-bottom: 1rem; flex-wrap: wrap; align-items: center; }
.works-list { display: flex; flex-direction: column; gap: 0.75rem; }
.work-card { padding: 0; overflow: hidden; }

.work-header {
  display: flex; align-items: flex-start; justify-content: space-between;
  padding: 0.9rem 1rem; cursor: pointer; gap: 1rem;
  transition: background 0.1s;
}
.work-header:hover { background: var(--bg-hover); }
.work-header-left { display: flex; align-items: flex-start; gap: 0.75rem; flex: 1; min-width: 0; }
.work-header-right { display: flex; align-items: center; gap: 0.75rem; flex-shrink: 0; }
.expand-icon { font-size: 0.65rem; color: var(--text-muted); padding-top: 0.25rem; flex-shrink: 0; }
.work-title { font-weight: 600; margin-bottom: 0.3rem; }
.work-meta { display: flex; align-items: center; gap: 0.4rem; flex-wrap: wrap; }
.work-amount { font-weight: 600; font-size: 1rem; color: var(--accent); }

.work-detail {
  border-top: 1px solid var(--border);
  padding: 0.75rem 1rem 1rem 2.5rem;
}
.work-description { color: var(--text-secondary); font-size: 0.875rem; margin-bottom: 0.75rem; }

.quotes-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 0.5rem; }
.quotes-title { font-weight: 600; font-size: 0.875rem; color: var(--text-secondary); text-transform: uppercase; letter-spacing: 0.05em; }
.btn-sm { font-size: 0.8rem; padding: 0.2rem 0.6rem; }

.quotes-list { display: flex; flex-direction: column; gap: 0.5rem; }
.quote-row {
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  padding: 0.6rem 0.75rem;
  background: var(--bg-surface);
}
.quote-row.quote-approved { border-color: var(--accent-green, #4ade80); }
.quote-main { display: flex; align-items: center; justify-content: space-between; gap: 1rem; }
.quote-info { display: flex; align-items: center; gap: 0.5rem; flex-wrap: wrap; flex: 1; min-width: 0; }
.quote-right { display: flex; align-items: center; gap: 0.5rem; flex-shrink: 0; }
.quote-supplier { font-weight: 500; font-size: 0.9rem; }
.quote-amount { font-weight: 600; font-size: 0.95rem; }
.approved-mark { color: var(--accent-green, #4ade80); font-size: 0.8rem; font-weight: 600; }
.btn-approve { color: var(--accent-green, #4ade80) !important; }

.quote-docs { display: flex; gap: 0.4rem; flex-wrap: wrap; margin-top: 0.4rem; padding-top: 0.4rem; border-top: 1px dashed var(--border); }
.doc-chip {
  display: flex; align-items: center; gap: 0.3rem;
  padding: 0.15rem 0.5rem; border-radius: 99px;
  background: rgba(255,255,255,0.05); font-size: 0.78rem;
}
.doc-icon { font-size: 0.9rem; }
.doc-name { max-width: 120px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.doc-add-btn { font-size: 0.75rem; color: var(--text-muted); border: 1px dashed var(--border); border-radius: 99px; padding: 0.1rem 0.5rem; }

.loading-state.small { padding: 0.5rem; justify-content: flex-start; }
.empty-state.small { padding: 0.5rem; font-size: 0.85rem; color: var(--text-muted); }
.modal-lg { max-width: 720px; width: 100%; }
.row-actions { display: flex; gap: 0.3rem; }
</style>
