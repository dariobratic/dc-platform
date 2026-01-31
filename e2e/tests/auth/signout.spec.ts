import { test, expect } from '../../fixtures'

const TEST_EMAIL = process.env.E2E_USER_EMAIL ?? 'test@example.com'
const TEST_PASSWORD = process.env.E2E_USER_PASSWORD ?? 'TestPassword123'

test.describe('Signout flow', () => {
  // Signout tests must log in first because Playwright storageState
  // only persists cookies + localStorage, but auth tokens live in sessionStorage.
  test.use({ storageState: { cookies: [], origins: [] } })

  test('logged in user signs out and is redirected to login', async ({
    loginPage,
    page,
  }) => {
    // Log in first
    await loginPage.goto()
    await loginPage.login(TEST_EMAIL, TEST_PASSWORD)

    // Wait for redirect through org picker to dashboard
    // The org picker auto-selects when there's only one organization
    await expect(page).toHaveURL(/\/dashboard/, { timeout: 15000 })
    await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible()

    // Open user menu and click sign out
    await page.locator('.relative button').first().click()
    await page.getByRole('button', { name: 'Sign out' }).click()

    // Should be redirected to login page
    await expect(page).toHaveURL('/login', { timeout: 10000 })
  })

  test('after signout, accessing dashboard redirects to login', async ({
    loginPage,
    page,
  }) => {
    // Log in first
    await loginPage.goto()
    await loginPage.login(TEST_EMAIL, TEST_PASSWORD)

    // Wait for redirect through org picker to dashboard
    // The org picker auto-selects when there's only one organization
    await expect(page).toHaveURL(/\/dashboard/, { timeout: 15000 })

    // Sign out
    await page.locator('.relative button').first().click()
    await page.getByRole('button', { name: 'Sign out' }).click()
    await expect(page).toHaveURL('/login', { timeout: 10000 })

    // Try to access dashboard again - should redirect to login
    await page.goto('/dashboard')
    await expect(page).toHaveURL('/login', { timeout: 10000 })
  })
})
