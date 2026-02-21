import { defineStore } from 'pinia'
import { ref } from 'vue'
import apiClient from '@/services/api'

export const useMenuStore = defineStore('menu', () => {
  const menuItems = ref([])
  const loading = ref(false)
  const error = ref(null)

  /**
   * Fetches menu items from GET /api/menues
   * The backend already filters items based on current user's role/permissions.
   */
  async function fetchMenu() {
    if (menuItems.value.length > 0) return // already loaded
    loading.value = true
    error.value = null
    try {
      const response = await apiClient.get('/menues')
      menuItems.value = mapMenuItems(response.data)
    } catch (err) {
      if (err.response?.status === 404) {
        // No menu items configured for this user
        menuItems.value = []
      } else {
        error.value = 'Impossibile caricare il menu.'
        console.error('[MenuStore] fetchMenu error:', err)
      }
    } finally {
      loading.value = false
    }
  }

  function clearMenu() {
    menuItems.value = []
    error.value = null
  }

  /**
   * Maps backend MenuItemDto to PrimeVue sidebar-compatible format.
   * Expected dto shape:
   * { id, label, icon, route, parentId, order, children? }
   */
  function mapMenuItems(items) {
    if (!Array.isArray(items)) return []

    // Build flat → tree if backend returns flat list with parentId
    const roots = []
    const map = {}

    items.forEach((item) => {
      map[item.id] = {
        key: String(item.id),
        label: item.label ?? item.Label,
        icon: resolveIcon(item.icon ?? item.Icon),
        to: item.route ?? item.Route ?? null,
        order: item.order ?? item.Order ?? 0,
        items: [],
      }
    })

    items.forEach((item) => {
      const parentId = item.parentId ?? item.ParentId
      if (parentId && map[parentId]) {
        map[parentId].items.push(map[item.id])
      } else {
        roots.push(map[item.id])
      }
    })

    // Remove empty children arrays and sort
    return roots
      .sort((a, b) => a.order - b.order)
      .map((item) => {
        if (item.items.length === 0) delete item.items
        else item.items.sort((a, b) => a.order - b.order)
        return item
      })
  }

  /**
   * Maps icon strings to PrimeIcons class names.
   * Backend may send names like "dashboard", "settings", "building", etc.
   */
  function resolveIcon(iconName) {
    if (!iconName) return 'pi pi-circle'
    if (iconName.startsWith('pi ')) return iconName
    const iconMap = {
      dashboard: 'pi pi-home',
      home: 'pi pi-home',
      condomini: 'pi pi-building',
      building: 'pi pi-building',
      condominio: 'pi pi-building',
      anagrafica: 'pi pi-id-card',
      users: 'pi pi-users',
      utenti: 'pi pi-users',
      finance: 'pi pi-wallet',
      contabilita: 'pi pi-wallet',
      budget: 'pi pi-chart-bar',
      spese: 'pi pi-shopping-bag',
      rate: 'pi pi-calendar',
      pagamenti: 'pi pi-credit-card',
      rendiconto: 'pi pi-file',
      documenti: 'pi pi-folder',
      comunicazioni: 'pi pi-envelope',
      settings: 'pi pi-cog',
      impostazioni: 'pi pi-cog',
      report: 'pi pi-chart-line',
      assemblea: 'pi pi-microphone',
      fornitori: 'pi pi-truck',
      manutenzione: 'pi pi-wrench',
    }
    const key = iconName.toLowerCase().trim()
    return iconMap[key] ?? 'pi pi-circle'
  }

  return { menuItems, loading, error, fetchMenu, clearMenu }
})
