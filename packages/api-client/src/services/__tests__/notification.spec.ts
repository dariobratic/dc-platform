import { describe, it, expect, vi } from 'vitest'
import { sendEmail, sendPush } from '../notification'

function createMockClient() {
  return {
    post: vi.fn().mockResolvedValue({ data: {} }),
  } as any
}

describe('notification service', () => {
  it('sendEmail calls POST /api/v1/notifications/email', async () => {
    const client = createMockClient()
    const request = { to: 'user@example.com', subject: 'Hello', templateId: 'welcome', templateData: {} }
    await sendEmail(client, request)
    expect(client.post).toHaveBeenCalledWith('/api/v1/notifications/email', request)
  })

  it('sendPush calls POST /api/v1/notifications/push', async () => {
    const client = createMockClient()
    const request = { userId: 'user-1', title: 'Alert', message: 'New update' }
    await sendPush(client, request)
    expect(client.post).toHaveBeenCalledWith('/api/v1/notifications/push', request)
  })
})
