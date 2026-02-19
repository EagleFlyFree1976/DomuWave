<template>
  <div v-if="condominio">
    <div class="page-header">
      <div class="flex items-center gap-3">
        <router-link to="/condomini" class="btn btn-ghost btn-sm">← Indietro</router-link>
        <h1>{{ condominio.name }}</h1>
        <span class="badge" :class="condominio.isActive ? 'badge-green' : 'badge-muted'">{{ condominio.isActive ? 'Attivo' : 'Inattivo' }}</span>
      </div>
      <button class="btn btn-ghost" @click="edit = !edit">{{ edit ? 'Annulla' : 'Modifica' }}</button>
    </div>

    <div class="detail-grid">
      <!-- Info card -->
      <div class="card">
        <div class="card-header"><h2>Informazioni generali</h2></div>
        <dl class="info-list">
          <div class="info-row"><dt>Codice</dt><dd class="mono">{{ condominio.code || '—' }}</dd></div>
          <div class="info-row"><dt>Cod. fiscale</dt><dd class="mono">{{ condominio.taxCode || '—' }}</dd></div>
          <div class="info-row"><dt>P.IVA</dt><dd class="mono">{{ condominio.vatNumber || '—' }}</dd></div>
          <div class="info-row"><dt>Email</dt><dd>{{ condominio.email || '—' }}</dd></div>
          <div class="info-row"><dt>Telefono</dt><dd>{{ condominio.phone || '—' }}</dd></div>
          <div class="info-row"><dt>PEC</dt><dd>{{ condominio.pec || '—' }}</dd></div>
        </dl>
      </div>

      <!-- Numeri card -->
      <div class="card">
        <div class="card-header"><h2>Dati tecnici</h2></div>
        <dl class="info-list">
          <div class="info-row"><dt>Unità</dt><dd>{{ condominio.numberOfUnits }}</dd></div>
          <div class="info-row"><dt>Scale</dt><dd>{{ condominio.numberOfStaircases }}</dd></div>
          <div class="info-row"><dt>Piani</dt><dd>{{ condominio.numberOfFloors || '—' }}</dd></div>
          <div class="info-row"><dt>Anno costruz.</dt><dd>{{ condominio.yearOfConstruction || '—' }}</dd></div>
          <div class="info-row"><dt>Millesimi tot.</dt><dd class="mono">{{ condominio.totalMillesimal }}</dd></div>
          <div class="info-row"><dt>Ascensore</dt><dd>{{ condominio.hasElevator ? '✓ Sì' : '✕ No' }}</dd></div>
          <div class="info-row"><dt>Riscald. central.</dt><dd>{{ condominio.hasCentralHeating ? '✓ Sì' : '✕ No' }}</dd></div>
          <div class="info-row"><dt>Portineria</dt><dd>{{ condominio.hasConcierge ? '✓ Sì' : '✕ No' }}</dd></div>
        </dl>
      </div>

      <!-- Rate card -->
      <div class="card">
        <div class="card-header"><h2>Configurazione rate</h2></div>
        <dl class="info-list">
          <div class="info-row"><dt>Frequenza</dt><dd>{{ freqLabel }}</dd></div>
          <div class="info-row"><dt>Giorno scadenza</dt><dd>{{ condominio.installmentDueDay }}° del mese</dd></div>
          <div class="info-row"><dt>Inizio mandato</dt><dd>{{ fmtDate(condominio.mandateStartDate) }}</dd></div>
          <div class="info-row"><dt>Fine mandato</dt><dd>{{ fmtDate(condominio.mandateEndDate) }}</dd></div>
          <div class="info-row"><dt>Ultima assemblea</dt><dd>{{ fmtDate(condominio.lastAssemblyDate) }}</dd></div>
        </dl>
      </div>

      <!-- Note card -->
      <div class="card" v-if="condominio.notes">
        <div class="card-header"><h2>Note</h2></div>
        <p style="white-space:pre-wrap;line-height:1.7">{{ condominio.notes }}</p>
      </div>
    </div>

    <!-- Quick nav -->
    <div class="quick-nav">
      <router-link v-for="q in quickNav" :key="q.to" :to="q.to" class="qnav-chip">
        {{ q.icon }} {{ q.label }}
      </router-link>
    </div>
  </div>

  <div v-else class="loading-state">
    <div class="spinner"></div> Caricamento…
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { condominiumApi } from '@/services/api'

const route = useRoute()
const condominio = ref(null)
const edit = ref(false)

const freqMap = { Monthly: 'Mensile', Quarterly: 'Trimestrale', Biannual: 'Semestrale', Annual: 'Annuale' }
const freqLabel = computed(() => freqMap[condominio.value?.installmentFrequency] || '—')

function fmtDate(d) {
  if (!d) return '—'
  return new Date(d).toLocaleDateString('it-IT')
}

const quickNav = [
  { to: '/unita',  icon: '⊞', label: 'Unità' },
  { to: '/budget', icon: '◎', label: 'Budget' },
  { to: '/rate',   icon: '◷', label: 'Rate' },
  { to: '/fornitori', icon: '◈', label: 'Fornitori' },
  { to: '/documenti', icon: '▤', label: 'Documenti' },
  { to: '/comunicazioni', icon: '◉', label: 'Comunicazioni' },
]

onMounted(async () => {
  const { data } = await condominiumApi.getById(route.params.id)
  condominio.value = data
})
</script>

<style scoped>
.detail-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 1rem;
  margin-bottom: 1.5rem;
}
.info-list { display: flex; flex-direction: column; gap: 0; }
.info-row {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  padding: 0.55rem 0;
  border-bottom: 1px solid var(--border);
  gap: 1rem;
}
.info-row:last-child { border-bottom: none; }
dt { font-size: 0.8rem; color: var(--text-muted); flex-shrink: 0; }
dd { font-size: 0.875rem; color: var(--text-primary); text-align: right; }

.quick-nav { display: flex; flex-wrap: wrap; gap: 0.5rem; }
.qnav-chip {
  padding: 0.4rem 0.85rem;
  border-radius: 99px;
  border: 1px solid var(--border);
  background: var(--bg-surface);
  color: var(--text-secondary);
  font-size: 0.82rem;
  text-decoration: none;
  transition: all 0.15s;
}
.qnav-chip:hover { background: var(--bg-hover); color: var(--text-primary); text-decoration: none; }
</style>
