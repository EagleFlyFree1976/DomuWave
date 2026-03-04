<template>
  <div>
    <!-- ── Header ──────────────────────────────────────────────── -->
    <div class="page-header">
      <h1>Esercizi Fiscali</h1>
      <button v-if="canCreate" class="btn btn-primary" @click="openCreateModal">
        + Nuovo esercizio
      </button>
    </div>

    <!-- ── Lista ────────────────────────────────────────────────── -->
    <div class="card">
      <div v-if="loading" class="loading-state"><div class="spinner"></div></div>

      <div v-else-if="!store.selectedCondominioId" class="empty-state">
        <div class="empty-icon">◎</div>
        <div>Seleziona un condominio per vedere gli esercizi fiscali</div>
      </div>

      <div v-else-if="!fiscalYears.length" class="empty-state">
        <div class="empty-icon">◎</div>
        <div>Nessun esercizio fiscale. Crea il primo esercizio.</div>
      </div>

      <div v-else class="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Codice</th>
              <th>Descrizione</th>
              <th>Inizio</th>
              <th>Fine</th>
              <th>Stato</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <template v-for="fy in fiscalYears" :key="fy.id">
              <!-- Riga principale -->
              <tr :class="{ 'row-expanded': expandedId === fy.id }">
                <td class="mono">
                  <span v-if="fy.isActive" class="active-dot" title="Esercizio attivo"></span>
                  {{ fy.code }}
                </td>
                <td class="text-secondary">{{ fy.description || '—' }}</td>
                <td class="mono text-secondary">{{ fmtDate(fy.startDate) }}</td>
                <td class="mono text-secondary">{{ fmtDate(fy.endDate) }}</td>
                <td>
                  <span class="badge" :class="statusBadge(fy.status)">
                    {{ statusLabel(fy.status) }}
                  </span>
                </td>
                <td>
                  <div class="row-actions">
                    <!-- Dettaglio / Summary -->
                    <button class="btn btn-sm btn-ghost" @click="toggleDetail(fy)">
                      {{ expandedId === fy.id ? 'Chiudi' : 'Dettaglio' }}
                    </button>

                    <!-- Modifica (solo Open) -->
                    <button v-if="canEdit && fy.status === 'Open'"
                            class="btn-icon" title="Modifica"
                            @click="openEditModal(fy)">✎</button>

                    <!-- Transizioni di stato -->
                    <button v-if="canEdit && fy.status === 'Open'"
                            class="btn btn-sm btn-ghost btn-amber"
                            @click="openTransitionModal(fy, 'startClosing')">
                      Avvia chiusura
                    </button>
                    <button v-if="canEdit && fy.status === 'Closing'"
                            class="btn btn-sm btn-ghost btn-purple"
                            @click="openTransitionModal(fy, 'close')">
                      Chiudi
                    </button>
                    <button v-if="canEdit && fy.status === 'Closed'"
                            class="btn btn-sm btn-ghost btn-muted"
                            @click="openTransitionModal(fy, 'lock')">
                      Blocca
                    </button>
                  </div>
                </td>
              </tr>

              <!-- Riga espansa: riepilogo finanziario -->
              <tr v-if="expandedId === fy.id" class="detail-row">
                <td colspan="6">
                  <div class="detail-panel">
                    <div v-if="loadingSummary" class="detail-loading">
                      <div class="spinner"></div>
                    </div>
                    <template v-else-if="detail">
                      <div class="detail-grid">
                        <div class="detail-card">
                          <div class="detail-label">Spese totali</div>
                          <div class="detail-value text-red">{{ fmt(detail.summary?.totalExpenses) }}</div>
                        </div>
                        <div class="detail-card">
                          <div class="detail-label">Spese pagate</div>
                          <div class="detail-value">{{ fmt(detail.summary?.totalExpensesPaid) }}</div>
                        </div>
                        <div class="detail-card">
                          <div class="detail-label">Rate emesse</div>
                          <div class="detail-value">{{ fmt(detail.summary?.totalInstallmentsBilled) }}</div>
                        </div>
                        <div class="detail-card">
                          <div class="detail-label">Incassi ricevuti</div>
                          <div class="detail-value text-green">{{ fmt(detail.summary?.totalPaymentsReceived) }}</div>
                        </div>
                        <div class="detail-card detail-card--balance"
                             :class="(detail.summary?.balance ?? 0) >= 0 ? 'balance-pos' : 'balance-neg'">
                          <div class="detail-label">Saldo</div>
                          <div class="detail-value">{{ fmt(detail.summary?.balance) }}</div>
                        </div>
                      </div>
                      <div class="detail-meta" v-if="detail.closingNotes">
                        <span class="text-secondary">Note chiusura:</span> {{ detail.closingNotes }}
                      </div>
                      <div class="detail-meta">
                        <span class="text-secondary">Creato il:</span> {{ fmtDateTime(detail.creationDate) }}
                        <template v-if="detail.createdByFullName">
                          da <strong>{{ detail.createdByFullName }}</strong>
                        </template>
                      </div>
                    </template>
                  </div>
                </td>
              </tr>
            </template>
          </tbody>
        </table>
      </div>
    </div>

    <!-- ══════════════════════════════════════════════════
         MODAL — Crea / Modifica esercizio
    ══════════════════════════════════════════════════ -->
    <div class="modal-overlay" v-if="showModal" @click.self="showModal = false">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ editingId ? 'Modifica esercizio' : 'Nuovo esercizio fiscale' }}</h2>
          <button class="btn-icon" @click="showModal = false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-grid">
            <!-- Codice (readonly in edit) -->
            <div class="form-group" :class="{ 'has-error': errors.code }">
              <label class="form-label">Codice *</label>
              <input class="form-input" v-model="form.code"
                     :readonly="!!editingId"
                     placeholder="es. 2025, EF-2025-01"
                     @input="clearError('code')" />
              <span v-if="errors.code" class="field-error">{{ errors.code }}</span>
            </div>

            <!-- Descrizione -->
            <div class="form-group" :class="{ 'has-error': errors.description }">
              <label class="form-label">Descrizione</label>
              <input class="form-input" v-model="form.description"
                     placeholder="Descrizione opzionale"
                     @input="clearError('description')" />
              <span v-if="errors.description" class="field-error">{{ errors.description }}</span>
            </div>

            <!-- Data inizio (readonly in edit) -->
            <div class="form-group" :class="{ 'has-error': errors.startDate }">
              <label class="form-label">Data inizio *</label>
              <input class="form-input" type="date" v-model="form.startDate"
                     :readonly="!!editingId"
                     @change="clearError('startDate')" />
              <span v-if="errors.startDate" class="field-error">{{ errors.startDate }}</span>
            </div>

            <!-- Data fine -->
            <div class="form-group" :class="{ 'has-error': errors.endDate }">
              <label class="form-label">Data fine *</label>
              <input class="form-input" type="date" v-model="form.endDate"
                     @change="clearError('endDate')" />
              <span v-if="errors.endDate" class="field-error">{{ errors.endDate }}</span>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showModal = false">Annulla</button>
          <button class="btn btn-primary" @click="saveFiscalYear" :disabled="saving">
            <span v-if="saving" class="spinner" style="width:14px;height:14px"></span>
            {{ editingId ? 'Salva' : 'Crea' }}
          </button>
        </div>
      </div>
    </div>

    <!-- ══════════════════════════════════════════════════
         MODAL — Transizione di stato
    ══════════════════════════════════════════════════ -->
    <div class="modal-overlay" v-if="showTransitionModal" @click.self="showTransitionModal = false">
      <div class="modal modal-sm">
        <div class="modal-header">
          <h2>{{ transitionTitle }}</h2>
          <button class="btn-icon" @click="showTransitionModal = false">✕</button>
        </div>
        <div class="modal-body">
          <p class="transition-desc">{{ transitionDesc }}</p>
          <div class="form-group">
            <label class="form-label">Note amministratore</label>
            <textarea class="form-textarea" v-model="transitionNotes"
                      rows="3" placeholder="Note opzionali…"></textarea>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showTransitionModal = false">Annulla</button>
          <button class="btn btn-primary" :class="transitionBtnClass"
                  @click="executeTransition" :disabled="savingTransition">
            <span v-if="savingTransition" class="spinner" style="width:14px;height:14px"></span>
            Conferma
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useAppStore } from '@/stores/app'
import { fiscalYearApi } from '@/services/api'
import { usePermissions } from '@/composables/usePermissions'

