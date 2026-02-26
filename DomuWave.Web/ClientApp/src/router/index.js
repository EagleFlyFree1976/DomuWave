import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
 
const routes = [
  // ── Pubblica ────────────────────────────────────────────────────────────
  {
    path: '/login',
    component: () => import('@/views/LoginView.vue'),
    meta: { title: 'Accedi' },
  },

  // ── Protette (con AppLayout come wrapper) ────────────────────────────────
  {
    path: '/',
    component: () => import('@/components/layout/AppLayout.vue'),
    meta: { requiresAuth: true },
    redirect: '/dashboard',
    children: [
      { path: 'dashboard', component: () => import('@/views/DashboardView.vue'), meta: { title: 'Dashboard' } },
      { path: 'condomini', component: () => import('@/views/CondominiView.vue'), meta: { title: 'Condomini' } },
      { path: 'condomini/:id', component: () => import('@/views/CondominioDetailView.vue'), meta: { title: 'Dettaglio Condominio' } },
      { path: 'unita', component: () => import('@/views/UnitaView.vue'), meta: { title: 'Unità Immobiliari' } },
      { path: 'budget', component: () => import('@/views/BudgetView.vue'), meta: { title: 'Budget & Spese' } },
      { path: 'rate', component: () => import('@/views/RateView.vue'), meta: { title: 'Rate & Quote' } },
      { path: 'fornitori', component: () => import('@/views/FornitoriView.vue'), meta: { title: 'Fornitori' } },
      { path: 'documenti', component: () => import('@/views/DocumentiView.vue'), meta: { title: 'Documenti' } },
      { path: 'comunicazioni', component: () => import('@/views/ComunicazioniView.vue'), meta: { title: 'Comunicazioni' } },


      { path: 'tenants', component: () => import('@/views/tenants/TenantList.vue'), meta: { title: 'Gestione Tenant' } },
      {
        path: 'tenants/new', name: 'tenant-new', component: () => import('@/views/tenants/TenantDetail.vue'), meta: {
          title: 'Nuovo Tenant',
          requiresAuth: true,
          requiredRole: 'SuperAdmin',
        }
      },
      {
        path: 'tenants/:id', name: 'tenant-detail', component: import('@/views/tenants/TenantDetail.vue'), props: true,
        meta: {
          title: 'Modifica Tenant',
          requiresAuth: true,
          requiredRole: 'SuperAdmin',
        }
      },
    ],
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

// ── Navigation guard ────────────────────────────────────────────────────────
router.beforeEach((to, _from, next) => {
  const authStore = useAuthStore()

  const requiresAuth = to.matched.some((r) => r.meta?.requiresAuth)

  if (to.path === '/login' && authStore.isAuthenticated) {
    return next('/dashboard')
  }

  if (requiresAuth && !authStore.isAuthenticated) {
    return next({ path: '/login', query: { redirect: to.fullPath } })
  }

  next()
})

// ── Titolo pagina ────────────────────────────────────────────────────────────
router.afterEach((to) => {
  document.title = to.meta.title ? `${to.meta.title} · DomuWave` : 'DomuWave'
})

export default router
