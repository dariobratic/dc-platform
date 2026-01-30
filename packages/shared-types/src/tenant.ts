// Tenant context types — matches Directory service domain entities
// These are the CORE types used throughout the platform

export type OrganizationStatus = 'Active' | 'Suspended' | 'Deleted'
export type WorkspaceStatus = 'Active' | 'Suspended' | 'Deleted'
export type WorkspaceRole = 'Owner' | 'Admin' | 'Member' | 'Viewer'
export type InvitationStatus = 'Pending' | 'Accepted' | 'Expired' | 'Revoked'

export interface Organization {
  id: string
  name: string
  slug: string
  status: OrganizationStatus
  settings: Record<string, string>
  createdAt: string
  updatedAt: string | null
}

export interface Workspace {
  id: string
  organizationId: string
  name: string
  slug: string
  status: WorkspaceStatus
  createdAt: string
  updatedAt: string | null
}

export interface Membership {
  id: string
  workspaceId: string
  userId: string
  role: WorkspaceRole
  joinedAt: string
  invitedBy: string | null
}

export interface Invitation {
  id: string
  workspaceId: string
  email: string
  role: WorkspaceRole
  token: string
  expiresAt: string
  status: InvitationStatus
  createdAt: string
  acceptedAt: string | null
  invitedBy: string
}

// Frontend-only tenant context
export interface TenantContext {
  organizationId: string | null
  workspaceId: string | null
}
