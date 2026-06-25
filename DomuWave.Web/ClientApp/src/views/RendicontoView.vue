<template>
  <div>
    <!-- Toolbar: selettore esercizio -->
    <div class="toolbar">
      <select class="form-select" v-model="selectedFiscalYearId" style="min-width:200px"
              :disabled="!fiscalYears.length">
        <option :value="null" disabled>Seleziona esercizio…</option>
        <option v-for="fy in fiscalYears" :key="fy.id" :value="fy.id">
          {{ fy.code }}{{ fy.description ? ' – ' + fy.description : '' }}
        </option>
      </select>
      <div v-if="selectedFiscalYearId" class="tab-pills">
        <button class="tab-pill" :class="{ active: activeTab === 'economico' }" @click="activeTab = 'economico'">
          Conto economico
        </button>
        <button class="tab-pill" :class="{ active: activeTab === 'cassa' }" @click="activeTab = 'cassa'">
          Flussi di cassa
        </button>
        <button class="tab-pill" :class="{ active: activeTab === 'patrimoniale' }" @click="activeTab = 'patrimoniale'">
          Situazione patrimoniale
        </button>
        <button class="tab-pill" :class="{ active: activeTab === 'conguaglio' }" @click="activeTab = 'conguaglio'">
          Conguaglio per unità
        </button>
        <button class="tab-pill" :class="{ active: activeTab === 'conti' }" @click="activeTab = 'conti'">
          Dettaglio per conto
        </button>
      </div>
    </div>

    <!-- Toolbar export (solo per i prospetti di bilancio, quando i dati sono pronti) -->
    <div v-if="canExport" class="export-toolbar">
      <button class="btn btn-ghost btn-sm" @click="exportPdf">📄 Salva come PDF</button>
      <button class="btn btn-ghost btn-sm" @click="exportExcel">⬇ Scarica Excel</button>
    </div>

    <!-- No esercizio selezionato -->
    <div v-if="!selectedFiscalYearId" class="card">
      <div class="empty-state">
        <div class="empty-icon">◎</div>
        <div>Seleziona un esercizio fiscale per visualizzare il rendiconto</div>
      </div>
    </div>

    <template v-else>
      <!-- Avviso: nessun consuntivo approvato (solo per le tab che lo richiedono) -->
      <div v-if="!loading && !consuntivo && (activeTab === 'conguaglio' || activeTab === 'conti')"
           class="warning-banner" style="margin-bottom:12px">
        ⚠ Nessun budget consuntivo approvato per questo esercizio. Il rendiconto non è disponibile.
      </div>

      <!-- ══ TAB: Conto economico (competenza) ══════════════════════════════ -->
      <template v-if="activeTab === 'economico'">
        <div v-if="loadingEconomico" class="card"><div class="loading-state"><div class="spinner"></div></div></div>
        <template v-else-if="contoEconomico">
          <div class="card summary-card">
            <div class="summary-meta">
              <div><span class="meta-label">Periodo</span>
                <strong>{{ fmtDate(contoEconomico.startDate) }} – {{ fmtDate(contoEconomico.endDate) }}</strong></div>
            </div>
            <div class="kpi-row">
              <div class="kpi"><span class="kpi-label">Totale entrate</span>
                <span class="kpi-value text-green">{{ fmt(contoEconomico.totaleEntrate) }}</span></div>
              <div class="kpi"><span class="kpi-label">Totale uscite</span>
                <span class="kpi-value text-red">{{ fmt(contoEconomico.totaleUscite) }}</span></div>
              <div class="kpi"><span class="kpi-label">{{ contoEconomico.saldoTipo }}</span>
                <span class="kpi-value"
                      :class="contoEconomico.saldoFinale > 0 ? 'text-green' : contoEconomico.saldoFinale < 0 ? 'text-red' : 'text-muted'">
                  {{ contoEconomico.saldoFinale >= 0 ? '+' : '' }}{{ fmt(contoEconomico.saldoFinale) }}</span></div>
            </div>
          </div>

          <div class="balance-grid">
            <!-- ENTRATE -->
            <div class="card">
              <h3 class="section-title">Entrate</h3>
              <div class="table-wrap">
                <table>
                  <tbody>
                    <tr><td>Versamenti dei condòmini</td>
                      <td class="mono text-right">{{ fmt(contoEconomico.versamentiCondomini) }}</td></tr>
                    <tr v-for="r in contoEconomico.entrateRows" :key="'e-'+r.accountId">
                      <td><span class="mono text-muted" style="margin-right:6px">{{ r.accountCode }}</span>{{ r.accountName }}</td>
                      <td class="mono text-right">{{ fmt(r.amount) }}</td></tr>
                  </tbody>
                  <tfoot>
                    <tr class="row-total"><td><strong>Totale entrate</strong></td>
                      <td class="mono text-right"><strong>{{ fmt(contoEconomico.totaleEntrate) }}</strong></td></tr>
                  </tfoot>
                </table>
              </div>
            </div>
            <!-- USCITE -->
            <div class="card">
              <h3 class="section-title">Uscite</h3>
              <div class="table-wrap">
                <table>
                  <tbody>
                    <tr v-for="r in contoEconomico.usciteRows" :key="'u-'+r.accountId">
                      <td><span class="mono text-muted" style="margin-right:6px">{{ r.accountCode }}</span>{{ r.accountName }}</td>
                      <td class="mono text-right">{{ fmt(r.amount) }}</td></tr>
                    <tr v-if="!contoEconomico.usciteRows.length"><td colspan="2" class="text-muted">Nessuna spesa contabilizzata.</td></tr>
                  </tbody>
                  <tfoot>
                    <tr v-if="contoEconomico.saldoEsercizioPrecedente"><td>Saldo esercizio precedente</td>
                      <td class="mono text-right">{{ fmt(contoEconomico.saldoEsercizioPrecedente) }}</td></tr>
                    <tr class="row-total"><td><strong>Totale uscite</strong></td>
                      <td class="mono text-right"><strong>{{ fmt(contoEconomico.totaleUscite) }}</strong></td></tr>
                  </tfoot>
                </table>
              </div>
            </div>
          </div>
        </template>
      </template>

      <!-- ══ TAB: Flussi di cassa (cassa) ═══════════════════════════════════ -->
      <template v-else-if="activeTab === 'cassa'">
        <div v-if="loadingCassa" class="card"><div class="loading-state"><div class="spinner"></div></div></div>
        <template v-else-if="flussiCassa">
          <div class="card summary-card">
            <div class="summary-meta">
              <div><span class="meta-label">Periodo</span>
                <strong>{{ fmtDate(flussiCassa.startDate) }} – {{ fmtDate(flussiCassa.endDate) }}</strong></div>
            </div>
            <div class="kpi-row">
              <div class="kpi"><span class="kpi-label">Avanzo iniziale</span>
                <span class="kpi-value">{{ fmt(flussiCassa.avanzoInizialeCassa) }}</span></div>
              <div class="kpi"><span class="kpi-label">Totale incassi</span>
                <span class="kpi-value text-green">{{ fmt(flussiCassa.totaleIncassi) }}</span></div>
              <div class="kpi"><span class="kpi-label">Totale pagamenti</span>
                <span class="kpi-value text-red">{{ fmt(flussiCassa.totalePagamenti) }}</span></div>
              <div class="kpi"><span class="kpi-label">Avanzo finale</span>
                <span class="kpi-value"
                      :class="flussiCassa.avanzoFinaleCassa >= 0 ? 'text-green' : 'text-red'">
                  {{ fmt(flussiCassa.avanzoFinaleCassa) }}</span></div>
            </div>
          </div>

          <div class="balance-grid">
            <!-- INCASSI -->
            <div class="card">
              <h3 class="section-title">Incassi</h3>
              <div class="table-wrap">
                <table>
                  <tbody>
                    <tr><td>Avanzo di cassa iniziale</td>
                      <td class="mono text-right">{{ fmt(flussiCassa.avanzoInizialeCassa) }}</td></tr>
                    <tr><td>Versamenti dei condòmini</td>
                      <td class="mono text-right">{{ fmt(flussiCassa.versamentiCondomini) }}</td></tr>
                  </tbody>
                  <tfoot>
                    <tr class="row-total"><td><strong>Totale incassi</strong></td>
                      <td class="mono text-right"><strong>{{ fmt(flussiCassa.totaleIncassi) }}</strong></td></tr>
                  </tfoot>
                </table>
              </div>
            </div>
            <!-- PAGAMENTI -->
            <div class="card">
              <h3 class="section-title">Pagamenti</h3>
              <div class="table-wrap">
                <table>
                  <tbody>
                    <tr><td>Spese sostenute dell'esercizio</td>
                      <td class="mono text-right">{{ fmt(flussiCassa.pagamentiEsercizioCorrente) }}</td></tr>
                    <tr><td>Spese di esercizi precedenti</td>
                      <td class="mono text-right">{{ fmt(flussiCassa.pagamentiEserciziPrecedenti) }}</td></tr>
                    <tr v-if="flussiCassa.usciteIndividuali"><td>Uscite individuali</td>
                      <td class="mono text-right">{{ fmt(flussiCassa.usciteIndividuali) }}</td></tr>
                  </tbody>
                  <tfoot>
                    <tr class="row-total"><td><strong>Totale pagamenti</strong></td>
                      <td class="mono text-right"><strong>{{ fmt(flussiCassa.totalePagamenti) }}</strong></td></tr>
                    <tr><td>Avanzo di cassa finale</td>
                      <td class="mono text-right"
                          :class="flussiCassa.avanzoFinaleCassa >= 0 ? 'text-green' : 'text-red'">
                        <strong>{{ fmt(flussiCassa.avanzoFinaleCassa) }}</strong></td></tr>
                  </tfoot>
                </table>
              </div>
            </div>
          </div>
        </template>
      </template>

      <!-- ══ TAB: Situazione patrimoniale (cassa) ═══════════════════════════ -->
      <template v-else-if="activeTab === 'patrimoniale'">
        <div v-if="loadingPatrimoniale" class="card"><div class="loading-state"><div class="spinner"></div></div></div>
        <template v-else-if="patrimoniale">
          <div v-if="Math.abs(patrimoniale.sbilancio) >= 0.01" class="warning-banner" style="margin-bottom:12px">
            ⚠ Il bilancio non è in pareggio (scostamento {{ fmt(patrimoniale.sbilancio) }}).
            Verifica i saldi iniziali dei conti e i fondi.
          </div>
          <div class="balance-grid">
            <!-- ATTIVITÀ -->
            <div class="card">
              <h3 class="section-title">Attività</h3>
              <div class="table-wrap">
                <table>
                  <tbody>
                    <tr><td>Crediti verso condòmini</td>
                      <td class="mono text-right">{{ fmt(patrimoniale.creditiVersoCondomini) }}</td></tr>
                    <tr v-for="r in patrimoniale.disponibilita" :key="'d-'+r.accountId">
                      <td><span class="mono text-muted" style="margin-right:6px">{{ r.accountCode }}</span>{{ r.accountName }}</td>
                      <td class="mono text-right">{{ fmt(r.amount) }}</td></tr>
                  </tbody>
                  <tfoot>
                    <tr class="row-total"><td><strong>Totale attività</strong></td>
                      <td class="mono text-right"><strong>{{ fmt(patrimoniale.totaleAttivita) }}</strong></td></tr>
                  </tfoot>
                </table>
              </div>
            </div>
            <!-- PASSIVITÀ -->
            <div class="card">
              <h3 class="section-title">Passività</h3>
              <div class="table-wrap">
                <table>
                  <tbody>
                    <tr><td>Debiti verso condòmini</td>
                      <td class="mono text-right">{{ fmt(patrimoniale.debitiVersoCondomini) }}</td></tr>
                    <tr><td>Debiti verso terzi</td>
                      <td class="mono text-right">{{ fmt(patrimoniale.debitiVersoTerzi) }}</td></tr>
                    <tr v-for="r in patrimoniale.fondi" :key="'f-'+r.accountId">
                      <td><span class="mono text-muted" style="margin-right:6px">{{ r.accountCode }}</span>{{ r.accountName }}</td>
                      <td class="mono text-right">{{ fmt(r.amount) }}</td></tr>
                  </tbody>
                  <tfoot>
                    <tr class="row-total"><td><strong>Totale passività</strong></td>
                      <td class="mono text-right"><strong>{{ fmt(patrimoniale.totalePassivita) }}</strong></td></tr>
                  </tfoot>
                </table>
              </div>
            </div>
          </div>
        </template>
      </template>

      <!-- ══ TAB: Conguaglio per unità ══════════════════════════════════════ -->
      <template v-else-if="activeTab === 'conguaglio'">
        <div v-if="loading" class="card"><div class="loading-state"><div class="spinner"></div></div></div>

        <div v-else-if="conguaglioError" class="card">
          <div class="empty-state">
            <div class="empty-icon">⚠</div>
            <div>{{ conguaglioError }}</div>
          </div>
        </div>

        <template v-else-if="conguaglio">
          <!-- Riepilogo globale -->
          <div class="card summary-card">
            <div class="summary-meta">
              <div><span class="meta-label">Esercizio</span><strong>{{ conguaglio.fiscalYearCode }}</strong></div>
              <div v-if="conguaglio.approvalDate">
                <span class="meta-label">Approvazione consuntivo</span>
                <span>{{ fmtDate(conguaglio.approvalDate) }}</span>
              </div>
            </div>
            <div class="kpi-row">
              <div class="kpi">
                <span class="kpi-label">Spese consuntive totali</span>
                <span class="kpi-value">{{ fmt(conguaglio.totalExpenses) }}</span>
              </div>
              <div class="kpi">
                <span class="kpi-label">Rate incassate (preventivo)</span>
                <span class="kpi-value text-green">{{ fmt(conguaglio.totalPaid) }}</span>
              </div>
              <div class="kpi">
                <span class="kpi-label">Saldo residuo globale</span>
                <span class="kpi-value"
                      :class="conguaglio.globalBalance > 0 ? 'text-red' : conguaglio.globalBalance < 0 ? 'text-green' : 'text-muted'">
                  {{ conguaglio.globalBalance >= 0 ? '+' : '' }}{{ fmt(conguaglio.globalBalance) }}
                </span>
              </div>
            </div>
          </div>

          <!-- Tabella per unità / gruppi -->
          <div class="card">
            <div class="table-wrap">
              <table>
                <thead>
                  <tr>
                    <th>Unità / Gruppo</th>
                    <th class="text-right">Millesimi</th>
                    <th class="text-right">Saldo apertura</th>
                    <th class="text-right">Rate addebitate</th>
                    <th class="text-right">Rate incassate</th>
                    <th class="text-right">Quota consuntiva</th>
                    <th class="text-right">Conguaglio</th>
                    <th class="text-right">Saldo finale</th>
                    <th class="text-center">Stato</th>
                  </tr>
                </thead>
                <tbody>
                  <template v-for="u in enrichedUnits" :key="u.isGroup ? 'g-' + u.billingGroupId : 'u-' + u.unitId">
                    <!-- Riga principale (gruppo o unità singola) -->
                    <tr :class="u.isGroup ? 'row-group' : ''"
                        @click="u.isGroup && toggleGroup(u.billingGroupId)"
                        :style="u.isGroup ? 'cursor:pointer' : ''">
                      <td>
                        <span v-if="u.isGroup" class="expand-icon">{{ expandedGroups.has(u.billingGroupId) ? '▾' : '▸' }}</span>
                        <span v-if="u.isGroup" class="badge badge-muted group-badge">Gruppo</span>
                        <span v-else class="mono text-secondary" style="margin-right:6px">{{ u.unitInternalNumber }}</span>
                        {{ u.unitDescription }}
                      </td>
                      <td class="mono text-right text-secondary">{{ u.millesimal.toFixed(3) }}</td>
                      <td class="mono text-right"
                          :class="u.openingBalance > 0 ? 'text-red' : u.openingBalance < 0 ? 'text-green' : 'text-muted'">
                        {{ fmt(u.openingBalance) }}
                      </td>
                      <td class="mono text-right">{{ fmt(u.rateAddebitate) }}</td>
                      <td class="mono text-right text-green">{{ fmt(u.rateIncassate) }}</td>
                      <td class="mono text-right">{{ fmt(u.quotaConsuntiva) }}</td>
                      <td class="mono text-right"
                          :class="u.saldoConguaglio > 0 ? 'text-red' : u.saldoConguaglio < 0 ? 'text-green' : 'text-muted'">
                        {{ u.saldoConguaglio >= 0 ? '+' : '' }}{{ fmt(u.saldoConguaglio) }}
                      </td>
                      <td class="mono text-right"
                          :class="u.closingBalance > 0 ? 'text-red' : u.closingBalance < 0 ? 'text-green' : 'text-muted'">
                        <strong>{{ u.closingBalance >= 0 ? '+' : '' }}{{ fmt(u.closingBalance) }}</strong>
                      </td>
                      <td class="text-center">
                        <span class="badge"
                              :class="u.saldoType === 'Debit' ? 'badge-red' : u.saldoType === 'Credit' ? 'badge-green' : 'badge-muted'">
                          {{ u.saldoType === 'Debit' ? 'A debito' : u.saldoType === 'Credit' ? 'A credito' : 'Pari' }}
                        </span>
                      </td>
                    </tr>
                    <!-- Sotto-righe unità del gruppo (espandibili) -->
                    <template v-if="u.isGroup && expandedGroups.has(u.billingGroupId)">
                      <tr v-for="sub in enrichedSubUnits(u)" :key="'sub-' + sub.unitId" class="row-subunit">
                        <td style="padding-left:2.25rem">
                          <span class="mono text-secondary" style="margin-right:6px">{{ sub.unitInternalNumber }}</span>
                          {{ sub.unitDescription }}
                        </td>
                        <td class="mono text-right text-muted">{{ sub.millesimal.toFixed(3) }}</td>
                        <td class="mono text-right text-muted"
                            :class="sub.openingBalance > 0 ? 'text-red' : sub.openingBalance < 0 ? 'text-green' : 'text-muted'">
                          {{ fmt(sub.openingBalance) }}
                        </td>
                        <td class="mono text-right text-muted">{{ fmt(sub.rateAddebitate) }}</td>
                        <td class="mono text-right text-green" style="opacity:.8">{{ fmt(sub.rateIncassate) }}</td>
                        <td class="mono text-right text-muted">{{ fmt(sub.quotaConsuntiva) }}</td>
                        <td class="mono text-right text-muted">{{ sub.saldoConguaglio >= 0 ? '+' : '' }}{{ fmt(sub.saldoConguaglio) }}</td>
                        <td class="mono text-right text-muted">{{ sub.closingBalance >= 0 ? '+' : '' }}{{ fmt(sub.closingBalance) }}</td>
                        <td></td>
                      </tr>
                    </template>
                  </template>
                </tbody>
                <tfoot>
                  <tr class="row-total">
                    <td><strong>Totale</strong></td>
                    <td class="mono text-right text-secondary">
                      {{ conguaglio.units.reduce((s, u) => s + u.millesimal, 0).toFixed(3) }}
                    </td>
                    <td class="mono text-right">{{ fmt(totOpeningBalance) }}</td>
                    <td class="mono text-right">{{ fmt(totRateAddebitate) }}</td>
                    <td class="mono text-right text-green">{{ fmt(totRateIncassate) }}</td>
                    <td class="mono text-right"><strong>{{ fmt(conguaglio.totalExpenses) }}</strong></td>
                    <td class="mono text-right"
                        :class="totConguaglio > 0 ? 'text-red' : totConguaglio < 0 ? 'text-green' : 'text-muted'">
                      {{ totConguaglio >= 0 ? '+' : '' }}{{ fmt(totConguaglio) }}
                    </td>
                    <td class="mono text-right"
                        :class="conguaglio.globalBalance > 0 ? 'text-red' : conguaglio.globalBalance < 0 ? 'text-green' : 'text-muted'">
                      <strong>{{ conguaglio.globalBalance >= 0 ? '+' : '' }}{{ fmt(conguaglio.globalBalance) }}</strong>
                    </td>
                    <td></td>
                  </tr>
                </tfoot>
              </table>
            </div>
          </div>
        </template>
      </template>

      <!-- ══ TAB: Dettaglio per conto ═══════════════════════════════════════ -->
      <template v-else-if="activeTab === 'conti'">
        <div v-if="loadingDetail" class="card"><div class="loading-state"><div class="spinner"></div></div></div>

        <div v-else-if="!consuntivoDetail" class="card">
          <div class="empty-state">
            <div class="empty-icon">◎</div>
            <div>Nessun dettaglio disponibile. Verifica che il consuntivo sia approvato e che le spese siano state ripartite.</div>
          </div>
        </div>

        <template v-else>
          <!-- Vista per conto -->
          <div class="card">
            <div class="table-wrap">
              <table>
                <thead>
                  <tr>
                    <th style="min-width:260px">Conto</th>
                    <th class="text-right" style="min-width:140px">Importo totale</th>
                    <th class="text-right text-secondary" style="min-width:80px">% sul tot.</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="acc in consuntivoDetail.accounts" :key="acc.accountId"
                      :class="acc.expenses.length ? 'row-has-detail' : ''">
                    <td>
                      <span :style="{ paddingLeft: (acc.level * 16) + 'px' }">
                        <span class="mono text-muted" style="margin-right:6px">{{ acc.accountCode }}</span>
                        {{ acc.accountName }}
                      </span>
                    </td>
                    <td class="mono text-right">{{ fmt(acc.totalAmount) }}</td>
                    <td class="mono text-right text-secondary">
                      {{ consuntivoDetail.accounts.filter(a => a.level === 0).reduce((s,a) => s + a.totalAmount, 0) > 0
                         ? (acc.totalAmount / consuntivoDetail.accounts.filter(a => a.level === 0).reduce((s,a) => s + a.totalAmount, 0) * 100).toFixed(1) + '%'
                         : '—' }}
                    </td>
                  </tr>
                </tbody>
                <tfoot>
                  <tr class="row-total">
                    <td><strong>Totale spese</strong></td>
                    <td class="mono text-right">
                      <strong>{{ fmt(consuntivoDetail.accounts.filter(a => a.level === 0).reduce((s,a) => s + a.totalAmount, 0)) }}</strong>
                    </td>
                    <td></td>
                  </tr>
                </tfoot>
              </table>
            </div>
          </div>

          <!-- Vista per unità / gruppi (se ci sono ripartizioni) -->
          <template v-if="consuntivoDetail.hasAllocations && consuntivoDetail.units.length">
            <h3 class="section-title">Ripartizione per unità</h3>
            <div class="card">
              <div class="table-wrap">
                <table>
                  <thead>
                    <tr>
                      <th>Unità / Gruppo</th>
                      <th class="text-right">Spese ripartite</th>
                      <th class="text-right">Rate addebitate</th>
                      <th class="text-right">Rate pagate</th>
                      <th class="text-right">Insoluto rate</th>
                    </tr>
                  </thead>
                  <tbody>
                    <template v-for="u in consuntivoDetail.units" :key="u.isGroup ? 'g-' + u.billingGroupId : 'u-' + u.unitId">
                      <tr :class="u.isGroup ? 'row-group' : ''"
                          @click="u.isGroup && toggleDetailGroup(u.billingGroupId)"
                          :style="u.isGroup ? 'cursor:pointer' : ''">
                        <td>
                          <span v-if="u.isGroup" class="expand-icon">{{ expandedDetailGroups.has(u.billingGroupId) ? '▾' : '▸' }}</span>
                          <span v-if="u.isGroup" class="badge badge-muted group-badge">Gruppo</span>
                          {{ u.unitName }}
                        </td>
                        <td class="mono text-right">{{ fmt(u.total) }}</td>
                        <td class="mono text-right">{{ fmt(u.amountDue) }}</td>
                        <td class="mono text-right text-green">{{ fmt(u.amountPaid) }}</td>
                        <td class="mono text-right"
                            :class="u.balance > 0 ? 'text-red' : u.balance < 0 ? 'text-green' : 'text-muted'">
                          {{ u.balance > 0 ? '+' : '' }}{{ fmt(u.balance) }}
                        </td>
                      </tr>
                      <template v-if="u.isGroup && expandedDetailGroups.has(u.billingGroupId)">
                        <tr v-for="sub in u.units" :key="'dsub-' + sub.unitId" class="row-subunit">
                          <td style="padding-left:2.25rem">{{ sub.unitName }}</td>
                          <td class="mono text-right text-muted">{{ fmt(sub.total) }}</td>
                          <td class="mono text-right text-muted">{{ fmt(sub.amountDue) }}</td>
                          <td class="mono text-right text-green" style="opacity:.8">{{ fmt(sub.amountPaid) }}</td>
                          <td class="mono text-right text-muted">{{ fmt(sub.balance) }}</td>
                        </tr>
                      </template>
                    </template>
                  </tbody>
                  <tfoot>
                    <tr class="row-total">
                      <td><strong>Totale</strong></td>
                      <td class="mono text-right"><strong>{{ fmt(consuntivoDetail.units.reduce((s,u) => s + u.total, 0)) }}</strong></td>
                      <td class="mono text-right"><strong>{{ fmt(consuntivoDetail.units.reduce((s,u) => s + u.amountDue, 0)) }}</strong></td>
                      <td class="mono text-right text-green"><strong>{{ fmt(consuntivoDetail.units.reduce((s,u) => s + u.amountPaid, 0)) }}</strong></td>
                      <td class="mono text-right"
                          :class="consuntivoDetail.units.reduce((s,u) => s + u.balance, 0) > 0 ? 'text-red' : 'text-green'">
                        <strong>{{ fmt(consuntivoDetail.units.reduce((s,u) => s + u.balance, 0)) }}</strong>
                      </td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            </div>
          </template>
        </template>
      </template>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { useAppStore } from '@/stores/app'
