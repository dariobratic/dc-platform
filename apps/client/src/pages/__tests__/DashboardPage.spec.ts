import { describe, it, expect, vi, beforeEach } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import DashboardPage from '../DashboardPage.vue'

vi.mock('@/composables/useApiClient', () => ({
  useApiClient: vi.fn().mockReturnValue({}),
}))

const mockGetWorkspacesByOrganization = vi.fn()
const mockGetAuditEntries = vi.fn()

vi.mock('@dc-platform/api-client', () => ({
  getWorkspacesByOrganization: (...args: any[]) => mockGetWorkspacesByOrganization(...args),
  getAuditEntries: (...args: any[]) => mockGetAuditEntries(...args),
}))

import { mountPage } from '../../../test/helpers'

describe('DashboardPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    sessionStorage.clear()
  })

  it('shows spinner during load', () => {
    sessionStorage.setItem('organizationId', 'org-1')
    mockGetWorkspacesByOrganization.mockReturnValue(new Promise(() => {}))
    mockGetAuditEntries.mockReturnValue(new Promise(() => {}))

    const wrapper = mountPage(DashboardPage)
    expect(wrapper.find('[data-testid="dc-spinner"]').exists()).toBe(true)
  })

  it('renders dashboard data after load', async () => {
    sessionStorage.setItem('organizationId', 'org-1')
    sessionStorage.setItem('oidc.user:http://localhost:8080/realms/dc-platform:dc-platform-admin', JSON.stringify({
      access_token: 'test-token',
      profile: { preferred_username: 'John', email: 'john@example.com', sub: 'user-123' },
    }))

    mockGetWorkspacesByOrganization.mockResolvedValue([
      { id: 'ws-1', name: 'Engineering', slug: 'eng', status: 'Active' },
    ])
    mockGetAuditEntries.mockResolvedValue({ items: [], totalCount: 0 })

    const wrapper = mountPage(DashboardPage)
    await flushPromises()

    expect(wrapper.find('[data-testid="dc-spinner"]').exists()).toBe(false)
    expect(wrapper.text()).toContain('Welcome back, John')
    expect(wrapper.text()).toContain('Engineering')
  })

  it('shows error alert on failure', async () => {
    sessionStorage.setItem('organizationId', 'org-1')
    mockGetWorkspacesByOrganization.mockRejectedValue(new Error('Failed'))
    mockGetAuditEntries.mockRejectedValue(new Error('Failed'))

    const wrapper = mountPage(DashboardPage)
    await flushPromises()

    expect(wrapper.find('[data-testid="dc-alert"]').exists()).toBe(true)
  })

  it('shows error when no organization selected', async () => {
    // No organizationId in sessionStorage
    const wrapper = mountPage(DashboardPage)
    await flushPromises()

    expect(wrapper.find('[data-testid="dc-alert"]').exists()).toBe(true)
    expect(wrapper.text()).toContain('No organization selected')
  })
})
