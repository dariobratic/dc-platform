import { test as base } from '@playwright/test'
import { LoginPage } from '../pages/LoginPage'
import { SignupPage } from '../pages/SignupPage'
import { OrganizationPickerPage } from '../pages/OrganizationPickerPage'
import { DashboardPage } from '../pages/DashboardPage'
import { AdminDashboardPage } from '../pages/AdminDashboardPage'
import { AdminOrganizationsPage } from '../pages/AdminOrganizationsPage'
import { AdminWorkspacesPage } from '../pages/AdminWorkspacesPage'
import { AdminRolesPage } from '../pages/AdminRolesPage'
import { AdminAuditLogPage } from '../pages/AdminAuditLogPage'
import { AdminOrganizationDetailPage } from '../pages/AdminOrganizationDetailPage'
import { AdminWorkspaceDetailPage } from '../pages/AdminWorkspaceDetailPage'
import { ClientDashboardPage } from '../pages/ClientDashboardPage'
import { ClientWorkspacePage } from '../pages/ClientWorkspacePage'
import * as fs from 'fs'
import * as path from 'path'

type TestOptions = {
  injectAuth: boolean
}

type PageFixtures = {
  loginPage: LoginPage
  signupPage: SignupPage
  orgPickerPage: OrganizationPickerPage
  dashboardPage: DashboardPage
  adminDashboardPage: AdminDashboardPage
  adminOrganizationsPage: AdminOrganizationsPage
  adminOrganizationDetailPage: AdminOrganizationDetailPage
  adminWorkspacesPage: AdminWorkspacesPage
  adminWorkspaceDetailPage: AdminWorkspaceDetailPage
  adminRolesPage: AdminRolesPage
  adminAuditLogPage: AdminAuditLogPage
  clientDashboardPage: ClientDashboardPage
  clientWorkspacePage: ClientWorkspacePage
}

// Extend the base test with a fixture that injects sessionStorage
export const test = base.extend<PageFixtures & TestOptions>({
  // Option to control sessionStorage injection. Defaults to true.
  // Set to false in tests that need unauthenticated state.
  injectAuth: [true, { option: true }],

  context: async ({ context, injectAuth }, use) => {
    if (injectAuth) {
      const authFile = path.join(__dirname, '..', '.auth', 'user.json')
      if (fs.existsSync(authFile)) {
        const savedState = JSON.parse(fs.readFileSync(authFile, 'utf-8'))
        const origin = savedState.origins?.find((o: {origin: string}) => o.origin === 'http://localhost:3000')

        if (origin?.sessionStorage && origin.sessionStorage.length > 0) {
          // Inject sessionStorage into all new pages in this context
          await context.addInitScript(({sessionData}) => {
            for (const item of sessionData) {
              sessionStorage.setItem(item.name, item.value)
            }
          }, { sessionData: origin.sessionStorage })
        }
      }
    }

    await use(context)
  },
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
  adminDashboardPage: async ({ page }, use) => {
    await use(new AdminDashboardPage(page))
  },
  adminOrganizationsPage: async ({ page }, use) => {
    await use(new AdminOrganizationsPage(page))
  },
  adminOrganizationDetailPage: async ({ page }, use) => {
    await use(new AdminOrganizationDetailPage(page))
  },
  adminWorkspacesPage: async ({ page }, use) => {
    await use(new AdminWorkspacesPage(page))
  },
  adminWorkspaceDetailPage: async ({ page }, use) => {
    await use(new AdminWorkspaceDetailPage(page))
  },
  adminRolesPage: async ({ page }, use) => {
    await use(new AdminRolesPage(page))
  },
  adminAuditLogPage: async ({ page }, use) => {
    await use(new AdminAuditLogPage(page))
  },
  clientDashboardPage: async ({ page }, use) => {
    await use(new ClientDashboardPage(page))
  },
  clientWorkspacePage: async ({ page }, use) => {
    await use(new ClientWorkspacePage(page))
  },
})

export { expect } from '@playwright/test'
