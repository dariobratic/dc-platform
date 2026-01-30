# Admin API Service

Aggregation and orchestration layer for the DC Platform admin dashboard. This service provides unified APIs that combine data from multiple downstream microservices.

## Overview

Admin API is a stateless aggregation layer with NO database. It uses typed HttpClients to call downstream services (Directory, Audit, etc.) and combines their responses into admin-friendly views.

**Key characteristics:**
- No database or persistence layer
- Read-only operations (GET endpoints only)
- Parallel service calls using `Task.WhenAll()`
- Graceful degradation when downstream services fail
- Structured logging with correlation IDs

## Quick Start

### Prerequisites
- .NET 10 SDK
- Running instances of downstream services (Directory, Audit, etc.)

### Run Locally

```bash
cd services/admin-api
dotnet restore
dotnet build
dotnet run --project AdminApi.API
```

Service runs on:
- HTTP: http://localhost:5007
- HTTPS: https://localhost:7007

### Verify Health

```bash
curl http://localhost:5007/health
```

Expected response:
```json
{
  "serviceName": "AdminApi",
  "status": "Healthy",
  "timestamp": "2026-01-30T12:00:00Z"
}
```

## API Documentation

### 1. Get Admin Dashboard

Aggregates key metrics from multiple services.

```bash
curl http://localhost:5007/api/v1/admin/dashboard
```

**Response:**
```json
{
  "organizationCount": 5,
  "auditEntryCount": 1234,
  "generatedAt": "2026-01-30T12:00:00Z"
}
```

**Implementation:** Calls Directory and Audit services in parallel using `Task.WhenAll()`.

### 2. Get System Health

Checks health status of all platform services.

```bash
curl http://localhost:5007/api/v1/admin/system/health
```

**Response:**
```json
{
  "overallStatus": "Healthy",
  "services": [
    {
      "serviceName": "Gateway",
      "status": "Healthy",
      "statusCode": 200,
      "responseTimeMs": null
    },
    {
      "serviceName": "Directory",
      "status": "Healthy",
      "statusCode": 200,
      "responseTimeMs": null
    },
    {
      "serviceName": "Audit",
      "status": "Unreachable",
      "statusCode": null,
      "responseTimeMs": null
    }
  ],
  "checkedAt": "2026-01-30T12:00:00Z"
}
```

**Statuses:**
- `Healthy`: Service responded with 2xx
- `Unhealthy`: Service responded with non-2xx
- `Unreachable`: Service did not respond (timeout or network error)

### 3. Get All Organizations

Fetches organization list from Directory service.

```bash
curl http://localhost:5007/api/v1/admin/organizations
```

**Response:**
```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "name": "Acme Corp",
    "slug": "acme-corp",
    "status": "Active",
    "createdAt": "2026-01-15T10:30:00Z"
  }
]
```

### 4. Get Recent Audit Entries

Fetches recent audit log entries from Audit service.

```bash
curl "http://localhost:5007/api/v1/admin/audit/recent?count=20"
```

**Query Parameters:**
- `count` (optional, default: 20): Number of recent entries to fetch

**Response:**
```json
[
  {
    "id": "789e4567-e89b-12d3-a456-426614174000",
    "action": "Organization.Created",
    "entityType": "Organization",
    "entityId": "123e4567-e89b-12d3-a456-426614174000",
    "userId": "456e4567-e89b-12d3-a456-426614174000",
    "userEmail": "admin@acme.com",
    "serviceName": "Directory",
    "timestamp": "2026-01-30T11:45:00Z"
  }
]
```

### 5. Health Check

Local health check for Admin API itself.

```bash
curl http://localhost:5007/api/v1/admin/health
```

**Response:**
```json
{
  "serviceName": "AdminApi",
  "status": "Healthy",
  "timestamp": "2026-01-30T12:00:00Z"
}
```

## Architecture

### Typed HttpClient Pattern

Admin API uses ASP.NET Core's typed HttpClient for strongly-typed service communication:

```
AdminController
    ├── IDirectoryServiceClient (injected)
    │   └── DirectoryServiceClient
    │       └── HttpClient (baseAddress: http://localhost:5001)
    │
    ├── IAuditServiceClient (injected)
    │   └── AuditServiceClient
    │       └── HttpClient (baseAddress: http://localhost:5004)
    │
    └── IServiceHealthChecker (injected)
        └── ServiceHealthChecker
            └── IHttpClientFactory (creates clients on-demand)
```

### Service Dependencies

```
Admin API (5007)
    │
    ├─> Gateway (5000)           [Health Check]
    ├─> Directory (5001)         [Organizations, Counts]
    ├─> Authentication (5002)    [Health Check]
    ├─> Access Control (5003)    [Health Check]
    ├─> Audit (5004)             [Audit Entries, Counts]
    ├─> Notification (5005)      [Health Check]
    └─> Configuration (5006)     [Health Check]
```

### Request Flow Example

