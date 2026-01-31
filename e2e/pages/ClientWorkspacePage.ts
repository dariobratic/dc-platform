import type { Page, Locator } from '@playwright/test'
import { BasePage } from './BasePage'

export class ClientWorkspacePage extends BasePage {
  constructor(page: Page) {
    super(page)
  }

  get overviewTab(): Locator {
    return this.page.getByRole('button', { name: 'Overview' })
  }

  get membersTab(): Locator {
    return this.page.getByRole('button', { name: 'Members' })
  }

  get membersList(): Locator {
    return this.page.locator('[data-testid="members-list"]')
  }

  async goto(workspaceId: string): Promise<void> {
    await this.navigateTo(`/app/workspace/${workspaceId}`)
  }
}
