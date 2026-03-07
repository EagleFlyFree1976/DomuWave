<template>
  <div class="setup-page">

    <!-- ── HEADER ──────────────────────────────────────────────────── -->
    <div class="setup-header">
      <div>
        <h2 class="setup-title">
          <i class="pi pi-check-square" /> Checklist Operatività
        </h2>
        <p class="setup-subtitle">
          Verifica e completa tutti i passaggi per rendere il condominio pienamente operativo.
        </p>
      </div>
      <button class="btn btn-ghost" :disabled="loading" @click="load">
        <i class="pi pi-refresh" :class="{ 'pi-spin': loading }" /> Aggiorna
      </button>
    </div>

    <!-- ── LOADING ──────────────────────────────────────────────────── -->
    <div v-if="loading" class="state-box">
      <div class="spinner" /> Verifica in corso…
    </div>

    <template v-else-if="status">

      <!-- ── BARRA PROGRESSO GLOBALE ──────────────────────────────────── -->
      <div class="progress-card">
        <div class="progress-info">
          <span class="progress-label">Completamento</span>
          <span class="progress-pct" :class="progressClass">
            {{ status.completedSections }} / {{ status.totalSections }} sezioni completate
          </span>
        </div>
        <div class="progress-bar-wrap">
          <div class="progress-bar-track">
            <div class="progress-bar-fill" :class="progressClass" :style="{ width: progressPct + '%' }" />
          </div>
          <span class="progress-pct-num" :class="progressClass">{{ progressPct }}%</span>
        </div>
      </div>

      <!-- ── SEZIONI ─────────────────────────────────────────────────── -->
      <SectionCard
        v-for="s in sections"
        :key="s.key"
        :title="s.title"
        :icon="s.icon"
        :desc="s.desc"
        :section="status[s.key]"
        :link="s.link"
        :link-label="s.linkLabel"
      />

    </template>

  </div>
</template>

<script setup>
import { ref, computed, inject, onMounted } from 'vue'
import { condominiumApi } from '@/services/api'
import SectionCard from './setup/SectionCard.vue'

// ── inject dal CondominioLayout ──────────────────────────────────────────────
const condominiumId = inject('condominiumId')

// ── state ─────────────────────────────────────────────────────────────────────
const loading = ref(false)
const status  = ref(null)

// ── sezioni da visualizzare ───────────────────────────────────────────────────
const sections = computed(() => [
  {
    key: 'units',
    icon: '⊞', title: 'Unità Immobiliari',
    desc: 'Le unità devono essere censite per attivare la gestione millesimale e le quote.',
    link: `/condomini/${condominiumId.value}/unita`, linkLabel: 'Vai alle Unità',
  },
  {
    key: 'occupants',
    icon: '👤', title: 'Proprietari & Inquilini',
    desc: 'Ogni unità attiva dovrebbe avere almeno un proprietario o inquilino registrato.',
    link: `/condomini/${condominiumId.value}/panoramica`, linkLabel: 'Vai alla Panoramica',
  },
  {
    key: 'chartOfAccounts',
    icon: '◎', title: 'Piano dei Conti',
    desc: 'Necessario per la gestione del budget e delle spese.',
    link: '/piano-dei-conti', linkLabel: 'Vai al Piano dei Conti',
  },
  {
    key: 'millesimalTables',
    icon: '◑', title: 'Tabelle Millesimali',
    desc: 'Necessarie per la ripartizione delle spese tra le unità.',
    link: '/tabelle-millesimali', linkLabel: 'Vai alle Tabelle',
  },
  {
    key: 'fiscalYear',
    icon: '◷', title: 'Esercizio Fiscale',
    desc: 'Deve esistere un esercizio fiscale aperto per la gestione corrente.',
    link: '/esercizi-fiscali', linkLabel: 'Vai agli Esercizi',
  },
  {
    key: 'budget',
    icon: '◈', title: 'Budget Preventivo',
    desc: 'Necessario per generare le rate di pagamento.',
    link: `/condomini/${condominiumId.value}/budget`, linkLabel: 'Vai al Budget',
  },
])

// ── progress ──────────────────────────────────────────────────────────────────
const progressPct = computed(() => {
  if (!status.value) return 0
  return Math.round((status.value.completedSections / status.value.totalSections) * 100)
})
const progressClass = computed(() => {
  if (progressPct.value === 100) return 'status-ok'
  if (progressPct.value >= 60)   return 'status-warn'
  return 'status-error'
})

// ── load ──────────────────────────────────────────────────────────────────────
async function load() {
  loading.value = true
  try {
    const { data } = await condominiumApi.getSetupStatus(condominiumId.value)
    status.value = data
  } catch {
    status.value = null
  } finally {
    loading.value = false
  }
}

onMounted(() => load())
</script>

<style scoped>
.setup-page {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.setup-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
}
.setup-title {
  font-size: 1.1rem;
  font-weight: 700;
  color: var(--text-primary);
  margin: 0 0 4px;
  display: flex;
  align-items: center;
  gap: 8px;
}
.setup-subtitle {
  font-size: 0.8rem;
  color: var(--text-muted);
  margin: 0;
}

.progress-card {
  background: var(--surface2, var(--surface));
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 14px 18px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.progress-info { display: flex; justify-content: space-between; align-items: center; }
.progress-label { font-size: 0.8rem; color: var(--text-muted); font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px; }
.progress-pct   { font-size: 0.8rem; font-weight: 700; }
.progress-bar-wrap { display: flex; align-items: center; gap: 10px; }
.progress-bar-track { flex: 1; height: 8px; background: var(--border); border-radius: 4px; overflow: hidden; }
.progress-bar-fill  { height: 100%; border-radius: 4px; transition: width 0.4s ease; }
.progress-pct-num   { font-size: 0.75rem; font-weight: 700; min-width: 36px; text-align: right; }

.status-ok    { color: #22c55e; }
.status-warn  { color: #f59e0b; }
.status-error { color: #ef4444; }
.progress-bar-fill.status-ok    { background: #22c55e; }
.progress-bar-fill.status-warn  { background: #f59e0b; }
.progress-bar-fill.status-error { background: #ef4444; }

.state-box {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 40px;
  justify-content: center;
  color: var(--text-muted);
  font-size: 0.9rem;
}
</style>
