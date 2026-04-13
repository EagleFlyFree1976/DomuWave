<template>
  <div>
    <div class="page-header">
      <h1>Consumi</h1>
    </div>

    <div v-if="!store.selectedCondominioId" class="card">
      <div class="empty-state">
        <div class="empty-icon">◎</div>
        <div>Seleziona un condominio per gestire i consumi</div>
      </div>
    </div>

    <template v-else>
      <!-- Tab bar -->
      <div class="tab-bar">
        <button class="tab-btn" :class="{ active: tab === 'types' }"    @click="tab = 'types'">Tipi consumo</button>
        <button class="tab-btn" :class="{ active: tab === 'meters' }"   @click="tab = 'meters'">Contatori</button>
        <button class="tab-btn" :class="{ active: tab === 'readings' }" @click="tab = 'readings'">Letture</button>
        <button class="tab-btn" :class="{ active: tab === 'charges' }"  @click="tab = 'charges'">Ripartizioni</button>
      </div>

      <!-- ── TAB: Tipi consumo ──────────────────────────────────────────── -->
      <div v-if="tab === 'types'" class="card">
        <div class="toolbar">
          <span class="text-secondary">{{ visibleTypes.length }} tipo/i</span>
          <div style="display:flex;gap:0.5rem;align-items:center">
            <button class="btn btn-ghost btn-sm btn-icon-only" @click="loadTypes" :disabled="loadingTypes" title="Aggiorna">
              <span v-if="loadingTypes" class="spinner" style="width:13px;height:13px"></span>
              <span v-else>↺</span>
            </button>
            <button class="btn btn-ghost btn-sm"
                    :class="{ 'btn-ghost--active': showDeletedTypes }"
                    @click="showDeletedTypes = !showDeletedTypes"
                    title="Mostra/nascondi tipi cancellati">
              {{ showDeletedTypes ? 'Nascondi cancellati' : 'Mostra cancellati' }}
            </button>
            <button v-if="canCreate" class="btn btn-primary btn-sm" @click="openTypeModal()">+ Nuovo tipo</button>
          </div>
        </div>
        <div v-if="loadingTypes" class="loading-state"><div class="spinner"></div></div>
        <div v-else-if="!visibleTypes.length" class="empty-state">
          <div class="empty-icon">◎</div>
          <div>Nessun tipo di consumo. Crea il primo.</div>
        </div>
        <div v-else class="table-wrap">
          <table>
            <thead><tr><th>Nome</th><th>U.M.</th><th>Conto spesa</th><th>Stato</th><th></th></tr></thead>
            <tbody>
              <tr v-for="t in visibleTypes" :key="t.id" :class="{ 'row-type-deleted': t.isDeleted }">
                <td>
                  <button class="link-btn" @click="selectType(t)" :disabled="t.isDeleted">{{ t.name }}</button>
                  <span v-if="t.isDeleted" class="badge badge-muted type-deleted-badge" title="Tipo cancellato">Cancellato</span>
                </td>
                <td class="text-secondary mono">{{ t.unitOfMeasure }}</td>
                <td class="text-secondary" style="font-size:0.85em">
                  <span v-if="t.accountId" class="mono">{{ t.accountCode }}</span>
                  <span v-if="t.accountId"> {{ t.accountName }}</span>
                  <span v-else class="text-muted">—</span>
                </td>
                <td>
                  <span v-if="!t.isDeleted" class="badge" :class="t.isActive ? 'badge-green' : 'badge-muted'">{{ t.isActive ? 'Attivo' : 'Inattivo' }}</span>
                  <span v-else class="badge badge-muted">—</span>
                </td>
                <td>
                  <div class="row-actions">
                    <button v-if="canEdit && !t.isDeleted" class="btn-icon" @click="openTypeModal(t)">✎</button>
                    <button v-if="canDelete && !t.isDeleted" class="btn-icon btn-icon--danger" @click="deleteType(t)">🗑</button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- ── TAB: Contatori ─────────────────────────────────────────────── -->
      <div v-if="tab === 'meters'" class="card">
        <div class="toolbar">
          <span class="text-secondary">{{ visibleMeters.length }} contatore/i</span>
          <div style="display:flex;gap:0.5rem;align-items:center">
            <button class="btn btn-ghost btn-sm btn-icon-only" @click="loadMeters" :disabled="loadingMeters" title="Aggiorna">
              <span v-if="loadingMeters" class="spinner" style="width:13px;height:13px"></span>
              <span v-else>↺</span>
            </button>
            <button class="btn btn-ghost btn-sm"
                    :class="{ 'btn-ghost--active': showDeletedMeters }"
                    @click="showDeletedMeters = !showDeletedMeters"
                    title="Mostra/nascondi contatori e tipi cancellati">
              {{ showDeletedMeters ? 'Nascondi cancellati' : 'Mostra cancellati' }}
            </button>
            <button v-if="canCreate" class="btn btn-primary btn-sm" @click="openMeterModal()">+ Nuovo contatore</button>
          </div>
        </div>
        <div v-if="loadingMeters" class="loading-state"><div class="spinner"></div></div>
        <div v-else-if="!visibleMeters.length" class="empty-state">
          <div class="empty-icon">◎</div>
          <div>Nessun contatore. Crea il primo.</div>
        </div>
        <div v-else class="table-wrap">
          <table>
            <thead><tr><th>Tipo</th><th>Unità</th><th>Matricola</th><th>Stato</th><th></th></tr></thead>
          <tbody>
            <tr v-for="m in visibleMeters" :key="m.id"
                :class="{ 'row-type-deleted': m.consumptionTypeIsDeleted || m.isDeleted }">
              <td>
                {{ m.consumptionTypeName }}
                <span v-if="m.consumptionTypeIsDeleted" class="badge badge-muted type-deleted-badge" title="Il tipo consumo è stato eliminato">Cancellato</span>
              </td>
              <td class="text-secondary">{{ m.unitName }}</td>
              <td class="mono">
                {{ m.code }}
                <span v-if="m.isDeleted" class="badge badge-muted type-deleted-badge" title="Contatore eliminato">Cancellato</span>
              </td>
              <td>
                <span v-if="!m.isDeleted" class="badge" :class="m.isActive ? 'badge-green' : 'badge-muted'">{{ m.isActive ? 'Attivo' : 'Inattivo' }}</span>
                <span v-else class="badge badge-muted">—</span>
              </td>
              <td>
                <div class="row-actions">
                  <button v-if="canEdit && !m.isDeleted && !m.consumptionTypeIsDeleted" class="btn-icon" @click="openMeterModal(m)">✎</button>
                  <button v-if="canDelete && !m.isDeleted" class="btn-icon btn-icon--danger" @click="deleteMeter(m.id)">🗑</button>
                </div>
              </td>
            </tr>
          </tbody>
          </table>
        </div>
      </div>

      <!-- ── TAB: Letture ───────────────────────────────────────────────── -->
      <div v-if="tab === 'readings'" class="card">
        <!-- Toolbar con selettori integrati -->
        <div class="toolbar readings-toolbar">
          <div class="readings-selectors">
            <select class="form-select readings-select" v-model="selectedTypeId" @change="onTypeOrFyChange">
              <option :value="null" disabled>— Tipo consumo —</option>
              <option v-for="t in consumptionTypes" :key="t.id" :value="t.id">{{ t.name }} ({{ t.unitOfMeasure }}){{ t.isDeleted ? ' — Cancellato' : '' }}</option>
            </select>
            <select class="form-select readings-select" v-model="selectedFiscalYearId" @change="onTypeOrFyChange">
              <option :value="null" disabled>— Esercizio —</option>
              <option v-for="fy in fiscalYears" :key="fy.id" :value="fy.id">{{ fy.code }}</option>
            </select>
          </div>
          <div style="display:flex;gap:0.5rem;align-items:center;flex-wrap:wrap">
            <button v-if="selectedTypeId && selectedFiscalYearId"
                    class="btn btn-ghost btn-sm" @click="onTypeOrFyChange" :disabled="loadingReadings"
                    title="Aggiorna letture">
              <span v-if="loadingReadings" class="spinner" style="width:14px;height:14px"></span>
              <span v-else>↺</span>
            </button>
            <button v-if="selectedTypeId && selectedFiscalYearId"
                    class="btn btn-ghost btn-sm"
                    :class="{ 'btn-ghost--active': showDeletedReadings }"
                    @click="showDeletedReadings = !showDeletedReadings"
                    title="Mostra/nascondi contatori cancellati">
              {{ showDeletedReadings ? 'Nascondi cancellati' : 'Mostra cancellati' }}
            </button>
            <button v-if="canEdit && selectedTypeId && selectedFiscalYearId && validReadingRows.length > 0"
                    class="btn btn-primary btn-sm" @click="saveReadings" :disabled="savingReadings">
              <span v-if="savingReadings" class="spinner" style="width:14px;height:14px"></span>
              Salva tutte ({{ validReadingRows.length }})
            </button>
          </div>
        </div>

        <!-- Placeholder se non selezionato -->
        <div v-if="!selectedTypeId || !selectedFiscalYearId" class="empty-state">
          <div class="empty-icon">◎</div>
          <div>Seleziona un tipo di consumo e un esercizio fiscale</div>
        </div>

        <template v-else>
          <div v-if="loadingReadings" class="loading-state"><div class="spinner"></div></div>
          <div v-else-if="!visibleGroups.length" class="empty-state">
            <div class="empty-icon">◎</div>
            <div>Nessun contatore per questo tipo di consumo. Aggiungi prima i contatori.</div>
          </div>
          <div v-else class="table-wrap">
            <table class="readings-table">
              <thead>
                <tr>
                  <th>Unità</th>
                  <th>Matricola</th>
                  <th class="col-num">Lett. iniziale</th>
                  <th class="col-date">Data inizio</th>
                  <th class="col-num">Lett. finale</th>
                  <th class="col-date">Data fine</th>
                  <th class="col-num">Consumo</th>
                  <th class="col-action"></th>
                </tr>
              </thead>
              <tbody>
                <template v-for="group in visibleGroups" :key="group.meterId">
                  <tr v-for="(row, rowIdx) in group.rows" :key="rowIdx"
                      :class="{ 'row-error': rowError(row), 'row-saved': row.saved && !row.modified, 'row-charged': !!row.chargeId, 'row-meter-deleted': group.meterIsDeleted }">
                    <!-- Unità e matricola solo nella prima riga del gruppo -->
                    <td>
                      <span v-if="rowIdx === 0">{{ group.unitName }}</span>
                    </td>
                    <td class="mono text-secondary">
                      <span v-if="rowIdx === 0">
                        {{ group.meterCode }}
                        <span v-if="group.meterIsDeleted" class="badge badge-muted meter-deleted-badge" title="Contatore eliminato">Eliminato</span>
                      </span>
                    </td>
                    <td class="col-num">
                      <span v-if="isReadonly(row)" class="inline-value mono">{{ fmtNum(row.initialValue) }}</span>
                      <input v-else class="inline-input" type="number" step="0.001" v-model.number="row.initialValue" @input="onRowInput(row)" />
                    </td>
                    <td class="col-date">
                      <span v-if="isReadonly(row)" class="inline-value text-secondary">{{ row.initialDate }}</span>
                      <input v-else class="inline-input" type="date" v-model="row.initialDate" @input="onRowInput(row)" />
                    </td>
                    <td class="col-num">
                      <span v-if="isReadonly(row)" class="inline-value mono">{{ fmtNum(row.finalValue) }}</span>
                      <input v-else class="inline-input" type="number" step="0.001" v-model.number="row.finalValue" @input="onRowInput(row)" />
                    </td>
                    <td class="col-date">
                      <span v-if="isReadonly(row)" class="inline-value text-secondary">{{ row.finalDate }}</span>
                      <input v-else class="inline-input" type="date" v-model="row.finalDate" @input="onRowInput(row)" />
                    </td>
                    <td class="col-num" :class="consumptionClass(row)">
                      <span v-if="rowError(row)" class="row-error-icon" :title="rowError(row)">⚠</span>
                      <span v-else class="mono">{{ fmtNum(row.finalValue - row.initialValue) }}</span>
                    </td>
                    <td class="col-action">
                      <span v-if="isReadonly(row)" class="badge badge-muted charged-badge" title="Inclusa in una ripartizione approvata">✓ Approvata</span>
                      <div v-else class="row-btns">
                        <button v-if="canEdit && row.modified && !rowError(row)"
                                class="btn-icon btn-icon--save"
                                :disabled="row.saving"
                                title="Salva lettura"
                                @click="saveRow(row)">
                          <span v-if="row.saving" class="spinner" style="width:12px;height:12px"></span>
                          <span v-else>✓</span>
                        </button>
                        <span v-else-if="row.saved && !row.modified && !row.chargeId" class="saved-check" title="Salvato">✓</span>
                        <span v-else-if="row.chargeId" class="badge badge-green charged-badge" title="Inclusa in una ripartizione in bozza">↗ Bozza</span>
                        <button v-if="canDelete && !isReadonly(row)"
                                class="btn-icon btn-icon--danger"
                                title="Elimina lettura"
                                @click="deleteRow(group, row)">🗑</button>
                      </div>
                    </td>
                  </tr>
                  <!-- Riga "+ Aggiungi lettura" per ogni contatore (non sui cancellati) -->
                  <tr v-if="canEdit && !group.meterIsDeleted" class="row-add">
                    <td colspan="8">
                      <button class="btn-add-reading" @click="addRow(group)">+ Aggiungi lettura</button>
                    </td>
                  </tr>
                </template>
              </tbody>
            </table>
          </div>
        </template>
      </div>

      <!-- ── TAB: Ripartizioni ──────────────────────────────────────────── -->
      <div v-if="tab === 'charges'" class="card">
        <div class="toolbar">
          <div class="form-group" style="margin:0; min-width:200px">
            <select class="form-select" v-model="chargesFiscalYearId" @change="loadCharges">
              <option :value="null" disabled>— Esercizio —</option>
              <option v-for="fy in fiscalYears" :key="fy.id" :value="fy.id">{{ fy.code }}</option>
            </select>
          </div>
          <div style="display:flex;gap:0.5rem;align-items:center">
            <button v-if="chargesFiscalYearId" class="btn btn-ghost btn-sm btn-icon-only" @click="loadCharges" :disabled="loadingCharges" title="Aggiorna">
              <span v-if="loadingCharges" class="spinner" style="width:13px;height:13px"></span>
              <span v-else>↺</span>
            </button>
            <button v-if="chargesFiscalYearId" class="btn btn-ghost btn-sm"
                    :class="{ 'btn-ghost--active': showDeletedCharges }"
                    @click="showDeletedCharges = !showDeletedCharges"
                    title="Mostra/nascondi ripartizioni con tipo cancellato">
              {{ showDeletedCharges ? 'Nascondi cancellati' : 'Mostra cancellati' }}
            </button>
            <button v-if="canCreate && chargesFiscalYearId" class="btn btn-primary btn-sm" @click="openChargeModal()">+ Nuova ripartizione</button>
          </div>
        </div>

        <div v-if="unchargedCount > 0 && chargesFiscalYearId && !loadingCharges" class="uncharged-warning">
          <span class="uncharged-icon">⚠</span>
          <span>
            Ci sono <strong>{{ unchargedCount }}</strong>
            {{ unchargedCount === 1 ? 'lettura non ancora ripartita' : 'letture non ancora ripartite' }}
            in questo esercizio.
          </span>
          <button class="btn btn-sm btn-ghost" @click="tab = 'readings'">Vai alle letture</button>
        </div>

        <div v-if="loadingCharges" class="loading-state"><div class="spinner"></div></div>
        <div v-else-if="!visibleCharges.length && chargesFiscalYearId" class="empty-state">
          <div class="empty-icon">◎</div>
          <div>Nessuna ripartizione per questo esercizio.</div>
        </div>
        <template v-else>
          <div v-for="charge in visibleCharges" :key="charge.id" class="charge-card"
               :class="{ 'charge-type-deleted': charge.consumptionTypeIsDeleted }">

            <!-- Testata cliccabile -->
            <div class="charge-header" :class="{ 'charge-header--expandable': true }"
                 @click="toggleCharge(charge.id)">
              <span class="charge-expand-icon">{{ expandedChargeId === charge.id ? '▾' : '▸' }}</span>
              <div class="charge-title">
                <span class="charge-type">{{ charge.consumptionTypeName }}</span>
                <span v-if="charge.consumptionTypeIsDeleted" class="badge badge-muted type-deleted-badge" title="Il tipo consumo è stato eliminato">Cancellato</span>
                <span class="text-secondary mono">{{ charge.unitOfMeasure }}</span>
                <span class="badge" :class="charge.statusId === 2 ? 'badge-green' : 'badge-draft'">{{ charge.statusName }}</span>
                <span v-if="charge.hasWarnings" class="badge badge-amber" title="Alcune unità non hanno letture">⚠ warning</span>
                <span v-if="charge.hasNoReadings && charge.statusId === 1"
                      class="badge badge-amber"
                      title="Nessuna lettura inserita — inserisci almeno una lettura e ricalcola prima di approvare">
                  ⚠ nessuna lettura
                </span>
              </div>
              <div class="charge-amount">{{ fmt(charge.totalAmount) }}</div>
              <div class="row-actions" v-if="charge.statusId === 1" @click.stop>
                <button v-if="canEdit" class="btn btn-sm btn-ghost" @click="recalculate(charge.id)"
                        :disabled="recalculatingId === charge.id">
                  <span v-if="recalculatingId === charge.id" class="spinner" style="width:13px;height:13px;margin-right:4px"></span>
                  <span v-else>↺</span>
                  {{ recalculatingId === charge.id ? 'Ricalcolo…' : 'Ricalcola' }}
                </button>
                <button v-if="canEdit" class="btn btn-sm btn-ghost btn-green"
                        :disabled="charge.hasNoReadings"
                        :title="charge.hasNoReadings ? 'Inserisci almeno una lettura e ricalcola prima di approvare' : ''"
                        @click="approveCharge(charge.id)">✓ Approva</button>
                <button v-if="canEdit" class="btn-icon" @click="openChargeModal(charge)">✎</button>
                <button v-if="canDelete" class="btn-icon btn-icon--danger" @click="deleteCharge(charge.id)">🗑</button>
              </div>
            </div>

            <!-- Dettaglio unità (collassabile) -->
            <div v-if="expandedChargeId === charge.id && charge.items?.length" class="charge-items">
              <table class="items-table">
                <thead>
                  <tr>
                    <th>Unità</th>
                    <th class="col-num">Consumo</th>
                    <th class="col-num">% sul totale</th>
                    <th class="col-num">Importo</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="item in charge.items" :key="item.id" :class="{ 'row-warning': item.hasWarning }">
                    <td>{{ item.unitName }}</td>
                    <td class="col-num mono">{{ fmtNum(item.consumption) }}</td>
                    <td class="col-num">{{ fmtPct(item.percentage) }}</td>
                    <td class="col-num" :class="item.amount > 0 ? '' : 'text-muted'">{{ fmt(item.amount) }}</td>
                    <td>
                      <span v-if="item.hasWarning" class="warn-icon" title="Nessuna lettura per questa unità">⚠</span>
                    </td>
                  </tr>
                </tbody>
                <tfoot>
                  <tr class="totals-row">
                    <td>Totale</td>
                    <td class="col-num mono">{{ fmtNum(charge.items.reduce((s,i) => s + i.consumption, 0)) }}</td>
                    <td class="col-num">{{ fmtPct(charge.items.reduce((s,i) => s + i.percentage, 0)) }}</td>
                    <td class="col-num">{{ fmt(charge.items.reduce((s,i) => s + i.amount, 0)) }}</td>
                    <td></td>
                  </tr>
                </tfoot>
              </table>
            </div>
            <div v-else-if="expandedChargeId === charge.id && !charge.items?.length" class="charge-empty">
              Nessuna unità calcolata — clicca Ricalcola.
            </div>

          </div>
        </template>
      </div>
    </template>

    <!-- ══ MODAL: Tipo consumo ════════════════════════════════════════════ -->
    <BaseModal :show="showTypeModal" @close="showTypeModal = false"
               :title="editingTypeId ? 'Modifica tipo consumo' : 'Nuovo tipo consumo'"
               size="sm">
      <div class="form-grid">
        <div class="form-group" :class="{ 'has-error': typeErrors.name }">
          <label class="form-label">Nome *</label>
          <input class="form-input" v-model="typeForm.name" @input="delete typeErrors.name" />
          <span v-if="typeErrors.name" class="field-error">{{ typeErrors.name }}</span>
        </div>
        <div class="form-group" :class="{ 'has-error': typeErrors.unitOfMeasure }">
          <label class="form-label">Unità di misura *</label>
          <input class="form-input" v-model="typeForm.unitOfMeasure" placeholder="mc, kWh, litri…" @input="delete typeErrors.unitOfMeasure" />
          <span v-if="typeErrors.unitOfMeasure" class="field-error">{{ typeErrors.unitOfMeasure }}</span>
        </div>
        <div class="form-group form-group--full">
          <label class="form-label">Conto spesa (piano dei conti)</label>
          <select class="form-select" v-model="typeForm.accountId">
            <option :value="null">— Nessuno —</option>
            <option v-for="a in uscitaAccounts" :key="a.id" :value="a.id">
              {{ a.code }} – {{ a.name }}
            </option>
          </select>
          <span class="field-hint">Se impostato, all'approvazione della ripartizione viene creata automaticamente una spesa su questo conto.</span>
        </div>
        <div v-if="editingTypeId" class="form-group form-group--full">
          <label class="form-label">
            <input type="checkbox" v-model="typeForm.isActive" style="margin-right:6px" />
            Attivo
          </label>
        </div>
        <div class="form-group form-group--full">
          <label class="form-label">Note</label>
          <textarea class="form-textarea" v-model="typeForm.notes" rows="2"></textarea>
        </div>
      </div>
      <template #footer>
        <button class="btn btn-ghost" @click="showTypeModal = false">Annulla</button>
        <button class="btn btn-primary" @click="saveType" :disabled="savingType">
          <span v-if="savingType" class="spinner" style="width:14px;height:14px"></span>
          {{ editingTypeId ? 'Salva' : 'Crea' }}
        </button>
      </template>
    </BaseModal>

    <!-- ══ MODAL: Contatore ═══════════════════════════════════════════════ -->
    <BaseModal :show="showMeterModal" @close="showMeterModal = false"
               :title="editingMeterId ? 'Modifica contatore' : 'Nuovo contatore'"
               size="sm">
      <div class="form-grid">
        <div v-if="!editingMeterId" class="form-group" :class="{ 'has-error': meterErrors.consumptionTypeId }">
          <label class="form-label">Tipo consumo *</label>
          <select class="form-select" v-model="meterForm.consumptionTypeId" @change="delete meterErrors.consumptionTypeId">
            <option :value="null" disabled>— Seleziona —</option>
            <option v-for="t in consumptionTypes.filter(x => !x.isDeleted)" :key="t.id" :value="t.id">{{ t.name }}</option>
          </select>
          <span v-if="meterErrors.consumptionTypeId" class="field-error">{{ meterErrors.consumptionTypeId }}</span>
        </div>
        <div v-if="!editingMeterId" class="form-group" :class="{ 'has-error': meterErrors.unitId }">
          <label class="form-label">Unità immobiliare *</label>
          <select class="form-select" v-model="meterForm.unitId" @change="delete meterErrors.unitId">
            <option :value="null" disabled>— Seleziona —</option>
            <option v-for="u in units" :key="u.id" :value="u.id">{{ u.internalNumber }}{{ u.displayName ? ' — ' + u.displayName : '' }}</option>
          </select>
          <span v-if="meterErrors.unitId" class="field-error">{{ meterErrors.unitId }}</span>
        </div>
        <div class="form-group form-group--full" :class="{ 'has-error': meterErrors.code }">
          <label class="form-label">Matricola contatore *</label>
          <input class="form-input" v-model="meterForm.code" @input="delete meterErrors.code" />
          <span v-if="meterErrors.code" class="field-error">{{ meterErrors.code }}</span>
        </div>
        <div v-if="editingMeterId" class="form-group form-group--full">
          <label class="form-label">
            <input type="checkbox" v-model="meterForm.isActive" style="margin-right:6px" />
            Attivo
          </label>
        </div>
        <div class="form-group form-group--full">
          <label class="form-label">Note</label>
          <textarea class="form-textarea" v-model="meterForm.notes" rows="2"></textarea>
        </div>
      </div>
      <template #footer>
        <button class="btn btn-ghost" @click="showMeterModal = false">Annulla</button>
        <button class="btn btn-primary" @click="saveMeter" :disabled="savingMeter">
          <span v-if="savingMeter" class="spinner" style="width:14px;height:14px"></span>
          {{ editingMeterId ? 'Salva' : 'Crea' }}
        </button>
      </template>
    </BaseModal>

    <!-- ══ MODAL: Ripartizione ════════════════════════════════════════════ -->
    <BaseModal :show="showChargeModal" @close="showChargeModal = false"
               :title="editingChargeId ? 'Modifica ripartizione' : 'Nuova ripartizione'"
               size="sm">
      <div class="form-grid">
        <div v-if="!editingChargeId" class="form-group form-group--full" :class="{ 'has-error': chargeErrors.consumptionTypeId }">
          <label class="form-label">Tipo consumo *</label>
          <select class="form-select" v-model="chargeForm.consumptionTypeId" @change="delete chargeErrors.consumptionTypeId">
            <option :value="null" disabled>— Seleziona —</option>
            <option v-for="t in consumptionTypes.filter(x => !x.isDeleted)" :key="t.id" :value="t.id">{{ t.name }}</option>
          </select>
          <span v-if="chargeErrors.consumptionTypeId" class="field-error">{{ chargeErrors.consumptionTypeId }}</span>
        </div>
        <div class="form-group form-group--full" :class="{ 'has-error': chargeErrors.totalAmount }">
          <label class="form-label">Importo totale spesa (€) *</label>
          <input class="form-input" type="number" step="0.01" v-model.number="chargeForm.totalAmount" @input="delete chargeErrors.totalAmount" />
          <span v-if="chargeErrors.totalAmount" class="field-error">{{ chargeErrors.totalAmount }}</span>
        </div>
        <div class="form-group form-group--full">
          <label class="form-label">Note</label>
          <textarea class="form-textarea" v-model="chargeForm.notes" rows="2"></textarea>
        </div>
      </div>
      <template #footer>
        <button class="btn btn-ghost" @click="showChargeModal = false">Annulla</button>
        <button class="btn btn-primary" @click="saveCharge" :disabled="savingCharge">
          <span v-if="savingCharge" class="spinner" style="width:14px;height:14px"></span>
          {{ editingChargeId ? 'Salva' : 'Crea e calcola' }}
        </button>
      </template>
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useAppStore } from '@/stores/app'
import { usePermissions } from '@/composables/usePermissions'
import {
  consumptionTypeApi, meterApi, consumptionReadingApi, consumptionChargeApi,
  fiscalYearApi, unitApi, chartOfAccountsApi,
} from '@/services/api'
import BaseModal from '@/components/BaseModal.vue'