const store = useAppStore()
const { canCreate, canEdit } = usePermissions()

// ── Lista ──────────────────────────────────────────────────────────────────
const fiscalYears    = ref([])
const loading        = ref(false)

async function loadFiscalYears() {
  if (!store.selectedCondominioId) { fiscalYears.value = []; return }
  loading.value = true
  try {
    const { data } = await fiscalYearApi.getByCondominium(store.selectedCondominioId)
    fiscalYears.value = data ?? []
  } catch { fiscalYears.value = [] } finally { loading.value = false }
}

// ── Dettaglio / Summary ───────────────────────────────────────────────────
const expandedId     = ref(null)
const detail         = ref(null)
const loadingSummary = ref(false)

async function toggleDetail(fy) {
  if (expandedId.value === fy.id) {
    expandedId.value = null
    detail.value = null
    return
  }
  expandedId.value = fy.id
  detail.value = null
  loadingSummary.value = true
  try {
    const { data } = await fiscalYearApi.getById(fy.id)
    detail.value = data
  } catch { detail.value = null } finally { loadingSummary.value = false }
}

// ── Crea / Modifica ───────────────────────────────────────────────────────
const showModal  = ref(false)
const editingId  = ref(null)
const saving     = ref(false)
const form       = ref({ code: '', description: '', startDate: '', endDate: '' })
const errors     = ref({})

