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
      <button class="btn btn-ghost" :disabled="!report || loading" @click="printReport">🖨 PDF / Stampa</button>
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

      <div v-if="!report.rows.length" class="empty-state">Nessun dato per questo esercizio.</div>

      <div v-else class="table-wrap">
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
            <tr v-for="(r, i) in report.rows" :key="i" :class="{ 'row-tenant': r.rowType === 'Inquilini' }">
              <td>{{ r.nominativo || '—' }}</td>
              <td class="text-secondary">{{ r.rowType }}</td>
              <template v-for="c in report.consumoColumns" :key="'c'+i+'_'+c.consumptionTypeId">
                <td class="col-num mono">{{ cellQta(r, c.consumptionTypeId) }}</td>
                <td class="col-num mono">{{ cellImp(r, c.consumptionTypeId) }}</td>
              </template>
              <template v-for="m in report.millesimaleColumns" :key="'m'+i+'_'+m.millesimalTableId">
                <td class="col-num mono text-secondary">{{ cellMillVal(r, m.millesimalTableId) }}</td>
                <td class="col-num mono">{{ cellMill(r, m.millesimalTableId) }}</td>
              </template>
              <td class="col-num mono">{{ fmt(r.dirette) }}</td>
              <td class="col-num mono text-green">{{ fmt(r.accrediti) }}</td>
              <td class="col-num mono"><strong>{{ fmt(r.totale) }}</strong></td>
              <td class="col-num mono">{{ fmt(r.versato) }}</td>
              <td class="col-num mono" :class="r.saldo >= 0 ? 'text-green' : 'text-red'">{{ fmt(r.saldo) }}</td>
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

async function load() {
  if (!selectedFiscalYearId.value) { report.value = null; return }
  loading.value = true; report.value = null
  try {
    const { data } = await fiscalYearApi.getBilancioRipartizione(selectedFiscalYearId.value)
    report.value = data
  } catch { /* gestito dal global error handler */ } finally { loading.value = false }
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

@media print {
  .no-print { display: none !important; }
  .report-sheet { border: none; padding: 0; background: #fff; color: #000; }
  .bilancio-table th, .bilancio-table td { color: #000 !important; border-color: #999 !important; }
  @page { size: landscape; }
}
</style>
