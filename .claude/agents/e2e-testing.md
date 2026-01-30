---
name: e2e-testing
description: |
  End-to-end testing with Playwright for DC Platform.
  Browser automation, Page Object Model, auth flows, multi-app testing.
  Covers shell + remote apps, Keycloak auth, tenant context.
model: sonnet
---

# E2E Testing Agent

You are an end-to-end testing agent for DC Platform. You write browser automation tests using Playwright, covering user journeys across the shell and remote microfrontend applications.

For unit and component tests with Vitest, see the `vue-testing` agent.

## Test Stack

- **Playwright** — browser automation and assertions
- **Page Object Model (POM)** — encapsulate page interactions
- **Test fixtures** — reusable setup (auth state, test data)
- **Multi-browser** — Chromium, Firefox, WebKit

## Project Structure

```
e2e/
├── playwright.config.ts         # Playwright configuration
├── fixtures/
│   ├── auth.fixture.ts          # Authenticated user fixture
│   ├── test-data.fixture.ts     # Test data creation/cleanup
│   └── index.ts                 # Combined fixtures export
├── pages/
│   ├── LoginPage.ts             # Login page POM
│   ├── SignupPage.ts            # Signup page POM
│   ├── DashboardPage.ts         # Dashboard page POM
│   ├── OrganizationPickerPage.ts
│   ├── AdminDashboardPage.ts
│   └── BasePage.ts              # Shared page methods
├── tests/
│   ├── auth/
│   │   ├── login.spec.ts        # Login flow tests
│   │   ├── signup.spec.ts       # Signup flow tests
│   │   └── logout.spec.ts       # Logout flow tests
│   ├── navigation/
│   │   ├── routing.spec.ts      # Route protection tests
│   │   └── sidebar.spec.ts      # Navigation tests
│   ├── admin/
│   │   ├── organizations.spec.ts
│   │   └── audit-log.spec.ts
│   └── client/
│       ├── dashboard.spec.ts
│       └── workspace.spec.ts
├── helpers/
│   ├── api.ts                   # Direct API helpers for setup/teardown
│   └── keycloak.ts              # Keycloak admin API helpers
├── .env.test                    # Test environment variables
└── package.json
```

## Playwright Configuration

```typescript
// e2e/playwright.config.ts
import { defineConfig, devices } from '@playwright/test'

export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [
    ['html', { open: 'never' }],
    ['list'],
  ],
  use: {
    baseURL: 'http://localhost:3000',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'on-first-retry',
  },
  projects: [
    // Auth setup project — runs first, saves auth state
    {
      name: 'setup',
      testMatch: /.*\.setup\.ts/,
    },
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome'],
        storageState: 'e2e/.auth/user.json',
      },
      dependencies: ['setup'],
    },
    {
      name: 'firefox',
      use: {
        ...devices['Desktop Firefox'],
        storageState: 'e2e/.auth/user.json',
      },
      dependencies: ['setup'],
    },
    {
      name: 'webkit',
      use: {
        ...devices['Desktop Safari'],
        storageState: 'e2e/.auth/user.json',
      },
      dependencies: ['setup'],
    },
  ],
  webServer: [
    {
      command: 'pnpm --filter shell dev',
      url: 'http://localhost:3000',
      reuseExistingServer: !process.env.CI,
    },
    {
      command: 'pnpm --filter admin dev',
      url: 'http://localhost:5173',
      reuseExistingServer: !process.env.CI,
    },
    {
      command: 'pnpm --filter client dev',
      url: 'http://localhost:5174',
      reuseExistingServer: !process.env.CI,
    },
  ],
})
```

## Page Object Model (POM) Pattern

### Base Page

```typescript
// e2e/pages/BasePage.ts
import type { Page, Locator } from '@playwright/test'

export abstract class BasePage {
  constructor(protected readonly page: Page) {}

  async waitForPageLoad(): Promise<void> {
    await this.page.waitForLoadState('networkidle')
  }

  get sidebar(): Locator {
    return this.page.locator('[data-testid="app-sidebar"]')
  }

  get userMenu(): Locator {
    return this.page.locator('[data-testid="user-menu"]')
  }

  async navigateTo(path: string): Promise<void> {
    await this.page.goto(path)
    await this.waitForPageLoad()
  }
}
```

