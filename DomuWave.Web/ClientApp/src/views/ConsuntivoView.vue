<template>
  <div>
    <div class="page-header">
      <h1>Consuntivo</h1>
    </div>

    <div class="tab-toolbar">
      <select class="form-select" v-model="selectedFiscalYearId" style="min-width:180px"
              :disabled="!fiscalYears.length">
        <option :value="null" disabled>Seleziona esercizio…</option>
        <option v-for="fy in fiscalYears" :key="fy.id" :value="fy.id">
          {{ fy.code }}{{ fy.description ? ' – ' + fy.description : '' }}
        </option>
      </select>

      <!-- Tab switcher -->
      <div v-if="selectedFiscalYearId" class="tab-pills" style="margin-left:auto">
        <button class="tab-pill" :class="{ active: activeTab === 'budget' }" @click="activeTab = 'budget'">
          Preventivo vs Consuntivo
        </button>
        <button class="tab-pill" :class="{ active: activeTab === 'conguaglio' }" @click="loadConguaglio(); activeTab = 'conguaglio'">
          Conguaglio
        </button>
      </div>
    </div>

    <!-- ══════════════════════════════════════════════════
         TAB — Preventivo vs Consuntivo
    ══════════════════════════════════════════════════ -->
    <template v-if="activeTab === 'budget'">
      <div v-if="!selectedFiscalYearId" class="card">
        <div class="empty-state">
          <div class="empty-icon">◎</div>
          <div>Seleziona un esercizio fiscale</div>
        </div>
      </div>

      <div v-else-if="loadingBudgets" class="card">
        <div class="loading-state"><div class="spinner"></div></div>
      </div>

      <template v-else-if="preventivo || consuntivo">
        <!-- Riepilogo globale -->
        <div class="summary-bar" v-if="preventivo && consuntivo">
          <div class="summary-item">
            <span class="summary-label">Totale preventivo</span>
            <span class="summary-value">{{ fmt(prevTotal) }}</span>
          </div>
          <div class="summary-item">
            <span class="summary-label">Totale consuntivo</span>
            <span class="summary-value">{{ fmt(consTotal) }}</span>
          </div>
          <div class="summary-item">
            <span class="summary-label">Scostamento</span>
            <span class="summary-value" :class="scostamento >= 0 ? 'text-red' : 'text-green'">
              {{ scostamento >= 0 ? '+' : '' }}{{ fmt(scostamento) }}
            </span>
          </div>
          <div class="summary-item">
            <span class="summary-label">Variazione %</span>
            <span class="summary-value" :class="scostamento >= 0 ? 'text-red' : 'text-green'">
              {{ prevTotal > 0 ? ((scostamento / prevTotal) * 100).toFixed(1) + '%' : '—' }}
            </span>
          </div>
        </div>

        <!-- Avvisi mancanti -->
        <div v-if="!preventivo" class="warning-banner" style="margin-bottom:12px">
          Nessun budget preventivo trovato per questo esercizio.
        </div>
        <div v-if="!consuntivo" class="warning-banner" style="margin-bottom:12px">
          Nessun budget consuntivo approvato trovato. Approva il consuntivo per vedere il confronto completo.
        </div>

        <!-- Tabella confronto voci -->
        <div class="card">
          <div v-if="loadingItems" class="loading-state"><div class="spinner"></div></div>
          <div v-else-if="!comparisonRows.length" class="empty-state">
            <div class="empty-icon">◎</div>
            <div>Nessuna voce disponibile</div>
          </div>
          <div v-else class="table-wrap">
            <table>
              <thead>
                <tr>
                  <th style="min-width:220px">Voce</th>
                  <th class="text-right" style="min-width:130px">Preventivo</th>
                  <th class="text-right" style="min-width:130px">Consuntivo</th>
                  <th class="text-right" style="min-width:130px">Scostamento</th>
                  <th class="text-right" style="min-width:80px">Var. %</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="row in comparisonRows" :key="row.accountId"
                    :class="row.isHeader ? 'row-header' : 'row-leaf'">
                  <td>
                    <span :style="{ paddingLeft: (row.level * 16) + 'px' }">
                      <span v-if="row.isHeader" class="text-muted" style="margin-right:4px">▸</span>
                      <span :class="row.isHeader ? '' : 'text-secondary'">{{ row.accountCode }}</span>
                      {{ row.accountName }}
                    </span>
                  </td>
                  <td class="mono text-right">{{ row.prevAmount != null ? fmt(row.prevAmount) : '—' }}</td>
                  <td class="mono text-right">{{ row.consAmount != null ? fmt(row.consAmount) : '—' }}</td>
                  <td class="mono text-right"
                      :class="row.diff > 0 ? 'text-red' : row.diff < 0 ? 'text-green' : 'text-muted'">
                    <template v-if="row.prevAmount != null && row.consAmount != null">
                      {{ row.diff >= 0 ? '+' : '' }}{{ fmt(row.diff) }}
                    </template>
                    <template v-else>—</template>
                  </td>
                  <td class="mono text-right text-secondary">
                    <template v-if="row.prevAmount && row.prevAmount !== 0 && row.consAmount != null">
                      {{ ((row.diff / row.prevAmount) * 100).toFixed(1) }}%
                    </template>
                    <template v-else>—</template>
                  </td>
                </tr>
              </tbody>
              <tfoot>
                <tr class="row-total">
                  <td><strong>Totale</strong></td>
                  <td class="mono text-right"><strong>{{ preventivo ? fmt(prevTotal) : '—' }}</strong></td>
                  <td class="mono text-right"><strong>{{ consuntivo ? fmt(consTotal) : '—' }}</strong></td>
                  <td class="mono text-right" :class="scostamento >= 0 ? 'text-red' : 'text-green'">
                    <strong v-if="preventivo && consuntivo">
                      {{ scostamento >= 0 ? '+' : '' }}{{ fmt(scostamento) }}
                    </strong>
                    <span v-else>—</span>
                  </td>
                  <td></td>
                </tr>
              </tfoot>
            </table>
          </div>
        </div>
      </template>

      <div v-else class="card">
        <div class="empty-state">
          <div class="empty-icon">◎</div>
          <div>Nessun budget trovato per questo esercizio</div>
        </div>
      </div>
    </template>

    <!-- ══════════════════════════════════════════════════
         TAB — Conguaglio per unità
    ══════════════════════════════════════════════════ -->
    <template v-else-if="activeTab === 'conguaglio'">
      <div v-if="!selectedFiscalYearId" class="card">
        <div class="empty-state">
          <div class="empty-icon">◎</div>
          <div>Seleziona un esercizio fiscale</div>
        </div>
      </div>

      <div v-else-if="loadingConguaglio" class="card">
        <div class="loading-state"><div class="spinner"></div></div>
      </div>

      <div v-else-if="conguaglioError" class="card">
        <div class="empty-state">
          <div class="empty-icon">⚠</div>
          <div>{{ conguaglioError }}</div>
        </div>
      </div>

      <template v-else-if="conguaglio">
        <!-- Intestazione documento -->
        <div class="conguaglio-header card" style="margin-bottom:12px">
          <div class="conguaglio-meta">
            <div>
              <span class="text-muted">Esercizio:</span>
              <strong style="margin-left:6px">{{ conguaglio.fiscalYearCode }}</strong>
            </div>
            <div>
              <span class="text-muted">Approvazione consuntivo:</span>
              <span style="margin-left:6px">{{ fmtDate(conguaglio.approvalDate) }}</span>
            </div>
          </div>
          <div class="summary-bar" style="margin-top:16px">
            <div class="summary-item">
              <span class="summary-label">Spese totali</span>
              <span class="summary-value">{{ fmt(conguaglio.totalExpenses) }}</span>
            </div>
            <div class="summary-item">
              <span class="summary-label">Già incassato</span>
              <span class="summary-value text-green">{{ fmt(conguaglio.totalPaid) }}</span>
            </div>
            <div class="summary-item">
              <span class="summary-label">Saldo globale</span>
              <span class="summary-value"
                    :class="conguaglio.globalBalance > 0 ? 'text-red' : conguaglio.globalBalance < 0 ? 'text-green' : 'text-muted'">
                {{ fmt(conguaglio.globalBalance) }}
              </span>
            </div>
          </div>
        </div>

        <!-- Tabella per unità -->
        <div class="card">
          <div class="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Unità</th>
                  <th class="text-right">Millesimi</th>
                  <th class="text-right">Quota consuntiva</th>
                  <th class="text-right">Già pagato</th>
                  <th class="text-right">Saldo</th>
                  <th class="text-center">Tipo</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="u in conguaglio.units" :key="u.unitId">
                  <td>
                    <span class="mono text-secondary" style="margin-right:6px">{{ u.unitInternalNumber }}</span>
                    {{ u.unitDescription }}
                  </td>
                  <td class="mono text-right text-secondary">{{ u.millesimal.toFixed(3) }}</td>
                  <td class="mono text-right">{{ fmt(u.quotaConsuntiva) }}</td>
                  <td class="mono text-right text-green">{{ fmt(u.alreadyPaid) }}</td>
                  <td class="mono text-right"
                      :class="u.saldo > 0 ? 'text-red' : u.saldo < 0 ? 'text-green' : 'text-muted'">
                    {{ u.saldo >= 0 ? '+' : '' }}{{ fmt(u.saldo) }}
                  </td>
                  <td class="text-center">
                    <span class="badge"
                          :class="u.saldoType === 'Debit' ? 'badge-red' : u.saldoType === 'Credit' ? 'badge-green' : 'badge-muted'">
                      {{ u.saldoType === 'Debit' ? 'A debito' : u.saldoType === 'Credit' ? 'A credito' : 'Pari' }}
                    </span>
                  </td>
                </tr>
              </tbody>
              <tfoot>
                <tr class="row-total">
                  <td><strong>Totale</strong></td>
                  <td class="mono text-right text-secondary">
                    {{ conguaglio.units.reduce((s, u) => s + u.millesimal, 0).toFixed(3) }}
                  </td>
                  <td class="mono text-right"><strong>{{ fmt(conguaglio.totalExpenses) }}</strong></td>
                  <td class="mono text-right text-green"><strong>{{ fmt(conguaglio.totalPaid) }}</strong></td>
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

      <div v-else class="card">
        <div class="empty-state">
          <div class="empty-icon">◎</div>
          <div>Nessun dato disponibile</div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { useAppStore } from '@/stores/app'
