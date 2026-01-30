import { defineConfig } from 'vitest/config'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src'),
      'admin/routes': resolve(__dirname, 'test/mocks/admin-routes.ts'),
      'client/routes': resolve(__dirname, 'test/mocks/client-routes.ts'),
      '@dc-platform/ui-kit': resolve(__dirname, 'test/mocks/ui-kit.ts'),
    },
  },
  test: {
    environment: 'happy-dom',
    globals: true,
    setupFiles: ['./test/setup.ts'],
    include: ['src/**/__tests__/*.spec.ts'],
  },
})
