import type { AxiosInstance } from 'axios'
import type {
  AuditEntryResponse,
  CreateAuditEntryRequest,
  PagedResponse,
} from '@dc-platform/shared-types'

export interface AuditQueryParams {
  organizationId?: string
  workspaceId?: string
  userId?: string
  entityType?: string
  action?: string
  serviceName?: string
  from?: string
  to?: string
  skip?: number
  take?: number
}

export async function getAuditEntry(client: AxiosInstance, id: string): Promise<AuditEntryResponse> {
  const { data } = await client.get(`/api/v1/audit/${id}`)
  return data
}

export async function getAuditEntries(client: AxiosInstance, params: AuditQueryParams = {}): Promise<PagedResponse<AuditEntryResponse>> {
  const { data } = await client.get('/api/v1/audit', { params })
  return data
}

export async function getEntityAuditHistory(client: AxiosInstance, entityType: string, entityId: string): Promise<AuditEntryResponse[]> {
  const { data } = await client.get(`/api/v1/audit/entity/${entityType}/${entityId}`)
  return data
}

export async function createAuditEntry(client: AxiosInstance, request: CreateAuditEntryRequest): Promise<AuditEntryResponse> {
  const { data } = await client.post('/api/v1/audit', request)
  return data
}
