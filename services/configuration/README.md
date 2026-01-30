# Configuration Service

Tenant-specific configuration and feature flag management for DC Platform.

## Overview

The Configuration service provides a centralized key-value store for organization-specific settings and feature flags. It enables dynamic configuration without code deployments and supports gradual feature rollouts via feature flags.

## Quick Start

### Prerequisites

- .NET 10 SDK
- PostgreSQL 16+
- Database `dc_platform` with schema `configuration`

### Setup

1. **Restore dependencies**
   ```bash
   dotnet restore
   ```

2. **Apply database migrations**
   ```bash
   dotnet ef database update --project Configuration.API
   ```

3. **Run the service**
   ```bash
   dotnet run --project Configuration.API
   ```

   The service will start on:
   - HTTP: `http://localhost:5006`
   - HTTPS: `https://localhost:7006`

4. **Verify health**
   ```bash
   curl http://localhost:5006/api/v1/config/health
   ```

## API Documentation

### Configuration Settings

#### Get All Settings
```bash
curl http://localhost:5006/api/v1/config/{organizationId}
```

**Response:**
```json
{
  "organizationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "settings": {
    "theme": "dark",
    "timezone": "America/New_York",
    "maxUsers": "50"
  },
  "lastUpdated": "2026-01-30T12:34:56Z"
}
```

#### Update Settings (Batch)
```bash
curl -X PUT http://localhost:5006/api/v1/config/{organizationId} \
  -H "Content-Type: application/json" \
  -d '{
    "settings": {
      "theme": "light",
      "language": "en",
      "notifications": "true"
    }
  }'
```

**Behavior:**
- Creates new keys if they don't exist
- Updates existing keys with new values
- Keys not in request remain unchanged
- Returns updated full configuration

### Feature Flags

#### Get All Feature Flags
```bash
curl http://localhost:5006/api/v1/config/{organizationId}/features
```

**Response:**
```json
[
  {
    "key": "beta-dashboard",
    "description": "New beta dashboard UI",
    "isEnabled": true,
    "updatedAt": "2026-01-30T12:34:56Z"
  },
  {
    "key": "ai-assistant",
    "description": "AI-powered virtual assistant",
    "isEnabled": false,
    "updatedAt": null
  }
]
```

#### Toggle Feature Flag
```bash
curl -X PUT http://localhost:5006/api/v1/config/{organizationId}/features/beta-dashboard \
  -H "Content-Type: application/json" \
  -d '{
    "isEnabled": true,
    "description": "Enable new beta dashboard for testing"
  }'
```

**Behavior:**
- Creates feature flag if it doesn't exist
- Updates `isEnabled` state if it exists
- Optionally updates description
- Returns updated feature flag

### Health Check
```bash
curl http://localhost:5006/api/v1/config/health
```

**Response:**
```json
{
  "serviceName": "Configuration",
  "status": "Healthy",
  "timestamp": "2026-01-30T12:34:56Z"
}
```

## Configuration

