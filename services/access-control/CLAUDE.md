# Access Control Service

RBAC permission management service for DC Platform. Manages Roles, Permissions, and Role Assignments scoped to Organizations or Workspaces.

## Service Scope

### This Service IS Responsible For:
- Role CRUD (create, read, update, delete) within scopes
- Permission management within roles
- Role assignment to users within scopes
- Permission checking for authorization decisions
- Retrieving user permissions for a given scope

### This Service IS NOT Responsible For:
- User authentication (→ Authentication Service)
- User identity/credentials (→ Keycloak)
- Organization/Workspace management (→ Directory Service)
- Audit logging (→ Audit Service via events)
- Policy evaluation beyond simple RBAC (future → ABAC expansion)

## Domain Model

```
Role (aggregate root, scoped to Organization or Workspace)
├── Id: Guid
├── Name: string (unique within scope)
├── Description: string?
├── ScopeId: Guid (Organization or Workspace ID)
├── ScopeType: ScopeType (Organization | Workspace)
├── IsSystem: bool (system roles cannot be modified/deleted)
├── CreatedAt, UpdatedAt
│
└── Permissions[]
    ├── Id: Guid
    ├── RoleId: Guid
    ├── Action: string (format: "resource:action", e.g., "document:read")
    └── CreatedAt

RoleAssignment
├── Id: Guid
├── RoleId: Guid
├── UserId: Guid (external - from Keycloak)
├── ScopeId: Guid (Organization or Workspace ID)
├── ScopeType: ScopeType (must match role's scope)
├── AssignedAt: DateTime
└── AssignedBy: Guid
```

## API Endpoints

### Roles
- `POST /api/v1/roles` - Create role
- `GET /api/v1/roles/{id}` - Get role by ID
- `GET /api/v1/roles?scopeId={id}&scopeType={type}` - List roles by scope
- `PUT /api/v1/roles/{id}` - Update role (name, description, permissions)
- `DELETE /api/v1/roles/{id}` - Delete role (fails if assignments exist)

### Role Assignments
- `POST /api/v1/roles/{roleId}/assignments` - Assign role to user
- `DELETE /api/v1/roles/{roleId}/assignments` - Revoke role from user

### Permissions
- `GET /api/v1/permissions/check?userId={id}&scopeId={id}&permission={action}` - Check if user has permission
- `GET /api/v1/users/{userId}/permissions?scopeId={id}` - Get all user permissions in scope

## Architecture

Clean Architecture with four layers:

```
services/access-control/
├── src/
│   ├── AccessControl.API/           # ASP.NET Core Web API (Port: 5003)
│   │   ├── Controllers/             # PermissionsController, RolesController
│   │   ├── DTOs/                    # Request/Response models
│   │   ├── Middleware/              # Exception handling
│   │   └── Program.cs
│   │
│   ├── AccessControl.Application/   # Business logic (CQRS)
│   │   ├── Commands/                # Write operations
│   │   │   ├── Roles/               # CreateRole, UpdateRole, DeleteRole
│   │   │   └── RoleAssignments/     # AssignRole, RevokeRole
│   │   ├── Queries/                 # Read operations
│   │   │   ├── Permissions/         # CheckPermission, GetUserPermissions
│   │   │   └── Roles/               # GetRoleById, GetRolesByScope
│   │   ├── Validators/              # FluentValidation rules
│   │   ├── Responses/               # RoleResponse, RoleAssignmentResponse, PermissionCheckResponse
│   │   ├── Interfaces/              # IRoleRepository, IRoleAssignmentRepository
│   │   └── Behaviors/               # ValidationBehavior
│   │
│   ├── AccessControl.Domain/        # Core domain
│   │   ├── Entities/                # Role, Permission, RoleAssignment, BaseEntity
│   │   ├── Enums/                   # ScopeType
│   │   ├── Events/                  # RoleCreated, RoleUpdated, RoleDeleted, RoleAssignmentCreated, RoleAssignmentRevoked
│   │   └── Exceptions/              # DomainException
│   │
│   └── AccessControl.Infrastructure/ # External concerns
│       ├── Persistence/             # AccessControlDbContext
│       ├── Configuration/           # Entity configurations
│       └── Repositories/            # RoleRepository, RoleAssignmentRepository
│
├── tests/
│   └── AccessControl.Domain.Tests/  # Domain unit tests
│
├── AccessControl.slnx
├── CLAUDE.md                        # This file
└── README.md
```

## Technical Requirements

### Database
- PostgreSQL with schema: `access_control`
- Connection string key: `AccessControlDb`
- Tables: `roles`, `permissions`, `role_assignments`
- Unique constraints:
  - `roles`: (Name, ScopeId, ScopeType)
  - `permissions`: (RoleId, Action)
  - `role_assignments`: (RoleId, UserId, ScopeId)

### Domain Events (publish to message bus)
- `RoleCreated`, `RoleUpdated`, `RoleDeleted`
- `RoleAssignmentCreated`, `RoleAssignmentRevoked`

### Dependencies
- MediatR for CQRS
- FluentValidation for input validation
- Entity Framework Core 10 for data access
- Npgsql for PostgreSQL

## Business Rules

1. **System Roles**: Cannot be modified or deleted (IsSystem = true)
2. **Role Deletion**: Fails if role has active assignments
3. **Scope Matching**: Role assignment scope must match role's scope
4. **Permission Format**: Must match pattern `^[a-z][a-z0-9_]*:[a-z][a-z0-9_]*$`
5. **Name Uniqueness**: Role names must be unique within a scope (ScopeId + ScopeType)
6. **No Duplicate Assignments**: Cannot assign same role to same user in same scope twice
7. **No Duplicate Permissions**: A role cannot have duplicate permission actions

## Permission Naming Convention

Format: `resource:action`

Examples:
- `document:read`
- `document:write`
- `document:delete`
- `workspace:manage`
- `member:invite`
- `role:assign`

## Configuration

### Port
- Development: `http://localhost:5003`

### Connection String
```json
{
  "ConnectionStrings": {
    "AccessControlDb": "Host=localhost;Port=5432;Database=dc_platform;Username=postgres;Password=postgres;Search Path=access_control"
  }
}
```

## Commands

```bash
# From services/access-control/
dotnet restore
dotnet build
dotnet test
dotnet run --project src/AccessControl.API

# Database migrations (from src/AccessControl.Infrastructure/)
dotnet ef migrations add InitialCreate --startup-project ../AccessControl.API
dotnet ef database update --startup-project ../AccessControl.API
```

## Coding Rules for This Service

1. **No Direct User Data**: Only store UserId (Guid), get user details from Authentication service
2. **Scope Validation**: Always verify scope exists in Directory service before creating roles
3. **Events for Side Effects**: Publish domain events, don't call other services directly
4. **DTOs at API Boundary**: Never expose domain entities in API responses
5. **Permission Checking**: Always check both Organization and Workspace scopes (inheritance)
6. **Async All the Way**: All I/O operations must be async

## What NOT to Do

- Do NOT create user profiles or store user details
- Do NOT manage organizations or workspaces (that's Directory service)
- Do NOT implement authentication (that's Authentication service)
- Do NOT hard delete anything (soft delete not applicable here, but prevent deletion with active assignments)
- Do NOT access Directory service database directly (use API calls or events)
- Do NOT allow modification of system roles
- Do NOT skip scope validation

## Future Enhancements

- ABAC (Attribute-Based Access Control) support
- Permission inheritance from Organization to Workspace
- Role templates for common use cases
- Permission groups/categories
- Conditional permissions based on attributes
- Time-based role assignments (expiration)