import { budgetApi, fiscalYearApi, unitApi } from '@/services/api'
import {
  exportContoEconomicoPdf, exportContoEconomicoExcel,
  exportFlussiCassaPdf, exportFlussiCassaExcel,
  exportSituazionePatrimonialePdf, exportSituazionePatrimonialeExcel,
} from '@/composables/useBilancioExport'

const store = useAppStore()

const fiscalYears          = computed(() => store.fiscalYears ?? [])
const selectedFiscalYearId = ref(null)
const activeTab            = ref('economico')

// ── Data ────────────────────────────────────────────────────────────────────
const loading              = ref(false)
const loadingDetail        = ref(false)
const consuntivo           = ref(null)
const conguaglio           = ref(null)
const conguaglioError      = ref(null)
const openingBalances      = ref([])
const consuntivoDetail     = ref(null)

// ── Bilancio (conto economico / flussi di cassa / situazione patrimoniale) ──
const loadingEconomico     = ref(false)
const loadingCassa         = ref(false)
const loadingPatrimoniale  = ref(false)
const contoEconomico       = ref(null)
const flussiCassa          = ref(null)
const patrimoniale         = ref(null)
const expandedGroups       = ref(new Set())
const expandedDetailGroups = ref(new Set())

// ── Load ────────────────────────────────────────────────────────────────────
function toggleGroup(id) {
  const s = new Set(expandedGroups.value)
  s.has(id) ? s.delete(id) : s.add(id)
  expandedGroups.value = s
}

