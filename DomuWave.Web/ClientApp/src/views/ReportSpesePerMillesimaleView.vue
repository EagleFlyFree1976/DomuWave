<template>
  <div class="report-page">
    <!-- ── Toolbar (nascosta in stampa) ─────────────────────────── -->
    <div class="toolbar no-print">
      <h1>Report spese per tabella millesimale</h1>
      <select class="form-select" v-model.number="selectedFiscalYearId" style="min-width:220px">
        <option :value="null" disabled>Seleziona esercizio…</option>
        <option v-for="fy in fiscalYears" :key="fy.id" :value="fy.id">
          {{ fy.code }}{{ fy.description ? ' – ' + fy.description : '' }}
        </option>
      </select>
      <div class="group-toggle" role="group" aria-label="Raggruppa per">
        <button type="button" class="group-toggle__btn" :class="{ active: groupBy === 'table' }"
          @click="groupBy = 'table'">Tabella millesimale</button>
        <button type="button" class="group-toggle__btn" :class="{ active: groupBy === 'supplier' }"
          @click="groupBy = 'supplier'">Fornitore</button>
      </div>
      <button class="btn btn-ghost" :disabled="!report || loading" @click="printReport">
        🖨️ Stampa / PDF
      </button>
    </div>

    <div v-if="!selectedFiscalYearId" class="card empty-state no-print">
      Seleziona un esercizio fiscale per generare il report.
    </div>

    <div v-else-if="loading" class="card loading-state"><div class="spinner"></div></div>

    <div v-else-if="report" class="report-sheet">
      <!-- Intestazione report (visibile anche in stampa) -->
      <div class="report-header">
        <h2>Elenco spese per {{ groupBy === 'supplier' ? 'fornitore' : 'tabella millesimale' }}</h2>
        <div class="report-meta">
          <span><strong>Condominio:</strong> {{ report.condominiumName || '—' }}</span>
          <span><strong>Esercizio:</strong> {{ report.fiscalYearCode || '—' }}</span>
          <span class="report-gen">Generato il {{ today }}</span>
        </div>
      </div>

      <div v-if="!displayGroups.length" class="empty-state">
        Nessuna spesa registrata per questo esercizio.
      </div>

      <!-- Un blocco per ogni gruppo (tabella millesimale / immobile / fornitore) -->
      <div v-for="(g, gi) in displayGroups" :key="gi" class="report-group">
        <h3 class="group-title" :class="{ 'group-direct': g.isDirectUnit, 'group-consumption': g.isConsumption, 'group-supplier': groupBy === 'supplier' }">
          {{ g.groupName }}
          <span v-if="g.isConsumption" class="group-hint">ripartite in base ai consumi, non a millesimi</span>
        </h3>
        <table class="report-table">
          <thead>
            <tr>
              <th style="width:90px">Data</th>
              <th style="width:90px">N° doc.</th>
              <th>Descrizione</th>
              <th v-if="groupBy === 'supplier'">Tabella / Immobile</th>
              <th v-else>Fornitore</th>
              <th>Conto</th>
              <th v-if="g.isDirectUnit">Immobile</th>
              <th class="text-right" style="width:120px">Importo</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(e, ei) in g.expenses" :key="e.expenseId + '-' + ei">
              <td class="mono text-secondary">{{ fmtDate(e.documentDate) }}</td>
              <td class="text-secondary">{{ e.documentNumber || '—' }}</td>
              <td>{{ e.name }}</td>
              <td v-if="groupBy === 'supplier'" class="text-secondary">{{ e.tableName || '—' }}</td>
              <td v-else class="text-secondary">{{ e.supplierName || '—' }}</td>
              <td class="text-secondary">{{ e.accountName || '—' }}</td>
              <td v-if="g.isDirectUnit">{{ e.unitName || '—' }}</td>
              <td class="mono text-right">{{ fmt(e.grossAmount) }}</td>
            </tr>
          </tbody>
          <tfoot>
            <tr class="group-total-row">
              <td :colspan="g.isDirectUnit ? 6 : 5">Totale {{ g.groupName }}</td>
              <td class="mono text-right">{{ fmt(g.groupTotal) }}</td>
            </tr>
          </tfoot>
        </table>
      </div>

      <!-- Totale complessivo -->
      <div v-if="displayGroups.length" class="grand-total">
        <span>TOTALE COMPLESSIVO</span>
        <span class="mono">{{ fmt(report.grandTotal) }}</span>
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
const groupBy              = ref('table')   // 'table' | 'supplier'

// I gruppi da mostrare dipendono dalla modalità selezionata.
// In modalità 'table' usiamo direttamente i gruppi del backend (per tabella millesimale);
// in modalità 'supplier' ricostruiamo i gruppi lato client raggruppando le spese per fornitore.
const displayGroups = computed(() => {
  if (!report.value) return []
  const groups = report.value.groups ?? []
  if (groupBy.value !== 'supplier') return groups

  // Raccogli tutte le spese di tutti i gruppi, annotando la tabella d'origine.
  const bySupplier = new Map()
  for (const g of groups) {
    for (const e of (g.expenses ?? [])) {
      const key  = e.supplierName?.trim() || '— Senza fornitore —'
      let bucket = bySupplier.get(key)
      if (!bucket) {
        bucket = { groupName: key, isDirectUnit: false, isConsumption: false, expenses: [], groupTotal: 0 }
        bySupplier.set(key, bucket)
      }
      // conserva il nome della tabella/gruppo d'origine per la colonna dedicata
      bucket.expenses.push({ ...e, tableName: g.groupName })
      bucket.groupTotal += Number(e.grossAmount ?? 0)
    }
  }
  // ordina alfabeticamente per nome fornitore (it-IT)
  return [...bySupplier.values()].sort((a, b) => a.groupName.localeCompare(b.groupName, 'it'))
})

