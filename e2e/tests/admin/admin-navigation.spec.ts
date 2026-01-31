import { test, expect } from '../../fixtures'

test.describe('Admin Portal Navigation', () => {
  test('navigate to admin dashboard', async ({ page, adminDashboardPage }) => {
    await adminDashboardPage.goto()

    // Verify admin dashboard loads
    await expect(page).toHaveURL('/admin')
    await expect(page.getByRole('heading', { name: 'Admin Dashboard' })).toBeVisible({
      timeout: 10000,
    })
  })

  test('navigate to organizations page', async ({ page, adminOrganizationsPage }) => {
    await adminOrganizationsPage.goto()

    // Verify organizations page loads
    await expect(page).toHaveURL('/admin/organizations')
    await expect(page.getByRole('heading', { name: 'Organizations' })).toBeVisible({
      timeout: 10000,
    })
  })

  test('navigate to workspaces page', async ({ page, adminWorkspacesPage }) => {
    await adminWorkspacesPage.goto()

    // Verify workspaces page loads
    await expect(page).toHaveURL('/admin/workspaces')
    await expect(page.getByRole('heading', { name: 'Workspaces' })).toBeVisible({
      timeout: 10000,
    })
  })

  test('navigate to roles page', async ({ page, adminRolesPage }) => {
    await adminRolesPage.goto()

    // Verify roles page loads
    await expect(page).toHaveURL('/admin/roles')
    await expect(page.getByRole('heading', { name: /Roles/ })).toBeVisible({ timeout: 10000 })
  })

  test('navigate to audit log page', async ({ page, adminAuditLogPage }) => {
    await adminAuditLogPage.goto()

    // Verify audit log page loads
    await expect(page).toHaveURL('/admin/audit')
    await expect(page.getByRole('heading', { name: 'Audit Log' })).toBeVisible({
      timeout: 10000,
    })
  })

  test('sidebar navigation to admin portal', async ({ page }) => {
    await page.goto('/dashboard')
    await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible({ timeout: 10000 })

    // Click Admin in sidebar
    await page.getByRole('link', { name: 'Admin' }).click()

    // Verify admin dashboard loads
    await expect(page).toHaveURL(/\/admin/)
    await expect(page.getByRole('heading', { name: 'Admin Dashboard' })).toBeVisible({
      timeout: 10000,
    })
  })
})
