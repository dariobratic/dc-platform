// Audit service DTOs
// Matches: services/audit/src/Audit.API/DTOs/

export interface CreateAuditEntryRequest {
  userId: string
  action: string
  entityType: string
  entityId: string
  serviceName: string
  userEmail?: string
  organizationId?: string
  workspaceId?: string
  details?: string
  ipAddress?: string
  userAgent?: string
  correlationId?: string
}

export interface AuditEntryResponse {
  id: string
  timestamp: string
  userId: string
  userEmail: string | null
  action: string
  entityType: string
  entityId: string
  organizationId: string | null
  workspaceId: string | null
  details: string | null
  ipAddress: string | null
  userAgent: string | null
  serviceName: string
  correlationId: string | null
}

// Frontend query filter (not a backend DTO)
export interface AuditFilter {
  userId?: string
  action?: string
  entityType?: string
  entityId?: string
  serviceName?: string
  organizationId?: string
  workspaceId?: string
  from?: string
  to?: string
  skip?: number
  take?: number
}
