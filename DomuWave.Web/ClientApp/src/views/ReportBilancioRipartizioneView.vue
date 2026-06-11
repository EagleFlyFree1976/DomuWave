<template>
  <div class="report-page">
    <div class="toolbar no-print">
      <h1>Bilancio di ripartizione</h1>
      <select class="form-select" v-model.number="selectedFiscalYearId" style="min-width:220px">
        <option :value="null" disabled>Seleziona esercizio…</option>
        <option v-for="fy in fiscalYears" :key="fy.id" :value="fy.id">
          {{ fy.code }}{{ fy.description ? ' – ' + fy.description : '' }}
        </option>
      </select>
      <button class="btn btn-ghost" :disabled="!report || loading" @click="exportExcel">⬇ Excel</button>
      <button class="btn btn-ghost" :disabled="!report || loading" @click="exportPdf">📄 PDF</button>
      <button class="btn btn-ghost" :disabled="!report || loading" @click="printReport">🖨 Stampa</button>

      <template v-if="report && report.isEditable">
        <template v-if="!editMode">
          <button class="btn btn-ghost" :disabled="loading" @click="enterEdit">✏ Modifica</button>
          <button class="btn btn-ghost" :disabled="loading || saving || !report.hasOverrides" @click="resetOverrides">↺ Ripristina automatico</button>
        </template>
        <template v-else>
          <button class="btn btn-primary" :disabled="saving" @click="saveEdits">
            <span v-if="saving" class="spinner" style="width:14px;height:14px"></span> Salva
          </button>
          <button class="btn btn-ghost" :disabled="saving" @click="cancelEdit">Annulla</button>
        </template>
      </template>
    </div>

    <div v-if="!selectedFiscalYearId" class="card empty-state no-print">
      Seleziona un esercizio fiscale per generare il report.
    </div>
    <div v-else-if="loading" class="card loading-state"><div class="spinner"></div></div>

    <div v-else-if="report" class="report-sheet">
      <div class="report-header">
        <h2>Bilancio di ripartizione</h2>
        <div class="report-meta">
          <span><strong>Condominio:</strong> {{ report.condominiumName || '—' }}</span>
          <span><strong>Esercizio:</strong> {{ report.fiscalYearCode || '—' }}</span>
          <span class="report-gen">Generato il {{ today }}</span>
        </div>
      </div>

      <div v-if="report.hasOverrides && Math.abs(scostamento) >= 0.005" class="scostamento-banner no-print">
        ⚠ Il totale modificato a mano differisce dal totale calcolato di
        <strong>{{ fmt(scostamento) }}</strong>
        (calcolato {{ fmt(report.totaleCalcolato) }} → effettivo {{ fmt(report.totaleEffettivo) }}).
      </div>
      <div v-else-if="report.hasOverrides" class="scostamento-banner scostamento-ok no-print">
        ✓ Sono presenti modifiche manuali; il totale resta allineato a quello calcolato.
      </div>

      <div v-if="!report.rows.length" class="empty-state">Nessun dato per questo esercizio.</div>

      <div v-else class="table-wrap no-print">
        <table class="bilancio-table">
          <thead>
            <tr>
              <th rowspan="2" class="col-nom">Nominativo</th>
              <th rowspan="2">Tipo</th>
              <th v-for="c in report.consumoColumns" :key="'ch'+c.consumptionTypeId" colspan="2" class="grp-consumo">
                {{ c.name }}<span v-if="c.unitOfMeasure" class="uom"> ({{ c.unitOfMeasure }})</span>
              </th>
              <th v-for="m in report.millesimaleColumns" :key="'mh'+m.millesimalTableId" colspan="2" class="grp-mill">
                {{ m.name }}
              </th>
              <th rowspan="2" class="col-num">Dirette</th>
              <th rowspan="2" class="col-num">Accrediti</th>
              <th rowspan="2" class="col-num">Totale speso</th>
              <th rowspan="2" class="col-num">Versato</th>
              <th rowspan="2" class="col-num">Saldo</th>
            </tr>
            <tr>
              <template v-for="c in report.consumoColumns" :key="'sub'+c.consumptionTypeId">
                <th class="col-num sub">Q.tà</th>
                <th class="col-num sub">Importo</th>
              </template>
              <template v-for="m in report.millesimaleColumns" :key="'subm'+m.millesimalTableId">
                <th class="col-num sub">Mill.</th>
                <th class="col-num sub">Importo</th>
              </template>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(r, i) in displayRows" :key="i" :class="{ 'row-tenant': r.rowType === 'Inquilini' }">
              <td>{{ r.nominativo || '—' }}</td>
              <td class="text-secondary">{{ r.rowType }}</td>
              <template v-for="c in report.consumoColumns" :key="'c'+i+'_'+c.consumptionTypeId">
                <td class="col-num mono">{{ cellQta(r, c.consumptionTypeId) }}</td>
                <td class="col-num mono" :class="{ 'cell-ovr': findC(r, c.consumptionTypeId)?.isOverridden }">
                  <input v-if="editMode" class="cell-input" type="number" step="0.01"
                         :value="findC(r, c.consumptionTypeId)?.importo ?? 0"
                         @input="e => { const cc = findC(r, c.consumptionTypeId); if (cc) { cc.importo = e.target.value; markConsumo(r, c.consumptionTypeId) } }" />
                  <template v-else>{{ cellImp(r, c.consumptionTypeId) }}</template>
                </td>
              </template>
              <template v-for="m in report.millesimaleColumns" :key="'m'+i+'_'+m.millesimalTableId">
                <td class="col-num mono text-secondary">{{ cellMillVal(r, m.millesimalTableId) }}</td>
                <td class="col-num mono" :class="{ 'cell-ovr': r.millesimali.find(x => x.millesimalTableId === m.millesimalTableId)?.isOverridden }">
                  <input v-if="editMode" class="cell-input" type="number" step="0.01"
                         :value="r.millesimali.find(x => x.millesimalTableId === m.millesimalTableId)?.importo ?? 0"
                         @input="e => { const mm = r.millesimali.find(x => x.millesimalTableId === m.millesimalTableId); if (mm) { mm.importo = e.target.value; markMill(r, m.millesimalTableId) } }" />
                  <template v-else>{{ cellMill(r, m.millesimalTableId) }}</template>
                </td>
              </template>
              <td class="col-num mono" :class="{ 'cell-ovr': r.diretteOverridden }">
                <input v-if="editMode" class="cell-input" type="number" step="0.01"
                       :value="r.dirette" @input="e => { r.dirette = e.target.value; r.diretteOverridden = true }" />
                <template v-else>{{ fmt(r.dirette) }}</template>
              </td>
              <td class="col-num mono text-green" :class="{ 'cell-ovr': r.accreditiOverridden }">
                <input v-if="editMode" class="cell-input" type="number" step="0.01"
                       :value="r.accrediti" @input="e => { r.accrediti = e.target.value; r.accreditiOverridden = true }" />
                <template v-else>{{ fmt(r.accrediti) }}</template>
              </td>
              <td class="col-num mono"><strong>{{ fmt(rowTotale(r)) }}</strong></td>
              <td class="col-num mono" :class="{ 'cell-ovr': r.versatoOverridden }">
                <input v-if="editMode" class="cell-input" type="number" step="0.01"
                       :value="r.versato" @input="e => { r.versato = e.target.value; r.versatoOverridden = true }" />
                <template v-else>{{ fmt(r.versato) }}</template>
              </td>
              <td class="col-num mono" :class="rowSaldo(r) >= 0 ? 'text-green' : 'text-red'">{{ fmt(rowSaldo(r)) }}</td>
            </tr>
          </tbody>
          <tfoot>
            <tr class="totals-row">
              <td colspan="2">TOTALI</td>
              <template v-for="c in report.consumoColumns" :key="'tc'+c.consumptionTypeId">
                <td class="col-num mono">{{ totQta(c.consumptionTypeId) }}</td>
                <td class="col-num mono">{{ totImp(c.consumptionTypeId) }}</td>
              </template>
              <template v-for="m in report.millesimaleColumns" :key="'tm'+m.millesimalTableId">
                <td class="col-num mono text-secondary">{{ totMillVal(m.millesimalTableId) }}</td>
                <td class="col-num mono">{{ totMill(m.millesimalTableId) }}</td>
              </template>
              <td class="col-num mono">{{ fmt(report.totals.dirette) }}</td>
              <td class="col-num mono">{{ fmt(report.totals.accrediti) }}</td>
              <td class="col-num mono"><strong>{{ fmt(report.totals.totale) }}</strong></td>
              <td class="col-num mono">{{ fmt(report.totals.versato) }}</td>
              <td class="col-num mono">{{ fmt(report.totals.saldo) }}</td>
            </tr>
          </tfoot>
        </table>
      </div>

      <!-- ── Stampa: una tabella per ogni gruppo di 2 colonne dinamiche ───────── -->
      <div v-if="report.rows.length" class="print-only">
        <div v-for="(page, pi) in printPages" :key="'pp'+pi" class="print-page">
          <div class="print-page-header">
            <h2>Bilancio di ripartizione</h2>
            <div class="report-meta">
              <span><strong>Condominio:</strong> {{ report.condominiumName || '—' }}</span>
              <span><strong>Esercizio:</strong> {{ report.fiscalYearCode || '—' }}</span>
              <span class="report-gen">Pag. {{ pi + 1 }} di {{ printPages.length }} — {{ today }}</span>
            </div>
          </div>

          <table class="bilancio-table">
            <thead>
              <tr>
                <th rowspan="2" class="col-nom">Nominativo</th>
                <th rowspan="2">Tipo</th>
                <th v-for="col in page" :key="'pch'+pi+'_'+col.kind+col.id" colspan="2"
                    :class="col.kind === 'consumo' ? 'grp-consumo' : 'grp-mill'">
                  {{ col.name }}<span v-if="col.uom" class="uom"> ({{ col.uom }})</span>
                </th>
                <th rowspan="2" class="col-num">Dirette</th>
                <th rowspan="2" class="col-num">Accrediti</th>
                <th rowspan="2" class="col-num">Totale speso</th>
                <th rowspan="2" class="col-num">Versato</th>
                <th rowspan="2" class="col-num">Saldo</th>
              </tr>
              <tr>
                <template v-for="col in page" :key="'pcs'+pi+'_'+col.kind+col.id">
                  <th class="col-num sub">{{ printSub2(col) }}</th>
                  <th class="col-num sub">Importo</th>
                </template>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(r, i) in report.rows" :key="'pr'+pi+'_'+i" :class="{ 'row-tenant': r.rowType === 'Inquilini' }">
                <td>{{ r.nominativo || '—' }}</td>
                <td class="text-secondary">{{ r.rowType }}</td>
                <template v-for="col in page" :key="'pc'+pi+'_'+i+'_'+col.kind+col.id">
                  <td class="col-num mono text-secondary">{{ printCellVal(r, col) }}</td>
                  <td class="col-num mono">{{ printCellImp(r, col) }}</td>
                </template>
                <td class="col-num mono">{{ fmt(r.dirette) }}</td>
                <td class="col-num mono">{{ fmt(r.accrediti) }}</td>
                <td class="col-num mono"><strong>{{ fmt(r.totale) }}</strong></td>
                <td class="col-num mono">{{ fmt(r.versato) }}</td>
                <td class="col-num mono">{{ fmt(r.saldo) }}</td>
              </tr>
            </tbody>
            <tfoot>
              <tr class="totals-row">
                <td colspan="2">TOTALI</td>
                <template v-for="col in page" :key="'pt'+pi+'_'+col.kind+col.id">
                  <td class="col-num mono">{{ printTotVal(col) }}</td>
                  <td class="col-num mono">{{ printTotImp(col) }}</td>
                </template>
                <td class="col-num mono">{{ fmt(report.totals.dirette) }}</td>
                <td class="col-num mono">{{ fmt(report.totals.accrediti) }}</td>
                <td class="col-num mono"><strong>{{ fmt(report.totals.totale) }}</strong></td>
                <td class="col-num mono">{{ fmt(report.totals.versato) }}</td>
                <td class="col-num mono">{{ fmt(report.totals.saldo) }}</td>
              </tr>
            </tfoot>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { useAppStore } from '@/stores/app'
