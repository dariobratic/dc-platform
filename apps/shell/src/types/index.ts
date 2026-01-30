export interface Organization {
  id: string
  name: string
  slug: string
  createdAt: string
}

export interface Workspace {
  id: string
  organizationId: string
  name: string
  slug: string
  createdAt: string
}

export interface UserProfile {
  sub: string
  email: string
  name: string
  preferred_username: string
  tenant_id?: string
  organization_id?: string
}

export interface ApiError {
  message: string
  code?: string
  details?: Record<string, unknown>
}

export interface OrganizationsResponse {
  organizations: Organization[]
}
