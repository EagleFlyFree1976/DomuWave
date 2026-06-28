<template>
  <div class="payments-page">

    <!-- ── HEADER ─────────────────────────────────────────────────────────── -->
    <div class="page-header">
      <div class="page-header__left">
        <h1 class="page-title">
          <i class="pi pi-credit-card page-title__icon" />
          Pagamenti Online
        </h1>
        <span class="page-subtitle">
          Abilita i condòmini a pagare le quote con carta tramite Stripe. L'incasso arriva
          direttamente sul conto del condominio.
        </span>
      </div>
    </div>

    <!-- ── NO CONDOMINIO ──────────────────────────────────────────────────── -->
    <div v-if="!store.selectedCondominioId" class="info-card">
      <i class="pi pi-info-circle" />
      <span>Seleziona un condominio per configurare i pagamenti online.</span>
    </div>

    <!-- ── LOADING ────────────────────────────────────────────────────────── -->
    <div v-else-if="loading" class="loading-state">
      <i class="pi pi-spinner pi-spin loading-spinner" />
    </div>

    <!-- ── CONFIG CARD ────────────────────────────────────────────────────── -->
    <div v-else class="config-card">
      <div class="config-card__head">
        <span class="config-card__title"><i class="pi pi-credit-card" /> Stripe</span>
        <span class="status-badge" :class="onboardingComplete ? 'status-badge--active' : 'status-badge--inactive'">
          <span class="status-dot" />
          {{ onboardingComplete ? 'Pagamenti attivi' : 'Non attivo' }}
        </span>
      </div>

      <div class="config-cond">{{ store.selectedCondominio?.name }}</div>

      <p class="config-desc">
        <template v-if="onboardingComplete">
          Questo condominio può ricevere pagamenti online. I condòmini vedranno il pulsante
          “Paga online” sulle proprie quote.
        </template>
        <template v-else>
          Collega un account Stripe per attivare i pagamenti online. Verrai reindirizzato a
          Stripe per inserire i dati del condominio e l'IBAN su cui ricevere gli incassi.
        </template>
      </p>

      <div class="config-actions">
        <Button :icon="onboardingComplete ? 'pi pi-external-link' : 'pi pi-link'"
                :label="onboardingComplete ? 'Gestisci su Stripe' : 'Collega Stripe'"
                class="btn-primary" :loading="working" @click="startOnboarding" />
        <Button icon="pi pi-refresh" label="Aggiorna stato"
                class="btn-ghost" :loading="working" @click="refreshStatus" />
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useAppStore } from '@/stores/app'
import { condominiumApi, condominiumPaymentApi } from '@/services/api'

import Button from 'primevue/button'

const store = useAppStore()
const route = useRoute()

const loading            = ref(false)
const working            = ref(false)
const onboardingComplete = ref(false)

async function loadStatus() {
  if (!store.selectedCondominioId) return
  loading.value = true
  try {
    const { data } = await condominiumApi.getById(store.selectedCondominioId)
    onboardingComplete.value = !!data?.stripeOnboardingComplete
  } catch {
    // gestito dall'handler globale api:error
  } finally {
    loading.value = false
  }
}

async function startOnboarding() {
  if (working.value) return
  working.value = true
  try {
    const { data } = await condominiumPaymentApi.startOnboarding(store.selectedCondominioId)
    if (data?.url) window.location.href = data.url
    else store.toast('Impossibile avviare il collegamento Stripe', 'error')
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    working.value = false
  }
}

async function refreshStatus() {
  if (working.value) return
  working.value = true
  try {
    const { data } = await condominiumPaymentApi.refreshStatus(store.selectedCondominioId)
    onboardingComplete.value = !!data?.complete
    store.toast(onboardingComplete.value ? 'Pagamenti online attivi' : 'Onboarding non ancora completato',
                onboardingComplete.value ? 'success' : 'info')
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    working.value = false
  }
}

onMounted(async () => {
  await loadStatus()
  // Ritorno dall'onboarding Stripe: ricontrolla lo stato reale.
  if (route.query.onboarding) await refreshStatus()
})
watch(() => store.selectedCondominioId, loadStatus)
</script>

<style scoped>
/* ── PAGE ─────────────────────────────────────────────────────────────────── */
.payments-page {
  display: flex;
  flex-direction: column;
  gap: 20px;
  padding: 28px 32px;
  min-height: 100%;
}

/* ── HEADER ──────────────────────────────────────────────────────────────── */
.page-header { display: flex; align-items: flex-start; }
.page-header__left { display: flex; flex-direction: column; gap: 4px; }
.page-title {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 22px;
  font-weight: 700;
  color: var(--text);
  margin: 0;
}
.page-title__icon { color: var(--accent); font-size: 20px; }
.page-subtitle { font-size: 13px; color: var(--text-dim); max-width: 680px; }

/* ── INFO / LOADING ──────────────────────────────────────────────────────── */
.info-card {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 14px 16px;
  font-size: 13px;
  color: var(--text-dim);
  background: var(--surface2);
  border: 1px solid var(--border);
  border-radius: 10px;
}
.info-card .pi { color: var(--accent); }
.loading-state {
  display: flex;
  justify-content: center;
  padding: 40px 0;
}
.loading-spinner { font-size: 28px; color: var(--accent); }

/* ── CONFIG CARD ─────────────────────────────────────────────────────────── */
.config-card {
  display: flex;
  flex-direction: column;
  gap: 12px;
  max-width: 680px;
  background: var(--surface2);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 18px;
}
.config-card__head {
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.config-card__title {
  display: flex;
  align-items: center;
  gap: 7px;
  font-size: 12px;
  font-weight: 600;
  color: var(--text-dim);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
.config-card__title .pi { color: var(--accent); }
.config-cond { font-size: 15px; font-weight: 600; color: var(--text); }
.config-desc { margin: 0; font-size: 13px; color: var(--text-dim); line-height: 1.55; }

/* ── STATUS BADGE ────────────────────────────────────────────────────────── */
.status-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 3px 10px;
  border-radius: 999px;
  font-size: 11px;
  font-weight: 600;
  border: 1px solid transparent;
}
.status-dot { width: 7px; height: 7px; border-radius: 50%; background: currentColor; }
.status-badge--active {
  background: rgba(52,211,153,0.10);
  border-color: rgba(52,211,153,0.30);
  color: var(--accent);
}
.status-badge--inactive {
  background: rgba(148,163,184,0.10);
  border-color: rgba(148,163,184,0.30);
  color: var(--text-dim);
}

/* ── ACTIONS / BUTTONS ───────────────────────────────────────────────────── */
.config-actions { display: flex; gap: 10px; margin-top: 4px; }
.btn-primary {
  background: var(--accent) !important;
  border-color: var(--accent) !important;
  color: #000 !important;
  font-weight: 600 !important;
  font-size: 13px !important;
}
.btn-ghost {
  background: transparent !important;
  border-color: var(--border) !important;
  color: var(--text-dim) !important;
}

@media (max-width: 768px) {
  .payments-page { padding: 16px; }
  .config-actions { flex-direction: column; align-items: stretch; }
}
</style>
