import { test, expect } from '../../fixtures'

test.describe('Protected routes', () => {
  // These tests must run without auth — clear storage state and disable sessionStorage injection
  test.use({ storageState: { cookies: [], origins: [] }, injectAuth: false })

  test('unauthenticated user accessing /dashboard is redirected to login', async ({
    page,
  }) => {
    await page.goto('/dashboard')

    await expect(page).toHaveURL('/login', { timeout: 10000 })
  })

  test('unauthenticated user accessing /admin is redirected to login', async ({
    page,
  }) => {
    await page.goto('/admin')

    await expect(page).toHaveURL('/login', { timeout: 10000 })
  })

  test('unauthenticated user accessing /app is redirected to login', async ({
    page,
  }) => {
    await page.goto('/app')

    await expect(page).toHaveURL('/login', { timeout: 10000 })
  })

  test('unauthenticated user accessing /select-organization is redirected to login', async ({
    page,
  }) => {
    await page.goto('/select-organization')

    await expect(page).toHaveURL('/login', { timeout: 10000 })
  })

  test('/login is accessible without authentication', async ({ page }) => {
    await page.goto('/login')

    await expect(page).toHaveURL('/login')
    await expect(
      page.getByRole('heading', { name: 'Sign in to DC Platform' }),
    ).toBeVisible()
  })

  test('/signup is accessible without authentication', async ({ page }) => {
    await page.goto('/signup')

    await expect(page).toHaveURL('/signup')
    await expect(
      page.getByRole('heading', { name: 'Create your account' }),
    ).toBeVisible()
  })
})
