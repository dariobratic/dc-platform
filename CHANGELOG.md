# Changelog

All notable changes to DC Platform will be documented in this file.

Format: `[MAJOR.BUILD] - YYYY-MM-DD`

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
| Gateway | ✅ Complete | 0.3 |
| Authentication | ✅ Complete | 0.4 |
| Access Control | ✅ Complete | 0.5 |
| Audit | 🔲 Not started | - |
| Notification | 🔲 Not started | - |
| Configuration | 🔲 Not started | - |
| Admin API | 🔲 Not started | - |
