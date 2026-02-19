<template>
  <div class="app-shell">
    <!-- Sidebar -->
    <aside class="sidebar">
      <div class="sidebar-brand">
        <span class="brand-icon">⬡</span>
        <span class="brand-name">DomuWave</span>
      </div>

      <!-- Condominio selector -->
      <div class="condo-selector" v-if="store.condomini.length">
        <label class="form-label">Condominio</label>
        <select class="form-select condo-select" :value="store.selectedCondominioId"
          @change="store.selectCondominio(+$event.target.value)">
          <option v-for="c in store.condomini" :key="c.id" :value="c.id">{{ c.name }}</option>
        </select>
      </div>

      <nav class="sidebar-nav">
        <router-link v-for="item in navItems" :key="item.to" :to="item.to" class="nav-item">
          <span class="nav-icon">{{ item.icon }}</span>
          <span class="nav-label">{{ item.label }}</span>
        </router-link>
      </nav>

      <div class="sidebar-footer">
        <div class="user-chip">
          <div class="user-avatar">A</div>
          <div class="user-info">
            <div class="user-name">Admin</div>
            <div class="user-role text-muted">Amministratore</div>
          </div>
        </div>
      </div>
    </aside>

    <!-- Main -->
    <main class="main-area">
      <header class="topbar">
        <div class="topbar-title">{{ currentTitle }}</div>
        <div class="topbar-actions">
          <span class="text-muted mono" style="font-size:0.78rem">{{ today }}</span>
        </div>
      </header>

      <div class="page-content">
        <router-view v-slot="{ Component }">
          <transition name="fade" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </div>
    </main>

    <!-- Toast notifications -->
    <div class="toast-stack">
      <transition-group name="toast">
        <div v-for="t in store.toasts" :key="t.id" class="toast" :class="`toast-${t.type}`">
          <span class="toast-icon">{{ toastIcon(t.type) }}</span>
          {{ t.message }}
        </div>
      </transition-group>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useAppStore } from '@/store/app'

const store = useAppStore()
const route = useRoute()

const navItems = [
  { to: '/dashboard',     icon: '◈', label: 'Dashboard' },
  { to: '/condomini',     icon: '⬡', label: 'Condomini' },
  { to: '/unita',         icon: '⊞', label: 'Unità Immobiliari' },
  { to: '/budget',        icon: '◎', label: 'Budget & Spese' },
  { to: '/rate',          icon: '◷', label: 'Rate & Quote' },
  { to: '/fornitori',     icon: '◈', label: 'Fornitori' },
  { to: '/documenti',     icon: '▤', label: 'Documenti' },
  { to: '/comunicazioni', icon: '◉', label: 'Comunicazioni' },
]

const currentTitle = computed(() => route.meta.title || 'DomuWave')
const today = computed(() => new Date().toLocaleDateString('it-IT', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' }))

function toastIcon(type) {
  return { success: '✓', error: '✕', warning: '⚠', info: 'ℹ' }[type] || 'ℹ'
}

onMounted(() => store.loadCondomini())
</script>

<style scoped>
.app-shell {
  display: flex;
  min-height: 100vh;
}

/* ── Sidebar ─────────────────────────────── */
.sidebar {
  width: var(--sidebar-w);
  flex-shrink: 0;
  background: var(--bg-surface);
  border-right: 1px solid var(--border);
  display: flex;
  flex-direction: column;
  position: fixed;
  inset-block: 0;
  left: 0;
  z-index: 50;
}

.sidebar-brand {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  padding: 1.1rem 1.25rem;
  border-bottom: 1px solid var(--border);
}
.brand-icon {
  color: var(--accent);
  font-size: 1.3rem;
  line-height: 1;
}
.brand-name {
  font-size: 1rem;
  font-weight: 600;
  letter-spacing: -0.01em;
}

.condo-selector {
  padding: 0.85rem 1.1rem;
  border-bottom: 1px solid var(--border);
}
.condo-selector .form-label { margin-bottom: 0.3rem; }
.condo-select { font-size: 0.82rem; }

.sidebar-nav {
  flex: 1;
  padding: 0.5rem 0.6rem;
  display: flex;
  flex-direction: column;
  gap: 2px;
  overflow-y: auto;
}
.nav-item {
  display: flex;
  align-items: center;
  gap: 0.65rem;
  padding: 0.55rem 0.7rem;
  border-radius: var(--radius-sm);
  color: var(--text-secondary);
  font-size: 0.875rem;
  font-weight: 400;
  text-decoration: none;
  transition: all 0.12s ease;
}
.nav-item:hover {
  background: var(--bg-hover);
  color: var(--text-primary);
  text-decoration: none;
}
.nav-item.router-link-active {
  background: var(--accent-glow);
  color: var(--accent);
  font-weight: 500;
}
.nav-icon { font-size: 0.95rem; opacity: 0.75; width: 1.1rem; text-align: center; }

.sidebar-footer {
  padding: 0.85rem 1rem;
  border-top: 1px solid var(--border);
}
.user-chip { display: flex; align-items: center; gap: 0.6rem; }
.user-avatar {
  width: 30px; height: 30px;
  background: var(--accent-glow);
  color: var(--accent);
  border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
  font-size: 0.8rem; font-weight: 600;
  flex-shrink: 0;
}
.user-name { font-size: 0.83rem; font-weight: 500; }
.user-role { font-size: 0.75rem; }

/* ── Main ────────────────────────────────── */
.main-area {
  flex: 1;
  margin-left: var(--sidebar-w);
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

.topbar {
  height: var(--header-h);
  background: var(--bg-surface);
  border-bottom: 1px solid var(--border);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 1.5rem;
  position: sticky;
  top: 0;
  z-index: 40;
}
.topbar-title {
  font-size: 0.9rem;
  font-weight: 500;
  color: var(--text-secondary);
}

.page-content {
  flex: 1;
  padding: 1.75rem 1.5rem;
  max-width: 1280px;
}

/* ── Toasts ──────────────────────────────── */
.toast-stack {
  position: fixed;
  bottom: 1.5rem;
  right: 1.5rem;
  z-index: 200;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}
.toast {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.65rem 1rem;
  border-radius: var(--radius-sm);
  font-size: 0.875rem;
  background: var(--bg-elevated);
  border: 1px solid var(--border);
  box-shadow: var(--shadow-md);
  min-width: 240px;
}
.toast-success { border-color: rgba(104,211,145,0.3); color: var(--accent-green); }
.toast-error   { border-color: rgba(252,129,129,0.3); color: var(--accent-red); }
.toast-warning { border-color: rgba(246,173,85,0.3);  color: var(--accent-amber); }
.toast-icon    { font-size: 0.9rem; }

.toast-enter-active { transition: all 0.25s ease; }
.toast-leave-active { transition: all 0.2s ease; }
.toast-enter-from   { opacity: 0; transform: translateX(20px); }
.toast-leave-to     { opacity: 0; transform: translateX(20px); }
</style>
