import { test, expect } from '../../fixtures'

test.describe('Client Portal Navigation', () => {
  test('navigate to client dashboard', async ({ page, clientDashboardPage }) => {
    await clientDashboardPage.goto()

    // Verify client dashboard loads
    await expect(page).toHaveURL('/app')
    await expect(clientDashboardPage.heading).toBeVisible()
  })

  test('sidebar navigation to client portal', async ({ page }) => {
    await page.goto('/dashboard')

    // Click Client Portal in sidebar
    await page.getByRole('link', { name: 'Client Portal' }).click()

    // Verify client dashboard loads
    await expect(page).toHaveURL(/\/app/)
    await expect(page.getByRole('heading')).toBeVisible()
  })

  test('client dashboard shows welcome message', async ({ clientDashboardPage }) => {
    await clientDashboardPage.goto()

    // Verify welcome message is visible
    await expect(clientDashboardPage.welcomeMessage).toBeVisible()
  })
})
