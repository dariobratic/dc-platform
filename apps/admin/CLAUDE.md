# Admin App - Claude Instructions

Admin microfrontend for DC Platform. Module Federation remote exposing admin routes to shell host.

## App Scope

**This app owns:**
- Admin dashboard and system overview
- Organization management (CRUD)
- Workspace listing and filtering
- User management UI (future)
- Role and permission management
- Audit log viewer with search/filter

**This app does NOT own:**
- Authentication (shell owns)
- Tenant context management (shell owns)
- HTTP client configuration (api-client package)
- Reusable UI components (ui-kit package)
- Layout/navigation shell (shell owns)

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
- `src/components/AdminBreadcrumb.vue` - Breadcrumb navigation from route meta
- `src/components/PageHeader.vue` - Page title + breadcrumb + action slot
- `src/components/StatsCard.vue` - Dashboard metric card
- `src/components/EmptyState.vue` - Empty state placeholder with action slot
- `src/components/ConfirmDialog.vue` - Confirmation modal wrapper around DcModal
- `src/components/StatusBadge.vue` - Status badge with color variants

### Pages
- `src/pages/DashboardPage.vue` - Admin dashboard (stats + recent activity)
- `src/pages/OrganizationsPage.vue` - Organization list with search + create/delete
- `src/pages/OrganizationDetailPage.vue` - Single org with tabs (overview, workspaces, members)
- `src/pages/WorkspacesPage.vue` - All workspaces with org filter
- `src/pages/WorkspaceDetailPage.vue` - Single workspace with tabs (overview, members, settings)
- `src/pages/UsersPage.vue` - User management placeholder (coming soon)
- `src/pages/RolesPage.vue` - Role management with permission assignment
- `src/pages/AuditLogPage.vue` - Audit log with filters and pagination

### Styles
- `src/style.css` - Tailwind CSS imports

## Module Federation Configuration

### Remote Config (vite.config.ts)

```typescript
federation({
  name: 'admin',
  filename: 'remoteEntry.js',
  exposes: {
    './routes': './src/routes.ts',  // Shell imports this
  },
  shared: ['vue', 'vue-router', 'pinia'],  // Shared as singletons
})
```

### Route Exposure Pattern

`src/routes.ts` exports a plain array of `RouteRecordRaw` objects:

```typescript
const routes: RouteRecordRaw[] = [
  {
    path: '',  // Relative to /admin
    name: 'admin-dashboard',
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
const adminRoutes = await import('admin/routes')
router.addRoute({
  path: '/admin',
  children: adminRoutes.default,
})
```

## API Client Pattern

### Composable Setup

`useApiClient` creates a singleton axios instance with interceptors:

```typescript
import { createApiClient } from '@dc-platform/api-client'

let clientInstance: AxiosInstance | null = null

export function useApiClient(): AxiosInstance {
  if (!clientInstance) {
    clientInstance = createApiClient({
      baseURL: import.meta.env.VITE_API_BASE_URL,
      getAccessToken: async () => {
        // Read from oidc-client-ts sessionStorage
        const storageKey = Object.keys(sessionStorage).find(k => k.startsWith('oidc.user:'))
        if (storageKey) {
          const userData = JSON.parse(sessionStorage.getItem(storageKey) || '{}')
          return userData.access_token || null
        }
        return null
      },
      getOrganizationId: () => sessionStorage.getItem('organizationId') || null,
      getWorkspaceId: () => sessionStorage.getItem('workspaceId') || null,
      onUnauthorized: () => window.location.href = '/login',
    })
  }
  return clientInstance
}
```

### Usage in Pages

```typescript
import { useApiClient } from '@/composables/useApiClient'
import { getOrganization, deleteOrganization } from '@dc-platform/api-client'

const client = useApiClient()

// GET request
const org = await getOrganization(client, orgId)

// DELETE request
await deleteOrganization(client, orgId)
```

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

### Modal Pattern