import { fiscalYearApi } from '@/services/api'

const store = useAppStore()
const fiscalYears          = computed(() => store.fiscalYears ?? [])
const selectedFiscalYearId = ref(null)
const report               = ref(null)
const loading              = ref(false)

// ── Modalità modifica (override manuali) ────────────────────────────────────
const editMode  = ref(false)
const saving    = ref(false)
const editRows  = ref([])   // copia editabile delle righe del report

// Costanti coerenti col backend (BilancioRowType / BilancioCellType)
const ROW_PROPRIETARI = 0, ROW_INQUILINI = 1
const CELL_CONSUMO = 0, CELL_MILLESIMALE = 1, CELL_DIRETTE = 2, CELL_ACCREDITI = 3, CELL_VERSATO = 4
const rowTypeCode = (r) => r.rowType === 'Inquilini' ? ROW_INQUILINI : ROW_PROPRIETARI

const scostamento = computed(() => {
  if (!report.value) return 0
  return Number(report.value.totaleEffettivo ?? 0) - Number(report.value.totaleCalcolato ?? 0)
})

// Righe mostrate in tabella: editabili in modalità modifica, altrimenti quelle del report.
const displayRows = computed(() => editMode.value ? editRows.value : (report.value?.rows ?? []))

// ── Stampa: suddivisione colonne dinamiche (consumi + millesimali) ──────────
// Le tabelle millesimali/consumi possono essere arbitrariamente tante e sforano la
// pagina. In stampa generiamo più tabelle, ciascuna con le colonne fisse +
// COLS_PER_PAGE colonne dinamiche, con page-break tra l'una e l'altra.
const COLS_PER_PAGE = 2

