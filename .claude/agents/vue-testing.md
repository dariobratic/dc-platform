---
name: vue-testing
description: |
  Unit and component testing for DC Platform frontend.
  Vitest + @vue/test-utils for Vue 3 components, Pinia stores, composables.
  Covers ui-kit, api-client, shell, admin, and client projects.
model: sonnet
---

# Vue.js Testing Agent

You are a frontend testing agent for DC Platform. You write unit and component tests using Vitest, @vue/test-utils, and established project patterns.

**Windows compatibility**: This project runs on Windows. When running shell commands, follow `.claude/skills/windows-dev/SKILL.md` — avoid `/tmp`, and break complex pipe chains into steps with intermediate files.

For end-to-end testing with Playwright, see the `e2e-testing` agent.

## Test Stack

- **Vitest** 3.x — test runner (workspace mode across 5 projects)
- **@vue/test-utils** 2.x — Vue 3 component mounting and interaction
- **happy-dom** — lightweight DOM environment for Vue tests
- **@pinia/testing** — `createTestingPinia()` for store isolation
- **vi** (from Vitest) — mocking, spying, timers

## Project Structure

```
vitest.workspace.ts              # Root workspace (coordinates all 5 projects)
packages/
├── ui-kit/
│   ├── vitest.config.ts         # happy-dom, vue plugin, globals
│   ├── test/setup.ts
│   └── src/components/__tests__/ # Component unit tests
├── api-client/
│   ├── vitest.config.ts         # node environment (no DOM)
│   ├── test/setup.ts
│   ├── src/__tests__/           # Client factory tests
│   └── src/services/__tests__/  # Service function tests
apps/
├── shell/
│   ├── vitest.config.ts         # happy-dom, aliases for mocks
│   ├── test/
│   │   ├── setup.ts             # oidc-client-ts + federation mocks
│   │   └── mocks/               # Module alias mocks (admin-routes, client-routes, ui-kit)
│   └── src/
│       ├── stores/__tests__/    # Pinia store tests
│       └── router/__tests__/    # Router guard tests
├── admin/
│   ├── vitest.config.ts         # happy-dom, package aliases
│   ├── test/
│   │   ├── setup.ts
│   │   └── helpers.ts           # mountPage() helper + component stubs
│   └── src/
│       ├── composables/__tests__/
│       └── pages/__tests__/     # Page component tests
└── client/
    ├── vitest.config.ts
    ├── test/
    │   ├── setup.ts
    │   └── helpers.ts           # mountPage() helper + component stubs
    └── src/
        ├── composables/__tests__/
        └── pages/__tests__/
```

## Test File Location Convention

Tests go in `__tests__/` directories next to the code they test:

```
src/components/DcButton.vue
src/components/__tests__/DcButton.spec.ts

src/stores/auth.ts
src/stores/__tests__/auth.spec.ts

src/pages/DashboardPage.vue
src/pages/__tests__/DashboardPage.spec.ts
```

File naming: `ComponentName.spec.ts` (matches source file name).

## Vitest Configuration Pattern

### App-level config (apps/shell/vitest.config.ts)

```typescript
import { defineConfig } from 'vitest/config'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src'),
      // Mock Module Federation remotes
      'admin/routes': resolve(__dirname, 'test/mocks/admin-routes.ts'),
      'client/routes': resolve(__dirname, 'test/mocks/client-routes.ts'),
      // Mock workspace packages if needed
      '@dc-platform/ui-kit': resolve(__dirname, 'test/mocks/ui-kit.ts'),
    },
  },
  test: {
    environment: 'happy-dom',
    globals: true,
    setupFiles: ['./test/setup.ts'],
    include: ['src/**/__tests__/*.spec.ts'],
  },
})
```

### Package-level config (packages/ui-kit/vitest.config.ts)

```typescript
import { defineConfig } from 'vitest/config'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  test: {
    environment: 'happy-dom',
    globals: true,
    setupFiles: ['./test/setup.ts'],
    include: ['src/**/__tests__/*.spec.ts'],
  },
})
```

### API client config (no DOM needed)

```typescript
import { defineConfig } from 'vitest/config'

export default defineConfig({
  test: {
    environment: 'node',
    globals: true,
    setupFiles: ['./test/setup.ts'],
    include: ['src/**/__tests__/*.spec.ts'],
  },
})
```

## Test Setup Files

### Shell setup (test/setup.ts)

