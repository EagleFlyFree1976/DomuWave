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

        <button v-if="canCreate && canCreateBudget" class="btn btn-primary" style="margin-left:auto"
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
                <th class="text-right">Entrate prev.</th>
                <th class="text-right">Uscite prev.</th>
                <th>Approvazione</th>
                <th>Stato</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="b in budgets" :key="b.id">
                <td><span class="text-muted mono">#{{ b.id }}</span> {{ b.type === 1 ? 'Preventivo' : 'Consuntivo' }}</td>
                <td class="mono text-right text-green">{{ fmt(b.totalIncome) }}</td>
                <td class="mono text-right text-red">{{ fmt(b.totalExpenses) }}</td>
                <td class="text-secondary">{{ fmtDate(b.approvalDate) }}</td>
                <td><span class="badge" :class="statusBadge(b.statusId)">{{ statusLabel(b.statusId) }}</span></td>
                <td>
                  <div class="row-actions">
                    <button class="btn btn-sm btn-ghost" @click="openItemsModal(b)">Voci</button>
                    <button v-if="b.type === 1 && (b.statusId === 2 || b.statusId === 3)"
                            class="btn btn-sm btn-ghost" @click="goToRate(b)" title="Vai alle rate generate">Rate ↗</button>
                    <button v-if="canEdit && b.statusId === 1 && !blockedTypes.has(b.type)"
                            class="btn btn-sm btn-ghost"
                            @click="openApproveModal(b)">Approva</button>
                    <span v-if="canEdit && b.statusId === 1 && blockedTypes.has(b.type)"
                          class="badge badge-muted" title="Esiste già un budget di questo tipo approvato o chiuso">
                      Non approvabile
                    </span>
                    <button v-if="canEdit && b.statusId === 2" class="btn btn-sm btn-ghost"
                            @click="closeBudget(b)">Chiudi</button>
                    <button v-if="session.isSuperAdmin && b.statusId === 2" class="btn btn-sm btn-ghost btn-reopen"
                            @click="reopenBudget(b)" title="Riapri in bozza (SuperAdmin)">Riapri</button>
                    <button v-if="canEdit && b.statusId === 2 && b.type === 1"
                            class="btn btn-sm btn-ghost"
                            @click="openGenerateModal(b)">Genera rate</button>
                    <button v-if="canEdit && b.type === 2 && b.statusId !== 3"
                            class="btn btn-sm btn-ghost"
                            @click="recalculateConsuntivoItems(b)" title="Ricalcola voci da spese registrate">Ricalcola voci</button>
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
        <select class="form-select" v-model.number="expTypeFilter" style="width:160px" @change="loadExpenses">
          <option :value="0">Tutti i tipi</option>
          <option :value="1">Manutenzione</option>
          <option :value="2">Pulizie</option>
          <option :value="3">Sicurezza</option>
          <option :value="4">Utenze</option>
          <option :value="5">Professionale</option>
          <option :value="6">Altro</option>
        </select>
        <select class="form-select" v-model="expFilter" style="width:140px" @change="loadExpenses">
          <option value="">Tutte</option>
          <option value="unpaid">Non pagate</option>
        </select>
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
                <td><span class="badge" :class="payBadge(e.paymentStatusId)">{{ e.paymentStatusName }}</span></td>
                <td>
                  <div class="row-actions">
                    <button v-if="canEdit && e.paymentStatusId !== 2" class="btn btn-sm btn-ghost"
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
         MODAL — Approvazione Budget + Rate
    ══════════════════════════════════════════════════ -->
    <div class="modal-overlay" v-if="showApproveModal" @click.self="showApproveModal = false">
      <div class="modal">
        <div class="modal-header">
          <h2>Approva budget</h2>
          <button class="btn-icon" @click="showApproveModal = false">✕</button>
        </div>
        <div class="modal-body">
          <p class="approve-info">
            Approvando il budget verranno generate automaticamente le rate e le relative quote per ogni unità
            in base alla tabella millesimale attiva.
          </p>
          <div class="form-grid">
            <div class="form-group">
              <label class="form-label">Numero di rate *</label>
              <input class="form-input" type="number" min="1" max="24"
                     v-model.number="approveForm.numberOfInstallments" />
            </div>
            <div class="form-group">
              <label class="form-label">Prima scadenza *</label>
              <input class="form-input" type="date" v-model="approveForm.firstDueDate" />
            </div>
          </div>
          <p class="approve-hint">
            Le scadenze successive saranno distanziate di un mese l'una dall'altra.
          </p>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showApproveModal = false">Annulla</button>
          <button class="btn btn-primary" @click="confirmApprove" :disabled="savingApprove">
            <span v-if="savingApprove" class="spinner" style="width:14px;height:14px"></span>
            Approva e genera rate
          </button>
        </div>
      </div>
    </div>

    <!-- ══════════════════════════════════════════════════
         MODAL — Genera rate da budget
    ══════════════════════════════════════════════════ -->
    <div class="modal-overlay" v-if="showGenerateModal" @click.self="showGenerateModal = false">
      <div class="modal">
        <div class="modal-header">
          <h2>Genera rate</h2>
          <button class="btn-icon" @click="showGenerateModal = false">✕</button>
        </div>
        <div class="modal-body">
          <div v-if="generateHasExisting" class="generate-warning">
            <span class="generate-warning-icon">⚠</span>
            <div>
              <strong>Attenzione: rate già generate</strong><br>
              Le rate esistenti per questo budget verranno eliminate e rigenerate.
              Questa operazione non può essere annullata.
            </div>
          </div>
          <p v-else class="approve-info">
            Verranno generate le rate e le relative quote per ogni unità
            in base alla tabella millesimale attiva.
          </p>
          <div class="form-grid">
            <div class="form-group">
              <label class="form-label">Numero di rate *</label>
              <input class="form-input" type="number" min="1" max="24"
                     v-model.number="generateForm.numberOfInstallments" />
            </div>
            <div class="form-group">
              <label class="form-label">Prima scadenza *</label>
              <input class="form-input" type="date" v-model="generateForm.firstDueDate" />
            </div>
          </div>
          <p class="approve-hint">
            Le scadenze successive saranno distanziate di un mese l'una dall'altra.
          </p>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showGenerateModal = false">Annulla</button>
          <button class="btn btn-primary" :class="{ 'btn-danger': generateHasExisting }"
                  @click="confirmGenerate" :disabled="savingGenerate">
            <span v-if="savingGenerate" class="spinner" style="width:14px;height:14px"></span>
            {{ generateHasExisting ? 'Rigenera rate' : 'Genera rate' }}
          </button>
        </div>
      </div>
    </div>

    <!-- ══════════════════════════════════════════════════
         MODAL — Crea / Modifica Budget
    ══════════════════════════════════════════════════ -->
    <BaseModal
      :show="showBudgetModal"
      @close="showBudgetModal = false"
      :title="editingBudget ? 'Modifica budget' : 'Nuovo budget'"
      :subtitle="store.selectedCondominio?.name"
    >
      <div class="form-grid">
        <div class="form-group" v-if="!editingBudget">
          <label class="form-label">Tipo *</label>
          <select class="form-select" v-model.number="budgetForm.type">
            <option :value="1" :disabled="existingTypes.has(1)">
              Preventivo{{ existingTypes.has(1) ? ' (già presente)' : '' }}
            </option>
            <option :value="2" :disabled="existingTypes.has(2)">
              Consuntivo{{ existingTypes.has(2) ? ' (già presente)' : '' }}
            </option>
          </select>
        </div>
      </div>

      <!-- ── Sezione base da consuntivo (solo Preventivo in creazione) ── -->
      <template v-if="!editingBudget && budgetForm.type === 1">
        <fieldset class="form-fieldset" style="margin-top:12px">
          <legend class="form-fieldset-legend">Importa da consuntivo precedente (opzionale)</legend>
          <div class="form-grid">
            <div class="form-group form-group--full">
              <label class="form-label">Budget consuntivo di base</label>
              <select class="form-select" v-model.number="budgetForm.sourceConsuntivoId">
                <option :value="null">— Nessuno (voci vuote) —</option>
                <option v-for="c in availableConsuntivi" :key="c.id" :value="c.id">
                  {{ c.fiscalYearCode || c.fiscalYearId }} · {{ c.statusName }}
                  {{ c.totalExpenses != null ? ' · €' + c.totalExpenses.toLocaleString('it-IT', {minimumFractionDigits:2}) : '' }}
                </option>
              </select>
            </div>
            <div class="form-group" v-if="budgetForm.sourceConsuntivoId">
              <label class="form-label">Maggiorazione %</label>
              <input class="form-input" type="number" step="0.1" min="-100" max="999"
                     v-model.number="budgetForm.increasePercentage"
                     placeholder="es. 5 = +5%" />
            </div>
          </div>
        </fieldset>
      </template>

      <div class="form-group" style="margin-top:12px">
        <label class="form-label">Note</label>
        <textarea class="form-textarea" v-model="budgetForm.notes" rows="2"></textarea>
      </div>
      <template #footer>
        <button class="btn btn-ghost" @click="showBudgetModal = false">Annulla</button>
        <button class="btn btn-primary" @click="saveBudget" :disabled="savingBudget">
          <span v-if="savingBudget" class="spinner" style="width:14px;height:14px"></span>
          {{ editingBudget ? 'Salva' : 'Crea' }}
        </button>
      </template>
    </BaseModal>

    <!-- ══════════════════════════════════════════════════
         MODAL — Voci di Budget (Piano dei conti completo)
    ══════════════════════════════════════════════════ -->
    <BaseModal :show="showItemsModal" @close="closeItemsModal" size="lg">
      <template #title>Voci di budget</template>
      <template #subtitle>
        <span v-if="store.selectedCondominio" class="modal-condominium">{{ store.selectedCondominio.name }}</span>
        <span v-if="store.selectedCondominio && selectedBudget"> · </span>
        <span v-if="selectedBudget">{{ selectedBudget.type === 1 ? 'Preventivo' : 'Consuntivo' }}</span>
        <span v-if="selectedBudget?.fiscalYearCode"> · {{ selectedBudget.fiscalYearCode }}</span>
        <span v-if="selectedBudget" class="badge ml-2" :class="statusBadge(selectedBudget.statusId)">
          {{ statusLabel(selectedBudget.statusId) }}
        </span>
      </template>

      <div v-if="loadingItems" class="loading-state"><div class="spinner"></div></div>
      <div v-else-if="budgetTabs.length">

        <!-- ── Riepilogo totali (fisso in cima) ── -->
        <div class="budget-summary-top">
          <div class="summary-pill summary-pill-green">
            <span class="summary-pill-label">Entrate + Patrim.</span>
            <span class="summary-pill-value mono">{{ fmt(totalIncome) }}</span>
          </div>
          <div class="summary-pill summary-pill-red">
            <span class="summary-pill-label">Uscite</span>
            <span class="summary-pill-value mono">{{ fmt(totalExpenses) }}</span>
          </div>
          <div class="summary-pill" :class="totalBalance >= 0 ? 'summary-pill-green' : 'summary-pill-red'">
            <span class="summary-pill-label">Saldo</span>
            <span class="summary-pill-value mono">{{ fmt(totalBalance) }}</span>
          </div>
          <!-- Salva bar inline -->
          <div v-if="hasDirtyRows && canEdit && selectedBudget?.statusId === 1" class="summary-save-actions">
            <button class="btn btn-ghost btn-sm" @click="discardChanges">Annulla</button>
            <button class="btn btn-primary btn-sm" @click="saveAllItems" :disabled="savingItem">
              <span v-if="savingItem" class="spinner" style="width:12px;height:12px"></span>
              Salva
            </button>
          </div>
        </div>

        <!-- ── Tab: Entrate / Uscite / Patrimoniale ── -->
        <div class="budget-tabs">
          <button
            v-for="tab in budgetTabs" :key="tab.id"
            class="budget-tab-btn"
            :class="{ active: activeItemTab === tab.id }"
            @click="activeItemTab = tab.id"
          >
            <span class="tab-label">{{ tab.label }}</span>
            <span class="tab-total mono" :class="tab.id === 'Uscita' ? 'text-red' : 'text-green'">
              {{ fmt(tab.total) }}
            </span>
          </button>
        </div>

        <!-- ── Tabella righe del tab attivo (gerarchica) ── -->
        <div class="budget-tab-content">
          <table class="budget-accounts-table" v-if="activeTabRows.length">
            <thead>
              <tr>
                <th style="width:90px">Codice</th>
                <th>Voce</th>
                <th style="width:160px" class="text-right">Importo (€)</th>
                <th v-if="canEdit && selectedBudget?.statusId === 1" style="width:36px"></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in activeTabRows" :key="row.accountId"
                  :class="{
                    'row-edited':  row.dirty,
                    'row-header':  row.isHeader,
                    'row-leaf':    !row.isHeader,
                  }">
                <td class="mono text-secondary" style="font-size:0.82em">{{ row.accountCode }}</td>
                <td :style="{ paddingLeft: (0.65 + (row.level - 1) * 1.25) + 'rem' }">
                  {{ row.accountName }}
                </td>
                <td class="text-right">
                  <input
                    v-if="!row.isHeader && canEdit && selectedBudget?.statusId === 1"
                    class="form-input budget-amount-input"
                    type="number" step="0.01" min="0"
                    :value="row.amount"
                    @change="onAmountChange(row, $event)"
                    @focus="$event.target.select()"
                  />
                  <span v-else class="mono" :class="row.isHeader ? 'row-header-amount' : ''">
                    {{ fmt(row.amount) }}
                  </span>
                </td>
                <td v-if="canEdit && selectedBudget?.statusId === 1">
                  <button
                    v-if="!row.isHeader && row.itemId"
                    class="btn-icon btn-delete-row"
                    title="Rimuovi voce"
                    @click="deleteItem(row)"
                  >✕</button>
                </td>
              </tr>
            </tbody>
            <tfoot>
              <tr class="section-total-row">
                <td colspan="2" class="text-secondary">Totale</td>
                <td class="mono text-right">{{ fmt(activeTabRows.filter(r => !r.isHeader).reduce((s, r) => s + (r.amount || 0), 0)) }}</td>
              </tr>
            </tfoot>
          </table>
          <div v-else class="empty-state" style="padding:1rem">
            <div class="empty-icon">◎</div>
            <div>Nessuna voce in questa sezione</div>
          </div>
        </div>
      </div>
      <div v-else class="empty-state" style="padding:2rem">
        <div class="empty-icon">◎</div>
        <div>Nessuna voce di budget</div>
      </div>

      <template #footer>
        <button class="btn btn-ghost" @click="closeItemsModal">Chiudi</button>
      </template>
    </BaseModal>

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
            <optgroup v-for="grp in expenseAccountGroups" :key="grp.typeLabel" :label="grp.typeLabel">
              <option v-for="r in grp.rows" :key="r.accountId" :value="r.accountId">{{ r.label }}</option>
            </optgroup>
          </select>
          <span v-if="expErrors.accountId" class="field-error">{{ expErrors.accountId }}</span>
        </div>
        <div class="form-group" :class="{ 'has-error': expErrors.millesimalTableId }">
          <label class="form-label">Tabella millesimale *</label>
          <select class="form-select" v-model.number="expForm.millesimalTableId" @change="clearExpError('millesimalTableId')">
            <option :value="null" disabled>Seleziona tabella…</option>
            <option v-for="t in enabledMillesimalTables" :key="t.id" :value="t.id">
              {{ t.code }}{{ t.name ? ' – ' + t.name : '' }}{{ !t.isEnabled ? ' (disabilitata)' : '' }}
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
      <div class="form-group" style="margin-top:0.5rem">
        <label class="form-label">A carico di</label>
        <div class="radio-group">
          <label class="radio-label">
            <input type="radio" v-model.number="expForm.chargeabilityTypeId" :value="0" />
            Proprietario
          </label>
          <label class="radio-label">
            <input type="radio" v-model.number="expForm.chargeabilityTypeId" :value="1" />
            Inquilino (se presente)
          </label>
          <label class="radio-label">
            <input type="radio" v-model.number="expForm.chargeabilityTypeId" :value="2" />
            Automatico
          </label>
        </div>
      </div>
      <template #footer>
        <button class="btn btn-ghost" @click="showExpenseModal = false">Annulla</button>
        <button class="btn btn-primary" @click="saveExpense" :disabled="savingExp">
          <span v-if="savingExp" class="spinner" style="width:14px;height:14px"></span>
          {{ editingExp ? 'Salva' : 'Crea' }}
        </button>
      </template>
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAppStore } from '@/stores/app'
import { useSessionStore } from '@/stores/sessionStore'
import { budgetApi, budgetItemApi, chartOfAccountsApi, expenseApi, supplierApi, millesimalTableApi, installmentApi } from '@/services/api'
import { usePermissions } from '@/composables/usePermissions'
import BaseModal from '@/components/BaseModal.vue'

