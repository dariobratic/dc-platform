import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useTenantStore } from '@/stores/tenant'

const routes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'login',
    component: () => import('@/pages/LoginPage.vue'),
    meta: { layout: 'auth' },
  },
  {
    path: '/callback',
    name: 'callback',
    component: () => import('@/pages/AuthCallback.vue'),
    meta: { layout: 'auth' },
  },
  {
    path: '/select-organization',
    name: 'select-organization',
    component: () => import('@/pages/OrganizationPickerPage.vue'),
    meta: { requiresAuth: true, layout: 'auth' },
  },
  {
    path: '/dashboard',
    name: 'dashboard',
    component: () => import('@/pages/DashboardPage.vue'),
    meta: { requiresAuth: true, requiresOrganization: true },
  },
  {
    path: '/admin/:pathMatch(.*)*',
    name: 'admin',
    component: () => import('@/pages/AdminPlaceholder.vue'),
    meta: { requiresAuth: true, requiresOrganization: true },
  },
  {
    path: '/app/:pathMatch(.*)*',
    name: 'client',
    component: () => import('@/pages/ClientPlaceholder.vue'),
    meta: { requiresAuth: true, requiresOrganization: true },
  },
  {
    path: '/',
    redirect: () => {
      const authStore = useAuthStore()
      return authStore.isAuthenticated ? '/dashboard' : '/login'
    },
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach(async (to, _from, next) => {
  const authStore = useAuthStore()
  const tenantStore = useTenantStore()

  if (!authStore.user && to.name !== 'login' && to.name !== 'callback') {
    await authStore.initialize()
  }

  const requiresAuth = to.meta.requiresAuth as boolean
  const requiresOrganization = to.meta.requiresOrganization as boolean

  if (requiresAuth && !authStore.isAuthenticated) {
    if (to.path !== '/login') {
      sessionStorage.setItem('intendedRoute', to.fullPath)
    }
    await authStore.login()
    return next(false)
  }

  if (requiresOrganization && !tenantStore.hasOrganization) {
    if (to.path !== '/select-organization') {
      sessionStorage.setItem('intendedRoute', to.fullPath)
      return next('/select-organization')
    }
  }

  next()
})

export default router
