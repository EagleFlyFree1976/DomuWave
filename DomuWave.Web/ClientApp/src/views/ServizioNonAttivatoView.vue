<script setup>
import { ref, onMounted } from 'vue'
import { licenseApi } from '@/services/api'

const plans    = ref([])
const loading  = ref(false)
const redirecting = ref(false)

onMounted(async () => {
  loading.value = true
  try {
    const { data } = await licenseApi.getPlans()
    plans.value = data ?? []
  } catch {
    // errore gestito globalmente
  } finally {
    loading.value = false
  }
})

async function acquista() {
  redirecting.value = true
  try {
    const { data } = await licenseApi.getHandoffUrl()
    if (data?.url) window.location.href = data.url
  } catch {
    redirecting.value = false
  }
}

function formatPrice(price, billingPeriod) {
  const period = billingPeriod === 'yearly' ? 'anno' : 'mese'
  return `€ ${Number(price).toFixed(2)} / ${period}`
}
</script>

<template>
  <div class="nolicence-page">
    <div class="nolicence-hero">
      <div class="nolicence-icon">🔒</div>
      <h1>Servizio non attivato</h1>
      <p class="nolicence-sub">
        Questa funzionalità non è inclusa nella tua licenza attuale.<br>
        Scegli un piano per sbloccare l'accesso completo a DomuWave.
      </p>
    </div>

    <div v-if="loading" class="loading-state">
      <span class="spinner"></span> Caricamento piani…
    </div>

    <div v-else-if="plans.length" class="plans-grid">
      <div
        v-for="plan in plans"
        :key="plan.id"
        class="plan-card"
        :class="{ 'plan-card--featured': plan.badge === 'popular' }"
      >
        <div v-if="plan.badge" class="plan-badge">{{ plan.badge }}</div>
        <h2 class="plan-name">{{ plan.name }}</h2>
        <div class="plan-price">{{ formatPrice(plan.price, plan.billingPeriod) }}</div>
        <ul class="plan-features">
          <li v-for="f in plan.features" :key="f.id">
            <span class="plan-check">✓</span> {{ f.name }}
          </li>
        </ul>
      </div>
    </div>

    <div v-else class="empty-state">
      Nessun piano disponibile al momento.
    </div>

    <div class="nolicence-actions">
      <button class="btn btn-primary" :disabled="redirecting" @click="acquista">
        <span v-if="redirecting" class="spinner" style="width:14px;height:14px"></span>
        Acquista licenza
      </button>
      <button class="btn btn-ghost" @click="$router.back()">Torna indietro</button>
    </div>
  </div>
</template>

<style scoped>
.nolicence-page {
  max-width: 900px;
  margin: 48px auto;
  padding: 0 24px;
  display: flex;
  flex-direction: column;
  gap: 40px;
}

.nolicence-hero {
  text-align: center;
}

.nolicence-icon {
  font-size: 3rem;
  margin-bottom: 16px;
}

.nolicence-hero h1 {
  font-size: 2rem;
  font-weight: 700;
  color: var(--text);
  margin: 0 0 12px;
}

.nolicence-sub {
  color: var(--text-secondary);
  font-size: 1.05rem;
  line-height: 1.6;
}

.plans-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 20px;
}

.plan-card {
  background: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 28px 24px;
  position: relative;
  display: flex;
  flex-direction: column;
  gap: 16px;
  transition: box-shadow 0.2s;
}

.plan-card:hover {
  box-shadow: 0 4px 20px rgba(0,0,0,0.08);
}

.plan-card--featured {
  border-color: var(--accent);
  box-shadow: 0 0 0 2px var(--accent-glow);
}

.plan-badge {
  position: absolute;
  top: -12px;
  left: 50%;
  transform: translateX(-50%);
  background: var(--accent);
  color: #fff;
  font-size: 0.75rem;
  font-weight: 600;
  padding: 3px 14px;
  border-radius: 20px;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.plan-name {
  font-size: 1.15rem;
  font-weight: 700;
  color: var(--text);
  margin: 0;
}

.plan-price {
  font-size: 1.6rem;
  font-weight: 800;
  color: var(--accent);
}

.plan-features {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
  color: var(--text-secondary);
  font-size: 0.9rem;
}

.plan-check {
  color: var(--accent-green);
  font-weight: 700;
  margin-right: 6px;
}

.nolicence-actions {
  display: flex;
  gap: 12px;
  justify-content: center;
}
</style>
