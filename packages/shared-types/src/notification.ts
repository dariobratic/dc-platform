// Notification service DTOs
// Matches: services/notification/src/Notification.API/DTOs/

export interface EmailNotificationRequest {
  to: string
  subject: string
  body: string
  templateId?: string
  templateData?: Record<string, string>
}

export interface PushNotificationRequest {
  userId: string
  title: string
  message: string
  data?: Record<string, string>
}

export interface NotificationResponse {
  id: string
  status: string
  sentAt: string
}
