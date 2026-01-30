import { describe, it, expect, vi } from 'vitest'
import { getAuditEntry, getAuditEntries, getEntityAuditHistory, createAuditEntry } from '../audit'

function createMockClient() {
  return {
    get: vi.fn().mockResolvedValue({ data: {} }),
    post: vi.fn().mockResolvedValue({ data: {} }),
  } as any
}

describe('audit service', () => {
  it('getAuditEntry calls GET /api/v1/audit/:id', async () => {
    const client = createMockClient()
    await getAuditEntry(client, 'entry-1')
    expect(client.get).toHaveBeenCalledWith('/api/v1/audit/entry-1')
  })

  it('getAuditEntries calls GET /api/v1/audit with params', async () => {
    const client = createMockClient()
    const params = { organizationId: 'org-1', take: 50 }
    await getAuditEntries(client, params)
    expect(client.get).toHaveBeenCalledWith('/api/v1/audit', { params })
  })

  it('getAuditEntries works with empty params', async () => {
    const client = createMockClient()
    await getAuditEntries(client)
    expect(client.get).toHaveBeenCalledWith('/api/v1/audit', { params: {} })
  })

  it('getEntityAuditHistory calls GET /api/v1/audit/entity/:type/:id', async () => {
    const client = createMockClient()
    await getEntityAuditHistory(client, 'Organization', 'org-1')
    expect(client.get).toHaveBeenCalledWith('/api/v1/audit/entity/Organization/org-1')
  })

  it('createAuditEntry calls POST /api/v1/audit', async () => {
    const client = createMockClient()
    const request = { userId: 'user-1', action: 'org.created', entityType: 'Organization', entityId: 'org-1', serviceName: 'Directory' }
    await createAuditEntry(client, request)
    expect(client.post).toHaveBeenCalledWith('/api/v1/audit', request)
  })
})
