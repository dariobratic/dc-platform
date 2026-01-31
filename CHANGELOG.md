# Changelog

All notable changes to DC Platform will be documented in this file.

Format: `[MAJOR.BUILD] - YYYY-MM-DD`

---

## [0.34] - 2026-01-31

### Added
- **dc-platform-architect agent** — Domain expert for architecture decisions: service boundaries, API design, DB schemas, multi-tenancy, cross-service communication, and security patterns

---

## [0.33] - 2026-01-31

### Fixed
- **Root CLAUDE.md** — Added missing `vue-testing` and `e2e-testing` agents to decision table
- **Shell CLAUDE.md** — Added missing `SignupPage.vue` to Pages section and `/signup` route to Routes table
- **Admin CLAUDE.md** — Added missing `WorkspaceDetailPage.vue` to Pages section

---

## [0.32] - 2026-01-31

### Added
- **claude-architect agent** — Meta-agent for managing CLAUDE.md files, skills, agents, commands, hooks, and learnings
- **continuous-learning skill** — Pattern capture and knowledge retention with promotion criteria
- **/learn command** — Capture reusable patterns from current session into `.claude/learnings/`
- **/claude-sync command** — Audit all CLAUDE.md files, skills, and agents for staleness and consistency
- **Skill cross-references** — Added `## Related Skills` sections to all 6 existing skills with bidirectional links
- **Command reference table** — Added "Which Command to Use" section to root CLAUDE.md decision flow

---

## [0.31] - 2026-01-31

### Added
- **Workspace detail page** — New admin page (`/admin/workspaces/:id`) with overview, members management, and settings tabs (rename, delete)
- **Organization member management** — Full members tab in org detail with collapsible workspace sections, add/remove members, and role changes
- **Workspace switcher** — Header dropdown to switch workspace context, fetches workspaces for current org
- **Organization inline edit** — Edit organization name directly from the overview tab
- **Workspace delete** — Delete button on workspace cards in org detail with confirmation dialog

### Fixed
- **Signup dashboard display** — Pass organization object to tenant store after signup so dashboard shows actual org name instead of "Unknown"
- **Role badge colors** — Client workspace members tab now uses role-dependent badge colors (Owner/Admin/Member/Viewer)

### Changed
- **Tenant store** — Added `currentWorkspace` state and updated `setWorkspace` to accept optional workspace object
- **Gitignore** — Added `tmp_*.json` pattern to exclude temporary API debugging files

---

## [0.30] - 2026-01-31

### Fixed
- **Auth API error handling** — AuthController SignIn now catches HTTP 401 from Keycloak (not just 400) for invalid ROPC credentials
- **Keycloak user update** — KeycloakService UpdateUserAttributesAsync sends full user representation to prevent Keycloak from resetting emailVerified
- **HTTP interceptor** — Shell app skips auth token injection and 401 auto-logout for public auth endpoints (signin, signup, token, refresh), preventing signinSilent() from hanging
- **Logout flow** — Shell auth store uses removeUser() with backend token revocation instead of signoutRedirect() which fails with ROPC flow
- **DcInput accessibility** — Added aria-hidden to required asterisk span so Playwright getByLabel matches correctly
- **Database auto-migration** — Added MigrateAsync() on startup for directory, access-control, audit, and configuration services
- **Keycloak realm config** — Added service account roles (manage-users, view-users), user profile attributes (organizationId, tenantId), disabled brute force protection for test stability
- **E2E test stability** — Fixed auth setup, signout, signin, signup tests to handle org picker auto-select, sessionStorage limitations, and parallel execution

---

## [0.29] - 2026-01-31

### Added
- **Frontend Dockerfiles** — Multi-stage builds (node:20-alpine → nginx:alpine) for shell, admin, and client apps
  - `apps/shell/Dockerfile` — Shell host with VITE_API_BASE_URL, VITE_KEYCLOAK_URL, VITE_KEYCLOAK_REALM, VITE_KEYCLOAK_CLIENT_ID build args
  - `apps/admin/Dockerfile` — Admin remote with VITE_API_BASE_URL build arg
  - `apps/client/Dockerfile` — Client remote with VITE_API_BASE_URL, VITE_KEYCLOAK_URL, VITE_KEYCLOAK_REALM build args
  - Shared nginx SPA config (`infrastructure/nginx/spa.conf`) with try_files fallback and CORS headers for Module Federation
  - Root `.dockerignore` optimized for frontend builds