function toggleDetailGroup(id) {
  const s = new Set(expandedDetailGroups.value)
  s.has(id) ? s.delete(id) : s.add(id)
  expandedDetailGroups.value = s
}

async function load() {
  if (!selectedFiscalYearId.value) return
  loading.value              = true
  conguaglio.value           = null
  conguaglioError.value      = null
  consuntivo.value           = null
  openingBalances.value      = []
  expandedGroups.value       = new Set()
  expandedDetailGroups.value = new Set()
  contoEconomico.value       = null
  flussiCassa.value          = null
  patrimoniale.value         = null

  try {
    const [budgetsRes, conguaglioRes, balancesRes] = await Promise.allSettled([
      budgetApi.getByFiscalYear(selectedFiscalYearId.value),
      fiscalYearApi.getConguaglio(selectedFiscalYearId.value),
      unitApi.getOpeningBalancesByFiscalYear(selectedFiscalYearId.value),
    ])

    if (budgetsRes.status === 'fulfilled') {
      const all = budgetsRes.value.data ?? []
      consuntivo.value = all.find(b => b.type === 2 && (b.statusId === 2 || b.statusId === 3)) ?? null
    }

    if (conguaglioRes.status === 'fulfilled') {
      conguaglio.value = conguaglioRes.value.data
    } else {
      const err = conguaglioRes.reason
      conguaglioError.value = err?.response?.data?.message
        ?? err?.response?.data?.title
        ?? 'Consuntivo non ancora approvato.'
    }

    if (balancesRes.status === 'fulfilled') {
      openingBalances.value = balancesRes.value.data ?? []
    }
  } finally {
    loading.value = false
  }
}