const router  = useRouter()
const store   = useAppStore()
const session = useSessionStore()
const { canCreate, canEdit, canDelete } = usePermissions()

function goToRate(b) {
  router.push({ path: '/rate', query: { budgetId: b.id } })
}

const activeTab = ref('budget')

// ─── Fiscal Years — dallo store globale ───────────────────────
const fiscalYears          = computed(() => store.fiscalYears)
const selectedFiscalYearId = computed({
  get: () => store.selectedFiscalYearId,
  set: (v) => { store.selectedFiscalYearId = v },
})

// ─── Budget ───────────────────────────────────────────────────
const budgets        = ref([])
const loadingBudget  = ref(false)
const showBudgetModal     = ref(false)
const editingBudget       = ref(null)
const savingBudget        = ref(false)
const budgetForm          = ref({ type: 1, notes: '', sourceConsuntivoId: null, increasePercentage: 0 })
const availableConsuntivi = ref([])

async function loadBudgets() {
  if (!selectedFiscalYearId.value) { budgets.value = []; return }
  loadingBudget.value = true
  try {
    const { data } = await budgetApi.getByFiscalYear(selectedFiscalYearId.value)
    budgets.value = data ?? []
  } catch { budgets.value = [] } finally { loadingBudget.value = false }
}

async function openBudgetModal(b = null) {
  editingBudget.value = b?.id ?? null
  if (b) {
    budgetForm.value = { notes: b.notes ?? '', sourceConsuntivoId: null, increasePercentage: 0 }
  } else {
    const defaultType = !existingTypes.value.has(1) ? 1 : 2
    budgetForm.value = { type: defaultType, notes: '', sourceConsuntivoId: null, increasePercentage: 0 }
    // Carica i consuntivi disponibili come base (tutti gli esercizi del condominio)
    try {
      const { data } = await budgetApi.getByCondominium(store.selectedCondominioId)
      availableConsuntivi.value = (data ?? []).filter(b => b.type === 2)
    } catch { availableConsuntivi.value = [] }
  }
  showBudgetModal.value = true
}

