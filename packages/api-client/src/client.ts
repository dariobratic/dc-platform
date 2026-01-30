import axios from 'axios'
import type { AxiosInstance, InternalAxiosRequestConfig } from 'axios'

export interface ApiClientOptions {
  baseURL: string
  getAccessToken?: () => Promise<string | null>
  getOrganizationId?: () => string | null
  getWorkspaceId?: () => string | null
  onUnauthorized?: () => void
}

export function createApiClient(options: ApiClientOptions): AxiosInstance {
  const instance = axios.create({
    baseURL: options.baseURL,
    headers: { 'Content-Type': 'application/json' },
  })

  // Request interceptor: Bearer token
  instance.interceptors.request.use(async (config: InternalAxiosRequestConfig) => {
    if (options.getAccessToken) {
      const token = await options.getAccessToken()
      if (token) {
        config.headers.Authorization = `Bearer ${token}`
      }
    }
    return config
  })

  // Request interceptor: tenant headers
  instance.interceptors.request.use((config: InternalAxiosRequestConfig) => {
    const orgId = options.getOrganizationId?.()
    if (orgId) {
      config.headers['X-Organization-Id'] = orgId
    }
    const wsId = options.getWorkspaceId?.()
    if (wsId) {
      config.headers['X-Workspace-Id'] = wsId
    }
    return config
  })

  // Response interceptor: 401 handling
  instance.interceptors.response.use(
    (response) => response,
    (error) => {
      if (error.response?.status === 401) {
        options.onUnauthorized?.()
      }
      return Promise.reject(error)
    }
  )

  return instance
}
