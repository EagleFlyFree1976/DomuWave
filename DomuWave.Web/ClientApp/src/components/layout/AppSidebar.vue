<template>
  <aside class="app-sidebar" :class="{ collapsed: sidebarCollapsed }">
    <!-- Logo area -->
    <div class="sidebar-header">
      <div class="sidebar-logo">
        <div class="logo-icon">
          <i class="pi pi-building"></i>
        </div>
        <Transition name="fade-slide">
          <div class="logo-text" v-if="!sidebarCollapsed">
            <span class="logo-name">DomuWave</span>
            <span class="logo-sub">Gestione Condomini</span>
          </div>
        </Transition>
      </div>
      <Button :icon="sidebarCollapsed ? 'pi pi-angle-right' : 'pi pi-angle-left'"
              text
              rounded
              class="collapse-btn"
              @click="toggleSidebar"
              v-tooltip.right="sidebarCollapsed ? 'Espandi' : 'Comprimi'" />
    </div>

    <!-- Navigation -->
    <nav class="sidebar-nav">
      <!-- Loading skeleton -->
      <template v-if="menuStore.loading">
        <div class="menu-skeleton" v-for="i in 5" :key="i">
          <Skeleton height="2.5rem" border-radius="10px" />
        </div>
      </template>

      <!-- Error state -->
      <div class="menu-error" v-else-if="menuStore.error">
        <i class="pi pi-exclamation-triangle"></i>
        <span v-if="!sidebarCollapsed">{{ menuStore.error }}</span>
      </div>

      <!-- Menu items -->
      <template v-else>
        <ul class="menu-list">
          <li v-for="item in menuStore.menuItems"
              :key="item.key"
              class="menu-item"
              :class="{ 'has-children': item.items?.length }">
            <!-- Item with children (group) -->
            <template v-if="item.items?.length">
              <button class="menu-link menu-group-toggle"
                      :class="{ active: isGroupActive(item), open: openGroups.includes(item.key) }"
                      @click="toggleGroup(item.key)"
                      v-tooltip.right="sidebarCollapsed ? item.label : undefined">
                <i :class="[item.icon, 'menu-icon']"></i>
                <Transition name="fade-slide">
                  <span class="menu-label" v-if="!sidebarCollapsed">{{ item.label }}</span>
                </Transition>
                <Transition name="fade-slide">
                  <i class="pi menu-chevron"
                     :class="openGroups.includes(item.key) ? 'pi-chevron-down' : 'pi-chevron-right'"
                     v-if="!sidebarCollapsed"></i>
                </Transition>
              </button>

              <!-- Sub-items -->
              <Transition name="submenu">
                <ul v-if="!sidebarCollapsed && openGroups.includes(item.key)"
                    class="submenu-list">
                  <li v-for="child in item.items" :key="child.key" class="submenu-item">
                    <RouterLink :to="child.to ?? '#'"
                                class="menu-link submenu-link"
                                :class="{ active: route.path === child.to }">
                      <i :class="[child.icon, 'menu-icon submenu-icon']"></i>
                      <span class="menu-label">{{ child.label }}</span>
                    </RouterLink>
                  </li>
                </ul>
              </Transition>
            </template>

            <!-- Simple link -->
            <RouterLink v-else
                        :to="item.to ?? '#'"
                        class="menu-link"
                        :class="{ active: route.path === item.to }"
                        v-tooltip.right="sidebarCollapsed ? item.label : undefined">
              <i :class="[item.icon, 'menu-icon']"></i>
              <Transition name="fade-slide">
                <span class="menu-label" v-if="!sidebarCollapsed">{{ item.label }}</span>
              </Transition>
            </RouterLink>
          </li>
        </ul>
      </template>
    </nav>

    <!-- Bottom: user info + logout -->
    <div class="sidebar-footer">
      <div class="user-info" v-tooltip.right="sidebarCollapsed ? authStore.currentUser?.displayName : undefined">
        <Avatar :label="userInitials"
                shape="circle"
                class="user-avatar" />
        <Transition name="fade-slide">
          <div class="user-details" v-if="!sidebarCollapsed">
            <span class="user-name">{{ authStore.currentUser?.displayName }}</span>
            <span class="user-role">{{ authStore.currentUser?.role }}</span>
          </div>
        </Transition>
      </div>

      <Button icon="pi pi-sign-out"
              text
              rounded
              severity="secondary"
              class="logout-btn"
              v-tooltip.right="'Esci'"
              @click="handleLogout" />
    </div>
  </aside>
