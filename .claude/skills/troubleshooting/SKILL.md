---
name: troubleshooting
description: |
  Debugging and troubleshooting guide. Use when tests fail, cross-service issues
  occur, or when deciding between bug fix, refactor, or architectural change.
---

# Troubleshooting & Debugging - DC Platform

## 1. Test Failure Triage

When a test fails, follow this exact order. Do NOT jump to "the code is broken" before ruling out test issues.

### Step 1: Is the assertion valid?

Ask: "Does this test actually assert the right thing?"

- Read the test name: `MethodName_Scenario_ExpectedResult` — does the assertion match `ExpectedResult`?
- Check for wrong status code expectations (e.g., expecting 201 when the endpoint returns 200)
- Check for wrong DTO shape (property name casing, missing fields, enum as string vs int)
- Check for flaky assertions: `DateTime.UtcNow` comparisons without tolerance, ordering assumptions on unordered collections
- Check for wrong HTTP method or route typo

**Common false failures:**
```
Expected 201 Created but got 400 → check request body shape, not the endpoint
Expected "Active" but got "active" → JSON serialization casing mismatch
Expected 1 item but got 0 → test data wasn't created, not a query bug
```

### Step 2: Is the test setup correct?

Ask: "Is the test environment configured correctly?"

- **Fixture issues**: Is the database migrated? Is the container healthy?
- **Data isolation**: Does this test depend on data from another test? (It should NOT)
- **Unique constraints**: Are slugs/keys unique per test? Check `UniqueSlug()` helpers
- **Missing dependencies**: Did WebApplicationFactory register all services?
- **Connection string**: Is the test container connection string injected correctly?
- **Schema**: Does the test DB have the correct schema (e.g., `directory`, not `public`)?

**Debug checklist:**
```
1. Run the single failing test in isolation → still fails?
2. Add logging to the fixture InitializeAsync → container started?
3. Check _postgres.GetConnectionString() → valid?
4. Check db.Database.CanConnect() → true?
5. Check db.Database.GetAppliedMigrations() → all present?
```

### Step 3: Is there a bug in the code?

Only after confirming steps 1 and 2 are clean:

- Read the controller action being tested
- Trace the flow: Controller → MediatR Command/Query → Handler → Repository → DbContext
- Check FluentValidation rules — is valid input being rejected?
- Check exception handling middleware — is the right exception mapped to the right status code?
- Check EF Core query filters (soft delete: `DeletedAt == null` might hide data)
- Check EF Core entity configuration (unique indexes, cascade behavior)

**Reproduce locally:**
```bash
# Run just the failing test
dotnet test --filter "FullyQualifiedName~ClassName.MethodName"

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Check DB state (if using Testcontainers, add a breakpoint or log)
```

## 2. Cross-Service Debugging

### Request Flow

```
Client → Gateway (5000) → YARP → Downstream Service → PostgreSQL
                ↓                       ↓
         Correlation ID          Serilog Logs
         propagated via          written to
         X-Correlation-Id       infrastructure/logs/{service}/
```

### Tracing a Request

**Step 1: Get the Correlation ID**

Every request gets an `X-Correlation-Id` (auto-generated if not provided). This ID is:
- Set in `CorrelationIdMiddleware` (every service has this)
- Stored in `HttpContext.Items["CorrelationId"]`
- Pushed to `Serilog.Context.LogContext`
- Added to the response `X-Correlation-Id` header
- Written to every log entry as `correlationId` field

**Step 2: Find logs by Correlation ID**

```bash
# Search JSON log files for a specific correlation ID
grep "correlation-id-here" infrastructure/logs/*/log-*.json

# Search specific service
grep "correlation-id-here" infrastructure/logs/directory/log-2025-01-30.json

# Pretty-print matching entries
grep "correlation-id-here" infrastructure/logs/gateway/log-*.json | jq .
```

**Step 3: Reconstruct the flow**

Log entries include these fields for reconstruction:
- `service` — which service processed it (Gateway, Directory, etc.)
- `correlationId` — ties all entries together
- `requestMethod` + `requestPath` — what was called
- `statusCode` — what was returned
- `duration` — how long it took
- `userId`, `organizationId`, `workspaceId` — tenant context
- `exceptionType`, `exceptionMessage`, `stackTrace` — if error occurred

### Which Logs Are Relevant

| Symptom | Check First | Then Check |
|---------|-------------|------------|
| 404 Not Found | Downstream service log (entity exists?) | Gateway log (route matched?) |
| 500 Internal Server Error | Downstream service log (exception details) | Stack trace in log |
| 502 Bad Gateway | Gateway log (YARP connection error) | Is downstream service running? |
| Timeout | Downstream service log (slow query?) | Gateway YARP timeout config |
| Auth failures | Authentication service log | Keycloak logs (`docker logs dc-platform-keycloak`) |
| Data not showing | Service log (query filters?) | DB direct query (soft delete?) |
| Wrong data returned | Service log (tenant isolation?) | Check organizationId in query |

### Service Health Quick Check

```bash
# Check all services at once (Admin API aggregates this)
curl http://localhost:5007/api/v1/admin/system/health

# Check individual service
curl http://localhost:5001/health   # Directory
curl http://localhost:5002/health   # Authentication
curl http://localhost:5003/health   # Access Control
curl http://localhost:5004/health   # Audit
curl http://localhost:5005/health   # Notification
curl http://localhost:5006/health   # Configuration

# Docker container health
docker compose -f infrastructure/docker-compose.yml ps
```