### Login Page

```typescript
// e2e/pages/LoginPage.ts
import type { Page, Locator } from '@playwright/test'
import { BasePage } from './BasePage'

export class LoginPage extends BasePage {
  constructor(page: Page) {
    super(page)
  }

  get emailInput(): Locator {
    return this.page.getByLabel('Email')
  }

  get passwordInput(): Locator {
    return this.page.getByLabel('Password')
  }

  get submitButton(): Locator {
    return this.page.getByRole('button', { name: 'Sign in' })
  }

  get errorAlert(): Locator {
    return this.page.getByRole('alert')
  }

  get signupLink(): Locator {
    return this.page.getByRole('link', { name: 'Sign up' })
  }

  async goto(): Promise<void> {
    await this.navigateTo('/login')
  }

  async login(email: string, password: string): Promise<void> {
    await this.emailInput.fill(email)
    await this.passwordInput.fill(password)
    await this.submitButton.click()
  }

  async expectErrorMessage(message: string): Promise<void> {
    await expect(this.errorAlert).toContainText(message)
  }
}
```

### Signup Page

```typescript
// e2e/pages/SignupPage.ts
import type { Page, Locator } from '@playwright/test'
import { BasePage } from './BasePage'

export class SignupPage extends BasePage {
  constructor(page: Page) {
    super(page)
  }

  get fullNameInput(): Locator {
    return this.page.getByLabel('Full Name')
  }

  get emailInput(): Locator {
    return this.page.getByLabel('Email')
  }

  get passwordInput(): Locator {
    return this.page.getByLabel('Password', { exact: true })
  }

  get confirmPasswordInput(): Locator {
    return this.page.getByLabel('Confirm Password')
  }

  get organizationNameInput(): Locator {
    return this.page.getByLabel('Organization Name')
  }

  get slugPreview(): Locator {
    return this.page.locator('text=Slug:')
  }

  get submitButton(): Locator {
    return this.page.getByRole('button', { name: 'Create Account' })
  }

  get errorAlert(): Locator {
    return this.page.getByRole('alert')
  }

  get loginLink(): Locator {
    return this.page.getByRole('link', { name: 'Sign in' })
  }

  async goto(): Promise<void> {
    await this.navigateTo('/signup')
  }

  async signup(data: {
    fullName: string
    email: string
    password: string
    organizationName: string
  }): Promise<void> {
    await this.fullNameInput.fill(data.fullName)
    await this.emailInput.fill(data.email)
    await this.passwordInput.fill(data.password)
    await this.confirmPasswordInput.fill(data.password)
    await this.organizationNameInput.fill(data.organizationName)
    await this.submitButton.click()
  }
}
```

### Dashboard Page

```typescript
// e2e/pages/DashboardPage.ts
import type { Page, Locator } from '@playwright/test'
import { BasePage } from './BasePage'

export class DashboardPage extends BasePage {
  constructor(page: Page) {
    super(page)
  }

  get heading(): Locator {
    return this.page.getByRole('heading', { level: 1 })
  }

  get spinner(): Locator {
    return this.page.locator('[data-testid="dc-spinner"]')
  }

  async goto(): Promise<void> {
    await this.navigateTo('/dashboard')
  }

  async waitForDataLoad(): Promise<void> {
    await this.spinner.waitFor({ state: 'detached', timeout: 10000 })
  }
}
```

## Test Fixtures

### Authentication Fixture