- **Docker Compose frontend services** — Added shell (port 3000), admin (port 5173), client (port 5174) to `infrastructure/docker-compose.yml` with health checks and depends_on ordering
- **E2E testing infrastructure** — Playwright setup with Page Object Model, auth fixtures, and 19 auth flow tests
  - `e2e/` workspace package with Playwright config (chromium, screenshots on failure)
  - Page Objects: LoginPage, SignupPage, OrganizationPickerPage, DashboardPage
  - Tests: signup flow, signin flow, signout flow, invalid credentials, protected routes
  - Auth fixture for reusing authenticated state across tests
- **Docker convenience scripts** — `docker:up`, `docker:down`, `docker:logs` in root package.json
- **E2E convenience scripts** — `test:e2e`, `test:e2e:ui`, `test:e2e:report` in root package.json

---

## [0.28] - 2026-01-30

### Added
- **E2E testing agent** (`.claude/agents/e2e-testing.md`) — Playwright browser automation agent for end-to-end testing with Page Object Model, auth fixtures, multi-browser support, and DC Platform-specific patterns (shell + remote apps, Keycloak auth, tenant context)
- **Vue testing agent** (`.claude/agents/vue-testing.md`) — Vitest + @vue/test-utils agent for unit and component testing across all 5 frontend projects with mocking patterns, mountPage helper, and store/composable/page testing conventions

---

## [0.27] - 2026-01-30

### Added
- **Custom signup and signin flow** — Replace Keycloak hosted login pages with custom forms
  - `POST /api/v1/auth/signin` — ROPC authentication with email/password via `dc-platform-admin` public client
  - `POST /api/v1/auth/signup` — Full orchestration: create Keycloak user, ROPC login, create org + workspace + membership via Directory, update user attributes, return fresh tokens
  - Auth service `DirectoryService` — HTTP client calling Directory at configurable `Services:DirectoryUrl` (avoids gateway circular dependency)
  - Auth service `KeycloakService` — `CreateUserAsync`, `AuthenticateWithPasswordAsync`, `UpdateUserAttributesAsync` with service account admin token
  - Auth route changed from `api/auth` to `api/v1/auth` to align with Gateway YARP pattern
  - Keycloak `dc-platform-admin` client: enabled ROPC (`directAccessGrantsEnabled`), added `tenant-id`, `organization-id`, `realm-roles` protocol mappers
  - Shell `LoginPage.vue` — Custom email/password form with error handling (401, 429)
  - Shell `SignupPage.vue` — Registration form with client-side validation, org slug preview, tenant context auto-setup
  - Shell auth store — `storeCustomTokens` (oidc-client-ts sessionStorage compatibility), `loginWithCredentials`, `signupUser`
  - Shell router guard — redirects to `/login` page instead of Keycloak hosted login
  - Shared types: `SignupRequest`, `SigninRequest`, `SignupResponse`; api-client: `signup()`, `signin()` functions
  - All existing api-client auth paths updated from `/api/auth/*` to `/api/v1/auth/*`

---

## [0.26] - 2026-01-30

### Added
- **Frontend testing infrastructure** — Vitest + @vue/test-utils across all 5 frontend projects (204 tests, 29 test files)
  - `packages/ui-kit` — 8 component specs (DcButton, DcInput, DcSelect, DcModal, DcAlert, DcBadge, DcCard, DcSpinner) testing variants, sizes, props, emits, slots, disabled/loading states
  - `packages/api-client` — client factory spec + 7 service specs testing HTTP methods, URLs, interceptors (auth, tenant headers, 401 handling)
  - `apps/shell` — auth store (14 tests), tenant store (10 tests), router guards (5 tests) with oidc-client-ts and Module Federation mocking
  - `apps/admin` — useApiClient composable + 5 page specs (Dashboard, Organizations, OrganizationDetail, Roles, AuditLog) with loading/data/error states
  - `apps/client` — useApiClient composable + 3 page specs (Dashboard, Workspace, Profile) with OIDC sessionStorage bridge testing
  - Root `vitest.workspace.ts` orchestrating all projects, per-project `vitest.config.ts` with `@vitejs/plugin-vue` (isolated from Module Federation)
  - `happy-dom` environment for DOM testing, `node` environment for api-client
  - `mountPage()` test helper pattern with `createTestingPinia()` and component stubs
