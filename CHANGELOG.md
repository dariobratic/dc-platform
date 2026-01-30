# Changelog

All notable changes to DC Platform will be documented in this file.

Format: `[MAJOR.BUILD] - YYYY-MM-DD`

---

## [0.11] - 2025-01-30

### Added - Admin API Service
- .NET 10 Web API project (lightweight, no database, no Clean Architecture layers)
- Admin dashboard endpoint (`GET /api/v1/admin/dashboard`) — aggregated stats (org count, audit entry count) fetched in parallel
- System health endpoint (`GET /api/v1/admin/system/health`) — checks all 7 downstream services in parallel, reports Healthy/Degraded
- Organizations list endpoint (`GET /api/v1/admin/organizations`) — proxies to Directory service
- Recent audit endpoint (`GET /api/v1/admin/audit/recent?count=20`) — proxies to Audit service
- Health check endpoint (`GET /api/v1/admin/health`)
- **Typed HttpClient services**: `DirectoryServiceClient`, `AuditServiceClient`, `ServiceHealthChecker`
- Graceful degradation — returns partial data when downstream services are unavailable
- Structured JSON logging via Serilog (console + file sinks)
- CorrelationIdMiddleware and ExceptionHandlingMiddleware
- CLAUDE.md and README.md with service documentation
- Port: 5007

---

## [0.10] - 2025-01-30

### Added - Gateway Reverse Proxy (YARP)
- **YARP Integration** — Added `Yarp.ReverseProxy` 2.3.0 to Gateway service
- **10 proxy routes** configured in appsettings.json, routing all API traffic to downstream services:
  - `/api/v1/organizations/*`, `/api/v1/workspaces/*`, `/api/v1/memberships/*`, `/api/v1/invitations/*` → Directory (5001)
  - `/api/v1/auth/*` → Authentication (5002)
  - `/api/v1/roles/*`, `/api/v1/permissions/*` → Access Control (5003)
  - `/api/v1/audit/*` → Audit (5004)
  - `/api/v1/notifications/*` → Notification (5005)
  - `/api/v1/config/*` → Configuration (5006)
- **6 YARP clusters** — one per downstream service with destination addresses
- Gateway health endpoints (`/health`, `/api/health`) remain direct (not proxied)
- Replaced static `ServiceRoutes` config with YARP `ReverseProxy` section
- Updated CLAUDE.md and README.md with routing documentation

---

## [0.9] - 2025-01-30

### Added - Configuration Service
- .NET 10 Web API project (lightweight single-project, no Clean Architecture layers)
- Get organization settings endpoint (`GET /api/v1/config/{organizationId}`) — returns key-value settings dictionary
- Update organization settings endpoint (`PUT /api/v1/config/{organizationId}`) — batch upsert settings
- Get feature flags endpoint (`GET /api/v1/config/{organizationId}/features`) — returns all feature flags for org
- Toggle feature flag endpoint (`PUT /api/v1/config/{organizationId}/features/{featureKey}`) — upsert feature flag
- Health check endpoint (`GET /api/v1/config/health`)
- Entities: `OrganizationSetting` (key-value pairs), `FeatureFlag` (boolean toggles)
- EF Core with PostgreSQL, `configuration` schema
  - Composite unique indexes on (OrganizationId, Key) for both tables
  - Initial migration included
- Structured JSON logging via Serilog (console + file sinks)
- CorrelationIdMiddleware and ExceptionHandlingMiddleware
- Multi-tenant isolation via organizationId in all queries
- CLAUDE.md and README.md with service documentation
- Port: 5006

---

## [0.8] - 2025-01-29

### Added - Notification Service
- .NET 10 Web API project (lightweight, no database, no Clean Architecture layers)
- Email notification endpoint (`POST /api/v1/notifications/email`) — logs email details (SMTP integration planned)
- Push notification endpoint (`POST /api/v1/notifications/push`) — placeholder for future implementation
- Health check endpoint (`GET /api/v1/notifications/health`)
- `INotificationService` / `NotificationService` with in-memory template engine
  - Built-in templates: `welcome`, `password-reset`, `invitation`
  - `{{placeholder}}` replacement with templateData dictionary
- Request models: `EmailNotificationRequest`, `PushNotificationRequest`, `NotificationResponse`
- Structured JSON logging via Serilog (console + file sinks)
- CorrelationIdMiddleware and ExceptionHandlingMiddleware
- Placeholder SMTP configuration in appsettings.json
- CLAUDE.md and README.md with service documentation
- Port: 5005

