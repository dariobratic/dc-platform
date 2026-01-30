# @dc-platform/shared-types - Claude Agent Instructions

TypeScript-only package providing cross-app type definitions matching backend .NET DTOs.

## Package Scope

**This package contains ONLY TypeScript type definitions:**
- Interfaces (`export interface`)
- Type aliases (`export type`)
- String literal unions (for C# enums)
- No runtime code, no functions, no classes
- Zero dependencies (only `typescript` as devDependency)

## File Structure and Mapping

| File | Backend Service | Contains |
|------|----------------|----------|
| `src/auth.ts` | `services/authentication` | Token types, UserInfo, AuthState |
| `src/tenant.ts` | `services/directory` (domain) | Core entities: Organization, Workspace, Membership, Invitation |
| `src/api.ts` | Cross-service | PagedResponse, ApiError, ApiResult (generic wrappers) |
| `src/directory.ts` | `services/directory` (DTOs) | Request/response types for Directory API endpoints |
| `src/access-control.ts` | `services/access-control` | Role, RoleAssignment, Permission types |
| `src/audit.ts` | `services/audit` | Audit entry types and query filters |
| `src/notification.ts` | `services/notification` | Email and push notification requests |
| `src/configuration.ts` | `services/configuration` | Config and feature flag types |
| `src/admin.ts` | `services/admin-api` | Admin dashboard, health check, summary types |
| `src/index.ts` | Barrel export | Re-exports all types (alphabetically sorted) |

## Type Conventions

### C# to TypeScript Mapping Rules

1. **Guid → string**
   ```typescript
   id: string  // C#: Guid Id
   ```

2. **DateTime → string** (ISO 8601 format)
   ```typescript
   createdAt: string  // C#: DateTime CreatedAt
   ```

3. **Enum → String Literal Union** (exact C# enum value names)
   ```typescript
   export type OrganizationStatus = 'Active' | 'Suspended' | 'Deleted'
   // C#: enum OrganizationStatus { Active, Suspended, Deleted }
   ```

4. **Dictionary<string, string> → Record<string, string>**
   ```typescript
   settings: Record<string, string>  // C#: Dictionary<string, string> Settings
   ```

5. **List<T> → T[]**
   ```typescript
   items: Organization[]  // C#: List<Organization> Items
   ```

6. **Nullable Reference Types → T | null**
   ```typescript
   updatedAt: string | null  // C#: DateTime? UpdatedAt
   ```

7. **Optional Parameters → T?**
   ```typescript
   description?: string  // C#: string? Description (optional in request)
   ```

### Naming Conventions

- **Request DTOs**: `Create*Request`, `Update*Request`, `Add*Request`, `*Request`
- **Response DTOs**: `*Response` or type alias to domain entity
- **Domain Entities**: PascalCase noun (e.g., `Organization`, `Workspace`)
- **Enums**: PascalCase type, PascalCase values (matching C# exactly)

### Module Organization

Each `.ts` file (except `index.ts`) represents one cohesive domain:
- **Domain entities** go in `tenant.ts` (Organization, Workspace, etc.)
- **API DTOs** go in service-specific files (`directory.ts`, `access-control.ts`)
- **Generic wrappers** go in `api.ts` (PagedResponse, ApiError)
- **Barrel export** in `index.ts` re-exports everything with `export type`

## Adding New Types

### When Backend DTOs Change

1. **Identify the service** - Which backend service owns this DTO?
   - Directory domain entity? → `tenant.ts`
   - Directory API request/response? → `directory.ts`
   - Access Control? → `access-control.ts`
   - Audit? → `audit.ts`
   - etc.

2. **Create/update the interface** - Follow C# to TypeScript mapping rules above

3. **Export from index.ts** - Add to the alphabetically sorted export block for that module

4. **Type-check** - Run `pnpm exec tsc --noEmit` to verify

### Example: Adding a New Directory DTO

Backend adds `ArchiveWorkspaceRequest`:

```csharp
// services/directory/src/Directory.API/DTOs/ArchiveWorkspaceRequest.cs
public record ArchiveWorkspaceRequest(string? Reason);
```

Add to `src/directory.ts`:

```typescript
export interface ArchiveWorkspaceRequest {
  reason?: string
}
```

Export from `src/index.ts` (in the Directory section, alphabetically):

```typescript
export type {
  AcceptInvitationRequest,
  AddMemberRequest,
  ArchiveWorkspaceRequest,  // <-- NEW
  ChangeMemberRoleRequest,
  // ... rest
} from './directory'
```

## What Belongs in This Package

### YES - Include Here

- DTOs from backend services (exact property-by-property match)
- Domain entities exposed via API (Organization, Workspace, Membership)
- Enums used in API contracts (OrganizationStatus, WorkspaceRole)
- Generic API wrappers (PagedResponse<T>, ApiError, ApiResult<T>)
- Minimal frontend-specific extensions (TenantContext, AuthState)

### NO - Keep Elsewhere

- Vue component prop types → Keep in component files with `defineProps<T>()`
- App-specific UI state → Keep in `apps/*/src/types/` directories
- Runtime code (functions, classes) → Belongs in `packages/api-client` or `packages/ui-kit`
- Third-party library extensions → Create separate `*.d.ts` files in consuming apps
- Environment config types → Keep in app-specific `env.d.ts`

## Type Safety Rules

This package enforces strict TypeScript:

```json
{
  "strict": true,                          // All strict checks
  "noUnusedLocals": true,                  // No unused variables
  "noUnusedParameters": true,              // No unused function params
  "forceConsistentCasingInFileNames": true // Case-sensitive imports
}
```

When adding types:
- No `any` - Use `unknown` and narrow, or define proper types
- No default exports - Named exports only
- No runtime code - Types only
- No comments unless clarifying non-obvious backend behavior
- English everywhere

## Build and Import Strategy

**This package has NO build step.**

Consumers import TypeScript source files directly:

```typescript
// Consumer's vite.config.ts or tsconfig.json handles compilation
import type { Organization } from '@dc-platform/shared-types'
```

The `package.json` `exports` map points to source `.ts` files:

```json
{
  "types": "./src/index.ts",
  "exports": {
    ".": "./src/index.ts",
    "./auth": "./src/auth.ts",
    // ...
  }
}
```

This works because:
1. Monorepo packages import via `workspace:*` protocol
2. Consuming apps (shell, admin, client) have their own TypeScript compilation
3. Vite/tsup handles `.ts` imports natively in development and build

## Commands

```bash
# Type-check this package
cd packages/shared-types
pnpm exec tsc --noEmit

# Install dependencies (only typescript)
pnpm install

# No build, no test, no dev server - types only!
```

## Common Tasks

### Sync Types After Backend API Change

1. Identify changed DTO in backend (e.g., `services/directory/src/Directory.API/DTOs/`)
2. Find corresponding module in `src/` (e.g., `directory.ts`)
3. Update interface to match C# properties exactly
4. Verify export in `index.ts`
5. Run `pnpm exec tsc --noEmit` to type-check
6. Commit with message: `feat(shared-types): update Directory DTOs`

### Add Support for New Backend Service

1. Create new module: `src/new-service.ts`
2. Add types matching backend DTOs
3. Export types from `index.ts` (new section, alphabetically sorted)
4. Add entry to `package.json` `exports` map
5. Document in README.md (table of modules)
6. Update this CLAUDE.md file-to-service mapping table

## Integration with Other Packages

### packages/api-client

API client imports types from this package:

```typescript
import type { Organization, CreateOrganizationRequest } from '@dc-platform/shared-types'

export async function createOrganization(
  request: CreateOrganizationRequest
): Promise<Organization> {
  // ...
}
```

### packages/ui-kit

UI components import domain types:

```typescript
import type { Workspace } from '@dc-platform/shared-types'

interface Props {
  workspace: Workspace
}
```

### apps/shell, apps/admin, apps/client

Frontend apps import types for API calls, state management, component props:

```typescript
import type { Organization, Workspace, WorkspaceRole } from '@dc-platform/shared-types'
import { useTenantStore } from '@/stores/tenant'

const org = ref<Organization | null>(null)
```

## What NOT to Do

- Do NOT add runtime code (functions, classes, constants)
- Do NOT install dependencies other than `typescript` as devDependency
- Do NOT create a build step or dist/ output
- Do NOT duplicate types from backend - always import from here
- Do NOT use `any` type - define proper interfaces or use `unknown`
- Do NOT use default exports - named exports only
- Do NOT add frontend-specific logic types here (those go in app src/types/)
- Do NOT modify backend DTOs to "improve" them - match exactly

## Troubleshooting

### Type import fails in consuming app

**Symptom**: `Cannot find module '@dc-platform/shared-types'`

**Fix**: Ensure consuming app has dependency in package.json:
```json
{
  "dependencies": {
    "@dc-platform/shared-types": "workspace:*"
  }
}
```

Run `pnpm install` from monorepo root.

### Type mismatch with backend response

**Symptom**: Runtime data doesn't match TypeScript type

**Fix**: Backend DTO changed - update corresponding interface in this package to match C# definition exactly. Check property names, casing, nullability.

### Circular dependency error

**Symptom**: Import cycle detected

**Fix**: This package should never import from other packages. Only `import type` from internal modules. If you need to reference a type from another package, that type likely belongs here instead.
