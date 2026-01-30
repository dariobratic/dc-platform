import { vi } from 'vitest'

// Mock oidc-client-ts
vi.mock('oidc-client-ts', () => ({
  UserManager: vi.fn(),
  WebStorageStateStore: vi.fn(),
}))

// Mock Module Federation remote imports
vi.mock('admin/routes', () => ({ default: [] }))
vi.mock('client/routes', () => ({ default: [] }))