async function loadContoEconomico() {
  if (!selectedFiscalYearId.value) return
  loadingEconomico.value = true
  try {
    const { data } = await fiscalYearApi.getContoEconomico(selectedFiscalYearId.value)
    contoEconomico.value = data
  } catch {
    // global error handler
  } finally {
    loadingEconomico.value = false
  }
}

async function loadFlussiCassa() {
  if (!selectedFiscalYearId.value) return
  loadingCassa.value = true
  try {
    const { data } = await fiscalYearApi.getFlussiCassa(selectedFiscalYearId.value)
    flussiCassa.value = data
  } catch {
    // global error handler
  } finally {
    loadingCassa.value = false
  }
}

async function loadPatrimoniale() {
  if (!selectedFiscalYearId.value) return
  loadingPatrimoniale.value = true
  try {
    const { data } = await fiscalYearApi.getSituazionePatrimoniale(selectedFiscalYearId.value)
    patrimoniale.value = data
  } catch {
    // global error handler
  } finally {
    loadingPatrimoniale.value = false
  }
}

async function loadDetail() {
  if (!consuntivo.value) return
  loadingDetail.value    = true
  consuntivoDetail.value = null
  try {
    const { data } = await budgetApi.consuntivoDetail(consuntivo.value.id)
    consuntivoDetail.value = data
  } catch {
    // leave null — empty state shown
  } finally {
    loadingDetail.value = false
  }
}

