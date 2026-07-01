<template>
  <div class="app-layout">
    <AppSidebar />

    <!-- Overlay scurito quando il drawer è aperto su mobile -->
    <div
      v-if="ui.mobileSidebarOpen"
      class="sidebar-overlay"
      @click="ui.closeMobileSidebar()"
    ></div>

    <div class="app-main">
      <!-- Topbar mobile: visibile solo su schermi stretti -->
      <header class="app-topbar-mobile">
        <button class="topbar-hamburger" @click="ui.toggleMobileSidebar()" aria-label="Menu">
          <i class="pi pi-bars"></i>
        </button>
        <span class="topbar-title">VizaDomus</span>
      </header>

      <div class="app-content">
        <CondominiBreadcrumb />
        <RouterView :key="session.activeTenant?.id" />
      </div>
      <footer class="app-footer">Powered by VizaSoft S.r.l — 2026</footer>
    </div>
  </div>
</template>

<script setup>
  import { onMounted } from 'vue'
  import AppSidebar from '@/components/layout/AppSidebar.vue'
  import CondominiBreadcrumb from '@/components/layout/CondominiBreadcrumb.vue'
  import { useSessionStore } from '@/stores/sessionStore'
  import { useAppStore } from '@/stores/app'
  import { useUiStore } from '@/stores/uiStore'

  const session = useSessionStore()
  const appStore = useAppStore()
  const ui = useUiStore()

  onMounted(async () => {
    if (!appStore.condomini.length) {
      await appStore.loadCondomini()
    }
  })
</script>

<style scoped>
  .app-layout {
    display: flex;
    height: 100vh;
    overflow: hidden;
    background: var(--bg-base);
  }

  .app-main {
    flex: 1;
    min-width: 0;
    height: 100vh;
    overflow-y: auto;
    display: flex;
    flex-direction: column;
  }

  .app-content {
    flex: 1;
    padding: 1.75rem 2rem;
    max-width: 1400px;
    width: 100%;
  }

  .app-footer {
    flex-shrink: 0;
    text-align: center;
    font-size: 0.7rem;
    color: var(--text-muted);
    padding: 0.6rem 1rem;
    border-top: 1px solid var(--border);
    letter-spacing: 0.3px;
    margin-top: auto;
  }

  /* ── Overlay drawer mobile (nascosto su desktop) ── */
  .sidebar-overlay {
    display: none;
  }

  /* ── Topbar mobile (nascosta su desktop) ── */
  .app-topbar-mobile {
    display: none;
    align-items: center;
    gap: 0.75rem;
    padding: 0.6rem 0.9rem;
    background: #0f172a;
    border-bottom: 1px solid var(--border);
    position: sticky;
    top: 0;
    z-index: 900;
    /* Override del global `.app-main > * { flex:1; padding:1.5rem }`:
       la topbar non deve crescere né ricevere il padding generico,
       altrimenti diventa una fascia alta e vuota. */
    flex: 0 0 auto;
  }

  .topbar-hamburger {
    background: transparent;
    border: none;
    cursor: pointer;
    width: 38px;
    height: 38px;
    border-radius: 8px;
    color: var(--text-primary);
    font-size: 1.2rem;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .topbar-hamburger:hover {
    background: #1e293b;
  }

  .topbar-title {
    font-weight: 700;
    font-size: 1rem;
    color: var(--text-primary);
    letter-spacing: 0.3px;
  }

  @media (max-width: 768px) {
    .app-topbar-mobile {
      display: flex;
    }

    .sidebar-overlay {
      display: block;
      position: fixed;
      inset: 0;
      background: rgba(0, 0, 0, 0.5);
      z-index: 1050;
    }

    .app-content {
      padding: 1rem;
    }
  }

  @media print {
    .app-topbar-mobile,
    .sidebar-overlay {
      display: none !important;
    }
  }
</style>
