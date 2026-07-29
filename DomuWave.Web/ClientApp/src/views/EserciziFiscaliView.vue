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
                    <button class="btn-icon btn-icon--expand"
                            :class="{ expanded: expandedId === fy.id }"
                            :title="expandedId === fy.id ? 'Comprimi' : 'Espandi dettaglio'"
                            @click="toggleDetail(fy)">
                      <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
                        <path d="M4 6l4 4 4-4" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
                      </svg>
                    </button>
                    <button v-if="canEdit && (fy.statusId === 1 || fy.statusId === 2)"
                            class="btn-icon" title="Modifica"
                            @click="openEditModal(fy)">✎</button>
                    <button v-if="canDelete && fy.statusId === 1"
                            class="btn-icon btn-icon--danger" title="Elimina"
                            @click="openDeleteModal(fy)">🗑</button>
                    <button v-if="canEdit && fy.statusId === 1"
                            class="btn btn-sm btn-ghost"
                            title="Imposta saldi iniziali unità"
                            @click="openOpeningBalancesModal(fy)">
                      Saldi iniziali
                    </button>
                    <button v-if="canEdit && fy.statusId === 1"
                            class="btn btn-sm btn-ghost btn-green"
                            @click="openTransitionModal(fy, 'open')">
                      Apri
                    </button>
                    <button v-if="canEdit && (fy.statusId === 1 || fy.statusId === 2) && fy.previousFiscalYearId"
                            class="btn btn-sm btn-ghost"
                            title="Ricalcola i saldi iniziali dalle chiusure dell'esercizio precedente"
                            :disabled="propagatingId === fy.id"
                            @click="propagateOpeningBalances(fy)">
                      <span v-if="propagatingId === fy.id" class="spinner" style="width:12px;height:12px"></span>
                      Propaga saldi
                    </button>
                    <button v-if="canEdit && fy.statusId === 2"
                            class="btn btn-sm btn-ghost btn-amber"
                            @click="openTransitionModal(fy, 'startClosing')">
                      Avvia chiusura
                    </button>
                    <button v-if="canEdit && fy.statusId === 2"
                            class="btn btn-sm btn-ghost btn-muted"
                            title="Riporta in Bozza per correggere i saldi iniziali"
                            @click="openTransitionModal(fy, 'revertToDraft')">
                      ↩ Bozza
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
                          <div class="detail-label">{{ fy.previousFiscalYearCode ? 'Saldo anno precedente' : 'Saldo iniziale' }}</div>
                          <div class="detail-value"
                               :class="(detail.summary?.totalOpeningBalance ?? 0) >= 0 ? 'text-green' : 'text-red'">
                            {{ fmtAccounting(detail.summary?.totalOpeningBalance, 'balance') }}
                          </div>
                        </div>
                        <div class="detail-card">
                          <div class="detail-label">Spese totali</div>
                          <div class="detail-value text-red">{{ fmtAccounting(detail.summary?.totalExpenses, 'expense') }}</div>
                        </div>
                        <div class="detail-card">
                          <div class="detail-label">Spese pagate</div>
                          <div class="detail-value text-red">{{ fmtAccounting(detail.summary?.totalExpensesPaid, 'expense') }}</div>
                        </div>
                        <div class="detail-card">
                          <div class="detail-label">Rate emesse</div>
                          <div class="detail-value">{{ fmtAccounting(detail.summary?.totalInstallmentsBilled, 'income') }}</div>
                        </div>
                        <div class="detail-card">
                          <div class="detail-label">Incassi ricevuti</div>
                          <div class="detail-value text-green">{{ fmtAccounting(detail.summary?.totalPaymentsReceived, 'income') }}</div>
                        </div>
                        <div class="detail-card detail-card--balance"
                             :class="(detail.summary?.balance ?? 0) >= 0 ? 'balance-pos' : 'balance-neg'">
                          <div class="detail-label">Saldo</div>
                          <div class="detail-value">{{ fmtAccounting(detail.summary?.balance, 'balance') }}</div>
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
                                <td class="col-num"
                                    :class="b.isOpeningBalanceEditable && editingBalanceId !== b.id ? 'cell-editable' : ''"
                                    @click="b.isOpeningBalanceEditable && editingBalanceId !== b.id ? startBalanceEdit(b) : null">
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
                                        @click.stop
                                        ref="inlineInputRef"
                                      />
                                      <button class="inline-btn ok" @click.stop="saveBalanceOpening(b)" :disabled="savingBalance">✓</button>
                                      <button class="inline-btn cancel" @click.stop="cancelBalanceEdit">✕</button>
                                    </div>
                                  </template>
                                  <template v-else>
                                    <span>{{ fmt(b.openingBalance) }}</span>
                                    <span v-if="b.isOpeningBalanceEditable" class="edit-hint">✎</span>
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
                              </tr>
                            </tbody>
                          </table>
                        </div>
                      </div>
                      <!-- Saldi iniziali per unità -->
                      <div class="balances-section">
                        <div class="balances-header">
                          <span class="balances-title">Saldi iniziali per unità</span>
                          <span v-if="loadingUnitOpeningBalances" class="spinner" style="width:14px;height:14px"></span>
                        </div>
                        <div v-if="!loadingUnitOpeningBalances && !unitOpeningBalances.length" class="balances-empty">
                          Nessun saldo disponibile.
                        </div>
                        <div v-else-if="!loadingUnitOpeningBalances" class="balances-table-wrap">
                          <table class="balances-table">
                            <thead>
                              <tr>
                                <th>Unità</th>
                                <th class="col-num">Saldo iniziale</th>
                                <th class="col-num">Rate addebitate</th>
                                <th class="col-num">Rate incassate</th>
                                <th class="col-num">Quota consuntiva</th>
                                <th class="col-num">Saldo conguaglio</th>
                                <th class="col-num">Saldo finale</th>
                              </tr>
                            </thead>
                            <tbody>
                              <tr v-for="ub in unitOpeningBalances" :key="ub.unitId">
                                <td>{{ ub.unitName }}</td>
                                <td class="col-num mono">{{ fmt(ub.openingBalance) }}</td>
                                <td class="col-num text-secondary">{{ fmt(ub.rateAddebitate) }}</td>
                                <td class="col-num text-secondary">{{ fmt(ub.rateIncassate) }}</td>
                                <td class="col-num text-secondary">{{ fmt(ub.quotaConsuntiva) }}</td>
                                <td class="col-num text-secondary">{{ fmt(ub.saldoConguaglio) }}</td>
                                <td class="col-num" :class="ub.closingBalance >= 0 ? 'text-green' : 'text-red'">
                                  {{ fmt(ub.closingBalance) }}
                                </td>
                              </tr>
                            </tbody>
                            <tfoot>
                              <tr class="balances-total-row">
                                <td>Totale</td>
                                <td class="col-num mono">{{ fmt(unitOpeningBalancesTotals.openingBalance) }}</td>
                                <td class="col-num mono">{{ fmt(unitOpeningBalancesTotals.rateAddebitate) }}</td>
                                <td class="col-num mono">{{ fmt(unitOpeningBalancesTotals.rateIncassate) }}</td>
                                <td class="col-num mono">{{ fmt(unitOpeningBalancesTotals.quotaConsuntiva) }}</td>
                                <td class="col-num mono">{{ fmt(unitOpeningBalancesTotals.saldoConguaglio) }}</td>
                                <td class="col-num mono" :class="unitOpeningBalancesTotals.closingBalance >= 0 ? 'text-green' : 'text-red'">
                                  {{ fmt(unitOpeningBalancesTotals.closingBalance) }}
                                </td>
                              </tr>
                            </tfoot>
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
               @close="closeCreateModal"
               :title="editingId ? 'Modifica esercizio' : (createStep === 2 ? 'Saldi iniziali unità' : 'Nuovo esercizio fiscale')"
               :subtitle="store.selectedCondominio?.name"
               size="lg">

      <!-- Step 1: dati esercizio -->
      <template v-if="!editingId && createStep === 1">
        <!-- Selector esercizio precedente (solo se esistono già esercizi) -->
        <div v-if="fiscalYears.length > 0" class="form-group form-group--full" :class="{ 'has-error': errors.previousFiscalYear }">
          <label class="form-label">Esercizio precedente *</label>
          <select class="form-select" v-model="selectedPreviousFiscalYearId" @change="onPreviousFiscalYearChange">
            <option :value="null" disabled>— Seleziona esercizio precedente —</option>
            <option v-for="fy in fiscalYears" :key="fy.id" :value="fy.id">
              {{ fy.code }}{{ fy.description ? ' – ' + fy.description : '' }}
              ({{ fmtDate(fy.startDate) }} / {{ fmtDate(fy.endDate) }})
            </option>
          </select>
          <span v-if="errors.previousFiscalYear" class="field-error">{{ errors.previousFiscalYear }}</span>
        </div>

        <div class="form-grid">
          <div class="form-group" :class="{ 'has-error': errors.code }">
            <label class="form-label">Codice *</label>
            <input class="form-input" v-model="form.code"
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
                   :readonly="fiscalYears.length > 0"
                   :class="{ 'input-readonly': fiscalYears.length > 0 }"
                   @change="clearError('startDate')" />
            <span v-if="errors.startDate" class="field-error">{{ errors.startDate }}</span>
            <span v-if="fiscalYears.length > 0 && form.startDate" class="field-hint">
              Calcolata automaticamente dal giorno successivo alla fine dell'esercizio precedente
            </span>
          </div>

          <div class="form-group" :class="{ 'has-error': errors.endDate }">
            <label class="form-label">Data fine *</label>
            <input class="form-input" type="date" v-model="form.endDate"
                   @change="clearError('endDate')" />
            <span v-if="errors.endDate" class="field-error">{{ errors.endDate }}</span>
          </div>
        </div>
      </template>

      <!-- Step 1 edit: form modifica -->
      <template v-if="editingId">
        <div class="form-grid">
          <div class="form-group" :class="{ 'has-error': errors.code }">
            <label class="form-label">Codice</label>
            <input class="form-input input-readonly" :value="form.code" readonly />
          </div>

          <div class="form-group" :class="{ 'has-error': errors.description }">
            <label class="form-label">Descrizione</label>
            <input class="form-input" v-model="form.description"
                   placeholder="Descrizione opzionale"
                   @input="clearError('description')" />
            <span v-if="errors.description" class="field-error">{{ errors.description }}</span>
          </div>

          <div class="form-group">
            <label class="form-label">Data inizio</label>
            <input class="form-input input-readonly" :value="form.startDate" readonly />
          </div>

          <div class="form-group" :class="{ 'has-error': errors.endDate }">
            <label class="form-label">Data fine *</label>
            <input class="form-input" type="date" v-model="form.endDate"
                   @change="clearError('endDate')" />
            <span v-if="errors.endDate" class="field-error">{{ errors.endDate }}</span>
          </div>
        </div>
      </template>

      <!-- Step 2: saldi iniziali unità (solo primo esercizio) -->
      <template v-if="!editingId && createStep === 2">
        <p v-if="isFirstFiscalYear" class="step2-desc">
          È il primo esercizio del condominio. Inserisci il saldo iniziale per ogni unità immobiliare
          (lascia 0 se non ci sono crediti/debiti pregressi).
        </p>
        <p v-else class="step2-desc" style="color: var(--text-secondary)">
          I saldi iniziali vengono propagati automaticamente dal saldo di chiusura dell'esercizio precedente.
          Per aggiornare i saldi usa il pulsante <strong>Propaga saldi</strong> sulla riga dell'esercizio.
        </p>
        <div v-if="loadingUnits" class="loading-state"><div class="spinner"></div></div>
        <div v-else-if="!unitBalances.length" class="empty-state" style="padding: 1rem 0">
          Nessuna unità immobiliare trovata per questo condominio.
        </div>
        <div v-else class="opening-balances-table-wrap">
          <table class="opening-balances-table">
            <thead>
              <tr>
                <th>Unità / Gruppo</th>
                <th class="col-num">Saldo iniziale (€)</th>
                <th v-if="isFirstFiscalYear">Note</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="ub in groupedUnitBalances" :key="ub.isGroup ? `group-${ub.billingGroupId}` : `unit-${ub.unitId}`">
                <td>
                  <span v-if="ub.isGroup" class="group-header-label">◈ {{ ub.unitName }}</span>
                  <span v-else>{{ ub.unitName }}</span>
                </td>
                <td class="col-num">
                  <input v-if="isFirstFiscalYear" class="inline-input" type="number" step="0.01"
                         v-model.number="ub.openingBalance" />
                  <span v-else class="mono">{{ fmt(ub.openingBalance) }}</span>
                </td>
                <td v-if="isFirstFiscalYear">
                  <input class="form-input form-input--sm" type="text"
                         v-model="ub.notes" placeholder="Note opzionali" />
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </template>

      <template #footer>
        <button class="btn btn-ghost" @click="closeCreateModal">Annulla</button>

        <!-- Crea step 1 -->
        <button v-if="!editingId && createStep === 1"
                class="btn btn-primary" @click="saveFiscalYear" :disabled="saving">
          <span v-if="saving" class="spinner" style="width:14px;height:14px"></span>
          Crea
        </button>

        <!-- Crea step 2 — solo se primo esercizio -->
        <template v-if="!editingId && createStep === 2 && isFirstFiscalYear">
          <button class="btn btn-ghost" @click="skipOpeningBalances" :disabled="savingBalances">
            Salta
          </button>
          <button class="btn btn-primary" @click="saveOpeningBalances" :disabled="savingBalances || loadingUnits">
            <span v-if="savingBalances" class="spinner" style="width:14px;height:14px"></span>
            Salva saldi
          </button>
        </template>
        <!-- Crea step 2 — EF con precedente: solo chiudi -->
        <button v-if="!editingId && createStep === 2 && !isFirstFiscalYear"
                class="btn btn-primary" @click="closeCreateModal">
          Chiudi
        </button>

        <!-- Modifica -->
        <button v-if="editingId"
                class="btn btn-primary" @click="saveFiscalYear" :disabled="saving">
          <span v-if="saving" class="spinner" style="width:14px;height:14px"></span>
          Salva
        </button>
      </template>
    </BaseModal>

    <!-- ══════════════════════════════════════════════════
         MODAL — Conferma eliminazione
    ══════════════════════════════════════════════════ -->
    <BaseModal :show="showDeleteModal"
               @close="showDeleteModal = false"
               title="Elimina esercizio"
               :subtitle="deleteTarget?.code"
               size="sm">
      <p class="transition-desc">
        Stai per eliminare l'esercizio <strong>{{ deleteTarget?.code }}</strong>.
        L'operazione è irreversibile. Continuare?
      </p>
      <template #footer>
        <button class="btn btn-ghost" @click="showDeleteModal = false">Annulla</button>
        <button class="btn btn-danger" @click="executeDelete" :disabled="deleting">
          <span v-if="deleting" class="spinner" style="width:14px;height:14px"></span>
          Elimina
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
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { useAppStore } from '@/stores/app'
import { fiscalYearApi, accountBalanceApi, unitApi } from '@/services/api'
import { usePermissions } from '@/composables/usePermissions'
import { useAccountingFormat } from '@/composables/useAccountingFormat'
import BaseModal from '@/components/BaseModal.vue'

