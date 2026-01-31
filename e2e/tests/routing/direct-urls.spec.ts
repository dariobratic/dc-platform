import { test, expect } from '../../fixtures'

test.describe('Direct URL Navigation', () => {
  test('navigate directly to /admin', async ({ page }) => {
    await page.goto('/admin')

    // Should load admin dashboard without white screen
    await expect(page).toHaveURL('/admin')
    await expect(page.getByRole('heading', { name: 'Admin Dashboard' })).toBeVisible({
      timeout: 10000,
    })
  })

  test('navigate directly to /admin/organizations', async ({ page }) => {
    await page.goto('/admin/organizations')

    // Should load organizations page
    await expect(page).toHaveURL('/admin/organizations')
    await expect(page.getByRole('heading', { name: 'Organizations' })).toBeVisible({
      timeout: 10000,
    })
  })

  test('navigate directly to /admin/workspaces', async ({ page }) => {
    await page.goto('/admin/workspaces')

    // Should load workspaces page
    await expect(page).toHaveURL('/admin/workspaces')
    await expect(page.getByRole('heading', { name: 'Workspaces' })).toBeVisible({
      timeout: 10000,
    })
  })

  test('navigate directly to /admin/roles', async ({ page }) => {
    await page.goto('/admin/roles')

    // Should load roles page
    await expect(page).toHaveURL('/admin/roles')
    await expect(page.getByRole('heading', { name: 'Roles' })).toBeVisible({ timeout: 10000 })
  })

  test('navigate directly to /admin/audit', async ({ page }) => {
    await page.goto('/admin/audit')

    // Should load audit log page
    await expect(page).toHaveURL('/admin/audit')
    await expect(page.getByRole('heading', { name: 'Audit Log' })).toBeVisible({
      timeout: 10000,
    })
  })

  test('navigate directly to /app', async ({ page }) => {
    await page.goto('/app')

    // Should load client dashboard
    await expect(page).toHaveURL('/app')
    await expect(page.getByRole('heading', { name: /Welcome back/ })).toBeVisible({
      timeout: 10000,
    })
  })

  test('refresh on admin route preserves page', async ({ page }) => {
    // Navigate to admin organizations
    await page.goto('/admin/organizations')
    await expect(page.getByRole('heading', { name: 'Organizations' })).toBeVisible({
      timeout: 10000,
    })

    // Reload page
    await page.reload()

    // Should still be on organizations page
    await expect(page).toHaveURL('/admin/organizations')
    await expect(page.getByRole('heading', { name: 'Organizations' })).toBeVisible({
      timeout: 10000,
    })
  })

  test('refresh on client route preserves page', async ({ page }) => {
    // Navigate to client dashboard
    await page.goto('/app')
    await expect(page.getByRole('heading', { name: /Welcome back/ })).toBeVisible({
      timeout: 10000,
    })

    // Reload page
    await page.reload()

    // After reload, auth is re-initialized from sessionStorage.
    // The page may briefly redirect through org picker before returning.
    await expect(page.getByRole('heading', { name: /Welcome back|Dashboard/ })).toBeVisible({
      timeout: 15000,
    })
    // Verify we're on a valid authenticated route (either /app or /dashboard)
    await expect(page).toHaveURL(/\/(app|dashboard)/, { timeout: 5000 })
  })

  test('navigate directly to /admin/organizations/:id', async ({ page }) => {
    // First get a real organization ID from the organizations list
    await page.goto('/admin/organizations')
    await expect(page.getByRole('heading', { name: 'Organizations' })).toBeVisible({
      timeout: 10000,
    })

    const viewButton = page.getByRole('button', { name: 'View' }).first()
    if (!(await viewButton.isVisible({ timeout: 5000 }))) {
      test.skip(true, 'No organizations available to test direct URL')
      return
    }

    await viewButton.click()
    await expect(page).toHaveURL(/\/admin\/organizations\//, { timeout: 10000 })

    // Capture the current URL with the org ID
    const orgDetailUrl = new URL(page.url()).pathname

    // Navigate away
    await page.goto('/admin')
    await expect(page.getByRole('heading', { name: 'Admin Dashboard' })).toBeVisible({
      timeout: 10000,
    })

    // Navigate directly back to the org detail URL
    await page.goto(orgDetailUrl)

    // Should load the org detail page without white screen
    await expect(page).toHaveURL(orgDetailUrl)
    await expect(page.getByRole('heading').first()).toBeVisible({ timeout: 10000 })
  })

  test('refresh on admin detail route preserves page', async ({ page }) => {
    // Navigate to organizations list and get to a detail page
    await page.goto('/admin/organizations')
    await expect(page.getByRole('heading', { name: 'Organizations' })).toBeVisible({
      timeout: 10000,
    })

    const viewButton = page.getByRole('button', { name: 'View' }).first()
    if (!(await viewButton.isVisible({ timeout: 5000 }))) {
      test.skip(true, 'No organizations available to test refresh')
      return
    }

    await viewButton.click()
    await expect(page).toHaveURL(/\/admin\/organizations\//, { timeout: 10000 })
    await expect(page.getByRole('heading').first()).toBeVisible({ timeout: 10000 })

    const urlBeforeRefresh = page.url()

    // Reload the page
    await page.reload()

    // Should still be on the same page
    await expect(page).toHaveURL(urlBeforeRefresh)
    await expect(page.getByRole('heading').first()).toBeVisible({ timeout: 10000 })
  })

  test('navigate directly to /admin/users', async ({ page }) => {
    await page.goto('/admin/users')

    // Should load users page
    await expect(page).toHaveURL('/admin/users')
    await expect(page.getByRole('heading', { name: 'Users' })).toBeVisible({ timeout: 10000 })
  })
})
