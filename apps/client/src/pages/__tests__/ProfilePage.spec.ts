import { describe, it, expect, beforeEach } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import ProfilePage from '../ProfilePage.vue'

import { mountPage } from '../../../test/helpers'

describe('ProfilePage', () => {
  beforeEach(() => {
    sessionStorage.clear()
  })

  it('renders user information from OIDC session', async () => {
    sessionStorage.setItem('oidc.user:http://localhost:8080/realms/dc-platform:dc-platform-admin', JSON.stringify({
      access_token: 'test-token',
      profile: {
        preferred_username: 'JaneDoe',
        email: 'jane@example.com',
        sub: 'user-abc-123',
      },
    }))

    const wrapper = mountPage(ProfilePage)
    await flushPromises()

    expect(wrapper.text()).toContain('JaneDoe')
    expect(wrapper.text()).toContain('jane@example.com')
    expect(wrapper.text()).toContain('user-abc-123')
  })

  it('renders preferences section', async () => {
    const wrapper = mountPage(ProfilePage)
    await flushPromises()

    expect(wrapper.text()).toContain('Preferences')
  })

  it('shows fallback when no OIDC data', async () => {
    // No OIDC data in sessionStorage
    const wrapper = mountPage(ProfilePage)
    await flushPromises()

    // The card with user info should not render (v-if="user")
    expect(wrapper.text()).toContain('Profile')
    expect(wrapper.text()).toContain('Preferences')
  })
})