async function load() {
  if (!selectedFiscalYearId.value) { report.value = null; return }
  loading.value = true
  report.value  = null
  try {
    const { data } = await fiscalYearApi.getExpensesByMillesimalReport(selectedFiscalYearId.value)
    report.value = data
  } catch {
    // gestito dal global error handler
  } finally {
    loading.value = false
  }
}

function printReport() { window.print() }

// ── Lifecycle ───────────────────────────────────────────────────────────────
watch(selectedFiscalYearId, load)
watch(fiscalYears, (fys) => {
  if (fys.length && !selectedFiscalYearId.value) selectedFiscalYearId.value = fys[0].id
}, { immediate: true })

async function refresh() {
  selectedFiscalYearId.value = null
  await store.loadFiscalYears()
}
onMounted(() => { if (!store.fiscalYears?.length) store.loadFiscalYears() })
onUnmounted(() => window.removeEventListener('app:refresh', refresh))
window.addEventListener('app:refresh', refresh)

// ── Formatters ────────────────────────────────────────────────────────────────
const fmt     = (v) => v != null ? '€ ' + Number(v).toLocaleString('it-IT', { minimumFractionDigits: 2 }) : '—'
const fmtDate = (d) => d ? new Date(d).toLocaleDateString('it-IT') : '—'
const today   = new Date().toLocaleDateString('it-IT')
</script>

<style scoped>
.toolbar { display: flex; align-items: center; gap: 1rem; margin-bottom: 1rem; flex-wrap: wrap; }
.toolbar h1 { margin: 0; margin-right: auto; }

.group-toggle { display: inline-flex; border: 1px solid var(--border); border-radius: 6px; overflow: hidden; }
.group-toggle__btn {
  border: none; background: var(--bg-surface); color: var(--text-secondary);
  padding: 0.4rem 0.8rem; font-size: 0.82rem; cursor: pointer; transition: background 0.15s, color 0.15s;
}
.group-toggle__btn + .group-toggle__btn { border-left: 1px solid var(--border); }
.group-toggle__btn:hover { background: var(--accent-glow, rgba(99,102,241,0.08)); }
.group-toggle__btn.active { background: var(--accent); color: #fff; }

.report-sheet { background: var(--bg-surface); border: 1px solid var(--border); border-radius: 8px; padding: 1.5rem 1.75rem; }

.report-header { border-bottom: 2px solid var(--border); padding-bottom: 0.75rem; margin-bottom: 1.25rem; }
.report-header h2 { margin: 0 0 0.5rem; font-size: 1.15rem; }
.report-meta { display: flex; gap: 1.5rem; flex-wrap: wrap; font-size: 0.85rem; color: var(--text-secondary); }
.report-gen { margin-left: auto; color: var(--text-muted); }

.report-group { margin-bottom: 1.5rem; }
.group-title {
  font-size: 0.95rem; font-weight: 700; margin: 0 0 0.4rem;
  padding: 0.35rem 0.6rem; background: var(--accent-glow, rgba(99,102,241,0.08));
  border-left: 3px solid var(--accent); border-radius: 3px;
}
.group-title.group-direct { border-left-color: var(--accent-green, #22c55e); }
.group-title.group-consumption { border-left-color: var(--accent-amber, #f59e0b); }
.group-title.group-supplier { border-left-color: var(--accent-teal, #14b8a6); }
.group-hint {
  font-size: 0.72rem; font-weight: 500; color: var(--text-muted);
  margin-left: 0.5rem; text-transform: none; letter-spacing: 0;
}

.report-table { width: 100%; border-collapse: collapse; font-size: 0.825rem; }
.report-table th {
  text-align: left; font-size: 0.72rem; font-weight: 600; text-transform: uppercase;
  letter-spacing: 0.3px; color: var(--text-muted);
  padding: 0.35rem 0.5rem; border-bottom: 1px solid var(--border);
}
.report-table td { padding: 0.35rem 0.5rem; border-bottom: 1px solid var(--border); vertical-align: top; }
.report-table tbody tr:last-child td { border-bottom: 1px solid var(--border); }
.group-total-row td { font-weight: 700; border-top: 2px solid var(--border); background: var(--bg); }

.grand-total {
  display: flex; justify-content: space-between; align-items: center;
  margin-top: 1rem; padding: 0.75rem 1rem;
  background: var(--accent-glow, rgba(99,102,241,0.1));
  border: 1px solid var(--accent); border-radius: 6px;
  font-size: 1.05rem; font-weight: 800;
}

.text-right { text-align: right; }
.mono { font-family: monospace; }

/* ── Stampa ─────────────────────────────────────────────────────────────────── */
@media print {
  .no-print { display: none !important; }
  .report-sheet { border: none; padding: 0; background: #fff; color: #000; }
  .group-title { background: #f0f0f0 !important; border-left: 3px solid #333; color: #000; }
  .report-table th, .report-table td { color: #000 !important; }
  .grand-total { background: #f0f0f0 !important; border: 1px solid #333; color: #000; }
  .report-group { page-break-inside: avoid; }
}
</style>
