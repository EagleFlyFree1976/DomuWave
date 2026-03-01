import { computed } from 'vue'
import { useAuthStore } from '@/stores/authStore'

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

  const profile = computed(() => Number(authStore.user?.profile ?? 0))

  const isSuperAdmin  = computed(() => profile.value === 1)
  const isTenantAdmin = computed(() => profile.value === 2)
  const isCondomino   = computed(() => profile.value === 3)

  // Solo SuperAdmin e TenantAdmin possono creare, modificare o eliminare
  const canCreate = computed(() => profile.value === 1 || profile.value === 2)
  const canEdit   = computed(() => profile.value === 1 || profile.value === 2)
  const canDelete = computed(() => profile.value === 1 || profile.value === 2)

  return { isSuperAdmin, isTenantAdmin, isCondomino, canCreate, canEdit, canDelete }
}
