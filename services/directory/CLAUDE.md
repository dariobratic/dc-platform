# Directory Service

Organization, Workspace, and Membership management service for DC Platform.

## Service Scope

### This Service IS Responsible For:
- Organization CRUD (create, read, update, soft-delete)
- Workspace management within organizations
- User membership in workspaces (with roles)
- Invitation workflow (invite user to workspace)
- Organization and workspace metadata

### This Service IS NOT Responsible For:
- User authentication (→ Authentication Service)
- User identity/credentials (→ Keycloak)
- Permission evaluation (→ Access Control Service)
- Audit logging (→ Audit Service via events)
- Notifications (→ Notification Service via events)

## Domain Model

```
Organization (tenant boundary)
├── Id: Guid
├── Name: string
├── Slug: string (unique, URL-friendly)
├── Status: OrganizationStatus (Active, Suspended, Deleted)
├── Settings: OrganizationSettings (JSON)
├── CreatedAt, UpdatedAt, DeletedAt
│
└── Workspaces[]
    ├── Id: Guid
    ├── OrganizationId: Guid
    ├── Name: string
    ├── Slug: string (unique within org)
    ├── Status: WorkspaceStatus
    ├── CreatedAt, UpdatedAt, DeletedAt
    │
    └── Memberships[]
        ├── Id: Guid
        ├── WorkspaceId: Guid
        ├── UserId: Guid (external - from Keycloak)
        ├── Role: WorkspaceRole (Owner, Admin, Member, Viewer)
        ├── JoinedAt: DateTime
        └── InvitedBy: Guid?

Invitation
├── Id: Guid
├── WorkspaceId: Guid
├── Email: string
├── Role: WorkspaceRole
├── Token: string (unique)
├── ExpiresAt: DateTime
├── Status: InvitationStatus (Pending, Accepted, Expired, Revoked)
├── CreatedAt, AcceptedAt
└── InvitedBy: Guid
```

## API Endpoints

### Organizations
- `POST /api/v1/organizations` - Create organization
- `GET /api/v1/organizations/{id}` - Get organization
- `PUT /api/v1/organizations/{id}` - Update organization
- `DELETE /api/v1/organizations/{id}` - Soft delete

### Workspaces
- `POST /api/v1/organizations/{orgId}/workspaces` - Create workspace
- `GET /api/v1/organizations/{orgId}/workspaces` - List workspaces
- `GET /api/v1/workspaces/{id}` - Get workspace
- `PUT /api/v1/workspaces/{id}` - Update workspace
- `DELETE /api/v1/workspaces/{id}` - Soft delete

### Memberships
- `POST /api/v1/workspaces/{id}/members` - Add member
- `GET /api/v1/workspaces/{id}/members` - List members
- `PUT /api/v1/workspaces/{id}/members/{userId}` - Update role
- `DELETE /api/v1/workspaces/{id}/members/{userId}` - Remove member
- `GET /api/v1/users/{userId}/memberships` - User's memberships

### Invitations
- `POST /api/v1/workspaces/{id}/invitations` - Create invitation
- `GET /api/v1/invitations/{token}` - Get invitation by token
- `POST /api/v1/invitations/{token}/accept` - Accept invitation
- `DELETE /api/v1/invitations/{id}` - Revoke invitation

## Project Structure

```
services/directory/
├── src/
│   ├── Directory.API/           # ASP.NET Core Web API
│   │   ├── Controllers/         # API controllers
│   │   ├── DTOs/                # Request/Response models
│   │   ├── Middleware/          # Exception handling, tenant context
│   │   └── Program.cs
│   │
│   ├── Directory.Application/   # Business logic
│   │   ├── Services/            # Application services
│   │   ├── Interfaces/          # Abstractions
│   │   ├── Commands/            # Write operations (MediatR)
│   │   ├── Queries/             # Read operations (MediatR)
│   │   └── Validators/          # FluentValidation
│   │
│   ├── Directory.Domain/        # Core domain
│   │   ├── Entities/            # Organization, Workspace, Membership
│   │   ├── ValueObjects/        # Slug, OrganizationSettings
│   │   ├── Events/              # Domain events
│   │   └── Exceptions/          # Domain exceptions
│   │
│   └── Directory.Infrastructure/# External concerns
│       ├── Persistence/         # DbContext, migrations
│       ├── Repositories/        # Repository implementations
│       └── Configuration/       # Entity configurations
│
├── tests/
│   ├── Directory.API.Tests/
│   ├── Directory.Application.Tests/
│   └── Directory.Domain.Tests/
│
├── Directory.slnx
├── CLAUDE.md                    # This file
└── README.md
```

## Technical Requirements

### Database
- PostgreSQL with schema: `directory`
- All entities have `TenantId` (= OrganizationId for tenant isolation)
- Soft delete pattern: `DeletedAt` timestamp, never hard delete
- Unique constraints: `(Slug)` for org, `(OrganizationId, Slug)` for workspace

### Events (publish to message bus)
- `OrganizationCreated`, `OrganizationUpdated`, `OrganizationDeleted`
- `WorkspaceCreated`, `WorkspaceUpdated`, `WorkspaceDeleted`
- `MemberAdded`, `MemberRemoved`, `MemberRoleChanged`
- `InvitationCreated`, `InvitationAccepted`

### Dependencies
- MediatR for CQRS
- FluentValidation for input validation
- Entity Framework Core for data access
- Npgsql for PostgreSQL

## Coding Rules for This Service

1. **Tenant Isolation**: Every query MUST filter by OrganizationId
2. **No Direct User Data**: Only store UserId (Guid), get user details from Auth service
3. **Soft Delete Only**: Never use hard delete, set DeletedAt
4. **Events for Side Effects**: Don't call other services directly, publish events
5. **DTOs at API Boundary**: Never expose domain entities in API responses

## Logging

This service uses structured JSON logging via Serilog (see `.claude/skills/structured-logging/SKILL.md`).

- **Log output**: Console (structured text) + File (JSON)
- **File path**: `infrastructure/logs/directory/log-{date}.json`
- **Rotation**: Daily, 30-day retention
- **Correlation ID**: All requests tagged via `X-Correlation-Id` header
- **Context enrichment**: RequestMethod, RequestPath, UserId, OrganizationId, WorkspaceId

See the structured-logging skill for log level guidelines and best practices.

## Commands

```bash
# From services/directory/
dotnet restore
dotnet build
dotnet test
dotnet run --project src/Directory.API
```