async function saveBudget() {
  savingBudget.value = true
  try {
    if (editingBudget.value) {
      await budgetApi.update(editingBudget.value, { notes: budgetForm.value.notes })
    } else {
      await budgetApi.create({
        condominiumId:       store.selectedCondominioId,
        fiscalYearId:        selectedFiscalYearId.value,
        type:                budgetForm.value.type,
        notes:               budgetForm.value.notes,
        sourceConsuntivoId:  budgetForm.value.type === 1 ? (budgetForm.value.sourceConsuntivoId || null) : null,
        increasePercentage:  budgetForm.value.type === 1 ? (budgetForm.value.increasePercentage || 0) : 0,
      })
    }
    showBudgetModal.value = false
    await loadBudgets()
  } catch { /* toast handled globally */ } finally { savingBudget.value = false }
}

// Tipi già approvati o chiusi (non approvabili di nuovo)
const blockedTypes = computed(() => {
  const s = new Set()
  for (const b of budgets.value) {
    if (b.statusId === 2 || b.statusId === 3) s.add(b.type)
  }
  return s
})

// Tipi già esistenti in qualsiasi stato (non creabili di nuovo)
const existingTypes = computed(() => {
  const s = new Set()
  for (const b of budgets.value) s.add(b.type)
  return s
})