```vue
<script setup lang="ts">
const showModal = ref(false)
const form = ref({ name: '', slug: '' })
const modalLoading = ref(false)
const modalError = ref<string | null>(null)

async function handleSubmit() {
  modalLoading.value = true
  modalError.value = null
  try {
    await someApiCall(client, form.value)
    showModal.value = false
    form.value = { name: '', slug: '' }  // Reset
    await reloadData()
  } catch (e) {
    modalError.value = e instanceof Error ? e.message : 'Failed'
  } finally {
    modalLoading.value = false
  }
}
</script>

<template>
  <DcModal :open="showModal" title="Create Item" @close="showModal = false">
    <div class="space-y-4">
      <DcAlert v-if="modalError" variant="error" :title="modalError" />
      <DcInput v-model="form.name" label="Name" />
      <DcInput v-model="form.slug" label="Slug" />
    </div>
    <template #footer>
      <div class="flex justify-end space-x-3">
        <DcButton variant="secondary" @click="showModal = false">Cancel</DcButton>
        <DcButton :loading="modalLoading" @click="handleSubmit">Create</DcButton>
      </div>
    </template>
  </DcModal>
</template>
```

### Table Pattern

```vue
<template>
  <div class="overflow-hidden border border-gray-200 rounded-lg bg-white">
    <table class="min-w-full divide-y divide-gray-200">
      <thead class="bg-gray-50">
        <tr>
          <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
            Column Name
          </th>
        </tr>
      </thead>
      <tbody class="bg-white divide-y divide-gray-200">
        <tr v-for="item in items" :key="item.id" class="hover:bg-gray-50">
          <td class="px-6 py-4 whitespace-nowrap">
            {{ item.name }}
          </td>
        </tr>
      </tbody>
    </table>
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
10. **Route names prefixed `admin-`** - Avoid collisions with shell

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
     name: 'admin-new-page',
     component: () => import('./pages/SomeNewPage.vue'),
     meta: { breadcrumb: 'New Page' },
   }
   ```

3. **Type-check**
   ```bash
   cd apps/admin
   pnpm exec vue-tsc --noEmit
   ```

4. **Test in shell**
   - Run shell: `cd apps/shell && pnpm dev`
   - Run admin: `cd apps/admin && pnpm dev`
   - Navigate to `/admin/new-page`

## Environment Variables

All env vars prefixed with `VITE_`:

```env
VITE_API_BASE_URL=http://localhost:5000
```

Access via `import.meta.env.VITE_API_BASE_URL`.

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

Run `pnpm dev` to start standalone server on port 5173. Routes are available at `/admin/*` relative to localhost:5173.

### Integrated Mode (Production)

Shell loads `remoteEntry.js` at runtime and mounts routes under `/admin/*`. The admin app runs inside the shell's layout with shared Vue/router/pinia instances.

## Troubleshooting

### Routes not loading in shell

**Symptom**: `/admin` shows placeholder instead of admin pages

**Fix**: Ensure admin dev server is running on port 5173 and shell's `vite.config.ts` has correct remote URL:

```typescript
remotes: {
  admin: 'http://localhost:5173/assets/remoteEntry.js',
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

**Fix**: Ensure shell and admin have identical versions of shared deps:

```bash
cd apps/shell && pnpm list vue vue-router pinia
cd apps/admin && pnpm list vue vue-router pinia
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
2. Is this client-facing? → Client app (separate remote)
3. Is this authentication? → Shell app
4. Is this tenant selection? → Shell app

### When to create a component

Ask:
1. Is this reusable across apps? → ui-kit package
2. Is this admin-specific and reused? → `src/components/`
3. Is this page-specific and used once? → Keep inline in page

### When to add a composable

Ask:
1. Is this API client setup? → Already exists (useApiClient)
2. Is this page-specific state logic? → Create in `src/composables/`
3. Is this cross-app state? → Probably belongs in shell stores

## Related Files

- **Shell**: `apps/shell/src/router/index.ts` - Loads admin routes
- **API Client**: `packages/api-client/src/services/admin.ts` - Admin API functions
- **Types**: `packages/shared-types/src/admin.ts` - Admin DTO types
- **UI Kit**: `packages/ui-kit/src/components/` - Shared components

## Support

For issues or questions:
- Check main platform docs at `docs/`
- Review shell CLAUDE.md for integration patterns
- Review api-client CLAUDE.md for API patterns
- Review ui-kit CLAUDE.md for component patterns
