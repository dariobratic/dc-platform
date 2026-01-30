import { describe, it, expect, vi, beforeEach } from 'vitest'

vi.mock('@dc-platform/api-client', () => ({
  createApiClient: vi.fn().mockReturnValue({ defaults: { baseURL: 'http://localhost:5000' } }),
}))

describe('useApiClient', () => {
  beforeEach(() => {
    vi.resetModules()
    sessionStorage.clear()
  })

  it('returns an axios instance', async () => {
    const { useApiClient } = await import('../useApiClient')
    const client = useApiClient()
    expect(client).toBeDefined()
    expect(client.defaults.baseURL).toBe('http://localhost:5000')
  })

  it('returns the same instance on subsequent calls', async () => {
    const { useApiClient } = await import('../useApiClient')
    const client1 = useApiClient()
    const client2 = useApiClient()
    expect(client1).toBe(client2)
  })
})
