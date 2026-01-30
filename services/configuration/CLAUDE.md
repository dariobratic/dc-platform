# Configuration Service

Configuration and feature flag management service for DC Platform.

## Service Scope

### This Service IS Responsible For:
- Storing and retrieving organization-specific configuration settings
- Managing feature flags per organization
- Providing key-value configuration storage
- Batch updates of configuration settings
- Toggling feature flags on/off

### This Service IS NOT Responsible For:
- User authentication (-> Authentication Service)
- User-level preferences (-> Directory Service)
- Application deployment configuration (-> Infrastructure)
- Cross-organization global settings (those go in appsettings.json)
- Authorization decisions (-> Access Control Service)

## Architecture

The Configuration service is a **single-project API** (similar to Gateway) with direct DbContext usage. It does not use full Clean Architecture patterns to keep it lightweight.

### Key Characteristics:
- **Simple architecture** - Single API project with DbContext
- **Tenant-scoped** - All settings and flags are per organization
- **Upsert behavior** - Settings are created if they don't exist, updated if they do
- **Key-value storage** - Simple dictionary-based configuration

## Domain Model

```
OrganizationSetting
├── Id: Guid
├── OrganizationId: Guid (tenant context)
├── Key: string (max 256 chars)
├── Value: string (max 4096 chars)
├── CreatedAt: DateTime
└── UpdatedAt: DateTime?

FeatureFlag
├── Id: Guid
├── OrganizationId: Guid (tenant context)
├── Key: string (max 256 chars)
├── Description: string (max 1024 chars)
├── IsEnabled: bool
├── CreatedAt: DateTime
└── UpdatedAt: DateTime?
```

### Database Constraints
- **OrganizationSetting**: Unique index on (OrganizationId, Key)
- **FeatureFlag**: Unique index on (OrganizationId, Key)

## API Endpoints

### Get Organization Configuration
```
GET /api/v1/config/{organizationId}
```
Returns all configuration settings for the organization as a dictionary.

Response: `200 OK`
```json
{
  "organizationId": "guid",
  "settings": {
    "theme": "dark",
    "timezone": "UTC",
    "maxUsers": "100"
  },
  "lastUpdated": "2026-01-30T12:00:00Z"
}
```

### Update Organization Configuration
```
PUT /api/v1/config/{organizationId}
```
Batch update configuration settings. Creates new keys, updates existing ones.

Request body:
```json
{
  "settings": {
    "theme": "light",
    "language": "en"
  }
}
```

Response: `200 OK` with updated `ConfigurationResponse`

**Behavior**:
- Existing keys are updated with new values
- New keys are created
- Keys not in the request remain unchanged
- Each change is logged

### Get Feature Flags
```
GET /api/v1/config/{organizationId}/features
```
Returns all feature flags for the organization, ordered by key.

Response: `200 OK`
```json
[
  {
    "key": "beta-dashboard",
    "description": "Enable new beta dashboard",
    "isEnabled": true,
    "updatedAt": "2026-01-30T12:00:00Z"
  },
  {
    "key": "ai-assistant",
    "description": "AI-powered assistant",
    "isEnabled": false,
    "updatedAt": null
  }
]
```

### Toggle Feature Flag
```
PUT /api/v1/config/{organizationId}/features/{featureKey}
```
Enable/disable a feature flag. Creates the flag if it doesn't exist.

Request body:
```json
{
  "isEnabled": true,
  "description": "Optional description"
}
```

Response: `200 OK`
```json
{
  "key": "beta-dashboard",
  "description": "Enable new beta dashboard",
  "isEnabled": true,
  "updatedAt": "2026-01-30T12:00:00Z"
}
```

**Behavior**:
- If the feature flag exists: updates `isEnabled` and optionally `description`
- If the feature flag doesn't exist: creates it with provided values
- Each change is logged

### Health Check
```
GET /api/v1/config/health
```
Response: `200 OK`
```json
{
  "serviceName": "Configuration",
  "status": "Healthy",
  "timestamp": "2026-01-30T12:00:00Z"
}
```

## Project Structure

```
services/configuration/
├── Configuration.API/
│   ├── Controllers/
│   │   └── ConfigurationController.cs
│   ├── DTOs/
│   │   ├── ConfigurationResponse.cs
│   │   ├── UpdateConfigurationRequest.cs
│   │   ├── FeatureFlagResponse.cs
│   │   ├── ToggleFeatureRequest.cs
│   │   └── HealthResponse.cs
│   ├── Entities/
│   │   ├── OrganizationSetting.cs
│   │   └── FeatureFlag.cs
│   ├── Middleware/
│   │   ├── CorrelationIdMiddleware.cs
│   │   └── ExceptionHandlingMiddleware.cs
│   ├── Persistence/
│   │   ├── ConfigurationDbContext.cs
│   │   └── Configurations/
│   │       ├── OrganizationSettingConfiguration.cs
│   │       └── FeatureFlagConfiguration.cs
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Configuration.API.csproj
│   └── Program.cs
│
├── Configuration.slnx
├── CLAUDE.md                   # This file
└── README.md
```

