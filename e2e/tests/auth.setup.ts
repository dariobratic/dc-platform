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

  // If on org picker, wait for it to resolve (auto-select or manual)
  if (page.url().includes('select-organization')) {
    // The org picker auto-selects when there's only one org,
    // so just wait for dashboard rather than clicking a button
    await expect(page).toHaveURL(/\/dashboard/, { timeout: 15000 })
  }

  // Save auth + storage state for reuse
  await page.context().storageState({ path: authFile })
})
