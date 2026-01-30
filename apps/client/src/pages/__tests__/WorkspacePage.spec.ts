import { describe, it, expect, vi, beforeEach } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import WorkspacePage from '../WorkspacePage.vue'

vi.mock('@/composables/useApiClient', () => ({
  useApiClient: vi.fn().mockReturnValue({}),
}))

const mockGetWorkspace = vi.fn()
const mockGetWorkspaceMembers = vi.fn()

vi.mock('@dc-platform/api-client', () => ({
  getWorkspace: (...args: any[]) => mockGetWorkspace(...args),
  getWorkspaceMembers: (...args: any[]) => mockGetWorkspaceMembers(...args),
}))

vi.mock('vue-router', () => ({
  useRoute: () => ({ params: { id: 'ws-123' } }),
}))

import { mountPage } from '../../../test/helpers'

describe('WorkspacePage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('shows spinner during load', () => {
    mockGetWorkspace.mockReturnValue(new Promise(() => {}))
    mockGetWorkspaceMembers.mockReturnValue(new Promise(() => {}))

    const wrapper = mountPage(WorkspacePage)
    expect(wrapper.find('[data-testid="dc-spinner"]').exists()).toBe(true)
  })

  it('renders workspace data after load', async () => {
    mockGetWorkspace.mockResolvedValue({
      id: 'ws-123',
      name: 'Engineering',
      slug: 'eng',
      status: 'Active',
      createdAt: '2026-01-01',
    })
    mockGetWorkspaceMembers.mockResolvedValue([
      { id: 'm1', userId: 'user-1', role: 'Admin', joinedAt: '2026-01-01' },
    ])

    const wrapper = mountPage(WorkspacePage)
    await flushPromises()

    expect(wrapper.find('[data-testid="dc-spinner"]').exists()).toBe(false)
    expect(wrapper.text()).toContain('Engineering')
    expect(wrapper.text()).toContain('eng')
  })

  it('shows error alert on failure', async () => {
    mockGetWorkspace.mockRejectedValue(new Error('Not found'))
    mockGetWorkspaceMembers.mockRejectedValue(new Error('Not found'))

    const wrapper = mountPage(WorkspacePage)
    await flushPromises()

    expect(wrapper.find('[data-testid="dc-alert"]').exists()).toBe(true)
  })
})