### Database Debugging

```bash
# Connect to PostgreSQL
docker exec -it dc-platform-postgres psql -U dcplatform -d dc_platform

# List schemas
\dn

# Check tables in a schema
SET search_path TO directory;
\dt

# Check data
SELECT * FROM directory.organizations LIMIT 10;
SELECT * FROM audit.audit_entries ORDER BY "Timestamp" DESC LIMIT 10;

# Check soft-deleted records (normally hidden by EF query filters)
SELECT * FROM directory.organizations WHERE "DeletedAt" IS NOT NULL;
```

## 3. Escalation Criteria

### Bug Fix (just fix it)

The problem is:
- A typo in code, config, or SQL
- A missing null check or validation
- Wrong status code mapping in exception middleware
- A query that doesn't filter correctly
- A missing `await` or incorrect async pattern
- An EF Core configuration issue (wrong column type, missing index)

**Action:** Fix, write a test, commit as `fix(scope): description`.

### Refactor (change structure, same behavior)

The problem is:
- Code works but is hard to understand or maintain
- Duplicate logic across handlers
- Missing abstraction that would prevent future bugs
- Test infrastructure needs improvement

**Action:** Refactor, ensure tests pass, commit as `refactor(scope): description`.

### Architectural Change (needs discussion)

The problem requires:
- Changing service boundaries (moving endpoints between services)
- Adding a new service or removing one
- Changing the database strategy (schema isolation → separate DBs)
- Adding a new infrastructure component (Redis, RabbitMQ)
- Changing the authentication/authorization flow
- Modifying the API contract in a breaking way
- Adding cross-service synchronous calls (service A directly calls service B)

**Action:**
1. Check `CLAUDE.md` (root and service-level) for existing guidance
2. Check `docs/` for Architecture Decision Records (ADRs)
3. If no existing guidance covers it → create an ADR in `docs/adr/`
4. ADR format:
   ```
   # ADR-NNN: Title
   ## Status: Proposed | Accepted | Deprecated
   ## Context: Why this decision is needed
   ## Decision: What we chose
   ## Consequences: Trade-offs and implications
   ```

### When to Consult CLAUDE.md

- **Root CLAUDE.md**: Cross-cutting decisions (multi-tenancy, service boundaries, git workflow, coding conventions)
- **Service CLAUDE.md**: Service-specific scope, domain model, API endpoints, business rules, what NOT to do
- **Skills**: Specific technical patterns (logging, Docker, Keycloak, testing)

### When to Create an ADR

- The change affects more than one service
- The change introduces a new technology or pattern
- The change is irreversible or expensive to reverse
- There are multiple valid approaches and the choice matters
- Future developers would ask "why did we do it this way?"

## 4. Docker Build Troubleshooting

### Common Build Failures

| Error | Cause | Fix |
|-------|-------|-----|
| `NETSDK1064: Package ... was not found` | `COPY . .` overwrites Docker-restored `obj/project.assets.json` with stale host version | Add `.dockerignore` with `**/bin/`, `**/obj/`, `**/.vs/` to each service |
| `Microsoft.AspNetCore.OpenApi version mismatch` | Different services reference different patch versions (e.g., 10.0.0 vs 10.0.2) | Align all `Microsoft.AspNetCore.OpenApi` versions to match the SDK in the Docker image |
| Health check failing / container unhealthy | `curl` not available in `aspnet:10.0` runtime image | Add `RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*` to the `base` stage |
| Dependent services never start | Service health checks fail, so `depends_on: condition: service_healthy` blocks forever | Fix health check command (install curl) or adjust health check to use a tool available in the image |

### Package Version Rules

All Microsoft packages in .csproj files must use the same patch version that ships with the .NET SDK Docker image:

- `Microsoft.AspNetCore.OpenApi` → must match the SDK version (e.g., `10.0.0`)
- `Microsoft.EntityFrameworkCore.*` → keep consistent across all services
- `Microsoft.AspNetCore.Authentication.JwtBearer` → must match the SDK version

Check the SDK version in the Docker image:
```bash
docker run --rm mcr.microsoft.com/dotnet/sdk:10.0 dotnet --version
```

### .dockerignore (Required)

Every service MUST have a `.dockerignore` file to prevent host build artifacts from corrupting Docker builds:

```
**/bin/
**/obj/
**/.vs/
```

Without this, `COPY . .` in the Dockerfile copies the host's `obj/project.assets.json` (which may reference different package versions than what `dotnet restore` downloaded inside Docker), causing `NETSDK1064` errors during `dotnet publish --no-restore`.

### Dockerfile Health Check Pattern

All service Dockerfiles must install `curl` in the `base` stage for docker-compose health checks:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE {port}
ENV ASPNETCORE_URLS=http://+:{port}
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
```

### Quick Docker Debug Commands

```bash
# Build all services
cd infrastructure && docker compose build

# Build single service
docker compose build directory

# View build logs for a specific service
docker compose build --no-cache gateway 2>&1 | tail -50

# Check container status
docker compose ps

# View container logs
docker compose logs -f directory

# Shell into a running container
docker exec -it dc-platform-directory /bin/bash

# Clean rebuild
docker compose down -v && docker compose build --no-cache && docker compose up
```
