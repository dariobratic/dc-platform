# Audit Service

Immutable audit log service for DC Platform. Records all significant actions across the platform for compliance, security, and debugging purposes.

## Features

- Immutable audit log (entries cannot be modified or deleted)
- Flexible querying by organization, workspace, user, entity, time range, and more
- Entity-specific audit history trails
- Distributed tracing support via correlation IDs
- Performance-optimized with strategic database indexes
- Pagination support for large result sets

## Getting Started

### Prerequisites

- .NET 10 SDK
- PostgreSQL 15+

### Configuration

Update `appsettings.json` with your database connection:

```json
{
  "ConnectionStrings": {
    "AuditDb": "Host=localhost;Port=5432;Database=dc_platform;Username=postgres;Password=postgres;Search Path=audit"
  }
}
```

### Setup

```bash
# Restore dependencies
dotnet restore

# Create database schema (first time only)
dotnet ef database update --project src/Audit.Infrastructure --startup-project src/Audit.API

# Run the service
dotnet run --project src/Audit.API
```

The API will be available at `http://localhost:5004`.

### Run Tests

```bash
dotnet test
```

## API Documentation

### Create Audit Entry

```http
POST /api/v1/audit
Content-Type: application/json

{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "action": "organization.created",
  "entityType": "Organization",
  "entityId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
  "serviceName": "Directory",
  "userEmail": "user@example.com",
  "organizationId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
  "details": "{\"name\":\"Acme Corporation\",\"slug\":\"acme\"}",
  "ipAddress": "192.168.1.100",
  "userAgent": "Mozilla/5.0",
  "correlationId": "trace-abc-123"
}
```

**Response:** `201 Created`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa8",
  "timestamp": "2026-01-29T10:30:00Z",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userEmail": "user@example.com",
  "action": "organization.created",
  "entityType": "Organization",
  "entityId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
  "organizationId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
  "workspaceId": null,
  "details": "{\"name\":\"Acme Corporation\",\"slug\":\"acme\"}",
  "ipAddress": "192.168.1.100",
  "userAgent": "Mozilla/5.0",
  "serviceName": "Directory",
  "correlationId": "trace-abc-123"
}
```

### Get Audit Entry by ID

```http
GET /api/v1/audit/{id}
```

**Response:** `200 OK` with audit entry details.

### Query Audit Entries

```http
GET /api/v1/audit?organizationId={guid}&userId={guid}&from={datetime}&to={datetime}&skip=0&take=50
```

**Query Parameters:**
- `organizationId` (optional) - Filter by organization
- `workspaceId` (optional) - Filter by workspace
- `userId` (optional) - Filter by user
- `entityType` (optional) - Filter by entity type (e.g., "Organization", "Workspace")
- `action` (optional) - Filter by action (e.g., "organization.created")
- `serviceName` (optional) - Filter by service (e.g., "Directory", "AccessControl")
- `from` (optional) - Start timestamp (ISO 8601 format)
- `to` (optional) - End timestamp (ISO 8601 format)
- `skip` (optional, default: 0) - Pagination offset
- `take` (optional, default: 50, max: 200) - Page size

**Response:** `200 OK`
```json
{
  "items": [...],
  "totalCount": 150,
  "skip": 0,
  "take": 50,
  "hasMore": true
}
```

### Get Entity Audit History

```http
GET /api/v1/audit/entity/{entityType}/{entityId}
```

Returns complete audit history for a specific entity, ordered by timestamp (newest first).

**Example:**
```http
GET /api/v1/audit/entity/Organization/3fa85f64-5717-4562-b3fc-2c963f66afa7
```

**Response:** `200 OK` with list of audit entries.

## Architecture

This service follows Clean Architecture:

- **Domain Layer**: Immutable `AuditEntry` entity with business rules
- **Application Layer**: CQRS commands/queries, validators, response DTOs
- **Infrastructure Layer**: EF Core, PostgreSQL, repository implementations
- **API Layer**: REST controllers, middleware, configuration

## Important Notes

- **Immutability**: Audit entries are immutable by design. There are no update or delete operations.
- **Compliance**: This service is critical for compliance (GDPR, SOC2, audit trails).
- **Performance**: Database indexes optimize common query patterns. For very large datasets, consider time-based partitioning.
- **Data Retention**: Implement retention policies at the infrastructure level (e.g., PostgreSQL partitioning, archiving old data).

## Common Actions

Here are common action naming conventions:

- `{entity}.created` - Entity was created
- `{entity}.updated` - Entity was updated
- `{entity}.deleted` - Entity was soft-deleted
- `{entity}.restored` - Entity was restored
- `role.assigned` - Role was assigned to a user
- `role.revoked` - Role was revoked from a user
- `permission.granted` - Permission was granted
- `permission.denied` - Permission check failed
- `login.success` - User logged in
- `login.failed` - Login attempt failed

## Integration Example

```csharp
// From another service (e.g., Directory service)
public async Task<Organization> CreateOrganization(CreateOrgRequest request)
{
    var org = Organization.Create(request.Name, request.Slug);
    await _orgRepository.AddAsync(org);

    // Record audit entry
    await _httpClient.PostAsJsonAsync("http://localhost:5004/api/v1/audit", new
    {
        userId = _currentUser.Id,
        action = "organization.created",
        entityType = "Organization",
        entityId = org.Id,
        serviceName = "Directory",
        userEmail = _currentUser.Email,
        organizationId = org.Id,
        details = JsonSerializer.Serialize(new { name = org.Name, slug = org.Slug }),
        ipAddress = _httpContext.Connection.RemoteIpAddress?.ToString(),
        userAgent = _httpContext.Request.Headers["User-Agent"].ToString(),
        correlationId = _httpContext.TraceIdentifier
    });

    return org;
}
```

## License

Proprietary - Digital Control Platform
