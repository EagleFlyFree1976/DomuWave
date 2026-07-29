<template>
  <div>
    <!-- ── Header ──────────────────────────────────────── -->
    <div class="page-header">
      <h1>Movimenti</h1>
      <div v-if="canCreate" class="header-actions">
        <button class="btn btn-primary" @click="openExpenseModal(null, 'Uscita')">+ Inserisci spesa</button>
        <button class="btn btn-primary" @click="openExpenseModal(null, 'Entrata')">+ Inserisci entrata</button>
        <button class="btn btn-primary" @click="openExpenseModal(null, 'Patrimoniale')">+ Inserisci patrimoniale</button>
      </div>
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

      <select class="form-select" v-model.number="movementTypeFilter" style="width:160px" @change="resetAndLoad">
        <option :value="null">Tutti i movimenti</option>
        <option :value="1">Entrata</option>
        <option :value="2">Spesa</option>
        <option :value="3">Patrimoniale</option>
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
        <option :value="1">Non evasa</option>
        <option :value="2">Evasa</option>
      </select>

      <select class="form-select" v-model.number="accountFilter" style="min-width:200px" @change="resetAndLoad">
        <option :value="null">Tutti i conti</option>
        <optgroup v-for="grp in expenseAccountGroups" :key="grp.typeId" :label="grp.typeLabel">
          <option v-for="r in grp.rows" :key="r.accountId" :value="r.accountId">{{ r.label }}</option>
        </optgroup>
      </select>

      <select class="form-select" v-model.number="supplierFilter" style="min-width:180px" @change="resetAndLoad">
        <option :value="null">Tutti i fornitori</option>
        <option v-for="s in suppliers" :key="s.id" :value="s.id">{{ s.name || s.companyName }}</option>
      </select>

      <div class="filter-daterange">
        <span class="filter-daterange-label">Data documento</span>
        <input class="form-input" type="date" v-model="docDateFrom" @change="resetAndLoad" title="Dal" />
        <span class="filter-daterange-sep">–</span>
        <input class="form-input" type="date" v-model="docDateTo" @change="resetAndLoad" title="Al" />
      </div>

      <div class="filter-daterange">
        <span class="filter-daterange-label">Data pagamento</span>
        <input class="form-input" type="date" v-model="payDateFrom" @change="resetAndLoad" title="Dal" />
        <span class="filter-daterange-sep">–</span>
        <input class="form-input" type="date" v-model="payDateTo" @change="resetAndLoad" title="Al" />
      </div>

      <button v-if="hasActiveFilters" class="btn btn-ghost btn-sm" @click="clearFilters" title="Azzera filtri">
        ✕ Azzera filtri
      </button>
    </div>

    <!-- ── Toolbar export ──────────────────────────────── -->
    <div class="export-toolbar">
      <label class="checkbox-label group-toggle">
        <input type="checkbox" v-model="groupBySupplierExport" />
        Raggruppa per fornitore
      </label>
      <button class="btn btn-ghost btn-sm" :disabled="exporting || !expenses.length" @click="onExportPdf">
        <span v-if="exporting === 'pdf'" class="spinner" style="width:12px;height:12px"></span>
        <template v-else>🖨 Stampa PDF</template>
      </button>
      <button class="btn btn-ghost btn-sm" :disabled="exporting || !expenses.length" @click="onExportExcel">
        <span v-if="exporting === 'excel'" class="spinner" style="width:12px;height:12px"></span>
        <template v-else>⇩ Esporta Excel</template>
      </button>
    </div>

    <!-- ── Tabella ──────────────────────────────────────── -->
    <div class="card">
      <div v-if="loadingExp" class="loading-state"><div class="spinner"></div></div>
      <div v-else-if="!expenses.length" class="empty-state">
        <div class="empty-icon">◎</div><div>Nessuna spesa trovata</div>
      </div>
      <div v-else class="table-wrap">
        <AppPaginator v-model="currentPage" v-model:pageSize="pageSize" :total-count="totalCount" />
        <table class="cards-on-mobile">
          <thead>
            <tr>
              <th class="sortable" @click="setSort('documentdate')">
                Data <span class="sort-icon">{{ sortIcon('documentdate') }}</span>
              </th>
              <th>Tipo</th>
              <th>Descrizione</th>
              <th class="sortable" @click="setSort('suppliername')">
                Fornitore <span class="sort-icon">{{ sortIcon('suppliername') }}</span>
              </th>
              <th>Esercizio</th>
              <th class="text-right sortable" @click="setSort('grossamount')">
                Importo <span class="sort-icon">{{ sortIcon('grossamount') }}</span>
              </th>
              <th class="text-right">IVA</th>
              <th class="sortable" @click="setSort('paymentdate')">
                Data pagamento <span class="sort-icon">{{ sortIcon('paymentdate') }}</span>
              </th>
              <th>Stato</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="e in expenses" :key="e.id">
              <td data-label="Data" class="mono text-secondary">{{ fmtDate(e.documentDate) }}</td>
              <td data-label="Tipo"><span class="badge" :class="movementBadge(e.accountType)">{{ movementLabel(e.accountType) }}</span></td>
              <td data-label="Descrizione">{{ e.name }}</td>
              <td data-label="Fornitore">{{ e.supplierName || '—' }}</td>
              <td data-label="Esercizio" class="text-secondary">{{ e.fiscalYearCode || '—' }}</td>
              <td data-label="Importo" class="mono text-right">{{ fmt(e.grossAmount) }}</td>
              <td data-label="IVA" class="mono text-right text-secondary">{{ fmt(e.vatAmount) }}</td>
              <td data-label="Data pagamento" class="mono text-secondary">{{ e.paymentDate ? fmtDate(e.paymentDate) : '—' }}</td>
              <td data-label="Stato"><span class="badge" :class="payBadge(e.paymentStatusId)">{{ paymentStatusLabel(e) }}</span></td>
              <td>
                <div class="row-actions">
                  <button v-if="canEdit && e.paymentStatusId !== 2" class="action-pill action-pill--pay"
                          :disabled="actionInProgress[`pay-${e.id}`]"
                          :title="isIncome(e) ? 'Incassa' : 'Paga'"
                          @click="markPaid(e.id)">
                    <span v-if="actionInProgress[`pay-${e.id}`]" class="spinner" style="width:12px;height:12px"></span>
                    <template v-else>
                      <span class="action-pill__icon">€</span>
                      <span class="action-pill__label">{{ isIncome(e) ? 'Incassa' : 'Paga' }}</span>
                    </template>
                  </button>
                  <button v-if="canEdit && e.paymentStatusId === 2" class="action-pill action-pill--undo"
                          :disabled="actionInProgress[`pay-${e.id}`]"
                          :title="isIncome(e) ? 'Segna non incassata' : 'Segna non pagata'"
                          @click="markUnpaid(e.id)">
                    <span v-if="actionInProgress[`pay-${e.id}`]" class="spinner" style="width:12px;height:12px"></span>
                    <span v-else class="action-pill__icon">↺</span>
                  </button>
                  <span class="action-sep"></span>
                  <button v-if="canEdit" class="action-icon-btn"
                          :disabled="actionInProgress[`pay-${e.id}`] || actionInProgress[`del-${e.id}`]"
                          title="Modifica"
                          @click="openExpenseModal(e)">✎</button>
                  <button v-if="canDelete" class="action-icon-btn action-icon-btn--danger"
                          :disabled="actionInProgress[`del-${e.id}`]"
                          title="Elimina"
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
      :title="modalTitle"
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
        <div class="form-group" v-if="expForm.movementType === 'Uscita'" :class="{ 'has-error': expErrors.expenseTypeId }">
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

        <!-- ── Spesa: dettaglio fiscale completo ──────────────────── -->
        <template v-if="expForm.movementType === 'Uscita'">
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
        </template>

        <!-- ── Entrata / Patrimoniale: solo importo ───────────────── -->
        <div v-else class="form-group" style="grid-column: span 2" :class="{ 'has-error': expErrors.taxableAmount }">
          <label class="form-label">Importo (€) *</label>
          <input class="form-input" type="number" step="0.01" min="0"
                 v-model.number="expForm.taxableAmount"
                 @input="clearExpError('taxableAmount'); autoCalcGross()" />
          <span v-if="expErrors.taxableAmount" class="field-error">{{ expErrors.taxableAmount }}</span>
        </div>

        <div class="form-group" style="grid-column: span 2" :class="{ 'has-error': expErrors.accountId }">
          <label class="form-label">Conto *</label>
          <select class="form-select" v-model.number="expForm.accountId" @change="clearExpError('accountId')">
            <option :value="null" disabled>Seleziona conto…</option>
            <optgroup v-for="grp in filteredAccountGroups" :key="grp.typeLabel" :label="grp.typeLabel">
              <option v-for="r in grp.rows" :key="r.accountId" :value="r.accountId">{{ r.label }}</option>
            </optgroup>
          </select>
          <span v-if="expErrors.accountId" class="field-error">{{ expErrors.accountId }}</span>
        </div>
        <div class="form-group" style="grid-column:span 2" :class="{ 'has-error': expErrors.allocation }">
          <label class="form-label">{{ expForm.movementType === 'Entrata' ? 'Imputazione entrata' : 'Imputazione spesa' }} *</label>
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
        <div class="form-group">
          <label class="form-label">Stato pagamento</label>
          <label class="checkbox-label" style="margin-top:0.35rem">
            <input type="checkbox" :checked="expForm.paymentStatusId === 2"
                   @change="onPaidToggle($event.target.checked)" />
            Spesa pagata / evasa
          </label>
        </div>
        <div class="form-group" v-if="expForm.paymentStatusId === 2">
          <label class="form-label">Data pagamento</label>
          <input class="form-input" type="date" v-model="expForm.paymentDate" />
        </div>
      </div>
      <div class="form-group form-group--full" style="margin-top:0.5rem">
        <label class="form-label">{{ expForm.movementType === 'Entrata' ? 'A favore di' : 'A carico di' }}</label>
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

    <!-- ══════════════════════════════════════════════════
         MODAL — Data di pagamento (segna come pagata)
    ══════════════════════════════════════════════════ -->
    <BaseModal
      :show="showPayModal"
      @close="showPayModal = false"
      title="Segna come pagata"
      :subtitle="store.selectedCondominio?.name"
      size="sm"
    >
      <div class="form-group">
        <label class="form-label">Data di pagamento</label>
        <input class="form-input" type="date" v-model="payDate" @keyup.enter="confirmMarkPaid" />
      </div>
      <template #footer>
        <button class="btn btn-ghost" @click="showPayModal = false">Annulla</button>
        <button class="btn btn-primary" @click="confirmMarkPaid" :disabled="!payDate">Conferma</button>
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
import { exportExpensesPdf, exportExpensesExcel } from '@/composables/useExpenseExport'
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
const movementTypeFilter  = ref(qInt('movementType'))
const accountFilter       = ref(qInt('accountId'))
const supplierFilter      = ref(qInt('supplierId'))
const docDateFrom         = ref(qStr('docFrom'))
const docDateTo           = ref(qStr('docTo'))
const payDateFrom         = ref(qStr('payFrom'))
const payDateTo           = ref(qStr('payTo'))
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
  if (movementTypeFilter.value)                q.movementType = movementTypeFilter.value
  if (accountFilter.value)                     q.accountId = accountFilter.value
  if (supplierFilter.value)                    q.supplierId = supplierFilter.value
  if (docDateFrom.value)                       q.docFrom   = docDateFrom.value
  if (docDateTo.value)                         q.docTo     = docDateTo.value
  if (payDateFrom.value)                       q.payFrom   = payDateFrom.value
  if (payDateTo.value)                         q.payTo     = payDateTo.value
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

