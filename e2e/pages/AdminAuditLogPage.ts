import type { Page, Locator } from '@playwright/test'
import { BasePage } from './BasePage'

export class AdminAuditLogPage extends BasePage {
  constructor(page: Page) {
    super(page)
  }

  get auditTable(): Locator {
    return this.page.locator('table')
  }

  get filterSection(): Locator {
    return this.page.getByText(/filter/i)
  }

  get emptyState(): Locator {
    return this.page.getByText(/no audit/i)
  }

  async goto(): Promise<void> {
    await this.navigateTo('/admin/audit')
  }
}
