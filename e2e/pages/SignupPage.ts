import type { Page, Locator } from '@playwright/test'
import { BasePage } from './BasePage'

export class SignupPage extends BasePage {
  constructor(page: Page) {
    super(page)
  }

  get fullNameInput(): Locator {
    return this.page.getByLabel('Full Name')
  }

  get emailInput(): Locator {
    return this.page.getByLabel('Email')
  }

  get passwordInput(): Locator {
    return this.page.getByLabel('Password', { exact: true })
  }

  get confirmPasswordInput(): Locator {
    return this.page.getByLabel('Confirm Password')
  }

  get organizationNameInput(): Locator {
    return this.page.getByLabel('Organization Name')
  }

  get slugPreview(): Locator {
    return this.page.locator('text=Slug:')
  }

  get submitButton(): Locator {
    return this.page.getByRole('button', { name: 'Create Account' })
  }

  get errorAlert(): Locator {
    return this.page.getByRole('alert')
  }

  get signinLink(): Locator {
    return this.page.getByRole('link', { name: 'Sign in' })
  }

  async goto(): Promise<void> {
    await this.navigateTo('/signup')
  }

  async signup(data: {
    fullName: string
    email: string
    password: string
    organizationName: string
  }): Promise<void> {
    await this.fullNameInput.fill(data.fullName)
    await this.emailInput.fill(data.email)
    await this.passwordInput.fill(data.password)
    await this.confirmPasswordInput.fill(data.password)
    await this.organizationNameInput.fill(data.organizationName)
    await this.submitButton.click()
  }
}
