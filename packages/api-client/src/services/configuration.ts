import type { AxiosInstance } from 'axios'
import type {
  ConfigurationResponse,
  UpdateConfigurationRequest,
  FeatureFlagResponse,
  ToggleFeatureRequest,
} from '@dc-platform/shared-types'

export async function getConfiguration(client: AxiosInstance, organizationId: string): Promise<ConfigurationResponse> {
  const { data } = await client.get(`/api/v1/config/${organizationId}`)
  return data
}

export async function updateConfiguration(client: AxiosInstance, organizationId: string, request: UpdateConfigurationRequest): Promise<ConfigurationResponse> {
  const { data } = await client.put(`/api/v1/config/${organizationId}`, request)
  return data
}

export async function getFeatureFlags(client: AxiosInstance, organizationId: string): Promise<FeatureFlagResponse[]> {
  const { data } = await client.get(`/api/v1/config/${organizationId}/features`)
  return data
}

export async function toggleFeature(client: AxiosInstance, organizationId: string, featureKey: string, request: ToggleFeatureRequest): Promise<FeatureFlagResponse> {
  const { data } = await client.put(`/api/v1/config/${organizationId}/features/${featureKey}`, request)
  return data
}
