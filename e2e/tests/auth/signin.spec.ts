import { test, expect } from '../../fixtures'

const TEST_EMAIL = process.env.E2E_USER_EMAIL ?? 'test@example.com'
const TEST_PASSWORD = process.env.E2E_USER_PASSWORD ?? 'TestPassword123'

test.describe('Signin flow', () => {
  // Signin tests must run without pre-existing auth state
  test.use({ storageState: { cookies: [], origins: [] } })

  test('existing user logs in and reaches org picker or dashboard', async ({
    loginPage,
    page,
  }) => {
    await loginPage.goto()

    // Verify login page loaded
    await expect(loginPage.heading).toHaveText('Sign in to DC Platform')

    // Submit credentials
    await loginPage.login(TEST_EMAIL, TEST_PASSWORD)

    // Should redirect to org picker or dashboard
    await expect(page).toHaveURL(/\/(select-organization|dashboard)/, {
      timeout: 15000,
    })
  })

  test('user selects organization and sees dashboard', async ({
    loginPage,
    orgPickerPage,
    page,
  }) => {
    await loginPage.goto()
    await loginPage.login(TEST_EMAIL, TEST_PASSWORD)

    // Wait for post-login redirect
    await expect(page).toHaveURL(/\/(select-organization|dashboard)/, {
      timeout: 15000,
    })

    // If on org picker, select an org
    if (page.url().includes('select-organization')) {
      await expect(orgPickerPage.heading).toHaveText('Select Organization')

      // Wait for organizations to load (spinner disappears)
      await orgPickerPage.loadingSpinner.waitFor({ state: 'detached', timeout: 10000 })

      // Select first organization
      await orgPickerPage.selectFirstOrganization()
    }

    // Should now be on dashboard
    await expect(page).toHaveURL('/dashboard', { timeout: 10000 })
    await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible()
  })

  test('navigate to signup from login page', async ({ loginPage, page }) => {
    await loginPage.goto()

    await loginPage.signupLink.click()

    await expect(page).toHaveURL('/signup')
  })
})
