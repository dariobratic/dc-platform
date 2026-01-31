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
      // Skip auth token for public auth endpoints
      const publicPaths = ['/api/v1/auth/signin', '/api/v1/auth/signup', '/api/v1/auth/token', '/api/v1/auth/refresh', '/api/v1/auth/logout']
      const isPublic = publicPaths.some(p => config.url?.includes(p))

      if (!isPublic) {
        const authStore = useAuthStore()
        const token = await authStore.getAccessToken()

        if (token) {
          config.headers.Authorization = `Bearer ${token}`
        }
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
      // Don't auto-logout on 401 from auth endpoints (invalid credentials is expected)
      const url = error.config?.url ?? ''
      const isAuthEndpoint = ['/api/v1/auth/signin', '/api/v1/auth/signup', '/api/v1/auth/token', '/api/v1/auth/refresh'].some(p => url.includes(p))

      if (error.response?.status === 401 && !isAuthEndpoint) {
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
