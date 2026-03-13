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
              <tr :class="{ 'row-expanded': expandedId === fy.id }">
                <td class="mono">
                  <span v-if="fy.isActive" class="active-dot" title="Esercizio attivo"></span>
                  {{ fy.code }}
                </td>
                <td class="text-secondary">{{ fy.description || '—' }}</td>
                <td class="mono text-secondary">{{ fmtDate(fy.startDate) }}</td>
                <td class="mono text-secondary">{{ fmtDate(fy.endDate) }}</td>
                <td>
                  <span class="badge" :class="statusBadge(fy.statusId)">
                    {{ statusLabel(fy.statusId) }}
                  </span>
                </td>
                <td>
                  <div class="row-actions">
                    <button class="btn btn-sm btn-ghost" @click="toggleDetail(fy)">
                      {{ expandedId === fy.id ? 'Chiudi' : 'Dettaglio' }}
                    </button>
                    <button v-if="canEdit && (fy.statusId === 1 || fy.statusId === 2)"
                            class="btn-icon" title="Modifica"
                            @click="openEditModal(fy)">✎</button>
                    <button v-if="canEdit && fy.statusId === 1"
                            class="btn btn-sm btn-ghost btn-green"
                            @click="openTransitionModal(fy, 'open')">
                      Apri
                    </button>
                    <button v-if="canEdit && fy.statusId === 2"
                            class="btn btn-sm btn-ghost btn-amber"
                            @click="openTransitionModal(fy, 'startClosing')">
                      Avvia chiusura
                    </button>
                    <button v-if="canEdit && fy.statusId === 3"
                            class="btn btn-sm btn-ghost btn-purple"
                            @click="openTransitionModal(fy, 'close')">
                      Chiudi
                    </button>
                    <button v-if="canEdit && fy.statusId === 4"
                            class="btn btn-sm btn-ghost btn-muted"
                            @click="openTransitionModal(fy, 'lock')">
                      Blocca
                    </button>
                  </div>
                </td>
              </tr>

              <!-- Riga espansa: riepilogo finanziario + saldi -->
              <tr v-if="expandedId === fy.id" class="detail-row">
                <td colspan="6">
                  <div class="detail-panel">
                    <div v-if="loadingSummary" class="detail-loading">
                      <div class="spinner"></div>
                    </div>
                    <template v-else-if="detail">
                      <!-- Riepilogo finanziario -->
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

                      <!-- Saldi per conto -->
                      <div class="balances-section">
                        <div class="balances-header">
                          <span class="balances-title">Saldi per conto</span>
                          <span v-if="loadingBalances" class="spinner" style="width:14px;height:14px"></span>
                        </div>

                        <div v-if="!loadingBalances && !balances.length" class="balances-empty">
                          Nessun saldo disponibile. I saldi vengono generati all'apertura dell'esercizio.
                        </div>

                        <div v-else-if="!loadingBalances" class="balances-table-wrap">
                          <table class="balances-table">
                            <thead>
                              <tr>
                                <th>Codice</th>
                                <th>Conto</th>
                                <th>Tipo</th>
                                <th class="col-num">Saldo Iniziale</th>
                                <th class="col-num">Movimenti</th>
                                <th class="col-num">Saldo Totale</th>
                                <th v-if="fy.statusId >= 4" class="col-num">Saldo Finale</th>
                                <th v-if="balances.some(b => b.isOpeningBalanceEditable)"></th>
                              </tr>
                            </thead>
                            <tbody>
                              <tr v-for="b in balances" :key="b.id">
                                <td class="mono text-secondary">{{ b.accountCode }}</td>
                                <td>{{ b.accountName }}</td>
                                <td>
                                  <span class="badge-type-sm" :class="typeClass(b.accountType)">
                                    {{ b.accountTypeLabel }}
                                  </span>
                                </td>
                                <td class="col-num">
                                  <!-- Editing inline saldo iniziale -->
                                  <template v-if="editingBalanceId === b.id">
                                    <div class="inline-edit">
                                      <input
                                        class="inline-input"
                                        type="number"
                                        step="0.01"
                                        v-model.number="balanceEditValue"
                                        @keyup.enter="saveBalanceOpening(b)"
                                        @keyup.escape="cancelBalanceEdit"
                                      />
                                      <button class="inline-btn ok" @click="saveBalanceOpening(b)" :disabled="savingBalance">✓</button>
                                      <button class="inline-btn cancel" @click="cancelBalanceEdit">✕</button>
                                    </div>
                                  </template>
                                  <template v-else>
                                    {{ fmt(b.openingBalance) }}
                                  </template>
                                </td>
                                <td class="col-num text-secondary">{{ fmt(b.movementsTotal) }}</td>
                                <td class="col-num" :class="b.totalBalance >= 0 ? 'text-green' : 'text-red'">
                                  {{ fmt(b.totalBalance) }}
                                </td>
                                <td v-if="fy.statusId >= 4" class="col-num"
                                    :class="b.closingBalance >= 0 ? 'text-green' : 'text-red'">
                                  {{ fmt(b.closingBalance) }}
                                </td>
                                <td v-if="b.isOpeningBalanceEditable" class="col-action">
                                  <button v-if="editingBalanceId !== b.id"
                                          class="btn-icon" title="Modifica saldo iniziale"
                                          @click="startBalanceEdit(b)">✎</button>
                                </td>
                                <td v-else-if="balances.some(x => x.isOpeningBalanceEditable)"></td>
                              </tr>
                            </tbody>
                          </table>
                        </div>
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
    <BaseModal :show="showModal"
               @close="showModal = false"
               :title="editingId ? 'Modifica esercizio' : 'Nuovo esercizio fiscale'"
               :subtitle="store.selectedCondominio?.name"
               size="lg">
      <div class="form-grid">
        <div class="form-group" :class="{ 'has-error': errors.code }">
          <label class="form-label">Codice *</label>
          <input class="form-input" v-model="form.code"
                 :readonly="!!editingId"
                 placeholder="es. 2025, EF-2025-01"
                 @input="clearError('code')" />
          <span v-if="errors.code" class="field-error">{{ errors.code }}</span>
        </div>

        <div class="form-group" :class="{ 'has-error': errors.description }">
          <label class="form-label">Descrizione</label>
          <input class="form-input" v-model="form.description"
                 placeholder="Descrizione opzionale"
                 @input="clearError('description')" />
          <span v-if="errors.description" class="field-error">{{ errors.description }}</span>
        </div>

        <div class="form-group" :class="{ 'has-error': errors.startDate }">
          <label class="form-label">Data inizio *</label>
          <input class="form-input" type="date" v-model="form.startDate"
                 :readonly="!!editingId"
                 @change="clearError('startDate')" />
          <span v-if="errors.startDate" class="field-error">{{ errors.startDate }}</span>
        </div>

        <div class="form-group" :class="{ 'has-error': errors.endDate }">
          <label class="form-label">Data fine *</label>
          <input class="form-input" type="date" v-model="form.endDate"
                 @change="clearError('endDate')" />
          <span v-if="errors.endDate" class="field-error">{{ errors.endDate }}</span>
        </div>
      </div>
      <template #footer>
        <button class="btn btn-ghost" @click="showModal = false">Annulla</button>
        <button class="btn btn-primary" @click="saveFiscalYear" :disabled="saving">
          <span v-if="saving" class="spinner" style="width:14px;height:14px"></span>
          {{ editingId ? 'Salva' : 'Crea' }}
        </button>
      </template>
    </BaseModal>

    <!-- ══════════════════════════════════════════════════
         MODAL — Transizione di stato
    ══════════════════════════════════════════════════ -->
    <BaseModal :show="showTransitionModal"
               @close="showTransitionModal = false"
               :title="transitionTitle"
               :subtitle="transitionTarget?.code"
               size="sm">
      <p class="transition-desc">{{ transitionDesc }}</p>
      <div v-if="transitionType !== 'open'" class="form-group">
        <label class="form-label">Note amministratore</label>
        <textarea class="form-textarea" v-model="transitionNotes"
                  rows="3" placeholder="Note opzionali…"></textarea>
      </div>
      <template #footer>
        <button class="btn btn-ghost" @click="showTransitionModal = false">Annulla</button>
        <button class="btn btn-primary" :class="transitionBtnClass"
                @click="executeTransition" :disabled="savingTransition">
          <span v-if="savingTransition" class="spinner" style="width:14px;height:14px"></span>
          Conferma
        </button>
      </template>
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useAppStore } from '@/stores/app'
import { fiscalYearApi, accountBalanceApi } from '@/services/api'
import { usePermissions } from '@/composables/usePermissions'
import BaseModal from '@/components/BaseModal.vue'

