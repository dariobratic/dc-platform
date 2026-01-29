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

### Service Routes

Configure downstream service URLs:

```json
{
  "ServiceRoutes": {
    "Directory": "http://localhost:5001",
    "Authentication": "http://localhost:5002",
    "AccessControl": "http://localhost:5003"
  }
}
```

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
- Configuration-driven routing

For detailed development guidelines, see [CLAUDE.md](./CLAUDE.md).

## Future Features

- Request proxying to downstream services
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
