# Audit Service

**COMPLIANCE-CRITICAL**: Immutable audit log service for DC Platform. Audit entries MUST NOT be modified or deleted.

## Service Scope

### This Service IS Responsible For:
- Recording audit entries for all platform actions (creates, updates, deletes, access)
- Querying audit history by various filters (organization, workspace, user, entity, time range)
- Providing entity-specific audit trails
- Preserving immutable audit records for compliance

### This Service IS NOT Responsible For:
- User authentication (→ Authentication Service)
- Authorization decisions (→ Access Control Service)
- Real-time event streaming (this is historical record storage)
- Data retention policy enforcement (handled at infrastructure level)

### CRITICAL CONSTRAINTS:
- **IMMUTABILITY**: Audit entries are IMMUTABLE. Once created, they CANNOT be updated or deleted.
- **NO UPDATE/DELETE**: The domain entity, repository, and API have NO update or delete operations.
- **WRITE-ONLY DOMAIN**: This is an append-only log. Updates violate compliance requirements.

## Domain Model

```
AuditEntry (Immutable Aggregate Root)
├── Id: Guid
├── Timestamp: DateTime (UTC, set on creation)
├── UserId: Guid (who performed the action)
├── UserEmail: string? (denormalized for convenience)
├── Action: string (e.g., "organization.created", "role.assigned")
├── EntityType: string (e.g., "Organization", "Workspace", "Role")
├── EntityId: Guid (ID of the affected entity)
├── OrganizationId: Guid? (tenant context)
├── WorkspaceId: Guid? (workspace context)
├── Details: string? (JSON with before/after state)
├── IpAddress: string?
├── UserAgent: string?
├── ServiceName: string (e.g., "Directory", "AccessControl")
└── CorrelationId: string? (for distributed tracing)
```

### Domain Events
- `AuditEntryCreated` - raised when a new audit entry is created

## API Endpoints

### Create Audit Entry
```
POST /api/v1/audit
```
Request body:
```json
{
  "userId": "guid",
  "action": "organization.created",
  "entityType": "Organization",
  "entityId": "guid",
  "serviceName": "Directory",
  "userEmail": "user@example.com",
  "organizationId": "guid",
  "workspaceId": "guid",
  "details": "{\"before\":null,\"after\":{\"name\":\"Acme\"}}",
  "ipAddress": "192.168.1.1",
  "userAgent": "Mozilla/5.0",
  "correlationId": "trace-123"
}
```
Response: `201 Created` with `AuditEntryResponse`

### Get Audit Entry by ID
```
GET /api/v1/audit/{id}
```
Response: `200 OK` with `AuditEntryResponse`

### Query Audit Entries (Paginated)
```
GET /api/v1/audit?organizationId={guid}&workspaceId={guid}&userId={guid}&entityType={string}&action={string}&serviceName={string}&from={datetime}&to={datetime}&skip={int}&take={int}
```
Query parameters (all optional):
- `organizationId` - filter by organization
- `workspaceId` - filter by workspace
- `userId` - filter by user
- `entityType` - filter by entity type
- `action` - filter by action
- `serviceName` - filter by service
- `from` - filter by start timestamp (inclusive)
- `to` - filter by end timestamp (inclusive)
- `skip` - pagination offset (default: 0)
- `take` - page size (default: 50, max: 200)

Response: `200 OK` with `PagedResponse<AuditEntryResponse>`
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
```
GET /api/v1/audit/entity/{entityType}/{entityId}
```
Returns all audit entries for a specific entity, ordered by timestamp descending.

Response: `200 OK` with `List<AuditEntryResponse>`

## Project Structure

```
services/audit/
├── src/
│   ├── Audit.API/              # ASP.NET Core Web API
│   │   ├── Controllers/        # AuditController (NO update/delete endpoints)
│   │   ├── DTOs/               # CreateAuditEntryRequest
│   │   ├── Middleware/         # Exception handling
│   │   └── Program.cs
│   │
│   ├── Audit.Application/      # Business logic
│   │   ├── Commands/           # CreateAuditEntry (NO update/delete commands)
│   │   ├── Queries/            # GetById, GetPaged, GetEntityHistory
│   │   ├── Validators/         # FluentValidation
│   │   ├── Interfaces/         # IAuditEntryRepository
│   │   └── Responses/          # AuditEntryResponse, PagedResponse
│   │
│   ├── Audit.Domain/           # Core domain
│   │   ├── Entities/           # AuditEntry (immutable)
│   │   ├── Events/             # AuditEntryCreated
│   │   └── Exceptions/         # DomainException
│   │
│   └── Audit.Infrastructure/   # External concerns
│       ├── Persistence/        # AuditDbContext
│       ├── Configuration/      # AuditEntryConfiguration (EF Core)
│       └── Repositories/       # AuditEntryRepository
│
├── tests/
│   └── Audit.Domain.Tests/
│
├── Audit.slnx
├── CLAUDE.md                   # This file
└── README.md
```

