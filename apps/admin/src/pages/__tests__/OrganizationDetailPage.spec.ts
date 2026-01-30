import { describe, it, expect, vi, beforeEach } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import OrganizationDetailPage from '../OrganizationDetailPage.vue'

vi.mock('@/composables/useApiClient', () => ({
  useApiClient: vi.fn().mockReturnValue({}),
}))

const mockGetOrganization = vi.fn()
const mockGetWorkspacesByOrganization = vi.fn()

vi.mock('@dc-platform/api-client', () => ({
  getOrganization: (...args: any[]) => mockGetOrganization(...args),
  getWorkspacesByOrganization: (...args: any[]) => mockGetWorkspacesByOrganization(...args),
  createWorkspace: vi.fn(),
}))

vi.mock('vue-router', () => ({
  useRoute: () => ({ params: { id: 'org-123' } }),
}))

import { mountPage } from '../../../test/helpers'

describe('OrganizationDetailPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('shows spinner during load', () => {
    mockGetOrganization.mockReturnValue(new Promise(() => {}))
    mockGetWorkspacesByOrganization.mockReturnValue(new Promise(() => {}))

    const wrapper = mountPage(OrganizationDetailPage)
    expect(wrapper.find('[data-testid="dc-spinner"]').exists()).toBe(true)
  })

  it('renders organization details after load', async () => {
    mockGetOrganization.mockResolvedValue({
      id: 'org-123',
      name: 'Acme Corp',
      slug: 'acme',
      status: 'Active',
      createdAt: '2026-01-01',
      updatedAt: '2026-01-15',
      settings: {},
    })
    mockGetWorkspacesByOrganization.mockResolvedValue([
      { id: 'ws-1', name: 'Engineering', slug: 'eng', status: 'Active', createdAt: '2026-01-02' },
    ])

    const wrapper = mountPage(OrganizationDetailPage)
    await flushPromises()

    expect(wrapper.find('[data-testid="dc-spinner"]').exists()).toBe(false)
    expect(wrapper.text()).toContain('Acme Corp')
    expect(wrapper.text()).toContain('acme')
  })

  it('shows error alert on failure', async () => {
    mockGetOrganization.mockRejectedValue(new Error('Not found'))
    mockGetWorkspacesByOrganization.mockRejectedValue(new Error('Not found'))

    const wrapper = mountPage(OrganizationDetailPage)
    await flushPromises()

    expect(wrapper.find('[data-testid="dc-alert"]').exists()).toBe(true)
  })
})
