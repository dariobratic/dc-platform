# Client App

Client-facing microfrontend for DC Platform. Module Federation remote exposing client routes to shell host.

## Overview

The client app provides end-user features for interacting with workspaces, viewing profiles, and managing invitations. It runs on port 5174 and is loaded by the shell at runtime via Module Federation.

## Tech Stack

- Vue 3 (Composition API with `<script setup>`)
- TypeScript (strict mode)
- Tailwind CSS
- Vue Router (routes exposed via Module Federation)
- Pinia (shared with shell)
- Vite + vite-plugin-federation
- UI components from @dc-platform/ui-kit
- API client from @dc-platform/api-client
- Types from @dc-platform/shared-types

## Routes

All routes are relative to `/app` (shell mounts under `/app/*`):

- `/app` - Dashboard (workspace overview)
- `/app/workspace/:id` - Single workspace view
- `/app/profile` - User profile and preferences
- `/app/notifications` - Notification list
- `/app/invitations` - Pending invitations

## Project Structure

```
src/
├── main.ts                    # Standalone dev mode bootstrap
├── App.vue                    # Root component
├── style.css                  # Tailwind imports
├── routes.ts                  # Route definitions (exposed via Module Federation)
├── composables/
│   └── useApiClient.ts       # API client singleton
├── components/
│   ├── PageHeader.vue        # Page title + actions
│   ├── EmptyState.vue        # Empty state placeholder
│   ├── ActivityFeed.vue      # Recent activity list
│   ├── QuickActions.vue      # Quick action buttons
│   ├── WorkspaceCard.vue     # Workspace summary card
│   └── InvitationCard.vue    # Invitation card
└── pages/
    ├── DashboardPage.vue     # User dashboard
    ├── WorkspacePage.vue     # Workspace detail
    ├── ProfilePage.vue       # User profile
    ├── NotificationsPage.vue # Notifications
    └── InvitationsPage.vue   # Invitations
```

## Development

### Prerequisites

Monorepo root dependencies must be installed:

```bash
cd C:\Projects\dc-platform
pnpm install
```

### Run Standalone

```bash
cd apps/client
pnpm dev
```

Server starts on http://localhost:5174

### Run with Shell

Terminal 1:
```bash
cd apps/shell
pnpm dev
```

Terminal 2:
```bash
cd apps/client
pnpm dev
```

Navigate to http://localhost:3000/app

## Commands

```bash
# Development server
pnpm dev

# Type-check (no build)
pnpm type-check

# Production build
pnpm build

# Preview production build
pnpm preview
```

## Module Federation

### Remote Configuration

The client app exposes its routes to the shell:

```typescript
// vite.config.ts
federation({
  name: 'client',
  filename: 'remoteEntry.js',
  exposes: {
    './routes': './src/routes.ts',
  },
  shared: ['vue', 'vue-router', 'pinia'],
})
```

### Shell Integration

The shell loads client routes at runtime:

```typescript
// Shell's vite.config.ts
remotes: {
  client: 'http://localhost:5174/assets/remoteEntry.js',
}

// Shell's router
const clientRoutes = await import('client/routes')
router.addRoute({
  path: '/app/:pathMatch(.*)*',
  children: clientRoutes.default,
})
```

## Environment Variables

Create `.env.local` (gitignored) or use defaults:

```env
VITE_API_BASE_URL=http://localhost:5000
VITE_KEYCLOAK_URL=http://localhost:8080
VITE_KEYCLOAK_REALM=dc-platform
```

## Key Features

### Dashboard

- Welcome message with user name
- Quick action buttons (Profile, Invitations, Notifications)
- Workspace cards grid
- Recent activity feed

### Workspace Detail

- Overview tab with workspace details
- Members tab with role-based list
- Placeholder for future content management

### Profile

- User information from OIDC token
- Link to Keycloak account for password change
- Placeholder for preferences

### Notifications & Invitations

- Placeholder pages with empty states
- Ready for future API integration

## API Integration

All API calls go through the Gateway at `http://localhost:5000`:

```typescript
import { useApiClient } from '@/composables/useApiClient'
import { getWorkspacesByOrganization } from '@dc-platform/api-client'

const client = useApiClient()
const workspaces = await getWorkspacesByOrganization(client, orgId)
```

The API client automatically includes:
- Authorization header (from oidc-client-ts sessionStorage)
- X-Organization-Id header (from sessionStorage)
- X-Workspace-Id header (from sessionStorage)

## Styling

All styling uses Tailwind CSS utility classes. No custom CSS unless required for third-party overrides.

```vue
<template>
  <div class="p-6 bg-white rounded-lg shadow-sm">
    <h1 class="text-2xl font-bold text-gray-900">Title</h1>
  </div>
</template>
```

## Troubleshooting

### Routes not loading in shell

Ensure client dev server is running on port 5174 and shell's `vite.config.ts` has:

```typescript
remotes: {
  client: 'http://localhost:5174/assets/remoteEntry.js',
}
```

### API calls fail with 401

Check that oidc-client-ts stored token in sessionStorage. Inspect in browser dev tools: Application → Session Storage → look for key starting with `oidc.user:`

### Type errors

Run type-check and fix errors:

```bash
pnpm type-check
```

Common issues:
- Missing imports from @dc-platform/shared-types
- Wrong prop types
- Missing v-model bindings

## Related Files

- **Shell**: C:\Projects\dc-platform\apps\shell\src\router\index.ts
- **API Client**: C:\Projects\dc-platform\packages\api-client
- **UI Kit**: C:\Projects\dc-platform\packages\ui-kit
- **Types**: C:\Projects\dc-platform\packages\shared-types

## License

Private. Copyright Digital Control 2026.
