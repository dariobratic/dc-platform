import type { Page, Locator } from '@playwright/test'
import { BasePage } from './BasePage'

export class AdminOrganizationDetailPage extends BasePage {
  constructor(page: Page) {
    super(page)
  }

  // Tabs
  get overviewTab(): Locator {
    return this.page.getByRole('button', { name: 'Overview' })
  }

  get workspacesTab(): Locator {
    return this.page.getByRole('button', { name: /Workspaces/ })
  }

  get membersTab(): Locator {
    return this.page.getByRole('button', { name: 'Members' })
  }

  // Overview tab elements
  get editButton(): Locator {
    return this.page.getByRole('button', { name: 'Edit' })
  }

  get orgNameInput(): Locator {
    return this.page.getByLabel('Organization Name')
  }

  get saveButton(): Locator {
    return this.page.getByRole('button', { name: 'Save' })
  }

  get cancelButton(): Locator {
    return this.page.getByRole('button', { name: 'Cancel' })
  }

  // Detail fields
  get detailsList(): Locator {
    return this.page.locator('dl')
  }

  // Workspaces tab elements
  get createWorkspaceButton(): Locator {
    return this.page.getByRole('button', { name: 'Create Workspace' })
  }

  get workspaceCards(): Locator {
    return this.page.locator('.grid > div').filter({ has: this.page.locator('a, h3, .font-medium') })
  }

  get workspacesEmptyState(): Locator {
    return this.page.getByText(/no workspaces/i)
  }

  // Create workspace modal
  get workspaceNameInput(): Locator {
    return this.page.getByLabel('Name')
  }

  get workspaceSlugInput(): Locator {
    return this.page.getByLabel('Slug')
  }

  get modalCreateButton(): Locator {
    return this.page.locator('[role="dialog"]').getByRole('button', { name: 'Create' })
  }

  get modalCancelButton(): Locator {
    return this.page.locator('[role="dialog"]').getByRole('button', { name: 'Cancel' })
  }

  // Members tab elements
  get addMemberButton(): Locator {
    return this.page.getByRole('button', { name: 'Add Member' })
  }

  get membersEmptyState(): Locator {
    return this.page.getByText(/no workspaces in this organization/i)
  }

  // Loading & error
  get loadingSpinner(): Locator {
    return this.page.locator('.animate-spin')
  }

  get errorAlert(): Locator {
    return this.page.getByRole('alert')
  }

  async goto(orgId: string): Promise<void> {
    await this.navigateTo(`/admin/organizations/${orgId}`)
  }

  async switchToTab(tab: 'overview' | 'workspaces' | 'members'): Promise<void> {
    switch (tab) {
      case 'overview':
        await this.overviewTab.click()
        break
      case 'workspaces':
        await this.workspacesTab.click()
        break
      case 'members':
        await this.membersTab.click()
        break
    }
  }
}
