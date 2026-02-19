<template>
  <div>
    <div class="page-header">
      <h1>Dashboard</h1>
      <span class="badge badge-green">● Live</span>
    </div>

    <!-- Stats grid -->
    <div class="stats-grid">
      <div class="stat-card">
        <div class="stat-label">Condomini attivi</div>
        <div class="stat-value">{{ store.condomini.length }}</div>
        <div class="stat-sub">Totale gestiti</div>
      </div>
      <div class="stat-card">
        <div class="stat-label">Unità immobiliari</div>
        <div class="stat-value text-accent">{{ totalUnits }}</div>
        <div class="stat-sub">Tra tutti i condomini</div>
      </div>
      <div class="stat-card">
        <div class="stat-label">Rate aperte</div>
        <div class="stat-value text-amber">{{ openInstallments }}</div>
        <div class="stat-sub">In attesa di pagamento</div>
      </div>
      <div class="stat-card">
        <div class="stat-label">Spese non pagate</div>
        <div class="stat-value text-red">{{ unpaidExpenses }}</div>
        <div class="stat-sub">Da liquidare</div>
      </div>
    </div>

    <!-- Quick links -->
    <div class="quick-section">
      <h2 style="margin-bottom:1rem">Accesso rapido</h2>
      <div class="quick-grid">
        <router-link v-for="q in quickLinks" :key="q.to" :to="q.to" class="quick-card">
          <span class="quick-icon">{{ q.icon }}</span>
          <span class="quick-label">{{ q.label }}</span>
          <span class="quick-arrow">→</span>
        </router-link>
      </div>
    </div>

    <!-- Condomini list preview -->
    <div class="card" style="margin-top:1.5rem" v-if="store.condomini.length">
      <div class="card-header">
        <h2>Condomini</h2>
        <router-link to="/condomini" class="btn btn-ghost btn-sm">Vedi tutti</router-link>
      </div>
      <div class="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Nome</th>
              <th>Codice</th>
              <th>Unità</th>
              <th>Frequenza rate</th>
              <th>Stato</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="c in store.condomini.slice(0, 5)" :key="c.id">
              <td>
                <router-link :to="`/condomini/${c.id}`" class="text-accent">{{ c.name }}</router-link>
              </td>
              <td class="mono text-muted">{{ c.code || '—' }}</td>
              <td>{{ c.numberOfUnits }}</td>
              <td>{{ c.installmentFrequency }}</td>
              <td><span class="badge" :class="c.isActive ? 'badge-green' : 'badge-muted'">{{ c.isActive ? 'Attivo' : 'Inattivo' }}</span></td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div v-else class="empty-state">
      <div class="empty-icon">⬡</div>
      <div>Nessun condominio trovato</div>
      <router-link to="/condomini" class="btn btn-primary" style="margin-top:0.5rem">Aggiungi condominio</router-link>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAppStore } from '@/store/app'
import { unitApi, installmentApi, expenseApi } from '@/services/api'

const store = useAppStore()
const totalUnits = ref(0)
const openInstallments = ref(0)
const unpaidExpenses = ref(0)

const quickLinks = [
  { to: '/condomini',     icon: '⬡', label: 'Gestisci Condomini' },
  { to: '/unita',         icon: '⊞', label: 'Unità Immobiliari' },
  { to: '/budget',        icon: '◎', label: 'Budget & Spese' },
  { to: '/rate',          icon: '◷', label: 'Rate & Quote' },
  { to: '/fornitori',     icon: '◈', label: 'Fornitori' },
  { to: '/documenti',     icon: '▤', label: 'Documenti' },
]

onMounted(async () => {
  if (!store.condomini.length) await store.loadCondomini()

  if (store.selectedCondominioId) {
    try {
      const [units, inst, exp] = await Promise.allSettled([
        unitApi.getByCondominium(store.selectedCondominioId),
        installmentApi.getOpen(store.selectedCondominioId),
        expenseApi.getUnpaid(store.selectedCondominioId),
      ])
      if (units.status === 'fulfilled')  totalUnits.value = units.value.data?.length ?? 0
      if (inst.status === 'fulfilled')   openInstallments.value = inst.value.data?.length ?? 0
      if (exp.status === 'fulfilled')    unpaidExpenses.value = exp.value.data?.length ?? 0
    } catch {}
  }
})
</script>

<style scoped>
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 1rem;
  margin-bottom: 1.75rem;
}

.quick-section { margin-bottom: 0.5rem; }
.quick-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(170px, 1fr));
  gap: 0.75rem;
}
.quick-card {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  padding: 0.8rem 1rem;
  background: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  color: var(--text-secondary);
  text-decoration: none;
  font-size: 0.875rem;
  font-weight: 500;
  transition: all 0.15s;
}
.quick-card:hover {
  background: var(--bg-hover);
  border-color: var(--accent);
  color: var(--text-primary);
  text-decoration: none;
}
.quick-icon  { font-size: 1rem; color: var(--accent); flex-shrink: 0; }
.quick-label { flex: 1; }
.quick-arrow { color: var(--text-muted); font-size: 0.85rem; }
</style>