const store = useAppStore()
const { canCreate, canEdit } = usePermissions()

// ── Lista ──────────────────────────────────────────────────────────────────
const fiscalYears = ref([])
const loading     = ref(false)

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

// Saldi per conto
const balances         = ref([])
const loadingBalances  = ref(false)
const editingBalanceId = ref(null)
const balanceEditValue = ref(0)
const savingBalance    = ref(false)

async function toggleDetail(fy) {
  if (expandedId.value === fy.id) {
    expandedId.value  = null
    detail.value      = null
    balances.value    = []
    editingBalanceId.value = null
    return
  }
  expandedId.value  = fy.id
  detail.value      = null
  balances.value    = []
  loadingSummary.value  = true
  loadingBalances.value = true
  try {
    const [summaryRes, balancesRes] = await Promise.allSettled([
      fiscalYearApi.getById(fy.id),
      accountBalanceApi.getByFiscalYear(fy.id),
    ])
    if (summaryRes.status  === 'fulfilled') detail.value   = summaryRes.value.data
    if (balancesRes.status === 'fulfilled') balances.value = balancesRes.value.data ?? []
  } finally {
    loadingSummary.value  = false
    loadingBalances.value = false
  }
}

function startBalanceEdit(b) {
  editingBalanceId.value = b.id
  balanceEditValue.value = b.openingBalance
}

