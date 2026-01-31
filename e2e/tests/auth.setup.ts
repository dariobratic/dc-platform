import { test as setup, expect } from '@playwright/test'
import { LoginPage } from '../pages/LoginPage'

const TEST_EMAIL = process.env.E2E_USER_EMAIL ?? 'test@example.com'
const TEST_PASSWORD = process.env.E2E_USER_PASSWORD ?? 'TestPassword123'

const authFile = '.auth/user.json'

setup('authenticate', async ({ page }) => {
  const loginPage = new LoginPage(page)
  await loginPage.goto()
  await loginPage.login(TEST_EMAIL, TEST_PASSWORD)

  // Wait for redirect to org picker or dashboard
  await expect(page).toHaveURL(/\/(select-organization|dashboard)/, {
    timeout: 15000,
  })

  // If on org picker, wait for it to resolve (auto-select or manual)
  if (page.url().includes('select-organization')) {
    // The org picker auto-selects when there's only one org,
    // so just wait for dashboard rather than clicking a button
    await expect(page).toHaveURL(/\/dashboard/, { timeout: 15000 })
  }

  // Wait for page to be fully loaded and auth state to be stored in sessionStorage
  await page.waitForLoadState('networkidle')
  await page.waitForTimeout(1000) // Give sessionStorage time to be written

  // Extract all sessionStorage data manually AND set organizationId
  const sessionStorageData = await page.evaluate(() => {
    // Extract organization ID from the oidc user token
    const oidcKey = Object.keys(sessionStorage).find(k => k.startsWith('oidc.user:'))
    let organizationId: string | null = null

    if (oidcKey) {
      try {
        const userData = JSON.parse(sessionStorage.getItem(oidcKey) || '{}')
        organizationId = userData.profile?.organization_id || null
        if (organizationId) {
          // Store it in sessionStorage so the app can use it
          sessionStorage.setItem('organizationId', organizationId)
        }
      } catch (e) {
        console.error('Failed to parse oidc user:', e)
      }
    }

    const data: Record<string, string> = {}
    for (let i = 0; i < sessionStorage.length; i++) {
      const key = sessionStorage.key(i)
      if (key) {
        data[key] = sessionStorage.getItem(key) || ''
      }
    }
    return { data, organizationId }
  })

  console.log('[Setup] SessionStorage keys:', Object.keys(sessionStorageData.data))
  console.log('[Setup] Organization ID:', sessionStorageData.organizationId)

  if (!Object.keys(sessionStorageData.data).some(key => key.startsWith('oidc.user:'))) {
    throw new Error('Auth state not found in sessionStorage after login')
  }

  // Get standard storage state (cookies, etc.)
  const storageState = await page.context().storageState()

  // Inject sessionStorage into the storage state
  storageState.origins = [
    {
      origin: 'http://localhost:3000',
      localStorage: [],
      sessionStorage: Object.entries(sessionStorageData.data).map(([name, value]) => ({ name, value }))
    }
  ]

  // Save augmented storage state
  const fs = require('fs')
  fs.mkdirSync('.auth', { recursive: true })
  fs.writeFileSync(authFile, JSON.stringify(storageState, null, 2))
  console.log('[Setup] Saved auth state to', authFile)
})