function clearError(f) { delete errors.value[f] }

function validate() {
  errors.value = {}
  if (!form.value.code?.trim())      errors.value.code      = 'Codice obbligatorio'
  if (!form.value.startDate)         errors.value.startDate = 'Data inizio obbligatoria'
  if (!form.value.endDate)           errors.value.endDate   = 'Data fine obbligatoria'
  if (form.value.startDate && form.value.endDate && form.value.endDate <= form.value.startDate)
    errors.value.endDate = 'La data fine deve essere successiva alla data inizio'
  return Object.keys(errors.value).length === 0
}

function openCreateModal() {
  editingId.value = null
  form.value = { code: '', description: '', startDate: '', endDate: '' }
  errors.value = {}
  showModal.value = true
}

function openEditModal(fy) {
  editingId.value = fy.id
  form.value = {
    code:        fy.code,
    description: fy.description ?? '',
    startDate:   fy.startDate?.slice(0, 10) ?? '',
    endDate:     fy.endDate?.slice(0, 10) ?? '',
  }
  errors.value = {}
  showModal.value = true
}

async function saveFiscalYear() {
  if (!validate()) return
  saving.value = true
  try {
    if (editingId.value) {
      await fiscalYearApi.update(editingId.value, {
        description: form.value.description,
        endDate:     form.value.endDate || null,
      })
    } else {
      await fiscalYearApi.create({
        condominiumId: store.selectedCondominioId,
        code:          form.value.code.trim(),
        description:   form.value.description,
        startDate:     form.value.startDate,
        endDate:       form.value.endDate,
      })
    }
    showModal.value = false
    await loadFiscalYears()
  } catch { /* global toast */ } finally { saving.value = false }
}

// ── Transizioni di stato ──────────────────────────────────────────────────
const showTransitionModal = ref(false)
const transitionType      = ref('')
const transitionTarget    = ref(null)
const transitionNotes     = ref('')
const savingTransition    = ref(false)

const TRANSITION_META = {
  startClosing: {
    title: 'Avvia chiusura esercizio',
    desc:  "L'esercizio passerà in stato «In chiusura». Sarà ancora possibile registrare movimenti, ma non creare nuovi esercizi sovrapposti.",
    btn:   'btn-amber',
  },
  close: {
    title: 'Chiudi esercizio',
    desc:  "L'esercizio verrà definitivamente chiuso. Non sarà più possibile registrare nuovi movimenti in questo periodo.",
    btn:   'btn-purple',
  },
  lock: {
    title: 'Blocca esercizio',
    desc:  "L'esercizio verrà bloccato in modo irreversibile. Nessuna modifica sarà consentita.",
    btn:   '',
  },
}

const transitionTitle   = computed(() => TRANSITION_META[transitionType.value]?.title ?? '')
const transitionDesc    = computed(() => TRANSITION_META[transitionType.value]?.desc ?? '')
const transitionBtnClass = computed(() => TRANSITION_META[transitionType.value]?.btn ?? '')

function openTransitionModal(fy, type) {
  transitionTarget.value = fy
  transitionType.value   = type
  transitionNotes.value  = ''
  showTransitionModal.value = true
}