const store = useAppStore()
const { canCreate, canEdit, canDelete } = usePermissions()

// ── Tab ────────────────────────────────────────────────────────────────────
const tab = ref('types')

// ── Piano dei conti (per select nel form tipo consumo) ────────────────────
const accounts     = ref([])
const uscitaAccounts = computed(() => accounts.value.filter(a => a.type === 2 && a.isActive))

async function loadAccounts() {
  if (!store.selectedCondominioId) { accounts.value = []; return }
  try {
    const { data } = await chartOfAccountsApi.getByCondominium(store.selectedCondominioId)
    accounts.value = data ?? []
  } catch { accounts.value = [] }
}

// ── Tipi consumo ──────────────────────────────────────────────────────────
const consumptionTypes  = ref([])
const loadingTypes      = ref(false)
const showDeletedTypes  = ref(false)

const visibleTypes = computed(() =>
  showDeletedTypes.value
    ? consumptionTypes.value
    : consumptionTypes.value.filter(t => !t.isDeleted)
)

async function loadTypes() {
  if (!store.selectedCondominioId) { consumptionTypes.value = []; return }
  loadingTypes.value = true
  try {
    const { data } = await consumptionTypeApi.getByCondominium(store.selectedCondominioId)
    consumptionTypes.value = data ?? []
  } catch { consumptionTypes.value = [] } finally { loadingTypes.value = false }
}

