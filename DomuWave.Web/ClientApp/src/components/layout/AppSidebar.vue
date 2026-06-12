<template>
  <aside class="app-sidebar" :class="{ collapsed }">

    <!-- ── Header ── -->
    <div class="sidebar-header">
      <div class="sidebar-logo">
        <div class="logo-icon" v-show="collapsed">
          <i class="pi pi-building"></i>
        </div>
        <img v-show="!collapsed" :src="logoUrl || DEFAULT_LOGO" alt="Logo studio" class="sidebar-logo-img" />
      </div>
      <button class="icon-btn" @click="collapsed = !collapsed" :title="collapsed ? 'Espandi' : 'Comprimi'">
        <i class="pi" :class="collapsed ? 'pi-angle-right' : 'pi-angle-left'"></i>
      </button>
    </div>

    <!-- ── Nav ── -->
    <nav class="sidebar-nav" :class="{ 'nav-ready': initialized }" :style="!initialized ? { visibility: 'hidden' } : {}">
      <ul class="menu-list">
        <li v-for="item in visibleMenu" :key="item.path">

          <!-- Gruppo con figli -->
          <template v-if="item.children">
            <button class="menu-link group-toggle"
                    :class="{ active: isGroupActive(item) }"
                    @click="toggleGroup(item.path)"
                    :title="collapsed ? item.label : ''">
              <i class="pi menu-icon" :class="item.icon"></i>
              <span class="menu-label" v-show="!collapsed">{{ item.label }}</span>
              <i class="pi menu-chevron" v-show="!collapsed"
                 :class="openGroups.includes(item.path) ? 'pi-chevron-down' : 'pi-chevron-right'"></i>
            </button>
            <ul v-show="!collapsed && openGroups.includes(item.path)" class="submenu-list">
              <li v-for="child in item.children" :key="child.path">
                <RouterLink :to="child.path" class="menu-link submenu-link"
                            :class="{ active: route.path === child.path }">
                  <i class="pi menu-icon submenu-icon" :class="child.icon"></i>
                  <span class="menu-label">{{ child.label }}</span>
                </RouterLink>
              </li>
            </ul>
          </template>

          <!-- Link semplice -->
          <RouterLink v-else :to="item.path" class="menu-link"
                      :class="{ active: route.path === item.path }"
                      :title="collapsed ? item.label : ''">
            <i class="pi menu-icon" :class="item.icon"></i>
            <span class="menu-label" v-show="!collapsed">{{ item.label }}</span>
          </RouterLink>

        </li>
      </ul>

      <!-- Hint voci nascoste: visibile solo per superadmin senza tenant -->
      <Transition name="fade">
        <div
          v-if="session.isSuperAdmin && !session.hasTenantSelected && hiddenCount > 0 && !collapsed"
          class="tenant-required-hint"
        >
          <i class="pi pi-lock"></i>
          <span>{{ hiddenCount }} voci disponibili dopo la selezione del tenant</span>
        </div>
      </Transition>
    </nav>

    <TenantSelectorWidget />
    <CondominioSelectorWidget />

    <!-- ── Banner impersonificazione ── -->
    <div v-if="authStore.isImpersonating" class="impersonate-banner" :class="{ collapsed }">
      <div class="impersonate-banner__body" v-show="!collapsed">
        <i class="pi pi-user-edit impersonate-banner__icon"></i>
        <div class="impersonate-banner__text">
          <span class="impersonate-banner__label">Stai impersonando</span>
          <span class="impersonate-banner__name">{{ authStore.currentUser?.displayName }}</span>
        </div>
      </div>
      <button class="impersonate-banner__exit" @click="handleExitImpersonation" :title="collapsed ? 'Esci impersonificazione' : ''">
        <i class="pi pi-times"></i>
        <span v-show="!collapsed">Esci</span>
      </button>
    </div>

    <!-- ── Footer ── -->
    <div class="sidebar-footer">
      <div class="user-chip">
        <div class="user-avatar">{{ userInitials }}</div>
        <div class="user-details" v-show="!collapsed">
          <span class="user-name">{{ authStore.currentUser?.displayName ?? authStore.currentUser?.username }}</span>
          <span class="user-role">{{ authStore.currentUser?.role ?? 'Utente' }}</span>
        </div>
      </div>
      <button class="icon-btn logout-btn" @click="handleLogout" title="Esci">
        <i class="pi pi-sign-out"></i>
      </button>
    </div>

  </aside>
</template>

