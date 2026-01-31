import type { Page, Locator } from '@playwright/test'
import { BasePage } from './BasePage'

export class AdminRolesPage extends BasePage {
  constructor(page: Page) {
    super(page)
  }

  get rolesTable(): Locator {
    return this.page.locator('table')
  }

  get emptyState(): Locator {
    return this.page.getByText(/no roles/i)
  }

  async goto(): Promise<void> {
    await this.navigateTo('/admin/roles')
  }
}