import { usePermissions } from '@/composables/usePermissions'
import { budgetApi, fiscalYearApi } from '@/services/api'

const store = useAppStore()
const { canView } = usePermissions()

// ── Fiscal Years ────────────────────────────────────────────────
const fiscalYears          = computed(() => store.fiscalYears)
const selectedFiscalYearId = computed({
  get: () => store.selectedFiscalYearId,
  set: (v) => { store.selectedFiscalYearId = v },
})

// ── Tab ─────────────────────────────────────────────────────────
const activeTab = ref('budget')

// ── Budget (preventivo + consuntivo) ────────────────────────────
const loadingBudgets = ref(false)
const preventivo     = ref(null)   // budget con type === 1
const consuntivo     = ref(null)   // budget con type === 2, status approvato/chiuso

async function loadBudgets() {
  if (!selectedFiscalYearId.value) {
    preventivo.value = null
    consuntivo.value = null
    return
  }
  loadingBudgets.value = true
  try {
    const { data } = await budgetApi.getByFiscalYear(selectedFiscalYearId.value)
    const all = data ?? []
    preventivo.value = all.find(b => b.type === 1) ?? null
    consuntivo.value = all.find(b => b.type === 2 && (b.statusId === 2 || b.statusId === 3)) ?? null
  } catch {
    preventivo.value = null
    consuntivo.value = null
  } finally {
    loadingBudgets.value = false
  }
}