async function executeTransition() {
  if (!transitionTarget.value) return
  savingTransition.value = true
  try {
    const id    = transitionTarget.value.id
    const notes = transitionNotes.value || null
    if (transitionType.value === 'startClosing') await fiscalYearApi.startClosing(id, notes)
    else if (transitionType.value === 'close')   await fiscalYearApi.close(id, notes)
    else if (transitionType.value === 'lock')    await fiscalYearApi.lock(id, notes)
    showTransitionModal.value = false
    // Se era espanso, ricarica il dettaglio
    if (expandedId.value === id) {
      const { data } = await fiscalYearApi.getById(id)
      detail.value = data
    }
    await loadFiscalYears()
  } catch { /* global toast */ } finally { savingTransition.value = false }
}

// ── Formatters ─────────────────────────────────────────────────────────────
const fmtDate = (d) => d ? new Date(d).toLocaleDateString('it-IT') : '—'
const fmtDateTime = (d) => d ? new Date(d).toLocaleString('it-IT') : '—'
const fmt = (v) => v != null ? '€ ' + Number(v).toLocaleString('it-IT', { minimumFractionDigits: 2 }) : '—'

const statusBadge = (s) => ({
  Open:    'badge-green',
  Closing: 'badge-amber',
  Closed:  'badge-purple',
  Locked:  'badge-muted',
}[s] ?? 'badge-muted')

const statusLabel = (s) => ({
  Open:    'Aperto',
  Closing: 'In chiusura',
  Closed:  'Chiuso',
  Locked:  'Bloccato',
}[s] ?? s)

// ── Watchers / Init ────────────────────────────────────────────────────────
watch(() => store.selectedCondominioId, () => {
  expandedId.value = null
  detail.value = null
  loadFiscalYears()
})
onMounted(loadFiscalYears)
</script>

<style scoped>
.row-actions { display: flex; gap: 0.4rem; align-items: center; justify-content: flex-end; flex-wrap: wrap; }

/* Dot esercizio attivo */
.active-dot {
  display: inline-block;
  width: 7px; height: 7px;
  border-radius: 50%;
  background: var(--accent);
  margin-right: 5px;
  vertical-align: middle;
  animation: pulse 2s infinite;
}
@keyframes pulse {
  0%, 100% { opacity: 1; transform: scale(1); }
  50%       { opacity: 0.5; transform: scale(0.85); }
}

/* Riga espansa */
.row-expanded td { background: var(--bg-surface); }
.detail-row td   { padding: 0; border-bottom: 1px solid var(--border); }

/* Pannello dettaglio */
.detail-panel {
  padding: 1.25rem 1.5rem;
  background: var(--bg-surface);
}
.detail-loading { display: flex; justify-content: center; padding: 1rem; }

.detail-grid {
  display: flex; flex-wrap: wrap; gap: 1rem;
  margin-bottom: 0.75rem;
}
.detail-card {
  flex: 1 1 140px;
  background: var(--bg);
  border: 1px solid var(--border);
  border-radius: 8px;
  padding: 0.75rem 1rem;
}
.detail-card--balance { border-width: 2px; }
.balance-pos { border-color: var(--accent-green, #22c55e); }
.balance-neg { border-color: var(--accent-red, #ef4444); }

.detail-label { font-size: 0.72rem; text-transform: uppercase; letter-spacing: 0.8px; color: var(--text-muted); margin-bottom: 0.25rem; }
.detail-value { font-size: 1.1rem; font-weight: 600; font-family: monospace; }
.text-green   { color: var(--accent-green, #22c55e); }
.text-red     { color: var(--accent-red, #ef4444); }

.detail-meta { font-size: 0.8rem; color: var(--text-muted); margin-top: 0.4rem; }

/* Modal sm */
.modal-sm { max-width: 460px; }
.transition-desc { font-size: 0.875rem; color: var(--text-muted); margin-bottom: 1rem; line-height: 1.5; }

/* Bottoni stato */
.btn-amber  { color: #f59e0b !important; border-color: #f59e0b50 !important; }
.btn-amber:hover  { background: #f59e0b15 !important; }
.btn-purple { color: #a855f7 !important; border-color: #a855f750 !important; }
.btn-purple:hover { background: #a855f715 !important; }
</style>
