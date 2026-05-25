import { defineStore } from 'pinia'
import { ref } from 'vue'
import apiClient from '@/services/api'

export const useMenuStore = defineStore('menu', () => {
  const menuItems = ref([])
  const loading = ref(false)
  const error = ref(null)

  async function fetchMenu() {
    loading.value = true
    error.value = null
    try {
      const response = await apiClient.get('/menues')
      menuItems.value = buildTree(response.data)
    } catch (err) {
      if (err.response?.status === 404) {
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
   * Converte la lista piatta con parentMenuId in un albero gerarchico.
   * Struttura attesa dal backend:
   * { id, parentMenuId, icon, description, shortDescription, action, authorizationCode, orderKey }
   */
  function buildTree(flat) {
    if (!Array.isArray(flat) || flat.length === 0) return []

    // Mappa id → nodo
    const map = {}
    flat.forEach((item) => {
      map[item.id] = {
        key: String(item.id),
        label: item.description ?? item.shortDescription ?? '',
        path: item.action ?? '/',
        icon: resolveIcon(item.icon, item.action, item.description),
        order: item.orderKey ?? item.id,
        tags: item.tags ? item.tags.split(',').map((t) => t.trim()) : [],
        features: item.features ? item.features.split(',').map((f) => f.trim()) : [],
        children: [],
      }
    })

    // Costruisce l'albero
    const roots = []
    flat.forEach((item) => {
      const node = map[item.id]
      const parent = item.parentMenuId ? map[item.parentMenuId] : null
      if (parent) {
        parent.children.push(node)
      } else {
        roots.push(node)
      }
    })

    // Rimuove array children vuoti e ordina
    function clean(nodes) {
      return nodes
        .sort((a, b) => a.order - b.order)
        .map((n) => {
          if (n.children.length === 0) delete n.children
          else n.children = clean(n.children)
          return n
        })
    }

    return clean(roots)
  }

  /**
   * Assegna un'icona PrimeIcons in base all'action o alla description.
   * Il backend non manda icone, quindi le deduciamo dal contesto.
   */
  function resolveIcon(iconField, action, description) {
    // Se il backend manda già un'icona valida, usala
    if (iconField && iconField.trim()) return iconField

    const key = ((action ?? '') + ' ' + (description ?? '')).toLowerCase()

    if (key.includes('dashboard') || key.includes('home') || action === '/')
      return 'pi-home'
    if (key.includes('account')) return 'pi-id-card'
    if (key.includes('user') || key.includes('utent'))
      return 'pi-users'
    if (key.includes('setting') || key.includes('impostaz'))
      return 'pi-cog'
    if (key.includes('condomin')) return 'pi-building'
    if (key.includes('unita') || key.includes('unit'))
      return 'pi-th-large'
    if (key.includes('budget')) return 'pi-chart-bar'
    if (key.includes('spesa') || key.includes('spese'))
      return 'pi-shopping-bag'
    if (key.includes('rata') || key.includes('rate') || key.includes('quota'))
      return 'pi-calendar'
    if (key.includes('pagament')) return 'pi-credit-card'
    if (key.includes('fornitor')) return 'pi-truck'
    if (key.includes('document')) return 'pi-folder'
    if (key.includes('comunicaz') || key.includes('messag'))
      return 'pi-envelope'
    if (key.includes('assembl')) return 'pi-microphone'
    if (key.includes('report') || key.includes('rendicont'))
      return 'pi-chart-line'
    if (key.includes('anagrafi')) return 'pi-address-book'
    if (key.includes('manutenzi')) return 'pi-wrench'
    if (key.includes('autorizzaz') || key.includes('permess') || key.includes('auth'))
      return 'pi-shield'

    return 'pi-circle'
  }

  return { menuItems, loading, error, fetchMenu, clearMenu }
})
