import { test as setup, expect } from '@playwright/test'
import { LoginPage } from '../pages/LoginPage'

const TEST_EMAIL = process.env.E2E_USER_EMAIL ?? 'test@example.com'
const TEST_PASSWORD = process.env.E2E_USER_PASSWORD ?? 'TestPassword123'

const authFile = 'e2e/.auth/user.json'

setup('authenticate', async ({ page }) => {
  const loginPage = new LoginPage(page)
  await loginPage.goto()
  await loginPage.login(TEST_EMAIL, TEST_PASSWORD)

  // Wait for redirect to org picker or dashboard
  await expect(page).toHaveURL(/\/(select-organization|dashboard)/, {
    timeout: 15000,
  })

  // If on org picker, select first org to complete auth
  if (page.url().includes('select-organization')) {
    // Wait for orgs to load
    await page.waitForResponse('**/api/v1/users/me/organizations')

    // Select first available organization
    await page.locator('button.w-full.border').first().click()

    await expect(page).toHaveURL(/\/dashboard/, { timeout: 10000 })
  }

  // Save auth + storage state for reuse
  await page.context().storageState({ path: authFile })
})
