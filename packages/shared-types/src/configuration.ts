// Configuration service DTOs
// Matches: services/configuration/Configuration.API/DTOs/

export interface UpdateConfigurationRequest {
  settings: Record<string, string>
}

export interface ToggleFeatureRequest {
  isEnabled: boolean
  description?: string
}

export interface ConfigurationResponse {
  organizationId: string
  settings: Record<string, string>
  lastUpdated: string | null
}

export interface FeatureFlagResponse {
  key: string
  description: string
  isEnabled: boolean
  updatedAt: string | null
}
