import type { Page, Locator } from '@playwright/test'
import { BasePage } from './BasePage'

export class ClientDashboardPage extends BasePage {
  constructor(page: Page) {
    super(page)
  }

  get welcomeMessage(): Locator {
    return this.page.getByText(/welcome/i)
  }

  get workspaceCards(): Locator {
    return this.page.locator('[data-testid="workspace-card"]')
  }

  get emptyState(): Locator {
    return this.page.getByText(/no workspaces/i)
  }

  async goto(): Promise<void> {
    await this.navigateTo('/app')
  }

  async getWorkspaceCard(name: string): Promise<Locator> {
    return this.page.locator('[data-testid="workspace-card"]').filter({ hasText: name })
  }
}
