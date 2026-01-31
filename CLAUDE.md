# DC Platform

Multi-tenant enterprise workflow automation platform. Foundation for Digital Control's productized solutions.

## Project Structure

```
dc-platform/
├── services/           # Backend microservices (.NET Core 10)
│   ├── gateway/        # API Gateway (YARP reverse proxy, port 5000)
│   ├── authentication/ # Auth + signup orchestration (port 5002)
│   ├── directory/      # Organization/Workspace/Membership (port 5001)
│   ├── access-control/ # RBAC/ABAC permissions (port 5003)
│   ├── audit/          # Immutable event log (port 5004)
│   ├── notification/   # Email/push delivery (port 5005)
│   ├── configuration/  # Tenant config, feature flags (port 5006)
│   └── admin-api/      # Admin aggregation layer (port 5007)
├── apps/               # Frontend applications (Vue.js 3 + Module Federation)
│   ├── shell/          # Microfrontend host (port 3000)
│   ├── admin/          # Admin remote (port 5173)
│   └── client/         # Client remote (port 5174)
├── packages/           # Shared libraries
│   ├── ui-kit/         # Shared Vue components (8 Dc* components)
│   ├── api-client/     # Typed API client functions per service
│   └── shared-types/   # Cross-service TypeScript types
├── e2e/                # Playwright E2E tests (19 tests)
├── infrastructure/     # Docker Compose, Dockerfiles, Keycloak config
│   └── docker-compose.yml  # Full platform (14 containers)
├── .claude/            # Claude Code infrastructure
│   ├── agents/         # 8 agent definitions
│   ├── skills/         # 7 technical skills
│   ├── commands/       # 3 slash commands (/commit, /learn, /claude-sync)
│   └── learnings/      # Captured solution patterns
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
- Keycloak 26.1 for identity (single realm `dc-platform`, tenant via user attributes)
- PostgreSQL 17 (shared instance, schema-per-service isolation)
- Docker Compose for local dev (14 containers: 3 infra + 8 backend + 3 frontend)
- Future: Redis for caching, RabbitMQ for async messaging

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
| `vue-testing` | Vue.js test code | Vitest + @vue/test-utils for components, stores, composables, API client |
| `e2e-testing` | E2E test code | Playwright browser tests, Page Object Model, auth flows, multi-app testing |
| `claude-architect` | Claude infrastructure | CLAUDE.md files, skills, agents, commands, hooks, learnings |
| `dc-platform-architect` | Architecture decisions | Service boundaries, API design, DB schemas, multi-tenancy, cross-service comms |

### Which Skill to Use

| Skill | File | When |
|-------|------|------|
| `structured-logging` | `.claude/skills/structured-logging/SKILL.md` | Adding Serilog to a service, configuring log levels, structured log patterns |
| `docker-compose` | `.claude/skills/docker-compose/SKILL.md` | Adding services to Docker Compose, Dockerfile patterns, container networking |
| `keycloak-integration` | `.claude/skills/keycloak-integration/SKILL.md` | JWT validation, auth middleware, token handling in .NET/Vue.js code |
| `keycloak-admin` | `.claude/skills/keycloak-admin/SKILL.md` | realm-export.json, client scopes, auth error fixes, JWT debugging |
| `troubleshooting` | `.claude/skills/troubleshooting/SKILL.md` | Test failures, cross-service debugging, correlation ID tracing, escalation decisions |
| `windows-dev` | `.claude/skills/windows-dev/SKILL.md` | Shell commands on Windows, Git Bash quirks, path translation, Docker exec, curl/JSON |
| `continuous-learning` | `.claude/skills/continuous-learning/SKILL.md` | Capturing non-trivial solution patterns, promoting learnings to skills |

### Which Command to Use

| Command | File | When |
|---------|------|------|
| `/commit` | `.claude/commands/commit.md` | Smart commit with tests, changelog, version bump, push |
| `/learn` | `.claude/commands/learn.md` | Capture a reusable pattern from the current session |
| `/claude-sync` | `.claude/commands/claude-sync.md` | Audit CLAUDE.md files, skills, agents for staleness |

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
  ├─ New service? → Use dc-platform-architect agent first, then dotnet-backend + docker-compose
  │
  ├─ Cross-service issue? → Follow troubleshooting skill (cross-service debugging)
  │
  ├─ Affects one service only? → Bug fix or refactor, just do it
  │
  ├─ Affects multiple services or architecture? → Use dc-platform-architect agent
  │
  ├─ API design or DB schema decision? → Use dc-platform-architect agent
  │
  ├─ Non-trivial fix worth remembering? → Use /learn command
  │
  └─ CLAUDE.md or skills seem stale? → Use /claude-sync command
```

### When NOT to Decide Alone

Escalate (ask the user or create an ADR) when:
- Changing service boundaries or API contracts
- Adding new infrastructure components (Redis, RabbitMQ)
- Modifying authentication/authorization flows
- Any breaking change to existing APIs
- Introducing cross-service synchronous dependencies

## Quick Start

```bash
# Start entire platform (all 14 containers)
pnpm docker:up

# Stop and clean up
pnpm docker:down
```

Open `http://localhost:3000` (shell app) after containers are healthy.

## Testing

### Unit Tests (204 tests)
```bash
# Backend (.NET)
dotnet test dc-platform.slnx --filter "Category!=Integration"

# Frontend (Vitest + @vue/test-utils)
pnpm test              # Run all 204 tests
pnpm test:watch        # Watch mode
pnpm test:coverage     # With coverage report
```

### E2E Tests (19 tests)
```bash
pnpm test:e2e          # Run Playwright tests (requires platform running)
pnpm test:e2e:ui       # Playwright UI mode
pnpm test:e2e:report   # View HTML report
```

E2E tests require the full platform running via `pnpm docker:up`.

## Commands

```bash
# Backend service (from service directory)
dotnet build
dotnet test
dotnet run

# Frontend (from root)
pnpm install           # Install all workspace dependencies
pnpm dev:all           # Start shell + admin + client dev servers
pnpm build:remotes     # Build admin + client for production

# Individual app dev servers
pnpm dev:shell         # Shell on port 3000
pnpm dev:admin         # Admin on port 5173
pnpm dev:client        # Client on port 5174

# Docker
pnpm docker:up         # Start full platform
pnpm docker:down       # Stop and remove containers
pnpm docker:logs       # Follow container logs
```
