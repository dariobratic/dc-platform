---
name: docker-compose
description: |
  Docker Compose configuration for DC Platform. Use when adding services to
  docker-compose.yml, creating Dockerfiles, configuring container networking,
  health checks, or environment variables.
---
# Docker Compose - DC Platform

## File Location
infrastructure/docker-compose.yml

## Services Structure

All platform services run in containers with consistent configuration.

### Naming Convention
- Service name: lowercase, matches folder name (e.g., `directory`, `gateway`)
- Container name: `dc-platform-{service}` (e.g., `dc-platform-directory`)

### Port Mapping
| Service | Internal | External |
|---------|----------|----------|
| Gateway | 5000 | 5000 |
| Directory | 5001 | 5001 |
| Authentication | 5002 | 5002 |
| Access Control | 5003 | 5003 |
| Audit | 5004 | 5004 |
| Notification | 5005 | 5005 |
| Configuration | 5006 | 5006 |
| Admin API | 5007 | 5007 |
| PostgreSQL | 5432 | 5432 |
| Keycloak | 8080 | 8080 |

### Environment Variables
Use `.env` file in infrastructure/ folder:
```env
POSTGRES_USER=dcplatform
POSTGRES_PASSWORD=dcplatform_dev
POSTGRES_DB=dc_platform
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD=admin
```

### Health Checks
Every service must have health check:
```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:{port}/health"]
  interval: 10s
  timeout: 5s
  retries: 3
  start_period: 30s
```

### Startup Order
```yaml
depends_on:
  postgres:
    condition: service_healthy
  keycloak:
    condition: service_healthy
```

### Volumes
```yaml
volumes:
  - ../logs:/app/logs              # Shared logs
  - postgres_data:/var/lib/postgresql/data
  - keycloak_data:/opt/keycloak/data
```

### Networks
Single bridge network for all services:
```yaml
networks:
  dc-platform:
    driver: bridge
```

### Database Connection Strings
Services use environment variable:
```yaml
environment:
  - ConnectionStrings__DefaultConnection=Host=postgres;Database=dc_platform;Username=dcplatform;Password=dcplatform_dev
```

### Keycloak Configuration
```yaml
environment:
  - Keycloak__Authority=http://keycloak:8080/realms/dc-platform
  - Keycloak__ClientId=dc-platform-gateway
  - Keycloak__ClientSecret=${KEYCLOAK_CLIENT_SECRET}
```

## Commands
```bash
# Start all services
docker-compose up -d

# Start with build
docker-compose up -d --build

# View logs
docker-compose logs -f [service-name]

# Stop all
docker-compose down

# Reset (including volumes)
docker-compose down -v
```

## Dockerfile Pattern

Each service has Dockerfile in its folder:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE {port}

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "{ServiceName}.API.dll"]
```