<template>
  <div class="app-layout">
    <AppSidebar />
    <div class="app-main">
      <div class="app-content">
        <CondominiBreadcrumb />
        <RouterView :key="session.activeTenant?.id" />
      </div>
      <footer class="app-footer">Powered by VizaSoft S.r.l — 2026</footer>
    </div>
  </div>
</template>

<script setup>
  import { onMounted, watch } from 'vue'
  import AppSidebar from '@/components/layout/AppSidebar.vue'
  import CondominiBreadcrumb from '@/components/layout/CondominiBreadcrumb.vue'
  import { useSessionStore } from '@/stores/sessionStore'
  import { useAppStore } from '@/stores/app'
  import { useTenantBranding } from '@/composables/useTenantBranding'

  const session = useSessionStore()
  const appStore = useAppStore()
  const { load: loadBranding, reload: reloadBranding } = useTenantBranding()

  onMounted(async () => {
    loadBranding()   // carica il logo del tenant (fail-safe → logo di default)
    if (!appStore.condomini.length) {
      await appStore.loadCondomini()
    }
  })

  // Al cambio di tenant (superadmin) ricarica il logo corretto
  watch(() => session.activeTenant?.id, (id, prev) => {
    if (id && id !== prev) reloadBranding()
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
</style>