- **Health check endpoints** — added `/health` endpoint to Access Control, Audit, Configuration, and Directory services
- **Gateway routes** — added `/api/v1/users/*` route (Directory) and `/api/v1/admin/*` route (Admin API) with cluster config
- **Current user organizations endpoint** (`GET /api/v1/memberships/users/me/organizations`) — returns organizations from JWT token `organization_id` claim

### Fixed
- **Keycloak health check** — changed Docker Compose health check port from 8080 to 9000 (management port) and simplified grep pattern

---

## [0.25] - 2026-01-30

### Fixed
- **Docker build failures** — aligned `Microsoft.AspNetCore.OpenApi` to `10.0.0` across all 8 services (was mixed `10.0.0`/`10.0.1`/`10.0.2`)
- **EF Core Design version mismatch** — aligned `Microsoft.EntityFrameworkCore.Design` to `10.0.0` in Directory.API and AccessControl.API
- **Docker health checks** — installed `curl` in all Dockerfile `base` stages (`aspnet:10.0` image lacks it), fixing `service_healthy` dependency chain
- **NETSDK1064 restore/publish mismatch** — added `.dockerignore` to all 8 services excluding `**/bin/`, `**/obj/`, `**/.vs/` to prevent stale host `project.assets.json` from overriding Docker-restored packages

### Changed
- **Troubleshooting skill** — added Docker Build Troubleshooting section with common failures, package version rules, `.dockerignore` requirements, and debug commands

---

## [0.24] - 2026-01-30

### Added
- **Module Federation integration** — shell dynamically loads remote routes from admin and client apps
  - Shell router loads `admin/routes` and `client/routes` via `@originjs/vite-plugin-federation` at startup
  - `RemoteRouterView` wrapper component with `<router-view>` for remote route mounting
  - `loadRemoteRoutes()` in shell main.ts with graceful fallback to placeholder pages if remotes unavailable
  - TypeScript module declarations for `admin/routes` and `client/routes` federation imports
  - Singleton shared dependency config (`vue`, `vue-router`, `pinia`) across all three apps to prevent duplicate instances
  - Preview port config on admin (5173) and client (5174) for federation dev workflow
  - Root scripts: `build:remotes` (build admin + client), `dev:all` (parallel dev all three apps)

---

## [0.23] - 2025-01-30

### Added
- **Client microfrontend** (`apps/client`) — Module Federation remote for end-user features
  - Module Federation remote config exposing `./routes` (port 5174, shared vue/router/pinia)
  - `DashboardPage` — welcome greeting, quick actions grid, workspace cards, recent activity feed
  - `WorkspacePage` — single workspace with tabs (overview details, members table with role badges)
  - `ProfilePage` — OIDC user info display with Keycloak account link for password change
  - `NotificationsPage` — placeholder for future notification inbox
  - `InvitationsPage` — placeholder for future invitation management
  - Shared components: PageHeader, EmptyState, ActivityFeed, QuickActions, WorkspaceCard, InvitationCard
  - OIDC sessionStorage bridge for user profile data and workspace context awareness

---

## [0.22] - 2025-01-30

### Added
- **Admin microfrontend** (`apps/admin`) — Module Federation remote for platform administration
  - Module Federation remote config exposing `./routes` (port 5173, shared vue/router/pinia)
  - `DashboardPage` — stats overview with org count, audit events, recent activity
  - `OrganizationsPage` — list, search, create, delete organizations with modal dialogs
  - `OrganizationDetailPage` — single org view with tabs (overview, workspaces, members)
  - `WorkspacesPage` — cross-org workspace listing with organization filter
  - `UsersPage` — user management placeholder (coming soon)
  - `RolesPage` — role CRUD with permission tag input, system role protection
  - `AuditLogPage` — searchable audit entries with 6 filters and pagination
  - Shared components: AdminBreadcrumb, PageHeader, StatsCard, EmptyState, ConfirmDialog, StatusBadge
  - API client composable with oidc-client-ts token bridge and tenant context
  - Uses `@dc-platform/ui-kit` for all base UI and `@dc-platform/api-client` for all API calls