// Sequenza unificata di colonne dinamiche: prima i consumi, poi i millesimali.
const dynamicColumns = computed(() => {
  const r = report.value
  if (!r) return []
  const consumi = (r.consumoColumns ?? []).map(c => ({
    kind: 'consumo', id: c.consumptionTypeId, name: c.name, uom: c.unitOfMeasure,
  }))
  const mill = (r.millesimaleColumns ?? []).map(m => ({
    kind: 'mill', id: m.millesimalTableId, name: m.name, uom: null,
  }))
  return [...consumi, ...mill]
})

// Gruppi di colonne dinamiche per pagina di stampa.
const printPages = computed(() => {
  const cols = dynamicColumns.value
  if (!cols.length) return [[]]   // almeno una pagina (solo colonne fisse)
  const pages = []
  for (let i = 0; i < cols.length; i += COLS_PER_PAGE)
    pages.push(cols.slice(i, i + COLS_PER_PAGE))
  return pages
})

// Helper celle per la stampa (lavorano su una colonna unificata {kind,id}).
const printSub2 = (col) => col.kind === 'consumo' ? 'Q.tà' : 'Mill.'
const printCellVal = (r, col) => col.kind === 'consumo' ? cellQta(r, col.id) : cellMillVal(r, col.id)
const printCellImp = (r, col) => col.kind === 'consumo' ? cellImp(r, col.id) : cellMill(r, col.id)
const printTotVal = (col) => col.kind === 'consumo' ? totQta(col.id) : totMillVal(col.id)
const printTotImp = (col) => col.kind === 'consumo' ? totImp(col.id) : totMill(col.id)

