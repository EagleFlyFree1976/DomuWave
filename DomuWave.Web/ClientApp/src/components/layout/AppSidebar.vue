<template>
  <aside class="app-sidebar" :class="{ collapsed }">

    <!-- ── Header ── -->
    <div class="sidebar-header">
      <div class="sidebar-logo">
        <div class="logo-icon">
          <i class="pi pi-building"></i>
        </div>
        <div class="logo-text" v-show="!collapsed">
          <span class="logo-name">DomuWave</span>
          <span class="logo-sub">Gestione Condomini</span>
        </div>
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
    </nav>

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
  import { ref, computed, onMounted } from 'vue'
  import { RouterLink, useRoute, useRouter } from 'vue-router'
  import { useAuthStore } from '@/stores/authStore'
  import { useMenuStore } from '@/stores/menuStore'

  const route = useRoute()
  const router = useRouter()
  const authStore = useAuthStore()
  const menuStore = useMenuStore()

  const collapsed = ref(false)
  const openGroups = ref([])
  const initialized = ref(false)  // blocca render menu finché non è pronto

  // Menu statico di fallback (corrisponde alle route reali del progetto)
  const staticMenu = [
    { path: '/dashboard', label: 'Dashboard', icon: 'pi-home' },
    { path: '/condomini', label: 'Condomini', icon: 'pi-building' },
    { path: '/unita', label: 'Unità Immobiliari', icon: 'pi-th-large' },
    {
      path: '/contabilita', label: 'Contabilità', icon: 'pi-wallet',
      children: [
        { path: '/budget', label: 'Budget & Spese', icon: 'pi-chart-bar' },
        { path: '/rate', label: 'Rate & Quote', icon: 'pi-calendar' },
      ],
    },
    { path: '/fornitori', label: 'Fornitori', icon: 'pi-truck' },
    { path: '/documenti', label: 'Documenti', icon: 'pi-folder' },
    { path: '/comunicazioni', label: 'Comunicazioni', icon: 'pi-envelope' },
  ]

  // Usa menu dall'API se disponibile, altrimenti il fallback statico
  const visibleMenu = computed(() =>
    menuStore.menuItems.length > 0 ? menuStore.menuItems : []// staticMenu
  )

  onMounted(async () => {
    if (authStore.isAuthenticated) {
      await menuStore.fetchMenu().catch(() => { })
      autoOpenCurrentGroup()
    }
    // Menu pronto (da API o fallback): sblocca il render
    initialized.value = true
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
    router.push('/login')
  }

  const userInitials = computed(() => {
    const name = authStore.currentUser?.displayName ?? authStore.currentUser?.username ?? '?'
    return name.split(' ').map((n) => n[0]).join('').toUpperCase().slice(0, 2)
  })
</script>

<style scoped>
  .app-sidebar {
    width: 248px;
    min-height: 100vh;
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
    gap: 0.65rem;
    flex: 1;
    min-width: 0;
    overflow: hidden;
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

  /* Skeleton pre-inizializzazione */
  .menu-skeleton-item {
    height: 36px;
    border-radius: 8px;
    background: linear-gradient(90deg, #1e293b 25%, #243044 50%, #1e293b 75%);
    background-size: 200% 100%;
    animation: shimmer 1.4s infinite;
    margin-bottom: 3px;
  }

  @keyframes shimmer {
    0% {
      background-position: 200% 0;
    }

    100% {
      background-position: -200% 0;
    }
  }

  .nav-ready {
    animation: nav-fade-in 0.15s ease;
  }

  @keyframes nav-fade-in {
    from {
      opacity: 0;
    }

    to {
      opacity: 1;
    }
  }
</style>
