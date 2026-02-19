import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  { path: '/', redirect: '/dashboard' },
  { path: '/dashboard',      component: () => import('@/views/DashboardView.vue'),      meta: { title: 'Dashboard' } },
  { path: '/condomini',      component: () => import('@/views/CondominiView.vue'),       meta: { title: 'Condomini' } },
  { path: '/condomini/:id',  component: () => import('@/views/CondominioDetailView.vue'),meta: { title: 'Dettaglio Condominio' } },
  { path: '/unita',          component: () => import('@/views/UnitaView.vue'),           meta: { title: 'Unità Immobiliari' } },
  { path: '/budget',         component: () => import('@/views/BudgetView.vue'),          meta: { title: 'Budget & Spese' } },
  { path: '/rate',           component: () => import('@/views/RateView.vue'),            meta: { title: 'Rate & Quote' } },
  { path: '/fornitori',      component: () => import('@/views/FornitoriView.vue'),       meta: { title: 'Fornitori' } },
  { path: '/documenti',      component: () => import('@/views/DocumentiView.vue'),       meta: { title: 'Documenti' } },
  { path: '/comunicazioni',  component: () => import('@/views/ComunicazioniView.vue'),   meta: { title: 'Comunicazioni' } },
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.afterEach((to) => {
  document.title = to.meta.title ? `${to.meta.title} · DomuWave` : 'DomuWave'
})

export default router
