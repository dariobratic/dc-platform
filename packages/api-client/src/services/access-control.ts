import type { AxiosInstance } from 'axios'
import type {
  RoleResponse,
  CreateRoleRequest,
  UpdateRoleRequest,
  RoleAssignmentResponse,
  AssignRoleRequest,
  RevokeRoleRequest,
  PermissionCheckResponse,
  ScopeType,
} from '@dc-platform/shared-types'

// Roles

export async function getRole(client: AxiosInstance, id: string): Promise<RoleResponse> {
  const { data } = await client.get(`/api/v1/roles/${id}`)
  return data
}

export async function getRolesByScope(client: AxiosInstance, scopeId: string, scopeType: ScopeType): Promise<RoleResponse[]> {
  const { data } = await client.get('/api/v1/roles', {
    params: { scopeId, scopeType },
  })
  return data
}

export async function createRole(client: AxiosInstance, request: CreateRoleRequest): Promise<RoleResponse> {
  const { data } = await client.post('/api/v1/roles', request)
  return data
}

export async function updateRole(client: AxiosInstance, id: string, request: UpdateRoleRequest): Promise<RoleResponse> {
  const { data } = await client.put(`/api/v1/roles/${id}`, request)
  return data
}

export async function deleteRole(client: AxiosInstance, id: string): Promise<void> {
  await client.delete(`/api/v1/roles/${id}`)
}

// Role Assignments

export async function assignRole(client: AxiosInstance, roleId: string, request: AssignRoleRequest): Promise<RoleAssignmentResponse> {
  const { data } = await client.post(`/api/v1/roles/${roleId}/assignments`, request)
  return data
}

export async function revokeRole(client: AxiosInstance, roleId: string, request: RevokeRoleRequest): Promise<void> {
  await client.delete(`/api/v1/roles/${roleId}/assignments`, { data: request })
}

// Permissions

export async function checkPermission(client: AxiosInstance, userId: string, scopeId: string, permission: string): Promise<PermissionCheckResponse> {
  const { data } = await client.get('/api/v1/permissions/check', {
    params: { userId, scopeId, permission },
  })
  return data
}

export async function getUserPermissions(client: AxiosInstance, userId: string, scopeId: string): Promise<string[]> {
  const { data } = await client.get(`/api/v1/users/${userId}/permissions`, {
    params: { scopeId },
  })
  return data
}