## Technical Requirements

### Database
- PostgreSQL with schema: `audit`
- Table: `audit_entries`
- **NO UPDATE or DELETE operations** - append-only
- Performance indexes:
  - `OrganizationId`
  - `Timestamp`
  - `(EntityType, EntityId)`
  - `UserId`
  - `ServiceName`
  - `(OrganizationId, Timestamp)` - for tenant-scoped time-range queries

### Entity Configuration
- `Details` stored as `jsonb` in PostgreSQL for efficient querying
- All indexes to support common query patterns
- No soft delete pattern (entries are never deleted)

### Dependencies
- MediatR for CQRS
- FluentValidation for input validation
- Entity Framework Core for data access
- Npgsql for PostgreSQL

## Usage by Other Services

Other services should call this API to record audit events. Example:

```csharp
// In Directory service, after creating an organization:
await _auditClient.CreateAuditEntry(new CreateAuditEntryRequest(
    UserId: currentUserId,
    Action: "organization.created",
    EntityType: "Organization",
    EntityId: org.Id,
    ServiceName: "Directory",
    OrganizationId: org.Id,
    Details: JsonSerializer.Serialize(new { Name = org.Name, Slug = org.Slug }),
    IpAddress: httpContext.Connection.RemoteIpAddress?.ToString(),
    UserAgent: httpContext.Request.Headers["User-Agent"].ToString()
));
```

## Coding Rules for This Service

1. **IMMUTABILITY IS SACRED**: NEVER add Update or Delete operations to AuditEntry
2. **NO MODIFICATIONS**: Repository has NO UpdateAsync or DeleteAsync methods
3. **NO SOFT DELETE**: Audit entries are permanent records
4. **APPEND-ONLY**: Only AddAsync operation allowed
5. **Comprehensive Logging**: Log all audit creation attempts for debugging
6. **Performance**: Ensure indexes support fast queries across large datasets
7. **Data Retention**: Handled at infrastructure level (partitioning, archiving), not in application code

## Configuration

### Connection String
```json
{
  "ConnectionStrings": {
    "AuditDb": "Host=localhost;Port=5432;Database=dc_platform;Username=postgres;Password=postgres;Search Path=audit"
  }
}
```

### Port
- Development: `http://localhost:5004`

### Schema
- PostgreSQL schema: `audit`
- Migrations history table: `audit.__EFMigrationsHistory`

## Commands

```bash
# From services/audit/
dotnet restore
dotnet build
dotnet test

# Run API
dotnet run --project src/Audit.API

# Create migration (if schema changes)
dotnet ef migrations add InitialCreate --project src/Audit.Infrastructure --startup-project src/Audit.API

# Apply migrations
dotnet ef database update --project src/Audit.Infrastructure --startup-project src/Audit.API
```

## What NOT to Do

- **NEVER** add Update or Delete methods to AuditEntry entity
- **NEVER** add UpdateAsync or DeleteAsync to IAuditEntryRepository
- **NEVER** add PUT or DELETE endpoints to AuditController
- **NEVER** modify existing audit entries (even to fix errors - create compensating entry instead)
- **NEVER** implement soft delete pattern (audit entries are permanent)
- **NEVER** store sensitive data in Details field (passwords, tokens, etc.)
- **NEVER** use this service for real-time event streaming (it's for historical records)

## Compliance Notes

- Audit entries are immutable to meet compliance requirements (GDPR, SOC2, etc.)
- Retention policies are enforced at database level (partitioning, archiving)
- If an audit entry is incorrect, create a compensating entry explaining the error
- Access to audit logs should be restricted to authorized personnel only
- Regular audit log exports should be performed for external compliance systems