// ── Contatori ─────────────────────────────────────────────────────────────
const meters             = ref([])
const loadingMeters      = ref(false)
const showDeletedMeters  = ref(false)

const visibleMeters = computed(() =>
  showDeletedMeters.value
    ? meters.value
    : meters.value.filter(m => !m.isDeleted && !m.consumptionTypeIsDeleted)
)

async function loadMeters() {
  if (!store.selectedCondominioId) { meters.value = []; return }
  loadingMeters.value = true
  try {
    const { data } = await meterApi.getByCondominium(store.selectedCondominioId)
    meters.value = data ?? []
  } catch { meters.value = [] } finally { loadingMeters.value = false }
}

// ── Unità (per modal contatore) ───────────────────────────────────────────
const units = ref([])
async function loadUnits() {
  if (!store.selectedCondominioId) return
  try {
    const { data } = await unitApi.getByCondominium(store.selectedCondominioId)
    units.value = data ?? []
  } catch { units.value = [] }
}

// ── Esercizi fiscali (per letture e ripartizioni) ─────────────────────────
const fiscalYears = ref([])
async function loadFiscalYears() {
  if (!store.selectedCondominioId) return
  try {
    const { data } = await fiscalYearApi.getByCondominium(store.selectedCondominioId)
    fiscalYears.value = data ?? []
  } catch { fiscalYears.value = [] }
}

