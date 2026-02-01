import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import SignInView from '../views/SignInView.vue'
import RegisterView from '../views/RegisterView.vue'

import { useAuthStore } from '@/stores/authStore'
const privateRoutes = [
  {
    path: '/dashboard',
    name: 'dashboard',
    component: () =>  import('@/views/private/DashboardView.vue'),
    meta: { requiresAuth: true }
  }
  
  , {
    path: '/profile',
    name: 'userprofile',
    component: () => import('@/views/private/UserProfileView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/accounts',
    name: 'accounts',
    component: () => import('../views/private/AccountsView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/accounts/:accountid/dashboard',
    name: 'accountdashboard',
    component: () => import('../views/private/AccountDashboardView.vue'),
    props:true,
    meta: { requiresAuth: true }
  },
  {
    path: '/accounts/:accountid',
    name: 'accountedit',
    component: () => import('../views/private/AccountEdit.vue'),
    props: true,
    meta: { requiresAuth: true }
  },
  {
    path: '/accounts/:accountid/home',
    name: 'accounthome',
    component: () => import('../views/private/AccountHome.vue'),
    props: true,
    meta: { requiresAuth: true }
  },
  {
    path: '/accounts/:accountid/transactions',
    name: 'accounttransactions',
    component: () => import('../views/private/AccountTransactions.vue'),
    props: true,
    meta: { requiresAuth: true }
  }
];
const privateAdminUsersRoutes = [
  {
    path: '/users',
    name: 'users',
    component: () => import('../views/private/admin/UsersView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/PaymentMethods',
    name: 'PaymentMethods',
    component: () => import('../views/private/admin/PaymentMethodsView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/Currencies',
    name: 'Currencies',
    component: () => import('../views/private/admin/Currencies/CurrenciesView.vue'),
    meta: { requiresAuth: true }
  },

  {
    path: '/Categories',
    name: 'Categories',
    component: () => import('../views/private/Categories/View.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/PaymentMethods/:paymentmethodid',
    name: 'paymentmethodedit',
    component: () => import('../views/private/admin/PaymentMethodsView.vue'),
    props: true,
    meta: { requiresAuth: true }
  },
  {
    path: '/Transactions',
    name: 'Transactions',
    component: () => import('../views/private/Transactions/TransactionsView.vue'),
    meta: { requiresAuth: true }
  }
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    ...privateRoutes,
    ...privateAdminUsersRoutes,
    {
      path: '/',
      name: 'home',
      component: HomeView,
      meta: { requiresAuth: false }
    }, 
    {
      path: '/signin',
      name: 'signin',
      component: SignInView,
      meta: { requiresAuth: false }
    },
    {
      path: '/logout',
      name: 'logout',
      component: () => import('@/views/LogoutView.vue'),
      meta: { requiresAuth: false }
    },
    {
      path: '/register',
      name: 'register',
      component: RegisterView,
      meta: { requiresAuth: false }
    },
    {
      path: '/about',
      name: 'about',
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import('../views/AboutView.vue'),
      meta: { requiresAuth: false }
    }

    
   


  ],
});

router.beforeEach((to, from, next) => {
  const auth = useAuthStore();

  // Carica token da storage se non presente in memoria (se implementato)
  if (!auth.token) {
    auth.loadFromStorage?.();
  }

  if (to.meta.requiresAuth && !auth.token) {
    // Se route protetta ma non autenticato, vai al login
    next({ name: 'signin' });
  } else if (!to.meta.requiresAuth && auth.token && (to.name === 'signin' || to.name === 'register' || to.name === 'home')) {
    // Se utente è autenticato e va su signin/register/home, reindirizza a dashboard
    next({ name: 'dashboard' });
  } else {
    next();
  }
});



export default router
