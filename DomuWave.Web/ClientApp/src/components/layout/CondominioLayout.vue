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
    <div class="tab-bar-wrap" ref="navWrap">
      <nav class="tab-bar" ref="navBar">
        <router-link :to="`/condomini/${condominiumId}`" class="tab-item" ref="infoTab">
          <i class="pi pi-info-circle"></i> Informazioni
        </router-link>
        <router-link
          v-for="q in visibleNav"
          :key="q.path"
          :to="`/condomini/${condominiumId}${q.path}`"
          class="tab-item"
        ><i class="pi" :class="q.icon"></i> {{ q.label }}</router-link>

        <!-- Dropdown "Altro" per le voci che non entrano -->
        <div v-if="overflowNav.length" class="tab-item tab-more" :class="{ active: overflowActive }" @click.stop="toggleMore">
          Altro <i class="pi pi-chevron-down" style="font-size:0.7rem; margin-left:2px"></i>
          <div v-if="showMore" class="more-dropdown" @click.stop>
            <router-link
              v-for="q in overflowNav"
              :key="q.path"
              :to="`/condomini/${condominiumId}${q.path}`"
              class="more-item"
              @click="showMore = false"
            ><i class="pi" :class="q.icon"></i> {{ q.label }}</router-link>
          </div>
        </div>
      </nav>
    </div>

    <!-- Tab content -->
    <RouterView />
  </div>

  <div v-else class="loading-state">
    <div class="spinner"></div> Caricamento…
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch, provide, nextTick } from 'vue'
import { useRoute } from 'vue-router'
import { condominiumApi } from '@/services/api'
import { useAppStore } from '@/stores/app'

const route = useRoute()
const store = useAppStore()
const condominium = ref(null)
const condominiumId = computed(() => Number(route.params.id))

const allNav = [
  { path: '/edifici',              icon: 'pi-home',            label: 'Edifici' },
  { path: '/scale',                icon: 'pi-sort-alt',        label: 'Scale' },
  { path: '/unita',                icon: 'pi-building',        label: 'Unità' },
  { path: '/panoramica',           icon: 'pi-sitemap',         label: 'Panoramica' },
  { path: '/gruppi-fatturazione',  icon: 'pi-objects-column',  label: 'Gruppi' },
  { path: '/rendiconto',           icon: 'pi-book',            label: 'Rendiconto' },
  { path: '/budget',               icon: 'pi-chart-bar',       label: 'Budget' },
  { path: '/spese',                icon: 'pi-credit-card',     label: 'Spese' },
  { path: '/consumi',              icon: 'pi-gauge',           label: 'Consumi' },
  { path: '/rate',                 icon: 'pi-calendar',        label: 'Rate' },
  { path: '/fornitori',            icon: 'pi-truck',           label: 'Fornitori' },
  { path: '/documenti',            icon: 'pi-folder-open',     label: 'Documenti' },
  { path: '/comunicazioni',        icon: 'pi-megaphone',       label: 'Comunicazioni' },
  { path: '/assemblee',            icon: 'pi-microphone',      label: 'Assemblee' },
  { path: '/setup',                icon: 'pi-list-check',      label: 'Setup' },
]

const navWrap     = ref(null)
const navBar      = ref(null)
const showMore    = ref(false)
const visibleCount = ref(allNav.length)

const visibleNav  = computed(() => allNav.slice(0, visibleCount.value))
const overflowNav = computed(() => allNav.slice(visibleCount.value))
const overflowActive = computed(() =>
  overflowNav.value.some(q => route.path === `/condomini/${condominiumId.value}${q.path}`)
)

function toggleMore() { showMore.value = !showMore.value }

function closeMore(e) {
  if (navBar.value && !navBar.value.contains(e.target)) showMore.value = false
}

// Measure how many tabs fit — approximate via average tab width
function recalc() {
  if (!navWrap.value) return
  const available = navWrap.value.offsetWidth
  // "Altro" button ~ 90px, "Informazioni" tab ~ 120px, each other tab avg ~110px
  const infoWidth = 120
  const moreWidth = 90
  const avgTab    = 110
  const space = available - infoWidth - moreWidth
  visibleCount.value = Math.max(0, Math.floor(space / avgTab))
  // If all fit without needing "Altro", show all
  if (visibleCount.value >= allNav.length) visibleCount.value = allNav.length
}

provide('condominium', condominium)
provide('condominiumId', condominiumId)

async function fetchCondominium(id) {
  condominium.value = null
  const { data } = await condominiumApi.getById(id)
  condominium.value = data
  store.selectCondominio(Number(id))
  await nextTick()
  recalc()
}

const ro = typeof ResizeObserver !== 'undefined' ? new ResizeObserver(recalc) : null

onMounted(() => {
  fetchCondominium(route.params.id)
  document.addEventListener('click', closeMore)
  if (ro && navWrap.value) ro.observe(navWrap.value)
})

onUnmounted(() => {
  document.removeEventListener('click', closeMore)
  ro?.disconnect()
})

watch(() => route.params.id, (newId) => { if (newId) fetchCondominium(newId) })
</script>

<style scoped>
.header-left { display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap; }

/* ── Tab bar ────────────────────────────────────────────────────────────────── */
.tab-bar-wrap {
  position: relative;
  margin-bottom: 1.5rem;
}

.tab-bar {
  display: flex;
  gap: 0;
  border-bottom: 2px solid var(--border);
  overflow: visible;
}

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

.tab-item:hover { color: var(--text); text-decoration: none; }

.tab-item.router-link-exact-active {
  color: var(--accent);
  border-bottom-color: var(--accent);
}

/* ── "Altro" dropdown trigger ───────────────────────────────────────────────── */
.tab-more {
  position: relative;
  user-select: none;
}

.tab-more.active {
  color: var(--accent);
  border-bottom-color: var(--accent);
}

.more-dropdown {
  position: absolute;
  top: calc(100% + 4px);
  right: 0;
  min-width: 180px;
  background: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: 8px;
  box-shadow: 0 8px 24px rgba(0,0,0,0.25);
  z-index: 200;
  overflow: hidden;
}

.more-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.55rem 1rem;
  font-size: 0.875rem;
  color: var(--text);
  text-decoration: none;
  transition: background 0.12s;
}

.more-item:hover { background: var(--accent-glow); color: var(--accent); text-decoration: none; }

.more-item.router-link-exact-active { color: var(--accent); font-weight: 600; }
</style>