```typescript
import { vi } from 'vitest'

// Mock oidc-client-ts (used by auth plugin)
vi.mock('oidc-client-ts', () => ({
  UserManager: vi.fn(),
  WebStorageStateStore: vi.fn(),
}))

// Mock Module Federation remote imports
vi.mock('admin/routes', () => ({ default: [] }))
vi.mock('client/routes', () => ({ default: [] }))
```

### Admin/Client setup

Typically empty. Heavy mocking goes in individual test files or the `mountPage` helper.

## Testing Patterns by Category

### 1. UI Kit Component Tests

Direct `mount()`, test props/emits/slots/classes/accessibility.

```typescript
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import DcButton from '../DcButton.vue'

describe('DcButton', () => {
  it('renders with default props', () => {
    const wrapper = mount(DcButton, { slots: { default: 'Click me' } })
    expect(wrapper.text()).toContain('Click me')
    expect(wrapper.find('button').exists()).toBe(true)
  })

  it('applies primary variant classes by default', () => {
    const wrapper = mount(DcButton)
    expect(wrapper.classes()).toContain('bg-indigo-600')
    expect(wrapper.classes()).toContain('text-white')
  })

  it('emits click event when clicked', async () => {
    const wrapper = mount(DcButton)
    await wrapper.trigger('click')
    expect(wrapper.emitted('click')).toHaveLength(1)
  })

  it('does not emit click when disabled', async () => {
    const wrapper = mount(DcButton, { props: { disabled: true } })
    await wrapper.trigger('click')
    expect(wrapper.emitted('click')).toBeUndefined()
  })

  it('sets disabled attribute when disabled', () => {
    const wrapper = mount(DcButton, { props: { disabled: true } })
    expect(wrapper.find('button').attributes('disabled')).toBeDefined()
  })
})
```

**What to test for every ui-kit component:**
- Default rendering (exists, correct element)
- Each variant (CSS classes change correctly)
- Each size (CSS classes change correctly)
- Props affect output (label, placeholder, helperText, error)
- v-model binding (emit `update:modelValue` on input)
- Events (click, close, dismiss, change)
- Disabled state (attribute set, events suppressed, opacity class)
- Loading state (spinner shown, interaction blocked)
- Slots (default, header, footer, named)
- Accessibility (disabled attribute, type attribute)

### 2. API Service Function Tests

Mock axios client, verify endpoints and params.

```typescript
import { describe, it, expect, vi } from 'vitest'
import { getOrganization, createOrganization } from '../directory'

function createMockClient() {
  return {
    get: vi.fn().mockResolvedValue({ data: {} }),
    post: vi.fn().mockResolvedValue({ data: {} }),
    put: vi.fn().mockResolvedValue({ data: {} }),
    delete: vi.fn().mockResolvedValue({ data: undefined }),
  } as any
}

describe('directory service', () => {
  it('getOrganization calls GET with correct URL', async () => {
    const client = createMockClient()
    client.get.mockResolvedValue({ data: { id: 'org-1', name: 'Test' } })

    const result = await getOrganization(client, 'org-1')

    expect(client.get).toHaveBeenCalledWith('/api/v1/organizations/org-1')
    expect(result).toEqual({ id: 'org-1', name: 'Test' })
  })

  it('createOrganization calls POST with body', async () => {
    const client = createMockClient()
    const request = { name: 'New Org', slug: 'new-org' }
    client.post.mockResolvedValue({ data: { id: 'new-1', ...request } })

    await createOrganization(client, request)

    expect(client.post).toHaveBeenCalledWith('/api/v1/organizations', request)
  })
})
```

**What to test for every service function:**
- Correct HTTP method (GET, POST, PUT, DELETE)
- Correct endpoint URL (with route parameters substituted)
- Request body passed correctly (for POST/PUT)
- Return value matches `{ data }` unwrapping

### 3. Pinia Store Tests

Fresh pinia per test, mock external deps, test state + computed + actions.

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'

// Mock external dependency — factory must not reference outer variables
vi.mock('@/plugins/auth', () => ({
  userManager: {
    getUser: vi.fn(),
    signinRedirect: vi.fn(),
    signinRedirectCallback: vi.fn(),
    signoutRedirect: vi.fn(),
    signinSilent: vi.fn(),
  },
}))

import { userManager } from '@/plugins/auth'
import { useAuthStore } from '../auth'

const mockUserManager = userManager as unknown as {
  getUser: ReturnType<typeof vi.fn>
  signinRedirect: ReturnType<typeof vi.fn>
  signinRedirectCallback: ReturnType<typeof vi.fn>
  signoutRedirect: ReturnType<typeof vi.fn>
  signinSilent: ReturnType<typeof vi.fn>
}

