import type { Page, Locator } from '@playwright/test'
import { BasePage } from './BasePage'

export class DashboardPage extends BasePage {
  constructor(page: Page) {
    super(page)
  }

  get welcomeMessage(): Locator {
    return this.page.getByText('Welcome back,')
  }

  get organizationCard(): Locator {
    return this.page.getByText('Organization')
  }

  get welcomeSection(): Locator {
    return this.page.getByText('Welcome to DC Platform')
  }

  get userMenu(): Locator {
    return this.page.locator('.relative button').first()
  }

  get signOutButton(): Locator {
    return this.page.getByRole('button', { name: 'Sign out' })
  }

  async goto(): Promise<void> {
    await this.navigateTo('/dashboard')
  }

  async openUserMenu(): Promise<void> {
    // Click the user avatar/name button in the header
    await this.userMenu.click()
  }

  async signOut(): Promise<void> {
    await this.openUserMenu()
    await this.signOutButton.click()
  }
}
