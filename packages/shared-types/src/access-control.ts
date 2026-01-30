// Access Control service DTOs
// Matches: services/access-control/src/AccessControl.API/DTOs/

export type ScopeType = 'Organization' | 'Workspace'

// --- Role ---

export interface CreateRoleRequest {
  name: string
  description?: string
  scopeId: string
  scopeType: ScopeType
  permissions: string[]
}

export interface UpdateRoleRequest {
  name: string
  description?: string
  permissions: string[]
}

export interface RoleResponse {
  id: string
  name: string
  description: string | null
  scopeId: string
  scopeType: ScopeType
  isSystem: boolean
  permissions: string[]
  createdAt: string
  updatedAt: string | null
}

// --- Role Assignment ---

export interface AssignRoleRequest {
  userId: string
  scopeId: string
  scopeType: ScopeType
  assignedBy: string
}

export interface RevokeRoleRequest {
  userId: string
  scopeId: string
}

export interface RoleAssignmentResponse {
  id: string
  roleId: string
  roleName: string
  userId: string
  scopeId: string
  scopeType: ScopeType
  assignedAt: string
  assignedBy: string
}

// --- Permission Check ---

export interface PermissionCheckResponse {
  hasPermission: boolean
  userId: string
  scopeId: string
  permission: string
}
