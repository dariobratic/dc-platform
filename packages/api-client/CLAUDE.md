# @dc-platform/api-client - Claude Agent Instructions

Typed HTTP client library for DC Platform backend services. Provides a thin, typed wrapper around Axios for all microservice API calls.

## Package Scope

**This package contains:**
- Axios-based HTTP client factory with authentication and tenant context interceptors
- Typed service functions for all backend API endpoints
- Request/response handling with proper error propagation
- Zero business logic (pure API client layer)

**Runtime dependencies:**
- `axios` - HTTP client
- `@dc-platform/shared-types` - TypeScript type definitions

## File Structure and Mapping

| File | Backend Service | Contains |
|------|----------------|----------|
| `src/client.ts` | Cross-service | Axios factory with auth + tenant interceptors |
| `src/services/directory.ts` | `services/directory` (port 5001) | Organizations, Workspaces, Memberships, Invitations |
| `src/services/access-control.ts` | `services/access-control` (port 5003) | Roles, RoleAssignments, Permissions |
| `src/services/audit.ts` | `services/audit` (port 5004) | Audit entries, queries, entity history |
| `src/services/auth.ts` | `services/authentication` (port 5002) | Token exchange, refresh, logout, userinfo |
| `src/services/notification.ts` | `services/notification` (port 5005) | Email, push notifications |
| `src/services/configuration.ts` | `services/configuration` (port 5006) | Org settings, feature flags |
| `src/services/admin.ts` | `services/admin-api` (port 5007) | Dashboard, system health, summaries |
| `src/index.ts` | Barrel export | Re-exports client factory and all service functions |

## Client Factory Pattern

### Creating an API Client

The `createApiClient` factory function creates a configured Axios instance with built-in auth and tenant context handling:

```typescript
import { createApiClient } from '@dc-platform/api-client'
import type { ApiClientOptions } from '@dc-platform/api-client'

const options: ApiClientOptions = {
  baseURL: 'http://localhost:5000', // Gateway URL

  // Auth token provider (async)
  getAccessToken: async () => {
    const authStore = useAuthStore()
    return authStore.accessToken
  },

  // Tenant context providers
  getOrganizationId: () => {
    const tenantStore = useTenantStore()
    return tenantStore.organizationId
  },

  getWorkspaceId: () => {
    const tenantStore = useTenantStore()
    return tenantStore.workspaceId
  },

  // 401 handler
  onUnauthorized: () => {
    const authStore = useAuthStore()
    authStore.logout()
  }
}

const apiClient = createApiClient(options)
```

### Interceptor Behavior

**Request Interceptors (applied in order):**

1. **Auth Token Interceptor**:
   - Calls `getAccessToken()` async function
   - Sets `Authorization: Bearer {token}` header if token exists
   - Runs on every request

2. **Tenant Context Interceptor**:
   - Calls `getOrganizationId()` and `getWorkspaceId()` sync functions
   - Sets `X-Organization-Id` header if organizationId exists
   - Sets `X-Workspace-Id` header if workspaceId exists
   - Required for multi-tenant API calls

**Response Interceptor:**

- **401 Unauthorized**: Calls `onUnauthorized()` callback (e.g., redirect to login)
- All other responses pass through unchanged
- Errors are re-thrown after callback

## Service Function Pattern

All service functions follow this pattern:

```typescript
export async function functionName(
  client: AxiosInstance,
  ...params: T[],
  request?: RequestDTO
): Promise<ResponseDTO> {
  const { data } = await client.method('/api/v1/endpoint', request)
  return data
}
```

**Rules:**
- First parameter is always `client: AxiosInstance`
- Route parameters come next (id, organizationId, etc.)
- Request body is last (optional for GET/DELETE)
- Return type matches backend response DTO from `@dc-platform/shared-types`
- No error handling (errors propagate to caller)
- No side effects (pure HTTP calls)

## Usage Examples

### Directory Service

```typescript
import { createApiClient, getOrganization, createWorkspace } from '@dc-platform/api-client'

const client = createApiClient(options)

// Get organization
const org = await getOrganization(client, orgId)

// Create workspace
const workspace = await createWorkspace(client, orgId, {
  name: 'Engineering',
  slug: 'engineering'
})

// Get workspace members
const members = await getWorkspaceMembers(client, workspace.id)
```

### Access Control Service

```typescript
import { getRolesByScope, assignRole, checkPermission } from '@dc-platform/api-client'

// Get all roles for an organization
const roles = await getRolesByScope(client, orgId, 'Organization')

// Assign role to user
const assignment = await assignRole(client, roleId, {
  userId: 'user-guid',
  scopeId: orgId,
  scopeType: 'Organization',
  assignedBy: 'admin-guid'
})

// Check if user has permission
const result = await checkPermission(client, userId, orgId, 'document:write')
console.log(result.hasPermission) // true/false
```