const hasActiveFilters = computed(() =>
  !!(search.value || expTypeFilter.value || paymentStatusFilter.value || movementTypeFilter.value
     || accountFilter.value || supplierFilter.value || docDateFrom.value || docDateTo.value || payDateFrom.value || payDateTo.value))

function clearFilters() {
  search.value = ''
  expTypeFilter.value = null
  paymentStatusFilter.value = null
  movementTypeFilter.value = null
  accountFilter.value = null
  supplierFilter.value = null
  docDateFrom.value = ''
  docDateTo.value = ''
  payDateFrom.value = ''
  payDateTo.value = ''
  resetAndLoad()
}

// ─── Metodi di pagamento ──────────────────────────────────────
const paymentMethods = ref([])

// ─── Spese ────────────────────────────────────────────────────
const expenses         = ref([])
const loadingExp       = ref(false)
const groupBySupplierExport = ref(false)
const exporting        = ref(null)   // null | 'pdf' | 'excel'
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

// Cambio tipo movimento: azzera conto (non più valido per il nuovo tipo) e campi fiscali non pertinenti
function onMovementTypeChange() {
  expForm.value.accountId = null
  if (expForm.value.movementType !== 'Uscita') {
    expForm.value.taxableAmountVatExempt = 0
    expForm.value.vatAmount      = 0
    expForm.value.pensionFund    = 0
    expForm.value.withholdingTax = 0
    expForm.value.stampDuty      = 0
    expForm.value.expenseTypeId  = 6   // Altro — non mostrato in UI per Entrata/Patrimoniale
  }
  clearExpError('accountId')
  clearExpError('expenseTypeId')
  autoCalcGross()
}

