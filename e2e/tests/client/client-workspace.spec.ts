import { test, expect } from '../../fixtures'

test.describe('Client Workspace', () => {
  test('view workspace cards on client dashboard', async ({ page, clientDashboardPage }) => {
    await clientDashboardPage.goto()
    await expect(page).toHaveURL('/app')

    // Dashboard should load with heading
    await expect(clientDashboardPage.heading).toBeVisible({ timeout: 10000 })

    // Should show either workspace cards or empty state
    const cards = clientDashboardPage.workspaceCards
    const emptyState = clientDashboardPage.emptyState
    await expect(cards.first().or(emptyState)).toBeVisible({ timeout: 10000 })
  })

  test('navigate to workspace detail from dashboard', async ({ page, clientDashboardPage }) => {
    await clientDashboardPage.goto()
    await expect(clientDashboardPage.heading).toBeVisible({ timeout: 10000 })

    // Check if workspace cards exist (they are buttons, not links)
    const cards = clientDashboardPage.workspaceCards
    const cardCount = await cards.count()

    if (cardCount > 0) {
      // Click the first workspace card button
      await cards.first().click()
      await expect(page).toHaveURL(/\/app\/workspace\//, { timeout: 10000 })
    } else {
      // Verify empty state
      await expect(clientDashboardPage.emptyState).toBeVisible()
    }
  })

  test('workspace page shows overview tab by default', async ({
    page,
    clientWorkspacePage,
    clientDashboardPage,
  }) => {
    // Navigate to client dashboard first to find a workspace
    await clientDashboardPage.goto()
    await expect(clientDashboardPage.heading).toBeVisible({ timeout: 10000 })

    const cards = clientDashboardPage.workspaceCards
    const cardCount = await cards.count()

    if (cardCount === 0) {
      test.skip(true, 'No workspaces available to test')
      return
    }

    // Click into first workspace (cards are buttons)
    await cards.first().click()
    await expect(page).toHaveURL(/\/app\/workspace\//, { timeout: 10000 })

    // Overview tab should be visible
    await expect(clientWorkspacePage.overviewTab).toBeVisible()
  })

  test('workspace page members tab shows members list', async ({
    page,
    clientWorkspacePage,
    clientDashboardPage,
  }) => {
    await clientDashboardPage.goto()
    await expect(clientDashboardPage.heading).toBeVisible({ timeout: 10000 })

    const cards = clientDashboardPage.workspaceCards
    const cardCount = await cards.count()

    if (cardCount === 0) {
      test.skip(true, 'No workspaces available to test')
      return
    }

    await cards.first().click()
    await expect(page).toHaveURL(/\/app\/workspace\//, { timeout: 10000 })

    // Click Members tab
    await clientWorkspacePage.membersTab.click()

    // Should show either members list/table or empty state
    const membersTable = page.locator('table')
    const emptyState = page.getByText(/no members/i)
    await expect(membersTable.or(emptyState)).toBeVisible({ timeout: 5000 })
  })

  test('workspace page shows workspace details', async ({
    page,
    clientDashboardPage,
  }) => {
    await clientDashboardPage.goto()
    await expect(clientDashboardPage.heading).toBeVisible({ timeout: 10000 })

    const cards = clientDashboardPage.workspaceCards
    const cardCount = await cards.count()

    if (cardCount === 0) {
      test.skip(true, 'No workspaces available to test')
      return
    }

    await cards.first().click()
    await expect(page).toHaveURL(/\/app\/workspace\//, { timeout: 10000 })

    // Should show workspace name in heading
    await expect(page.getByRole('heading').first()).toBeVisible({ timeout: 10000 })

    // Should show workspace details section
    const detailSection = page.locator('dl, .space-y-3').first()
    await expect(detailSection).toBeVisible()
  })
})
