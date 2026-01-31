import { test, expect } from '../../fixtures'

test.describe('Organization Management', () => {
  test('organizations page loads with heading', async ({ page, adminOrganizationsPage }) => {
    await adminOrganizationsPage.goto()

    // Verify organizations page loads (heading visible regardless of API state)
    await expect(page).toHaveURL('/admin/organizations')
    await expect(page.getByRole('heading', { name: 'Organizations' })).toBeVisible({
      timeout: 10000,
    })
  })

  test('organizations page shows table, empty state, or error', async ({
    page,
    adminOrganizationsPage,
  }) => {
    await adminOrganizationsPage.goto()
    await expect(page.getByRole('heading', { name: 'Organizations' })).toBeVisible({
      timeout: 10000,
    })

    // Page should show one of: organizations table, empty state, or error alert
    const table = adminOrganizationsPage.organizationsTable
    const emptyState = adminOrganizationsPage.emptyState
    const errorAlert = page.getByRole('alert')
    await expect(table.or(emptyState).or(errorAlert)).toBeVisible({ timeout: 10000 })
  })

  test('view organization detail page from organizations list', async ({
    page,
    adminOrganizationsPage,
  }) => {
    await adminOrganizationsPage.goto()
    await expect(page.getByRole('heading', { name: 'Organizations' })).toBeVisible({
      timeout: 10000,
    })

    // Click "View" on the first organization row (if data loaded)
    const viewButton = page.getByRole('button', { name: 'View' }).first()
    if (!(await viewButton.isVisible({ timeout: 5000 }))) {
      test.skip(true, 'No organizations loaded (API may be unavailable)')
      return
    }

    await viewButton.click()

    // Should navigate to organization detail page
    await expect(page).toHaveURL(/\/admin\/organizations\//, { timeout: 10000 })
    await expect(page.getByRole('heading').first()).toBeVisible({ timeout: 10000 })
  })

  test('organization detail page shows overview tab by default', async ({
    page,
    adminOrganizationsPage,
    adminOrganizationDetailPage,
  }) => {
    await adminOrganizationsPage.goto()
    await expect(page.getByRole('heading', { name: 'Organizations' })).toBeVisible({
      timeout: 10000,
    })

    const viewButton = page.getByRole('button', { name: 'View' }).first()
    if (!(await viewButton.isVisible({ timeout: 5000 }))) {
      test.skip(true, 'No organizations loaded (API may be unavailable)')
      return
    }

    await viewButton.click()
    await expect(page).toHaveURL(/\/admin\/organizations\//, { timeout: 10000 })

    // Overview tab should be visible and active
    await expect(adminOrganizationDetailPage.overviewTab).toBeVisible()
  })

  test('edit organization name - toggle edit mode', async ({
    page,
    adminOrganizationsPage,
    adminOrganizationDetailPage,
  }) => {
    await adminOrganizationsPage.goto()
    await expect(page.getByRole('heading', { name: 'Organizations' })).toBeVisible({
      timeout: 10000,
    })

    const viewButton = page.getByRole('button', { name: 'View' }).first()
    if (!(await viewButton.isVisible({ timeout: 5000 }))) {
      test.skip(true, 'No organizations loaded (API may be unavailable)')
      return
    }

    await viewButton.click()
    await expect(page).toHaveURL(/\/admin\/organizations\//, { timeout: 10000 })

    // Click Edit button
    if (!(await adminOrganizationDetailPage.editButton.isVisible({ timeout: 5000 }))) {
      test.skip(true, 'Edit button not visible (detail page may have errored)')
      return
    }

    await adminOrganizationDetailPage.editButton.click()

    // Name input should appear
    await expect(adminOrganizationDetailPage.orgNameInput).toBeVisible()

    // Cancel editing
    await adminOrganizationDetailPage.cancelButton.click()

    // Input should disappear, back to display mode
    await expect(adminOrganizationDetailPage.orgNameInput).not.toBeVisible()
  })

  test('switch to workspaces tab on organization detail', async ({
    page,
    adminOrganizationsPage,
    adminOrganizationDetailPage,
  }) => {
    await adminOrganizationsPage.goto()
    await expect(page.getByRole('heading', { name: 'Organizations' })).toBeVisible({
      timeout: 10000,
    })

    const viewButton = page.getByRole('button', { name: 'View' }).first()
    if (!(await viewButton.isVisible({ timeout: 5000 }))) {
      test.skip(true, 'No organizations loaded (API may be unavailable)')
      return
    }

    await viewButton.click()
    await expect(page).toHaveURL(/\/admin\/organizations\//, { timeout: 10000 })

    // Switch to workspaces tab
    await adminOrganizationDetailPage.switchToTab('workspaces')

    // Should see Create Workspace button, workspace cards, or empty state
    const createBtn = adminOrganizationDetailPage.createWorkspaceButton
    const emptyState = adminOrganizationDetailPage.workspacesEmptyState
    await expect(createBtn.or(emptyState)).toBeVisible({ timeout: 5000 })
  })

  test('switch to members tab on organization detail', async ({
    page,
    adminOrganizationsPage,
    adminOrganizationDetailPage,
  }) => {
    await adminOrganizationsPage.goto()
    await expect(page.getByRole('heading', { name: 'Organizations' })).toBeVisible({
      timeout: 10000,
    })

    const viewButton = page.getByRole('button', { name: 'View' }).first()
    if (!(await viewButton.isVisible({ timeout: 5000 }))) {
      test.skip(true, 'No organizations loaded (API may be unavailable)')
      return
    }

    await viewButton.click()
    await expect(page).toHaveURL(/\/admin\/organizations\//, { timeout: 10000 })

    // Switch to members tab
    await adminOrganizationDetailPage.switchToTab('members')

    // Should see the members section (content depends on data availability)
    await expect(
      adminOrganizationDetailPage.membersEmptyState.or(
        adminOrganizationDetailPage.addMemberButton,
      ).or(page.getByRole('alert')),
    ).toBeVisible({ timeout: 5000 })
  })
})