async function load() {
  if (!selectedFiscalYearId.value) { report.value = null; return }
  loading.value = true; report.value = null; editMode.value = false
  try {
    const { data } = await fiscalYearApi.getBilancioRipartizione(selectedFiscalYearId.value)
    report.value = data
  } catch { /* gestito dal global error handler */ } finally { loading.value = false }
}

function enterEdit() {
  // Copia profonda delle righe; le celle modificate diventano override.
  editRows.value = JSON.parse(JSON.stringify(report.value.rows))
  editMode.value = true
}
function cancelEdit() { editMode.value = false; editRows.value = [] }

// Marca una cella come override quando l'utente la modifica.
function markConsumo(r, id) { const c = r.consumi.find(x => x.consumptionTypeId === id); if (c) c.isOverridden = true }
function markMill(r, id)    { const m = r.millesimali.find(x => x.millesimalTableId === id); if (m) m.isOverridden = true }

// Totale di riga: in modifica ricalcola dai valori correnti (consumi + millesimali + dirette - accrediti).
function rowTotale(r) {
  if (!editMode.value) return r.totale
  const sum = (arr) => (arr ?? []).reduce((s, x) => s + (Number(x.importo) || 0), 0)
  return sum(r.consumi) + sum(r.millesimali) + (Number(r.dirette) || 0) - (Number(r.accrediti) || 0)
}
function rowSaldo(r) { return editMode.value ? (Number(r.versato) || 0) - rowTotale(r) : r.saldo }