<script setup>
  import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
  import { RouterLink, useRoute, useRouter } from 'vue-router'
  import { useAuthStore } from '@/stores/authStore'
  import { useMenuStore } from '@/stores/menuStore'
  import { useSessionStore } from '@/stores/sessionStore'
  import { useAppStore } from '@/stores/app'
  import { useFeatureStatus } from '@/composables/useFeatureStatus'
  import { useTenantBranding } from '@/composables/useTenantBranding'
  import TenantSelectorWidget from '@/components/layout/TenantSelectorWidget.vue'
  import CondominioSelectorWidget from '@/components/layout/CondominioSelectorWidget.vue'

  const route    = useRoute()
  const router   = useRouter()
  const authStore = useAuthStore()
  const menuStore = useMenuStore()
  const session   = useSessionStore()
  const appStore  = useAppStore()
  const { load: loadFeatureStatus, reset: resetFeatureStatus } = useFeatureStatus()
  const { logoUrl, DEFAULT_LOGO } = useTenantBranding()

  const collapsed   = ref(false)
  const openGroups  = ref([])
  const initialized = ref(false)

  /**
   * Prefissi di percorso che devono essere sempre visibili
   * indipendentemente dal tenant selezionato.
   * Tutto il resto richiede un tenant attivo (per i superadmin).
   */
  const ALWAYS_VISIBLE = ['/dashboard', '/tenants']

  function isAlwaysVisible(item) {
    const path = item.tags ?? ''
    if (path == null || path == "") return true
    // Un gruppo è always-visible se almeno un figlio lo è
    if (item.children?.some(c => c.tags == null || c.tags == '')) return true
    return false
  }

  /**
   * Voci di menu riservate ai SuperAdmin che non dipendono dal backend.
   * Vengono iniettate solo se non già presenti nel menu dell'API.
   */
  const superAdminItems = computed(() => {
    if (!session.isSuperAdmin) return []
    const existing = menuStore.menuItems
    return [
      /*{ key: 'sa-auth', label: 'Autorizzazioni', path: '/autorizzazioni', icon: 'pi-shield', order: 998, tags: [] },*/
    ].filter(item => !existing.some(m => m.path === item.path))
  })

  /**
   * Voci statiche visibili a TUTTI gli utenti loggati (role-agnostiche),
   * iniettate solo se non già presenti nel menu dell'API.
   * `tags: []` → sempre visibile (passa isAlwaysVisible anche senza tenant selezionato).
   */
  const staticItems = computed(() => {
    const existing = menuStore.menuItems
    return [
      { key: 'guida', label: 'Guida', path: '/guida', icon: 'pi-question-circle', order: 999, tags: [] },
    ].filter(item => !existing.some(m => m.path === item.path))
  })

  /** Tutti i menu items disponibili (API o vuoto se non ancora caricato) */
  const allMenuItems = computed(() => {
    const base = menuStore.menuItems.length > 0 ? menuStore.menuItems : []
    return [...base, ...superAdminItems.value, ...staticItems.value]
  })

  /**
   * Menu filtrato:
   * - Se superadmin senza tenant → solo voci always-visible
   * - Altrimenti → tutto il menu
   */
  const visibleMenu = computed(() => {
    if (session.isSuperAdmin && !session.hasTenantSelected) {
      return allMenuItems.value.filter(isAlwaysVisible)
    }
    return allMenuItems.value
  })

  /** Quante voci sono nascoste per mancanza di tenant */
  const hiddenCount = computed(() => {
    if (!session.isSuperAdmin || session.hasTenantSelected) return 0
    return allMenuItems.value.filter(item => !isAlwaysVisible(item)).length
  })

  onMounted(async () => {
    if (authStore.isAuthenticated) {
      await menuStore.fetchMenu().catch(() => {})
      autoOpenCurrentGroup()
      await loadFeatureStatus().catch(() => {})
    }
    initialized.value = true

    // Ricarica status quando un'operazione consumabile cambia i crediti
    window.addEventListener('feature:usage-changed', loadFeatureStatus)
  })

  onUnmounted(() => {
    window.removeEventListener('feature:usage-changed', loadFeatureStatus)
  })

  // Ricarica status al cambio di condominio (cambio tenant)
  watch(() => appStore.selectedCondominioId, () => {
    loadFeatureStatus().catch(() => {})
  })

  function autoOpenCurrentGroup() {
    visibleMenu.value.forEach((item) => {
      if (item.children?.some((c) => c.path === route.path)) {
        if (!openGroups.value.includes(item.path)) openGroups.value.push(item.path)
      }
    })
  }

  function toggleGroup(path) {
    const idx = openGroups.value.indexOf(path)
    idx > -1 ? openGroups.value.splice(idx, 1) : openGroups.value.push(path)
  }

  function isGroupActive(item) {
    return item.children?.some((c) => c.path === route.path)
  }

  function handleLogout() {
    authStore.logout()
    menuStore.clearMenu()
    session.reset()
    router.push('/login')
  }

  function handleExitImpersonation() {
    authStore.exitImpersonation()
    menuStore.clearMenu()
    router.push('/users').then(() => window.location.reload())
  }

  const userInitials = computed(() => {
    const name = authStore.currentUser?.displayName ?? authStore.currentUser?.username ?? '?'
    return name.split(' ').map((n) => n[0]).join('').toUpperCase().slice(0, 2)
  })