### Audit Service

```typescript
import { getAuditEntries, createAuditEntry } from '@dc-platform/api-client'

// Query audit logs
const logs = await getAuditEntries(client, {
  organizationId: orgId,
  entityType: 'Organization',
  from: '2026-01-01T00:00:00Z',
  take: 50
})

// Create audit entry
await createAuditEntry(client, {
  userId: currentUser.id,
  action: 'organization.updated',
  entityType: 'Organization',
  entityId: orgId,
  serviceName: 'Directory'
})
```

### Authentication Service

```typescript
import { exchangeCodeForToken, refreshToken, getUserInfo } from '@dc-platform/api-client'

// Exchange authorization code
const tokens = await exchangeCodeForToken(client, {
  code: authCode,
  redirectUri: 'http://localhost:3000/callback'
})

// Refresh access token
const newTokens = await refreshToken(client, {
  refreshToken: tokens.refreshToken
})

// Get current user info
const userInfo = await getUserInfo(client)
```

### Notification Service

```typescript
import { sendEmail, sendPush } from '@dc-platform/api-client'

// Send email
await sendEmail(client, {
  to: 'user@example.com',
  subject: 'Welcome!',
  templateId: 'welcome',
  templateData: { name: 'John Doe' }
})

// Send push notification
await sendPush(client, {
  userId: 'user-guid',
  title: 'New Message',
  message: 'You have a new message'
})
```

### Configuration Service

```typescript
import { getConfiguration, toggleFeature } from '@dc-platform/api-client'

// Get org config
const config = await getConfiguration(client, orgId)
const theme = config.settings['theme'] ?? 'light'

// Toggle feature flag
await toggleFeature(client, orgId, 'beta-dashboard', {
  isEnabled: true,
  description: 'Enable new beta dashboard'
})
```

### Admin Service

```typescript
import { getDashboard, getSystemHealth } from '@dc-platform/api-client'

// Get dashboard metrics
const dashboard = await getDashboard(client)
console.log(`Organizations: ${dashboard.organizationCount}`)

// Check system health
const health = await getSystemHealth(client)
health.services.forEach(service => {
  console.log(`${service.serviceName}: ${service.status}`)
})
```

## Integration with Frontend Apps

### Shell App Setup

```typescript
// apps/shell/src/plugins/api.ts
import { createApiClient } from '@dc-platform/api-client'
import { useAuthStore } from '@/stores/auth'
import { useTenantStore } from '@/stores/tenant'

export function setupApiClient() {
  const authStore = useAuthStore()
  const tenantStore = useTenantStore()

  return createApiClient({
    baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000',
    getAccessToken: async () => authStore.accessToken,
    getOrganizationId: () => tenantStore.organizationId,
    getWorkspaceId: () => tenantStore.workspaceId,
    onUnauthorized: () => authStore.logout()
  })
}
```

### Composable Usage Pattern

```typescript
// apps/shell/src/composables/useOrganizations.ts
import { ref, readonly } from 'vue'
import { getOrganization, updateOrganization } from '@dc-platform/api-client'
import type { Organization, UpdateOrganizationRequest } from '@dc-platform/shared-types'
import { useApi } from '@/composables/useApi'

export function useOrganizations() {
  const { client } = useApi()
  const organization = ref<Organization | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetch(id: string) {
    loading.value = true
    error.value = null
    try {
      organization.value = await getOrganization(client, id)
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to load organization'
    } finally {
      loading.value = false
    }
  }

  async function update(id: string, request: UpdateOrganizationRequest) {
    loading.value = true
    error.value = null
    try {
      organization.value = await updateOrganization(client, id, request)
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to update organization'
      throw e
    } finally {
      loading.value = false
    }
  }

  return {
    organization: readonly(organization),
    loading: readonly(loading),
    error: readonly(error),
    fetch,
    update
  }
}
```

## Adding New Endpoints

When backend adds a new endpoint, follow this process:

### 1. Read Backend Controller

Find the controller in `services/{service}/src/{Service}.API/Controllers/`:

```csharp
[HttpPost("organizations/{orgId:guid}/archive")]
public async Task<IActionResult> Archive(Guid orgId, ArchiveRequest request)
{
    // ...
    return Ok(result);
}
```

### 2. Add Types to shared-types (if new)

If new request/response DTOs exist, add to `packages/shared-types/src/{service}.ts`:

```typescript
export interface ArchiveOrganizationRequest {
  reason?: string
}
```