// ── Letture ───────────────────────────────────────────────────────────────
const selectedTypeId       = ref(null)
const selectedFiscalYearId = ref(null)
const meterGroups          = ref([])   // [{ meterId, unitName, meterCode, meterIsDeleted, rows: [...] }]
const loadingReadings      = ref(false)
const savingReadings       = ref(false)
const showDeletedReadings  = ref(false)

const visibleGroups = computed(() =>
  showDeletedReadings.value
    ? meterGroups.value
    : meterGroups.value.filter(g => !g.meterIsDeleted)
)

function makeEmptyRow(meterId, meterIsDeleted = false) {
  return { id: 0, meterId, initialValue: 0, initialDate: '', finalValue: 0, finalDate: '', notes: '', meterIsDeleted, modified: true, saving: false, saved: false, prevFinalValue: null, prevFinalDate: null }
}

function dayAfter(dateStr) {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  d.setDate(d.getDate() + 1)
  return d.toISOString().slice(0, 10)
}

/** Restituisce il messaggio di errore della riga, o null se valida. */
function rowError(row) {
  if (!row.initialDate) return 'Data inizio mancante'
  if (!row.finalDate)   return 'Data fine mancante'
  if (row.initialDate > row.finalDate) return 'Data inizio > data fine'
  if (row.finalValue < row.initialValue) return 'Lettura finale < lettura iniziale'
  if (row.prevFinalValue !== null && row.initialValue !== row.prevFinalValue)
    return `Lettura iniziale deve essere ${row.prevFinalValue} (finale lettura precedente)`
  if (row.prevFinalDate !== null && row.initialDate !== dayAfter(row.prevFinalDate))
    return `Data inizio deve essere ${dayAfter(row.prevFinalDate)} (giorno dopo data finale precedente)`
  return null
}

