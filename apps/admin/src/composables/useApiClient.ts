import { createApiClient } from '@dc-platform/api-client'
import type { AxiosInstance } from 'axios'

let clientInstance: AxiosInstance | null = null

export function useApiClient(): AxiosInstance {
  if (!clientInstance) {
    const baseURL = (import.meta.env.VITE_API_BASE_URL as string | undefined) ?? 'http://localhost:5000'
    clientInstance = createApiClient({
      baseURL,
      getAccessToken: async () => {
        const storageKey = Object.keys(sessionStorage).find(k => k.startsWith('oidc.user:'))
        if (storageKey) {
          const userData = JSON.parse(sessionStorage.getItem(storageKey) || '{}')
          return userData.access_token || null
        }
        return null
      },
      getOrganizationId: () => {
        return sessionStorage.getItem('organizationId') || null
      },
      getWorkspaceId: () => {
        return sessionStorage.getItem('workspaceId') || null
      },
      onUnauthorized: () => {
        window.location.href = '/login'
      },
    })
  }
  return clientInstance
}