</template>

<script setup>
  import { ref, computed, onMounted } from 'vue'
  import { RouterLink, useRoute, useRouter } from 'vue-router'
  import { useAuthStore } from '@/stores/authStore'
  import { useMenuStore } from '@/stores/menuStore'
  import Button from 'primevue/button'
  import Avatar from 'primevue/avatar'
  import Skeleton from 'primevue/skeleton'

  const route = useRoute()
  const router = useRouter()
  const authStore = useAuthStore()
  const menuStore = useMenuStore()

  const sidebarCollapsed = ref(false)
  const openGroups = ref([])

  // Load menu on mount if not already loaded
  onMounted(async () => {
    if (authStore.isAuthenticated && menuStore.menuItems.length === 0) {
      await menuStore.fetchMenu()
      // Auto-open the group containing the current route
      autoOpenCurrentGroup()
    }
  })

  function autoOpenCurrentGroup() {
    menuStore.menuItems.forEach((item) => {
      if (item.items?.some((child) => child.to === route.path)) {
        if (!openGroups.value.includes(item.key)) {
          openGroups.value.push(item.key)
        }
      }
    })
  }

  function toggleSidebar() {
    sidebarCollapsed.value = !sidebarCollapsed.value
  }

  function toggleGroup(key) {
    const idx = openGroups.value.indexOf(key)
    if (idx > -1) {
      openGroups.value.splice(idx, 1)
    } else {
      openGroups.value.push(key)
    }
  }

  function isGroupActive(item) {
    return item.items?.some((child) => child.to === route.path)
  }

  function handleLogout() {
    authStore.logout()
    menuStore.clearMenu()
    router.push({ name: 'Login' })
  }

  const userInitials = computed(() => {
    const name = authStore.currentUser?.displayName ?? authStore.currentUser?.username ?? '?'
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2)
  })
</script>