---

## [0.21] - 2025-01-30

### Added
- **UI kit package** (`packages/ui-kit`, `@dc-platform/ui-kit`) — shared Vue 3 component library with Tailwind CSS
  - `DcButton` — primary, secondary, danger, ghost variants with sm/md/lg sizes and loading state
  - `DcInput` — text input with v-model, label, error/success states, helper text
  - `DcSelect` — native select with typed options array, validation states
  - `DcModal` — teleport-based dialog with backdrop, transitions, Escape key, persistent mode
  - `DcSpinner` — animated SVG loading indicator with sizes and custom color
  - `DcAlert` — success/error/warning/info alerts with SVG icons, dismissible option
  - `DcCard` — container with optional header/footer slots
  - `DcBadge` — status indicator with 5 variants, 2 sizes, optional dot

---

## [0.20] - 2025-01-30

### Added
- **API client package** (`packages/api-client`, `@dc-platform/api-client`) — typed HTTP client for all backend microservices
  - Axios-based client factory with Bearer token, tenant context headers, and 401 handling interceptors
  - `services/directory.ts` — 18 functions for organizations, workspaces, memberships, invitations
  - `services/access-control.ts` — 9 functions for roles, role assignments, permission checks
  - `services/audit.ts` — 4 functions for audit entries and entity history queries
  - `services/auth.ts` — 4 functions for token exchange, refresh, userinfo, logout
  - `services/notification.ts` — 2 functions for email and push notifications
  - `services/configuration.ts` — 4 functions for org settings and feature flags
  - `services/admin.ts` — 4 functions for dashboard, system health, summaries
  - Subpath exports for per-service imports (`@dc-platform/api-client/services/*`)

---

## [0.19] - 2025-01-30

### Added
- **Shared types package** (`packages/shared-types`, `@dc-platform/shared-types`) — cross-app TypeScript type definitions matching backend .NET DTOs
  - `auth.ts` — TokenRequest/Response, UserInfoResponse, AuthState
  - `tenant.ts` — Organization, Workspace, Membership, Invitation with status/role enums
  - `api.ts` — PagedResponse\<T\>, ApiError, ApiResult\<T\>
  - `directory.ts` — Create/Update request and response DTOs for Directory service
  - `access-control.ts` — Role, RoleAssignment, PermissionCheck, ScopeType
  - `audit.ts` — AuditEntryResponse, AuditFilter
  - `notification.ts` — Email and push notification request/response types
  - `configuration.ts` — ConfigurationResponse, FeatureFlagResponse
  - `admin.ts` — DashboardResponse, SystemHealthResponse, summaries
  - Subpath exports for per-module imports (`@dc-platform/shared-types/tenant`, etc.)

---

## [0.18] - 2025-01-30

### Added
- **Shell microfrontend app** (`apps/shell`) — Vue 3 host application with Keycloak OIDC, tenant context, and Module Federation
  - Keycloak PKCE authentication via oidc-client-ts (dc-platform-admin public client)
  - Post-login organization picker (auto-select single org, picker for multiple)
  - Axios HTTP client with Bearer token, X-Organization-Id, X-Workspace-Id interceptors
  - Collapsible sidebar layout, header with org display and user menu
  - Vue Router with auth guards and organization requirement checks
  - Module Federation host config for admin (port 5173) and client (port 5174) remotes
  - Placeholder pages for dashboard, admin module, client module