// "Nuovo budget" ha senso solo se manca almeno un tipo
const canCreateBudget = computed(() =>
  !!selectedFiscalYearId.value && existingTypes.value.size < 2
)

// ─── Approvazione ─────────────────────────────────────────────
const showApproveModal = ref(false)
const approvingBudget  = ref(null)
const savingApprove    = ref(false)
const approveForm      = ref({ numberOfInstallments: 4, firstDueDate: '' })

function openApproveModal(b) {
  approvingBudget.value = b
  const next = new Date()
  next.setDate(1)
  next.setMonth(next.getMonth() + 1)
  approveForm.value = { numberOfInstallments: 4, firstDueDate: next.toISOString().slice(0, 10) }
  showApproveModal.value = true
}

async function confirmApprove() {
  if (!approvingBudget.value) return
  savingApprove.value = true
  try {
    await budgetApi.approve(approvingBudget.value.id, {
      numberOfInstallments: approveForm.value.numberOfInstallments,
      firstDueDate: approveForm.value.firstDueDate,
    })
    showApproveModal.value = false
    approvingBudget.value  = null
    await loadBudgets()
  } catch { /* global */ } finally { savingApprove.value = false }
}

async function closeBudget(b) {
  const tipo = b.type === 1 ? 'preventivo' : 'consuntivo'
  if (!confirm(`Chiudere il budget ${tipo}?\n\nDopo la chiusura sarà possibile approvare un nuovo budget dello stesso tipo.`)) return
  try { await budgetApi.close(b.id); await loadBudgets() } catch {}
}

