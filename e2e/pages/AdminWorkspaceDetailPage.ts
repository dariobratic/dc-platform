import type { Page, Locator } from '@playwright/test'
import { BasePage } from './BasePage'

export class AdminWorkspaceDetailPage extends BasePage {
  constructor(page: Page) {
    super(page)
  }

  // Tabs
  get overviewTab(): Locator {
    return this.page.getByRole('button', { name: 'Overview' })
  }

  get membersTab(): Locator {
    return this.page.getByRole('button', { name: /Members/ })
  }

  get settingsTab(): Locator {
    return this.page.getByRole('button', { name: 'Settings' })
  }

  // Overview tab elements
  get detailsList(): Locator {
    return this.page.locator('dl')
  }

  get organizationLink(): Locator {
    return this.page.locator('dd a')
  }

  // Members tab elements
  get addMemberButton(): Locator {
    return this.page.getByRole('button', { name: 'Add Member' })
  }

  get membersTable(): Locator {
    return this.page.locator('table')
  }

  get membersEmptyState(): Locator {
    return this.page.getByText(/no members/i)
  }

  // Settings tab elements
  get workspaceNameInput(): Locator {
    return this.page.getByLabel('Workspace Name')
  }

  get saveNameButton(): Locator {
    return this.page.getByRole('button', { name: 'Save' })
  }

  get deleteWorkspaceButton(): Locator {
    return this.page.getByRole('button', { name: 'Delete Workspace' })
  }

  // Add member modal
  get userIdInput(): Locator {
    return this.page.getByLabel('User ID')
  }

  get roleSelect(): Locator {
    return this.page.getByLabel('Role')
  }

  get modalAddButton(): Locator {
    return this.page.locator('[role="dialog"]').getByRole('button', { name: 'Add Member' })
  }

  get modalCancelButton(): Locator {
    return this.page.locator('[role="dialog"]').getByRole('button', { name: 'Cancel' })
  }

  // Loading & error
  get loadingSpinner(): Locator {
    return this.page.locator('.animate-spin')
  }

  get errorAlert(): Locator {
    return this.page.getByRole('alert')
  }

  get successAlert(): Locator {
    return this.page.locator('.text-green-800, [data-variant="success"]')
  }

  async goto(workspaceId: string): Promise<void> {
    await this.navigateTo(`/admin/workspaces/${workspaceId}`)
  }

  async switchToTab(tab: 'overview' | 'members' | 'settings'): Promise<void> {
    switch (tab) {
      case 'overview':
        await this.overviewTab.click()
        break
      case 'members':
        await this.membersTab.click()
        break
      case 'settings':
        await this.settingsTab.click()
        break
    }
  }
}
