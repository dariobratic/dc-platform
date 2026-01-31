import { defineConfig, devices } from '@playwright/test'

export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [
    ['html', { open: 'never' }],
    ['list'],
  ],
  use: {
    baseURL: process.env.BASE_URL ?? 'http://localhost:3000',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'on-first-retry',
  },
  projects: [
    // Auth setup — runs first, saves storage state
    {
      name: 'setup',
      testMatch: /.*\.setup\.ts/,
    },
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome'],
        storageState: '.auth/user.json',
      },
      dependencies: ['setup'],
    },
  ],
  webServer: [
    // Remotes must be built first, then served in preview mode
    // because @originjs/vite-plugin-federation doesn't serve
    // remoteEntry.js reliably in Vite 6 dev mode.
    {
      command: 'pnpm --filter admin build && pnpm --filter admin preview',
      url: 'http://localhost:5173',
      reuseExistingServer: !process.env.CI,
      cwd: '..',
      timeout: 120000,
    },
    {
      command: 'pnpm --filter client build && pnpm --filter client preview',
      url: 'http://localhost:5174',
      reuseExistingServer: !process.env.CI,
      cwd: '..',
      timeout: 120000,
    },
    {
      command: 'pnpm --filter shell dev',
      url: 'http://localhost:3000',
      reuseExistingServer: !process.env.CI,
      cwd: '..',
    },
  ],
})
