import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

// Mock the auth plugin - factory must not reference outer variables (vi.mock is hoisted)
vi.mock('@/plugins/auth', () => ({
  userManager: {
    getUser: vi.fn().mockResolvedValue(null),
    signinRedirect: vi.fn(),
    signinRedirectCallback: vi.fn(),
    signoutRedirect: vi.fn(),
    signinSilent: vi.fn(),
    events: {
      addUserSignedOut: vi.fn(),
      addAccessTokenExpired: vi.fn(),
      addSilentRenewError: vi.fn(),
    },
  },
  setupAuthErrorHandling: vi.fn(),
}))

import { userManager } from '@/plugins/auth'
import router from '../../router/index'
import { useAuthStore } from '../../stores/auth'
import { useTenantStore } from '../../stores/tenant'

const mockUserManager = userManager as any

describe('router guards', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    sessionStorage.clear()
  })

  it('allows navigation to login without auth', async () => {
    await router.push('/login')
    expect(router.currentRoute.value.name).toBe('login')
  })

  it('allows navigation to callback without auth', async () => {
    await router.push('/callback')
    expect(router.currentRoute.value.name).toBe('callback')
  })

  it('redirects to login when accessing auth-required route unauthenticated', async () => {
    mockUserManager.getUser.mockResolvedValue(null)

    await router.push('/dashboard')

    expect(router.currentRoute.value.path).toBe('/login')
  })

  it('redirects to select-organization when no org set', async () => {
    const pinia = createPinia()
    setActivePinia(pinia)

    const authStore = useAuthStore()
    authStore.user = { expired: false, access_token: 'token', profile: {} } as any

    // No organization set — tenantStore has no org by default

    await router.push('/dashboard')

    expect(router.currentRoute.value.path).toBe('/select-organization')
  })

  it('allows navigation when authenticated with organization', async () => {
    const pinia = createPinia()
    setActivePinia(pinia)

    const authStore = useAuthStore()
    authStore.user = { expired: false, access_token: 'token', profile: {} } as any

    const tenantStore = useTenantStore()
    tenantStore.setOrganization('org-1')

    await router.push('/dashboard')

    expect(router.currentRoute.value.path).toBe('/dashboard')
  })

  it('allows navigation to signup without auth', async () => {
    await router.push('/signup')
    expect(router.currentRoute.value.name).toBe('signup')
  })
})
