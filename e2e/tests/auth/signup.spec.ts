import { test, expect } from '../../fixtures'

test.describe('Signup flow', () => {
  // Signup tests must run without pre-existing auth state
  test.use({ storageState: { cookies: [], origins: [] } })

  test('new user creates account with organization and lands on dashboard', async ({
    signupPage,
    page,
  }) => {
    const uniqueId = Date.now()
    const email = `e2e-signup-${uniqueId}@test.com`
    const orgName = `E2E Test Org ${uniqueId}`

    await signupPage.goto()

    // Verify signup page loaded
    await expect(signupPage.heading).toHaveText('Create your account')

    // Fill and submit the form
    await signupPage.signup({
      fullName: 'E2E Test User',
      email,
      password: 'SecurePassword123',
      organizationName: orgName,
    })

    // Should land on dashboard after signup (org auto-selected)
    await expect(page).toHaveURL('/dashboard', { timeout: 15000 })

    // Dashboard should show welcome message
    await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible()
  })

  test('shows slug preview when typing organization name', async ({
    signupPage,
  }) => {
    await signupPage.goto()

    await signupPage.organizationNameInput.fill('My Test Company')

    await expect(signupPage.slugPreview).toContainText('my-test-company')
  })

  test('shows validation error for mismatched passwords', async ({
    signupPage,
    page,
  }) => {
    await signupPage.goto()

    await signupPage.fullNameInput.fill('Test User')
    await signupPage.emailInput.fill('test@example.com')
    await signupPage.passwordInput.fill('SecurePassword123')
    await signupPage.confirmPasswordInput.fill('DifferentPassword')
    await signupPage.organizationNameInput.fill('Test Org')
    await signupPage.submitButton.click()

    // Should stay on signup page (client-side validation prevents submission)
    await expect(page).toHaveURL('/signup')
  })

  test('navigate to signin from signup page', async ({ signupPage, page }) => {
    await signupPage.goto()

    await signupPage.signinLink.click()

    await expect(page).toHaveURL('/login')
  })
})