### Database Connection

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "ConfigurationDb": "Host=localhost;Port=5432;Database=dc_platform;Username=postgres;Password=postgres;Search Path=configuration"
  }
}
```

### Logging

Structured logging with Serilog:
- **Console**: Human-readable format with correlation IDs
- **File**: JSON logs in `infrastructure/logs/configuration/log-{date}.json`
- **Retention**: 30 days

## Architecture

### Single-Project Design

Unlike full Clean Architecture services (Directory, Audit, etc.), the Configuration service uses a simplified single-project structure:

```
Configuration.API/
├── Controllers/        # API endpoints
├── DTOs/              # Request/response models
├── Entities/          # EF Core entities
├── Middleware/        # Cross-cutting concerns
└── Persistence/       # DbContext and configurations
```

### Why Simple Architecture?

- **No complex business logic**: Simple CRUD operations
- **Lightweight domain**: Just key-value storage
- **Fast development**: Less boilerplate
- **Easy maintenance**: Single project to navigate

### Multi-Tenancy

All data is scoped by `organizationId`:
- Route parameter determines tenant context
- Unique constraints per organization
- No cross-tenant data exposure

## Common Use Cases

### Application Theming
```json
{
  "theme": "dark",
  "primaryColor": "#1976d2",
  "logo": "https://cdn.example.com/logo.png"
}
```

### Business Rules
```json
{
  "maxUsers": "100",
  "allowGuests": "true",
  "sessionTimeout": "3600"
}
```

### Feature Flags
```json
[
  { "key": "beta-features", "isEnabled": true },
  { "key": "analytics", "isEnabled": false },
  { "key": "export-pdf", "isEnabled": true }
]
```

### Integration Settings
```json
{
  "smtp.host": "smtp.example.com",
  "smtp.port": "587",
  "smtp.from": "noreply@example.com",
  "api.timeout": "30000"
}
```

## Development

### Running Tests
```bash
dotnet test
```

### Creating Migrations
```bash
dotnet ef migrations add <MigrationName> --project Configuration.API
```

### Applying Migrations
```bash
dotnet ef database update --project Configuration.API
```

### Reverting Migrations
```bash
dotnet ef database update <PreviousMigration> --project Configuration.API
```

## Error Handling

The service returns standard HTTP problem details for errors:

### 400 Bad Request
- Empty settings dictionary
- Invalid JSON format

### 404 Not Found
- Organization configuration not found (returns empty settings)

### 409 Conflict
- Duplicate key constraint violation

### 500 Internal Server Error
- Database connection failure
- Unexpected errors

**Example error response:**
```json
{
  "status": 400,
  "title": "Bad Request",
  "detail": "Settings dictionary cannot be empty."
}
```

## Best Practices

### Setting Keys
- Use dot notation for namespacing: `smtp.host`, `ui.theme`
- Lowercase with hyphens for feature flags: `beta-dashboard`
- Keep keys under 256 characters
- Document all keys used by your application

### Setting Values
- Store as strings (parse to correct type in application)
- Keep values under 4096 characters
- Never store secrets (use environment variables or secret management)
- Validate on read, not on write

### Feature Flags
- Use descriptive keys: `enable-ai-assistant`, not `flag1`
- Add meaningful descriptions
- Document flag purpose and rollout plan
- Clean up unused flags regularly

## Security Considerations

- **No authentication yet**: Add authentication middleware in production
- **No authorization**: Implement organization access control
- **No secret storage**: Never store passwords, API keys, or tokens
- **Audit logging**: All changes are logged with correlation IDs
- **Input validation**: Keys and values are length-constrained

## Integration Example

### .NET Client
```csharp
public class ConfigurationClient
{
    private readonly HttpClient _httpClient;
    private readonly Guid _organizationId;

    public async Task<Dictionary<string, string>> GetSettingsAsync()
    {
        var response = await _httpClient.GetAsync(
            $"api/v1/config/{_organizationId}");
        response.EnsureSuccessStatusCode();

        var config = await response.Content
            .ReadFromJsonAsync<ConfigurationResponse>();
        return config.Settings;
    }

    public async Task<bool> IsFeatureEnabledAsync(string featureKey)
    {
        var response = await _httpClient.GetAsync(
            $"api/v1/config/{_organizationId}/features");
        response.EnsureSuccessStatusCode();

        var features = await response.Content
            .ReadFromJsonAsync<List<FeatureFlagResponse>>();
        return features.FirstOrDefault(f => f.Key == featureKey)
            ?.IsEnabled ?? false;
    }
}
```

## Troubleshooting

### Service won't start
- Check PostgreSQL is running: `pg_isready -h localhost -p 5432`
- Verify database exists: `psql -U postgres -d dc_platform -c '\dn'`
- Check port 5006 is available: `netstat -an | grep 5006`

### Database errors
- Ensure schema exists: `CREATE SCHEMA IF NOT EXISTS configuration;`
- Run migrations: `dotnet ef database update --project Configuration.API`
- Check connection string in appsettings.json

### Logs not appearing
- Verify `infrastructure/logs/configuration/` directory exists
- Check file permissions
- Review Serilog configuration in appsettings.json

## License

Proprietary - Digital Control Platform

## Support

For issues or questions:
- Create an issue in the DC Platform repository
- Contact the platform team
- See `CLAUDE.md` for architectural guidance
