import type { AxiosInstance } from 'axios'
import type {
  DashboardResponse,
  SystemHealthResponse,
  OrganizationSummary,
  AuditEntrySummary,
} from '@dc-platform/shared-types'

export async function getDashboard(client: AxiosInstance): Promise<DashboardResponse> {
  const { data } = await client.get('/api/v1/admin/dashboard')
  return data
}

export async function getSystemHealth(client: AxiosInstance): Promise<SystemHealthResponse> {
  const { data } = await client.get('/api/v1/admin/system/health')
  return data
}

export async function getOrganizations(client: AxiosInstance): Promise<OrganizationSummary[]> {
  const { data } = await client.get('/api/v1/admin/organizations')
  return data
}

export async function getRecentAuditEntries(client: AxiosInstance, count = 20): Promise<AuditEntrySummary[]> {
  const { data } = await client.get('/api/v1/admin/audit/recent', {
    params: { count },
  })
  return data
}
