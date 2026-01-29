# Notification Service

Notification delivery service for DC Platform. Handles email and push notifications (future).

## Service Scope

### This Service IS Responsible For:
- Sending email notifications via SMTP/SendGrid (future integration)
- Sending push notifications to mobile/web clients (placeholder for future)
- Managing notification templates
- Queuing notifications for delivery
- Logging notification delivery status

### This Service IS NOT Responsible For:
- User authentication (-> Authentication Service)
- User preferences/notification settings (-> Configuration Service)
- Real-time WebSocket notifications (-> separate service in future)
- SMS notifications (future feature)
- Storing notification history long-term (-> Audit Service)

## Architecture

The Notification service is a **lightweight API service** without full Clean Architecture layers. It focuses on notification delivery with minimal complexity.

### Key Characteristics:
- **No database** - Stateless notification delivery (future: queue-based)
- **No Clean Architecture layers** - Single API project
- **Template-based** - Simple in-memory templates for common notifications
- **Integration-ready** - Placeholder for SMTP/SendGrid/Firebase integration

## API Endpoints

### Send Email Notification
```
POST /api/v1/notifications/email
```
Request body:
```json
{
  "to": "user@example.com",
  "subject": "Welcome to DC Platform",
  "body": "Welcome! Your account has been created.",
  "templateId": "welcome",
  "templateData": {
    "name": "John Doe"
  }
}
```
Response: `200 OK` with `NotificationResponse`
```json
{
  "id": "guid",
  "status": "Sent",
  "sentAt": "2026-01-29T12:00:00Z"
}
```

### Send Push Notification
```
POST /api/v1/notifications/push
```
Request body:
```json
{
  "userId": "guid",
  "title": "New Message",
  "message": "You have a new message from Admin",
  "data": {
    "messageId": "123",
    "type": "direct"
  }
}
```
Response: `200 OK` with `NotificationResponse`
```json
{
  "id": "guid",
  "status": "Queued",
  "sentAt": "2026-01-29T12:00:00Z"
}
```

### Health Check
```
GET /api/v1/notifications/health
GET /health
```
Response: `200 OK` with service health status

## Email Templates

The service includes simple in-memory templates for common notifications:

| Template ID | Purpose | Placeholders |
|-------------|---------|--------------|
| `welcome` | Welcome new users | `{{name}}` |
| `password-reset` | Password reset emails | `{{name}}`, `{{resetLink}}` |
| `invitation` | Workspace invitations | `{{name}}`, `{{organizationName}}` |

Usage example:
```json
{
  "to": "user@example.com",
  "subject": "Welcome!",
  "templateId": "welcome",
  "templateData": {
    "name": "John Doe"
  }
}
```

The template engine replaces `{{key}}` placeholders with values from `templateData`.

## Project Structure

```
services/notification/
├── src/
│   └── Notification.API/            # ASP.NET Core Web API
│       ├── Controllers/             # NotificationsController
│       ├── Middleware/              # CorrelationId, Exception handling
│       ├── Models/                  # Request/Response DTOs
│       ├── Services/                # INotificationService, NotificationService
│       ├── Properties/              # Launch settings
│       ├── appsettings.json         # Configuration
│       └── Program.cs
│
├── Notification.slnx
├── CLAUDE.md                        # This file
└── README.md
```

## Technical Requirements

### No Database
This service is currently stateless. Future versions may add:
- Message queue (RabbitMQ) for reliable delivery
- Redis for rate limiting and deduplication
- PostgreSQL for notification history (optional)

### Dependencies
- Serilog for structured logging
- ASP.NET Core OpenAPI for documentation

### Future Integrations
- **Email**: SMTP server, SendGrid, or ZeptoMail
- **Push**: Firebase Cloud Messaging (FCM), Apple Push Notification Service (APNS)
- **SMS**: Twilio (future feature)

## Configuration

### Port
- Development: `http://localhost:5005`

### SMTP Settings (Placeholder)
```json
{
  "SmtpSettings": {
    "Host": "smtp.example.com",
    "Port": 587,
    "Username": "",
    "Password": "",
    "FromEmail": "noreply@example.com",
    "FromName": "DC Platform"
  }
}
```

These settings are placeholders. Actual SMTP integration will be implemented when needed.

## Usage by Other Services

Other services can call this API to send notifications:

```csharp
// Example: Send welcome email after user registration
await _notificationClient.SendEmail(new EmailNotificationRequest(
    To: user.Email,
    Subject: "Welcome to DC Platform",
    TemplateId: "welcome",
    TemplateData: new Dictionary<string, string>
    {
        ["name"] = user.Name
    }
));

// Example: Send push notification for new message
await _notificationClient.SendPush(new PushNotificationRequest(
    UserId: recipientUserId,
    Title: "New Message",
    Message: $"You have a new message from {senderName}",
    Data: new Dictionary<string, string>
    {
        ["messageId"] = messageId.ToString(),
        ["senderId"] = senderId.ToString()
    }
));
```

## Coding Rules for This Service

1. **Simple and Focused**: Keep this service lightweight - no heavy architecture
2. **Template-Based**: Use templates for common notification patterns
3. **Async All The Way**: All notification operations are async
4. **Structured Logging**: Log all notification attempts with correlation IDs
5. **Fail Fast**: Return errors immediately for invalid requests
6. **Idempotency**: Design for idempotent notification delivery (future)
7. **No Secrets in Logs**: Never log email content, user data, or credentials

## Logging

This service uses structured JSON logging via Serilog (see `.claude/skills/structured-logging/SKILL.md`).

- **Log output**: Console (structured text) + File (JSON)
- **File path**: `infrastructure/logs/notification/log-{date}.json`
- **Rotation**: Daily, 30-day retention
- **Correlation ID**: All requests tagged via `X-Correlation-Id` header
- **Context enrichment**: RequestMethod, RequestPath, UserId, OrganizationId, WorkspaceId

### What to Log
- Email send attempts (recipient, subject, template)
- Push notification delivery (userId, title)
- Delivery failures and retries
- Template rendering errors

### What NOT to Log
- Email body content (privacy)
- User personal data beyond userId/email
- SMTP credentials
- Authentication tokens

## Commands

```bash
# From services/notification/
dotnet restore
dotnet build
dotnet run --project src/Notification.API

# Run on specific port
dotnet run --project src/Notification.API --urls "http://localhost:5005"
```

## Future Features

1. **Real SMTP Integration**: Connect to SendGrid, ZeptoMail, or SMTP server
2. **Push Notifications**: Firebase Cloud Messaging for web/mobile
3. **Template Management**: Database-backed templates with versioning
4. **Queued Delivery**: RabbitMQ-based message queue for reliable delivery
5. **Retry Logic**: Exponential backoff for failed deliveries
6. **Rate Limiting**: Prevent notification spam
7. **Delivery Tracking**: Track open rates, click rates (for emails)
8. **Multi-language Support**: Locale-based template selection

## Development Notes

- Default port: `5005` (HTTP), `7005` (HTTPS)
- Health endpoint: `http://localhost:5005/api/health`
- No database setup required
- No migrations needed
- Currently returns simulated responses (Status: "Sent" or "Queued")
- Template rendering is basic string replacement (future: use Liquid or Handlebars)
