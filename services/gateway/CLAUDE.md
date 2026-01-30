# Gateway Service

API Gateway / Backend for Frontend (BFF) for DC Platform.

## Service Scope

### This Service IS Responsible For:
- Request routing to backend microservices
- CORS configuration for frontend applications
- Request/response transformation (future)
- API versioning and path management
- Health check aggregation from downstream services
- Rate limiting and throttling (future)
- Request/response logging and monitoring (future)

### This Service IS NOT Responsible For:
- Business logic (→ Domain services)
- Data persistence (no database)
- Authentication/authorization logic (→ Authentication Service)
- User session management (→ Authentication Service)

## Architecture

The Gateway is a **stateless** API Gateway that acts as a single entry point for all client applications. It does not follow Clean Architecture patterns as it has no business logic or domain model.

### Key Characteristics:
- **No database** - Stateless routing only
- **No Clean Architecture layers** - Single API project
- **Minimal middleware** - CORS, routing, health checks
- **Configuration-driven routing** - Service URLs in appsettings.json

## API Endpoints

### Health & Status
- `GET /health` - Basic health check (ASP.NET Core standard)
- `GET /api/health` - Detailed health response with service info

### Active Routes (YARP Reverse Proxy)

All routes are configured in `appsettings.json` under the `ReverseProxy` section.

| Route Pattern | Destination Service | Port |
|---|---|---|
| `/api/v1/organizations/{**catch-all}` | Directory | 5001 |
| `/api/v1/workspaces/{**catch-all}` | Directory | 5001 |
| `/api/v1/memberships/{**catch-all}` | Directory | 5001 |
| `/api/v1/invitations/{**catch-all}` | Directory | 5001 |
| `/api/v1/auth/{**catch-all}` | Authentication | 5002 |
| `/api/v1/roles/{**catch-all}` | Access Control | 5003 |
| `/api/v1/permissions/{**catch-all}` | Access Control | 5003 |
| `/api/v1/audit/{**catch-all}` | Audit | 5004 |
| `/api/v1/notifications/{**catch-all}` | Notification | 5005 |
| `/api/v1/config/{**catch-all}` | Configuration | 5006 |

YARP automatically forwards incoming requests to the appropriate downstream service based on the path pattern.

## Configuration

### CORS Settings (`appsettings.json`)
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173"
    ]
  }
}
```

### YARP Reverse Proxy Configuration (`appsettings.json`)

YARP routing is configured via the `ReverseProxy` section with two parts:

1. **Routes**: Define path patterns and which cluster to forward to
2. **Clusters**: Define destination service addresses

Example:
```json
{
  "ReverseProxy": {
    "Routes": {
      "organizations-route": {
        "ClusterId": "directory",
        "Match": {
          "Path": "/api/v1/organizations/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "directory": {
        "Destinations": {
          "destination1": {
            "Address": "http://localhost:5001"
          }
        }
      }
    }
  }
}
```

See the full configuration in `appsettings.json` for all routes and clusters.

## Project Structure

```
services/gateway/
├── src/
│   └── Gateway.API/            # ASP.NET Core Web API
│       ├── Models/             # DTOs (health response, etc.)
│       ├── Properties/         # Launch settings
│       ├── appsettings.json    # Configuration
│       ├── Gateway.API.csproj
│       └── Program.cs
│
├── Gateway.slnx
├── CLAUDE.md                   # This file
└── README.md
```

## Technical Requirements

### No Database
This service is stateless and does not persist any data.

### No Domain Logic
All business logic resides in downstream services. The Gateway only routes requests.

### Dependencies
- Microsoft.AspNetCore.OpenApi for API documentation
- ASP.NET Core Health Checks for monitoring
- YARP (Yet Another Reverse Proxy) for request routing to downstream services

## Future Features

1. **Authentication Middleware**: Validate JWT tokens from Authentication service
3. **Rate Limiting**: Per-user/per-tenant rate limiting
4. **Circuit Breaker**: Polly-based circuit breaker for downstream service failures
5. **Request/Response Logging**: Centralized logging for all API calls
6. **API Versioning**: Support for multiple API versions

## Coding Rules for This Service

1. **Stateless Only**: Never store state in memory or cache
2. **No Business Logic**: Only routing and transformation
3. **Configuration-Driven**: Service URLs and settings via appsettings
4. **Fast Fail**: Return errors immediately, don't retry by default
5. **Observability**: Log all requests for monitoring and debugging

## Logging

This service uses structured JSON logging via Serilog (see `.claude/skills/structured-logging/SKILL.md`).

- **Log output**: Console (structured text) + File (JSON)
- **File path**: `infrastructure/logs/gateway/log-{date}.json`
- **Rotation**: Daily, 30-day retention
- **Correlation ID**: All requests tagged via `X-Correlation-Id` header
- **Context enrichment**: RequestMethod, RequestPath, UserId, OrganizationId, WorkspaceId

See the structured-logging skill for log level guidelines and best practices.

## Commands

```bash
# From services/gateway/
dotnet restore
dotnet build
dotnet run --project src/Gateway.API

# Run on specific port
dotnet run --project src/Gateway.API --urls "http://localhost:5000"
```

## Development Notes

- Default port: `5000` (HTTP), `5001` (HTTPS)
- Health endpoint: `http://localhost:5000/api/health`
- No migrations or database setup required
- No Clean Architecture setup needed (intentionally simple)