function cancelBalanceEdit() {
  editingBalanceId.value = null
}

async function saveBalanceOpening(b) {
  savingBalance.value = true
  try {
    const { data } = await accountBalanceApi.updateOpening(b.id, { openingBalance: balanceEditValue.value })
    const idx = balances.value.findIndex(x => x.id === b.id)
    if (idx !== -1) balances.value[idx] = data
    editingBalanceId.value = null
  } catch (err) {
    if (!err?.response) throw err
  } finally { savingBalance.value = false }
}

// ── Crea / Modifica ───────────────────────────────────────────────────────
const showModal = ref(false)
const editingId = ref(null)
const saving    = ref(false)
const form      = ref({ code: '', description: '', startDate: '', endDate: '' })
const errors    = ref({})

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
  form.value      = { code: '', description: '', startDate: '', endDate: '' }
  errors.value    = {}
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
  errors.value    = {}
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
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { saving.value = false }
}

// ── Transizioni di stato ──────────────────────────────────────────────────
const showTransitionModal = ref(false)
const transitionType      = ref('')
const transitionTarget    = ref(null)
const transitionNotes     = ref('')
const savingTransition    = ref(false)

const TRANSITION_META = {
  open: {
    title: 'Apri esercizio',
    desc:  "L'esercizio passerà in stato «Aperto». Sarà possibile registrare spese, rate e incassi. Non potranno esistere altri esercizi aperti per lo stesso condominio.",
    btn:   'btn-green',
  },
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

const transitionTitle    = computed(() => TRANSITION_META[transitionType.value]?.title ?? '')
const transitionDesc     = computed(() => TRANSITION_META[transitionType.value]?.desc ?? '')
const transitionBtnClass = computed(() => TRANSITION_META[transitionType.value]?.btn ?? '')

function openTransitionModal(fy, type) {
  transitionTarget.value    = fy
  transitionType.value      = type
  transitionNotes.value     = ''
  showTransitionModal.value = true
}

async function executeTransition() {
  if (!transitionTarget.value) return
  savingTransition.value = true
  try {
    const id    = transitionTarget.value.id
    const notes = transitionNotes.value || null
    if (transitionType.value === 'open')          await fiscalYearApi.open(id)
    else if (transitionType.value === 'startClosing') await fiscalYearApi.startClosing(id, notes)
    else if (transitionType.value === 'close')   await fiscalYearApi.close(id, notes)
    else if (transitionType.value === 'lock')    await fiscalYearApi.lock(id, notes)
    showTransitionModal.value = false
    if (expandedId.value === id) {
      const { data } = await fiscalYearApi.getById(id)
      detail.value = data
    }
    await loadFiscalYears()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { savingTransition.value = false }
}

// ── Formatters ─────────────────────────────────────────────────────────────
const fmtDate     = (d) => d ? new Date(d).toLocaleDateString('it-IT') : '—'
const fmtDateTime = (d) => d ? new Date(d).toLocaleString('it-IT') : '—'
const fmt         = (v) => v != null ? '€ ' + Number(v).toLocaleString('it-IT', { minimumFractionDigits: 2 }) : '—'

const typeClass = (type) => ({ 1: 'type-entrata', 2: 'type-uscita', 3: 'type-patrimoniale' }[type] ?? '')

const statusBadge = (id) => ({
  1: 'badge-draft',
  2: 'badge-green',
  3: 'badge-amber',
  4: 'badge-purple',
  5: 'badge-muted',
}[id] ?? 'badge-muted')

const statusLabel = (id) => ({
  1: 'Bozza',
  2: 'Aperto',
  3: 'In chiusura',
  4: 'Chiuso',
  5: 'Bloccato',
}[id] ?? '—')

// ── Watchers / Init ────────────────────────────────────────────────────────
watch(() => store.selectedCondominioId, () => {
  expandedId.value       = null
  detail.value           = null
  balances.value         = []
  editingBalanceId.value = null
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
.detail-panel   { padding: 1.25rem 1.5rem; background: var(--bg-surface); }
.detail-loading { display: flex; justify-content: center; padding: 1rem; }

.detail-grid { display: flex; flex-wrap: wrap; gap: 1rem; margin-bottom: 0.75rem; }
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
.detail-meta  { font-size: 0.8rem; color: var(--text-muted); margin-top: 0.4rem; }

/* Transizione modale */
.transition-desc { font-size: 0.875rem; color: var(--text-muted); margin-bottom: 1rem; line-height: 1.5; }

/* Badge bozza */
:global(.badge-draft) { background: #f3f4f6; color: #6b7280; border: 1px solid #d1d5db; }

/* Bottoni stato */
.btn-green        { color: #22c55e !important; border-color: #22c55e50 !important; }
.btn-green:hover  { background: #22c55e15 !important; }
.btn-amber        { color: #f59e0b !important; border-color: #f59e0b50 !important; }
.btn-amber:hover  { background: #f59e0b15 !important; }
.btn-purple       { color: #a855f7 !important; border-color: #a855f750 !important; }
.btn-purple:hover { background: #a855f715 !important; }

/* Form validation */
.has-error .form-input { border-color: var(--accent-red, #e53e3e); }
.field-error { font-size: 0.78rem; color: var(--accent-red, #e53e3e); margin-top: 0.2rem; display: block; }

/* ── Saldi per conto ── */
.balances-section     { margin-top: 1.25rem; }
.balances-header      { display: flex; align-items: center; gap: 0.5rem; margin-bottom: 0.6rem; }
.balances-title       { font-size: 0.78rem; font-weight: 600; text-transform: uppercase; letter-spacing: 0.7px; color: var(--text-muted); }
.balances-empty       { font-size: 0.85rem; color: var(--text-muted); padding: 0.5rem 0; }
.balances-table-wrap  { overflow-x: auto; border: 1px solid var(--border); border-radius: 6px; }
.balances-table       { width: 100%; border-collapse: collapse; font-size: 0.825rem; }
.balances-table th    { background: var(--bg); color: var(--text-muted); font-size: 0.72rem; font-weight: 600;
                        text-transform: uppercase; letter-spacing: 0.5px;
                        padding: 0.45rem 0.75rem; text-align: left; border-bottom: 1px solid var(--border); }
.balances-table td    { padding: 0.45rem 0.75rem; border-bottom: 1px solid var(--border); vertical-align: middle; }
.balances-table tbody tr:last-child td { border-bottom: none; }
.balances-table tbody tr:hover         { background: var(--bg); }
.col-num    { text-align: right; font-family: monospace; }
.col-action { width: 32px; text-align: center; }

.badge-type-sm { display: inline-block; padding: 0.1rem 0.4rem; border-radius: 3px; font-size: 0.68rem; font-weight: 600; }
.type-entrata      { background: rgba(52,211,153,.12); color: #34d399; }
.type-uscita       { background: rgba(248,113,113,.12); color: #f87171; }
.type-patrimoniale { background: rgba(99,102,241,.12);  color: #818cf8; }

/* Inline editing saldo iniziale */
.inline-edit  { display: flex; align-items: center; gap: 0.25rem; justify-content: flex-end; }
.inline-input { width: 110px; padding: 0.2rem 0.4rem; border: 1px solid var(--border); border-radius: 4px;
                background: var(--bg-surface); color: var(--text-primary); font-size: 0.82rem;
                font-family: monospace; text-align: right; }
.inline-input:focus { outline: none; border-color: #34d399; }
.inline-btn   { width: 22px; height: 22px; border: none; border-radius: 4px; cursor: pointer;
                font-size: 0.75rem; display: flex; align-items: center; justify-content: center; }
.inline-btn.ok     { background: #34d399; color: #0f172a; }
.inline-btn.cancel { background: var(--bg-surface); color: var(--text-muted); border: 1px solid var(--border); }
.inline-btn:disabled { opacity: 0.5; cursor: not-allowed; }
</style>
