import type { AxiosInstance } from 'axios'
import type {
  EmailNotificationRequest,
  PushNotificationRequest,
  NotificationResponse,
} from '@dc-platform/shared-types'

export async function sendEmail(client: AxiosInstance, request: EmailNotificationRequest): Promise<NotificationResponse> {
  const { data } = await client.post('/api/v1/notifications/email', request)
  return data
}

export async function sendPush(client: AxiosInstance, request: PushNotificationRequest): Promise<NotificationResponse> {
  const { data } = await client.post('/api/v1/notifications/push', request)
  return data
}
