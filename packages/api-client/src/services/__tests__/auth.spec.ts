import { describe, it, expect, vi } from 'vitest'
import { exchangeCodeForToken, refreshToken, getUserInfo, logout } from '../auth'

function createMockClient() {
  return {
    get: vi.fn().mockResolvedValue({ data: {} }),
    post: vi.fn().mockResolvedValue({ data: {} }),
  } as any
}

describe('auth service', () => {
  it('exchangeCodeForToken calls POST /api/auth/token', async () => {
    const client = createMockClient()
    const request = { code: 'auth-code', redirectUri: 'http://localhost:3000/callback' }
    await exchangeCodeForToken(client, request)
    expect(client.post).toHaveBeenCalledWith('/api/auth/token', request)
  })

  it('refreshToken calls POST /api/auth/refresh', async () => {
    const client = createMockClient()
    const request = { refreshToken: 'refresh-123' }
    await refreshToken(client, request)
    expect(client.post).toHaveBeenCalledWith('/api/auth/refresh', request)
  })

  it('getUserInfo calls GET /api/auth/userinfo', async () => {
    const client = createMockClient()
    await getUserInfo(client)
    expect(client.get).toHaveBeenCalledWith('/api/auth/userinfo')
  })

  it('logout calls POST /api/auth/logout', async () => {
    const client = createMockClient()
    const request = { refreshToken: 'refresh-123' }
    await logout(client, request)
    expect(client.post).toHaveBeenCalledWith('/api/auth/logout', request)
  })
})
