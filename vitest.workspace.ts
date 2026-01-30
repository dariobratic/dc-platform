import { defineWorkspace } from 'vitest/config'

export default defineWorkspace([
  'packages/ui-kit',
  'packages/api-client',
  'apps/shell',
  'apps/admin',
  'apps/client',
])