// ─── Genera rate ──────────────────────────────────────────────
const showGenerateModal    = ref(false)
const generatingBudget     = ref(null)
const savingGenerate       = ref(false)
const generateHasExisting  = ref(false)
const generateForm         = ref({ numberOfInstallments: 4, firstDueDate: '' })

async function openGenerateModal(b) {
  generatingBudget.value   = b
  generateHasExisting.value = false
  const next = new Date()
  next.setDate(1)
  next.setMonth(next.getMonth() + 1)
  generateForm.value = { numberOfInstallments: 4, firstDueDate: next.toISOString().slice(0, 10) }
  try {
    const { data } = await installmentApi.getByBudget(b.id)
    generateHasExisting.value = Array.isArray(data) && data.length > 0
  } catch { /* ignore */ }
  showGenerateModal.value = true
}

async function confirmGenerate() {
  if (!generatingBudget.value) return
  savingGenerate.value = true
  try {
    await budgetApi.generateInstallments(generatingBudget.value.id, {
      numberOfInstallments: generateForm.value.numberOfInstallments,
      firstDueDate: generateForm.value.firstDueDate,
      force: generateHasExisting.value,
    })
    store.toast('Rate generate con successo', 'success')
    showGenerateModal.value = false
    generatingBudget.value  = null
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { savingGenerate.value = false }
}

async function deleteBudget(id) {
  if (!confirm('Eliminare il budget?')) return
  try { await budgetApi.delete(id); await loadBudgets() } catch {}
}

async function reopenBudget(b) {
  const tipo = b.type === 1 ? 'preventivo' : 'consuntivo'
  if (!confirm(`Riaprire il budget ${tipo}?\n\nVerrà riportato in stato Bozza e sarà nuovamente modificabile.`)) return
  try {
    await budgetApi.reopen(b.id)
    store.toast('Budget riaperto', 'success')
    await loadBudgets()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

async function recalculateConsuntivoItems(b) {
  if (!confirm('Ricalcolare le voci del consuntivo?\n\n⚠️ ATTENZIONE: tutte le modifiche manuali alle voci verranno annullate e sostituite con i valori calcolati automaticamente dalle spese registrate e dai movimenti di entrata.\n\nContinuare?')) return
  try {
    await budgetApi.recalculateItems(b.id)
    store.toast('Voci ricalcolate', 'success')
    await loadBudgets()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

// ─── Budget Items — Piano dei conti gerarchico ───────────────
const showItemsModal = ref(false)
const selectedBudget = ref(null)
const loadingItems   = ref(false)
const savingItem     = ref(false)

// 3 tab fissi: Uscite / Entrate / Patrimoniale
const budgetTabs    = ref([])   // { id, label, rows, total }
const activeItemTab = ref(null)

// Righe del tab attivo (lista piatta ordinata gerarchicamente)
const activeTabRows = computed(() => {
  const tab = budgetTabs.value.find(t => t.id === activeItemTab.value)
  return tab?.rows ?? []
})

// Solo righe foglia (editabili) su tutti i tab
const allLeafRows = computed(() => budgetTabs.value.flatMap(t => t.rows.filter(r => !r.isHeader)))

const hasDirtyRows = computed(() => allLeafRows.value.some(r => r.dirty))

const totalIncome   = computed(() =>
  (budgetTabs.value.find(t => t.id === 'Entrata')?.total ?? 0) +
  (budgetTabs.value.find(t => t.id === 'Patrimoniale')?.total ?? 0)
)
const totalExpenses = computed(() =>
  budgetTabs.value.find(t => t.id === 'Uscita')?.total ?? 0
)
const totalBalance = computed(() => totalIncome.value - totalExpenses.value)

// snapshot per "discard"
let originalAmounts = {}

// ChartOfAccounts (per dropdown spese)
const chartOfAccounts      = ref([])
const accountGroups        = ref([])
const expenseAccountGroups = ref([])
let accountsLoaded = false

async function openItemsModal(budget) {
  selectedBudget.value = budget
  showItemsModal.value = true
  budgetTabs.value     = []
  activeItemTab.value  = null
  await loadBudgetItems()
}

function closeItemsModal() {
  showItemsModal.value = false
  selectedBudget.value = null
  budgetTabs.value     = []
  activeItemTab.value  = null
}

// Costruisce una lista piatta ordinata gerarchicamente a partire dai nodi radice del tipo dato.
// Ogni elemento ha: accountId, accountCode, accountName, accountType, level, isHeader,
//                   itemId, amount, dirty
function buildHierarchyRows(items, typeLabel) {
  const byParent  = {}
  for (const i of items) {
    const pid = i.parentAccountId ?? null
    if (!byParent[pid]) byParent[pid] = []
    byParent[pid].push(i)
  }
  for (const list of Object.values(byParent))
    list.sort((a, b) => (a.accountCode ?? '').localeCompare(b.accountCode ?? ''))

  const result = []
  function walk(parentId, level) {
    for (const i of (byParent[parentId] ?? [])) {
      const hasChildren = !!(byParent[i.accountId]?.length)
      result.push({
        accountId:   i.accountId,
        accountCode: i.accountCode,
        accountName: i.accountName,
        accountType: typeLabel,
        level,
        isHeader:    hasChildren,   // nodi con figli = sola lettura, importo calcolato
        itemId:      hasChildren ? null : (i.id ?? null),
        amount:      i.amount ?? 0,
        dirty:       false,
      })
      walk(i.accountId, level + 1)
    }
  }
  // Parti dai nodi senza padre
  walk(null, 1)
  // Se non ci sono nodi con parentAccountId=null, usa tutti come radici piatte
  if (result.length === 0) {
    for (const i of items.sort((a, b) => (a.accountCode ?? '').localeCompare(b.accountCode ?? ''))) {
      result.push({
        accountId: i.accountId, accountCode: i.accountCode,
        accountName: i.accountName, accountType: typeLabel,
        level: 1, isHeader: false,
        itemId: i.id ?? null, amount: i.amount ?? 0, dirty: false,
      })
    }
  }
  return result
}

// Ricalcola l'importo di ogni nodo-header come somma dei figli foglia discendenti
function recalcHeaders(rows) {
  // Calcola per ogni accountId la somma delle foglie discendenti
  const amountById = {}
  // Prima passa: somma solo le foglie
  for (const r of rows) if (!r.isHeader) amountById[r.accountId] = r.amount
  // Seconda passa bottom-up: propaga ai parent
  // Usiamo l'ordine inverso della lista (i figli vengono dopo i padri nella DFS)
  for (let i = rows.length - 1; i >= 0; i--) {
    const r = rows[i]
    if (!r.isHeader) continue
    // Somma tutti i discendenti foglia che vengono dopo nella lista
    // fino al prossimo nodo dello stesso livello o superiore
    let total = 0
    for (let j = i + 1; j < rows.length; j++) {
      if (rows[j].level <= r.level) break
      if (!rows[j].isHeader) total += rows[j].amount
    }
    r.amount = total
  }
}

async function loadBudgetItems() {
  if (!selectedBudget.value) return
  loadingItems.value = true
  try {
    const { data } = await budgetItemApi.getByBudget(selectedBudget.value.id)
    const items = data ?? []

    // Mappa type numerico → label stringa
    const TYPE_LABEL = { 1: 'Entrata', 2: 'Uscita', 3: 'Patrimoniale' }

    budgetTabs.value = [
      { id: 'Uscita',       label: 'Uscite' },
      { id: 'Entrata',      label: 'Entrate' },
      { id: 'Patrimoniale', label: 'Patrimoniale' },
    ].map(def => {
      const typeItems = items.filter(i => (TYPE_LABEL[i.accountType] ?? i.accountType) === def.id)
      const rows = buildHierarchyRows(typeItems, def.id)
      recalcHeaders(rows)
      const total = rows.filter(r => !r.isHeader).reduce((s, r) => s + (r.amount || 0), 0)
      return { ...def, rows, total }
    })

    activeItemTab.value = budgetTabs.value[0].id

    // Snapshot per discard (solo foglie)
    originalAmounts = {}
    for (const row of allLeafRows.value) {
      originalAmounts[row.accountId] = row.amount
    }

  } catch {
    budgetTabs.value = []
  } finally {
    loadingItems.value = false
  }
}

function onAmountChange(row, event) {
  const val = parseFloat(event.target.value) || 0
  row.amount = val
  row.dirty  = val !== (originalAmounts[row.accountId] ?? 0)
  const tab = budgetTabs.value.find(t => t.id === activeItemTab.value)
  if (tab) {
    recalcHeaders(tab.rows)
    tab.total = tab.rows.filter(r => !r.isHeader).reduce((s, r) => s + (r.amount || 0), 0)
  }
}

function discardChanges() {
  for (const row of allLeafRows.value) {
    row.amount = originalAmounts[row.accountId] ?? 0
    row.dirty  = false
  }
  for (const tab of budgetTabs.value) {
    recalcHeaders(tab.rows)
    tab.total = tab.rows.filter(r => !r.isHeader).reduce((s, r) => s + (r.amount || 0), 0)
  }
}

async function saveAllItems() {
  if (!selectedBudget.value) return
  savingItem.value = true
  try {
    const dirtyRows = allLeafRows.value.filter(r => r.dirty)
    for (const row of dirtyRows) {
      if (row.itemId) {
        await budgetItemApi.update(row.itemId, { accountId: row.accountId, amount: row.amount })
      } else if (row.amount > 0) {
        const { data } = await budgetItemApi.create({
          budgetId:  selectedBudget.value.id,
          accountId: row.accountId,
          amount:    row.amount,
        })
        row.itemId = data?.id ?? null
      }
      row.dirty = false
      originalAmounts[row.accountId] = row.amount
    }
    await loadBudgets()
  } catch { /* global */ } finally { savingItem.value = false }
}

async function deleteItem(row) {
  if (!row.itemId) return
  if (!confirm(`Rimuovere la voce "${row.accountName}"?`)) return
  try {
    await budgetItemApi.delete(row.itemId)
    row.itemId = null
    row.amount = 0
    row.dirty  = false
    delete originalAmounts[row.accountId]
    const tab = budgetTabs.value.find(t => t.rows.includes(row))
    if (tab) {
      recalcHeaders(tab.rows)
      tab.total = tab.rows.filter(r => !r.isHeader).reduce((s, r) => s + (r.amount || 0), 0)
    }
    await loadBudgets()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

// ─── Spese ────────────────────────────────────────────────────
const expenses         = ref([])
const loadingExp       = ref(false)
const showExpenseModal = ref(false)
const editingExp       = ref(null)
const savingExp        = ref(false)
const expErrors        = ref({})
const expFilter        = ref('')
const expTypeFilter    = ref(0)

function clearExpError(field) { delete expErrors.value[field] }

function validateExpForm() {
  const e = {}
  const f = expForm.value
  if (!f.name?.trim())          e.name             = 'Campo obbligatorio'
  if (!f.expenseTypeId)         e.expenseTypeId    = 'Selezionare un tipo spesa'
  if (!f.documentDate)          e.documentDate     = 'Campo obbligatorio'
  if (!f.registrationDate)      e.registrationDate = 'Campo obbligatorio'
  if (!f.grossAmount || f.grossAmount <= 0) e.grossAmount = 'Inserire un importo maggiore di zero'
  if (!f.accountId)             e.accountId        = 'Selezionare un conto'
  if (!f.millesimalTableId)     e.millesimalTableId = 'Selezionare una tabella millesimale'
  expErrors.value = e
  return Object.keys(e).length === 0
}

const today   = new Date().toISOString().slice(0, 10)
const suppliers        = ref([])
const millesimalTables = ref([])
const enabledMillesimalTables = computed(() => {
  const currentId = editingExp.value ? expForm.value.millesimalTableId : null
  return millesimalTables.value.filter(t => t.isEnabled || t.id === currentId)
})

const emptyExpForm = () => ({
  name: '', documentNumber: '', documentDate: today, registrationDate: today,
  grossAmount: 0, vatAmount: 0, expenseTypeId: 0, paymentStatusId: 1,
  paymentMethod: '', supplierId: null, accountId: null, millesimalTableId: null, description: '',
  chargeabilityTypeId: 0,
})
const expForm = ref(emptyExpForm())

async function loadExpenses() {
  if (!store.selectedCondominioId) return
  loadingExp.value = true
  try {
    let res
    if (expFilter.value === 'unpaid') {
      res = await expenseApi.getUnpaid(store.selectedCondominioId)
    } else if (expTypeFilter.value) {
      res = await expenseApi.getByType(store.selectedCondominioId, expTypeFilter.value)
    } else {
      res = await expenseApi.getByCondominium(store.selectedCondominioId)
    }
    expenses.value = res.data
  } catch { expenses.value = [] } finally { loadingExp.value = false }
}

async function openExpenseModal(e = null) {
  expErrors.value  = {}
  editingExp.value = e?.id ?? null
  expForm.value = e ? {
    name:              e.name ?? '',
    documentNumber:    e.documentNumber ?? '',
    documentDate:      e.documentDate?.slice(0, 10) ?? today,
    registrationDate:  e.registrationDate?.slice(0, 10) ?? today,
    grossAmount:       e.grossAmount ?? 0,
    vatAmount:         e.vatAmount ?? 0,
    expenseTypeId:     e.expenseTypeId ?? 0,
    paymentStatusId:   e.paymentStatusId ?? 1,
    paymentMethod:     e.paymentMethod ?? '',
    supplierId:        e.supplierId ?? null,
    accountId:         e.accountId ?? null,
    millesimalTableId:  e.millesimalTableId ?? null,
    description:        e.description ?? '',
    chargeabilityTypeId:  e.chargeabilityTypeId ?? 0,
  } : emptyExpForm()

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

function buildExpenseAccountGroups() {
  // type dal backend è int: Entrata=1, Uscita=2, Patrimoniale=3
  const TYPE_MAP    = { 1: 'Entrata', 2: 'Uscita', 3: 'Patrimoniale' }
  const TYPE_ORDER  = ['Uscita', 'Entrata', 'Patrimoniale']
  const TYPE_LABELS = { Uscita: 'Uscite', Entrata: 'Entrate', Patrimoniale: 'Patrimoniale' }

  const accountById = Object.fromEntries(chartOfAccounts.value.map(a => [a.id, a]))

  const groups = {}
  // Solo conti di livello 2+ (esclude i capitoli di primo livello)
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
      name:              expForm.value.name,
      documentNumber:    expForm.value.documentNumber || null,
      documentDate:      expForm.value.documentDate,
      registrationDate:  expForm.value.registrationDate,
      grossAmount:       expForm.value.grossAmount,
      vatAmount:         expForm.value.vatAmount,
      netAmount:         expForm.value.grossAmount - expForm.value.vatAmount,
      expenseTypeId:     expForm.value.expenseTypeId,
      paymentStatusId:   expForm.value.paymentStatusId || 1,
      paymentMethod:     expForm.value.paymentMethod || null,
      description:       expForm.value.description || null,
      condominiumId:     store.selectedCondominioId,
      fiscalYearId:      selectedFiscalYearId.value       ?? null,
      accountId:          expForm.value.accountId         ?? null,
      millesimalTableId:  expForm.value.millesimalTableId ?? null,
      supplierId:         expForm.value.supplierId        ?? null,
      chargeabilityTypeId:  expForm.value.chargeabilityTypeId ?? 0,
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
  try { await expenseApi.markAsPaid(id, today, 'BankTransfer'); await loadExpenses() } catch {}
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
const payBadge     = (id) => ({ 1: 'badge-amber', 2: 'badge-green', 3: 'badge-red' }[id] ?? 'badge-muted')

const TYPE_CLASS = { Uscita: 'type-uscita', Entrata: 'type-entrata', Patrimoniale: 'type-patrimoniale' }
const typeClass  = (typeId) => ['type-badge', TYPE_CLASS[typeId] ?? ''].join(' ')

// Auto-select DefaultMillesimalTable when account changes (expense form)
watch(() => expForm.value.accountId, (accountId) => {
  if (!accountId) return
  const account = chartOfAccounts.value.find(a => a.id === accountId)
  if (account?.defaultMillesimalTableId) {
    expForm.value.millesimalTableId = account.defaultMillesimalTableId
  }
})

// ─── Watchers / Init ──────────────────────────────────────────
watch(() => store.selectedCondominioId, async () => {
  accountsLoaded = false
  expenseAccountGroups.value = []
  budgetTabs.value = []
  expTypeFilter.value = 0
  expFilter.value = ''
  // loadFiscalYears è già gestito dal watch in appStore
  await loadExpenses()
})
watch(() => store.selectedFiscalYearId, loadBudgets)

onMounted(async () => {
  if (!store.fiscalYears.length) await store.loadFiscalYears()
  await loadBudgets()
  await loadExpenses()
})
</script>

<style scoped>
.tab-toolbar { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1rem; flex-wrap: wrap; }
.btn-reopen  { color: var(--accent-amber, #f59e0b) !important; border-color: rgba(245,158,11,0.3) !important; }
.btn-active  { background: var(--accent-glow) !important; color: var(--accent) !important; border-color: var(--border-active) !important; }
.row-actions { display: flex; gap: 0.4rem; justify-content: flex-end; }
.text-right  { text-align: right; }
.text-green  { color: var(--accent-green, #22c55e); }
.text-red    { color: var(--accent-red,   #ef4444); }
.ml-2        { margin-left: 0.5rem; }

/* ── Riepilogo totali in cima al modal ── */
.budget-summary-top {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
  margin-bottom: 1rem;
  padding: 0.6rem 0.75rem;
  background: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: 8px;
}
.summary-pill {
  display: flex;
  align-items: center;
  gap: 0.35rem;
  padding: 0.3rem 0.65rem;
  border-radius: 20px;
  background: var(--bg-base, #1a1a2e);
  border: 1px solid var(--border);
  font-size: 0.82rem;
}
.summary-pill-green { border-color: rgba(34,197,94,0.4);  }
.summary-pill-red   { border-color: rgba(239,68,68,0.4); }
.summary-pill-label { color: var(--text-muted); }
.summary-pill-value { font-weight: 700; }
.summary-save-actions { margin-left: auto; display: flex; gap: 0.4rem; }

/* ── Tab capitoli ── */
.budget-tabs {
  display: flex;
  flex-wrap: wrap;
  gap: 0.25rem;
  margin-bottom: 0;
  border-bottom: 2px solid var(--border);
  padding-bottom: 0;
}
.budget-tab-btn {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  padding: 0.4rem 0.75rem;
  border: 1px solid transparent;
  border-bottom: none;
  border-radius: 6px 6px 0 0;
  background: transparent;
  cursor: pointer;
  font-size: 0.82rem;
  color: var(--text-secondary);
  transition: background 0.15s;
  position: relative;
  bottom: -2px;
}
.budget-tab-btn:hover { background: var(--bg-surface); }
.budget-tab-btn.active {
  background: var(--bg-surface);
  border-color: var(--border);
  border-bottom-color: var(--bg-surface);
  color: var(--text);
  font-weight: 600;
}
.tab-label  { line-height: 1.2; }
.tab-total  { font-size: 0.78rem; font-weight: 700; line-height: 1.2; }

/* ── Contenuto tab ── */
.budget-tab-content {
  border: 1px solid var(--border);
  border-top: none;
  border-radius: 0 0 8px 8px;
  overflow: hidden;
}

/* ── Tabella voci ── */
.budget-accounts-table { width: 100%; border-collapse: collapse; }
.budget-accounts-table th,
.budget-accounts-table td { padding: 0.45rem 0.65rem; border-bottom: 1px solid var(--border); font-size: 0.875rem; }
.budget-accounts-table thead th { background: var(--bg-surface); font-weight: 600; }
.budget-accounts-table tr.row-edited td { background: rgba(99,102,241,0.06); }
.budget-accounts-table tbody tr:last-child td { border-bottom: none; }
.budget-accounts-table tr.row-header td { background: var(--bg-surface); font-weight: 600; border-top: 1px solid var(--border); }
.budget-accounts-table tr.row-header td:first-child { color: var(--text-muted); }
.row-header-amount { font-weight: 600; color: var(--text); }

.budget-amount-input {
  width: 120px;
  padding: 0.25rem 0.4rem;
  text-align: right;
  font-family: monospace;
  font-size: 0.875rem;
}
.section-total-row td { font-weight: 600; background: var(--bg-surface); border-top: 2px solid var(--border); }
.btn-delete-row { color: var(--text-muted); font-size: 0.75rem; opacity: 0.5; transition: opacity 0.15s, color 0.15s; }
.btn-delete-row:hover { color: var(--accent-red); opacity: 1; }

/* Approve modal */
.approve-info { font-size: .875rem; color: var(--text-secondary); margin-bottom: 1rem; }
.generate-warning {
  display: flex; gap: .75rem; align-items: flex-start;
  background: rgba(239,68,68,.08); border: 1px solid rgba(239,68,68,.3);
  border-radius: 6px; padding: .75rem 1rem; margin-bottom: 1rem;
  font-size: .875rem; color: var(--accent-red);
}
.generate-warning-icon { font-size: 1.25rem; flex-shrink: 0; margin-top: .05rem; }
.btn-danger { background: var(--accent-red) !important; color: #fff !important; border-color: var(--accent-red) !important; }
.approve-hint { font-size: .78rem;  color: var(--text-muted);     margin-top: .5rem; }

/* Form validation */
.has-error .form-input,
.has-error .form-select { border-color: var(--accent-red, #e53e3e); }
.field-error { font-size: 0.78rem; color: var(--accent-red, #e53e3e); margin-top: 0.2rem; display: block; }
</style>
