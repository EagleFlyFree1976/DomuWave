<template>
  <div class="licenze-page">

    <div class="page-header">
      <h1>La mia licenza</h1>
      <button class="btn btn-ghost" :disabled="redirecting" @click="goToPortale">
        <span v-if="redirecting" class="spinner" style="width:14px;height:14px"></span>
        🔗 Gestisci su LicenseManager
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="loading-state">
      <span class="spinner"></span> Caricamento…
    </div>

    <template v-else>

      <!-- ── Feature attive ─────────────────────────────────────── -->
      <div class="card">
        <div class="card-title">Feature attive</div>

        <div v-if="!status.length" class="empty-state">
          <div>Nessuna feature attiva per questo tenant.</div>
        </div>

        <div v-else class="feature-grid">
          <div v-for="f in status" :key="f.code" class="feature-card"
               :class="{ 'feature-card--exhausted': f.isExhausted, 'feature-card--warning': f.isWarning && !f.isExhausted }">

            <div class="feature-card-header">
              <span class="code-badge">{{ f.code }}</span>
              <span class="badge" :class="f.isExhausted ? 'badge-red' : 'badge-green'">
                {{ f.isExhausted ? 'Esaurita' : 'Attiva' }}
              </span>
            </div>

            <!-- Feature a consumo -->
            <template v-if="f.isConsumable">
              <div class="usage-bar-wrap">
                <div class="usage-bar">
                  <div class="usage-bar-fill"
                       :class="{ 'usage-bar-fill--warning': f.isWarning, 'usage-bar-fill--exhausted': f.isExhausted }"
                       :style="{ width: usagePercent(f) + '%' }">
                  </div>
                </div>
                <div class="usage-numbers">
                  <span>{{ f.used }} / {{ f.limit ?? '∞' }}</span>
                  <span v-if="f.remaining !== null" class="text-muted">
                    {{ f.remaining }} rimasti
                  </span>
                </div>
              </div>

              <div v-if="f.isWarning && !f.isExhausted" class="feature-alert feature-alert--warning">
                ⚠️ Stai per esaurire i crediti disponibili
              </div>
              <div v-if="f.isExhausted" class="feature-alert feature-alert--error">
                ⛔ Crediti esauriti — acquista un pacchetto aggiuntivo
              </div>
            </template>

            <!-- Feature on/off -->
            <div v-else class="feature-onoff text-muted">
              Accesso illimitato
            </div>

          </div>
        </div>
      </div>

      <!-- ── Piani disponibili ──────────────────────────────────── -->
      <div class="card">
        <div class="card-title">Piani disponibili</div>

        <div v-if="!plans.length" class="empty-state">
          Nessun piano configurato.
        </div>

        <div v-else class="plans-grid">
          <div v-for="plan in plans" :key="plan.id" class="plan-card"
               :class="{ 'plan-card--featured': plan.badge }">
            <div v-if="plan.badge" class="plan-badge">{{ plan.badge }}</div>
            <div class="plan-name">{{ plan.name }}</div>

            <div class="plan-features">
              <div v-if="plan.allFeatures" class="plan-feature-item">
                <span class="check">✓</span> Tutte le feature incluse
              </div>
              <div v-for="f in plan.features" :key="f.featureId" class="plan-feature-item">
                <span class="check">✓</span>
                <span class="code-badge code-badge--sm">{{ f.code }}</span>
                {{ f.name }}
                <span v-if="f.includedUsage" class="text-muted">({{ f.includedUsage }} inclusi)</span>
              </div>
            </div>

            <div class="plan-pricings">
              <div v-for="pr in sortedPricings(plan)" :key="pr.id" class="plan-pricing-row">
                <span class="plan-period">{{ formatPeriod(pr.billingPeriod) }}</span>
                <span class="plan-price">€ {{ Number(pr.price).toFixed(2) }}</span>
              </div>
            </div>

            <button class="btn btn-primary btn-sm" @click="goToPortale">Acquista</button>
          </div>
        </div>
      </div>

    </template>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { licenseApi } from '@/services/api'
import { useFeatureStatus } from '@/composables/useFeatureStatus'

const loading     = ref(true)
const redirecting = ref(false)
const plans       = ref([])
const status      = ref([])

const { load: loadStatus } = useFeatureStatus()

