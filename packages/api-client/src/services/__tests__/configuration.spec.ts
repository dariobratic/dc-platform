import { describe, it, expect, vi } from 'vitest'
import { getConfiguration, updateConfiguration, getFeatureFlags, toggleFeature } from '../configuration'

function createMockClient() {
  return {
    get: vi.fn().mockResolvedValue({ data: {} }),
    put: vi.fn().mockResolvedValue({ data: {} }),
  } as any
}

describe('configuration service', () => {
  it('getConfiguration calls GET /api/v1/config/:orgId', async () => {
    const client = createMockClient()
    await getConfiguration(client, 'org-1')
    expect(client.get).toHaveBeenCalledWith('/api/v1/config/org-1')
  })

  it('updateConfiguration calls PUT /api/v1/config/:orgId', async () => {
    const client = createMockClient()
    const request = { settings: { theme: 'dark' } }
    await updateConfiguration(client, 'org-1', request)
    expect(client.put).toHaveBeenCalledWith('/api/v1/config/org-1', request)
  })

  it('getFeatureFlags calls GET /api/v1/config/:orgId/features', async () => {
    const client = createMockClient()
    await getFeatureFlags(client, 'org-1')
    expect(client.get).toHaveBeenCalledWith('/api/v1/config/org-1/features')
  })

  it('toggleFeature calls PUT /api/v1/config/:orgId/features/:key', async () => {
    const client = createMockClient()
    const request = { isEnabled: true, description: 'Enable beta' }
    await toggleFeature(client, 'org-1', 'beta-dashboard', request)
    expect(client.put).toHaveBeenCalledWith('/api/v1/config/org-1/features/beta-dashboard', request)
  })
})
