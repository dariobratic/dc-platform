import type { Page, Locator } from '@playwright/test'
import { BasePage } from './BasePage'

export class LoginPage extends BasePage {
  constructor(page: Page) {
    super(page)
  }

  get emailInput(): Locator {
    return this.page.getByLabel('Email')
  }

  get passwordInput(): Locator {
    return this.page.getByLabel('Password')
  }

  get submitButton(): Locator {
    return this.page.getByRole('button', { name: 'Sign in' })
  }

  get errorAlert(): Locator {
    return this.page.getByRole('alert')
  }

  get signupLink(): Locator {
    return this.page.getByRole('link', { name: 'Sign up' })
  }

  async goto(): Promise<void> {
    await this.navigateTo('/login')
  }

  async login(email: string, password: string): Promise<void> {
    await this.emailInput.fill(email)
    await this.passwordInput.fill(password)
    await this.submitButton.click()
  }
}