// ── Budget Items ─────────────────────────────────────────────────
const loadingItems  = ref(false)
const prevItems     = ref([])
const consItems     = ref([])

async function loadItems() {
  prevItems.value  = []
  consItems.value  = []
  if (!selectedFiscalYearId.value) return

  loadingItems.value = true
  try {
    const tasks = []
    if (preventivo.value) tasks.push(budgetApi.getItems(preventivo.value.id).then(r => { prevItems.value  = r.data ?? [] }))
    if (consuntivo.value) tasks.push(budgetApi.getItems(consuntivo.value.id).then(r => { consItems.value = r.data ?? [] }))
    await Promise.all(tasks)
  } catch {
    // silently handled by empty arrays
  } finally {
    loadingItems.value = false
  }
}

// ── Comparison rows ──────────────────────────────────────────────
// Merges prev and cons items on accountId into a flat sorted hierarchy
const comparisonRows = computed(() => {
  const allAccountIds = new Set([
    ...prevItems.value.map(i => i.accountId),
    ...consItems.value.map(i => i.accountId),
  ])

  const prevMap = Object.fromEntries(prevItems.value.map(i => [i.accountId, i]))
  const consMap = Object.fromEntries(consItems.value.map(i => [i.accountId, i]))

  // collect all items (union) for hierarchy building
  const allItems = [...prevItems.value, ...consItems.value.filter(i => !prevMap[i.accountId])]

  if (!allItems.length) return []

  // Build parent→children map
  const byParent = {}
  for (const i of allItems) {
    const pid = i.parentAccountId ?? null
    if (!byParent[pid]) byParent[pid] = new Map()
    if (!byParent[pid].has(i.accountId)) byParent[pid].set(i.accountId, i)
  }

  const result = []
  function walk(parentId, level) {
    const children = byParent[parentId]
    if (!children) return
    const sorted = [...children.values()].sort((a, b) => (a.accountCode ?? '').localeCompare(b.accountCode ?? ''))
    for (const i of sorted) {
      const hasChildren = !!byParent[i.accountId]?.size
      const prev = prevMap[i.accountId]
      const cons = consMap[i.accountId]

      let prevAmount = null
      let consAmount = null

      if (!hasChildren) {
        // leaf: use item amount directly
        prevAmount = prev?.amount ?? null
        consAmount = cons?.amount ?? null
      } else {
        // header: sum leaf descendants
        prevAmount = sumDescendants(i.accountId, prevMap)
        consAmount = sumDescendants(i.accountId, consMap)
      }

      const diff = prevAmount != null && consAmount != null
        ? consAmount - prevAmount
        : null

      result.push({
        accountId:   i.accountId,
        accountCode: i.accountCode,
        accountName: i.accountName,
        level,
        isHeader:    hasChildren,
        prevAmount,
        consAmount,
        diff,
      })
      walk(i.accountId, level + 1)
    }
  }

  // find roots (parentAccountId is null or not present in any item's accountId)
  const knownAccountIds = new Set(allItems.map(i => i.accountId))
  const roots = allItems.filter(i => !i.parentAccountId || !knownAccountIds.has(i.parentAccountId))
  const rootParentIds = new Set(roots.map(i => i.parentAccountId ?? null))
  for (const pid of rootParentIds) walk(pid, 0)

  return result

  function sumDescendants(accountId, itemMap) {
    const directChildren = byParent[accountId]
    if (!directChildren) {
      return itemMap[accountId]?.amount ?? 0
    }
    let total = 0
    for (const child of directChildren.values()) {
      total += sumDescendants(child.accountId, itemMap)
    }
    return total
  }
})

