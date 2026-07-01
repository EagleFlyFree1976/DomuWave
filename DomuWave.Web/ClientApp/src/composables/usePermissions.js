import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { condominoCanEdit, condominoCanCreate } from '@/config/condominoAccess'

/**
 * Permessi basati sul profilo dell'utente loggato.
 *
 * Profili:
 *   1 = SuperAdmin      → accesso completo
 *   2 = TenantAdmin     → gestione del proprio tenant
 *   3 = User/Condomino  → sola lettura
 */
export function usePermissions() {
  const authStore = useAuthStore()
  const route = useRoute()

  const profile = computed(() => Number(authStore.user?.profile ?? 0))

  const isSuperAdmin  = computed(() => profile.value === 1)
  const isTenantAdmin = computed(() => profile.value === 2)
  const isCondomino   = computed(() => profile.value === 3)

  const isAdmin = computed(() => profile.value === 1 || profile.value === 2)

  /**
   * Per il condòmino i permessi sono per-sezione (dedotta dalla route corrente)
   * e limitati ai propri dati lato backend. Vedi config/condominoAccess.js.
   */
  const condominoEditHere   = computed(() => isCondomino.value && condominoCanEdit(route.path))
  const condominoCreateHere = computed(() => isCondomino.value && condominoCanCreate(route.path))

  // SuperAdmin/TenantAdmin: pieno controllo.
  // Condòmino: crea/modifica solo dove l'allow-list lo consente; mai elimina.
  const canCreate = computed(() => isAdmin.value || condominoCreateHere.value)
  const canEdit   = computed(() => isAdmin.value || condominoEditHere.value)
  const canDelete = computed(() => isAdmin.value)

  return { isSuperAdmin, isTenantAdmin, isCondomino, isAdmin, canCreate, canEdit, canDelete }
}
