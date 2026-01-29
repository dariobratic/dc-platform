# Access Control Service

RBAC (Role-Based Access Control) permission management service for DC Platform.

## Overview

The Access Control service manages roles, permissions, and role assignments within the DC Platform. It enables fine-grained authorization by allowing administrators to define custom roles with specific permissions and assign those roles to users within organizational or workspace scopes.

## Features

- **Role Management**: Create, update, and delete custom roles scoped to organizations or workspaces
- **Permission Management**: Define granular permissions using a consistent `resource:action` format
- **Role Assignments**: Assign roles to users within specific scopes
- **Permission Checking**: Fast permission verification for authorization decisions
- **System Roles**: Protected system-defined roles that cannot be modified
- **Scope Isolation**: Roles are scoped to either organizations or workspaces for proper multi-tenancy

## Getting Started

### Prerequisites

- .NET 10 SDK
- PostgreSQL 15+
- Access to DC Platform database

### Installation

1. Clone the repository
2. Navigate to the service directory:
   ```bash
   cd services/access-control
   ```

3. Restore dependencies:
   ```bash
   dotnet restore
   ```

4. Update the database connection string in `src/AccessControl.API/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "AccessControlDb": "Host=localhost;Port=5432;Database=dc_platform;Username=postgres;Password=postgres;Search Path=access_control"
     }
   }
   ```

5. Run database migrations:
   ```bash
   dotnet ef database update --project src/AccessControl.Infrastructure --startup-project src/AccessControl.API
   ```

6. Run the service:
   ```bash
   dotnet run --project src/AccessControl.API
   ```

The service will start on `http://localhost:5003`.

### Running Tests

```bash
dotnet test
```

## API Documentation

### Roles

#### Create Role
```http
POST /api/v1/roles
Content-Type: application/json

{
  "name": "Editor",
  "description": "Can edit documents",
  "scopeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "scopeType": "Workspace",
  "permissions": [
    "document:read",
    "document:write",
    "document:update"
  ]
}
```

#### Get Role
```http
GET /api/v1/roles/{id}
```

#### List Roles by Scope
```http
GET /api/v1/roles?scopeId={scopeId}&scopeType={Organization|Workspace}
```

#### Update Role
```http
PUT /api/v1/roles/{id}
Content-Type: application/json

{
  "name": "Senior Editor",
  "description": "Can edit and delete documents",
  "permissions": [
    "document:read",
    "document:write",
    "document:update",
    "document:delete"
  ]
}
```

#### Delete Role
```http
DELETE /api/v1/roles/{id}
```

**Note**: Deletion fails if the role has active assignments.

### Role Assignments

#### Assign Role to User
```http
POST /api/v1/roles/{roleId}/assignments
Content-Type: application/json

{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "scopeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "scopeType": "Workspace",
  "assignedBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

#### Revoke Role from User
```http
DELETE /api/v1/roles/{roleId}/assignments
Content-Type: application/json

{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "scopeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

### Permissions

#### Check Permission
```http
GET /api/v1/permissions/check?userId={userId}&scopeId={scopeId}&permission=document:read
```

Response:
```json
{
  "hasPermission": true,
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "scopeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "permission": "document:read"
}
```

#### Get User Permissions
```http
GET /api/v1/users/{userId}/permissions?scopeId={scopeId}
```

Response:
```json
[
  "document:read",
  "document:write",
  "workspace:view"
]
```

## Permission Format

Permissions follow the format `resource:action`:

- **resource**: The entity or feature being accessed (lowercase, alphanumeric + underscore)
- **action**: The operation being performed (lowercase, alphanumeric + underscore)

### Examples

- `document:read` - Read documents
- `document:write` - Create documents
- `document:update` - Update documents
- `document:delete` - Delete documents
- `workspace:manage` - Manage workspace settings
- `member:invite` - Invite members
- `role:assign` - Assign roles to users

### Validation

Permissions must match the regex pattern: `^[a-z][a-z0-9_]*:[a-z][a-z0-9_]*$`

## Architecture

The service follows Clean Architecture principles with four layers:

1. **Domain**: Core business logic and entities (Role, Permission, RoleAssignment)
2. **Application**: Use cases implemented as Commands and Queries (CQRS pattern)
3. **Infrastructure**: Data access using Entity Framework Core
4. **API**: HTTP endpoints and request/response handling

## Configuration

### Database Schema

The service uses the `access_control` schema in PostgreSQL with three main tables:

- `roles`: Role definitions
- `permissions`: Permission entries (owned by roles)
- `role_assignments`: User-role mappings

### Environment Variables

- `ConnectionStrings__AccessControlDb`: Database connection string
- `ASPNETCORE_ENVIRONMENT`: Environment (Development, Staging, Production)

## Development

### Project Structure

```
src/
├── AccessControl.API/           # Web API layer
├── AccessControl.Application/   # Business logic (CQRS)
├── AccessControl.Domain/        # Core domain entities
└── AccessControl.Infrastructure/ # Data access

tests/
└── AccessControl.Domain.Tests/  # Domain unit tests
```

### Adding a New Permission

1. Define the permission string following the `resource:action` format
2. Add it to the appropriate role during role creation or update
3. Use the permission string in authorization checks

### Creating System Roles

System roles are protected and cannot be modified or deleted by users. To create a system role:

1. Create the role through code or migration
2. Set `IsSystem = true`
3. System roles are typically created during initial setup

## Troubleshooting

### Role Deletion Fails

If you cannot delete a role, it likely has active assignments. First revoke all assignments, then delete the role.

### Permission Check Returns False

Ensure:
1. The user has a role assignment in the specified scope
2. The role contains the permission being checked
3. The permission string matches exactly (case-sensitive)

### Scope Mismatch Error

When assigning a role, ensure the `scopeId` and `scopeType` match the role's scope. A workspace role cannot be assigned at the organization level and vice versa.

## Security Considerations

- Always validate user identity before permission checks
- Use the Gateway service for authentication
- Never trust client-provided scope information without verification
- Audit role assignments through domain events
- Regularly review and prune unused roles

## Support

For issues and questions, please refer to the main DC Platform documentation or create an issue in the repository.

## License

Copyright © 2025 Digital Control. All rights reserved.
