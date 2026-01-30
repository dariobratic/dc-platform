# DC Platform Shell

The shell application serves as the microfrontend container host for DC Platform. It provides authentication, tenant context management, layout, and routing infrastructure for remote microfrontend modules.

## Architecture

This app uses Module Federation to load remote microfrontends dynamically:

- **Shell (Host)**: Runs on port 3000, provides auth, layout, routing
- **Admin Remote**: Expected on port 5173 (not yet implemented)
- **Client Remote**: Expected on port 5174 (not yet implemented)

## Tech Stack

- Vue.js 3 with Composition API (`<script setup>`)
- TypeScript (strict mode)
- Tailwind CSS
- Vue Router 4
- Pinia (state management)
- Axios (HTTP client)
- oidc-client-ts (Keycloak OIDC)
- Vite with Module Federation plugin

## Prerequisites

- Node.js 20+
- pnpm 9+
- Running backend services (Gateway on localhost:5000)
- Running Keycloak (localhost:8080)

## Setup

1. Install dependencies from monorepo root:
   ```bash
   cd /c/Projects/dc-platform
   pnpm install
   ```

2. Configure environment variables:
   ```bash
   cd apps/shell
   cp .env.example .env.local
   # Edit .env.local if needed (defaults work for local dev)
   ```

3. Start the development server:
   ```bash
   pnpm dev
   # or from root: pnpm dev:shell
   ```

4. Open http://localhost:3000

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `VITE_API_BASE_URL` | Gateway API base URL | `http://localhost:5000` |
| `VITE_KEYCLOAK_URL` | Keycloak server URL | `http://localhost:8080` |
| `VITE_KEYCLOAK_REALM` | Keycloak realm name | `dc-platform` |
| `VITE_KEYCLOAK_CLIENT_ID` | Keycloak client ID | `dc-platform-admin` |

## Available Scripts

```bash
pnpm dev          # Start dev server (port 3000)
pnpm build        # Build for production
pnpm preview      # Preview production build
pnpm type-check   # Run TypeScript type checking
pnpm lint         # Run ESLint
```

## Project Structure

```
src/
├── App.vue               # Root component with layout routing
├── main.ts               # App entry point
├── style.css             # Global styles (Tailwind imports)
├── env.d.ts              # Vite env type declarations
├── router/
│   └── index.ts          # Vue Router config with auth guards
├── layouts/
│   ├── DefaultLayout.vue # Main layout (sidebar + header)
│   └── AuthLayout.vue    # Minimal layout for login/callback
├── pages/
│   ├── LoginPage.vue                # Redirects to Keycloak
│   ├── AuthCallback.vue             # OAuth callback handler
│   ├── OrganizationPickerPage.vue   # Org selection after login
│   ├── DashboardPage.vue            # Main dashboard
│   ├── AdminPlaceholder.vue         # Placeholder for admin remote
│   └── ClientPlaceholder.vue        # Placeholder for client remote
├── components/
│   ├── AppSidebar.vue    # Navigation sidebar
│   ├── AppHeader.vue     # Header with org switcher
│   └── UserMenu.vue      # User profile dropdown
├── stores/
│   ├── auth.ts           # Authentication state (Pinia)
│   └── tenant.ts         # Tenant/org context (Pinia)
├── plugins/
│   ├── auth.ts           # oidc-client-ts UserManager setup
│   └── http.ts           # Axios instance with interceptors
└── types/
    └── index.ts          # App-level TypeScript types
```

## Authentication Flow

1. User navigates to `/` → redirected to `/login`
2. LoginPage triggers Keycloak redirect (Authorization Code + PKCE)
3. User authenticates in Keycloak
4. Keycloak redirects to `/callback`
5. AuthCallback handles OAuth callback, stores session
6. User redirected to OrganizationPickerPage
7. If 1 org: auto-select and go to dashboard
8. If multiple orgs: show picker UI
9. Selected org stored in tenant store
10. User navigated to dashboard or intended route

## Tenant Context

All API requests include these headers (auto-injected by HTTP client):

- `Authorization: Bearer {access_token}`
- `X-Organization-Id: {organizationId}`
- `X-Workspace-Id: {workspaceId}` (when set)

Tenant context is managed by the `useTenantStore` Pinia store.

## Routing

| Path | Component | Auth | Org Required |
|------|-----------|------|--------------|
| `/` | Redirect to `/dashboard` or `/login` | - | - |
| `/login` | LoginPage | No | No |
| `/callback` | AuthCallback | No | No |
| `/select-organization` | OrganizationPickerPage | Yes | No |
| `/dashboard` | DashboardPage | Yes | Yes |
| `/admin/*` | AdminPlaceholder | Yes | Yes |
| `/app/*` | ClientPlaceholder | Yes | Yes |

## Module Federation

The shell is configured to load remotes:

```typescript
remotes: {
  admin: 'http://localhost:5173/assets/remoteEntry.js',
  client: 'http://localhost:5174/assets/remoteEntry.js',
}
```

Shared dependencies: `vue`, `vue-router`, `pinia`

## Coding Standards

- Always use `<script setup lang="ts">`
- Props via `defineProps<T>()`
- Emits via `defineEmits<T>()`
- Composables for reusable logic
- Tailwind CSS only (no `<style>` blocks)
- English for all code and UI strings
- Named exports (except Vue SFC files)
- No `any` types

## Next Steps

- Implement admin remote app
- Implement client remote app
- Create shared UI component library (packages/ui-kit)
- Create API client package (packages/api-client)
- Add comprehensive error handling
- Add loading states and animations
- Implement workspace switching
- Add breadcrumb navigation