// ── Enrich a single conguaglio row with openingBalance data ─────────────────
function enrichRow(u, balMap) {
  if (u.isGroup) {
    // Per i gruppi, somma i saldi di apertura/chiusura dalle sotto-unità
    const subEnriched = (u.units ?? []).map(sub => enrichRow(sub, balMap))
    return {
      ...u,
      openingBalance:  subEnriched.reduce((s, r) => s + r.openingBalance, 0),
      rateAddebitate:  subEnriched.reduce((s, r) => s + r.rateAddebitate, 0),
      rateIncassate:   subEnriched.reduce((s, r) => s + r.rateIncassate, 0),
      saldoConguaglio: subEnriched.reduce((s, r) => s + r.saldoConguaglio, 0),
      closingBalance:  subEnriched.reduce((s, r) => s + r.closingBalance, 0),
      _subEnriched:    subEnriched,
    }
  }
  const bal = balMap[u.unitId]
  return {
    ...u,
    openingBalance:  bal?.openingBalance  ?? 0,
    rateAddebitate:  bal?.rateAddebitate  ?? u.alreadyPaid,
    rateIncassate:   bal?.rateIncassate   ?? u.alreadyPaid,
    saldoConguaglio: bal?.saldoConguaglio ?? u.saldo,
    closingBalance:  bal?.closingBalance  ?? u.saldo,
  }
}

