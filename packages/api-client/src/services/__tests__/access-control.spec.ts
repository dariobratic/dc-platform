import { describe, it, expect, vi } from 'vitest'
import {
  getRole,
  getRolesByScope,
  createRole,
  updateRole,
  deleteRole,
  assignRole,
  revokeRole,
  checkPermission,
  getUserPermissions,
} from '../access-control'

function createMockClient() {
  return {
    get: vi.fn().mockResolvedValue({ data: {} }),
    post: vi.fn().mockResolvedValue({ data: {} }),
    put: vi.fn().mockResolvedValue({ data: {} }),
    delete: vi.fn().mockResolvedValue({ data: undefined }),
  } as any
}

describe('access-control service', () => {
  it('getRole calls GET /api/v1/roles/:id', async () => {
    const client = createMockClient()
    await getRole(client, 'role-1')
    expect(client.get).toHaveBeenCalledWith('/api/v1/roles/role-1')
  })

  it('getRolesByScope calls GET /api/v1/roles with params', async () => {
    const client = createMockClient()
    await getRolesByScope(client, 'org-1', 'Organization')
    expect(client.get).toHaveBeenCalledWith('/api/v1/roles', {
      params: { scopeId: 'org-1', scopeType: 'Organization' },
    })
  })

  it('createRole calls POST /api/v1/roles', async () => {
    const client = createMockClient()
    const request = { name: 'Admin', description: '', scopeId: 'org-1', scopeType: 'Organization', permissions: [] }
    await createRole(client, request)
    expect(client.post).toHaveBeenCalledWith('/api/v1/roles', request)
  })

  it('updateRole calls PUT /api/v1/roles/:id', async () => {
    const client = createMockClient()
    const request = { name: 'Updated' }
    await updateRole(client, 'role-1', request)
    expect(client.put).toHaveBeenCalledWith('/api/v1/roles/role-1', request)
  })

  it('deleteRole calls DELETE /api/v1/roles/:id', async () => {
    const client = createMockClient()
    await deleteRole(client, 'role-1')
    expect(client.delete).toHaveBeenCalledWith('/api/v1/roles/role-1')
  })

  it('assignRole calls POST /api/v1/roles/:id/assignments', async () => {
    const client = createMockClient()
    const request = { userId: 'user-1', scopeId: 'org-1', scopeType: 'Organization', assignedBy: 'admin' }
    await assignRole(client, 'role-1', request)
    expect(client.post).toHaveBeenCalledWith('/api/v1/roles/role-1/assignments', request)
  })

  it('revokeRole calls DELETE /api/v1/roles/:id/assignments', async () => {
    const client = createMockClient()
    const request = { userId: 'user-1', scopeId: 'org-1' }
    await revokeRole(client, 'role-1', request)
    expect(client.delete).toHaveBeenCalledWith('/api/v1/roles/role-1/assignments', { data: request })
  })

  it('checkPermission calls GET /api/v1/permissions/check', async () => {
    const client = createMockClient()
    await checkPermission(client, 'user-1', 'org-1', 'document:write')
    expect(client.get).toHaveBeenCalledWith('/api/v1/permissions/check', {
      params: { userId: 'user-1', scopeId: 'org-1', permission: 'document:write' },
    })
  })

  it('getUserPermissions calls GET /api/v1/users/:id/permissions', async () => {
    const client = createMockClient()
    await getUserPermissions(client, 'user-1', 'org-1')
    expect(client.get).toHaveBeenCalledWith('/api/v1/users/user-1/permissions', {
      params: { scopeId: 'org-1' },
    })
  })
})