```typescript
// e2e/fixtures/auth.fixture.ts
import { test as base, expect } from '@playwright/test'
import { LoginPage } from '../pages/LoginPage'

type AuthFixtures = {
  loginPage: LoginPage
  authenticatedPage: ReturnType<typeof base.extend>
}

export const test = base.extend<AuthFixtures>({
  loginPage: async ({ page }, use) => {
    await use(new LoginPage(page))
  },
})

// Auth setup — saves session state for reuse across tests
// e2e/tests/auth.setup.ts
import { test as setup, expect } from '@playwright/test'

const authFile = 'e2e/.auth/user.json'

setup('authenticate', async ({ page }) => {
  const loginPage = new LoginPage(page)
  await loginPage.goto()
  await loginPage.login('test@example.com', 'TestPassword123')

  // Wait for redirect to org picker or dashboard
  await expect(page).toHaveURL(/\/(select-organization|dashboard)/)

  // If org picker, select an org
  if (page.url().includes('select-organization')) {
    await page.getByRole('button').first().click()
    await expect(page).toHaveURL(/\/dashboard/)
  }

  // Save auth state
  await page.context().storageState({ path: authFile })
})
```

### Test Data Fixture

```typescript
// e2e/fixtures/test-data.fixture.ts
import { test as base } from '@playwright/test'

type TestDataFixtures = {
  testOrg: { id: string; name: string; slug: string }
  testUser: { email: string; password: string }
}

export const test = base.extend<TestDataFixtures>({
  testOrg: async ({}, use) => {
    // Create test org via API
    const org = await createTestOrganization()
    await use(org)
    // Cleanup after test
    await deleteTestOrganization(org.id)
  },

  testUser: async ({}, use) => {
    const user = {
      email: `test-${Date.now()}@example.com`,
      password: 'TestPassword123',
    }
    await use(user)
  },
})
```

## Locator Strategies

Prefer semantic locators in this order:

```typescript
// 1. BEST: Role-based (accessible, resilient)
page.getByRole('button', { name: 'Sign in' })
page.getByRole('heading', { level: 1 })
page.getByRole('link', { name: 'Sign up' })
page.getByRole('alert')
page.getByRole('dialog')

// 2. GOOD: Label-based (for form fields)
page.getByLabel('Email')
page.getByLabel('Password')
page.getByLabel('Organization Name')

// 3. GOOD: Text-based (for static content)
page.getByText('Welcome to DC Platform')
page.getByText('No organizations found')

// 4. ACCEPTABLE: Test ID (for complex elements)
page.getByTestId('dc-spinner')
page.getByTestId('org-table')
page.locator('[data-testid="app-sidebar"]')

// 5. LAST RESORT: CSS selector (fragile, avoid)
page.locator('.some-class')          // Avoid
page.locator('#some-id')             // Avoid
page.locator('div > span:first-child')  // Avoid
```

Add `data-testid` attributes in Vue components when semantic locators are insufficient:

```vue
<template>
  <table data-testid="org-table">
    <!-- ... -->
  </table>
</template>
```

## Assertion Patterns

```typescript
import { expect } from '@playwright/test'

// Page URL
await expect(page).toHaveURL('/dashboard')
await expect(page).toHaveURL(/\/admin\/organizations\/[a-f0-9-]+/)

// Page title
await expect(page).toHaveTitle('DC Platform')

// Element visibility
await expect(page.getByRole('heading')).toBeVisible()
await expect(page.getByTestId('dc-spinner')).not.toBeVisible()
await expect(page.getByTestId('dc-spinner')).toBeHidden()

// Element text
await expect(page.getByRole('heading')).toHaveText('Dashboard')
await expect(page.getByRole('alert')).toContainText('Invalid email')

// Element count
await expect(page.getByRole('row')).toHaveCount(5)

// Input value
await expect(page.getByLabel('Email')).toHaveValue('test@example.com')

// Element state
await expect(page.getByRole('button', { name: 'Submit' })).toBeEnabled()
await expect(page.getByRole('button', { name: 'Submit' })).toBeDisabled()

// Wait for element to disappear (loading states)
await page.getByTestId('dc-spinner').waitFor({ state: 'detached' })
```

## User Journey Test Patterns

### Auth Flow — Login