async function saveEdits() {
  saving.value = true
  try {
    const cells = []
    for (const r of editRows.value) {
      const ut = r.unitId, rt = rowTypeCode(r)
      for (const c of (r.consumi ?? []))
        if (c.isOverridden) cells.push({ unitId: ut, rowType: rt, cellType: CELL_CONSUMO, columnRefId: c.consumptionTypeId, amount: Number(c.importo) || 0 })
      for (const m of (r.millesimali ?? []))
        if (m.isOverridden) cells.push({ unitId: ut, rowType: rt, cellType: CELL_MILLESIMALE, columnRefId: m.millesimalTableId, amount: Number(m.importo) || 0 })
      if (r.diretteOverridden)   cells.push({ unitId: ut, rowType: rt, cellType: CELL_DIRETTE,   columnRefId: 0, amount: Number(r.dirette) || 0 })
      if (r.accreditiOverridden) cells.push({ unitId: ut, rowType: rt, cellType: CELL_ACCREDITI, columnRefId: 0, amount: Number(r.accrediti) || 0 })
      if (r.versatoOverridden)   cells.push({ unitId: ut, rowType: rt, cellType: CELL_VERSATO,   columnRefId: 0, amount: Number(r.versato) || 0 })
    }
    const { data } = await fiscalYearApi.saveBilancioOverrides(selectedFiscalYearId.value, cells)
    report.value = data
    editMode.value = false
    store.toast('Bilancio salvato', 'success')
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { saving.value = false }
}

async function resetOverrides() {
  if (!confirm('Ripristinare il bilancio calcolato automaticamente? Tutte le modifiche manuali verranno eliminate.')) return
  saving.value = true
  try {
    const { data } = await fiscalYearApi.resetBilancioOverrides(selectedFiscalYearId.value)
    report.value = data
    editMode.value = false
    store.toast('Bilancio ripristinato', 'success')
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { saving.value = false }
}

// ── Helper celle ──────────────────────────────────────────────────────────
const nf2 = new Intl.NumberFormat('it-IT', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
const nfQ = new Intl.NumberFormat('it-IT', { maximumFractionDigits: 2 })
const fmt    = (v) => v != null ? '€ ' + nf2.format(Number(v)) : '—'
const fmtQ   = (v) => v != null ? nfQ.format(Number(v)) : '—'

const findC  = (r, id) => r.consumi.find(c => c.consumptionTypeId === id)
const cellQta = (r, id) => { const c = findC(r, id); return c && c.quantita ? fmtQ(c.quantita) : '—' }
const cellImp = (r, id) => { const c = findC(r, id); return c && c.importo ? fmt(c.importo) : '—' }
const cellMill = (r, id) => { const m = r.millesimali.find(x => x.millesimalTableId === id); return m && m.importo ? fmt(m.importo) : '—' }
const cellMillVal = (r, id) => { const m = r.millesimali.find(x => x.millesimalTableId === id); return m && m.millesimal ? fmtQ(m.millesimal) : '—' }

const totQta = (id) => { const c = report.value.totals.consumi.find(x => x.consumptionTypeId === id); return c ? fmtQ(c.quantita) : '—' }
const totImp = (id) => { const c = report.value.totals.consumi.find(x => x.consumptionTypeId === id); return c ? fmt(c.importo) : '—' }
const totMill = (id) => { const m = report.value.totals.millesimali.find(x => x.millesimalTableId === id); return m ? fmt(m.importo) : '—' }
const totMillVal = (id) => { const m = report.value.totals.millesimali.find(x => x.millesimalTableId === id); return m ? fmtQ(m.millesimal) : '—' }

function printReport() { window.print() }

// ── Export PDF (file scaricabile) ───────────────────────────────────────────
// Stessa logica della stampa: una tabella per ogni gruppo di COLS_PER_PAGE
// colonne dinamiche, con le colonne fisse ripetute su ogni pagina.
async function exportPdf() {
  const rpt = report.value
  if (!rpt) return
  const { default: jsPDF }   = await import('jspdf')
  const { default: autoTable } = await import('jspdf-autotable')

  const doc = new jsPDF({ orientation: 'landscape', unit: 'mm', format: 'a4' })
  const pages = printPages.value

  pages.forEach((page, pi) => {
    if (pi > 0) doc.addPage()

    // Intestazione pagina
    doc.setFontSize(13); doc.setFont('helvetica', 'bold'); doc.setTextColor(0)
    doc.text('Bilancio di ripartizione', 14, 14)
    doc.setFontSize(9); doc.setFont('helvetica', 'normal'); doc.setTextColor(120)
    doc.text(
      `Condominio: ${rpt.condominiumName || '—'}   Esercizio: ${rpt.fiscalYearCode || '—'}   `
      + `Pag. ${pi + 1} di ${pages.length} — ${today}`,
      14, 20)
    doc.setTextColor(0)

    // Intestazione tabella: colonne fisse + le 2 colonne dinamiche (ognuna su 2 sub-colonne)
    const head1 = [
      { content: 'Nominativo', rowSpan: 2 },
      { content: 'Tipo',       rowSpan: 2 },
      ...page.map(col => ({ content: col.name + (col.uom ? ` (${col.uom})` : ''), colSpan: 2, styles: { halign: 'center' } })),
      { content: 'Dirette',      rowSpan: 2, styles: { halign: 'right' } },
      { content: 'Accrediti',    rowSpan: 2, styles: { halign: 'right' } },
      { content: 'Totale speso', rowSpan: 2, styles: { halign: 'right' } },
      { content: 'Versato',      rowSpan: 2, styles: { halign: 'right' } },
      { content: 'Saldo',        rowSpan: 2, styles: { halign: 'right' } },
    ]
    const head2 = page.flatMap(col => [
      { content: printSub2(col), styles: { halign: 'right' } },
      { content: 'Importo',      styles: { halign: 'right' } },
    ])

    const body = rpt.rows.map(r => {
      const cells = [r.nominativo || '—', r.rowType]
      for (const col of page) { cells.push(printCellVal(r, col), printCellImp(r, col)) }
      cells.push(fmt(r.dirette), fmt(r.accrediti), fmt(r.totale), fmt(r.versato), fmt(r.saldo))
      return cells
    })

    const foot = [{ content: 'TOTALI', colSpan: 2, styles: { fontStyle: 'bold' } }]
    for (const col of page) {
      foot.push({ content: printTotVal(col), styles: { halign: 'right', fontStyle: 'bold' } })
      foot.push({ content: printTotImp(col), styles: { halign: 'right', fontStyle: 'bold' } })
    }
    foot.push({ content: fmt(rpt.totals.dirette),   styles: { halign: 'right', fontStyle: 'bold' } })
    foot.push({ content: fmt(rpt.totals.accrediti), styles: { halign: 'right', fontStyle: 'bold' } })
    foot.push({ content: fmt(rpt.totals.totale),    styles: { halign: 'right', fontStyle: 'bold' } })
    foot.push({ content: fmt(rpt.totals.versato),   styles: { halign: 'right', fontStyle: 'bold' } })
    foot.push({ content: fmt(rpt.totals.saldo),     styles: { halign: 'right', fontStyle: 'bold' } })

    // Allinea a destra tutte le colonne numeriche (tutte tranne Nominativo+Tipo)
    const totalCols = 2 + page.length * 2 + 5
    const columnStyles = {}
    for (let i = 2; i < totalCols; i++) columnStyles[i] = { halign: 'right' }

    autoTable(doc, {
      startY: 25,
      head: [head1, head2],
      body,
      foot: [foot],
      styles:     { fontSize: 7.5, cellPadding: 1.4 },
      headStyles: { fillColor: [80, 80, 160], textColor: 255, fontSize: 7 },
      footStyles: { fillColor: [50, 50, 120], textColor: 255, fontSize: 7.5 },
      columnStyles,
      margin: { left: 10, right: 10 },
      showFoot: 'lastPage',
    })
  })

  doc.save(`Bilancio_ripartizione_${rpt.fiscalYearCode || 'esercizio'}.pdf`)
}

async function exportExcel() {
  const XLSX = await import('xlsx')
  const rpt = report.value

  // ── Intestazioni (con millesimi + importo per ogni tabella) ───────────────
  const header = ['Nominativo', 'Tipo']
  rpt.consumoColumns.forEach(c => {
    const um = c.unitOfMeasure ? ` (${c.unitOfMeasure})` : ''
    header.push(`${c.name} Q.tà${um}`, `${c.name} Importo`)
  })
  rpt.millesimaleColumns.forEach(m => { header.push(`${m.name} mill.`, `${m.name} importo`) })
  header.push('Dirette', 'Accrediti', 'Totale speso', 'Versato', 'Saldo')

  // Indici delle colonne numeriche (per formattazione): tutte tranne le prime 2
  const EUR = '#,##0.00 €'
  const NUM = '#,##0.00'

  // ── Righe (Array Of Arrays per controllo preciso) ─────────────────────────
  const aoa = [header]
  const numFmtByCol = {}   // colIndex -> formato

  // calcola i formati per colonna in base alla posizione
  let ci = 2
  rpt.consumoColumns.forEach(() => { numFmtByCol[ci] = NUM; numFmtByCol[ci+1] = EUR; ci += 2 })
  rpt.millesimaleColumns.forEach(() => { numFmtByCol[ci] = NUM; numFmtByCol[ci+1] = EUR; ci += 2 })
  ;['Dirette','Accrediti','Totale speso','Versato','Saldo'].forEach(() => { numFmtByCol[ci] = EUR; ci += 1 })

  function rowToArray(r) {
    const a = [r.nominativo || '', r.rowType]
    rpt.consumoColumns.forEach(c => {
      const cell = findC(r, c.consumptionTypeId)
      a.push(cell ? Number(cell.quantita) : 0, cell ? Number(cell.importo) : 0)
    })
    rpt.millesimaleColumns.forEach(m => {
      const cell = r.millesimali.find(x => x.millesimalTableId === m.millesimalTableId)
      a.push(cell ? Number(cell.millesimal) : 0, cell ? Number(cell.importo) : 0)
    })
    a.push(Number(r.dirette), Number(r.accrediti), Number(r.totale), Number(r.versato), Number(r.saldo))
    return a
  }
  rpt.rows.forEach(r => aoa.push(rowToArray(r)))

  // ── Riga totali ───────────────────────────────────────────────────────────
  const tot = ['TOTALI', '']
  rpt.consumoColumns.forEach(c => {
    const t = rpt.totals.consumi.find(x => x.consumptionTypeId === c.consumptionTypeId)
    tot.push(t ? Number(t.quantita) : 0, t ? Number(t.importo) : 0)
  })
  rpt.millesimaleColumns.forEach(m => {
    const t = rpt.totals.millesimali.find(x => x.millesimalTableId === m.millesimalTableId)
    tot.push(t ? Number(t.millesimal) : 0, t ? Number(t.importo) : 0)
  })
  tot.push(Number(rpt.totals.dirette), Number(rpt.totals.accrediti), Number(rpt.totals.totale), Number(rpt.totals.versato), Number(rpt.totals.saldo))
  aoa.push(tot)

  const ws = XLSX.utils.aoa_to_sheet(aoa)

  // ── Formattazione ─────────────────────────────────────────────────────────
  const nRows = aoa.length
  const nCols = header.length
  // larghezze colonne
  ws['!cols'] = header.map((h, idx) => ({ wch: idx === 0 ? 26 : Math.max(10, h.length + 2) }))
  // formato numerico + grassetto intestazione/totali
  for (let R = 0; R < nRows; R++) {
    for (let C = 0; C < nCols; C++) {
      const addr = XLSX.utils.encode_cell({ r: R, c: C })
      const cell = ws[addr]
      if (!cell) continue
      if (R === 0) { cell.s = { font: { bold: true }, alignment: { horizontal: 'center', wrapText: true } } }
      else if (R === nRows - 1) { cell.s = { font: { bold: true } } }
      if (R > 0 && numFmtByCol[C]) cell.z = numFmtByCol[C]
    }
  }
  // blocca la prima riga e le prime 2 colonne
  ws['!freeze'] = { xSplit: 2, ySplit: 1 }
  ws['!autofilter'] = { ref: XLSX.utils.encode_range({ s: { r: 0, c: 0 }, e: { r: nRows - 1, c: nCols - 1 } }) }

  const wb = XLSX.utils.book_new()
  XLSX.utils.book_append_sheet(wb, ws, 'Bilancio ripartizione')
  XLSX.writeFile(wb, `Bilancio_ripartizione_${rpt.fiscalYearCode || 'esercizio'}.xlsx`)
}

// ── Lifecycle ──────────────────────────────────────────────────────────────
watch(selectedFiscalYearId, load)
watch(fiscalYears, (fys) => {
  if (fys.length && !selectedFiscalYearId.value) selectedFiscalYearId.value = fys[0].id
}, { immediate: true })

async function refresh() { selectedFiscalYearId.value = null; await store.loadFiscalYears() }
onMounted(() => { if (!store.fiscalYears?.length) store.loadFiscalYears() })
onUnmounted(() => window.removeEventListener('app:refresh', refresh))
window.addEventListener('app:refresh', refresh)

const today = new Date().toLocaleDateString('it-IT')
</script>

<style scoped>
.toolbar { display: flex; align-items: center; gap: 1rem; margin-bottom: 1rem; flex-wrap: wrap; }
.toolbar h1 { margin: 0; margin-right: auto; }

.report-sheet { background: var(--bg-surface); border: 1px solid var(--border); border-radius: 8px; padding: 1.25rem 1.5rem; }
.report-header { border-bottom: 2px solid var(--border); padding-bottom: 0.75rem; margin-bottom: 1rem; }
.report-header h2 { margin: 0 0 0.5rem; font-size: 1.15rem; }
.report-meta { display: flex; gap: 1.5rem; flex-wrap: wrap; font-size: 0.85rem; color: var(--text-secondary); }
.report-gen { margin-left: auto; color: var(--text-muted); }

.table-wrap { overflow-x: auto; }
.bilancio-table { width: 100%; border-collapse: collapse; font-size: 0.8rem; }
.bilancio-table th, .bilancio-table td { padding: 0.3rem 0.5rem; border: 1px solid var(--border); white-space: nowrap; }
.bilancio-table th { background: var(--bg); font-size: 0.7rem; text-transform: uppercase; letter-spacing: 0.3px; color: var(--text-muted); font-weight: 600; text-align: center; }
.bilancio-table th.sub { font-size: 0.65rem; }
.grp-consumo { background: var(--accent-glow, rgba(99,102,241,0.08)); }
.grp-mill { vertical-align: bottom; }
.uom { text-transform: none; font-weight: 400; }
.col-nom { text-align: left; min-width: 160px; }
.col-num { text-align: right; }
.mono { font-family: monospace; }
.row-tenant td { background: rgba(34,197,94,0.05); font-style: italic; }
.totals-row td { font-weight: 700; border-top: 2px solid var(--border); background: var(--bg); }
.text-green { color: var(--accent-green, #22c55e); }
.text-secondary { color: var(--text-secondary); }

/* Override manuali */
.cell-ovr { background: var(--accent-glow, rgba(99,102,241,0.12)); box-shadow: inset 0 0 0 1px var(--accent, #6366f1); }
.cell-input {
  width: 100%; min-width: 70px; box-sizing: border-box;
  padding: 0.15rem 0.3rem; text-align: right; font-family: monospace; font-size: 0.78rem;
  border: 1px solid var(--border); border-radius: 4px; background: var(--bg-surface); color: var(--text);
}
.cell-input:focus { outline: none; border-color: var(--border-active, var(--accent)); }
.scostamento-banner {
  margin-bottom: 1rem; padding: 0.6rem 0.9rem; border-radius: 6px; font-size: 0.85rem;
  background: color-mix(in srgb, var(--accent-amber, #f59e0b) 12%, transparent);
  border: 1px solid var(--accent-amber, #f59e0b);
}
.scostamento-banner.scostamento-ok {
  background: color-mix(in srgb, var(--accent-green, #22c55e) 10%, transparent);
  border-color: var(--accent-green, #22c55e);
}

/* Blocco di sole tabelle di stampa: nascosto a schermo */
.print-only { display: none; }

@media print {
  .no-print { display: none !important; }
  .report-sheet { border: none; padding: 0; background: #fff; color: #000; }
  .bilancio-table th, .bilancio-table td { color: #000 !important; border-color: #999 !important; }
  @page { size: landscape; margin: 10mm; }

  /* In stampa mostriamo solo le tabelle spezzate, una per pagina */
  .print-only { display: block; }
  .print-page { page-break-after: always; break-after: page; }
  .print-page:last-child { page-break-after: auto; break-after: auto; }
  .print-page .bilancio-table { width: 100%; table-layout: fixed; }
  .print-page-header { border-bottom: 2px solid #000; padding-bottom: 0.4rem; margin-bottom: 0.6rem; }
  .print-page-header h2 { margin: 0 0 0.3rem; font-size: 1.05rem; color: #000; }
  .print-page-header .report-meta { color: #000; font-size: 0.8rem; }
}
</style>
