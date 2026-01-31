import type { Page, Locator } from '@playwright/test'
import { BasePage } from './BasePage'

export class OrganizationPickerPage extends BasePage {
  constructor(page: Page) {
    super(page)
  }

  get loadingSpinner(): Locator {
    return this.page.locator('.animate-spin')
  }

  get errorMessage(): Locator {
    return this.page.locator('.text-red-600')
  }

  get retryButton(): Locator {
    return this.page.getByRole('button', { name: 'Retry' })
  }

  get noOrganizationsMessage(): Locator {
    return this.page.getByText('You are not a member of any organizations yet')
  }

  get organizationButtons(): Locator {
    // Organization buttons are the full-width border buttons containing org name and slug
    return this.page.locator('button.w-full.border')
  }

  async goto(): Promise<void> {
    await this.navigateTo('/select-organization')
  }

  async selectOrganization(name: string): Promise<void> {
    await this.page.getByRole('button', { name }).click()
  }

  async selectFirstOrganization(): Promise<void> {
    await this.organizationButtons.first().click()
  }
}