const store = useAppStore()
const { canCreate, canEdit, canDelete } = usePermissions()
const { load: loadAccountingFormat, fmtAccounting } = useAccountingFormat()

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

// Saldi iniziali per unità
const unitOpeningBalances        = ref([])
const loadingUnitOpeningBalances = ref(false)

// Totali della tabella saldi iniziali per unità
const unitOpeningBalancesTotals = computed(() =>
  unitOpeningBalances.value.reduce((acc, ub) => {
    acc.openingBalance  += Number(ub.openingBalance)  || 0
    acc.rateAddebitate  += Number(ub.rateAddebitate)  || 0
    acc.rateIncassate   += Number(ub.rateIncassate)   || 0
    acc.quotaConsuntiva += Number(ub.quotaConsuntiva) || 0
    acc.saldoConguaglio += Number(ub.saldoConguaglio) || 0
    acc.closingBalance  += Number(ub.closingBalance)  || 0
    return acc
  }, { openingBalance: 0, rateAddebitate: 0, rateIncassate: 0, quotaConsuntiva: 0, saldoConguaglio: 0, closingBalance: 0 })
)

async function toggleDetail(fy) {
  if (expandedId.value === fy.id) {
    expandedId.value  = null
    detail.value      = null
    balances.value    = []
    unitOpeningBalances.value = []
    editingBalanceId.value = null
    return
  }
  expandedId.value  = fy.id
  detail.value      = null
  balances.value    = []
  unitOpeningBalances.value = []
  loadingSummary.value         = true
  loadingBalances.value        = true
  loadingUnitOpeningBalances.value = true
  try {
    const [summaryRes, balancesRes, unitBalancesRes] = await Promise.allSettled([
      fiscalYearApi.getById(fy.id),
      accountBalanceApi.getByFiscalYear(fy.id),
      unitApi.getOpeningBalancesByFiscalYear(fy.id),
    ])
    if (summaryRes.status     === 'fulfilled') detail.value              = summaryRes.value.data
    if (balancesRes.status    === 'fulfilled') balances.value            = balancesRes.value.data ?? []
    if (unitBalancesRes.status === 'fulfilled') unitOpeningBalances.value = unitBalancesRes.value.data ?? []
  } finally {
    loadingSummary.value             = false
    loadingBalances.value            = false
    loadingUnitOpeningBalances.value = false
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
const showModal  = ref(false)
const editingId  = ref(null)
const saving     = ref(false)
const createStep        = ref(1)   // 1 = dati esercizio, 2 = saldi iniziali (solo primo esercizio)
const isFirstFiscalYear = ref(true) // false quando l'EF ha un precedente (saldi propagati automaticamente)
const form       = ref({ code: '', description: '', startDate: '', endDate: '' })
const errors     = ref({})

// Selector esercizio precedente (quando non è il primo esercizio)
const selectedPreviousFiscalYearId = ref(null)

// Step 2: saldi iniziali unità
const unitBalances   = ref([])   // [{ unitId, unitName, openingBalance, notes }]
const loadingUnits   = ref(false)
const savingBalances = ref(false)
const createdFiscalYearId = ref(null)

function clearError(f) { delete errors.value[f] }

function validate() {
  errors.value = {}
  if (!form.value.code?.trim()) errors.value.code = 'Codice obbligatorio'
  if (!form.value.startDate)    errors.value.startDate = 'Data inizio obbligatoria'
  if (!form.value.endDate)      errors.value.endDate   = 'Data fine obbligatoria'
  if (form.value.startDate && form.value.endDate && form.value.endDate <= form.value.startDate)
    errors.value.endDate = 'La data fine deve essere successiva alla data inizio'
  if (fiscalYears.value.length > 0 && !selectedPreviousFiscalYearId.value)
    errors.value.previousFiscalYear = 'Seleziona l\'esercizio precedente'
  return Object.keys(errors.value).length === 0
}

// Operazioni su date come stringhe ISO "YYYY-MM-DD" senza conversione timezone.
// new Date("YYYY-MM-DD") è interpretato UTC e causa shift di un giorno nei fusi +N.
function isoAddDays(isoDate, days) {
  const [y, m, d] = isoDate.slice(0, 10).split('-').map(Number)
  const dt = new Date(y, m - 1, d + days)   // costruttore locale: nessun shift timezone
  return `${dt.getFullYear()}-${String(dt.getMonth() + 1).padStart(2, '0')}-${String(dt.getDate()).padStart(2, '0')}`
}

function isoAddYearsMinusOneDay(isoDate) {
  const [y, m, d] = isoDate.slice(0, 10).split('-').map(Number)
  const dt = new Date(y + 1, m - 1, d - 1)
  return `${dt.getFullYear()}-${String(dt.getMonth() + 1).padStart(2, '0')}-${String(dt.getDate()).padStart(2, '0')}`
}

function onPreviousFiscalYearChange() {
  clearError('previousFiscalYear')
  if (!selectedPreviousFiscalYearId.value) {
    form.value.startDate = ''
    form.value.endDate   = ''
    return
  }
  const prev = fiscalYears.value.find(f => f.id === selectedPreviousFiscalYearId.value)
  if (!prev?.endDate) return
  form.value.startDate = isoAddDays(prev.endDate, 1)
  form.value.endDate   = isoAddYearsMinusOneDay(form.value.startDate)
}

function openOpeningBalancesModal(fy) {
  editingId.value           = null
  createdFiscalYearId.value = fy.id
  isFirstFiscalYear.value   = !fy.previousFiscalYearId
  createStep.value          = 2
  errors.value              = {}
  unitBalances.value        = []
  showModal.value           = true
  loadUnitsForOpeningBalances()
}

function openCreateModal() {
  editingId.value                    = null
  createStep.value                   = 1
  selectedPreviousFiscalYearId.value = null
  errors.value                       = {}
  createdFiscalYearId.value          = null
  unitBalances.value                 = []

  // Per il primo esercizio, pre-popola le date con l'anno corrente
  if (fiscalYears.value.length === 0) {
    const y = new Date().getFullYear()
    const startDate = `${y}-01-01`
    form.value = { code: '', description: '', startDate, endDate: isoAddYearsMinusOneDay(startDate) }
  } else {
    form.value = { code: '', description: '', startDate: '', endDate: '' }
  }

  showModal.value = true
}

function closeCreateModal() {
  showModal.value = false
  if (createStep.value === 2) loadFiscalYears()
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
  createStep.value = 1
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
      showModal.value = false
      await loadFiscalYears()
    } else {
      const { data } = await fiscalYearApi.create({
        condominiumId:         store.selectedCondominioId,
        code:                  form.value.code.trim(),
        description:           form.value.description,
        startDate:             form.value.startDate,
        endDate:               form.value.endDate,
        previousFiscalYearId:  selectedPreviousFiscalYearId.value ?? null,
      })
      // Primo esercizio → passa allo step 2 per i saldi iniziali
      if (fiscalYears.value.length === 0) {
        createdFiscalYearId.value = data.id
        isFirstFiscalYear.value   = true
        unitBalances.value        = []
        createStep.value          = 2
        loadUnitsForOpeningBalances()
      } else {
        showModal.value = false
        await loadFiscalYears()
      }
    }
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { saving.value = false }
}

async function loadUnitsForOpeningBalances() {
  loadingUnits.value = true
  try {
    const { data } = await unitApi.getOpeningBalancesByFiscalYear(createdFiscalYearId.value)
    unitBalances.value = (data ?? []).map(b => ({
      unitId:           b.unitId,
      unitName:         b.unitName ?? `Unità ${b.unitId}`,
      openingBalance:   b.openingBalance,
      notes:            b.notes ?? '',
      isGroup:          b.isGroup ?? false,
      billingGroupId:   b.billingGroupId ?? null,
      billingGroupName: b.billingGroupName ?? null,
      millesimal:       b.millesimal ?? 0,
    }))
  } catch {
    unitBalances.value = []
  } finally {
    loadingUnits.value = false
  }
}

// Ogni riga è già indipendente: un gruppo di fatturazione conta come una singola "unità"
// con il proprio saldo (mai spalmato sulle unità componenti); le unità senza gruppo restano
// righe individuali. Ordinamento: gruppi e unità insieme, per nome.
const groupedUnitBalances = computed(() =>
  [...unitBalances.value].sort((a, b) => a.unitName.localeCompare(b.unitName))
)

async function saveOpeningBalances() {
  const fyId = createdFiscalYearId.value
  if (!fyId) { store.toast('Esercizio non identificato', 'error'); return }

  savingBalances.value = true
  try {
    await unitApi.setOpeningBalancesBulk({
      fiscalYearId: fyId,
      items: unitBalances.value
        .filter(ub => !ub.isGroup)
        .map(ub => ({
          unitId:         ub.unitId,
          openingBalance: ub.openingBalance,
          notes:          ub.notes || null,
        })),
      groupItems: unitBalances.value
        .filter(ub => ub.isGroup)
        .map(ub => ({
          billingGroupId: ub.billingGroupId,
          openingBalance: ub.openingBalance,
          notes:          ub.notes || null,
        })),
    })
    store.toast('Saldi iniziali salvati', 'success')
    showModal.value = false
    await loadFiscalYears()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { savingBalances.value = false }
}

function skipOpeningBalances() {
  showModal.value = false
  loadFiscalYears()
}

// ── Elimina ────────────────────────────────────────────────────────────────
// ── Propaga saldi iniziali ─────────────────────────────────────────────────
const propagatingId = ref(null)

async function propagateOpeningBalances(fy) {
  if (!confirm(`Propagare i saldi iniziali dall'esercizio precedente a "${fy.code}"?\nI saldi già presenti verranno aggiornati con il saldo di chiusura dell'esercizio precedente.`)) return
  propagatingId.value = fy.id
  try {
    const { data } = await fiscalYearApi.propagateOpeningBalances(fy.id)
    store.toast(`Saldi propagati: ${data.propagatedCount} unità aggiornate`, 'success')
    await loadFiscalYears()
  } catch (err) {
    if (!err?.response) store.toast('Errore di rete', 'error')
  } finally {
    propagatingId.value = null
  }
}

const showDeleteModal = ref(false)
const deleteTarget    = ref(null)
const deleting        = ref(false)

function openDeleteModal(fy) {
  deleteTarget.value    = fy
  showDeleteModal.value = true
}

async function executeDelete() {
  if (!deleteTarget.value) return
  deleting.value = true
  try {
    await fiscalYearApi.delete(deleteTarget.value.id)
    store.toast('Esercizio eliminato', 'success')
    showDeleteModal.value = false
    if (expandedId.value === deleteTarget.value.id) {
      expandedId.value = null
      detail.value     = null
      balances.value   = []
    }
    await loadFiscalYears()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { deleting.value = false }
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
  revertToDraft: {
    title: 'Riporta in Bozza',
    desc:  "L'esercizio tornerà in stato Bozza. Potrai inserire o correggere i saldi iniziali delle unità e riaprirlo in seguito. Consentito solo se non sono presenti movimenti (spese, rate o budget approvati).",
    btn:   'btn-muted',
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
    if (transitionType.value === 'open')               await fiscalYearApi.open(id)
    else if (transitionType.value === 'startClosing')  await fiscalYearApi.startClosing(id, notes)
    else if (transitionType.value === 'close')         await fiscalYearApi.close(id, notes)
    else if (transitionType.value === 'lock')          await fiscalYearApi.lock(id, notes)
    else if (transitionType.value === 'revertToDraft') await fiscalYearApi.revertToDraft(id)
    showTransitionModal.value = false
    if (expandedId.value === id) {
      const { data } = await fiscalYearApi.getById(id)
      detail.value = data
    }
    await loadFiscalYears()
    // Dopo il revert apri subito la modale saldi iniziali
    if (transitionType.value === 'revertToDraft') {
      const reverted = fiscalYears.value.find(f => f.id === id)
      createdFiscalYearId.value = id
      isFirstFiscalYear.value   = !reverted?.previousFiscalYearId
      editingId.value           = null
      createStep.value          = 2
      unitBalances.value        = []
      showModal.value           = true
      loadUnitsForOpeningBalances()
    }
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
onMounted(() => { loadFiscalYears(); loadAccountingFormat() })
onUnmounted(() => window.removeEventListener('app:refresh', loadFiscalYears))
window.addEventListener('app:refresh', loadFiscalYears)
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

/* Icona danger (elimina) */
.btn-icon--danger       { color: var(--accent-red, #ef4444) !important; }
.btn-icon--danger:hover { background: rgba(239,68,68,.1) !important; }

/* Chevron espandi/comprimi */
.btn-icon--expand       { color: var(--text-secondary); transition: color .15s; }
.btn-icon--expand:hover { color: var(--text); background: transparent !important; }
.btn-icon--expand svg   { transition: transform .2s ease; }
.btn-icon--expand.expanded svg { transform: rotate(180deg); }

/* Bottone elimina nel modal */
.btn-danger       { background: var(--accent-red, #ef4444); color: #fff; border: none; }
.btn-danger:hover { background: #dc2626; }
.btn-danger:disabled { opacity: 0.6; cursor: not-allowed; }

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
.balances-table tfoot td               { padding: 0.5rem 0.75rem; border-top: 2px solid var(--border); font-weight: 700; background: var(--bg); }
.balances-total-row td:first-child     { color: var(--text-muted); text-transform: uppercase; font-size: 0.72rem; letter-spacing: 0.03em; }
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

/* Inline editing — cella cliccabile */
.cell-editable        { cursor: pointer; }
.cell-editable:hover  { background: var(--bg-surface); }
.edit-hint            { margin-left: 0.4rem; font-size: 0.7rem; color: var(--text-muted);
                        opacity: 0; transition: opacity 0.15s; }
.cell-editable:hover .edit-hint { opacity: 1; }

/* Input readonly (non editabile visivamente) */
.input-readonly { opacity: 0.65; cursor: default; background: var(--bg-base); }

/* Hint sotto campo readonly */
.field-hint { display: block; font-size: 0.72rem; color: var(--text-muted); margin-top: 0.2rem; }

/* Step 2 — descrizione */
.step2-desc { font-size: 0.875rem; color: var(--text-muted); margin-bottom: 1rem; line-height: 1.5; }

/* Step 2 — tabella saldi iniziali unità */
.opening-balances-table-wrap { overflow-x: auto; border: 1px solid var(--border); border-radius: 6px; max-height: 340px; overflow-y: auto; }
.opening-balances-table      { width: 100%; border-collapse: collapse; font-size: 0.825rem; }
.opening-balances-table th   { background: var(--bg); color: var(--text-muted); font-size: 0.72rem; font-weight: 600;
                                text-transform: uppercase; letter-spacing: 0.5px;
                                padding: 0.45rem 0.75rem; text-align: left; border-bottom: 1px solid var(--border);
                                position: sticky; top: 0; z-index: 1; }
.opening-balances-table td   { padding: 0.4rem 0.75rem; border-bottom: 1px solid var(--border); vertical-align: middle; }
.opening-balances-table tbody tr:last-child td { border-bottom: none; }
.opening-balances-table .form-input--sm { padding: 0.2rem 0.4rem; font-size: 0.8rem; height: auto; }
.group-header-row td { background: var(--accent-glow, #eef2ff); font-weight: 600; }
.group-header-label { color: var(--accent); }

@media (max-width: 768px) {
  .table-wrap table th,
  .table-wrap table td {
    white-space: nowrap;
  }
  .detail-panel {
    padding: 1rem;
  }
  .detail-grid {
    gap: 0.6rem;
  }
  .detail-card {
    flex: 1 1 120px;
  }
}
</style>
