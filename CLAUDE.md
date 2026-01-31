# DC Platform

Multi-tenant enterprise workflow automation platform. Foundation for Digital Control's productized solutions.

## Project Structure

```
dc-platform/
├── services/           # Backend microservices (.NET Core)
│   ├── gateway/        # API Gateway/BFF
│   ├── authentication/ # Keycloak integration
│   ├── directory/      # Organization/Workspace/Membership
│   ├── access-control/ # RBAC/ABAC permissions
│   ├── audit/          # Immutable event log
│   ├── notification/   # Email (ZeptoMail), future: SMS, push
│   ├── configuration/  # Tenant config, feature flags
│   └── admin-api/      # Admin orchestration layer
├── apps/               # Frontend applications
│   ├── shell/          # Microfrontend shell (Vue.js)
│   ├── admin/          # Admin UI modules
│   └── client/         # Client-facing modules
├── packages/           # Shared libraries
│   ├── ui-kit/         # Shared Vue components
│   ├── api-client/     # Generated API clients
│   └── shared-types/   # Cross-service TypeScript types
├── infrastructure/     # Docker, K8s, deployment configs
└── docs/               # Architecture documentation
```

## Tech Stack

### Backend (Default: .NET Core 10+)
- Clean Architecture per service (API → Application → Domain → Infrastructure)
- Entity Framework Core for data access
- PostgreSQL per service (schema isolation)
- MediatR for CQRS
- FluentValidation for input validation

### Frontend
- Vue.js 3 with Composition API
- Microfrontend architecture with Module Federation
- pnpm as package manager
- TypeScript strict mode

### Infrastructure
- Keycloak for identity (single realm, tenant via attributes)
- Redis for caching and session
- RabbitMQ for async messaging
- Docker Compose for local dev

## Git Workflow

### Branching
- `main` - production-ready, protected, auto-versioned
- `feature/<scope>/<description>` - new features
- `fix/<scope>/<description>` - bug fixes
- All changes via Pull Request with review

### Commit Messages (Conventional Commits)
Format: `type(scope): description`

Types:
- `feat` - new feature
- `fix` - bug fix
- `refactor` - code change without new feature or fix
- `docs` - documentation only
- `test` - adding tests
- `chore` - maintenance, dependencies

Scope: service name or `platform` for cross-cutting changes

Examples:
- `feat(directory): add workspace membership API`
- `fix(gateway): handle token refresh edge case`
- `docs(platform): update deployment guide`

### Versioning
Format: `MAJOR.BUILD` (e.g., `0.15`)

- **MAJOR**: Breaking changes, significant milestones (manual increment)
- **BUILD**: Auto-increments on every push to main

Current version tracked in `VERSION` file at repo root.
Git tags created automatically: `v0.15 - 2025-01-29`

### Version History
Maintained in `CHANGELOG.md` at repo root.

## Service Boundaries

Each service:
- Has its own database/schema - NEVER access another service's DB directly
- Communicates via REST or message queue
- Owns its domain entities completely
- Has own README.md and CLAUDE.md

## Coding Conventions

### Backend (.NET)
- Async/await for all I/O
- Constructor dependency injection only
- DTOs for API contracts, never expose domain entities
- English for code, comments, and documentation

### Frontend (Vue.js)
- Composition API with `<script setup>`
- TypeScript for all components
- Props validation required
- English for all code and UI strings (i18n/l10n added later)

## What Claude Should NOT Do

- Access or modify files outside the current service scope without explicit request
- Create direct database dependencies between services
- Commit directly to main branch
- Store secrets in code or config files
- Mix business logic across service boundaries
- Create "god services" that do everything

## Multi-tenancy

- Tenant isolation via `tenantId` on all data
- Keycloak user attribute for tenant association
- All queries MUST filter by tenant context
- Never expose data across tenant boundaries

## Decision Making

### Which Agent to Use

| Task | Agent | When |
|------|-------|------|
| `dotnet-backend` | .NET service code | Creating/modifying services, controllers, entities, EF Core, CQRS handlers |
| `dotnet-testing` | .NET test code | Writing unit/integration/E2E tests, test fixtures, Testcontainers setup |
| `vue-frontend` | Vue.js frontend code | Vue 3 components, composables, Tailwind CSS, microfrontend apps, API client, i18n |
| `python-backend` | Python service code | FastAPI services, Pydantic models, Python tooling |

### Which Skill to Use

| Skill | File | When |
|-------|------|------|
| `structured-logging` | `.claude/skills/structured-logging/SKILL.md` | Adding Serilog to a service, configuring log levels, structured log patterns |
| `docker-compose` | `.claude/skills/docker-compose/SKILL.md` | Adding services to Docker Compose, Dockerfile patterns, container networking |
| `keycloak-integration` | `.claude/skills/keycloak-integration/SKILL.md` | JWT validation, auth middleware, token handling in .NET/Vue.js code |
| `keycloak-admin` | `.claude/skills/keycloak-admin/SKILL.md` | realm-export.json, client scopes, auth error fixes, JWT debugging |
| `troubleshooting` | `.claude/skills/troubleshooting/SKILL.md` | Test failures, cross-service debugging, correlation ID tracing, escalation decisions |
| `windows-dev` | `.claude/skills/windows-dev/SKILL.md` | Shell commands on Windows, Git Bash quirks, path translation, Docker exec, curl/JSON |

### Decision Flow

```
Problem identified
  │
  ├─ Test failure? → Follow troubleshooting skill (Step 1→2→3 triage)
  │
  ├─ Frontend work (apps/, packages/)? → Use vue-frontend agent
  │
  ├─ New feature in existing service? → Use dotnet-backend agent
  │     └─ Then write tests → Use dotnet-testing agent
  │
  ├─ New service? → Use dotnet-backend agent + docker-compose skill
  │
  ├─ Cross-service issue? → Follow troubleshooting skill (cross-service debugging)
  │
  ├─ Affects one service only? → Bug fix or refactor, just do it
  │
  └─ Affects multiple services or architecture? → Create ADR in docs/adr/
```

### When NOT to Decide Alone

Escalate (ask the user or create an ADR) when:
- Changing service boundaries or API contracts
- Adding new infrastructure components (Redis, RabbitMQ)
- Modifying authentication/authorization flows
- Any breaking change to existing APIs
- Introducing cross-service synchronous dependencies

## Commands

```bash
# Backend service
dotnet build
dotnet test
dotnet run

# Frontend
pnpm install
pnpm dev
pnpm build
pnpm test
```