const allRows          = computed(() => meterGroups.value.flatMap(g => g.rows))
const validReadingRows = computed(() => allRows.value.filter(r => !rowError(r) && !r.meterIsDeleted))

function onRowInput(row) {
  row.modified = true
  row.saved    = false
}

function addRow(group) {
  const prev = group.rows.at(-1)
  const newRow = makeEmptyRow(group.meterId)
  if (prev) {
    newRow.initialValue  = prev.finalValue
    newRow.initialDate   = dayAfter(prev.finalDate)
    newRow.prevFinalValue = prev.finalValue
    newRow.prevFinalDate  = prev.finalDate
  }
  group.rows.push(newRow)
}

async function deleteRow(group, row) {
  if (row.id > 0) {
    try {
      await consumptionReadingApi.delete(row.id)
    } catch (err) {
      if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
      return
    }
  }
  const idx = group.rows.indexOf(row)
  if (idx !== -1) group.rows.splice(idx, 1)
  if (group.rows.length === 0) {
    group.rows.push(makeEmptyRow(group.meterId))
  } else {
    // ricalcola i riferimenti alla riga precedente per le righe successive
    group.rows.forEach((r, i) => {
      r.prevFinalValue = i > 0 ? group.rows[i - 1].finalValue : null
      r.prevFinalDate  = i > 0 ? group.rows[i - 1].finalDate  : null
    })
  }
}