</script>

<style scoped>
  .app-sidebar {
    width: 248px;
    height: 100vh;
    position: sticky;
    top: 0;
    overflow-y: auto;
    flex-shrink: 0;
    background: #0f172a;
    border-right: 1px solid #1e293b;
    display: flex;
    flex-direction: column;
    transition: width 0.25s ease;
    flex-shrink: 0;
    overflow: hidden;
  }

    .app-sidebar.collapsed {
      width: 64px;
    }

  /* Header */
  .sidebar-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 1rem 0.75rem;
    border-bottom: 1px solid #1e293b;
    min-height: 64px;
    gap: 0.5rem;
  }

  .sidebar-logo {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.65rem;
    flex: 1;
    min-width: 0;
    overflow: hidden;
  }

  .sidebar-logo-img {
    max-width: 180px;
    height: auto;
    display: block;
    margin: 0 auto;
    border-radius: 8px;
  }

  .logo-icon {
    width: 36px;
    height: 36px;
    flex-shrink: 0;
    border-radius: 9px;
    background: linear-gradient(135deg, #34d399, #059669);
    display: flex;
    align-items: center;
    justify-content: center;
  }

    .logo-icon .pi {
      color: #fff;
      font-size: 1rem;
    }

  .logo-text {
    display: flex;
    flex-direction: column;
    overflow: hidden;
    white-space: nowrap;
  }

  .logo-name {
    font-weight: 700;
    font-size: 0.95rem;
    color: #f1f5f9;
    line-height: 1.2;
  }

  .logo-sub {
    font-size: 0.62rem;
    color: #475569;
    letter-spacing: 0.6px;
    text-transform: uppercase;
  }

  /* Icon buttons */
  .icon-btn {
    background: transparent;
    border: none;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 30px;
    height: 30px;
    border-radius: 7px;
    color: #475569;
    font-size: 0.85rem;
    flex-shrink: 0;
    transition: background 0.12s, color 0.12s;
  }

    .icon-btn:hover {
      background: #1e293b;
      color: #cbd5e1;
    }

  /* Nav */
  .sidebar-nav {
    flex: 1;
    overflow-y: auto;
    overflow-x: hidden;
    padding: 0.75rem 0.5rem;
    scrollbar-width: thin;
    scrollbar-color: #1e293b transparent;
    display: flex;
    flex-direction: column;
    gap: 0;
  }

  /* Menu list */
  .menu-list {
    list-style: none;
    margin: 0;
    padding: 0;
    display: flex;
    flex-direction: column;
    gap: 2px;
  }

  /* Menu link */
  .menu-link {
    display: flex;
    align-items: center;
    gap: 0.65rem;
    padding: 0.6rem 0.75rem;
    border-radius: 8px;
    color: #64748b;
    text-decoration: none;
    font-size: 0.875rem;
    font-weight: 500;
    cursor: pointer;
    background: transparent;
    border: none;
    width: 100%;
    text-align: left;
    white-space: nowrap;
    overflow: hidden;
    transition: background 0.12s, color 0.12s;
    font-family: inherit;
  }

    .menu-link:hover {
      background: #1e293b;
      color: #e2e8f0;
    }

    .menu-link.active {
      background: rgba(52,211,153,0.1);
      color: #34d399;
      font-weight: 600;
    }

      .menu-link.active .menu-icon {
        color: #34d399;
      }

  /* Icon */
  .menu-icon {
    font-size: 0.95rem;
    flex-shrink: 0;
    width: 18px;
    text-align: center;
  }

  .submenu-icon {
    font-size: 0.8rem;
  }

  /* Label */
  .menu-label {
    flex: 1;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  /* Chevron */
  .menu-chevron {
    font-size: 0.65rem;
    margin-left: auto;
    flex-shrink: 0;
    color: #334155;
  }

  /* Submenu */
  .submenu-list {
    list-style: none;
    margin: 2px 0 2px 0.75rem;
    padding: 0 0 0 0.75rem;
    border-left: 1px solid #1e293b;
    display: flex;
    flex-direction: column;
    gap: 1px;
  }

  .submenu-link {
    font-size: 0.83rem;
    padding: 0.45rem 0.65rem;
  }

  /* Hint tenant required */
  .tenant-required-hint {
    display: flex;
    align-items: center;
    gap: 7px;
    margin: 10px 4px 0;
    padding: 7px 10px;
    border-radius: 7px;
    background: rgba(71, 85, 105, 0.15);
    border: 1px dashed #334155;
    font-size: 11px;
    color: #475569;
    line-height: 1.35;
  }

  .tenant-required-hint .pi {
    font-size: 12px;
    flex-shrink: 0;
    color: #475569;
  }

  /* Banner impersonificazione */
  .impersonate-banner {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 0.5rem;
    margin: 0 0.5rem 0.5rem;
    padding: 0.6rem 0.75rem;
    background: rgba(99, 102, 241, 0.15);
    border: 1px solid rgba(99, 102, 241, 0.4);
    border-radius: 8px;
  }

  .impersonate-banner.collapsed {
    justify-content: center;
    padding: 0.6rem 0.25rem;
  }

  .impersonate-banner__body {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    min-width: 0;
  }

  .impersonate-banner__icon {
    color: #818cf8;
    font-size: 1rem;
    flex-shrink: 0;
  }

  .impersonate-banner__text {
    display: flex;
    flex-direction: column;
    min-width: 0;
  }

  .impersonate-banner__label {
    font-size: 0.7rem;
    color: #818cf8;
    text-transform: uppercase;
    letter-spacing: 0.05em;
  }

  .impersonate-banner__name {
    font-size: 0.8rem;
    font-weight: 600;
    color: #c7d2fe;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .impersonate-banner__exit {
    display: flex;
    align-items: center;
    gap: 0.3rem;
    background: rgba(99, 102, 241, 0.25);
    border: 1px solid rgba(99, 102, 241, 0.5);
    border-radius: 5px;
    color: #c7d2fe;
    font-size: 0.75rem;
    padding: 0.25rem 0.5rem;
    cursor: pointer;
    flex-shrink: 0;
    white-space: nowrap;
    transition: background 0.15s;
  }

  .impersonate-banner__exit:hover {
    background: rgba(99, 102, 241, 0.45);
  }

  /* Footer */
  .sidebar-footer {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 0.5rem;
    padding: 0.75rem;
    border-top: 1px solid #1e293b;
    min-height: 60px;
  }

  .sidebar-powered {
    text-align: center;
    font-size: 0.65rem;
    color: #475569;
    padding: 0.4rem 0.75rem 0.5rem;
    letter-spacing: 0.3px;
  }

  .user-chip {
    display: flex;
    align-items: center;
    gap: 0.6rem;
    overflow: hidden;
    flex: 1;
    min-width: 0;
  }

  .user-avatar {
    width: 32px;
    height: 32px;
    border-radius: 50%;
    background: linear-gradient(135deg, #34d399, #059669);
    color: #fff;
    font-size: 0.72rem;
    font-weight: 700;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
  }

  .user-details {
    display: flex;
    flex-direction: column;
    overflow: hidden;
    white-space: nowrap;
  }

  .user-name {
    font-size: 0.8rem;
    font-weight: 600;
    color: #f1f5f9;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .user-role {
    font-size: 0.68rem;
    color: #475569;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .logout-btn {
    color: #475569;
  }

    .logout-btn:hover {
      color: #f87171;
      background: rgba(248,113,113,0.08);
    }

  /* Animazioni */
  .nav-ready {
    animation: nav-fade-in 0.15s ease;
  }

  @keyframes nav-fade-in {
    from { opacity: 0; }
    to   { opacity: 1; }
  }

  .fade-enter-active,
  .fade-leave-active { transition: opacity 0.2s, transform 0.2s; }
  .fade-enter-from,
  .fade-leave-to     { opacity: 0; transform: translateY(-4px); }
</style>