---

## [0.7] - 2025-01-29

### Added - Structured Logging (All Services)
- **Serilog Integration** — Added structured JSON logging to all 5 services (Gateway, Authentication, Directory, Access Control, Audit)
  - Serilog.AspNetCore, Serilog.Sinks.File, Serilog.Expressions packages
  - Console sink with structured text template
  - File sink with daily rotation and 30-day retention
  - Per-service log paths: `infrastructure/logs/{service}/log-{date}.json`

- **CorrelationIdMiddleware** — Distributed tracing via `X-Correlation-Id` header in all services
  - Auto-generates correlation ID if not provided
  - Propagates via `Serilog.Context.LogContext`

- **Logging Skill** — Created `.claude/skills/structured-logging/SKILL.md`
  - Serilog configuration patterns
  - Log level guidelines (Verbose → Fatal)
  - Middleware order: CorrelationId → ExceptionHandling → SerilogRequestLogging → CORS → Auth
  - Context enrichment: RequestMethod, RequestPath, UserId, OrganizationId, WorkspaceId

- **Configuration Updates**
  - `appsettings.json` — Serilog config with Console + File sinks for each service
  - `appsettings.Development.json` — Debug-level override for development
  - CLAUDE.md — Logging section added to all service documentation

### Infrastructure
- Log directory structure: `infrastructure/logs/{gateway,authentication,directory,access-control,audit}/`
- `.gitkeep` files to preserve directory structure in git
- `.gitignore` updated with log file exclusion patterns

---

## [0.6] - 2025-01-29

### Added - Audit Service (Complete)
- **Domain Layer**
  - Entity: AuditEntry (sealed, immutable aggregate root — no update/delete)
  - Domain Event: AuditEntryCreated
  - 14 properties: UserId, Action, EntityType, EntityId, OrganizationId, WorkspaceId, Details (jsonb), IpAddress, UserAgent, ServiceName, CorrelationId, Timestamp, UserEmail

- **Application Layer**
  - Command: CreateAuditEntry (append-only, no update/delete commands)
  - Queries: GetAuditEntryById, GetAuditEntries (paginated with 8 filters), GetEntityAuditHistory
  - Validators: CreateAuditEntryValidator, GetAuditEntriesValidator
  - PagedResponse generic type for paginated results
  - MediatR CQRS pattern with ValidationBehavior pipeline

- **Infrastructure Layer**
  - AuditDbContext with domain event dispatching
  - Entity configuration with 6 performance indexes (OrganizationId, Timestamp, EntityType+EntityId, UserId, ServiceName, OrganizationId+Timestamp)
  - Repository: AuditEntry (AddAsync only — no update/delete)
  - PostgreSQL with `audit` schema
  - Initial EF Core migration

- **API Layer**
  - AuditController (4 endpoints: create, get by ID, paginated query, entity history)
  - No PUT/DELETE endpoints (compliance: immutable log)
  - ExceptionHandlingMiddleware (RFC 7807 ProblemDetails)

- **Tests**
  - Domain tests: 15 tests (immutability verification, validation, domain events)

---

## [0.5] - 2025-01-29

### Added - Access Control Service (Complete)
- **Domain Layer**
  - Entities: Role (aggregate root), Permission (child), RoleAssignment
  - Enum: ScopeType (Organization, Workspace)
  - Domain Events: RoleCreated, RoleUpdated, RoleDeleted, RoleAssignmentCreated, RoleAssignmentRevoked
  - Business rules: system role protection, scope matching, duplicate prevention

- **Application Layer**
  - Commands: CreateRole, UpdateRole, DeleteRole, AssignRole, RevokeRole
  - Queries: CheckPermission, GetUserPermissions, GetRoleById, GetRolesByScope
  - Validators: FluentValidation for all commands and queries
  - MediatR CQRS pattern with ValidationBehavior pipeline

- **Infrastructure Layer**
  - AccessControlDbContext with domain event dispatching
  - Entity configurations (EF Core Fluent API)
  - Repositories: Role, RoleAssignment
  - PostgreSQL with `access_control` schema
  - Initial EF Core migration (roles, permissions, role_assignments tables)

- **API Layer**
  - RolesController (7 endpoints: CRUD + assign/revoke)
  - PermissionsController (2 endpoints: check + list user permissions)
  - DTOs for all request/response models
  - ExceptionHandlingMiddleware (RFC 7807 ProblemDetails)

