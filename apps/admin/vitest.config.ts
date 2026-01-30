import { defineConfig } from 'vitest/config'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src'),
      '@dc-platform/ui-kit': resolve(__dirname, '../../packages/ui-kit/src/index.ts'),
      '@dc-platform/api-client': resolve(__dirname, '../../packages/api-client/src/index.ts'),
      '@dc-platform/shared-types': resolve(__dirname, '../../packages/shared-types/src/index.ts'),
    },
  },
  test: {
    environment: 'happy-dom',
    globals: true,
    setupFiles: ['./test/setup.ts'],
    include: ['src/**/__tests__/*.spec.ts'],
  },
})
