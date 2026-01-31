import type { Page, Locator } from '@playwright/test'

export abstract class BasePage {
  constructor(protected readonly page: Page) {}

  async waitForPageLoad(): Promise<void> {
    await this.page.waitForLoadState('networkidle')
  }

  get heading(): Locator {
    return this.page.getByRole('heading', { level: 1 })
  }

  async navigateTo(path: string): Promise<void> {
    await this.page.goto(path)
    await this.waitForPageLoad()
  }

  async currentPath(): Promise<string> {
    return new URL(this.page.url()).pathname
  }
}
