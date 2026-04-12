import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { useSessionStore } from '@/stores/sessionStore'

const routes = [
  // ── Pubblica ────────────────────────────────────────────────────────────
  {
    path: '/login',
    component: () => import('@/views/LoginView.vue'),
    meta: { title: 'Accedi' },
  },
  {
    path: '/reset-password',
    component: () => import('@/views/ResetPasswordView.vue'),
    meta: { title: 'Reimposta password' },
  },
  {
    path: '/select-tenant',
    component: () => import('@/views/CondomininoTenantSelectView.vue'),
    meta: { title: 'Seleziona condominio', requiresAuth: true },
  },

  // ── Protette (con AppLayout come wrapper) ────────────────────────────────
  {
    path: '/',
    component: () => import('@/components/layout/AppLayout.vue'),
    meta: { requiresAuth: true },
    redirect: '/dashboard',
    children: [
      { path: 'dashboard', component: () => import('@/views/DashboardView.vue'), meta: { title: 'Dashboard' } },
      { path: 'condomini', component: () => import('@/views/CondominiView.vue'), meta: { title: 'Condomini', requiresTenant: true } },
      {
        path: 'condomini/:id',
        component: () => import('@/components/layout/CondominioLayout.vue'),
        meta: { requiresTenant: true },
        children: [
          { path: '', component: () => import('@/views/CondominioDetailView.vue'), meta: { title: 'Dettaglio Condominio' } },
          { path: 'unita', component: () => import('@/views/UnitaView.vue'), meta: { title: 'Unità Immobiliari' } },
          { path: 'panoramica', component: () => import('@/views/PanoramicaView.vue'), meta: { title: 'Panoramica Occupanti' } },
          { path: 'budget', component: () => import('@/views/BudgetView.vue'), meta: { title: 'Budget' } },
          { path: 'budget/:budgetId/dettaglio', component: () => import('@/views/ConsuntivoDetailView.vue'), meta: { title: 'Dettaglio Consuntivo' } },
          { path: 'spese', component: () => import('@/views/ExpenseView.vue'), meta: { title: 'Spese' } },
          { path: 'consumi', component: () => import('@/views/ConsumiView.vue'), meta: { title: 'Consumi' } },
          { path: 'rate', component: () => import('@/views/RateView.vue'), meta: { title: 'Rate & Quote' } },
          { path: 'fornitori', component: () => import('@/views/FornitoriView.vue'), meta: { title: 'Fornitori' } },
          { path: 'documenti', component: () => import('@/views/DocumentiView.vue'), meta: { title: 'Documenti' } },
          { path: 'comunicazioni', component: () => import('@/views/ComunicazioniView.vue'), meta: { title: 'Comunicazioni' } },
          { path: 'setup', component: () => import('@/views/SetupCondominioView.vue'), meta: { title: 'Setup Condominio' } },
        ],
      },
      { path: 'esercizi-fiscali', component: () => import('@/views/EserciziFiscaliView.vue'), meta: { title: 'Esercizi Fiscali', requiresTenant: true } },
      { path: 'piano-dei-conti',  component: () => import('@/views/PianoDeiContiView.vue'),  meta: { title: 'Piano dei Conti',  requiresTenant: true } },
      { path: 'categorie-piano-dei-conti', component: () => import('@/views/CategoriePianoDeiContiView.vue'), meta: { title: 'Categorie Piano dei Conti', requiresTenant: true } },
      { path: 'template-categorie-piano-dei-conti', component: () => import('@/views/TemplateCategoriePianoDeiContiView.vue'), meta: { title: 'Template Categorie Piano dei Conti', requiresAuth: true, requiredRole: 'SuperAdmin' } },
      { path: 'template-piano-dei-conti', component: () => import('@/views/TemplateContiView.vue'), meta: { title: 'Template Piano dei Conti', requiresTenant: true } },
      { path: 'tabelle-millesimali', component: () => import('@/views/TabelleMillesimaliView.vue'), meta: { title: 'Tabelle Millesimali', requiresTenant: true } },
      { path: 'budget', component: () => import('@/views/BudgetView.vue'), meta: { title: 'Budget', requiresTenant: true } },
      { path: 'consuntivo', component: () => import('@/views/ConsuntivoView.vue'), meta: { title: 'Consuntivo', requiresTenant: true } },
      { path: 'spese',  component: () => import('@/views/ExpenseView.vue'), meta: { title: 'Spese', requiresTenant: true } },
      { path: 'consumi', component: () => import('@/views/ConsumiView.vue'), meta: { title: 'Consumi', requiresTenant: true } },
      { path: 'rate', component: () => import('@/views/RateView.vue'), meta: { title: 'Rate & Quote', requiresTenant: true } },
      { path: 'fornitori', component: () => import('@/views/FornitoriView.vue'), meta: { title: 'Fornitori', requiresTenant: true } },
      { path: 'manutenzioni', component: () => import('@/views/ManutenzioniView.vue'), meta: { title: 'Manutenzioni', requiresTenant: true } },
      { path: 'lavori-straordinari', component: () => import('@/views/LavoriStraordinariView.vue'), meta: { title: 'Lavori Straordinari', requiresTenant: true } },
      { path: 'documenti', component: () => import('@/views/DocumentiView.vue'), meta: { title: 'Documenti', requiresTenant: true } },
      { path: 'comunicazioni', component: () => import('@/views/ComunicazioniView.vue'), meta: { title: 'Comunicazioni', requiresTenant: true } },


      { path: 'tenants', name:'tenants', component: () => import('@/views/tenants/TenantList.vue'), meta: { title: 'Gestione Tenant' } },
      {
        path: 'tenants/new', name: 'tenant-new', component: () => import('@/views/tenants/TenantDetail.vue'), meta: {
          title: 'Nuovo Tenant',
          requiresAuth: true,
          requiredRole: 'SuperAdmin',
        }
      },
      {
        path: 'tenants/:id', name: 'tenant-detail', component: () => import('@/views/tenants/TenantDetail.vue'), props: true,
        meta: {
          title: 'Modifica Tenant',
          requiresAuth: true,
          requiredRole: 'SuperAdmin',
        }
      },

      // ── Autorizzazioni (SuperAdmin) ───────────────────────────────────────
      {
        path: 'autorizzazioni', name: 'autorizzazioni',
        component: () => import('@/views/authorizations/AuthorizationsView.vue'),
        meta: { title: 'Autorizzazioni', requiresAuth: true, requiredRole: 'SuperAdmin' },
      },

      // ── Utenti (SuperAdmin) ────────────────────────────────────────────────
      {
        path: 'users', name: 'users',
        component: () => import('@/views/users/UserList.vue'),
        meta: { title: 'Gestione Utenti', requiresAuth: true, requiredRole: 'SuperAdmin' },
      },
      {
        path: 'users/new', name: 'user-new',
        component: () => import('@/views/users/UserDetail.vue'),
        meta: { title: 'Nuovo Utente', requiresAuth: true, requiredRole: 'SuperAdmin' },
      },
      {
        path: 'users/:id', name: 'user-detail',
        component: () => import('@/views/users/UserDetail.vue'),
        props: true,
        meta: { title: 'Modifica Utente', requiresAuth: true, requiredRole: 'SuperAdmin' },
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
  const session = useSessionStore()

  const requiresAuth = to.matched.some((r) => r.meta?.requiresAuth)
  const requiresTenant = to.matched.some((r) => r.meta?.requiresTenant)

  if (to.path === '/login' && authStore.isAuthenticated) {
    return next('/dashboard')
  }

  if (requiresAuth && !authStore.isAuthenticated) {
    return next({ path: '/login', query: { redirect: to.fullPath } })
  }

  if (requiresTenant && session.isSuperAdmin && !session.hasTenantSelected) {
    return next('/tenants')
  }

  next()
})

// ── Titolo pagina ────────────────────────────────────────────────────────────
router.afterEach((to) => {
  document.title = to.meta.title ? `${to.meta.title} · DomuWave` : 'DomuWave'
})

export default router
