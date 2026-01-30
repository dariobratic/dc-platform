# Admin App

Admin remote microfrontend for DC Platform. Provides organization, workspace, user, role, and audit log management interfaces.

## Architecture

This is a **Module Federation remote** that exposes routes consumed by the shell host application. It runs as a standalone dev server on port 5173 but is designed to be loaded by the shell at runtime in production.

## Features

- **Dashboard**: System overview with stats and recent activity
- **Organizations**: Create, view, and manage organizations
- **Workspaces**: View workspaces across organizations
- **Users**: User management (coming soon)
- **Roles & Permissions**: Access control management
- **Audit Log**: Searchable activity log with filters

## Tech Stack

- **Vue 3** with Composition API (`<script setup>`)
- **TypeScript** in strict mode
- **Tailwind CSS** for styling
- **Vite** as build tool
- **Module Federation** (`@originjs/vite-plugin-federation`)
- **vue-router** for routing
- **pinia** for state management (shared with shell)

## Project Structure

```
src/
├── components/          # Shared components
│   ├── AdminBreadcrumb.vue
│   ├── PageHeader.vue
│   ├── StatsCard.vue
│   ├── EmptyState.vue
│   ├── ConfirmDialog.vue
│   └── StatusBadge.vue
├── composables/         # Vue composables
│   └── useApiClient.ts
├── pages/              # Route-level pages
│   ├── DashboardPage.vue
│   ├── OrganizationsPage.vue
│   ├── OrganizationDetailPage.vue
│   ├── WorkspacesPage.vue
│   ├── UsersPage.vue
│   ├── RolesPage.vue
│   └── AuditLogPage.vue
├── routes.ts           # Route definitions (exposed via Module Federation)
├── main.ts            # Standalone dev mode entry
├── App.vue            # Root component
└── style.css          # Tailwind imports
```

## Module Federation

### Exposed Modules

- `./routes` - Route definitions for `/admin/*` paths

### Shared Dependencies

- `vue` (singleton)
- `vue-router` (singleton)
- `pinia` (singleton)

### Remote Entry

Available at `http://localhost:5173/assets/remoteEntry.js` in development.

## Routes

All routes are prefixed with `/admin` by the shell:

| Path | Component | Description |
|------|-----------|-------------|
| `/admin` | DashboardPage | Admin dashboard |
| `/admin/organizations` | OrganizationsPage | Organization list |
| `/admin/organizations/:id` | OrganizationDetailPage | Organization detail |
| `/admin/workspaces` | WorkspacesPage | Workspace list |
| `/admin/users` | UsersPage | User management |
| `/admin/roles` | RolesPage | Role management |
| `/admin/audit` | AuditLogPage | Audit log |

## Development

### Prerequisites

- Node.js 20+
- pnpm 9+

### Setup

```bash
# From monorepo root
pnpm install

# Run standalone dev server
cd apps/admin
pnpm dev
```

The app will be available at `http://localhost:5173/admin` in standalone mode.

### Integration with Shell

To run with the shell:

```bash
# Terminal 1: Run shell (host)
cd apps/shell
pnpm dev

# Terminal 2: Run admin (remote)
cd apps/admin
pnpm dev
```

Access via shell at `http://localhost:3000/admin`.

## Environment Variables

Create `.env.local` in `apps/admin/`:

```env
VITE_API_BASE_URL=http://localhost:5000
```

## Build

```bash
pnpm build
```

Outputs to `dist/` with `remoteEntry.js` for Module Federation.

## Type Checking

```bash
pnpm type-check
```

## Dependencies

### Runtime

- `@dc-platform/ui-kit` - Shared UI components
- `@dc-platform/api-client` - Typed API client
- `@dc-platform/shared-types` - TypeScript types

### Dev

- Vite plugins, TypeScript, Tailwind CSS

## Component Patterns

All components follow Vue 3 Composition API with `<script setup lang="ts">`:

```vue
<script setup lang="ts">
import { ref, computed } from 'vue'
import type { SomeType } from '@dc-platform/shared-types'

interface Props {
  id: string
}

const props = defineProps<Props>()
const emit = defineEmits<{ updated: [value: string] }>()

const data = ref<SomeType | null>(null)
</script>

<template>
  <div class="p-4">
    <!-- Tailwind CSS only, no custom styles -->
  </div>
</template>
```

## API Integration

Use the `useApiClient` composable:

```typescript
import { useApiClient } from '@/composables/useApiClient'
import { getOrganization } from '@dc-platform/api-client'

const client = useApiClient()
const org = await getOrganization(client, orgId)
```

## Contributing

See the main project [CLAUDE.md](../../CLAUDE.md) for coding conventions and git workflow.
