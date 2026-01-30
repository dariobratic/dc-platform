import type { AxiosInstance } from 'axios'
import type {
  OrganizationResponse,
  CreateOrganizationRequest,
  UpdateOrganizationRequest,
  WorkspaceResponse,
  CreateWorkspaceRequest,
  UpdateWorkspaceRequest,
  MembershipResponse,
  AddMemberRequest,
  ChangeMemberRoleRequest,
  InvitationResponse,
  CreateInvitationRequest,
  AcceptInvitationRequest,
} from '@dc-platform/shared-types'

// Organizations

export async function getOrganization(client: AxiosInstance, id: string): Promise<OrganizationResponse> {
  const { data } = await client.get(`/api/v1/organizations/${id}`)
  return data
}

export async function createOrganization(client: AxiosInstance, request: CreateOrganizationRequest): Promise<OrganizationResponse> {
  const { data } = await client.post('/api/v1/organizations', request)
  return data
}

export async function updateOrganization(client: AxiosInstance, id: string, request: UpdateOrganizationRequest): Promise<OrganizationResponse> {
  const { data } = await client.put(`/api/v1/organizations/${id}`, request)
  return data
}

export async function deleteOrganization(client: AxiosInstance, id: string): Promise<void> {
  await client.delete(`/api/v1/organizations/${id}`)
}

// Workspaces

export async function getWorkspace(client: AxiosInstance, id: string): Promise<WorkspaceResponse> {
  const { data } = await client.get(`/api/v1/workspaces/${id}`)
  return data
}

export async function getWorkspacesByOrganization(client: AxiosInstance, organizationId: string): Promise<WorkspaceResponse[]> {
  const { data } = await client.get(`/api/v1/organizations/${organizationId}/workspaces`)
  return data
}

export async function createWorkspace(client: AxiosInstance, organizationId: string, request: CreateWorkspaceRequest): Promise<WorkspaceResponse> {
  const { data } = await client.post(`/api/v1/organizations/${organizationId}/workspaces`, request)
  return data
}

export async function updateWorkspace(client: AxiosInstance, id: string, request: UpdateWorkspaceRequest): Promise<WorkspaceResponse> {
  const { data } = await client.put(`/api/v1/workspaces/${id}`, request)
  return data
}

export async function deleteWorkspace(client: AxiosInstance, id: string): Promise<void> {
  await client.delete(`/api/v1/workspaces/${id}`)
}

// Memberships

export async function getWorkspaceMembers(client: AxiosInstance, workspaceId: string): Promise<MembershipResponse[]> {
  const { data } = await client.get(`/api/v1/workspaces/${workspaceId}/members`)
  return data
}

export async function getUserMemberships(client: AxiosInstance, userId: string): Promise<MembershipResponse[]> {
  const { data } = await client.get(`/api/v1/users/${userId}/memberships`)
  return data
}

export async function addMember(client: AxiosInstance, workspaceId: string, request: AddMemberRequest): Promise<MembershipResponse> {
  const { data } = await client.post(`/api/v1/workspaces/${workspaceId}/members`, request)
  return data
}

export async function changeMemberRole(client: AxiosInstance, workspaceId: string, userId: string, request: ChangeMemberRoleRequest): Promise<MembershipResponse> {
  const { data } = await client.put(`/api/v1/workspaces/${workspaceId}/members/${userId}`, request)
  return data
}

export async function removeMember(client: AxiosInstance, workspaceId: string, userId: string): Promise<void> {
  await client.delete(`/api/v1/workspaces/${workspaceId}/members/${userId}`)
}

// Invitations

export async function getInvitationByToken(client: AxiosInstance, token: string): Promise<InvitationResponse> {
  const { data } = await client.get(`/api/v1/invitations/${token}`)
  return data
}

export async function createInvitation(client: AxiosInstance, workspaceId: string, request: CreateInvitationRequest): Promise<InvitationResponse> {
  const { data } = await client.post(`/api/v1/workspaces/${workspaceId}/invitations`, request)
  return data
}

export async function acceptInvitation(client: AxiosInstance, token: string, request: AcceptInvitationRequest): Promise<MembershipResponse> {
  const { data } = await client.post(`/api/v1/invitations/${token}/accept`, request)
  return data
}

export async function revokeInvitation(client: AxiosInstance, id: string): Promise<void> {
  await client.delete(`/api/v1/invitations/${id}`)
}
