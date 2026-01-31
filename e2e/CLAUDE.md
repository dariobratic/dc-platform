# E2E Tests

Playwright end-to-end tests for DC Platform. Tests the full user flow across shell, admin, and client apps running in Docker containers.

## Scope

These tests verify:
- Authentication flows (signup, signin, signout)
- Protected route guards
- Invalid credential handling
- Cross-app navigation (shell → admin, shell → client)

These tests do NOT cover:
- Unit-level component behavior (→ Vitest in each app)
- Backend API logic (→ .NET integration tests)
- Visual regression (future)

## Prerequisites

The full platform must be running before executing tests:
```bash
# From project root
pnpm docker:up
# Wait for all containers to be healthy, then:
pnpm test:e2e
```

## Project Structure

```
e2e/
├── pages/                        # Page Object Model
│   ├── BasePage.ts               # Abstract base (navigation, heading, waitForPageLoad)
│   ├── LoginPage.ts              # Login form interactions
│   ├── SignupPage.ts             # Signup form interactions
│   ├── OrganizationPickerPage.ts # Organization selection
│   └── DashboardPage.ts          # Dashboard assertions
├── fixtures/
│   └── index.ts                  # Test fixtures (provides page objects to tests)
├── tests/
│   ├── auth.setup.ts             # Setup project: creates user, stores auth state
│   └── auth/
│       ├── signup.spec.ts        # Signup flow tests
│       ├── signin.spec.ts        # Signin flow tests
│       ├── signout.spec.ts       # Signout flow tests
│       ├── invalid-credentials.spec.ts  # Error handling tests
│       └── protected-routes.spec.ts     # Route guard tests
├── .auth/
│   └── user.json                 # Stored auth state (gitignored)
├── playwright.config.ts          # Playwright configuration
├── tsconfig.json
└── package.json
```

## Configuration

### playwright.config.ts

- **Base URL**: `http://localhost:3000` (shell app in Docker)
- **Browser**: Chromium only (desktop)
- **Parallel**: Enabled (`fullyParallel: true`)
- **Screenshots**: Only on failure
- **Video**: On first retry
- **Reporter**: HTML + list

### Test Projects

Two Playwright projects configured:

1. **setup** — Runs `auth.setup.ts` first. Creates a test user via signup, stores authenticated state to `e2e/.auth/user.json`.
2. **chromium** — Runs all spec files. Uses stored auth state from setup. Depends on setup completing first.

## Page Object Model

All page objects extend `BasePage`:

```typescript
abstract class BasePage {
  constructor(protected page: Page) {}
  abstract waitForPageLoad(): Promise<void>
  async navigateTo(path: string): Promise<void>
  get heading(): Locator  // h1 element
}
```

### Page Objects

| Page Object | Selectors | Actions |
|-------------|-----------|---------|
| `LoginPage` | emailInput, passwordInput, submitButton, errorAlert, signupLink | `login(email, password)` |
| `SignupPage` | fullNameInput, emailInput, passwordInput, confirmPasswordInput, organizationNameInput, slugPreview, submitButton, errorAlert, signinLink | `signup(name, email, password, orgName)` |
| `OrganizationPickerPage` | Org selection elements | Select organization |
| `DashboardPage` | Dashboard content elements | Verify dashboard loaded |

### Fixtures

Test fixtures are defined in `fixtures/index.ts` and extend Playwright's base test:

```typescript
import { test as base } from '@playwright/test'

export const test = base.extend<{
  loginPage: LoginPage
  signupPage: SignupPage
  orgPickerPage: OrganizationPickerPage
  dashboardPage: DashboardPage
}>({
  loginPage: async ({ page }, use) => { await use(new LoginPage(page)) },
  signupPage: async ({ page }, use) => { await use(new SignupPage(page)) },
  // ...
})
```

Tests import `test` and `expect` from fixtures instead of `@playwright/test`.

## Auth State Pattern

The setup project (`auth.setup.ts`) creates a fresh user via the signup flow, then saves the browser's authenticated state (cookies, sessionStorage) to `e2e/.auth/user.json`. All subsequent tests in the `chromium` project load this state, avoiding re-login for every test.

```typescript
// In auth.setup.ts
await page.context().storageState({ path: 'e2e/.auth/user.json' })

// In playwright.config.ts
{ name: 'chromium', use: { storageState: 'e2e/.auth/user.json' }, dependencies: ['setup'] }
```

## Adding New Tests

### New test in existing area

1. Add spec file in `e2e/tests/auth/` (or create new subdirectory for new area)
2. Import `test` and `expect` from `../fixtures` (not from `@playwright/test`)
3. Use page object fixtures:

```typescript
import { test, expect } from '../../fixtures'

test('should do something', async ({ loginPage, dashboardPage }) => {
  await loginPage.navigateTo('/login')
  await loginPage.login('user@example.com', 'password')
  await dashboardPage.waitForPageLoad()
  await expect(dashboardPage.heading).toBeVisible()
})
```

### New page object

1. Create `e2e/pages/NewPage.ts` extending `BasePage`
2. Define locators as getters (use `data-testid` attributes when available)
3. Add action methods for common interactions
4. Register in `e2e/fixtures/index.ts`

### New test area (e.g., admin tests)

1. Create directory: `e2e/tests/admin/`
2. Create spec files following existing patterns
3. If area needs different auth state, create a new setup project in `playwright.config.ts`

## Commands

```bash
# From project root (pnpm scripts)
pnpm test:e2e          # Run all tests
pnpm test:e2e:ui       # Playwright UI mode (interactive)
pnpm test:e2e:report   # View HTML test report

# From e2e/ directory (direct playwright)
pnpm test              # playwright test
pnpm test:headed       # Run with visible browser
pnpm test:debug        # Debug mode (step through)
pnpm install:browsers  # Install Chromium browser
```

## Coding Rules

1. **Page Object Model** — All selectors live in page objects, never inline in tests
2. **Fixtures** — Import `test`/`expect` from `fixtures/index.ts`, not `@playwright/test`
3. **Wait for navigation** — Use `waitForPageLoad()` after navigation actions
4. **Test isolation** — Each test should work independently (no ordering dependencies)
5. **Descriptive names** — Test names should describe the user behavior being verified
6. **No hardcoded waits** — Use Playwright's auto-waiting, not `page.waitForTimeout()`
7. **data-testid preferred** — When adding selectors, prefer `data-testid` over CSS classes

## Troubleshooting

### Tests fail with connection refused
Platform containers aren't running. Run `pnpm docker:up` and wait for health checks.

### Auth setup fails
Keycloak may not be ready. Check `docker compose logs keycloak` for startup status. The realm import can take time on first boot.

### Tests pass locally but fail in CI
Check that `baseURL` in playwright.config.ts matches the CI environment's shell URL. Screenshots from failed tests are saved in `e2e/test-results/`.

## Related Files

- **Shell app**: `apps/shell/` — hosts the frontend, auth pages
- **Keycloak config**: `infrastructure/keycloak/realm-export.json` — realm setup
- **Docker Compose**: `infrastructure/docker-compose.yml` — platform containers
- **Root scripts**: `package.json` — `test:e2e`, `test:e2e:ui`, `test:e2e:report`