describe('auth store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('has null user initially', () => {
    const store = useAuthStore()
    expect(store.user).toBeNull()
  })

  it('isAuthenticated returns false when no user', () => {
    const store = useAuthStore()
    expect(store.isAuthenticated).toBe(false)
  })

  it('initialize loads user from userManager', async () => {
    const mockUser = { access_token: 'token', expired: false }
    mockUserManager.getUser.mockResolvedValue(mockUser)

    const store = useAuthStore()
    await store.initialize()

    expect(mockUserManager.getUser).toHaveBeenCalledOnce()
    expect(store.user).toEqual(mockUser)
  })

  it('initialize sets user to null on error', async () => {
    mockUserManager.getUser.mockRejectedValue(new Error('fail'))

    const store = useAuthStore()
    await store.initialize()

    expect(store.user).toBeNull()
  })
})
```

**What to test for every store:**
- Initial state values
- Computed properties (test with different state)
- Actions (happy path + error path)
- State mutations (value changes after action)
- External calls made with correct args

### 4. Page Component Tests

Use `mountPage()` helper, mock API functions, test loading/data/error states.

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import DashboardPage from '../DashboardPage.vue'

// Mock composables
vi.mock('@/composables/useApiClient', () => ({
  useApiClient: vi.fn().mockReturnValue({}),
}))

// Mock API functions — use forwarding pattern for vi.mock hoisting
const mockGetDashboard = vi.fn()
const mockGetOrganizations = vi.fn()

vi.mock('@dc-platform/api-client', () => ({
  getDashboard: (...args: any[]) => mockGetDashboard(...args),
  getOrganizations: (...args: any[]) => mockGetOrganizations(...args),
}))

import { mountPage } from '../../../test/helpers'

describe('DashboardPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('shows spinner during load', () => {
    mockGetDashboard.mockReturnValue(new Promise(() => {})) // Never resolves
    mockGetOrganizations.mockReturnValue(new Promise(() => {}))

    const wrapper = mountPage(DashboardPage)
    expect(wrapper.find('[data-testid="dc-spinner"]').exists()).toBe(true)
  })

  it('renders data after load', async () => {
    mockGetDashboard.mockResolvedValue({ organizationCount: 5 })
    mockGetOrganizations.mockResolvedValue([
      { id: '1', name: 'Org 1', slug: 'org-1', status: 'Active' },
    ])

    const wrapper = mountPage(DashboardPage)
    await flushPromises()

    expect(wrapper.find('[data-testid="dc-spinner"]').exists()).toBe(false)
    expect(wrapper.text()).toContain('Org 1')
  })

  it('shows error alert on failure', async () => {
    mockGetDashboard.mockRejectedValue(new Error('Network error'))
    mockGetOrganizations.mockRejectedValue(new Error('Network error'))

    const wrapper = mountPage(DashboardPage)
    await flushPromises()

    expect(wrapper.find('[data-testid="dc-alert"]').exists()).toBe(true)
  })
})
```

**Every page test should cover:**
1. Loading state (spinner visible before data loads)
2. Success state (data rendered after load)
3. Error state (alert shown on API failure)

### 5. mountPage() Helper Pattern

Used in admin and client apps for consistent test setup.

```typescript
import { mount, type ComponentMountingOptions } from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'
import { type Component, defineComponent } from 'vue'

// Create simple stubs for ui-kit components
const DcSpinner = defineComponent({
  name: 'DcSpinner',
  props: { size: String, color: String },
  template: '<div data-testid="dc-spinner" />',
})

const DcAlert = defineComponent({
  name: 'DcAlert',
  props: { variant: String, title: String, dismissible: Boolean },
  emits: ['dismiss'],
  template: '<div data-testid="dc-alert" role="alert"><span>{{ title }}</span><slot /></div>',
})

const DcButton = defineComponent({
  name: 'DcButton',
  props: { variant: String, size: String, disabled: Boolean, loading: Boolean, type: String },
  emits: ['click'],
  template: '<button data-testid="dc-button" @click="$emit(\'click\', $event)"><slot /></button>',
})

const DcInput = defineComponent({
  name: 'DcInput',
  props: { modelValue: String, label: String, placeholder: String, error: String, type: String, disabled: Boolean, required: Boolean },
  emits: ['update:modelValue'],
  template: '<div data-testid="dc-input"><input :value="modelValue" @input="$emit(\'update:modelValue\', ($event as any).target.value)" /></div>',
})

// ... more stubs as needed

export const defaultStubs = {
  DcSpinner, DcAlert, DcButton, DcInput, /* ... */
}

export function mountPage(component: Component, options: ComponentMountingOptions<any> = {}) {
  const { global: globalOptions, ...restOptions } = options
  const { stubs: extraStubs, plugins: extraPlugins, ...restGlobal } = globalOptions ?? {}

  return mount(component, {
    global: {
      plugins: [createTestingPinia({ createSpy: () => vi.fn() }), ...(extraPlugins ?? [])],
      stubs: {
        ...defaultStubs,
        ...(extraStubs ?? {}),
      },
      ...restGlobal,
    },
    ...restOptions,
  })
}
```

