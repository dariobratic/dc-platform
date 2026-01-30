# Client App - Claude Instructions

Client-facing microfrontend for DC Platform. Module Federation remote exposing client routes to shell host.

## App Scope

**This app owns:**
- Client dashboard (workspace overview)
- Workspace detail view
- User profile and preferences
- Notifications page (placeholder for future)
- Invitations page (placeholder for future)
- End-user focused features

**This app does NOT own:**
- Authentication (shell owns)
- Tenant context management (shell owns)
- HTTP client configuration (api-client package)
- Reusable UI components (ui-kit package)
- Layout/navigation shell (shell owns)
- Admin features (admin app owns)

## Key Files Map

### Core Setup
- `src/main.ts` - Standalone dev mode bootstrap
- `src/App.vue` - Root component (standalone mode)
- `src/routes.ts` - **EXPOSED VIA MODULE FEDERATION** - Route definitions
- `vite.config.ts` - Module Federation remote config (exposes `./routes`)
- `index.html` - HTML entry (standalone mode only)

### Composables
- `src/composables/useApiClient.ts` - API client singleton with auth/tenant interceptors

### Components
- `src/components/PageHeader.vue` - Page title + action slot (no breadcrumb)
- `src/components/EmptyState.vue` - Empty state placeholder with action slot
- `src/components/ActivityFeed.vue` - Recent activity list from audit entries
- `src/components/QuickActions.vue` - Quick action buttons grid
- `src/components/WorkspaceCard.vue` - Workspace summary card with navigation
- `src/components/InvitationCard.vue` - Invitation card with accept/decline buttons

### Pages
- `src/pages/DashboardPage.vue` - User dashboard with workspaces + activity
- `src/pages/WorkspacePage.vue` - Single workspace with tabs (overview, members)
- `src/pages/ProfilePage.vue` - User profile from OIDC token
- `src/pages/NotificationsPage.vue` - Notifications placeholder
- `src/pages/InvitationsPage.vue` - Invitations placeholder

### Styles
- `src/style.css` - Tailwind CSS imports

## Module Federation Configuration

### Remote Config (vite.config.ts)

```typescript
federation({
  name: 'client',
  filename: 'remoteEntry.js',
  exposes: {
    './routes': './src/routes.ts',  // Shell imports this
  },
  shared: ['vue', 'vue-router', 'pinia'],  // Shared as singletons
})
```

**Port**: 5174 (admin is 5173, shell is 3000)

### Route Exposure Pattern

`src/routes.ts` exports a plain array of `RouteRecordRaw` objects:

```typescript
const routes: RouteRecordRaw[] = [
  {
    path: '',  // Relative to /app
    name: 'client-dashboard',
    component: () => import('./pages/DashboardPage.vue'),
    meta: { breadcrumb: 'Dashboard' },
  },
  // ...
]

export default routes
```

Shell loads this at runtime:

```typescript
// Shell's router setup
const clientRoutes = await import('client/routes')
router.addRoute({
  path: '/app/:pathMatch(.*)*',
  children: clientRoutes.default,
})
```

## API Client Pattern

### Composable Setup

Same pattern as admin app. See `src/composables/useApiClient.ts`.

### Usage in Pages

```typescript
import { useApiClient } from '@/composables/useApiClient'
import { getWorkspace, getWorkspaceMembers } from '@dc-platform/api-client'

const client = useApiClient()
const workspace = await getWorkspace(client, workspaceId)
const members = await getWorkspaceMembers(client, workspaceId)
```

## OIDC User Data Access

User profile data comes from oidc-client-ts sessionStorage:

```typescript
function getOidcUser(): { name: string; email: string; sub: string } | null {
  const storageKey = Object.keys(sessionStorage).find(k => k.startsWith('oidc.user:'))
  if (!storageKey) return null
  try {
    const userData = JSON.parse(sessionStorage.getItem(storageKey) || '{}')
    const profile = userData.profile || {}
    return {
      name: profile.preferred_username || profile.name || profile.sub || 'User',
      email: profile.email || '',
      sub: profile.sub || '',
    }
  } catch {
    return null
  }
}
```

Used in:
- `DashboardPage.vue` - Welcome message
- `ProfilePage.vue` - User info display

## Component Patterns

### Page Component Pattern

Every page follows this structure:

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { DcSpinner, DcAlert } from '@dc-platform/ui-kit'
import { someApiFunction } from '@dc-platform/api-client'
import type { SomeType } from '@dc-platform/shared-types'
import { useApiClient } from '@/composables/useApiClient'
import PageHeader from '@/components/PageHeader.vue'

