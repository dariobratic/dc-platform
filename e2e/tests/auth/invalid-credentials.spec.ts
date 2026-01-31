import { test, expect } from '../../fixtures'

test.describe('Invalid credentials', () => {
  // These tests must run without pre-existing auth state
  test.use({ storageState: { cookies: [], origins: [] } })

  test('wrong password shows error message', async ({ loginPage, page }) => {
    await loginPage.goto()

    await loginPage.login('nonexistent-wrong-pw@example.com', 'WrongPassword123')

    // Error alert should appear with the invalid credentials message
    await expect(loginPage.errorAlert).toBeVisible({ timeout: 10000 })
    await expect(loginPage.errorAlert).toContainText(
      'Invalid email or password',
    )

    // Should remain on login page
    await expect(page).toHaveURL('/login')
  })

  test('nonexistent email shows error message', async ({
    loginPage,
    page,
  }) => {
    await loginPage.goto()

    await loginPage.login('nonexistent@example.com', 'SomePassword123')

    await expect(loginPage.errorAlert).toBeVisible({ timeout: 10000 })
    await expect(loginPage.errorAlert).toContainText(
      'Invalid email or password',
    )

    await expect(page).toHaveURL('/login')
  })

  test('error alert can be dismissed', async ({ loginPage }) => {
    await loginPage.goto()

    await loginPage.login('nonexistent-dismiss@example.com', 'WrongPassword123')

    await expect(loginPage.errorAlert).toBeVisible({ timeout: 10000 })

    // Dismiss the alert (DcAlert has a dismiss button when dismissible)
    await loginPage.page.locator('[role="alert"] button').click()

    await expect(loginPage.errorAlert).not.toBeVisible()
  })
})
