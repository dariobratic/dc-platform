import { test as base } from '@playwright/test'
import { LoginPage } from '../pages/LoginPage'
import { SignupPage } from '../pages/SignupPage'
import { OrganizationPickerPage } from '../pages/OrganizationPickerPage'
import { DashboardPage } from '../pages/DashboardPage'

type PageFixtures = {
  loginPage: LoginPage
  signupPage: SignupPage
  orgPickerPage: OrganizationPickerPage
  dashboardPage: DashboardPage
}

export const test = base.extend<PageFixtures>({
  loginPage: async ({ page }, use) => {
    await use(new LoginPage(page))
  },
  signupPage: async ({ page }, use) => {
    await use(new SignupPage(page))
  },
  orgPickerPage: async ({ page }, use) => {
    await use(new OrganizationPickerPage(page))
  },
  dashboardPage: async ({ page }, use) => {
    await use(new DashboardPage(page))
  },
})

export { expect } from '@playwright/test'