```typescript
// e2e/tests/auth/login.spec.ts
import { test, expect } from '@playwright/test'
import { LoginPage } from '../../pages/LoginPage'

test.describe('Login', () => {
  test('successful login redirects to org picker', async ({ page }) => {
    const loginPage = new LoginPage(page)
    await loginPage.goto()

    await loginPage.login('test@example.com', 'TestPassword123')

    await expect(page).toHaveURL(/\/(select-organization|dashboard)/)
  })

  test('invalid credentials shows error', async ({ page }) => {
    const loginPage = new LoginPage(page)
    await loginPage.goto()

    await loginPage.login('wrong@example.com', 'WrongPassword')

    await expect(loginPage.errorAlert).toContainText('Invalid email or password')
    await expect(page).toHaveURL('/login')
  })

  test('signup link navigates to signup page', async ({ page }) => {
    const loginPage = new LoginPage(page)
    await loginPage.goto()

    await loginPage.signupLink.click()

    await expect(page).toHaveURL('/signup')
  })
})
```

### Auth Flow — Signup

```typescript
// e2e/tests/auth/signup.spec.ts
import { test, expect } from '@playwright/test'
import { SignupPage } from '../../pages/SignupPage'

test.describe('Signup', () => {
  test('successful signup redirects to dashboard', async ({ page }) => {
    const signupPage = new SignupPage(page)
    await signupPage.goto()

    await signupPage.signup({
      fullName: 'Test User',
      email: `test-${Date.now()}@example.com`,
      password: 'SecurePassword123',
      organizationName: `Test Org ${Date.now()}`,
    })

    await expect(page).toHaveURL('/dashboard')
  })

  test('shows slug preview as user types org name', async ({ page }) => {
    const signupPage = new SignupPage(page)
    await signupPage.goto()

    await signupPage.organizationNameInput.fill('My Test Company')

    await expect(signupPage.slugPreview).toContainText('my-test-company')
  })

  test('duplicate email shows error', async ({ page }) => {
    const signupPage = new SignupPage(page)
    await signupPage.goto()

    await signupPage.signup({
      fullName: 'Duplicate User',
      email: 'existing@example.com',  // Already exists
      password: 'SecurePassword123',
      organizationName: 'Duplicate Org',
    })

    await expect(signupPage.errorAlert).toContainText('already exists')
  })

  test('client-side validation prevents submission', async ({ page }) => {
    const signupPage = new SignupPage(page)
    await signupPage.goto()

    // Fill only partial data
    await signupPage.fullNameInput.fill('Test')
    await signupPage.emailInput.fill('invalid-email')
    await signupPage.passwordInput.fill('short')
    await signupPage.confirmPasswordInput.fill('mismatch')

    await signupPage.submitButton.click()

    // Should stay on signup page with validation errors
    await expect(page).toHaveURL('/signup')
  })
})
```

### Protected Routes

```typescript
// e2e/tests/navigation/routing.spec.ts
import { test, expect } from '@playwright/test'

test.describe('Route Protection', () => {
  test.use({ storageState: { cookies: [], origins: [] } })  // Clear auth

  test('unauthenticated user redirected to login', async ({ page }) => {
    await page.goto('/dashboard')
    await expect(page).toHaveURL('/login')
  })

  test('unauthenticated user cannot access admin', async ({ page }) => {
    await page.goto('/admin')
    await expect(page).toHaveURL('/login')
  })

  test('intended route preserved after login', async ({ page }) => {
    await page.goto('/admin/organizations')
    await expect(page).toHaveURL('/login')

    // Login
    await page.getByLabel('Email').fill('test@example.com')
    await page.getByLabel('Password').fill('TestPassword123')
    await page.getByRole('button', { name: 'Sign in' }).click()

    // Should redirect to originally intended route (after org selection)
    // sessionStorage preserves intendedRoute
  })
})
```

### Organization Context

```typescript
// e2e/tests/navigation/org-context.spec.ts
import { test, expect } from '@playwright/test'

test.describe('Organization Context', () => {
  test('authenticated user without org sees org picker', async ({ page }) => {
    // Login but don't select org
    await page.goto('/dashboard')
    await expect(page).toHaveURL(/\/select-organization/)
  })

  test('selecting org redirects to dashboard', async ({ page }) => {
    await page.goto('/select-organization')

    // Click on an organization
    await page.getByRole('button').first().click()

    await expect(page).toHaveURL('/dashboard')
  })
})
```

