import { createRouter, createWebHistory } from 'vue-router'

import StocksView from '../views/StocksView.vue'
import StockDetailsView from '../views/StockDetailsView.vue'
import WatchlistsView from '../views/WatchlistsView.vue'
import LoginView from '../views/LoginView.vue'
import RegisterView from '../views/RegisterView.vue'

import { hasValidAccessSession } from '../services/auth'

const router = createRouter({
  history: createWebHistory(),

  routes: [
    {
      path: '/',
      name: 'market',
      component: StocksView,
    },
    {
      path: '/stocks/:id',
      name: 'stock-details',
      component: StockDetailsView,
    },
    {
      path: '/watchlists',
      name: 'watchlists',
      component: WatchlistsView,
      meta: { requiresAuth: true },
    },
    {
      path: '/login',
      name: 'login',
      component: LoginView,
    },
    {
      path: '/register',
      name: 'register',
      component: RegisterView,
    },
  ],
})

router.beforeEach((to) => {
  const authenticated = hasValidAccessSession()

  if (to.meta.requiresAuth && !authenticated) {
    return {
      name: 'login',
      query: { redirect: to.fullPath },
    }
  }

  if (
    (to.name === 'login' || to.name === 'register') &&
    authenticated
  ) {
    return { name: 'watchlists' }
  }

  return true
})

export default router