- **pnpm monorepo setup** — root package.json and pnpm-workspace.yaml for apps/* and packages/*
- **Keycloak admin skill** (`.claude/skills/keycloak-admin/SKILL.md`) — realm-export.json configuration, client scopes, protocol mappers, common error fixes, JWT debugging

### Fixed
- **Keycloak realm-export.json** — added client scope definitions (openid, profile, email, roles, web-origins) and defaultClientScopes to both clients, fixing "Invalid scopes" error
- **Keycloak integration skill** — updated to cover Vue.js frontend patterns alongside .NET, added cross-reference to keycloak-admin skill

### Changed
- **`.gitignore`** — removed `**/packages/` NuGet pattern that would conflict with frontend packages/ directory

---

## [0.17] - 2025-01-30

### Added
- **Vue.js Frontend Agent** (`.claude/agents/vue-frontend.md`) — development agent for Vue 3 microfrontend apps
  - Vue 3 Composition API with `<script setup>`, TypeScript strict mode, Tailwind CSS
  - Microfrontend architecture patterns (Module Federation via vite-plugin-federation)
  - Project structure: apps/shell (host), apps/admin, apps/client (remotes), packages/ui-kit
  - API integration patterns with Gateway (port 5000), typed Axios client with interceptors
  - Keycloak OIDC authentication (PKCE flow, oidc-client-ts, auth store)
  - Tenant context via X-Organization-Id / X-Workspace-Id headers
  - Pinia state management, vue-router with auth guards
  - Coding rules, component/composable patterns, environment variables
- **CLAUDE.md** — added `vue-frontend` agent to decision table and decision flow

---

## [0.16] - 2025-01-30

### Fixed
- **Serilog ExpressionTemplate** — replaced invalid `@x?.GetType().Name` syntax with `@x` across all 8 service appsettings.json files
- **Enum JSON serialization** — added `JsonStringEnumConverter` to Directory and AccessControl API to restore string enum responses after .NET 10 default change
- **EF Core child entity inserts** — added `ValueGeneratedNever()` to Membership, Invitation, and Permission configurations to fix `DbUpdateConcurrencyException` on new child entities
- **Repository change tracking** — removed redundant `_context.Entity.Update()` calls in WorkspaceRepository and RoleRepository that overrode EF Core Added state
- **Duplicate member conflict** — added application-level duplicate check in AddMemberHandler returning HTTP 409 instead of 422

### Changed
- **`/commit` command** — upgraded to parameterless smart workflow: auto-runs unit tests, generates commit message, updates changelog/version, and pushes
- **Removed `validate-commit.js` hook** — commit format validation now built into `/commit` command
- **`.gitignore`** — updated log exclusion patterns to catch test-generated log files

---

## [0.15] - 2025-01-30

### Added - Root Solution & Housekeeping
- **Root solution file** (`dc-platform.slnx`) — includes all 25 projects (17 service + 8 test) organized into solution folders by service
- Solution folders mirror service structure: `services/{name}` for source, `services/{name}/tests` for tests

### Fixed
- **Docker Compose skill** — Dockerfile template updated from `dotnet:9.0` to `dotnet:10.0` base images
- **Directory CLAUDE.md** — corrected stale `Directory.sln` reference to `Directory.slnx`

---

## [0.14] - 2025-01-30

### Added - Expanded Test Coverage Across Services
- **AccessControl.API.Tests** — 18 integration tests via Testcontainers PostgreSQL
  - `RoleEndpointTests` (11 tests): Create (valid, with permissions, empty name, duplicate name in scope), Get (exists, not found), Get by scope, Update (valid, not found), Delete (exists, not found)
  - `RoleAssignmentEndpointTests` (3 tests): Assign role, duplicate assignment conflict, revoke role
  - `PermissionEndpointTests` (4 tests): Check permission (has/lacks), get user permissions, empty permissions
  - Test infrastructure: IntegrationTestFixture with mock IPublisher, IntegrationTestCollection, HttpResponseExtensions

- **Audit.API.Tests** — 14 integration tests via Testcontainers PostgreSQL
  - `AuditEntryEndpointTests` (14 tests): Create (all fields, required only), validation (empty action, entityType, serviceName), Get by ID (exists, not found), paginated query, filters (organizationId, userId, entityType, serviceName), entity history, immutability (PUT/DELETE return 405)
  - Test infrastructure: IntegrationTestFixture, IntegrationTestCollection, HttpResponseExtensions

- **AdminApi.Tests** — 17 unit tests with NSubstitute mocks
  - `AdminControllerTests` (7 tests): Dashboard metrics, system health (all healthy, degraded), organizations list, recent audit, health endpoint, graceful degradation
  - `DirectoryServiceClientTests` (3 tests): Get organizations, service unavailable fallback, count
  - `AuditServiceClientTests` (4 tests): Get entries, unavailable fallback, count success, count unavailable
  - `ServiceHealthCheckerTests` (3 tests): All healthy, one down (degraded), all unreachable
  - MockHttpMessageHandler for typed HttpClient testing

- **Directory.Application.Tests** — 31 unit tests for CQRS handlers and validators
  - `CreateOrganizationHandlerTests` (4 tests): Valid creation, with settings, duplicate slug conflict, response mapping
  - `UpdateOrganizationHandlerTests` (3 tests): Update and save, update with settings, not found
  - `DeleteOrganizationHandlerTests` (2 tests): Delete sets status, not found
  - `GetOrganizationByIdHandlerTests` (2 tests): Existing returns response, not found
  - `CreateOrganizationValidatorTests` (15 tests): Valid command, name validation, slug validation (empty, short, uppercase, spaces, special chars, leading/trailing hyphens)
  - `UpdateOrganizationValidatorTests` (5 tests): Valid, empty ID, name validation

### Test Coverage Summary
| Project | Type | Tests | Status |
|---------|------|-------|--------|
| AccessControl.API.Tests | Integration (Testcontainers) | 18 | Build ✅ (requires Docker) |
| Audit.API.Tests | Integration (Testcontainers) | 14 | Build ✅ (requires Docker) |
| AdminApi.Tests | Unit (NSubstitute) | 17 | Pass ✅ |
| Directory.Application.Tests | Unit (NSubstitute) | 31 | Pass ✅ |
| **Total new tests** | | **80** | |

---

## [0.13] - 2025-01-30

### Added - Integration Tests & Testing Agent
- **Testing Agent** (`.claude/agents/dotnet-testing.md`)
  - xUnit + FluentAssertions + NSubstitute + Testcontainers stack
  - Test categories: Unit, Integration, E2E
  - Naming convention: `MethodName_Scenario_ExpectedResult`
  - WebApplicationFactory + Testcontainers fixture patterns
  - Assertion and NSubstitute patterns reference

- **Directory Integration Tests** — 36 tests via real PostgreSQL 17 (Testcontainers)
  - `OrganizationEndpointTests` (11 tests): Create (valid, settings, duplicate slug, empty name, invalid slug), Get (exists, not found), Update (valid, settings, not found), Delete (exists, verify soft delete, not found)
  - `WorkspaceEndpointTests` (9 tests): Create (valid, duplicate slug same org, same slug different orgs), Get (exists, not found), List, Update, Delete (exists, verify soft delete)
  - `MembershipEndpointTests` (11 tests): Add (valid, owner role, duplicate), List, Change role (Admin, Owner), Remove (exists, verify removal), User memberships across workspaces, full add→change→remove E2E flow
  - **No InMemory provider** — all tests use real PostgreSQL via Testcontainers
  - Shared container per xUnit collection for performance

- **Test Infrastructure**
  - `IntegrationTestFixture` — PostgreSQL container + WebApplicationFactory + auto-migration
  - `IntegrationTestCollection` — xUnit shared fixture collection
  - `HttpResponseExtensions` — `ReadAsAsync<T>()` deserialization helper
  - `Testcontainers.PostgreSql` 4.4.0 added to test project
  - `InternalsVisibleTo` on Directory.API for Program class access

---

## [0.12] - 2025-01-30

### Added - Docker Compose Infrastructure
- **docker-compose.yml** — Full local development environment with 10 containers
  - 8 application services (Gateway, Directory, Authentication, Access Control, Audit, Notification, Configuration, Admin API)
  - PostgreSQL 17 with schema-per-service isolation
  - Keycloak 26.1 with realm auto-import
- **Dockerfiles** — Multi-stage .NET 10.0 builds for all 8 services
  - Optimized layer caching (restore → build → publish)
  - Clean Architecture services copy all project layers for restore
- **Keycloak Realm Import** (`infrastructure/keycloak/realm-export.json`)
  - `dc-platform` realm with brute-force protection
  - `dc-platform-gateway` confidential client (auth code + service accounts)
  - `dc-platform-admin` public SPA client (PKCE with S256)
  - Protocol mappers: `tenant_id`, `organization_id`, `roles`
  - Realm roles: `platform-admin`, `org-admin`, `org-member`
- **Schema initialization** (`infrastructure/init-schemas.sql`) — creates `directory`, `access_control`, `audit`, `configuration`, `keycloak` schemas
- **Environment configuration** (`.env.example`) — template for PostgreSQL and Keycloak credentials
- Health checks on all containers with `depends_on: condition: service_healthy`
- Gateway YARP clusters overridden to Docker service names
- Admin API service URLs overridden to Docker service names

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