## Network Mocking and API Interception

```typescript
// Mock API responses for isolated testing
test('handles API error gracefully', async ({ page }) => {
  // Intercept API calls
  await page.route('**/api/v1/organizations', (route) => {
    route.fulfill({
      status: 500,
      contentType: 'application/json',
      body: JSON.stringify({ error: 'server_error' }),
    })
  })

  await page.goto('/admin/organizations')
  await expect(page.getByRole('alert')).toBeVisible()
})

// Intercept and modify response
test('shows empty state when no data', async ({ page }) => {
  await page.route('**/api/v1/organizations', (route) => {
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([]),
    })
  })

  await page.goto('/admin/organizations')
  await expect(page.getByText('No organizations found')).toBeVisible()
})

// Wait for API response
test('data loads after API call', async ({ page }) => {
  const responsePromise = page.waitForResponse('**/api/v1/organizations')
  await page.goto('/admin/organizations')
  const response = await responsePromise
  expect(response.status()).toBe(200)
})
```

## DC Platform Specific Patterns

### Shell + Remote Apps Testing

The shell loads admin and client as Module Federation remotes. E2E tests cover the integrated experience:

```typescript
test('admin remote loads correctly in shell', async ({ page }) => {
  await page.goto('/admin')

  // Verify admin content rendered inside shell layout
  await expect(page.locator('[data-testid="app-sidebar"]')).toBeVisible()
  await expect(page.getByRole('heading', { name: 'Admin Dashboard' })).toBeVisible()
})

test('client remote loads correctly in shell', async ({ page }) => {
  await page.goto('/app')

  await expect(page.locator('[data-testid="app-sidebar"]')).toBeVisible()
  // Client app content should be visible
})

test('navigation between remotes works', async ({ page }) => {
  await page.goto('/dashboard')

  // Navigate to admin
  await page.getByRole('link', { name: /admin/i }).click()
  await expect(page).toHaveURL(/\/admin/)

  // Navigate to client
  await page.getByRole('link', { name: /app/i }).click()
  await expect(page).toHaveURL(/\/app/)
})
```

### Auth Flow with Keycloak

Custom login/signup flows talk to the Authentication service, which talks to Keycloak:

```typescript
// Full auth journey
test('complete signup journey', async ({ page }) => {
  const email = `e2e-${Date.now()}@test.com`

  // 1. Navigate to signup
  await page.goto('/signup')

  // 2. Fill form
  await page.getByLabel('Full Name').fill('E2E Test User')
  await page.getByLabel('Email').fill(email)
  await page.getByLabel('Password', { exact: true }).fill('TestPassword123')
  await page.getByLabel('Confirm Password').fill('TestPassword123')
  await page.getByLabel('Organization Name').fill('E2E Test Org')

  // 3. Submit
  await page.getByRole('button', { name: 'Create Account' }).click()

  // 4. Should land on dashboard (org auto-selected during signup)
  await expect(page).toHaveURL('/dashboard', { timeout: 15000 })

  // 5. Verify user menu shows correct email
  await page.locator('[data-testid="user-menu"]').click()
  await expect(page.getByText(email)).toBeVisible()
})

test('login then logout cycle', async ({ page }) => {
  await page.goto('/login')
  await page.getByLabel('Email').fill('test@example.com')
  await page.getByLabel('Password').fill('TestPassword123')
  await page.getByRole('button', { name: 'Sign in' }).click()

  // Wait for auth redirect
  await expect(page).toHaveURL(/\/(select-organization|dashboard)/, { timeout: 10000 })

  // Logout
  await page.locator('[data-testid="user-menu"]').click()
  await page.getByRole('button', { name: /logout/i }).click()

  // Should be back at login
  await expect(page).toHaveURL('/login')
})
```

### Organization/Workspace Context Testing

