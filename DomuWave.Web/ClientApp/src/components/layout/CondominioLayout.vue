<template>
  <div v-if="condominium">
    <!-- Shared header -->
    <div class="page-header">
      <div class="header-left">
        <router-link to="/condomini" class="btn btn-ghost btn-sm">← Indietro</router-link>
        <h1>{{ condominium.name }}</h1>
        <span class="badge" :class="condominium.isActive ? 'badge-green' : 'badge-muted'">
          {{ condominium.isActive ? 'Attivo' : 'Inattivo' }}
        </span>
      </div>
    </div>

    <!-- Tab bar (shared across all condominium sub-pages) -->
    <nav class="tab-bar">
      <router-link :to="`/condomini/${condominiumId}`" class="tab-item">⊟ Informazioni</router-link>
      <router-link
        v-for="q in quickNav"
        :key="q.path"
        :to="`/condomini/${condominiumId}${q.path}`"
        class="tab-item"
      >{{ q.icon }} {{ q.label }}</router-link>
    </nav>

    <!-- Tab content -->
    <RouterView />
  </div>

  <div v-else class="loading-state">
    <div class="spinner"></div> Caricamento…
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch, provide } from 'vue'
import { useRoute } from 'vue-router'
import { condominiumApi } from '@/services/api'

const route = useRoute()
const condominium = ref(null)
const condominiumId = computed(() => Number(route.params.id))

const quickNav = [
  { path: '/unita',         icon: '⊞', label: 'Unità' },
  { path: '/panoramica',    icon: '◫', label: 'Panoramica' },
  { path: '/budget',        icon: '◎', label: 'Budget' },
  { path: '/rate',          icon: '◷', label: 'Rate' },
  { path: '/fornitori',     icon: '◈', label: 'Fornitori' },
  { path: '/documenti',     icon: '▤', label: 'Documenti' },
  { path: '/comunicazioni', icon: '◉', label: 'Comunicazioni' },
  { path: '/setup',         icon: '✓', label: 'Setup' },
]

provide('condominium', condominium)
provide('condominiumId', condominiumId)

async function fetchCondominium(id) {
  condominium.value = null
  const { data } = await condominiumApi.getById(id)
  condominium.value = data
}

onMounted(() => fetchCondominium(route.params.id))

watch(() => route.params.id, (newId) => {
  if (newId) fetchCondominium(newId)
})
</script>

<style scoped>
.header-left { display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap; }

/* ── Tab bar ────────────────────────────────────────────────────────────────── */
.tab-bar {
  display: flex;
  gap: 0;
  border-bottom: 2px solid var(--border);
  margin-bottom: 1.5rem;
  overflow-x: auto;
  scrollbar-width: none;
}
.tab-bar::-webkit-scrollbar { display: none; }

.tab-item {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  padding: 0.6rem 1.1rem;
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--text-muted);
  text-decoration: none;
  border-bottom: 2px solid transparent;
  margin-bottom: -2px;
  white-space: nowrap;
  cursor: pointer;
  transition: color 0.15s, border-color 0.15s;
}

.tab-item:hover {
  color: var(--text-primary);
  text-decoration: none;
}

.tab-item.router-link-exact-active {
  color: var(--accent);
  border-bottom-color: var(--accent);
}
</style>