const prevTotal  = computed(() => prevItems.value.filter(i => !i.parentAccountId || !new Set(prevItems.value.map(x => x.accountId)).has(i.parentAccountId)).reduce((s, i) => s + (i.amount ?? 0), 0))
const consTotal  = computed(() => consItems.value.filter(i => !i.parentAccountId || !new Set(consItems.value.map(x => x.accountId)).has(i.parentAccountId)).reduce((s, i) => s + (i.amount ?? 0), 0))
const scostamento = computed(() => consTotal.value - prevTotal.value)

// ── Conguaglio ───────────────────────────────────────────────────
const loadingConguaglio = ref(false)
const conguaglio        = ref(null)
const conguaglioError   = ref(null)

async function loadConguaglio() {
  if (!selectedFiscalYearId.value) return
  if (conguaglio.value?.fiscalYearId === selectedFiscalYearId.value) return  // already loaded
  loadingConguaglio.value = true
  conguaglioError.value   = null
  conguaglio.value        = null
  try {
    const { data } = await fiscalYearApi.getConguaglio(selectedFiscalYearId.value)
    conguaglio.value = data
  } catch (err) {
    conguaglioError.value = err?.response?.data?.message
      ?? err?.response?.data?.title
      ?? 'Impossibile caricare il conguaglio. Verifica che il consuntivo sia approvato.'
  } finally {
    loadingConguaglio.value = false
  }
}

// ── Watchers ─────────────────────────────────────────────────────
watch(selectedFiscalYearId, async () => {
  conguaglio.value      = null
  conguaglioError.value = null
  await loadBudgets()
  await loadItems()
}, { immediate: true })

watch([preventivo, consuntivo], async () => {
  await loadItems()
})

// ── Formatters ───────────────────────────────────────────────────
const fmt     = (v) => v != null ? '€ ' + Number(v).toLocaleString('it-IT', { minimumFractionDigits: 2 }) : '—'
const fmtDate = (d) => d ? new Date(d).toLocaleDateString('it-IT') : '—'
</script>

<style scoped>
/* ── Tab pills ─────────────────────────────────────────── */
.tab-pills {
  display: flex;
  gap: 4px;
}
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

/* ── Summary bar ───────────────────────────────────────── */
.summary-bar {
  display: flex;
  gap: 24px;
  flex-wrap: wrap;
  padding: 12px 0;
}
.summary-item {
  display: flex;
  flex-direction: column;
  gap: 2px;
}
.summary-label {
  font-size: 0.75rem;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.04em;
}
.summary-value {
  font-size: 1.1rem;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
}

/* ── Table rows ────────────────────────────────────────── */
.row-header td {
  font-weight: 600;
  background: var(--bg-base);
}
.row-leaf td {
  background: var(--bg-surface);
}
.row-total td {
  border-top: 2px solid var(--border);
  background: var(--bg-base);
}

/* ── Conguaglio header card ────────────────────────────── */
.conguaglio-header {
  padding: 16px 20px;
}
.conguaglio-meta {
  display: flex;
  gap: 32px;
  font-size: 0.9rem;
  flex-wrap: wrap;
}

/* ── Colors ────────────────────────────────────────────── */
.text-green { color: var(--accent-green); }
.text-red   { color: var(--accent-red);   }

.badge-red { background: rgba(239,68,68,.12); color: var(--accent-red); }

/* ── Warning banner ────────────────────────────────────── */
.warning-banner {
  border: 1px solid #f59e0b;
  background: rgba(245,158,11,.08);
  color: #92400e;
  border-radius: 6px;
  padding: 10px 14px;
  font-size: 0.875rem;
}
</style>
