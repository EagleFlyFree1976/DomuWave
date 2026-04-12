<template>
  <div>
    <div class="page-header">
      <button class="btn btn-ghost btn-sm" @click="router.back()">← Torna al budget</button>
      <h1>Dettaglio consuntivo</h1>
      <span v-if="detail" class="text-secondary">{{ detail.fiscalYear }}</span>
    </div>

    <div v-if="loading" class="loading-state"><div class="spinner"></div></div>
    <div v-else-if="!detail || (!detail.accounts.length && !detail.units.length)" class="empty-state">
      <div class="empty-icon">◎</div>
      <div>Nessuna allocazione registrata per questo consuntivo</div>
    </div>
    <template v-else>

      <!-- ── Tab ──────────────────────────────────────────────────────────── -->
      <div class="detail-tabs">
        <button class="detail-tab-btn" :class="{ active: tab === 'accounts' }" @click="tab = 'accounts'">
          Per conto
        </button>
        <button class="detail-tab-btn" :class="{ active: tab === 'units' }" @click="tab = 'units'">
          Per unità
          <span v-if="!detail.hasAllocations" class="tab-badge-warn" title="Nessuna ripartizione millesimale calcolata">!</span>
        </button>
      </div>

      <!-- ══════════════════════════════════════════════════════════════════
           TAB 1 — Per conto
      ══════════════════════════════════════════════════════════════════ -->
      <div v-if="tab === 'accounts'">
        <div class="card" v-for="acc in detail.accounts" :key="acc.accountId">
          <div class="acc-header" @click="toggleAccount(acc.accountId)">
            <span class="acc-expand-icon">{{ expandedAccounts.has(acc.accountId) ? '▾' : '▸' }}</span>
            <span class="mono text-secondary acc-code">{{ acc.accountCode }}</span>
            <span class="acc-name">{{ acc.accountName }}</span>
            <span class="mono acc-total">{{ fmt(acc.totalAmount) }}</span>
          </div>

          <div v-if="expandedAccounts.has(acc.accountId)" class="acc-body">
            <div v-for="exp in acc.expenses" :key="exp.expenseId" class="expense-block">
              <div class="expense-header" @click="toggleExpense(exp.expenseId)">
                <span class="exp-expand-icon">{{ expandedExpenses.has(exp.expenseId) ? '▾' : '▸' }}</span>
                <span class="text-secondary" style="font-size:0.82em">{{ fmtDate(exp.documentDate) }}</span>
                <span class="exp-name">{{ exp.name }}</span>
                <span v-if="exp.supplierName" class="text-muted exp-supplier">{{ exp.supplierName }}</span>
                <span class="mono exp-amount">{{ fmt(exp.grossAmount) }}</span>
              </div>

              <table v-if="expandedExpenses.has(exp.expenseId)" class="alloc-table">
                <thead>
                  <tr>
                    <th>Unità</th>
                    <th class="text-right">Millesimi</th>
                    <th class="text-right">Importo (€)</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="a in exp.allocations" :key="a.unitId">
                    <td>{{ a.unitName }}</td>
                    <td class="mono text-right">{{ a.millesimal.toFixed(3) }}</td>
                    <td class="mono text-right">{{ fmt(a.allocatedAmount) }}</td>
                  </tr>
                </tbody>
                <tfoot>
                  <tr>
                    <td class="text-secondary">Totale</td>
                    <td class="mono text-right">{{ exp.allocations.reduce((s,a) => s + a.millesimal, 0).toFixed(3) }}</td>
                    <td class="mono text-right">{{ fmt(exp.allocations.reduce((s,a) => s + a.allocatedAmount, 0)) }}</td>
                  </tr>
                </tfoot>
              </table>
            </div>
          </div>
        </div>
      </div>

      <!-- ══════════════════════════════════════════════════════════════════
           TAB 2 — Per unità (pivot)
      ══════════════════════════════════════════════════════════════════ -->
      <div v-if="tab === 'units' && !detail.hasAllocations" class="empty-state">
        <div class="empty-icon">◎</div>
        <div>Nessuna ripartizione millesimale disponibile.</div>
        <div class="text-muted" style="font-size:0.85rem;margin-top:0.25rem">
          Le allocazioni per unità vengono calcolate automaticamente quando si registra una spesa con una tabella millesimale.
        </div>
      </div>
      <div v-else-if="tab === 'units'" class="card">
        <div class="table-wrap">
          <table class="pivot-table">
            <thead>
              <tr>
                <th>Unità</th>
                <th v-for="acc in allAccounts" :key="acc.accountId" class="text-right pivot-col-header">
                  <span class="mono pivot-col-code">{{ acc.accountCode }}</span>
                  <span class="pivot-col-name">{{ acc.accountName }}</span>
                </th>
                <th class="text-right pivot-total-col">Totale</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="unit in detail.units" :key="unit.unitId">
                <td class="unit-name-cell">{{ unit.unitName }}</td>
                <td v-for="acc in allAccounts" :key="acc.accountId" class="mono text-right">
                  {{ fmt(amountForUnit(unit, acc.accountId)) }}
                </td>
                <td class="mono text-right pivot-total-cell">{{ fmt(unit.total) }}</td>
              </tr>
            </tbody>
            <tfoot>
              <tr class="pivot-footer-row">
                <td class="text-secondary">Totale</td>
                <td v-for="acc in allAccounts" :key="acc.accountId" class="mono text-right">
                  {{ fmt(detail.accounts.find(a => a.accountId === acc.accountId)?.totalAmount ?? 0) }}
                </td>
                <td class="mono text-right pivot-total-cell">
                  {{ fmt(detail.units.reduce((s, u) => s + u.total, 0)) }}
                </td>
              </tr>
            </tfoot>
          </table>
        </div>
      </div>

    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAppStore } from '@/stores/app'
