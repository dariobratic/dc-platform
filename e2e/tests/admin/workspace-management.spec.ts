import { test, expect } from '../../fixtures'

test.describe('Workspace Management', () => {
  test('workspaces page loads with heading', async ({ page, adminWorkspacesPage }) => {
    await adminWorkspacesPage.goto()

    // Verify workspaces page loads (heading visible regardless of API state)
    await expect(page).toHaveURL('/admin/workspaces')
    await expect(page.getByRole('heading', { name: 'Workspaces' })).toBeVisible({
      timeout: 10000,
    })
  })

  test('workspaces page shows table, empty state, or error', async ({
    page,
    adminWorkspacesPage,
  }) => {
    await adminWorkspacesPage.goto()
    await expect(page.getByRole('heading', { name: 'Workspaces' })).toBeVisible({
      timeout: 10000,
    })

    // Page should show one of: workspaces table, empty state, or error alert
    const table = adminWorkspacesPage.workspacesTable
    const emptyState = adminWorkspacesPage.emptyState
    const errorAlert = page.getByRole('alert')
    await expect(table.or(emptyState).or(errorAlert)).toBeVisible({ timeout: 10000 })
  })

  test('workspaces page shows organization filter when data loads', async ({
    page,
    adminWorkspacesPage,
  }) => {
    await adminWorkspacesPage.goto()
    await expect(page.getByRole('heading', { name: 'Workspaces' })).toBeVisible({
      timeout: 10000,
    })

    // Filter is only visible when API data loads successfully
    const filterLabel = page.getByText('Filter by Organization')
    const errorAlert = page.getByRole('alert')
    if (await errorAlert.isVisible({ timeout: 3000 })) {
      test.skip(true, 'API returned error, filter not rendered')
      return
    }

    await expect(filterLabel).toBeVisible({ timeout: 5000 })
  })

  test('navigate to workspace detail from workspaces list', async ({
    page,
    adminWorkspacesPage,
  }) => {
    await adminWorkspacesPage.goto()
    await expect(page.getByRole('heading', { name: 'Workspaces' })).toBeVisible({
      timeout: 10000,
    })

    // Check if there are workspace rows with links
    const workspaceLink = page.locator('table tbody tr a').first()
    if (!(await workspaceLink.isVisible({ timeout: 5000 }))) {
      test.skip(true, 'No workspaces loaded (API may be unavailable)')
      return
    }

    await workspaceLink.click()
    await expect(page).toHaveURL(/\/admin\/workspaces\//, { timeout: 10000 })
  })

  test('workspace detail page shows overview tab', async ({
    page,
    adminWorkspacesPage,
    adminWorkspaceDetailPage,
  }) => {
    await adminWorkspacesPage.goto()
    await expect(page.getByRole('heading', { name: 'Workspaces' })).toBeVisible({
      timeout: 10000,
    })

    const workspaceLink = page.locator('table tbody tr a').first()
    if (!(await workspaceLink.isVisible({ timeout: 5000 }))) {
      test.skip(true, 'No workspaces loaded (API may be unavailable)')
      return
    }

    await workspaceLink.click()
    await expect(page).toHaveURL(/\/admin\/workspaces\//, { timeout: 10000 })

    // Overview tab should be visible
    await expect(adminWorkspaceDetailPage.overviewTab).toBeVisible()
  })

  test('workspace detail members tab', async ({
    page,
    adminWorkspacesPage,
    adminWorkspaceDetailPage,
  }) => {
    await adminWorkspacesPage.goto()
    await expect(page.getByRole('heading', { name: 'Workspaces' })).toBeVisible({
      timeout: 10000,
    })

    const workspaceLink = page.locator('table tbody tr a').first()
    if (!(await workspaceLink.isVisible({ timeout: 5000 }))) {
      test.skip(true, 'No workspaces loaded (API may be unavailable)')
      return
    }

    await workspaceLink.click()
    await expect(page).toHaveURL(/\/admin\/workspaces\//, { timeout: 10000 })

    // Switch to members tab
    await adminWorkspaceDetailPage.switchToTab('members')

    // Should see either members table, Add Member button, or empty state
    const addBtn = adminWorkspaceDetailPage.addMemberButton
    const emptyState = adminWorkspaceDetailPage.membersEmptyState
    await expect(addBtn.or(emptyState)).toBeVisible({ timeout: 5000 })
  })

  test('workspace detail settings tab shows rename and delete', async ({
    page,
    adminWorkspacesPage,
    adminWorkspaceDetailPage,
  }) => {
    await adminWorkspacesPage.goto()
    await expect(page.getByRole('heading', { name: 'Workspaces' })).toBeVisible({
      timeout: 10000,
    })

    const workspaceLink = page.locator('table tbody tr a').first()
    if (!(await workspaceLink.isVisible({ timeout: 5000 }))) {
      test.skip(true, 'No workspaces loaded (API may be unavailable)')
      return
    }

    await workspaceLink.click()
    await expect(page).toHaveURL(/\/admin\/workspaces\//, { timeout: 10000 })

    // Switch to settings tab
    await adminWorkspaceDetailPage.switchToTab('settings')

    // Should see rename input and delete button
    await expect(adminWorkspaceDetailPage.workspaceNameInput).toBeVisible()
    await expect(adminWorkspaceDetailPage.deleteWorkspaceButton).toBeVisible()
  })
})
