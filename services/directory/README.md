# Directory Service

Organization, Workspace, and Membership management for DC Platform.

## Overview

Directory Service is the core tenant management service that handles:
- **Organizations** - Top-level tenant boundary
- **Workspaces** - Isolated work areas within an organization
- **Memberships** - User associations with workspaces and their roles
- **Invitations** - Workflow for inviting users to workspaces

## Getting Started

### Prerequisites
- .NET SDK 8.0+
- PostgreSQL 15+
- Docker (optional, for local PostgreSQL)

### Local Development

```bash
# Start PostgreSQL (if using Docker)
docker run -d --name dc-postgres \
  -e POSTGRES_USER=dcplatform \
  -e POSTGRES_PASSWORD=devpassword \
  -e POSTGRES_DB=dcplatform \
  -p 5432:5432 \
  postgres:15

# Navigate to service
cd services/directory

# Restore dependencies
dotnet restore

# Run migrations
dotnet ef database update --project src/Directory.Infrastructure

# Run the service
dotnet run --project src/Directory.API

# Run tests
dotnet test
```

### Configuration

Configure via `appsettings.Development.json` or environment variables:

```json
{
  "ConnectionStrings": {
    "DirectoryDb": "Host=localhost;Database=dcplatform;Username=dcplatform;Password=devpassword;SearchPath=directory"
  },
  "MessageBus": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  }
}
```

## API Documentation

Base URL: `http://localhost:5001/api/v1`

### Organizations

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/organizations` | Create organization |
| GET | `/organizations/{id}` | Get organization |
| PUT | `/organizations/{id}` | Update organization |
| DELETE | `/organizations/{id}` | Delete organization |

### Workspaces

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/organizations/{orgId}/workspaces` | Create workspace |
| GET | `/organizations/{orgId}/workspaces` | List workspaces |
| GET | `/workspaces/{id}` | Get workspace |
| PUT | `/workspaces/{id}` | Update workspace |
| DELETE | `/workspaces/{id}` | Delete workspace |

### Memberships

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/workspaces/{id}/members` | Add member |
| GET | `/workspaces/{id}/members` | List members |
| PUT | `/workspaces/{id}/members/{userId}` | Update member role |
| DELETE | `/workspaces/{id}/members/{userId}` | Remove member |

### Invitations

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/workspaces/{id}/invitations` | Create invitation |
| POST | `/invitations/{token}/accept` | Accept invitation |
| DELETE | `/invitations/{id}` | Revoke invitation |

## Architecture

```
Directory.API          → HTTP interface, DTOs, middleware
Directory.Application  → Business logic, commands, queries
Directory.Domain       → Entities, value objects, domain events
Directory.Infrastructure → Database, repositories, messaging
```

## Testing

```bash
# All tests
dotnet test

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Specific project
dotnet test tests/Directory.Domain.Tests
```

## Related Services

- **Authentication Service** - User identity, tokens
- **Access Control Service** - Permission evaluation
- **Audit Service** - Consumes directory events for logging
- **Notification Service** - Consumes invitation events for emails
