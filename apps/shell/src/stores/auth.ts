import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { User } from 'oidc-client-ts'
import { userManager } from '@/plugins/auth'
import type { UserProfile } from '@/types'

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
    await userManager.signoutRedirect()
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
  }
})
