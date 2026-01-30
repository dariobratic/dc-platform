import { describe, it, expect, vi, beforeEach } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import RolesPage from '../RolesPage.vue'

vi.mock('@/composables/useApiClient', () => ({
  useApiClient: vi.fn().mockReturnValue({}),
}))

const mockGetOrganizations = vi.fn()
const mockGetRolesByScope = vi.fn()

vi.mock('@dc-platform/api-client', () => ({
  getOrganizations: (...args: any[]) => mockGetOrganizations(...args),
  getRolesByScope: (...args: any[]) => mockGetRolesByScope(...args),
  createRole: vi.fn(),
  deleteRole: vi.fn(),
}))

import { mountPage } from '../../../test/helpers'

describe('RolesPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('shows spinner during load', () => {
    mockGetOrganizations.mockReturnValue(new Promise(() => {}))

    const wrapper = mountPage(RolesPage)
    expect(wrapper.find('[data-testid="dc-spinner"]').exists()).toBe(true)
  })

  it('renders roles after load', async () => {
    mockGetOrganizations.mockResolvedValue([
      { id: 'org-1', name: 'Org 1' },
    ])
    mockGetRolesByScope.mockResolvedValue([
      { id: 'role-1', name: 'Admin', description: 'Full access', scopeType: 'Organization', permissions: ['read', 'write'], createdAt: '2026-01-01', isSystem: false },
    ])

    const wrapper = mountPage(RolesPage)
    await flushPromises()

    expect(wrapper.find('[data-testid="dc-spinner"]').exists()).toBe(false)
    expect(wrapper.text()).toContain('Admin')
    expect(wrapper.text()).toContain('2 permissions')
  })

  it('shows error alert on failure', async () => {
    mockGetOrganizations.mockRejectedValue(new Error('Failed'))

    const wrapper = mountPage(RolesPage)
    await flushPromises()

    expect(wrapper.find('[data-testid="dc-alert"]').exists()).toBe(true)
  })
})