function validateExpForm() {
  const e = {}
  const f = expForm.value
  const isUscita = f.movementType === 'Uscita'
  if (!f.fiscalYearId)          e.fiscalYearId     = 'Selezionare un esercizio fiscale'
  if (!f.name?.trim())          e.name             = 'Campo obbligatorio'
  if (isUscita && !f.expenseTypeId) e.expenseTypeId = 'Selezionare un tipo spesa'
  if (isUscita) {
    const hasTaxable    = f.taxableAmount          > 0
    const hasTaxExempt  = f.taxableAmountVatExempt > 0
    if (!hasTaxable && !hasTaxExempt)
      e.taxableBase = 'Inserire almeno un importo tra Imponibile e Imponibile esente IVA'
    if (hasTaxable && !(f.vatAmount > 0))
      e.vatAmount = "L'IVA è obbligatoria quando è presente l'imponibile"
  } else {
    if (!(f.taxableAmount > 0)) e.taxableAmount = "Inserire l'importo"
  }
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

// Modale "Segna come pagata" (scelta data di pagamento)
const showPayModal = ref(false)
const payDate      = ref(today)
const payTargetId  = ref(null)

const suppliers        = ref([])
const millesimalTables = ref([])
const units            = ref([])
const enabledMillesimalTables = computed(() => {
  const currentId = editingExp.value ? expForm.value.millesimalTableId : null
  return millesimalTables.value.filter(t => t.isEnabled || t.id === currentId)
})

const emptyExpForm = () => ({
  movementType: 'Uscita',   // 'Uscita' | 'Entrata' | 'Patrimoniale'
  fiscalYearId: null,
  name: '', documentNumber: '', documentDate: today, registrationDate: today,
  taxableAmount: 0, taxableAmountVatExempt: 0,
  grossAmount: 0, vatAmount: 0, netAmount: 0,
  pensionFund: 0, withholdingTax: 0, stampDuty: 0,
  expenseTypeId: 0, paymentStatusId: 1, paymentDate: null,
  paymentMethodId: null, supplierId: null, accountId: null, millesimalTableId: null, description: '',
  chargeabilityTypeId: 1,
  send770: false,
  allocationMode: 'table', unitId: null,
})

function autoCalcGross() {
  const f = expForm.value
  if (f.movementType !== 'Uscita') {
    f.grossAmount = Math.round((Number(f.taxableAmount) || 0) * 100) / 100
    return
  }
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

// Conti filtrati in base al tipo movimento selezionato (Spesa→Uscita, Entrata→Entrata, Patrimoniale→Patrimoniale)
const filteredAccountGroups = computed(() =>
  expenseAccountGroups.value.filter(g => g.typeId === expForm.value.movementType)
)

const MOVEMENT_LABELS = { Uscita: 'spesa', Entrata: 'entrata', Patrimoniale: 'movimento patrimoniale' }
const modalTitle = computed(() => {
  if (editingExp.value) return `Modifica ${MOVEMENT_LABELS[expForm.value.movementType] ?? 'spesa'}`
  return `Nuova ${MOVEMENT_LABELS[expForm.value.movementType] ?? 'spesa'}`
})

async function loadExpenses() {
  if (!store.selectedCondominioId) return
  syncUrl()
  loadingExp.value = true
  try {
    const { data } = await expenseApi.getByCondominium(store.selectedCondominioId, {
      page:            currentPage.value,
      pageSize:        pageSize.value,
      sortField:       sortField.value,
      sortAsc:         sortAsc.value,
      search:          search.value || undefined,
      expenseTypeId:   expTypeFilter.value  || undefined,
      paymentStatusId: paymentStatusFilter.value || undefined,
      // Filtro per esercizio fiscale tramite l'assegnazione della spesa, non per range di date.
      fiscalYearId:    selectedFiscalYearId.value || undefined,
      accountType:     movementTypeFilter.value || undefined,
      accountId:       accountFilter.value || undefined,
      supplierId:      supplierFilter.value || undefined,
      dateFrom:        docDateFrom.value || undefined,
      dateTo:          docDateTo.value || undefined,
      paymentDateFrom: payDateFrom.value || undefined,
      paymentDateTo:   payDateTo.value || undefined,
    })
    expenses.value   = data?.items      ?? []
    totalCount.value = data?.totalCount ?? 0
  } catch { expenses.value = []; totalCount.value = 0 } finally { loadingExp.value = false }
}

// Carica TUTTI i movimenti che rispettano i filtri correnti (non solo la pagina a schermo),
// per l'export PDF/Excel.
async function loadAllFilteredExpenses() {
  const { data } = await expenseApi.getByCondominium(store.selectedCondominioId, {
    page:            1,
    pageSize:        100000,
    sortField:       sortField.value,
    sortAsc:         sortAsc.value,
    search:          search.value || undefined,
    expenseTypeId:   expTypeFilter.value  || undefined,
    paymentStatusId: paymentStatusFilter.value || undefined,
    fiscalYearId:    selectedFiscalYearId.value || undefined,
    accountType:     movementTypeFilter.value || undefined,
    accountId:       accountFilter.value || undefined,
    supplierId:      supplierFilter.value || undefined,
    dateFrom:        docDateFrom.value || undefined,
    dateTo:          docDateTo.value || undefined,
    paymentDateFrom: payDateFrom.value || undefined,
    paymentDateTo:   payDateTo.value || undefined,
  })
  return data?.items ?? []
}

function exportSubtitle() {
  const fy = fiscalYears.value.find(f => f.id === selectedFiscalYearId.value)
  const parts = [store.selectedCondominio?.name]
  if (fy) parts.push(`${fy.code}${fy.description ? ' – ' + fy.description : ''}`)
  return parts.filter(Boolean).join(' — ')
}

async function onExportPdf() {
  if (!store.selectedCondominioId) return
  exporting.value = 'pdf'
  try {
    const all = await loadAllFilteredExpenses()
    await exportExpensesPdf(all, {
      subtitle: exportSubtitle(),
      groupBySupplier: groupBySupplierExport.value,
      filename: 'movimenti',
    })
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { exporting.value = null }
}

async function onExportExcel() {
  if (!store.selectedCondominioId) return
  exporting.value = 'excel'
  try {
    const all = await loadAllFilteredExpenses()
    exportExpensesExcel(all, {
      subtitle: exportSubtitle(),
      groupBySupplier: groupBySupplierExport.value,
      filename: 'movimenti',
    })
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { exporting.value = null }
}

async function openExpenseModal(e = null, movementType = 'Uscita') {
  expErrors.value  = {}
editingExp.value = e?.id ?? null
  savedExpenseId.value = null
  attachedDocs.value   = []

  // Auto-seleziona l'esercizio attivo per le nuove spese
  const defaultFyId = selectableFiscalYears.value.find(f => f.isActive)?.id
    ?? selectableFiscalYears.value[0]?.id
    ?? null

  expForm.value = e ? {
    movementType:       'Uscita',   // corretto sotto, dopo il caricamento dei conti
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
    paymentDate:        e.paymentDate?.slice(0, 10) ?? null,
    paymentMethodId:    e.paymentMethodId ?? null,
    supplierId:         e.supplierId ?? null,
    accountId:          e.accountId ?? null,
    millesimalTableId:  e.millesimalTableId ?? null,
    unitId:             e.unitId ?? null,
    allocationMode:     e.unitId ? 'unit' : 'table',
    description:        e.description ?? '',
    chargeabilityTypeId: e.chargeabilityTypeId ?? 1,
    send770:            e.send770 ?? false,
  } : { ...emptyExpForm(), fiscalYearId: defaultFyId, movementType, expenseTypeId: movementType === 'Uscita' ? 0 : 6 }

  if (!store.fiscalYears.length) await store.loadFiscalYears()

  if (!accountsLoaded && store.selectedCondominioId) {
    try {
      const { data } = await chartOfAccountsApi.getByCondominium(store.selectedCondominioId)
      chartOfAccounts.value = data ?? []
      accountsLoaded = true
    } catch { chartOfAccounts.value = [] }
  }
  buildExpenseAccountGroups()

  if (e) {
    const account = chartOfAccounts.value.find(a => a.id === e.accountId)
    const TYPE_MAP = { 1: 'Entrata', 2: 'Uscita', 3: 'Patrimoniale' }
    expForm.value.movementType = TYPE_MAP[account?.type] ?? 'Uscita'
  }

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
    const isUscita = expForm.value.movementType === 'Uscita'
    const payload = {
      name:               expForm.value.name,
      documentNumber:     expForm.value.documentNumber || null,
      documentDate:       expForm.value.documentDate || null,
      registrationDate:   expForm.value.registrationDate || null,
      taxableAmount:          expForm.value.taxableAmount          || 0,
      taxableAmountVatExempt: isUscita ? (expForm.value.taxableAmountVatExempt || 0) : 0,
      vatAmount:              isUscita ? (expForm.value.vatAmount              || 0) : 0,
      pensionFund:            isUscita ? (expForm.value.pensionFund            || 0) : 0,
      withholdingTax:         isUscita ? (expForm.value.withholdingTax         || 0) : 0,
      stampDuty:              isUscita ? (expForm.value.stampDuty              || 0) : 0,
      expenseTypeId:      isUscita ? expForm.value.expenseTypeId : 6,
      paymentStatusId:    expForm.value.paymentStatusId || 1,
      paymentDate:        expForm.value.paymentStatusId === 2 ? (expForm.value.paymentDate || null) : null,
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

// Toggle "spesa pagata" nel form: imposta lo stato e propone una data di pagamento
// di default (data di registrazione, altrimenti oggi) senza forzarla a "oggi".
function onPaidToggle(checked) {
  if (checked) {
    expForm.value.paymentStatusId = 2
    if (!expForm.value.paymentDate)
      expForm.value.paymentDate = expForm.value.registrationDate || today
  } else {
    expForm.value.paymentStatusId = 1
    expForm.value.paymentDate = null
  }
}

// Apre la modale per scegliere la data di pagamento (default oggi).
function markPaid(id) {
  payTargetId.value = id
  payDate.value = today
  showPayModal.value = true
}

async function confirmMarkPaid() {
  const id = payTargetId.value
  if (!id || !payDate.value) return
  showPayModal.value = false
  actionInProgress.value[`pay-${id}`] = true
  try { await expenseApi.markAsPaid(id, payDate.value, null); await loadExpenses() }
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

// accountType arriva dal backend come enum ChartOfAccountsType (1=Entrata, 2=Uscita, 3=Patrimoniale)
const ACCOUNT_TYPE_LABELS = { 1: 'Entrata', 2: 'Spesa', 3: 'Patrimoniale' }
const ACCOUNT_TYPE_BADGES = { 1: 'badge-green', 2: 'badge-red', 3: 'badge-muted' }
function movementLabel(accountType) { return ACCOUNT_TYPE_LABELS[accountType] ?? 'Spesa' }
function movementBadge(accountType) { return ACCOUNT_TYPE_BADGES[accountType] ?? 'badge-muted' }
function isIncome(e) { return e.accountType === 1 }
function paymentStatusLabel(e) {
  return e.paymentStatusId === 2 ? 'Evasa' : 'Non evasa'
}

// ─── Watchers / Init ──────────────────────────────────────────
watch(() => store.selectedCondominioId, () => {
  accountsLoaded = false
  expenseAccountGroups.value = []
  search.value              = ''
  expTypeFilter.value       = null
  paymentStatusFilter.value = null
  movementTypeFilter.value  = null
  accountFilter.value       = null
  supplierFilter.value      = null
  selectedFiscalYearId.value = null
  currentPage.value         = 1
  loadExpenses()
})

// I filtri che cambiano pagina resettano a 1 via resetAndLoad; currentPage chiama direttamente
watch(currentPage,         loadExpenses)
watch(pageSize,            () => { currentPage.value = 1; loadExpenses() })
watch(selectedFiscalYearId, () => { currentPage.value = 1; loadExpenses() })

// Carica i conti del condominio (riusati sia dalla modale sia dal filtro).
async function ensureAccountsLoaded() {
  if (accountsLoaded || !store.selectedCondominioId) return
  try {
    const { data } = await chartOfAccountsApi.getByCondominium(store.selectedCondominioId)
    chartOfAccounts.value = data ?? []
    accountsLoaded = true
    buildExpenseAccountGroups()
  } catch { chartOfAccounts.value = [] }
}

onMounted(async () => {
  if (!store.fiscalYears.length) await store.loadFiscalYears()
  const { data } = await expenseApi.getPaymentMethods()
  paymentMethods.value = data ?? []
  await ensureAccountsLoaded()
  try {
    const { data: supData } = await supplierApi.getAll()
    suppliers.value = supData ?? []
  } catch { suppliers.value = [] }
  await loadExpenses()
})
onUnmounted(() => window.removeEventListener('app:refresh', loadExpenses))
window.addEventListener('app:refresh', loadExpenses)
</script>

<style scoped>
.tab-toolbar { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1rem; flex-wrap: wrap; }
.header-actions { display: flex; gap: 0.5rem; flex-wrap: wrap; }

/* ── Export toolbar ─────────────────────────────────────────────── */
.export-toolbar {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 0.75rem;
  margin-bottom: 1rem;
  flex-wrap: wrap;
}
.export-toolbar .group-toggle {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.85rem;
  color: var(--text-secondary);
  cursor: pointer;
  user-select: none;
  margin-right: auto;
}
.export-toolbar .group-toggle input { cursor: pointer; }

/* ── Filtro range di date ──────────────────────────────────────── */
.filter-daterange { display: inline-flex; align-items: center; gap: 0.35rem; }
.filter-daterange-label { font-size: 0.75rem; font-weight: 600; color: var(--text-secondary); white-space: nowrap; }
.filter-daterange-sep { color: var(--text-muted); }
.filter-daterange .form-input { width: 140px; }

/* ── Azioni riga tabella movimenti ─────────────────────────────── */
.row-actions { display: flex; align-items: center; gap: 0.35rem; justify-content: flex-end; }

.action-pill {
  display: inline-flex; align-items: center; gap: 0.35rem;
  border: 1px solid var(--border); border-radius: 999px;
  background: var(--bg-surface); color: var(--text-secondary);
  padding: 0.3rem 0.65rem; font-size: 0.78rem; font-weight: 600;
  cursor: pointer; transition: background 0.15s, border-color 0.15s, color 0.15s, transform 0.1s;
  line-height: 1;
}
.action-pill:disabled { opacity: 0.5; cursor: not-allowed; }
.action-pill__icon { font-size: 0.85rem; line-height: 1; }
.action-pill__label { white-space: nowrap; }

.action-pill--pay {
  border-color: var(--accent-green); color: var(--accent-green);
}
.action-pill--pay:hover:not(:disabled) {
  background: var(--accent-green); color: #fff; transform: translateY(-1px);
}

.action-pill--undo {
  width: 1.85rem; height: 1.85rem; padding: 0; justify-content: center;
  border-color: var(--border-active); color: var(--text-muted);
}
.action-pill--undo:hover:not(:disabled) {
  background: var(--accent-glow, rgba(99,102,241,0.1)); color: var(--accent); border-color: var(--accent);
}
.action-pill--undo .action-pill__icon { font-size: 1rem; }

.action-sep {
  width: 1px; height: 1.1rem; background: var(--border); margin: 0 0.15rem;
}

.action-icon-btn {
  display: inline-flex; align-items: center; justify-content: center;
  width: 1.85rem; height: 1.85rem; border-radius: 6px;
  border: 1px solid transparent; background: transparent; color: var(--text-secondary);
  cursor: pointer; font-size: 0.85rem; transition: background 0.15s, color 0.15s;
}
.action-icon-btn:hover:not(:disabled) { background: var(--accent-glow, rgba(99,102,241,0.1)); color: var(--accent); }
.action-icon-btn:disabled { opacity: 0.4; cursor: not-allowed; }
.action-icon-btn--danger:hover:not(:disabled) { background: rgba(239,68,68,0.12); color: var(--accent-red); }

.radio-group { display: flex; gap: 1.25rem; flex-wrap: wrap; padding: .35rem 0; }
.radio-label {
  display: flex; align-items: center; gap: .4rem;
  font-size: .875rem; cursor: pointer; user-select: none;
}
.radio-label input[type="radio"] { cursor: pointer; }
.radio-label input[type="radio"]:disabled { cursor: not-allowed; }

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
