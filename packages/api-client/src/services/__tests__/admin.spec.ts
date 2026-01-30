import { describe, it, expect, vi } from 'vitest'
import { getDashboard, getSystemHealth, getOrganizations, getRecentAuditEntries } from '../admin'

function createMockClient() {
  return {
    get: vi.fn().mockResolvedValue({ data: {} }),
  } as any
}

describe('admin service', () => {
  it('getDashboard calls GET /api/v1/admin/dashboard', async () => {
    const client = createMockClient()
    await getDashboard(client)
    expect(client.get).toHaveBeenCalledWith('/api/v1/admin/dashboard')
  })

  it('getSystemHealth calls GET /api/v1/admin/system/health', async () => {
    const client = createMockClient()
    await getSystemHealth(client)
    expect(client.get).toHaveBeenCalledWith('/api/v1/admin/system/health')
  })

  it('getOrganizations calls GET /api/v1/admin/organizations', async () => {
    const client = createMockClient()
    await getOrganizations(client)
    expect(client.get).toHaveBeenCalledWith('/api/v1/admin/organizations')
  })

  it('getRecentAuditEntries calls GET /api/v1/admin/audit/recent with count', async () => {
    const client = createMockClient()
    await getRecentAuditEntries(client, 10)
    expect(client.get).toHaveBeenCalledWith('/api/v1/admin/audit/recent', {
      params: { count: 10 },
    })
  })

  it('getRecentAuditEntries defaults to count 20', async () => {
    const client = createMockClient()
    await getRecentAuditEntries(client)
    expect(client.get).toHaveBeenCalledWith('/api/v1/admin/audit/recent', {
      params: { count: 20 },
    })
  })
})
