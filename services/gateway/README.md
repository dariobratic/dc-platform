# Gateway Service

API Gateway / Backend for Frontend (BFF) for DC Platform.

## Overview

The Gateway service acts as a single entry point for all client applications (web, mobile, admin). It routes requests to the appropriate backend microservices, handles CORS, and provides health check endpoints.

**Key Characteristics:**
- Stateless (no database)
- Configuration-driven routing
- Minimal middleware
- Single API project (no Clean Architecture layers)

## Quick Start

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run --project src/Gateway.API

# Or run on specific port
dotnet run --project src/Gateway.API --urls "http://localhost:5000"
```

The service will start on `http://localhost:5000` (HTTP) or `https://localhost:5001` (HTTPS).

## Health Check

```bash
# Basic health check
curl http://localhost:5000/health

# Detailed health check
curl http://localhost:5000/api/health
```

Expected response:
```json
{
  "serviceName": "Gateway",
  "status": "Healthy",
  "timestamp": "2026-01-29T12:00:00Z"
}
```

## Configuration

### CORS Settings

Edit `appsettings.json` to allow frontend origins:

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

### Reverse Proxy Routing (YARP)

The Gateway uses YARP (Yet Another Reverse Proxy) to route incoming requests to the appropriate backend microservices.

#### Active Routes

All incoming requests matching these patterns are automatically proxied to the corresponding downstream service:

| Route Pattern | Destination Service | Address |
|---|---|---|
| `/api/v1/organizations/{**catch-all}` | Directory | http://localhost:5001 |
| `/api/v1/workspaces/{**catch-all}` | Directory | http://localhost:5001 |
| `/api/v1/memberships/{**catch-all}` | Directory | http://localhost:5001 |
| `/api/v1/invitations/{**catch-all}` | Directory | http://localhost:5001 |
| `/api/v1/auth/{**catch-all}` | Authentication | http://localhost:5002 |
| `/api/v1/roles/{**catch-all}` | Access Control | http://localhost:5003 |
| `/api/v1/permissions/{**catch-all}` | Access Control | http://localhost:5003 |
| `/api/v1/audit/{**catch-all}` | Audit | http://localhost:5004 |
| `/api/v1/notifications/{**catch-all}` | Notification | http://localhost:5005 |
| `/api/v1/config/{**catch-all}` | Configuration | http://localhost:5006 |

#### How It Works

1. Client sends request to Gateway (e.g., `http://localhost:5000/api/v1/organizations`)
2. YARP matches the path against configured routes
3. Request is proxied to the destination service (e.g., `http://localhost:5001/api/v1/organizations`)
4. Response is returned to the client

#### YARP Configuration

Routes and clusters are configured in `appsettings.json` under the `ReverseProxy` section:

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

To add new routes:
1. Add a new route entry with the path pattern and cluster ID
2. Add a new cluster entry with the destination service address
3. Restart the Gateway service

## Project Structure

```
services/gateway/
├── src/
│   └── Gateway.API/            # Main API project
│       ├── Models/             # DTOs
│       ├── Properties/         # Launch settings
│       ├── appsettings.json
│       └── Program.cs
├── Gateway.slnx                # Solution file
├── CLAUDE.md                   # Developer guide
└── README.md                   # This file
```

## Development

This service is intentionally simple:
- No database or EF Core
- No Clean Architecture layers
- No domain logic
- Configuration-driven routing via YARP

For detailed development guidelines, see [CLAUDE.md](./CLAUDE.md).

## Future Features

- JWT token validation
- Rate limiting and throttling
- Circuit breaker for resilience
- Request/response logging
- API versioning support

## Related Services

- **Directory Service** - Organization/Workspace management (port 5001)
- **Authentication Service** - User auth via Keycloak (port 5002)
- **Access Control Service** - Permissions (port 5003)

## Support

For questions or issues, refer to the main platform documentation at `docs/`.