const enrichedUnits = computed(() => {
  if (!conguaglio.value) return []
  const balMap = Object.fromEntries(openingBalances.value.map(b => [b.unitId, b]))
  return conguaglio.value.units.map(u => enrichRow(u, balMap))
})

function enrichedSubUnits(groupRow) {
  return groupRow._subEnriched ?? []
}

// ── Totals for footer ───────────────────────────────────────────────────────
const totOpeningBalance  = computed(() => enrichedUnits.value.reduce((s, u) => s + u.openingBalance, 0))
const totRateAddebitate  = computed(() => enrichedUnits.value.reduce((s, u) => s + u.rateAddebitate, 0))
const totRateIncassate   = computed(() => enrichedUnits.value.reduce((s, u) => s + u.rateIncassate, 0))
const totConguaglio      = computed(() => enrichedUnits.value.reduce((s, u) => s + u.saldoConguaglio, 0))

// ── Watchers ────────────────────────────────────────────────────────────────
watch(selectedFiscalYearId, async () => {
  activeTab.value        = 'economico'
  consuntivoDetail.value = null
  await load()
  await loadContoEconomico()
})

watch(activeTab, async (tab) => {
  if (tab === 'conti'         && !consuntivoDetail.value) await loadDetail()
  if (tab === 'economico'     && !contoEconomico.value)   await loadContoEconomico()
  if (tab === 'cassa'         && !flussiCassa.value)      await loadFlussiCassa()
  if (tab === 'patrimoniale'  && !patrimoniale.value)     await loadPatrimoniale()
})

