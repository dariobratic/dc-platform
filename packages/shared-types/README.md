# @dc-platform/shared-types

TypeScript type definitions for DC Platform. Provides cross-app types matching backend .NET DTOs.

## Overview

This package contains **type-only** TypeScript definitions with zero runtime code. All types are exact mappings from the .NET backend services to ensure type safety across the frontend/backend boundary.

## Installation

This package is part of the DC Platform monorepo. It's automatically available to workspace packages via pnpm workspaces.

```json
{
  "dependencies": {
    "@dc-platform/shared-types": "workspace:*"
  }
}
```

## Usage

### Import from barrel export (recommended)

```typescript
import type { Organization, Workspace, CreateOrganizationRequest } from '@dc-platform/shared-types'
```

### Import from specific module

```typescript
import type { Organization } from '@dc-platform/shared-types/tenant'
import type { CreateOrganizationRequest } from '@dc-platform/shared-types/directory'
```

## Type Modules

| Module | Backend Service | Purpose |
|--------|----------------|---------|
| `auth.ts` | `services/authentication` | Authentication tokens, user info |
| `tenant.ts` | `services/directory` | Core domain entities (Organization, Workspace, Membership) |
| `api.ts` | Cross-service | Generic API response wrappers (PagedResponse, ApiError) |
| `directory.ts` | `services/directory` | Request/response DTOs for Directory API |
| `access-control.ts` | `services/access-control` | RBAC/ABAC roles, assignments, permissions |
| `audit.ts` | `services/audit` | Audit log entries and filters |
| `notification.ts` | `services/notification` | Email and push notification requests |
| `configuration.ts` | `services/configuration` | Tenant config and feature flags |
| `admin.ts` | `services/admin-api` | Admin dashboard, health checks, summaries |

## Type Conventions

### C# to TypeScript Mapping

| C# Type | TypeScript Type | Example |
|---------|----------------|---------|
| `Guid` | `string` | `id: string` |
| `DateTime` | `string` | ISO 8601 format: `createdAt: string` |
| `enum` | String union | `type Status = 'Active' \| 'Suspended'` |
| `Dictionary<string, string>` | `Record<string, string>` | `settings: Record<string, string>` |
| `List<T>` | `T[]` | `items: Organization[]` |
| Nullable reference | `T \| null` | `updatedAt: string \| null` |
| Optional parameter | `T?` | `description?: string` |

### Naming Conventions

- **Request DTOs**: `Create*Request`, `Update*Request`, `*Request`
- **Response DTOs**: `*Response` or type alias to domain entity
- **Domain Entities**: `Organization`, `Workspace`, `Membership`
- **Enums**: PascalCase string unions matching C# enum values exactly

## Adding New Types

When backend DTOs change:

1. **Identify the service** - Find the corresponding module (e.g., Directory → `directory.ts`)
2. **Map C# types to TypeScript** - Follow conventions above
3. **Add to module** - Create/update interface in the appropriate file
4. **Export from index** - Add named export in `src/index.ts` (alphabetically sorted)
5. **Document in README** - Update this file if adding a new module

### Example: Adding a new Directory DTO

```typescript
// 1. Add to src/directory.ts
export interface ArchiveWorkspaceRequest {
  reason?: string
}

// 2. Export from src/index.ts
export type {
  // ... existing exports
  ArchiveWorkspaceRequest,
} from './directory'
```

## What Belongs Here

**YES:**
- DTOs from backend services (request/response types)
- Domain entities exposed via API
- Enums used in API contracts
- Generic API wrappers (PagedResponse, ApiError)
- Minimal frontend-specific types that extend backend types (e.g., `TenantContext`)

**NO:**
- Vue component props (keep in component files)
- App-specific UI state types (keep in app src/types/)
- Runtime code, functions, classes
- Third-party library type extensions
- Environment-specific types

## Type Safety

This package enforces strict TypeScript settings:

- `strict: true` - All strict mode flags enabled
- `noUnusedLocals: true` - No unused local variables
- `noUnusedParameters: true` - No unused function parameters
- `forceConsistentCasingInFileNames: true` - Enforce case-sensitive imports

## Build

This package has **no build step**. Consumers import TypeScript sources directly via the `exports` map in `package.json`. TypeScript compilation happens in the consuming application.

To type-check this package:

```bash
cd packages/shared-types
pnpm exec tsc --noEmit
```

## Contributing

When adding types:
- Match backend DTOs exactly (property names, casing, nullability)
- Use English for all names, comments, documentation
- Add JSDoc comments only for non-obvious types
- Keep modules focused (one backend service per file)
- Export types alphabetically in index.ts
- No default exports (named exports only)
