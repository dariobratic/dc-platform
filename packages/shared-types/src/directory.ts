// Directory service request/response DTOs
// Matches: services/directory/src/Directory.API/DTOs/

import type {
  Organization,
  Workspace,
  Membership,
  Invitation,
  WorkspaceRole,
} from './tenant'

// --- Organization ---

export interface CreateOrganizationRequest {
  name: string
  slug: string
  settings?: Record<string, string>
}

export interface UpdateOrganizationRequest {
  name: string
  settings?: Record<string, string>
}

export type OrganizationResponse = Organization

// --- Workspace ---

export interface CreateWorkspaceRequest {
  name: string
  slug: string
}

export interface UpdateWorkspaceRequest {
  name: string
}

export type WorkspaceResponse = Workspace

// --- Membership ---

export interface AddMemberRequest {
  userId: string
  role: WorkspaceRole
}

export interface ChangeMemberRoleRequest {
  role: WorkspaceRole
}

export type MembershipResponse = Membership

// --- Invitation ---

export interface CreateInvitationRequest {
  email: string
  role: WorkspaceRole
}

export interface AcceptInvitationRequest {
  userId: string
}

export type InvitationResponse = Invitation
