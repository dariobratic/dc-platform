import type { Page, Locator } from '@playwright/test'
import { BasePage } from './BasePage'

export class AdminOrganizationsPage extends BasePage {
  constructor(page: Page) {
    super(page)
  }

  get createOrgButton(): Locator {
    return this.page.getByRole('button', { name: 'Create Organization' })
  }

  get searchInput(): Locator {
    return this.page.getByPlaceholder(/search/i)
  }

  get organizationsTable(): Locator {
    return this.page.locator('table')
  }

  get emptyState(): Locator {
    return this.page.getByText(/no organizations/i)
  }

  async goto(): Promise<void> {
    await this.navigateTo('/admin/organizations')
  }

  async getOrganizationRow(name: string): Promise<Locator> {
    return this.page.getByRole('row').filter({ hasText: name })
  }
}