### 3. Add Function to Service File

Add to `packages/api-client/src/services/{service}.ts`:

```typescript
export async function archiveOrganization(
  client: AxiosInstance,
  id: string,
  request: ArchiveOrganizationRequest
): Promise<OrganizationResponse> {
  const { data } = await client.post(`/api/v1/organizations/${id}/archive`, request)
  return data
}
```

### 4. Export from index.ts

Already exported via `export * from './services/{service}'` — no changes needed.

### 5. Type-Check

```bash
cd packages/api-client
pnpm exec tsc --noEmit
```

## Commands

```bash
# Type-check this package
cd packages/api-client
pnpm exec tsc --noEmit

# Install dependencies (from monorepo root)
pnpm install

# No build, no test, no dev server - types only!
```

## What Belongs in This Package

### YES - Include Here

- Typed functions for backend API endpoints
- Axios instance factory with interceptors
- Request parameter types (query params, route params)
- HTTP method wrappers (GET, POST, PUT, DELETE, PATCH)

### NO - Keep Elsewhere

- Request/response DTO types → Keep in `@dc-platform/shared-types`
- Business logic → Belongs in frontend composables or services
- State management → Belongs in Pinia stores
- Validation logic → Belongs in forms or composables
- Caching → Belongs in composables or Pinia stores
- Retry logic → Belongs in Axios interceptors or client factory
- Mock data → Belongs in test files

## Error Handling Strategy

This package does NOT handle errors. Errors propagate to the caller.

**Caller Responsibility:**
- Wrap API calls in try-catch blocks
- Handle network errors (AxiosError)
- Handle 4xx/5xx HTTP errors
- Show user-friendly error messages
- Log errors for debugging

**Example:**

```typescript
try {
  const org = await getOrganization(client, orgId)
} catch (error) {
  if (axios.isAxiosError(error)) {
    if (error.response?.status === 404) {
      console.error('Organization not found')
    } else if (error.response?.status === 403) {
      console.error('Access denied')
    } else {
      console.error('API error:', error.response?.data)
    }
  } else {
    console.error('Unexpected error:', error)
  }
}
```

## Type Safety Rules

This package enforces strict TypeScript:

```json
{
  "strict": true,
  "noUnusedLocals": true,
  "noUnusedParameters": true,
  "forceConsistentCasingInFileNames": true
}
```

When adding functions:
- No `any` - Use `unknown` and narrow, or import proper types from shared-types
- No default exports - Named exports only
- No runtime code beyond Axios calls
- No side effects (logging, caching, state mutation)
- English everywhere (code, comments, docs)

## What NOT to Do

- Do NOT add business logic to API functions
- Do NOT cache responses in this package (caller's responsibility)
- Do NOT transform response data (return exactly what backend sends)
- Do NOT validate request data (backend handles validation)
- Do NOT retry failed requests automatically (use Axios interceptors if needed)
- Do NOT handle errors (let them propagate)
- Do NOT log API calls (use Axios interceptors if needed)
- Do NOT store secrets or tokens in this package
- Do NOT access localStorage/sessionStorage
- Do NOT use global state or singletons

## Troubleshooting

### Import fails in consuming app

**Symptom**: `Cannot find module '@dc-platform/api-client'`

**Fix**: Ensure consuming app has dependency in package.json:
```json
{
  "dependencies": {
    "@dc-platform/api-client": "workspace:*"
  }
}
```

Run `pnpm install` from monorepo root.

### Type mismatch with backend response

**Symptom**: Runtime data doesn't match TypeScript type

**Fix**: Backend DTO changed. Update types in `@dc-platform/shared-types` first, then update function signatures here if needed.

### 401 Unauthorized on every request

**Symptom**: All API calls fail with 401

**Fix**: Check `getAccessToken()` function is returning valid JWT token. Verify token is not expired. Check Keycloak configuration.

### Tenant context not applied

**Symptom**: Backend returns 400 "Organization ID required"

**Fix**: Verify `getOrganizationId()` and `getWorkspaceId()` are returning correct IDs. Check tenant store is populated.

### Circular dependency error

**Symptom**: Import cycle detected

**Fix**: This package should never import from apps. Only import from `@dc-platform/shared-types`. If you need app-specific logic, put it in the app's composables.

## Related Packages

- **@dc-platform/shared-types** - Type definitions (imported by this package)
- **@dc-platform/ui-kit** - Shared Vue components (imports this package)

## Support

For questions or issues, refer to:
- Main platform documentation at `docs/`
- Backend service CLAUDE.md files in `services/{service}/CLAUDE.md`
- Axios documentation: https://axios-http.com/