watch(fiscalYears, (fys) => {
  if (fys.length && !selectedFiscalYearId.value) {
    selectedFiscalYearId.value = fys[0].id
  }
}, { immediate: true })

// ── Formatters ──────────────────────────────────────────────────────────────
const fmt     = (v) => v != null ? '€\u00a0' + Number(v).toLocaleString('it-IT', { minimumFractionDigits: 2 }) : '—'
const fmtDate = (d) => d ? new Date(d).toLocaleDateString('it-IT') : '—'

// ── Export PDF / Excel (solo prospetti di bilancio) ─────────────────────────
const exportableData = computed(() => {
  if (activeTab.value === 'economico')    return contoEconomico.value
  if (activeTab.value === 'cassa')        return flussiCassa.value
  if (activeTab.value === 'patrimoniale') return patrimoniale.value
  return null
})
const canExport = computed(() => !!exportableData.value)

function exportPdf() {
  const d = exportableData.value
  if (!d) return
  if (activeTab.value === 'economico')    exportContoEconomicoPdf(d)
  if (activeTab.value === 'cassa')        exportFlussiCassaPdf(d)
  if (activeTab.value === 'patrimoniale') exportSituazionePatrimonialePdf(d)
}

function exportExcel() {
  const d = exportableData.value
  if (!d) return
  if (activeTab.value === 'economico')    exportContoEconomicoExcel(d)
  if (activeTab.value === 'cassa')        exportFlussiCassaExcel(d)
  if (activeTab.value === 'patrimoniale') exportSituazionePatrimonialeExcel(d)
}