onMounted(async () => {
  loading.value = true
  try {
    await loadStatus()
    const [statusRes, plansRes] = await Promise.all([
      licenseApi.getStatus(),
      licenseApi.getPlans(),
    ])
    status.value = statusRes.data ?? []
    plans.value  = plansRes.data  ?? []
  } catch {
    // errori gestiti globalmente
  } finally {
    loading.value = false
  }
})

async function goToPortale() {
  redirecting.value = true
  try {
    const { data } = await licenseApi.getHandoffUrl()
    if (data?.url) window.location.href = data.url
  } catch {
    redirecting.value = false
  }
}

function usagePercent(f) {
  if (!f.limit) return 0
  return Math.min(100, Math.round((f.used / f.limit) * 100))
}

const periodOrder = { monthly: 0, yearly: 1, 'one-time': 2 }
function sortedPricings(plan) {
  return [...(plan.pricings ?? [])].sort((a, b) => (periodOrder[a.billingPeriod] ?? 99) - (periodOrder[b.billingPeriod] ?? 99))
}
function formatPeriod(bp) {
  return bp === 'yearly' ? 'Annuale' : bp === 'monthly' ? 'Mensile' : 'Una tantum'
}
</script>

<style scoped>
.licenze-page { display: flex; flex-direction: column; gap: 24px; max-width: 960px; }

.card         { background: var(--bg-surface); border: 1px solid var(--border); border-radius: 12px; padding: 24px; }
.card-title   { font-size: 15px; font-weight: 700; color: var(--text); margin-bottom: 20px; }

/* Feature grid */
.feature-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
  gap: 16px;
}

.feature-card {
  background: var(--bg-base);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.feature-card--warning  { border-color: #f59e0b; }
.feature-card--exhausted { border-color: var(--accent-red, #ef4444); }

.feature-card-header { display: flex; align-items: center; justify-content: space-between; gap: 8px; }

.badge-green { background: #dcfce7; color: #16a34a; font-size: 11px; font-weight: 600; padding: 2px 8px; border-radius: 20px; }
.badge-red   { background: #fee2e2; color: #dc2626; font-size: 11px; font-weight: 600; padding: 2px 8px; border-radius: 20px; }

/* Usage bar */
.usage-bar-wrap  { display: flex; flex-direction: column; gap: 6px; }
.usage-bar       { height: 8px; background: var(--border); border-radius: 4px; overflow: hidden; }
.usage-bar-fill  { height: 100%; background: var(--accent); border-radius: 4px; transition: width .3s; }
.usage-bar-fill--warning   { background: #f59e0b; }
.usage-bar-fill--exhausted { background: var(--accent-red, #ef4444); }
.usage-numbers   { display: flex; justify-content: space-between; font-size: 12px; font-weight: 500; }

.feature-onoff { font-size: 13px; }

.feature-alert          { font-size: 12px; padding: 6px 10px; border-radius: 6px; }
.feature-alert--warning { background: #fef9c3; color: #a16207; }
.feature-alert--error   { background: #fee2e2; color: #dc2626; }

/* Plans grid */
.plans-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 20px;
}

.plan-card {
  background: var(--bg-base);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 14px;
  position: relative;
}
.plan-card--featured { border-color: var(--accent); box-shadow: 0 0 0 2px var(--accent-glow); }

.plan-badge {
  position: absolute; top: -11px; left: 50%; transform: translateX(-50%);
  background: var(--accent); color: #fff; font-size: 11px; font-weight: 600;
  padding: 2px 14px; border-radius: 20px; text-transform: uppercase; letter-spacing: .05em;
  white-space: nowrap;
}

.plan-name     { font-size: 16px; font-weight: 700; color: var(--text); }

.plan-features     { display: flex; flex-direction: column; gap: 6px; }
.plan-feature-item { display: flex; align-items: center; gap: 6px; font-size: 13px; color: var(--text-secondary); }
.check             { color: var(--accent-green, #22c55e); font-weight: 700; flex-shrink: 0; }

.code-badge--sm { font-size: 10px; padding: 1px 5px; }

.plan-pricings   { display: flex; flex-direction: column; gap: 4px; }
.plan-pricing-row { display: flex; justify-content: space-between; align-items: center; }
.plan-period     { font-size: 13px; color: var(--text-secondary); }
.plan-price      { font-size: 18px; font-weight: 800; color: var(--accent); }

.btn-sm { padding: 6px 14px; font-size: 13px; }
</style>
