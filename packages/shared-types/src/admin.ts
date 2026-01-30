// Admin API DTOs
// Matches: services/admin-api/AdminApi.API/DTOs/

export interface DashboardResponse {
  organizationCount: number
  auditEntryCount: number
  generatedAt: string
}

export interface SystemHealthResponse {
  overallStatus: string
  services: ServiceHealthStatus[]
  checkedAt: string
}

export interface ServiceHealthStatus {
  serviceName: string
  status: string
  statusCode: number | null
  responseTimeMs: number | null
}

export interface OrganizationSummary {
  id: string
  name: string
  slug: string
  status: string
  createdAt: string
}

export interface AuditEntrySummary {
  id: string
  action: string
  entityType: string
  entityId: string | null
  userId: string | null
  userEmail: string | null
  serviceName: string | null
  timestamp: string
}