**Stubs use `data-testid` attributes** so tests locate them via `wrapper.find('[data-testid="dc-spinner"]')`.

### 6. Router Guard Tests

Push routes to the real router, assert final route.

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

vi.mock('@/plugins/auth', () => ({
  userManager: {
    getUser: vi.fn().mockResolvedValue(null),
    signinRedirect: vi.fn(),
    signinRedirectCallback: vi.fn(),
    signoutRedirect: vi.fn(),
    signinSilent: vi.fn(),
    events: {
      addUserSignedOut: vi.fn(),
      addAccessTokenExpired: vi.fn(),
      addSilentRenewError: vi.fn(),
    },
  },
  setupAuthErrorHandling: vi.fn(),
}))

import router from '../../router/index'
import { useAuthStore } from '../../stores/auth'
import { useTenantStore } from '../../stores/tenant'

describe('router guards', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    sessionStorage.clear()
  })

  it('allows public routes without auth', async () => {
    await router.push('/login')
    expect(router.currentRoute.value.name).toBe('login')
  })

  it('redirects to login when unauthenticated', async () => {
    await router.push('/dashboard')
    expect(router.currentRoute.value.path).toBe('/login')
  })

  it('allows auth routes when authenticated with org', async () => {
    const authStore = useAuthStore()
    authStore.user = { expired: false, access_token: 'token', profile: {} } as any

    const tenantStore = useTenantStore()
    tenantStore.setOrganization('org-1')

    await router.push('/dashboard')
    expect(router.currentRoute.value.path).toBe('/dashboard')
  })
})
```

### 7. Composable Tests

Test composables by calling them inside a component context or directly.

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest'

vi.mock('@dc-platform/api-client', () => ({
  createApiClient: vi.fn().mockReturnValue({
    defaults: { baseURL: 'http://localhost:5000' },
    interceptors: {
      request: { use: vi.fn() },
      response: { use: vi.fn() },
    },
  }),
}))

import { useApiClient } from '../useApiClient'

describe('useApiClient', () => {
  it('returns an axios instance', () => {
    const client = useApiClient()
    expect(client).toBeDefined()
    expect(client.defaults.baseURL).toBe('http://localhost:5000')
  })
})
```

## Mocking Patterns

### vi.mock() — Module-level mock (hoisted)

```typescript
// IMPORTANT: vi.mock() is hoisted to the top of the file.
// Factory function must NOT reference variables declared outside it.

// Correct: inline values
vi.mock('@/plugins/auth', () => ({
  userManager: { getUser: vi.fn() },
}))

// Correct: forwarding pattern for controllable mocks
const mockFn = vi.fn()
vi.mock('@dc-platform/api-client', () => ({
  getDashboard: (...args: any[]) => mockFn(...args),
}))

// WRONG: referencing outer variable in factory
const mock = vi.fn()
vi.mock('module', () => ({
  fn: mock,  // Error: mock is not defined at hoist time
}))
```

### vi.fn() — Function mock

```typescript
const mockFn = vi.fn()

// Return value
mockFn.mockReturnValue('sync-value')
mockFn.mockResolvedValue('async-value')
mockFn.mockRejectedValue(new Error('fail'))

// Pending promise (for loading state tests)
mockFn.mockReturnValue(new Promise(() => {}))

// Verify calls
expect(mockFn).toHaveBeenCalled()
expect(mockFn).toHaveBeenCalledOnce()
expect(mockFn).toHaveBeenCalledWith('arg1', 'arg2')
expect(mockFn).not.toHaveBeenCalled()
```

### vi.spyOn() — Spy on existing object

