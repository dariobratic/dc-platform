import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createApiClient, type ApiClientOptions } from '../client'

describe('createApiClient', () => {
  const defaultOptions: ApiClientOptions = {
    baseURL: 'http://localhost:5000',
  }

  it('creates an axios instance with baseURL', () => {
    const client = createApiClient(defaultOptions)
    expect(client.defaults.baseURL).toBe('http://localhost:5000')
  })

  it('sets Content-Type header to application/json', () => {
    const client = createApiClient(defaultOptions)
    expect(client.defaults.headers['Content-Type']).toBe('application/json')
  })

  it('adds Authorization header when getAccessToken returns token', async () => {
    const getAccessToken = vi.fn().mockResolvedValue('test-token')
    const client = createApiClient({ ...defaultOptions, getAccessToken })

    // Execute request interceptors manually
    const config = { headers: {} } as any
    const interceptors = (client.interceptors.request as any).handlers
    for (const handler of interceptors) {
      if (handler && handler.fulfilled) {
        await handler.fulfilled(config)
      }
    }

    expect(config.headers.Authorization).toBe('Bearer test-token')
  })

  it('does not add Authorization header when token is null', async () => {
    const getAccessToken = vi.fn().mockResolvedValue(null)
    const client = createApiClient({ ...defaultOptions, getAccessToken })

    const config = { headers: {} } as any
    const interceptors = (client.interceptors.request as any).handlers
    for (const handler of interceptors) {
      if (handler && handler.fulfilled) {
        await handler.fulfilled(config)
      }
    }

    expect(config.headers.Authorization).toBeUndefined()
  })

  it('adds X-Organization-Id header when organizationId exists', async () => {
    const getOrganizationId = vi.fn().mockReturnValue('org-123')
    const client = createApiClient({ ...defaultOptions, getOrganizationId })

    const config = { headers: {} } as any
    const interceptors = (client.interceptors.request as any).handlers
    for (const handler of interceptors) {
      if (handler && handler.fulfilled) {
        await handler.fulfilled(config)
      }
    }

    expect(config.headers['X-Organization-Id']).toBe('org-123')
  })

  it('adds X-Workspace-Id header when workspaceId exists', async () => {
    const getWorkspaceId = vi.fn().mockReturnValue('ws-456')
    const client = createApiClient({ ...defaultOptions, getWorkspaceId })

    const config = { headers: {} } as any
    const interceptors = (client.interceptors.request as any).handlers
    for (const handler of interceptors) {
      if (handler && handler.fulfilled) {
        await handler.fulfilled(config)
      }
    }

    expect(config.headers['X-Workspace-Id']).toBe('ws-456')
  })

  it('calls onUnauthorized on 401 response', async () => {
    const onUnauthorized = vi.fn()
    const client = createApiClient({ ...defaultOptions, onUnauthorized })

    const interceptors = (client.interceptors.response as any).handlers
    const errorHandler = interceptors[0]?.rejected

    const error = { response: { status: 401 } }
    await expect(errorHandler(error)).rejects.toEqual(error)
    expect(onUnauthorized).toHaveBeenCalledOnce()
  })

  it('does not call onUnauthorized on non-401 errors', async () => {
    const onUnauthorized = vi.fn()
    const client = createApiClient({ ...defaultOptions, onUnauthorized })

    const interceptors = (client.interceptors.response as any).handlers
    const errorHandler = interceptors[0]?.rejected

    const error = { response: { status: 500 } }
    await expect(errorHandler(error)).rejects.toEqual(error)
    expect(onUnauthorized).not.toHaveBeenCalled()
  })
})
