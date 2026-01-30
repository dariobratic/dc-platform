import { describe, it, expect, vi, beforeEach } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import DashboardPage from '../DashboardPage.vue'

// Mock the composable
vi.mock('@/composables/useApiClient', () => ({
  useApiClient: vi.fn().mockReturnValue({}),
}))

// Mock the API functions
const mockGetDashboard = vi.fn()
const mockGetOrganizations = vi.fn()
const mockGetRecentAuditEntries = vi.fn()

vi.mock('@dc-platform/api-client', () => ({
  getDashboard: (...args: any[]) => mockGetDashboard(...args),
  getOrganizations: (...args: any[]) => mockGetOrganizations(...args),
  getRecentAuditEntries: (...args: any[]) => mockGetRecentAuditEntries(...args),
}))

// Import helper after mocks
import { mountPage } from '../../../test/helpers'

describe('DashboardPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('shows spinner during load', () => {
    mockGetDashboard.mockReturnValue(new Promise(() => {})) // Never resolves
    mockGetOrganizations.mockReturnValue(new Promise(() => {}))
    mockGetRecentAuditEntries.mockReturnValue(new Promise(() => {}))

    const wrapper = mountPage(DashboardPage)
    expect(wrapper.find('[data-testid="dc-spinner"]').exists()).toBe(true)
  })

  it('renders dashboard data after load', async () => {
    mockGetDashboard.mockResolvedValue({ organizationCount: 5, auditEntryCount: 100 })
    mockGetOrganizations.mockResolvedValue([
      { id: '1', name: 'Org 1', slug: 'org-1', status: 'Active' },
    ])
    mockGetRecentAuditEntries.mockResolvedValue([
      { id: 'a1', action: 'org.created', entityType: 'Organization', entityId: '12345678-abcd', timestamp: '2026-01-01T00:00:00Z' },
    ])

    const wrapper = mountPage(DashboardPage)
    await flushPromises()

    expect(wrapper.find('[data-testid="dc-spinner"]').exists()).toBe(false)
    expect(wrapper.text()).toContain('Admin Dashboard')
    expect(wrapper.text()).toContain('Org 1')
    expect(wrapper.text()).toContain('org.created')
  })

  it('shows error alert on failure', async () => {
    mockGetDashboard.mockRejectedValue(new Error('Network error'))
    mockGetOrganizations.mockRejectedValue(new Error('Network error'))
    mockGetRecentAuditEntries.mockRejectedValue(new Error('Network error'))

    const wrapper = mountPage(DashboardPage)
    await flushPromises()

    expect(wrapper.find('[data-testid="dc-spinner"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="dc-alert"]').exists()).toBe(true)
  })
})
