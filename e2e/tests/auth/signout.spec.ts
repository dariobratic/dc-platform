import { test, expect } from '../../fixtures'

test.describe('Signout flow', () => {
  // These tests use the pre-authenticated state from auth.setup.ts

  test('logged in user signs out and is redirected to login', async ({
    dashboardPage,
    page,
  }) => {
    await dashboardPage.goto()

    // Verify we're on dashboard (authenticated)
    await expect(page).toHaveURL('/dashboard')
    await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible()

    // Open user menu and click sign out
    await dashboardPage.signOut()

    // Should be redirected to login page
    await expect(page).toHaveURL('/login', { timeout: 10000 })
  })

  test('after signout, accessing dashboard redirects to login', async ({
    dashboardPage,
    page,
  }) => {
    await dashboardPage.goto()
    await expect(page).toHaveURL('/dashboard')

    // Sign out
    await dashboardPage.signOut()
    await expect(page).toHaveURL('/login', { timeout: 10000 })

    // Try to access dashboard again — should redirect to login
    await page.goto('/dashboard')
    await expect(page).toHaveURL('/login', { timeout: 10000 })
  })
})