```
1. Admin Dashboard Request
   └─> GET /api/v1/admin/dashboard
       │
       ├─> [Parallel] GET Directory/api/v1/organizations (count)
       │   └─> Returns: 5 organizations
       │
       ├─> [Parallel] GET Audit/api/v1/audit?pageSize=1 (total count)
       │   └─> Returns: TotalCount: 1234
       │
       └─> Combine results
           └─> Return: { organizationCount: 5, auditEntryCount: 1234 }
```

### Graceful Degradation

If Directory service is down:
```csharp
// DirectoryServiceClient.GetOrganizationCountAsync()
try {
    var response = await _httpClient.GetAsync("/api/v1/organizations");
    // ...
} catch (HttpRequestException ex) {
    _logger.LogWarning(ex, "Failed to fetch organizations");
    return []; // Return empty list instead of throwing
}
```

Dashboard still returns partial data:
```json
{
  "organizationCount": 0,        // Degraded: Directory unavailable
  "auditEntryCount": 1234,       // Healthy: Audit service OK
  "generatedAt": "2026-01-30T12:00:00Z"
}
```

## Configuration

### Service URLs (appsettings.json)

```json
{
  "ServiceUrls": {
    "Directory": "http://localhost:5001",
    "Authentication": "http://localhost:5002",
    "AccessControl": "http://localhost:5003",
    "Audit": "http://localhost:5004",
    "Notification": "http://localhost:5005",
    "Configuration": "http://localhost:5006"
  }
}
```

### Environment Variables (Production)

Override service URLs:
```bash
export ServiceUrls__Directory=https://directory.dcplatform.io
export ServiceUrls__Audit=https://audit.dcplatform.io
```

### Logging Configuration

Logs are written to:
- Console: Human-readable format with timestamps and correlation IDs
- File: `infrastructure/logs/admin-api/log-YYYYMMDD.json` (JSON format, 30-day retention)

Log levels:
- Development: Debug
- Production: Information

## Development

### Project Structure

```
services/admin-api/
├── AdminApi.API/               # Single-project API (no separate layers)
│   ├── Controllers/            # REST endpoints
│   ├── Models/                 # DTOs (records only)
│   ├── Services/               # Typed HttpClient implementations
│   ├── Middleware/             # Correlation ID, Exception handling
│   ├── Program.cs              # App configuration
│   ├── appsettings.json        # Service URLs
│   └── AdminApi.API.csproj     # NuGet packages
├── AdminApi.slnx               # Solution file
├── CLAUDE.md                   # Service scope and coding rules
└── README.md                   # This file
```

### Adding a New Downstream Service Client

1. Create interface and implementation in `Services/`:
   ```csharp
   public interface IConfigurationServiceClient
   {
       Task<List<FeatureFlagSummary>> GetFeatureFlagsAsync(CancellationToken ct);
   }

   public class ConfigurationServiceClient : IConfigurationServiceClient
   {
       private readonly HttpClient _httpClient;
       // Implementation with try-catch and graceful degradation
   }
   ```

2. Register typed HttpClient in `Program.cs`:
   ```csharp
   builder.Services.AddHttpClient<IConfigurationServiceClient, ConfigurationServiceClient>(client =>
   {
       client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Configuration"]!);
       client.Timeout = TimeSpan.FromSeconds(30);
   });
   ```

3. Inject into controller:
   ```csharp
   public class AdminController : ControllerBase
   {
       private readonly IConfigurationServiceClient _configClient;
       // Constructor injection
   }
   ```

### Testing

When unit tests are added:
```bash
dotnet test
```

Mock typed clients using NSubstitute:
```csharp
var mockDirectoryClient = Substitute.For<IDirectoryServiceClient>();
mockDirectoryClient.GetOrganizationCountAsync(Arg.Any<CancellationToken>())
    .Returns(Task.FromResult(5));
```

## Troubleshooting

### Service Returns Empty Data

Check downstream service health:
```bash
curl http://localhost:5007/api/v1/admin/system/health
```

If a service is "Unreachable", verify it's running:
```bash
curl http://localhost:5001/health  # Directory
curl http://localhost:5004/health  # Audit
```

### Logs Show HttpRequestException

Check `appsettings.json` service URLs. Ensure:
- URLs are correct (no typos)
- Ports match the downstream services
- Services are running and accessible

### Dashboard Returns Zero Counts

This is normal if:
- No organizations exist in Directory service
- No audit entries exist in Audit service

Verify with direct calls:
```bash
curl http://localhost:5001/api/v1/organizations
curl http://localhost:5004/api/v1/audit
```

## Port Assignment

- HTTP: 5007
- HTTPS: 7007

Follows DC Platform port convention:
- 5000: Gateway
- 5001: Directory
- 5002: Authentication
- 5003: Access Control
- 5004: Audit
- 5005: Notification
- 5006: Configuration
- 5007: Admin API

## Related Documentation

- [CLAUDE.md](./CLAUDE.md) - Service scope and coding rules
- [DC Platform Architecture](../../docs/architecture.md) - Overall platform design
- [Directory Service](../directory/README.md) - Organization data source
- [Audit Service](../audit/README.md) - Audit log data source
