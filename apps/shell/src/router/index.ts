import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { defineComponent } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useTenantStore } from '@/stores/tenant'

const RemoteRouterView = defineComponent({
  name: 'RemoteRouterView',
  template: '<router-view />',
})

const routes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'login',
    component: () => import('@/pages/LoginPage.vue'),
    meta: { layout: 'auth' },
  },
  {
    path: '/signup',
    name: 'signup',
    component: () => import('@/pages/SignupPage.vue'),
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
    path: '/admin',
    name: 'admin',
    component: RemoteRouterView,
    meta: { requiresAuth: true, requiresOrganization: true },
    children: [
      {
        path: ':pathMatch(.*)*',
        name: 'admin-fallback',
        component: () => import('@/pages/AdminPlaceholder.vue'),
      },
    ],
  },
  {
    path: '/app',
    name: 'client',
    component: RemoteRouterView,
    meta: { requiresAuth: true, requiresOrganization: true },
    children: [
      {
        path: ':pathMatch(.*)*',
        name: 'client-fallback',
        component: () => import('@/pages/ClientPlaceholder.vue'),
      },
    ],
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

let remotesLoaded = false

export async function loadRemoteRoutes(): Promise<void> {
  if (remotesLoaded) return
  remotesLoaded = true

  // Load admin remote routes
  try {
    const adminModule = await import('admin/routes')
    const adminRoutes: RouteRecordRaw[] = adminModule.default

    router.removeRoute('admin-fallback')
    for (const route of adminRoutes) {
      router.addRoute('admin', route)
    }
    console.log('[Shell] Admin remote routes loaded:', adminRoutes.length)
  } catch (error) {
    console.warn('[Shell] Failed to load admin remote:', error)
  }

  // Load client remote routes
  try {
    const clientModule = await import('client/routes')
    const clientRoutes: RouteRecordRaw[] = clientModule.default

    router.removeRoute('client-fallback')
    for (const route of clientRoutes) {
      router.addRoute('client', route)
    }
    console.log('[Shell] Client remote routes loaded:', clientRoutes.length)
  } catch (error) {
    console.warn('[Shell] Failed to load client remote:', error)
  }
}

router.beforeEach(async (to, _from, next) => {
  const authStore = useAuthStore()
  const tenantStore = useTenantStore()

  if (!authStore.user && to.name !== 'login' && to.name !== 'callback' && to.name !== 'signup') {
    await authStore.initialize()
  }

  const requiresAuth = to.meta.requiresAuth as boolean
  const requiresOrganization = to.meta.requiresOrganization as boolean

  if (requiresAuth && !authStore.isAuthenticated) {
    if (to.path !== '/login') {
      sessionStorage.setItem('intendedRoute', to.fullPath)
    }
    return next('/login')
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