<style scoped>
  /* ── Sidebar ─────────────────────────────────────────────────────────────── */
  .app-sidebar {
    width: 260px;
    min-height: 100vh;
    background: var(--p-surface-card);
    border-right: 1px solid var(--p-surface-border);
    display: flex;
    flex-direction: column;
    transition: width 0.28s cubic-bezier(0.4, 0, 0.2, 1);
    position: relative;
    z-index: 10;
    flex-shrink: 0;
  }

    .app-sidebar.collapsed {
      width: 68px;
    }

  /* ── Header ──────────────────────────────────────────────────────────────── */
  .sidebar-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 1.25rem 1rem 1.25rem 1.25rem;
    border-bottom: 1px solid var(--p-surface-border);
    min-height: 72px;
    gap: 0.5rem;
  }

  .sidebar-logo {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    overflow: hidden;
    flex: 1;
    min-width: 0;
  }

  .logo-icon {
    width: 38px;
    height: 38px;
    flex-shrink: 0;
    border-radius: 10px;
    background: linear-gradient(135deg, var(--p-primary-400), var(--p-primary-600));
    display: flex;
    align-items: center;
    justify-content: center;
  }

    .logo-icon i {
      color: #fff;
      font-size: 1.1rem;
    }

  .logo-text {
    display: flex;
    flex-direction: column;
    white-space: nowrap;
    overflow: hidden;
  }

  .logo-name {
    font-weight: 700;
    font-size: 1rem;
    color: var(--p-text-color);
    line-height: 1.2;
  }

  .logo-sub {
    font-size: 0.68rem;
    color: var(--p-text-muted-color);
    letter-spacing: 0.4px;
    text-transform: uppercase;
  }

  .collapse-btn {
    flex-shrink: 0;
    width: 28px !important;
    height: 28px !important;
    color: var(--p-text-muted-color) !important;
  }

  /* ── Nav ─────────────────────────────────────────────────────────────────── */
  .sidebar-nav {
    flex: 1;
    overflow-y: auto;
    overflow-x: hidden;
    padding: 1rem 0.75rem;
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
    scrollbar-width: thin;
    scrollbar-color: var(--p-surface-border) transparent;
  }

  /* ── Menu skeleton ───────────────────────────────────────────────────────── */
  .menu-skeleton {
    padding: 0.25rem 0;
  }

  /* ── Menu error ──────────────────────────────────────────────────────────── */
  .menu-error {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.75rem;
    color: var(--orange-400);
    font-size: 0.82rem;
  }

  /* ── Menu list ───────────────────────────────────────────────────────────── */
  .menu-list {
    list-style: none;
    margin: 0;
    padding: 0;
    display: flex;
    flex-direction: column;
    gap: 0.15rem;
  }

  /* ── Menu link ───────────────────────────────────────────────────────────── */
  .menu-link {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    padding: 0.65rem 0.85rem;
    border-radius: 10px;
    color: var(--p-text-muted-color);
    text-decoration: none;
    cursor: pointer;
    background: transparent;
    border: none;
    width: 100%;
    text-align: left;
    font-size: 0.875rem;
    font-weight: 500;
    transition: background 0.15s, color 0.15s;
    position: relative;
    white-space: nowrap;
    overflow: hidden;
  }

    .menu-link:hover {
      background: var(--p-surface-hover);
      color: var(--p-text-color);
    }

    .menu-link.active {
      background: var(--primary-900, rgba(99, 102, 241, 0.15));
      color: var(--p-primary-400);
      font-weight: 600;
    }

      .menu-link.active .menu-icon {
        color: var(--p-primary-400);
      }

  /* ── Group toggle active ─────────────────────────────────────────────────── */
  .menu-group-toggle.active {
    color: var(--p-primary-400);
  }

  /* ── Icons ───────────────────────────────────────────────────────────────── */
  .menu-icon {
    font-size: 1rem;
    flex-shrink: 0;
    width: 20px;
    text-align: center;
  }

  .submenu-icon {
    font-size: 0.8rem;
  }

  /* ── Label ───────────────────────────────────────────────────────────────── */
  .menu-label {
    flex: 1;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  /* ── Chevron ─────────────────────────────────────────────────────────────── */
  .menu-chevron {
    font-size: 0.7rem;
    margin-left: auto;
    flex-shrink: 0;
    transition: transform 0.2s;
  }

  /* ── Submenu ─────────────────────────────────────────────────────────────── */
  .submenu-list {
    list-style: none;
    margin: 0.2rem 0 0 1rem;
    padding: 0;
    display: flex;
    flex-direction: column;
    gap: 0.1rem;
    border-left: 2px solid var(--p-surface-border);
    padding-left: 0.5rem;
  }

  .submenu-link {
    font-size: 0.83rem;
    padding: 0.5rem 0.75rem;
  }

  /* ── Footer ──────────────────────────────────────────────────────────────── */
  .sidebar-footer {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 0.5rem;
    padding: 1rem 0.75rem;
    border-top: 1px solid var(--p-surface-border);
    min-height: 68px;
  }

  .user-info {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    overflow: hidden;
    flex: 1;
    min-width: 0;
    cursor: default;
  }

  .user-avatar {
    flex-shrink: 0;
    width: 34px !important;
    height: 34px !important;
    font-size: 0.75rem !important;
    background: var(--p-primary-700);
    color: var(--p-primary-100);
  }

  .user-details {
    display: flex;
    flex-direction: column;
    overflow: hidden;
    white-space: nowrap;
  }

  .user-name {
    font-size: 0.83rem;
    font-weight: 600;
    color: var(--p-text-color);
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .user-role {
    font-size: 0.72rem;
    color: var(--p-text-muted-color);
    overflow: hidden;
    text-overflow: ellipsis;
    text-transform: capitalize;
  }

  .logout-btn {
    flex-shrink: 0;
    width: 32px !important;
    height: 32px !important;
  }

  /* ── Transitions ─────────────────────────────────────────────────────────── */
  .fade-slide-enter-active,
  .fade-slide-leave-active {
    transition: opacity 0.18s ease, transform 0.18s ease;
  }

  .fade-slide-enter-from,
  .fade-slide-leave-to {
    opacity: 0;
    transform: translateX(-6px);
  }

  .submenu-enter-active,
  .submenu-leave-active {
    transition: all 0.22s ease;
    overflow: hidden;
  }

  .submenu-enter-from,
  .submenu-leave-to {
    max-height: 0;
    opacity: 0;
  }

  .submenu-enter-to,
  .submenu-leave-from {
    max-height: 300px;
    opacity: 1;
  }
</style>