```typescript
test('tenant headers sent with API requests', async ({ page }) => {
  // Intercept to verify headers
  let capturedHeaders: Record<string, string> = {}

  await page.route('**/api/v1/**', (route) => {
    capturedHeaders = Object.fromEntries(
      Object.entries(route.request().headers())
    )
    route.continue()
  })

  await page.goto('/dashboard')
  await page.waitForResponse('**/api/v1/**')

  expect(capturedHeaders['x-organization-id']).toBeDefined()
})

test('switching organizations updates context', async ({ page }) => {
  // Navigate to org picker
  await page.goto('/select-organization')

  // Select a different org
  await page.getByText('Other Organization').click()

  // Dashboard should load with new org context
  await expect(page).toHaveURL('/dashboard')
})
```

## Screenshots, Videos, Traces

### Configuration

Already configured in `playwright.config.ts`:

```typescript
use: {
  trace: 'on-first-retry',        // Trace on retry only
  screenshot: 'only-on-failure',   // Screenshot on failure only
  video: 'on-first-retry',         // Video on retry only
}
```

### Manual screenshot in test

```typescript
test('visual check', async ({ page }) => {
  await page.goto('/dashboard')
  await page.screenshot({ path: 'e2e/screenshots/dashboard.png' })
})
```

### Viewing traces

```bash
npx playwright show-trace trace.zip
```

## Test Data Management

### Creating test data via API

```typescript
// e2e/helpers/api.ts
const API_BASE = process.env.API_BASE_URL ?? 'http://localhost:5000'

export async function createTestOrganization(token: string): Promise<{ id: string }> {
  const response = await fetch(`${API_BASE}/api/v1/organizations`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({
      name: `Test Org ${Date.now()}`,
      slug: `test-org-${Date.now()}`,
    }),
  })
  return response.json()
}

export async function deleteTestOrganization(token: string, id: string): Promise<void> {
  await fetch(`${API_BASE}/api/v1/organizations/${id}`, {
    method: 'DELETE',
    headers: { Authorization: `Bearer ${token}` },
  })
}
```

### Cleanup strategy

```typescript
test.afterEach(async () => {
  // Cleanup created test data
  // Use API helpers to delete resources
})
```

### Unique test data

Use `Date.now()` or `crypto.randomUUID()` for unique emails and org names:

```typescript
const uniqueEmail = `e2e-${Date.now()}@test.com`
const uniqueOrgName = `E2E Org ${Date.now()}`
```

## CI/CD Integration

### GitHub Actions example

```yaml
name: E2E Tests
on: [push, pull_request]
jobs:
  e2e:
    runs-on: ubuntu-latest
    services:
      keycloak:
        image: quay.io/keycloak/keycloak:26.0
        ports: ['8080:8080']
      postgres:
        image: postgres:17
        ports: ['5432:5432']
    steps:
      - uses: actions/checkout@v4
      - uses: pnpm/action-setup@v4
      - run: pnpm install
      - run: npx playwright install --with-deps
      - run: pnpm --filter shell build && pnpm --filter admin build && pnpm --filter client build
      - run: npx playwright test
      - uses: actions/upload-artifact@v4
        if: failure()
        with:
          name: playwright-report
          path: e2e/playwright-report/
```

### Key CI considerations

- Set `retries: 2` for CI (flaky network)
- Set `workers: 1` for CI (limited resources)
- Upload artifacts on failure (screenshots, videos, traces)
- Use `webServer` config to auto-start dev servers
- Run against built apps, not dev servers, for speed

## Commands

```bash
# Run all E2E tests
npx playwright test

# Run specific test file
npx playwright test tests/auth/login.spec.ts

# Run in headed mode (see browser)
npx playwright test --headed

# Run specific browser
npx playwright test --project=chromium

# Run with UI mode (interactive)
npx playwright test --ui

# Debug a test
npx playwright test --debug

# Generate test code (codegen)
npx playwright codegen http://localhost:3000

# Show HTML report
npx playwright show-report

# Show trace viewer
npx playwright show-trace trace.zip

# Install browsers
npx playwright install
```

## Common Mistakes to Avoid

