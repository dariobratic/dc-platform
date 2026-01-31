# Shell App - Claude Instructions

## App Scope

The shell application is the **microfrontend host/container**. It owns:

- Authentication (Keycloak OIDC integration)
- Top-level routing and navigation
- Layout structure (sidebar, header, main content area)
- Tenant context management (organization, workspace)
- HTTP client configuration
- Auth guards and route protection

The shell does **NOT** own:

- Feature-specific pages (those live in remote apps)
- Business logic unrelated to auth/tenant/routing
- Domain-specific API calls (those belong in remotes or api-client package)

## Key Files Map

### Core Setup
- `src/main.ts` - App bootstrap, Pinia setup, router registration
- `src/App.vue` - Root component, dynamic layout switching
- `index.html` - HTML entry point
- `vite.config.ts` - Vite + Vue + Module Federation config

### Authentication
- `src/plugins/auth.ts` - oidc-client-ts UserManager setup
- `src/stores/auth.ts` - Auth state (Pinia): user, tokens, login/logout
- `src/pages/LoginPage.vue` - Triggers Keycloak redirect
- `src/pages/AuthCallback.vue` - Handles OAuth callback

### Tenant Context
- `src/stores/tenant.ts` - Organization/workspace state (Pinia)
- `src/pages/OrganizationPickerPage.vue` - Org selection after login

### HTTP Client
- `src/plugins/http.ts` - Axios instance with auth + tenant interceptors

### Routing
- `src/router/index.ts` - Vue Router with auth guards, route definitions

### Layout
- `src/layouts/DefaultLayout.vue` - Main layout (sidebar + header + content)
- `src/layouts/AuthLayout.vue` - Minimal layout (login, callback)
- `src/components/AppSidebar.vue` - Collapsible navigation sidebar
- `src/components/AppHeader.vue` - Org switcher, user menu
- `src/components/UserMenu.vue` - Profile dropdown with logout

### Pages
- `src/pages/DashboardPage.vue` - Main landing page after login
- `src/pages/SignupPage.vue` - Custom signup form with org creation
- `src/pages/AdminPlaceholder.vue` - Placeholder for admin remote
- `src/pages/ClientPlaceholder.vue` - Placeholder for client remote

### Types
- `src/types/index.ts` - App-level types (Organization, Workspace, etc.)
- `src/env.d.ts` - Vite environment variable types

### Styles
- `src/style.css` - Tailwind CSS imports, global styles

## Routes

| Path | Purpose | Auth Required | Org Required |
|------|---------|---------------|--------------|
| `/` | Root redirect | - | - |
| `/login` | Trigger Keycloak login | No | No |
| `/signup` | Custom signup flow | No | No |
| `/callback` | OAuth callback handler | No | No |
| `/select-organization` | Org picker | Yes | No |
| `/dashboard` | Main dashboard | Yes | Yes |
| `/admin/*` | Admin remote (future) | Yes | Yes |
| `/app/*` | Client remote (future) | Yes | Yes |

## Environment Variables

All env vars are prefixed with `VITE_` and defined in `.env.local` (gitignored):