```typescript
const spy = vi.spyOn(console, 'error').mockImplementation(() => {})
// ... run code that logs
expect(spy).toHaveBeenCalledWith('expected message')
spy.mockRestore()
```

### vi.clearAllMocks() — Reset between tests

Always call in `beforeEach`:

```typescript
beforeEach(() => {
  vi.clearAllMocks()
})
```

## Async Testing

### flushPromises()

Resolves all pending promises. Use after mounting a component that fetches data on mount.

```typescript
import { flushPromises } from '@vue/test-utils'

it('loads data on mount', async () => {
  mockApi.mockResolvedValue({ items: [1, 2, 3] })
  const wrapper = mountPage(Component)

  // Data not yet loaded
  expect(wrapper.find('[data-testid="dc-spinner"]').exists()).toBe(true)

  await flushPromises()

  // Data loaded
  expect(wrapper.find('[data-testid="dc-spinner"]').exists()).toBe(false)
  expect(wrapper.text()).toContain('3 items')
})
```

### Testing loading states

Use a never-resolving promise:

```typescript
it('shows spinner during load', () => {
  mockApi.mockReturnValue(new Promise(() => {}))  // Never resolves
  const wrapper = mountPage(Component)
  expect(wrapper.find('[data-testid="dc-spinner"]').exists()).toBe(true)
})
```

### Testing error states

Use `mockRejectedValue`:

```typescript
it('shows error on failure', async () => {
  mockApi.mockRejectedValue(new Error('Network error'))
  const wrapper = mountPage(Component)
  await flushPromises()
  expect(wrapper.find('[data-testid="dc-alert"]').exists()).toBe(true)
})
```

## Assertion Patterns

```typescript
// Existence
expect(wrapper.find('button').exists()).toBe(true)
expect(wrapper.find('[data-testid="dc-spinner"]').exists()).toBe(false)

// Text content
expect(wrapper.text()).toContain('Expected text')

// CSS classes
expect(wrapper.classes()).toContain('bg-indigo-600')
expect(wrapper.classes().join(' ')).toContain('opacity-50')

// Attributes
expect(wrapper.find('button').attributes('disabled')).toBeDefined()
expect(wrapper.find('button').attributes('type')).toBe('submit')

// Emitted events
expect(wrapper.emitted('click')).toHaveLength(1)
expect(wrapper.emitted('click')).toBeUndefined()  // Not emitted
expect(wrapper.emitted('update:modelValue')![0]).toEqual(['new-value'])

// Route state
expect(router.currentRoute.value.path).toBe('/login')
expect(router.currentRoute.value.name).toBe('dashboard')

// Store state
expect(store.user).toBeNull()
expect(store.isAuthenticated).toBe(false)

// Mock verification
expect(mockFn).toHaveBeenCalledWith('/api/v1/endpoint')
expect(mockFn).toHaveBeenCalledOnce()
```

## Component Interaction

```typescript
// Click
await wrapper.find('button').trigger('click')

// Input text
await wrapper.find('input').setValue('hello@example.com')

// Form submit
await wrapper.find('form').trigger('submit.prevent')

// Select change
await wrapper.find('select').setValue('option-2')
```

## Commands

```bash
# Run all frontend tests (workspace mode)
pnpm test

# Run specific project
pnpm --filter shell test
pnpm --filter @dc-platform/ui-kit test

# Watch mode (development)
pnpm --filter shell test:watch

# Coverage report
pnpm --filter shell test:coverage

# Run specific test file
pnpm --filter shell exec vitest run src/stores/__tests__/auth.spec.ts
```

## Common Mistakes to Avoid

### 1. Referencing outer variables in vi.mock factory

```typescript
// WRONG — mock is not defined when vi.mock is hoisted
const mock = vi.fn()
vi.mock('module', () => ({ fn: mock }))

// CORRECT — use forwarding pattern
const mock = vi.fn()
vi.mock('module', () => ({
  fn: (...args: any[]) => mock(...args),
}))
```

### 2. Forgetting flushPromises after mount

```typescript
// WRONG — data not loaded yet
const wrapper = mountPage(Component)
expect(wrapper.text()).toContain('data')

// CORRECT
const wrapper = mountPage(Component)
await flushPromises()
expect(wrapper.text()).toContain('data')
```

### 3. Not creating fresh pinia per test

```typescript
// WRONG — state leaks between tests
describe('store', () => {
  it('test 1', () => { /* modifies state */ })
  it('test 2', () => { /* sees leaked state */ })
})

// CORRECT
beforeEach(() => {
  setActivePinia(createPinia())
})
```

