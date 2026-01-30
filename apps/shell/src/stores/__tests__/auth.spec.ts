import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'

// Mock the auth plugin - factory must not reference outer variables (vi.mock is hoisted)
vi.mock('@/plugins/auth', () => ({
  userManager: {
    getUser: vi.fn(),
    signinRedirect: vi.fn(),
    signinRedirectCallback: vi.fn(),
    signoutRedirect: vi.fn(),
    signinSilent: vi.fn(),
  },
}))

import { userManager } from '@/plugins/auth'
import { useAuthStore } from '../auth'

const mockUserManager = userManager as unknown as {
  getUser: ReturnType<typeof vi.fn>
  signinRedirect: ReturnType<typeof vi.fn>
  signinRedirectCallback: ReturnType<typeof vi.fn>
  signoutRedirect: ReturnType<typeof vi.fn>
  signinSilent: ReturnType<typeof vi.fn>
}

describe('auth store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('has null user initially', () => {
    const store = useAuthStore()
    expect(store.user).toBeNull()
  })

  it('isAuthenticated returns false when no user', () => {
    const store = useAuthStore()
    expect(store.isAuthenticated).toBe(false)
  })

  it('isAuthenticated returns true when user exists and not expired', () => {
    const store = useAuthStore()
    store.user = { expired: false, access_token: 'token' } as any
    expect(store.isAuthenticated).toBe(true)
  })

  it('isAuthenticated returns false when user is expired', () => {
    const store = useAuthStore()
    store.user = { expired: true } as any
    expect(store.isAuthenticated).toBe(false)
  })

  it('accessToken returns token from user', () => {
    const store = useAuthStore()
    store.user = { access_token: 'my-token' } as any
    expect(store.accessToken).toBe('my-token')
  })

  it('accessToken returns null when no user', () => {
    const store = useAuthStore()
    expect(store.accessToken).toBeNull()
  })

  it('initialize loads user from userManager', async () => {
    const mockUser = { access_token: 'token', expired: false }
    mockUserManager.getUser.mockResolvedValue(mockUser)

    const store = useAuthStore()
    await store.initialize()

    expect(mockUserManager.getUser).toHaveBeenCalledOnce()
    expect(store.user).toEqual(mockUser)
  })

  it('initialize sets user to null on error', async () => {
    mockUserManager.getUser.mockRejectedValue(new Error('fail'))

    const store = useAuthStore()
    await store.initialize()

    expect(store.user).toBeNull()
  })

  it('login calls signinRedirect', async () => {
    const store = useAuthStore()
    await store.login()
    expect(mockUserManager.signinRedirect).toHaveBeenCalledOnce()
  })

  it('handleCallback calls signinRedirectCallback and sets user', async () => {
    const mockUser = { access_token: 'callback-token' }
    mockUserManager.signinRedirectCallback.mockResolvedValue(mockUser)

    const store = useAuthStore()
    const result = await store.handleCallback()

    expect(mockUserManager.signinRedirectCallback).toHaveBeenCalledOnce()
    expect(store.user).toEqual(mockUser)
    expect(result).toEqual(mockUser)
  })

  it('logout calls signoutRedirect and clears user', async () => {
    const store = useAuthStore()
    store.user = { access_token: 'token' } as any
    await store.logout()

    expect(mockUserManager.signoutRedirect).toHaveBeenCalledOnce()
    expect(store.user).toBeNull()
  })

  it('getAccessToken returns token when user is valid', async () => {
    const store = useAuthStore()
    store.user = { access_token: 'valid-token', expired: false } as any

    const token = await store.getAccessToken()
    expect(token).toBe('valid-token')
  })

  it('getAccessToken refreshes when user is expired', async () => {
    const refreshedUser = { access_token: 'refreshed-token', expired: false }
    mockUserManager.signinSilent.mockResolvedValue(refreshedUser)

    const store = useAuthStore()
    store.user = { access_token: 'old', expired: true } as any

    const token = await store.getAccessToken()
    expect(mockUserManager.signinSilent).toHaveBeenCalledOnce()
    expect(token).toBe('refreshed-token')
  })

  it('getAccessToken returns null when refresh fails', async () => {
    mockUserManager.signinSilent.mockRejectedValue(new Error('fail'))

    const store = useAuthStore()
    store.user = { expired: true } as any

    const token = await store.getAccessToken()
    expect(token).toBeNull()
  })
})
