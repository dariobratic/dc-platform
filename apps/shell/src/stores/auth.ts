import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { User } from 'oidc-client-ts'
import { userManager } from '@/plugins/auth'
import { http } from '@/plugins/http'
import type { UserProfile } from '@/types'
import type { SigninRequest, SignupRequest, SignupResponse } from '@dc-platform/shared-types'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null)

  const isAuthenticated = computed(() => !!user.value && !user.value.expired)
  const accessToken = computed(() => user.value?.access_token ?? null)
  const profile = computed(() => user.value?.profile as UserProfile | undefined)
  const tenantId = computed(() => profile.value?.tenant_id)
  const organizationId = computed(() => profile.value?.organization_id)

  async function initialize(): Promise<void> {
    try {
      user.value = await userManager.getUser()
    } catch (error) {
      console.error('Failed to initialize auth:', error)
      user.value = null
    }
  }

  async function login(): Promise<void> {
    await userManager.signinRedirect()
  }

  async function handleCallback(): Promise<User> {
    user.value = await userManager.signinRedirectCallback()
    return user.value
  }

  async function logout(): Promise<void> {
    try {
      // Revoke refresh token via backend
      const currentUser = await userManager.getUser()
      if (currentUser?.refresh_token) {
        await http.post('/api/v1/auth/logout', { refreshToken: currentUser.refresh_token }).catch(() => {})
      }
    } catch {
      // Best-effort token revocation
    }

    // Clear oidc-client-ts state
    await userManager.removeUser()
    user.value = null
  }

  async function getAccessToken(): Promise<string | null> {
    if (!user.value || user.value.expired) {
      try {
        user.value = await userManager.signinSilent()
      } catch (error) {
        console.error('Silent sign-in failed:', error)
        return null
      }
    }
    return user.value?.access_token ?? null
  }

  function storeCustomTokens(accessToken: string, refreshToken: string, expiresIn: number): void {
    // Decode JWT payload to extract claims
    const payloadBase64 = accessToken.split('.')[1]
    const payload = JSON.parse(atob(payloadBase64))

    const authority = `${import.meta.env.VITE_KEYCLOAK_URL}/realms/${import.meta.env.VITE_KEYCLOAK_REALM}`
    const clientId = import.meta.env.VITE_KEYCLOAK_CLIENT_ID

    // Build oidc-client-ts compatible user object
    const oidcUser = {
      id_token: accessToken,
      session_state: payload.session_state ?? null,
      access_token: accessToken,
      refresh_token: refreshToken,
      token_type: 'Bearer',
      scope: 'openid profile email',
      profile: {
        sub: payload.sub,
        email: payload.email,
        email_verified: payload.email_verified,
        name: payload.name,
        given_name: payload.given_name,
        family_name: payload.family_name,
        preferred_username: payload.preferred_username,
        tenant_id: payload.tenant_id,
        organization_id: payload.organization_id,
        roles: payload.roles,
      } as Record<string, unknown>,
      expires_at: Math.floor(Date.now() / 1000) + expiresIn,
    }

    // Store in sessionStorage with oidc-client-ts key format
    const storageKey = `oidc.user:${authority}:${clientId}`
    sessionStorage.setItem(storageKey, JSON.stringify(oidcUser))

    // Sync with userManager so oidc-client-ts picks it up
    userManager.getUser().then((u) => {
      user.value = u
    })
  }

  async function loginWithCredentials(email: string, password: string): Promise<void> {
    const request: SigninRequest = { email, password }
    const { data } = await http.post('/api/v1/auth/signin', request)
    storeCustomTokens(data.accessToken, data.refreshToken, data.expiresIn)
  }

  async function signupUser(request: SignupRequest): Promise<SignupResponse> {
    const { data } = await http.post<SignupResponse>('/api/v1/auth/signup', request)
    storeCustomTokens(data.accessToken, data.refreshToken, data.expiresIn)
    return data
  }

  return {
    user,
    isAuthenticated,
    accessToken,
    profile,
    tenantId,
    organizationId,
    initialize,
    login,
    handleCallback,
    logout,
    getAccessToken,
    storeCustomTokens,
    loginWithCredentials,
    signupUser,
  }
})