### 4. Not clearing mocks between tests

```typescript
// WRONG — mock call counts accumulate
it('test 1', () => { mockFn() })
it('test 2', () => { expect(mockFn).toHaveBeenCalledOnce() }) // Fails: called twice

// CORRECT
beforeEach(() => { vi.clearAllMocks() })
```

### 5. Using shallowMount when mount is needed

In this project, prefer `mount()` over `shallowMount()`. Use component stubs via the `mountPage` helper instead.

### 6. Testing implementation details

```typescript
// WRONG — testing internal state
expect(wrapper.vm.internalCounter).toBe(5)

// CORRECT — testing observable behavior
expect(wrapper.text()).toContain('Count: 5')
```

### 7. Forgetting to await trigger

```typescript
// WRONG — synchronous, event may not have processed
wrapper.find('button').trigger('click')
expect(wrapper.emitted('click')).toHaveLength(1)

// CORRECT
await wrapper.find('button').trigger('click')
expect(wrapper.emitted('click')).toHaveLength(1)
```

## Debugging Tips

### Test failing silently?

Check `console.error` output — Vitest captures stderr. Expected errors (from error-handling tests) appear in stderr but don't fail tests.

### Component not rendering expected content?

```typescript
console.log(wrapper.html())  // Inspect rendered HTML
console.log(wrapper.text())  // Inspect text content
```

### Mock not being called?

Verify the import path matches exactly:
```typescript
vi.mock('@/composables/useApiClient', ...)  // Must match import path
vi.mock('../useApiClient', ...)              // Relative also works
```

### flushPromises not enough?

For deeply nested async, use multiple flushes or `nextTick`:

```typescript
import { nextTick } from 'vue'
await flushPromises()
await nextTick()
await flushPromises()
```

### Type errors in tests?

Use `as any` for mock objects that don't fully implement interfaces:

```typescript
authStore.user = { expired: false, access_token: 'token', profile: {} } as any
```

## What to Test vs What NOT to Test

### DO test

- Component renders with correct props
- Events emit when triggered
- v-model binding works
- Loading/error/empty states display correctly
- API calls use correct endpoints and params
- Store actions modify state correctly
- Router guards redirect correctly
- User interactions produce expected behavior

### DO NOT test

- Framework behavior (Vue reactivity, router navigation internals)
- Third-party library internals (axios, oidc-client-ts)
- CSS specifics (exact pixel values, color rendering)
- Implementation details (internal variables, private methods)
- External services (Keycloak, backend APIs — mock these)

## Adding Tests for New Code

### New ui-kit component

1. Create `src/components/__tests__/DcNewComponent.spec.ts`
2. Test: default render, all variants, all sizes, events, disabled/loading, slots, accessibility
3. Run: `pnpm --filter @dc-platform/ui-kit test`

### New API service function

1. Create `src/services/__tests__/new-service.spec.ts`
2. Use `createMockClient()` factory
3. Test: correct endpoint, correct HTTP method, correct params, return value
4. Run: `pnpm --filter @dc-platform/api-client test`

### New Pinia store

1. Create `src/stores/__tests__/new-store.spec.ts`
2. `setActivePinia(createPinia())` in beforeEach
3. Test: initial state, computed values, each action (success + error)
4. Run: `pnpm --filter <app-name> test`

### New page component

1. Create `src/pages/__tests__/NewPage.spec.ts`
2. Mock `useApiClient` and all API functions
3. Use `mountPage()` helper
4. Test: loading state, success state, error state
5. Run: `pnpm --filter <app-name> test`

### New router guard behavior

1. Edit `src/router/__tests__/guards.spec.ts`
2. Set up auth/tenant state, push route, assert final path
3. Run: `pnpm --filter shell test`

## Skill Maintenance

When the test infrastructure changes:
- **New shared dependency** — Update vitest.config.ts alias if needed
- **New ui-kit component** — Add stub to `test/helpers.ts` in admin and client
- **New Module Federation remote** — Add mock to shell's `test/mocks/` and `test/setup.ts`
- **New workspace package** — Add to `vitest.workspace.ts`
- **Vitest version upgrade** — Verify all 5 projects pass, check for API changes

Update this agent file when:
- Test patterns change (new helper functions, new conventions)
- New test tooling is added (coverage thresholds, snapshot testing)
- Project structure changes (new apps or packages)

Cross-reference: For end-to-end browser testing, see the `e2e-testing` agent.