- `VITE_API_BASE_URL` - Gateway API URL (default: http://localhost:5000)
- `VITE_KEYCLOAK_URL` - Keycloak server URL (default: http://localhost:8080)
- `VITE_KEYCLOAK_REALM` - Keycloak realm (default: dc-platform)
- `VITE_KEYCLOAK_CLIENT_ID` - Client ID (default: dc-platform-admin)

## Coding Patterns

### Component Pattern
```vue
<script setup lang="ts">
import { ref, computed } from 'vue'
import type { SomeType } from '@/types'

interface Props {
  id: string
  optional?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  optional: false,
})

const emit = defineEmits<{
  updated: [value: string]
}>()

const value = ref('')
const computed Value = computed(() => value.value.toUpperCase())
</script>

<template>
  <div class="p-4 bg-white rounded-lg">
    {{ computedValue }}
  </div>
</template>
```

### Composable Pattern
```typescript
// composables/useSomething.ts
import { ref, readonly } from 'vue'

export function useSomething() {
  const state = ref(null)
  const loading = ref(false)

  async function fetch() {
    loading.value = true
    try {
      // fetch logic
    } finally {
      loading.value = false
    }
  }

  return {
    state: readonly(state),
    loading: readonly(loading),
    fetch,
  }
}
```

### API Call Pattern
```typescript
import { http } from '@/plugins/http'
import type { SomeResponse } from '@/types'

async function fetchData(): Promise<SomeResponse> {
  const { data } = await http.get<SomeResponse>('/api/v1/endpoint')
  return data
}
```

### Store Pattern (Pinia)
```typescript
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useMyStore = defineStore('my-store', () => {
  const value = ref<string | null>(null)
  const hasValue = computed(() => !!value.value)

  function setValue(newValue: string) {
    value.value = newValue
  }

  return { value, hasValue, setValue }
})
```

## Boundaries

### Shell OWNS:
- Authentication state and flows
- Layout structure and navigation
- Tenant context (org/workspace selection)
- Route guards and protection
- HTTP client with tenant headers
- Top-level error handling (401, 403)

### Shell DOES NOT OWN:
- Feature pages (belong in remotes)
- Domain logic (belongs in remotes or services)
- Feature-specific API calls (use api-client package)
- Business rules validation
- Domain entities beyond Organization/Workspace

### Remote Apps (admin, client) OWN:
- Their pages and components
- Their API client calls (using shared http instance)
- Their domain logic and business rules
- Their feature-specific routes (exposed to shell)

### Shared Packages (future) OWN:
- `ui-kit`: Reusable Vue components (buttons, forms, modals)
- `api-client`: Typed API client functions per service
- `shared-types`: Cross-service TypeScript types

## Module Federation Notes

The shell exposes these to remotes via shared deps:
- `vue` (singleton)
- `vue-router` (singleton)
- `pinia` (singleton)

Remotes must:
- Export a `./routes` module returning RouteRecordRaw[]
- Use the same versions of shared deps
- NOT bundle shared deps (marked as external)

## What Claude Should Do

When modifying the shell app:
- Add new routes to `src/router/index.ts`
- Add layout components to `src/layouts/`
- Add navigation items to `AppSidebar.vue`
- Extend auth store for new auth-related state
- Extend tenant store for tenant-related state
- Add interceptors to HTTP client if needed
- Create reusable components in `src/components/`
- Add app-level types to `src/types/index.ts`
- Update README.md when adding features

## What Claude Should NOT Do

- Add feature-specific business logic to the shell
- Create domain-specific API calls here (use api-client package)
- Add heavy dependencies that would bloat the shell
- Bypass auth guards or tenant context
- Store secrets in code or config
- Create tight coupling to remote apps
- Mix authentication and domain logic

## Testing Strategy (Future)

Unit tests:
- Stores (auth, tenant)
- Router guards
- HTTP interceptors
- Utility functions

Integration tests:
- Auth flow (login → callback → org picker → dashboard)
- Route protection
- Tenant header injection

E2E tests (with backend):
- Full login flow with real Keycloak
- Organization selection
- Navigation between routes
- Token refresh
- Logout

## Decision Making

When adding features, ask:

1. Does this belong in the shell or a remote?
   - If it's about auth, routing, layout, tenant → shell
   - If it's a feature page or domain logic → remote

2. Does this need a new dependency?
   - Check if it's already in package.json
   - Consider if it should be in a shared package instead

3. Does this affect remote apps?
   - Update Module Federation config if needed
   - Document in README if it changes the contract

4. Is this environment-specific?
   - Add to .env.example and .env.local
   - Update env.d.ts with types

When in doubt, ask the user before making architectural changes.
