import type { Page, Locator } from '@playwright/test'
import { BasePage } from './BasePage'

export class AdminDashboardPage extends BasePage {
  constructor(page: Page) {
    super(page)
  }

  get organizationsLink(): Locator {
    return this.page.getByRole('link', { name: 'Organizations' })
  }

  get workspacesLink(): Locator {
    return this.page.getByRole('link', { name: 'Workspaces' })
  }

  get rolesLink(): Locator {
    return this.page.getByRole('link', { name: 'Roles' })
  }

  get auditLogLink(): Locator {
    return this.page.getByRole('link', { name: 'Audit Log' })
  }

  get usersLink(): Locator {
    return this.page.getByRole('link', { name: 'Users' })
  }

  async goto(): Promise<void> {
    await this.navigateTo('/admin')
  }
}