## Technical Requirements

### Database
- PostgreSQL with schema: `configuration`
- Tables: `organization_settings`, `feature_flags`
- Connection string: `Host=localhost;Port=5432;Database=dc_platform;Username=postgres;Password=postgres;Search Path=configuration`

### Multi-Tenancy
- All queries filter by `OrganizationId` (from route parameter)
- Unique constraints ensure no duplicate keys within an organization
- Different organizations can have the same setting keys with different values

### Dependencies
- Entity Framework Core for data access
- Npgsql for PostgreSQL
- Serilog for structured logging

## Business Rules

1. **Upsert Behavior**: PUT operations create if not exists, update if exists
2. **Tenant Isolation**: All settings and flags are scoped to organizationId
3. **No Deletion**: Settings and flags can be updated but not deleted (set empty value instead)
4. **Audit Logging**: All mutations (create/update) are logged with old and new values
5. **Case Sensitivity**: Keys are case-sensitive

## Usage by Other Services

Other services can call this API to retrieve configuration:

```csharp
// Get all org settings
var config = await _configClient.GetConfiguration(organizationId);
var theme = config.Settings.GetValueOrDefault("theme", "light");

// Check feature flag
var features = await _configClient.GetFeatures(organizationId);
var isEnabled = features.FirstOrDefault(f => f.Key == "beta-dashboard")?.IsEnabled ?? false;

// Toggle feature
await _configClient.ToggleFeature(organizationId, "ai-assistant", new ToggleFeatureRequest(
    IsEnabled: true,
    Description: "Enable AI assistant for this org"
));
```

## Configuration

### Connection String
```json
{
  "ConnectionStrings": {
    "ConfigurationDb": "Host=localhost;Port=5432;Database=dc_platform;Username=postgres;Password=postgres;Search Path=configuration"
  }
}
```

### Port
- HTTP: `5006`
- HTTPS: `7006`

### Schema
- PostgreSQL schema: `configuration`
- Migrations history table: `configuration.__EFMigrationsHistory`

## Logging

This service uses structured JSON logging via Serilog (see `.claude/skills/structured-logging/SKILL.md`).

- **Log output**: Console (structured text) + File (JSON)
- **File path**: `infrastructure/logs/configuration/log-{date}.json`
- **Rotation**: Daily, 30-day retention
- **Correlation ID**: All requests tagged via `X-Correlation-Id` header
- **Context enrichment**: RequestMethod, RequestPath, UserId, OrganizationId, WorkspaceId

### What to Log
- Setting create/update operations (key, old value, new value)
- Feature flag toggles (key, old state, new state)
- Batch update operations (number of settings updated)
- Errors and validation failures

### What NOT to Log
- Sensitive configuration values (passwords, tokens, API keys)
- Full configuration dumps
- User personal data

## Commands

```bash
# From services/configuration/
dotnet restore
dotnet build
dotnet run --project Configuration.API

# Create migration
dotnet ef migrations add InitialCreate --project Configuration.API --context ConfigurationDbContext

# Apply migrations
dotnet ef database update --project Configuration.API --context ConfigurationDbContext

# Run on specific port
dotnet run --project Configuration.API --urls "http://localhost:5006"
```

## Coding Rules for This Service

1. **Tenant Isolation**: ALWAYS filter by organizationId from route parameter
2. **Upsert Pattern**: Settings and flags are created if missing, updated if present
3. **Structured Logging**: Log all mutations with sufficient context
4. **Direct DbContext**: No repository pattern - keep it simple
5. **No Soft Delete**: Settings persist; use empty values if needed
6. **Validation**: Validate organizationId and key names
7. **No Secrets in Values**: Never store passwords or tokens in settings

## Future Features

1. **Setting Types**: Support typed settings (string, int, bool, json)
2. **Setting Validation**: Schema validation for setting values
3. **Feature Flag Rollout**: Gradual rollout percentages
4. **Caching Layer**: Redis caching for frequently accessed settings
5. **Change History**: Track full history of setting changes
6. **Default Values**: System-wide default values
7. **Setting Groups**: Organize settings into named groups
8. **Import/Export**: Bulk import/export of organization settings

## Development Notes

- Default port: `5006` (HTTP), `7006` (HTTPS)
- Health endpoint: `http://localhost:5006/api/v1/config/health`
- No authentication required yet (future: integrate with Authentication service)
- Settings dictionary can be empty (returns empty object)
- Feature flags list can be empty (returns empty array)
