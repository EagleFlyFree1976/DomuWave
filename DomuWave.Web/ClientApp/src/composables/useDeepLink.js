/**
 * useDeepLink.js
 *
 * Composable that resolves deep-link context from a resource DTO.
 * When a URL contains a resource ID (e.g. ?spesa=42) but the resource is not
 * in the current filtered list, this helper selects the correct tenant
 * (SuperAdmin only) and condominium so the view can reload and show the item.
 *
 * Usage:
 *   const { resolveContext } = useDeepLink()
 *   await resolveContext(dto)  // dto must have { tenantId, condominiumId }
 */

import { useAppStore }     from '@/stores/app'
import { useSessionStore } from '@/stores/sessionStore'

export function useDeepLink() {
  const store   = useAppStore()
  const session = useSessionStore()

  /**
   * Selects tenant (if SuperAdmin) and condominium based on a resource DTO,
   * then reloads fiscal years so the view context is correct.
   *
   * @param {{ tenantId: string, condominiumId: number }} resource
   */
  async function resolveContext(resource) {
    if (!resource) return

    // SuperAdmin: select the correct tenant
    if (session.isSuperAdmin && resource.tenantId) {
      if (!session.availableTenants.length) await session.loadTenants()
      const tenant = session.availableTenants.find(
        t => t.id?.toLowerCase() === resource.tenantId?.toLowerCase()
      )
      if (tenant) session.selectTenant(tenant)
    }

    // Select the correct condominium
    if (resource.condominiumId) {
      if (!store.condomini.length) await store.loadCondomini()
      store.selectCondominio(resource.condominiumId)
      await store.loadFiscalYears()
    }
  }

  return { resolveContext }
}
