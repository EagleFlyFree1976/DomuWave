<template>
  <div>
    <div class="page-header">
      <h1>Dashboard</h1>
      <span class="badge badge-green">● Live</span>
    </div>

    <!-- SuperAdmin: solo contatori globali -->
    <template v-if="session.isSuperAdmin">
      <div class="stats-grid">
        <div class="stat-card">
          <div class="stat-label">Tenant attivi</div>
          <div class="stat-value">{{ summary.activeTenantsCount }}</div>
          <div class="stat-sub">Organizzazioni registrate</div>
        </div>
        <div class="stat-card">
          <div class="stat-label">Condomini attivi</div>
          <div class="stat-value text-accent">{{ summary.totalActiveCondominiumsCount }}</div>
          <div class="stat-sub">Totale su tutta la piattaforma</div>
        </div>
      </div>
    </template>

    <!-- Utente tenant: dati completi -->
    <template v-else>
      <div class="stats-grid">
        <div class="stat-card">
          <div class="stat-label">Condomini attivi</div>
          <div class="stat-value">{{ summary.condominiumsCount }}</div>
          <div class="stat-sub">Totale gestiti</div>
        </div>
        <div class="stat-card">
          <div class="stat-label">Unità immobiliari</div>
          <div class="stat-value text-accent">{{ summary.totalUnitsCount }}</div>
          <div class="stat-sub">Tra tutti i condomini</div>
        </div>
        <div class="stat-card">
          <div class="stat-label">Rate aperte</div>
          <div class="stat-value text-amber">{{ summary.openInstallmentsCount }}</div>
          <div class="stat-sub">In attesa di pagamento</div>
        </div>
        <div class="stat-card">
          <div class="stat-label">Spese non pagate</div>
          <div class="stat-value text-red">{{ summary.unpaidExpensesCount }}</div>
          <div class="stat-sub">Da liquidare</div>
        </div>
      </div>

      <!-- Prossime attività + scadenze -->
      <div class="dash-cols">
        <!-- Prossime attività (task) -->
        <div class="card">
          <div class="card-header">
            <h2>Prossime attività</h2>
            <router-link to="/attivita" class="btn btn-ghost btn-sm">Tutte</router-link>
          </div>
          <div v-if="deadlines.upcomingTasks.length" class="table-wrap">
            <table>
              <thead><tr><th>Attività</th><th>Assegnatario</th><th>Scadenza</th><th>Priorità</th></tr></thead>
              <tbody>
                <tr v-for="t in deadlines.upcomingTasks" :key="t.id">
                  <td>{{ t.title }}</td>
                  <td class="text-muted">{{ t.assignedToFullName || '—' }}</td>
                  <td class="mono" :class="t.urgency === 'Overdue' ? 'text-red' : ''">{{ fmtDate(t.dueDate) }}</td>
                  <td><span class="badge badge-muted">{{ t.priority || '—' }}</span></td>
                </tr>
              </tbody>
            </table>
          </div>
          <div v-else class="empty-mini">Nessuna attività in scadenza.</div>
        </div>

        <!-- Prossime scadenze (aggregato) -->
        <div class="card">
          <div class="card-header">
            <h2>Prossime scadenze</h2>
          </div>
          <div v-if="deadlines.items.length" class="table-wrap">
            <table>
              <thead><tr><th>Tipo</th><th>Descrizione</th><th>Condominio</th><th>Scadenza</th></tr></thead>
              <tbody>
                <tr v-for="(d, i) in deadlines.items" :key="d.type + d.id + i">
                  <td><span class="badge" :class="typeBadge(d.type)">{{ typeLabel(d.type) }}</span></td>
                  <td>
                    <router-link :to="d.frontendLink" class="text-accent">{{ d.title }}</router-link>
                    <div v-if="d.description" class="text-muted" style="font-size:0.78rem">{{ d.description }}</div>
                  </td>
                  <td class="text-muted">{{ d.condominiumName || '—' }}</td>
                  <td class="mono" :class="d.urgency === 'Overdue' ? 'text-red' : ''">{{ fmtDate(d.dueDate) }}</td>
                </tr>
              </tbody>
            </table>
          </div>
          <div v-else class="empty-mini">Nessuna scadenza nei prossimi 30 giorni.</div>
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

      <div v-else-if="!loading" class="empty-state">
        <div class="empty-icon">⬡</div>
        <div>Nessun condominio trovato</div>
        <router-link to="/condomini" class="btn btn-primary" style="margin-top:0.5rem">Aggiungi condominio</router-link>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAppStore } from '@/stores/app'
import { useSessionStore } from '@/stores/sessionStore'
import { dashboardApi } from '@/services/api'

const store = useAppStore()
const session = useSessionStore()
const loading = ref(false)

const summary = ref({
  activeTenantsCount: 0,
  totalActiveCondominiumsCount: 0,
  condominiumsCount: 0,
  totalUnitsCount: 0,
  openInstallmentsCount: 0,
  unpaidExpensesCount: 0,
})

const deadlines = ref({ upcomingTasks: [], items: [] })

const fmtDate = (d) => d ? new Date(d).toLocaleDateString('it-IT') : '—'
const typeLabel = (t) => ({ Task: 'Attività', Installment: 'Rata', Assembly: 'Assemblea' }[t] || t)
const typeBadge = (t) => ({ Task: 'badge-blue', Installment: 'badge-amber', Assembly: 'badge-green' }[t] || 'badge-muted')

const quickLinks = [
  { to: '/condomini',  icon: '⬡', label: 'Gestisci Condomini' },
  { to: '/unita',      icon: '⊞', label: 'Unità Immobiliari' },
  { to: '/budget',     icon: '◎', label: 'Budget & Spese' },
  { to: '/rate',       icon: '◷', label: 'Rate & Quote' },
  { to: '/fornitori',  icon: '◈', label: 'Fornitori' },
  { to: '/documenti',  icon: '▤', label: 'Documenti' },
]

onMounted(async () => {
  loading.value = true
  try {
    const { data } = await dashboardApi.getSummary()
    summary.value = data

    if (!session.isSuperAdmin) {
      if (!store.condomini.length) await store.loadCondomini()
      try {
        const { data: dl } = await dashboardApi.getDeadlines(30)
        deadlines.value = { upcomingTasks: dl?.upcomingTasks ?? [], items: dl?.items ?? [] }
      } catch { /* gestito dal global error handler */ }
    }
  } finally {
    loading.value = false
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

.dash-cols {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
  margin-bottom: 1.75rem;
}
@media (max-width: 1000px) { .dash-cols { grid-template-columns: 1fr; } }
.empty-mini { padding: 1.25rem; color: var(--text-muted); font-size: 0.875rem; }
.text-red { color: var(--accent-red, #ef4444); }
.mono { font-family: monospace; }

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
