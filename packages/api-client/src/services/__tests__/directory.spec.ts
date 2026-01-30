import { describe, it, expect, vi } from 'vitest'
import {
  getOrganization,
  createOrganization,
  updateOrganization,
  deleteOrganization,
  getWorkspace,
  getWorkspacesByOrganization,
  createWorkspace,
  updateWorkspace,
  deleteWorkspace,
  getWorkspaceMembers,
  getUserMemberships,
  addMember,
  changeMemberRole,
  removeMember,
  getInvitationByToken,
  createInvitation,
  acceptInvitation,
  revokeInvitation,
} from '../directory'

function createMockClient() {
  return {
    get: vi.fn().mockResolvedValue({ data: {} }),
    post: vi.fn().mockResolvedValue({ data: {} }),
    put: vi.fn().mockResolvedValue({ data: {} }),
    delete: vi.fn().mockResolvedValue({ data: undefined }),
  } as any
}

describe('directory service', () => {
  it('getOrganization calls GET /api/v1/organizations/:id', async () => {
    const client = createMockClient()
    client.get.mockResolvedValue({ data: { id: 'org-1', name: 'Test' } })
    const result = await getOrganization(client, 'org-1')
    expect(client.get).toHaveBeenCalledWith('/api/v1/organizations/org-1')
    expect(result).toEqual({ id: 'org-1', name: 'Test' })
  })

  it('createOrganization calls POST /api/v1/organizations', async () => {
    const client = createMockClient()
    const request = { name: 'New Org', slug: 'new-org' }
    await createOrganization(client, request)
    expect(client.post).toHaveBeenCalledWith('/api/v1/organizations', request)
  })

  it('updateOrganization calls PUT /api/v1/organizations/:id', async () => {
    const client = createMockClient()
    const request = { name: 'Updated' }
    await updateOrganization(client, 'org-1', request)
    expect(client.put).toHaveBeenCalledWith('/api/v1/organizations/org-1', request)
  })

  it('deleteOrganization calls DELETE /api/v1/organizations/:id', async () => {
    const client = createMockClient()
    await deleteOrganization(client, 'org-1')
    expect(client.delete).toHaveBeenCalledWith('/api/v1/organizations/org-1')
  })

  it('getWorkspace calls GET /api/v1/workspaces/:id', async () => {
    const client = createMockClient()
    await getWorkspace(client, 'ws-1')
    expect(client.get).toHaveBeenCalledWith('/api/v1/workspaces/ws-1')
  })

  it('getWorkspacesByOrganization calls GET /api/v1/organizations/:id/workspaces', async () => {
    const client = createMockClient()
    await getWorkspacesByOrganization(client, 'org-1')
    expect(client.get).toHaveBeenCalledWith('/api/v1/organizations/org-1/workspaces')
  })

  it('createWorkspace calls POST /api/v1/organizations/:id/workspaces', async () => {
    const client = createMockClient()
    const request = { name: 'WS', slug: 'ws' }
    await createWorkspace(client, 'org-1', request)
    expect(client.post).toHaveBeenCalledWith('/api/v1/organizations/org-1/workspaces', request)
  })

  it('updateWorkspace calls PUT /api/v1/workspaces/:id', async () => {
    const client = createMockClient()
    await updateWorkspace(client, 'ws-1', { name: 'Updated' })
    expect(client.put).toHaveBeenCalledWith('/api/v1/workspaces/ws-1', { name: 'Updated' })
  })

  it('deleteWorkspace calls DELETE /api/v1/workspaces/:id', async () => {
    const client = createMockClient()
    await deleteWorkspace(client, 'ws-1')
    expect(client.delete).toHaveBeenCalledWith('/api/v1/workspaces/ws-1')
  })

  it('getWorkspaceMembers calls GET /api/v1/workspaces/:id/members', async () => {
    const client = createMockClient()
    await getWorkspaceMembers(client, 'ws-1')
    expect(client.get).toHaveBeenCalledWith('/api/v1/workspaces/ws-1/members')
  })

  it('getUserMemberships calls GET /api/v1/users/:id/memberships', async () => {
    const client = createMockClient()
    await getUserMemberships(client, 'user-1')
    expect(client.get).toHaveBeenCalledWith('/api/v1/users/user-1/memberships')
  })

  it('addMember calls POST /api/v1/workspaces/:id/members', async () => {
    const client = createMockClient()
    const request = { userId: 'user-1', role: 'Member' }
    await addMember(client, 'ws-1', request)
    expect(client.post).toHaveBeenCalledWith('/api/v1/workspaces/ws-1/members', request)
  })

  it('changeMemberRole calls PUT /api/v1/workspaces/:wsId/members/:userId', async () => {
    const client = createMockClient()
    const request = { role: 'Admin' }
    await changeMemberRole(client, 'ws-1', 'user-1', request)
    expect(client.put).toHaveBeenCalledWith('/api/v1/workspaces/ws-1/members/user-1', request)
  })

  it('removeMember calls DELETE /api/v1/workspaces/:wsId/members/:userId', async () => {
    const client = createMockClient()
    await removeMember(client, 'ws-1', 'user-1')
    expect(client.delete).toHaveBeenCalledWith('/api/v1/workspaces/ws-1/members/user-1')
  })

  it('getInvitationByToken calls GET /api/v1/invitations/:token', async () => {
    const client = createMockClient()
    await getInvitationByToken(client, 'abc123')
    expect(client.get).toHaveBeenCalledWith('/api/v1/invitations/abc123')
  })

  it('createInvitation calls POST /api/v1/workspaces/:id/invitations', async () => {
    const client = createMockClient()
    const request = { email: 'user@example.com', role: 'Member' }
    await createInvitation(client, 'ws-1', request)
    expect(client.post).toHaveBeenCalledWith('/api/v1/workspaces/ws-1/invitations', request)
  })

  it('acceptInvitation calls POST /api/v1/invitations/:token/accept', async () => {
    const client = createMockClient()
    const request = { userId: 'user-1' }
    await acceptInvitation(client, 'abc123', request)
    expect(client.post).toHaveBeenCalledWith('/api/v1/invitations/abc123/accept', request)
  })

  it('revokeInvitation calls DELETE /api/v1/invitations/:id', async () => {
    const client = createMockClient()
    await revokeInvitation(client, 'inv-1')
    expect(client.delete).toHaveBeenCalledWith('/api/v1/invitations/inv-1')
  })
})
