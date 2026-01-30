import { describe, it, expect, vi, beforeEach } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import AuditLogPage from '../AuditLogPage.vue'

vi.mock('@/composables/useApiClient', () => ({
  useApiClient: vi.fn().mockReturnValue({}),
}))

const mockGetAuditEntries = vi.fn()

vi.mock('@dc-platform/api-client', () => ({
  getAuditEntries: (...args: any[]) => mockGetAuditEntries(...args),
}))

import { mountPage } from '../../../test/helpers'

describe('AuditLogPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('shows spinner during load', () => {
    mockGetAuditEntries.mockReturnValue(new Promise(() => {}))

    const wrapper = mountPage(AuditLogPage)
    expect(wrapper.find('[data-testid="dc-spinner"]').exists()).toBe(true)
  })

  it('renders audit entries after load', async () => {
    mockGetAuditEntries.mockResolvedValue({
      items: [
        {
          id: 'a1',
          action: 'organization.created',
          entityType: 'Organization',
          entityId: '12345678-1234-1234-1234-123456789012',
          userId: 'user-1',
          userEmail: 'admin@test.com',
          serviceName: 'Directory',
          timestamp: '2026-01-15T10:30:00Z',
        },
      ],
      totalCount: 1,
    })

    const wrapper = mountPage(AuditLogPage)
    await flushPromises()

    expect(wrapper.find('[data-testid="dc-spinner"]').exists()).toBe(false)
    expect(wrapper.text()).toContain('organization.created')
    expect(wrapper.text()).toContain('Directory')
  })

  it('shows error alert on failure', async () => {
    mockGetAuditEntries.mockRejectedValue(new Error('Server error'))

    const wrapper = mountPage(AuditLogPage)
    await flushPromises()

    expect(wrapper.find('[data-testid="dc-alert"]').exists()).toBe(true)
  })
})