- **Tests**
  - Domain tests: 19 tests (Permission, Role, RoleAssignment)

---

## [0.4] - 2025-01-29

### Added - Authentication Service
- .NET 10 Web API project (stateless, no database, no Clean Architecture layers)
- OAuth2 token exchange endpoint (`POST /api/auth/token`) — exchanges authorization code for tokens
- Token refresh endpoint (`POST /api/auth/refresh`) — refreshes expired access tokens
- User info endpoint (`GET /api/auth/userinfo`) — returns current user from JWT claims
- Logout endpoint (`POST /api/auth/logout`) — revokes refresh token in Keycloak (best-effort)
- Health check endpoints (`/health` and `/api/health`)
- KeycloakService for HTTP communication with Keycloak token/revoke endpoints
- JWT Bearer authentication with Keycloak validation
- CORS configuration (configurable origins via appsettings)
- Keycloak integration skill (`.claude/skills/keycloak-integration/SKILL.md`)
- CLAUDE.md and README.md with service documentation

---

## [0.3] - 2025-01-29

### Added - Gateway Service
- .NET 10 Web API project (no database, no Clean Architecture layers)
- Health check endpoints (`/health` and `/api/health`)
- CORS configuration (configurable origins via appsettings)
- Service route configuration for all platform microservices
- CLAUDE.md with service scope documentation

---

## [0.2] - 2025-01-29

### Added - Directory Service (Complete)
- **Domain Layer**
  - Entities: Organization, Workspace, Membership, Invitation
  - Value Objects: Slug, OrganizationSettings
  - Domain Events: 11 events (OrganizationCreated, WorkspaceCreated, MemberAdded, etc.)
  - Enums: OrganizationStatus, WorkspaceStatus, WorkspaceRole, InvitationStatus

- **Application Layer**
  - Commands: Create/Update/Delete for Organization, Workspace
  - Commands: AddMember, RemoveMember, ChangeMemberRole
  - Commands: CreateInvitation, AcceptInvitation, RevokeInvitation
  - Queries: GetById, GetByOrganization, GetByToken, GetUserMemberships
  - Validators: FluentValidation for all commands
  - MediatR CQRS pattern with ValidationBehavior pipeline

- **Infrastructure Layer**
  - DirectoryDbContext with domain event dispatching
  - Entity configurations (EF Core Fluent API)
  - Repositories: Organization, Workspace, Membership, Invitation
  - PostgreSQL with `directory` schema
  - Initial EF Core migration

- **API Layer**
  - OrganizationsController (4 endpoints)
  - WorkspacesController (5 endpoints)
  - MembershipsController (5 endpoints)
  - InvitationsController (4 endpoints)
  - DTOs for all request/response models
  - ExceptionHandlingMiddleware (RFC 7807 ProblemDetails)

- **Tests**
  - Domain tests: 88 tests (Organization, Workspace, Membership, Invitation)

### Added - Claude Code Tooling
- **Agent**: `dotnet-backend` - .NET backend development subagent
- **Command**: `/commit` - Standardized git commit workflow
- **Hook**: `validate-commit.js` - Conventional Commits validation

### Infrastructure
- .gitignore fix (was missing dot prefix)
- EF Core Design package for migrations

---

## [0.1] - 2025-01-29

### Added
- Initial project structure
- CLAUDE.md with project guidelines
- README.md with project documentation
- Git workflow and versioning strategy
- Base folder structure (services, apps, packages, infrastructure, docs)

### Infrastructure
- .gitignore configured for .NET, Node.js, and IDE files

---

## Version Legend

- **Added** - New features
- **Changed** - Changes in existing functionality
- **Deprecated** - Soon-to-be removed features
- **Removed** - Removed features
- **Fixed** - Bug fixes
- **Security** - Vulnerability fixes
- **Infrastructure** - DevOps, CI/CD, tooling changes

---

## Services Status

| Service | Status | Version |
|---------|--------|---------|
| Directory | ✅ Complete | 0.2 |
| Gateway | ✅ Complete (YARP) | 0.10 |
| Authentication | ✅ Complete | 0.4 |
| Access Control | ✅ Complete | 0.5 |
| Audit | ✅ Complete | 0.6 |
| Structured Logging | ✅ All Services | 0.7 |
| Notification | ✅ Complete | 0.8 |
| Configuration | ✅ Complete | 0.9 |
| Admin API | ✅ Complete | 0.11 |