const client = useApiClient()
const data = ref<SomeType[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

onMounted(async () => {
  try {
    data.value = await someApiFunction(client)
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed to load'
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="p-6">
    <PageHeader title="Page Title" />

    <DcSpinner v-if="loading" class="mx-auto mt-12" size="lg" />
    <DcAlert v-else-if="error" variant="error" :title="error" />
    <template v-else>
      <!-- Page content -->
    </template>
  </div>
</template>
```

### Tabs Pattern

From WorkspacePage.vue:

```vue
<script setup lang="ts">
const activeTab = ref<'overview' | 'members'>('overview')
</script>

<template>
  <div class="border-b border-gray-200 mb-6">
    <nav class="-mb-px flex space-x-8">
      <button
        class="border-b-2 py-2 px-1 text-sm font-medium transition-colors"
        :class="activeTab === 'overview' ? 'border-indigo-500 text-indigo-600' : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'"
        @click="activeTab = 'overview'"
      >
        Overview
      </button>
      <!-- More tabs -->
    </nav>
  </div>

  <div v-if="activeTab === 'overview'">
    <!-- Tab content -->
  </div>
</template>
```

## Coding Rules

1. **Always `<script setup lang="ts">`** - Never Options API
2. **Props via `defineProps<T>()`** - Always typed
3. **Emits via `defineEmits<T>()`** - Always typed
4. **Tailwind CSS only** - No `<style>` blocks
5. **English everywhere** - Code, comments, UI strings
6. **Named exports** - Except Vue SFC default exports
7. **API calls through useApiClient** - Never direct axios
8. **Types from shared-types** - Never duplicate types
9. **UI components from ui-kit** - Never custom buttons/inputs
10. **Route names prefixed `client-`** - Avoid collisions with shell and admin

## What NOT to Do

- Do NOT add authentication logic (shell owns)
- Do NOT create HTTP interceptors here (use api-client)
- Do NOT access shell stores directly (use sessionStorage bridge)
- Do NOT create custom UI components (use ui-kit or add to ui-kit)
- Do NOT bypass API client (always use api-client functions)
- Do NOT hard-code API URLs (use env vars)
- Do NOT use `npm` or `yarn` (pnpm only)
- Do NOT create global state (composables for local state only)
- Do NOT use `any` type (use `unknown` and narrow)
- Do NOT skip loading/error states in pages

## Adding New Pages

1. **Create page component** in `src/pages/`
   - File: `SomeNewPage.vue`
   - Pattern: loading → error → data states
   - Use PageHeader component
   - Use ui-kit components

2. **Add route** to `src/routes.ts`
   ```typescript
   {
     path: 'new-page',
     name: 'client-new-page',
     component: () => import('./pages/SomeNewPage.vue'),
     meta: { breadcrumb: 'New Page' },
   }
   ```

3. **Type-check**
   ```bash
   cd apps/client
   pnpm exec vue-tsc --noEmit
   ```

4. **Test in shell**
   - Run shell: `cd apps/shell && pnpm dev`
   - Run client: `cd apps/client && pnpm dev`
   - Navigate to `/app/new-page`

## Environment Variables

All env vars prefixed with `VITE_`:

```env
VITE_API_BASE_URL=http://localhost:5000
VITE_KEYCLOAK_URL=http://localhost:8080
VITE_KEYCLOAK_REALM=dc-platform
```

Access via `import.meta.env.VITE_*`.

## Commands

```bash
# Standalone dev server
pnpm dev

# Type-check
pnpm type-check

# Build for production
pnpm build

# Preview production build
pnpm preview
```

## Integration with Shell

### Standalone Mode (Development)

Run `pnpm dev` to start standalone server on port 5174. Routes are available at `/app/*` relative to localhost:5174.

### Integrated Mode (Production)

Shell loads `remoteEntry.js` at runtime and mounts routes under `/app/*`. The client app runs inside the shell's layout with shared Vue/router/pinia instances.

## Troubleshooting

### Routes not loading in shell

**Symptom**: `/app` shows placeholder instead of client pages

**Fix**: Ensure client dev server is running on port 5174 and shell's `vite.config.ts` has correct remote URL:

```typescript
remotes: {
  client: 'http://localhost:5174/assets/remoteEntry.js',
}
```

### API calls fail with 401

**Symptom**: All API calls return 401 Unauthorized

**Fix**: Check that:
1. oidc-client-ts stored token in sessionStorage
2. `useApiClient` reads token correctly
3. Token is not expired (check browser dev tools → Application → Session Storage)

### Shared dependency version mismatch

**Symptom**: Runtime errors about Vue/router/pinia

**Fix**: Ensure shell and client have identical versions of shared deps:

```bash
cd apps/shell && pnpm list vue vue-router pinia
cd apps/client && pnpm list vue vue-router pinia
```

Update `package.json` to match versions.

### TypeScript errors

**Symptom**: Type errors on build

**Fix**: Run type-check and fix errors:

```bash
pnpm type-check
```

Common issues:
- Missing imports from `@dc-platform/shared-types`
- Wrong prop types (use interface from shared-types)
- Missing `v-model` binding type

## Decision Making

### When to add a new page

Ask:
1. Is this admin-specific? → Admin app
2. Is this client-facing? → Client app (this app)
3. Is this authentication? → Shell app
4. Is this tenant selection? → Shell app

### When to create a component

Ask:
1. Is this reusable across apps? → ui-kit package
2. Is this client-specific and reused? → `src/components/`
3. Is this page-specific and used once? → Keep inline in page

### When to add a composable

Ask:
1. Is this API client setup? → Already exists (useApiClient)
2. Is this page-specific state logic? → Create in `src/composables/`
3. Is this cross-app state? → Probably belongs in shell stores

## Related Files

- **Shell**: C:\Projects\dc-platform\apps\shell\src\router\index.ts - Loads client routes
- **API Client**: C:\Projects\dc-platform\packages\api-client\src\services\ - API functions
- **Types**: C:\Projects\dc-platform\packages\shared-types\src\ - DTO types
- **UI Kit**: C:\Projects\dc-platform\packages\ui-kit\src\components\ - Shared components

## Support

For issues or questions:
- Check main platform docs at `docs/`
- Review shell CLAUDE.md for integration patterns
- Review api-client CLAUDE.md for API patterns
- Review ui-kit CLAUDE.md for component patterns