async function onTypeOrFyChange() {
  if (!selectedTypeId.value || !selectedFiscalYearId.value) return
  loadingReadings.value = true
  try {
    const [metersRes, readingsRes] = await Promise.all([
      meterApi.getByType(selectedTypeId.value),
      consumptionReadingApi.getByTypeFiscalYear(selectedTypeId.value, selectedFiscalYearId.value),
    ])
    const typeMeters    = metersRes.data ?? []
    const savedReadings = readingsRes.data ?? []

    // raggruppa letture per contatore
    const readingsByMeter = {}
    for (const r of savedReadings) {
      if (!readingsByMeter[r.meterId]) readingsByMeter[r.meterId] = []
      readingsByMeter[r.meterId].push(r)
    }

    // include anche contatori cancellati se hanno letture
    const metersWithReadings = new Set(savedReadings.map(r => r.meterId))
    const allMeters = typeMeters.filter(m => !m.isDeleted || metersWithReadings.has(m.id))

    meterGroups.value = allMeters
      .sort((a, b) => {
        if (a.isDeleted !== b.isDeleted) return a.isDeleted ? 1 : -1
        return (a.unitName ?? '').localeCompare(b.unitName ?? '', 'it', { sensitivity: 'base' })
      })
      .map(m => {
        const existing = (readingsByMeter[m.id] ?? [])
          .sort((a, b) => (a.initialDate ?? '') < (b.initialDate ?? '') ? -1 : 1)
        const rows = existing.length
          ? existing.map((r, idx) => ({
              id:             r.id,
              meterId:        m.id,
              initialValue:   r.initialValue,
              initialDate:    r.initialDate?.slice(0, 10) ?? '',
              finalValue:     r.finalValue,
              finalDate:      r.finalDate?.slice(0, 10) ?? '',
              notes:          r.notes ?? '',
              chargeId:       r.chargeId ?? null,
              chargeStatusId: r.chargeStatusId ?? null,
              meterIsDeleted: r.meterIsDeleted ?? m.isDeleted ?? false,
              modified:       false,
              saving:         false,
              saved:          true,
              prevFinalValue: idx > 0 ? existing[idx - 1].finalValue : null,
              prevFinalDate:  idx > 0 ? existing[idx - 1].finalDate?.slice(0, 10) ?? null : null,
            }))
          : [makeEmptyRow(m.id)]
        return { meterId: m.id, unitName: m.unitName, meterCode: m.code, meterIsDeleted: m.isDeleted ?? false, rows }
      })
  } catch { meterGroups.value = [] } finally { loadingReadings.value = false }
}

function rowToItem(r) {
  return { id: r.id, meterId: r.meterId, initialValue: r.initialValue, initialDate: r.initialDate || null, finalValue: r.finalValue, finalDate: r.finalDate || null, notes: r.notes || null }
}

