# Changelog

All notable changes to DC Platform will be documented in this file.

Format: `[MAJOR.BUILD] - YYYY-MM-DD`

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
| Gateway | 🔲 Not started | - |
| Authentication | 🔲 Not started | - |
| Access Control | 🔲 Not started | - |
| Audit | 🔲 Not started | - |
| Notification | 🔲 Not started | - |
| Configuration | 🔲 Not started | - |
| Admin API | 🔲 Not started | - |
