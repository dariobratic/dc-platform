import { describe, it, expect, vi, beforeEach } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import OrganizationsPage from '../OrganizationsPage.vue'

vi.mock('@/composables/useApiClient', () => ({
  useApiClient: vi.fn().mockReturnValue({}),
}))

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: vi.fn() }),
  useRoute: () => ({ params: {}, matched: [], meta: {} }),
}))

const mockGetOrganizations = vi.fn()
const mockCreateOrganization = vi.fn()
const mockDeleteOrganization = vi.fn()

vi.mock('@dc-platform/api-client', () => ({
  getOrganizations: (...args: any[]) => mockGetOrganizations(...args),
  createOrganization: (...args: any[]) => mockCreateOrganization(...args),
  deleteOrganization: (...args: any[]) => mockDeleteOrganization(...args),
}))

import { mountPage } from '../../../test/helpers'

describe('OrganizationsPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('shows spinner during load', () => {
    mockGetOrganizations.mockReturnValue(new Promise(() => {}))

    const wrapper = mountPage(OrganizationsPage)
    expect(wrapper.find('[data-testid="dc-spinner"]').exists()).toBe(true)
  })

  it('renders organizations after load', async () => {
    mockGetOrganizations.mockResolvedValue([
      { id: '1', name: 'Acme Corp', slug: 'acme', status: 'Active', createdAt: '2026-01-01' },
      { id: '2', name: 'Test Org', slug: 'test', status: 'Active', createdAt: '2026-01-02' },
    ])

    const wrapper = mountPage(OrganizationsPage)
    await flushPromises()

    expect(wrapper.find('[data-testid="dc-spinner"]').exists()).toBe(false)
    expect(wrapper.text()).toContain('Acme Corp')
    expect(wrapper.text()).toContain('Test Org')
  })

  it('shows error alert on failure', async () => {
    mockGetOrganizations.mockRejectedValue(new Error('Failed'))

    const wrapper = mountPage(OrganizationsPage)
    await flushPromises()

    expect(wrapper.find('[data-testid="dc-alert"]').exists()).toBe(true)
  })
})