async function refresh() {
  selectedFiscalYearId.value = null
  await store.loadFiscalYears()
}

onMounted(async () => {
  // Carica gli esercizi se lo store non li ha ancora: il watch su fiscalYears
  // selezionerà il primo e avvierà il caricamento del prospetto.
  if (!fiscalYears.value.length) await store.loadFiscalYears()
})
onUnmounted(() => window.removeEventListener('app:refresh', refresh))
window.addEventListener('app:refresh', refresh)
</script>

<style scoped>
.toolbar {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 1rem;
  flex-wrap: wrap;
}

/* ── Export toolbar ───────────────────────────────────────────────────────── */
.export-toolbar {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  margin-bottom: 1rem;
}

/* ── Tab pills ────────────────────────────────────────────────────────────── */
.tab-pills { display: flex; gap: 4px; margin-left: auto; }
.tab-pill {
  padding: 6px 14px;
  border-radius: 6px;
  border: 1px solid var(--border);
  background: transparent;
  color: var(--text-secondary);
  cursor: pointer;
  font-size: 0.875rem;
  transition: background 0.15s, color 0.15s;
}
.tab-pill.active {
  background: var(--accent);
  color: #fff;
  border-color: var(--accent);
}

/* ── Summary card ────────────────────────────────────────────────────────── */
.summary-card { padding: 1.25rem 1.5rem 1.5rem; margin-bottom: 1.25rem; }
.summary-meta {
  display: flex;
  gap: 2rem;
  font-size: 0.875rem;
  flex-wrap: wrap;
  margin-bottom: 1rem;
}
.meta-label {
  font-size: 0.75rem;
  color: var(--text-muted);
  margin-right: 6px;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}
.kpi-row { display: flex; gap: 2rem; flex-wrap: wrap; }
.kpi { display: flex; flex-direction: column; gap: 2px; }
.kpi-label { font-size: 0.75rem; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.04em; }
.kpi-value { font-size: 1.1rem; font-weight: 600; font-variant-numeric: tabular-nums; }

/* ── Table rows ─────────────────────────────────────────────────────────── */
.row-total td {
  border-top: 2px solid var(--border);
  background: var(--bg-base);
}
.row-has-detail { background: var(--bg-surface); }
.row-group td {
  background: var(--bg-base);
  font-weight: 600;
}
.row-group:hover td { background: var(--accent-glow, #eef2ff); }
.row-subunit td {
  background: var(--bg-surface);
  font-size: 0.845rem;
  color: var(--text-secondary);
}
.expand-icon {
  display: inline-block;
  width: 1rem;
  font-size: 0.75rem;
  color: var(--text-muted);
  margin-right: 4px;
}
.group-badge {
  font-size: 0.65rem;
  margin-right: 6px;
  vertical-align: middle;
}

/* ── Section title ──────────────────────────────────────────────────────── */
.section-title {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--text-secondary);
  margin: 1.25rem 0 0.5rem;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

/* ── Balance grid (Entrate/Uscite, Incassi/Pagamenti, Attività/Passività) ── */
.balance-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1.25rem;
  align-items: start;
}
.balance-grid .card { padding: 1.25rem 1.5rem; }
.balance-grid .section-title {
  margin: 0 0 0.75rem;
  padding-bottom: 0.5rem;
  border-bottom: 1px solid var(--border);
}
/* Righe dei prospetti con più respiro */
.balance-grid table { width: 100%; }
.balance-grid .table-wrap td {
  padding: 0.55rem 0.25rem;
  vertical-align: middle;
}
.balance-grid tfoot td { padding-top: 0.7rem; }
.balance-grid .row-total td { border-top: 2px solid var(--border); }
@media (max-width: 880px) {
  .balance-grid { grid-template-columns: 1fr; }
}

/* ── Colors ─────────────────────────────────────────────────────────────── */
.text-green { color: var(--accent-green); }
.text-red   { color: var(--accent-red);   }
.badge-red  { background: rgba(239,68,68,.12); color: var(--accent-red); }

/* ── Warning ────────────────────────────────────────────────────────────── */
.warning-banner {
  border: 1px solid #f59e0b;
  background: rgba(245,158,11,.08);
  color: #92400e;
  border-radius: 6px;
  padding: 10px 14px;
  font-size: 0.875rem;
}
</style>
