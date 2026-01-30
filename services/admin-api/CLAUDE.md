# Admin API Service

Aggregation and orchestration layer for the admin dashboard. This service has NO database and calls downstream microservices via typed HttpClients.

## Service Scope

This service IS responsible for:
- Aggregating data from multiple downstream services for admin dashboard
- System health monitoring across all platform services
- Providing unified admin-facing APIs that combine data from Directory, Audit, and other services
- Request orchestration and parallel data fetching
- Graceful degradation when downstream services are unavailable

This service IS NOT responsible for:
- Direct database access (NO database, NO DbContext, NO EF Core)
- Business logic or data mutation (read-only aggregation)
- Storing any state or persisting data
- Authentication/authorization (delegated to downstream services)
- Tenant-specific data filtering (relies on downstream service filtering)

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/admin/dashboard` | Get aggregated dashboard metrics (org count, audit entry count) |
| GET | `/api/v1/admin/system/health` | Check health status of all platform services |
| GET | `/api/v1/admin/organizations` | Get list of all organizations from Directory service |
| GET | `/api/v1/admin/audit/recent?count=20` | Get recent audit entries from Audit service |
| GET | `/api/v1/admin/health` | Health check for Admin API service itself |
| GET | `/health` | ASP.NET Health Checks endpoint |

## Project Structure

```
AdminApi.API/
├── Controllers/
│   └── AdminController.cs           # Main admin endpoints
├── Models/
│   ├── DashboardResponse.cs         # Dashboard aggregated metrics
│   ├── SystemHealthResponse.cs      # System-wide health status
│   ├── OrganizationSummary.cs       # Organization DTO
│   ├── AuditEntrySummary.cs         # Audit entry DTO
│   └── HealthResponse.cs            # Health check response
├── Services/
│   ├── IDirectoryServiceClient.cs   # Directory service interface
│   ├── DirectoryServiceClient.cs    # Typed HttpClient for Directory
│   ├── IAuditServiceClient.cs       # Audit service interface
│   ├── AuditServiceClient.cs        # Typed HttpClient for Audit
│   ├── IServiceHealthChecker.cs     # Health check interface
│   └── ServiceHealthChecker.cs      # Multi-service health checker
├── Middleware/
│   ├── CorrelationIdMiddleware.cs   # Correlation ID propagation
│   └── ExceptionHandlingMiddleware.cs # Global exception handler
├── Program.cs                       # Application entry point
├── appsettings.json                 # Service URLs configuration
└── appsettings.Development.json     # Dev-specific settings
```

## Typed HttpClient Configuration

This service uses ASP.NET Core's typed HttpClient pattern for downstream service calls:

```csharp
// In Program.cs
builder.Services.AddHttpClient<IDirectoryServiceClient, DirectoryServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Directory"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IAuditServiceClient, AuditServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Audit"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

Benefits:
- Automatic HttpClient lifecycle management
- Built-in resilience and performance optimizations
- Strongly-typed service interfaces
- Easy to mock for testing

## Downstream Service Dependencies

| Service | Port | Used For |
|---------|------|----------|
| Gateway | 5000 | Health check only |
| Directory | 5001 | Organization data, membership counts |
| Authentication | 5002 | Health check only (future: user stats) |
| Access Control | 5003 | Health check only (future: permission stats) |
| Audit | 5004 | Audit log entries, event counts |
| Notification | 5005 | Health check only (future: notification stats) |
| Configuration | 5006 | Health check only (future: feature flag stats) |

## Graceful Degradation

All downstream service calls are wrapped in try-catch blocks. If a service is unavailable:
- Return empty collections (`[]`) instead of throwing exceptions
- Return 0 for counts
- Log warnings but continue processing
- Mark service as "Unreachable" in health checks

This ensures the admin dashboard remains partially functional even when some services are down.

## Logging Patterns

Structured logging with Serilog:
- Console output with template formatting
- JSON file logging in `infrastructure/logs/admin-api/`
- Correlation ID propagation via middleware
- Enrichment with user ID, organization ID, request path, HTTP method

Log entries include:
- `Service`: "AdminApi"
- `CorrelationId`: Unique request ID
- `RequestPath`: API endpoint called
- `RequestMethod`: HTTP method (GET, POST, etc.)
- `UserId`: Authenticated user (if available)
- `OrganizationId`: Tenant context (if available)

## Configuration

Downstream service URLs are configured in `appsettings.json`:

```json
"ServiceUrls": {
  "Directory": "http://localhost:5001",
  "Authentication": "http://localhost:5002",
  "AccessControl": "http://localhost:5003",
  "Audit": "http://localhost:5004",
  "Notification": "http://localhost:5005",
  "Configuration": "http://localhost:5006"
}
```

For production, override with environment variables:
```bash
ServiceUrls__Directory=https://directory.dcplatform.io
ServiceUrls__Audit=https://audit.dcplatform.io
```

## Commands

Build:
```bash
cd services/admin-api
dotnet build
```

Run:
```bash
cd services/admin-api
dotnet run --project AdminApi.API
```

Test (when tests are added):
```bash
cd services/admin-api
dotnet test
```

## Coding Rules

1. **No Database Access**: This service NEVER accesses a database directly. All data comes from HTTP calls to other services.

2. **Typed HttpClients Only**: Use `IDirectoryServiceClient`, `IAuditServiceClient`, etc. Never use raw `HttpClient` directly.

3. **Records for DTOs**: All response models must be `record` types for immutability and value semantics.

4. **Async/Await**: All I/O operations (HTTP calls) must be async with `CancellationToken` support.

5. **Parallel Fetching**: When multiple independent service calls are needed, use `Task.WhenAll()` for parallel execution:
   ```csharp
   var orgCountTask = _directoryClient.GetOrganizationCountAsync(cancellationToken);
   var auditCountTask = _auditClient.GetTotalCountAsync(cancellationToken);
   await Task.WhenAll(orgCountTask, auditCountTask);
   ```

6. **Graceful Degradation**: Always catch `HttpRequestException` and return empty/default values instead of propagating errors.

7. **Correlation ID**: All downstream HTTP calls should include the `X-Correlation-Id` header from the incoming request (future enhancement).

8. **Logging**: Log at entry point of each operation. Use structured logging with context (service name, entity IDs, counts).

9. **Constructor DI**: All dependencies injected via constructor. No service locator pattern.

10. **English Only**: All code, comments, and logs in English.

## Future Enhancements

- Add correlation ID forwarding to downstream services
- Implement circuit breaker pattern with Polly
- Add response caching for dashboard metrics
- Add more aggregated views (user stats, permission stats, notification stats)
- Implement server-sent events (SSE) for real-time admin dashboard updates
- Add admin-specific filtering and search capabilities