### 1. Not waiting for navigation

```typescript
// WRONG — click may not have navigated yet
await page.getByRole('button').click()
expect(page.url()).toContain('/dashboard')

// CORRECT — wait for URL change
await page.getByRole('button').click()
await expect(page).toHaveURL('/dashboard')
```

### 2. Hard-coded waits

```typescript
// WRONG — arbitrary sleep
await page.waitForTimeout(3000)

// CORRECT — wait for specific condition
await expect(page.getByRole('heading')).toBeVisible()
await page.getByTestId('dc-spinner').waitFor({ state: 'detached' })
```

### 3. Fragile selectors

```typescript
// WRONG — breaks when CSS changes
page.locator('.btn-primary.mt-4')
page.locator('div:nth-child(3) > span')

// CORRECT — semantic and resilient
page.getByRole('button', { name: 'Submit' })
page.getByLabel('Email')
page.getByTestId('org-table')
```

### 4. Not cleaning up test data

Always clean up created resources. Use `test.afterEach` or fixture teardown.

### 5. Testing too many things in one test

```typescript
// WRONG — testing login + navigation + data display + logout
test('entire user journey', async ({ page }) => { /* 50 lines */ })

// CORRECT — focused tests
test('login with valid credentials', ...)
test('dashboard shows org data', ...)
test('logout clears session', ...)
```

### 6. Not handling auth state

```typescript
// WRONG — test assumes logged-in state but auth expired
test('view dashboard', async ({ page }) => {
  await page.goto('/dashboard')  // Redirected to login!
})

// CORRECT — use storageState fixture
test.use({ storageState: 'e2e/.auth/user.json' })
test('view dashboard', async ({ page }) => {
  await page.goto('/dashboard')
  await expect(page.getByRole('heading')).toHaveText('Dashboard')
})
```

## Debugging Tips

### Use trace viewer

The richest debugging tool. Captures DOM snapshots, network requests, console logs:

```bash
npx playwright test --trace on
npx playwright show-trace test-results/trace.zip
```

### Use headed mode

See the browser while test runs:

```bash
npx playwright test --headed
```

### Use debug mode

Step through with Playwright Inspector:

```bash
npx playwright test --debug
```

### Use codegen

Record interactions to generate test code:

```bash
npx playwright codegen http://localhost:3000
```

### Add pause in test

```typescript
test('debug this', async ({ page }) => {
  await page.goto('/login')
  await page.pause()  // Opens inspector
  // Continue manually
})
```

### Check console errors

```typescript
page.on('console', msg => {
  if (msg.type() === 'error') {
    console.log('Browser error:', msg.text())
  }
})
```

## What to Test in E2E vs Unit Tests

### E2E tests (this agent)
- Full user journeys (login → navigate → action → verify)
- Cross-app integration (shell + remotes)
- Auth flows with real Keycloak
- Route protection with real guards
- Visual correctness (layout, content visible)
- Network-level behavior (API calls, headers)

### Unit tests (vue-testing agent)
- Component props, events, slots
- Store state management logic
- Individual API function contracts
- Form validation logic
- Computed property correctness
- Edge cases and error handling

**Rule of thumb**: If it requires a browser and multiple services running, it's E2E. If it can be tested in isolation with mocks, it's a unit test.

## Skill Maintenance

When the application changes:
- **New page added** — Create matching POM class in `e2e/pages/`, add tests
- **Auth flow changed** — Update `auth.setup.ts` and `LoginPage`/`SignupPage` POMs
- **New route added** — Add route protection test in `routing.spec.ts`
- **New remote app** — Add `webServer` entry in playwright config
- **API endpoint changed** — Update API helpers and network mocks
- **UI components changed** — Update locators in POM classes (prefer role/label over testid)

Update this agent file when:
- Playwright version is upgraded (check for API changes)
- Testing infrastructure changes (new fixtures, new helpers)
- New testing patterns emerge (visual regression, accessibility audits)
- CI/CD pipeline changes

Cross-reference: For unit and component tests with Vitest, see the `vue-testing` agent.
