import axios from 'axios'
import type { AxiosInstance, InternalAxiosRequestConfig } from 'axios'
import { useAuthStore } from '@/stores/auth'
import { useTenantStore } from '@/stores/tenant'

export function createHttpClient(): AxiosInstance {
  const client = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000',
    headers: {
      'Content-Type': 'application/json',
    },
    timeout: 30000,
  })

  client.interceptors.request.use(
    async (config: InternalAxiosRequestConfig) => {
      const authStore = useAuthStore()
      const token = await authStore.getAccessToken()

      if (token) {
        config.headers.Authorization = `Bearer ${token}`
      }

      return config
    },
    (error) => {
      return Promise.reject(error)
    }
  )

  client.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
      const tenantStore = useTenantStore()

      if (tenantStore.organizationId) {
        config.headers['X-Organization-Id'] = tenantStore.organizationId
      }

      if (tenantStore.workspaceId) {
        config.headers['X-Workspace-Id'] = tenantStore.workspaceId
      }

      return config
    },
    (error) => {
      return Promise.reject(error)
    }
  )

  client.interceptors.response.use(
    (response) => response,
    async (error) => {
      if (error.response?.status === 401) {
        const authStore = useAuthStore()
        console.error('Unauthorized request, redirecting to login')
        await authStore.logout()
      }
      return Promise.reject(error)
    }
  )

  return client
}

export const http = createHttpClient()