/** Salva una singola riga. */
async function saveRow(row) {
  row.saving = true
  try {
    const res = await consumptionReadingApi.saveBulk({
      consumptionTypeId: selectedTypeId.value,
      fiscalYearId:      selectedFiscalYearId.value,
      items:             [rowToItem(row)],
    })
    // aggiorna l'id se era nuovo
    const saved = res.data?.[0]
    if (saved && row.id === 0) row.id = saved.id
    row.modified = false
    row.saved    = true
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { row.saving = false }
}

/** Salva tutte le righe valide. */
async function saveReadings() {
  if (!selectedTypeId.value || !selectedFiscalYearId.value) return
  const toSave = validReadingRows.value
  if (!toSave.length) return
  savingReadings.value = true
  try {
    const res = await consumptionReadingApi.saveBulk({
      consumptionTypeId: selectedTypeId.value,
      fiscalYearId:      selectedFiscalYearId.value,
      items:             toSave.map(rowToItem),
    })
    // aggiorna gli id per le righe nuove
    const saved = res.data ?? []
    toSave.forEach((r, i) => {
      if (r.id === 0 && saved[i]?.id) r.id = saved[i].id
      r.modified = false
      r.saved    = true
    })
    const skipped = allRows.value.length - toSave.length
    store.toast(
      skipped > 0
        ? `${toSave.length} letture salvate, ${skipped} saltate per dati non validi`
        : `${toSave.length} letture salvate`,
      'success'
    )
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { savingReadings.value = false }
}

/** Lettura non modificabile: ha una ripartizione già approvata (statusId !== 1 = Draft). */
function isReadonly(row) {
  return !!row.chargeId && row.chargeStatusId !== 1
}

function consumptionClass(row) {
  if (rowError(row)) return ''
  const c = row.finalValue - row.initialValue
  if (c < 0) return 'text-red'
  if (c > 0) return 'text-green'
  return 'text-muted'
}

function selectType(t) {
  selectedTypeId.value = t.id
  tab.value = 'readings'
}

// ── Ripartizioni ──────────────────────────────────────────────────────────
const charges             = ref([])
const loadingCharges      = ref(false)
const chargesFiscalYearId = ref(null)
const unchargedCount      = ref(0)
const showDeletedCharges  = ref(false)
const expandedChargeId    = ref(null)

function toggleCharge(id) {
  expandedChargeId.value = expandedChargeId.value === id ? null : id
}

const visibleCharges = computed(() =>
  showDeletedCharges.value
    ? charges.value
    : charges.value.filter(c => !c.consumptionTypeIsDeleted)
)

async function loadCharges() {
  if (!chargesFiscalYearId.value) return
  loadingCharges.value = true
  try {
    const [chargesRes, unchargedRes] = await Promise.all([
      consumptionChargeApi.getByFiscalYear(chargesFiscalYearId.value),
      consumptionReadingApi.getUnchargedCount(chargesFiscalYearId.value),
    ])
    charges.value = chargesRes.data ?? []
    unchargedCount.value = unchargedRes.data ?? 0
  } catch { charges.value = [] } finally { loadingCharges.value = false }
}

const recalculatingId = ref(null)

async function recalculate(id) {
  recalculatingId.value = id
  try {
    const { data } = await consumptionChargeApi.recalculate(id)
    const idx = charges.value.findIndex(c => c.id === id)
    if (idx !== -1) charges.value[idx] = data
    store.toast('Ripartizione ricalcolata', 'success')
  } catch (err) {
    if (!err?.response) store.toast('Errore di rete', 'error')
  } finally {
    recalculatingId.value = null
  }
}

async function approveCharge(id) {
  if (!confirm('Approvare la ripartizione? Verranno generate le rate per ogni unità.')) return
  try {
    const { data } = await consumptionChargeApi.approve(id)
    const idx = charges.value.findIndex(c => c.id === id)
    if (idx !== -1) charges.value[idx] = data
    store.toast('Ripartizione approvata — rate generate', 'success')
  } catch (err) {
    if (!err?.response) store.toast('Errore di rete', 'error')
    // altrimenti l'errore HTTP viene mostrato dal gestore globale api:error
  }
}

async function deleteCharge(id) {
  if (!confirm('Eliminare questa ripartizione?')) return
  try {
    await consumptionChargeApi.delete(id)
    charges.value = charges.value.filter(c => c.id !== id)
    store.toast('Ripartizione eliminata', 'success')
  } catch (err) { if (!err?.response) store.toast('Errore di rete', 'error') }
}

// ── Modal tipo consumo ────────────────────────────────────────────────────
const showTypeModal  = ref(false)
const editingTypeId  = ref(null)
const savingType     = ref(false)
const typeErrors     = ref({})
const typeForm       = ref({ name: '', unitOfMeasure: '', accountId: null, notes: '', isActive: true })

function openTypeModal(t = null) {
  editingTypeId.value = t?.id ?? null
  typeForm.value = t
    ? { name: t.name, unitOfMeasure: t.unitOfMeasure, accountId: t.accountId ?? null, notes: t.notes ?? '', isActive: t.isActive }
    : { name: '', unitOfMeasure: '', accountId: null, notes: '', isActive: true }
  typeErrors.value = {}
  showTypeModal.value = true
}

async function saveType() {
  typeErrors.value = {}
  if (!typeForm.value.name?.trim()) typeErrors.value.name = 'Obbligatorio'
  if (!typeForm.value.unitOfMeasure?.trim()) typeErrors.value.unitOfMeasure = 'Obbligatorio'
  if (Object.keys(typeErrors.value).length) return
  savingType.value = true
  try {
    if (editingTypeId.value) {
      await consumptionTypeApi.update(editingTypeId.value, typeForm.value)
    } else {
      await consumptionTypeApi.create({ ...typeForm.value, condominiumId: store.selectedCondominioId })
    }
    store.toast(editingTypeId.value ? 'Tipo aggiornato' : 'Tipo creato', 'success')
    showTypeModal.value = false
    await loadTypes()
  } catch (err) { if (!err?.response) store.toast('Errore di rete', 'error') } finally { savingType.value = false }
}

async function deleteType(t) {
  const msg = `Eliminare il tipo di consumo "${t.name}"?\n\n` +
    `Se non esistono letture in ripartizioni approvate, verranno eliminati anche tutti i contatori e le letture associate.\n` +
    `Se invece esistono letture approvate, il tipo verrà solo disattivato (cancellazione logica).`
  if (!confirm(msg)) return
  try {
    await consumptionTypeApi.delete(t.id)
    store.toast('Tipo eliminato', 'success')
    await loadTypes()
  } catch (err) { if (!err?.response) store.toast('Errore di rete', 'error') }
}

// ── Modal contatore ───────────────────────────────────────────────────────
const showMeterModal = ref(false)
const editingMeterId = ref(null)
const savingMeter    = ref(false)
const meterErrors    = ref({})
const meterForm      = ref({ consumptionTypeId: null, unitId: null, code: '', notes: '', isActive: true })

function openMeterModal(m = null) {
  editingMeterId.value = m?.id ?? null
  meterForm.value = m
    ? { consumptionTypeId: m.consumptionTypeId, unitId: m.unitId, code: m.code, notes: m.notes ?? '', isActive: m.isActive }
    : { consumptionTypeId: null, unitId: null, code: '', notes: '', isActive: true }
  meterErrors.value = {}
  showMeterModal.value = true
}

async function saveMeter() {
  meterErrors.value = {}
  if (!editingMeterId.value && !meterForm.value.consumptionTypeId) meterErrors.value.consumptionTypeId = 'Obbligatorio'
  if (!editingMeterId.value && !meterForm.value.unitId) meterErrors.value.unitId = 'Obbligatorio'
  if (!meterForm.value.code?.trim()) meterErrors.value.code = 'Obbligatorio'
  if (Object.keys(meterErrors.value).length) return
  savingMeter.value = true
  try {
    if (editingMeterId.value) {
      await meterApi.update(editingMeterId.value, { code: meterForm.value.code, notes: meterForm.value.notes, isActive: meterForm.value.isActive })
    } else {
      await meterApi.create(meterForm.value)
    }
    store.toast(editingMeterId.value ? 'Contatore aggiornato' : 'Contatore creato', 'success')
    showMeterModal.value = false
    await loadMeters()
  } catch (err) { if (!err?.response) store.toast('Errore di rete', 'error') } finally { savingMeter.value = false }
}

async function deleteMeter(id) {
  if (!confirm('Eliminare questo contatore?')) return
  try {
    await meterApi.delete(id)
    store.toast('Contatore eliminato', 'success')
    await loadMeters()
  } catch (err) { if (!err?.response) store.toast('Errore di rete', 'error') }
}

// ── Modal ripartizione ────────────────────────────────────────────────────
const showChargeModal  = ref(false)
const editingChargeId  = ref(null)
const savingCharge     = ref(false)
const chargeErrors     = ref({})
const chargeForm       = ref({ consumptionTypeId: null, totalAmount: 0, notes: '' })

function openChargeModal(c = null) {
  editingChargeId.value = c?.id ?? null
  chargeForm.value = c
    ? { consumptionTypeId: c.consumptionTypeId, totalAmount: c.totalAmount, notes: c.notes ?? '' }
    : { consumptionTypeId: null, totalAmount: 0, notes: '' }
  chargeErrors.value = {}
  showChargeModal.value = true
}

async function saveCharge() {
  chargeErrors.value = {}
  if (!editingChargeId.value && !chargeForm.value.consumptionTypeId) chargeErrors.value.consumptionTypeId = 'Obbligatorio'
  if (!chargeForm.value.totalAmount || chargeForm.value.totalAmount <= 0) chargeErrors.value.totalAmount = 'Deve essere maggiore di zero'
  if (Object.keys(chargeErrors.value).length) return
  savingCharge.value = true
  try {
    if (editingChargeId.value) {
      const { data } = await consumptionChargeApi.update(editingChargeId.value, {
        totalAmount: chargeForm.value.totalAmount,
        notes: chargeForm.value.notes,
      })
      const idx = charges.value.findIndex(c => c.id === editingChargeId.value)
      if (idx !== -1) charges.value[idx] = data
    } else {
      await consumptionChargeApi.create({
        consumptionTypeId: chargeForm.value.consumptionTypeId,
        fiscalYearId:      chargesFiscalYearId.value,
        totalAmount:       chargeForm.value.totalAmount,
        notes:             chargeForm.value.notes,
      })
      await loadCharges()
    }
    store.toast(editingChargeId.value ? 'Ripartizione aggiornata' : 'Ripartizione creata', 'success')
    showChargeModal.value = false
  } catch (err) { if (!err?.response) store.toast('Errore di rete', 'error') } finally { savingCharge.value = false }
}

// ── Formatters ─────────────────────────────────────────────────────────────
const fmt    = (v) => v != null ? '€ ' + Number(v).toLocaleString('it-IT', { minimumFractionDigits: 2 }) : '—'
const fmtNum = (v) => v != null ? Number(v).toLocaleString('it-IT', { minimumFractionDigits: 3, maximumFractionDigits: 3 }) : '—'
const fmtPct = (v) => v != null ? (Number(v) * 100).toLocaleString('it-IT', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' %' : '—'

// ── Init ──────────────────────────────────────────────────────────────────
async function loadAll() {
  await Promise.all([loadTypes(), loadMeters(), loadUnits(), loadFiscalYears(), loadAccounts()])
}

onMounted(loadAll)
watch(() => store.selectedCondominioId, () => {
  selectedTypeId.value       = null
  selectedFiscalYearId.value = null
  chargesFiscalYearId.value  = null
  meterGroups.value          = []
  charges.value              = []
  loadAll()
})
</script>

<style scoped>
.tab-bar { display: flex; gap: 0.25rem; margin-bottom: 1rem; border-bottom: 1px solid var(--border); }
.tab-btn {
  padding: 0.5rem 1.25rem;
  border: none; background: none; cursor: pointer;
  color: var(--text-secondary); font-size: 0.9rem;
  border-bottom: 2px solid transparent; margin-bottom: -1px;
  transition: color 0.15s, border-color 0.15s;
}
.tab-btn:hover:not(:disabled) { color: var(--text); }
.tab-btn.active { color: var(--accent); border-bottom-color: var(--accent); font-weight: 600; }
.tab-btn:disabled { opacity: 0.4; cursor: not-allowed; }

.toolbar {
  display: flex; gap: 0.75rem; align-items: center; flex-wrap: wrap;
  padding: 0.75rem 1rem;
  border-bottom: 1px solid var(--border);
}
.readings-toolbar { justify-content: space-between; }
.readings-selectors { display: flex; gap: 0.5rem; flex-wrap: nowrap; }
.readings-select { width: 220px; flex-shrink: 0; }

.readings-table tbody tr:hover td { background: var(--bg-surface); }
.readings-table .col-num    { width: 120px; text-align: right; }
.readings-table th.col-num  { text-align: right; }
.readings-table .col-date   { width: 150px; }
.readings-table .col-action { width: 48px; text-align: center; padding: 0; }

.inline-input {
  width: 100%;
  padding: 0.3rem 0.45rem;
  background: var(--bg-base);
  border: 1px solid var(--border);
  border-radius: 4px;
  color: var(--text);
  font-size: 0.875rem;
  font-family: inherit;
  transition: border-color 0.15s;
  box-sizing: border-box;
}
.inline-input:focus {
  outline: none;
  border-color: var(--border-active, var(--accent));
}
input[type="date"].inline-input { color-scheme: dark; }

.btn-icon--save {
  display: inline-flex; align-items: center; justify-content: center;
  width: 28px; height: 28px; border-radius: 50%;
  border: none; cursor: pointer;
  background: color-mix(in srgb, var(--accent-green) 15%, transparent);
  color: var(--accent-green);
  font-size: 0.9rem;
  transition: background 0.15s;
}
.btn-icon--save:hover:not(:disabled) { background: color-mix(in srgb, var(--accent-green) 28%, transparent); }
.btn-icon--save:disabled { opacity: 0.5; cursor: not-allowed; }

.row-error-icon { color: var(--accent-red); font-size: 1rem; cursor: help; }

.uncharged-warning {
  display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap;
  margin: 0.75rem 1rem;
  padding: 0.65rem 1rem;
  background: color-mix(in srgb, var(--accent-amber, #f59e0b) 10%, transparent);
  border: 1px solid color-mix(in srgb, var(--accent-amber, #f59e0b) 35%, transparent);
  border-radius: 6px;
  font-size: 0.875rem;
  color: var(--text);
}
.uncharged-icon { color: var(--accent-amber, #f59e0b); font-size: 1rem; flex-shrink: 0; }

.row-charged td { opacity: 0.6; }
.row-meter-deleted td { opacity: 0.5; background: color-mix(in srgb, var(--accent-red) 4%, transparent); }
.meter-deleted-badge { font-size: 0.7rem; margin-left: 0.35rem; vertical-align: middle; }
.btn-ghost--active { background: var(--accent-glow); color: var(--accent); }
.btn-icon-only { width: 30px; padding: 0; justify-content: center; }
.inline-value { display: block; padding: 0.3rem 0.45rem; font-size: 0.875rem; }
.charged-badge { font-size: 0.75rem; white-space: nowrap; }

.row-btns { display: flex; gap: 0.25rem; align-items: center; justify-content: center; }

.row-add td { padding: 0.25rem 0.75rem; border-bottom: 2px solid var(--border); }
.btn-add-reading {
  background: none; border: none; cursor: pointer;
  color: var(--accent); font-size: 0.8rem; padding: 0.15rem 0;
  opacity: 0.75; transition: opacity 0.15s;
}
.btn-add-reading:hover { opacity: 1; }

.row-warning td { background: color-mix(in srgb, var(--accent-amber, #f59e0b) 8%, transparent); }
.warn-icon { color: var(--accent-amber, #f59e0b); cursor: help; }

.charge-card {
  border: 1px solid var(--border);
  border-radius: 8px;
  margin-bottom: 1rem;
  overflow: hidden;
}
.charge-header {
  display: flex; align-items: center; gap: 1rem;
  padding: 0.75rem 1rem;
  background: var(--bg-surface);
  border-bottom: 1px solid var(--border);
  user-select: none;
}
.charge-header--expandable { cursor: pointer; }
.charge-header--expandable:hover { background: color-mix(in srgb, var(--accent) 4%, var(--bg-surface)); }
.charge-expand-icon { color: var(--text-muted); font-size: 0.75rem; flex-shrink: 0; width: 14px; }
.charge-empty { padding: 0.75rem 1rem; font-size: 0.875rem; color: var(--text-muted); }
.charge-title { display: flex; align-items: center; gap: 0.5rem; flex: 1; flex-wrap: wrap; }
.charge-type  { font-weight: 600; }
.charge-amount { font-weight: 600; font-size: 1.05rem; white-space: nowrap; }
.charge-items { padding: 0.75rem 1rem; }

.items-table { width: 100%; border-collapse: collapse; font-size: 0.875rem; }
.items-table th, .items-table td { padding: 0.35rem 0.5rem; }
.items-table thead th { color: var(--text-secondary); font-weight: 500; border-bottom: 1px solid var(--border); }
.items-table tbody tr:hover td { background: var(--bg-surface); }
.items-table tfoot .totals-row td { border-top: 1px solid var(--border); font-weight: 600; }
.items-table .col-num { text-align: right; }

.link-btn { background: none; border: none; color: var(--accent); cursor: pointer; padding: 0; text-decoration: underline; font-size: inherit; }

.row-type-deleted td { opacity: 0.65; }
.type-deleted-badge { font-size: 0.7rem; margin-left: 0.35rem; vertical-align: middle; }

.charge-type-deleted { opacity: 0.65; border-color: color-mix(in srgb, var(--accent-red) 30%, var(--border)); }
.charge-type-deleted .charge-header { background: color-mix(in srgb, var(--accent-red) 4%, var(--bg-surface)); }

.row-error td { background: color-mix(in srgb, var(--accent-red) 6%, transparent); }
.row-saved td { background: color-mix(in srgb, var(--accent-green) 6%, transparent); }
.row-error-msg { color: var(--accent-red); font-size: 0.8rem; white-space: nowrap; }
.saved-check { color: var(--accent-green); font-size: 1rem; }
.field-hint { display: block; font-size: 0.78rem; color: var(--text-muted); margin-top: 4px; }
</style>
