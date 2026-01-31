import type { Page, Locator } from '@playwright/test'
import { BasePage } from './BasePage'

export class AdminWorkspacesPage extends BasePage {
  constructor(page: Page) {
    super(page)
  }

  get workspacesTable(): Locator {
    return this.page.locator('table')
  }

  get searchInput(): Locator {
    return this.page.getByPlaceholder(/search/i)
  }

  get emptyState(): Locator {
    return this.page.getByText(/no workspaces/i)
  }

  async goto(): Promise<void> {
    await this.navigateTo('/admin/workspaces')
  }

  async getWorkspaceRow(name: string): Promise<Locator> {
    return this.page.getByRole('row').filter({ hasText: name })
  }
}