import { budgetApi } from '@/services/api'

const route  = useRoute()
const router = useRouter()
const store  = useAppStore()

const budgetId = Number(route.params.budgetId)

const loading = ref(false)
const detail  = ref(null)
const tab     = ref('accounts')

const expandedAccounts = ref(new Set())
const expandedExpenses = ref(new Set())

function toggleAccount(id) {
  if (expandedAccounts.value.has(id)) expandedAccounts.value.delete(id)
  else expandedAccounts.value.add(id)
  expandedAccounts.value = new Set(expandedAccounts.value)
}

function toggleExpense(id) {
  if (expandedExpenses.value.has(id)) expandedExpenses.value.delete(id)
  else expandedExpenses.value.add(id)
  expandedExpenses.value = new Set(expandedExpenses.value)
}

// Tutti i conti presenti (per la pivot), ordinati per codice
const allAccounts = computed(() =>
  (detail.value?.accounts ?? []).map(a => ({ accountId: a.accountId, accountCode: a.accountCode, accountName: a.accountName }))
)

function amountForUnit(unit, accountId) {
  return unit.entries.find(e => e.accountId === accountId)?.allocatedAmount ?? 0
}

const fmt     = (v) => new Intl.NumberFormat('it-IT', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(v ?? 0)
const fmtDate = (d) => d ? new Date(d).toLocaleDateString('it-IT') : '—'

async function load() {
  loading.value = true
  try {
    const { data } = await budgetApi.consuntivoDetail(budgetId)
    detail.value = data
    // Espandi tutti i conti di default
    expandedAccounts.value = new Set((data?.accounts ?? []).map(a => a.accountId))
  } catch {
    detail.value = null
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<style scoped>
.page-header { display: flex; align-items: center; gap: 1rem; margin-bottom: 1.5rem; flex-wrap: wrap; }
.page-header h1 { margin: 0; font-size: 1.4rem; }

/* Tab */
.detail-tabs { display: flex; gap: 0.5rem; margin-bottom: 1rem; }
.detail-tab-btn {
  padding: 0.4rem 1.1rem;
  border: 1px solid var(--border);
  border-radius: 6px;
  background: transparent;
  color: var(--text-secondary);
  cursor: pointer;
  font-size: 0.9rem;
  transition: background 0.15s, color 0.15s;
}
.detail-tab-btn.active { background: var(--accent); color: #fff; border-color: var(--accent); }
.tab-badge-warn { display: inline-block; margin-left: 4px; background: var(--accent-red); color: #fff; border-radius: 50%; width: 14px; height: 14px; font-size: 0.65rem; line-height: 14px; text-align: center; font-weight: 700; }

/* Per conto */
.card { margin-bottom: 0.75rem; }

.acc-header {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  padding: 0.65rem 1rem;
  cursor: pointer;
  user-select: none;
  border-radius: 8px;
  transition: background 0.12s;
}
.acc-header:hover { background: var(--bg-surface); }
.acc-expand-icon { color: var(--text-muted); width: 1rem; flex-shrink: 0; }
.acc-code { font-size: 0.8em; min-width: 70px; }
.acc-name { flex: 1; font-weight: 600; }
.acc-total { font-size: 0.95rem; margin-left: auto; color: var(--accent-red); }

.acc-body { padding: 0 1rem 0.75rem 2rem; }

.expense-block { border-left: 2px solid var(--border); margin-bottom: 0.5rem; }
.expense-header {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  padding: 0.4rem 0.75rem;
  cursor: pointer;
  user-select: none;
  transition: background 0.12s;
  border-radius: 4px;
}
.expense-header:hover { background: var(--bg-surface); }
.exp-expand-icon { color: var(--text-muted); width: 1rem; flex-shrink: 0; }
.exp-name { flex: 1; }
.exp-supplier { font-size: 0.82em; color: var(--text-muted); }
.exp-amount { font-size: 0.9rem; margin-left: auto; }

.alloc-table { width: 100%; border-collapse: collapse; margin: 0.25rem 0.75rem 0.5rem; font-size: 0.88rem; }
.alloc-table th { text-align: left; padding: 0.3rem 0.5rem; color: var(--text-muted); font-weight: 500; border-bottom: 1px solid var(--border); }
.alloc-table td { padding: 0.3rem 0.5rem; border-bottom: 1px solid var(--border); }
.alloc-table tfoot td { font-weight: 600; border-top: 2px solid var(--border); border-bottom: none; }

/* Pivot per unità */
.pivot-table { border-collapse: collapse; font-size: 0.85rem; min-width: 100%; }
.pivot-table th, .pivot-table td { padding: 0.4rem 0.6rem; border-bottom: 1px solid var(--border); white-space: nowrap; }
.pivot-table th { color: var(--text-muted); font-weight: 500; background: var(--bg-surface); position: sticky; top: 0; }
.pivot-col-header { max-width: 110px; }
.pivot-col-code { display: block; font-size: 0.78em; color: var(--text-muted); }
.pivot-col-name { display: block; font-size: 0.82em; white-space: normal; line-height: 1.2; }
.unit-name-cell { font-weight: 500; position: sticky; left: 0; background: var(--bg-base); z-index: 1; }
.pivot-total-col { background: var(--bg-surface); font-weight: 600; }
.pivot-total-cell { font-weight: 600; color: var(--accent-red); }
.pivot-footer-row td { font-weight: 600; background: var(--bg-surface); border-top: 2px solid var(--border); }
</style>
