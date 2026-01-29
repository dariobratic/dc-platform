# Notification Service

Notification delivery service for DC Platform. Handles email and push notifications.

## Overview

The Notification service provides a centralized API for sending email and push notifications across the DC Platform. It supports template-based notifications and structured logging for delivery tracking.

**Key Characteristics:**
- Stateless (no database)
- Template-based notifications
- Simple in-memory template engine
- Placeholder for future SMTP/push integrations

## Quick Start

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run --project src/Notification.API

# Or run on specific port
dotnet run --project src/Notification.API --urls "http://localhost:5005"
```

The service will start on `http://localhost:5005` (HTTP) or `https://localhost:7005` (HTTPS).

## API Endpoints

### Send Email
```bash
curl -X POST http://localhost:5005/api/v1/notifications/email \
  -H "Content-Type: application/json" \
  -d '{
    "to": "user@example.com",
    "subject": "Welcome!",
    "templateId": "welcome",
    "templateData": {
      "name": "John Doe"
    }
  }'
```

Response:
```json
{
  "id": "guid",
  "status": "Sent",
  "sentAt": "2026-01-29T12:00:00Z"
}
```

### Send Push Notification
```bash
curl -X POST http://localhost:5005/api/v1/notifications/push \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
    "title": "New Message",
    "message": "You have a new message",
    "data": {
      "messageId": "123"
    }
  }'
```

Response:
```json
{
  "id": "guid",
  "status": "Queued",
  "sentAt": "2026-01-29T12:00:00Z"
}
```

### Health Check
```bash
curl http://localhost:5005/api/v1/notifications/health
```

Response:
```json
{
  "serviceName": "Notification",
  "status": "Healthy",
  "timestamp": "2026-01-29T12:00:00Z"
}
```

## Email Templates

Built-in templates:

| Template ID | Description | Placeholders |
|-------------|-------------|--------------|
| `welcome` | Welcome new users | `{{name}}` |
| `password-reset` | Password reset link | `{{name}}`, `{{resetLink}}` |
| `invitation` | Workspace invitation | `{{name}}`, `{{organizationName}}` |

Example with template:
```json
{
  "to": "user@example.com",
  "subject": "Reset Your Password",
  "templateId": "password-reset",
  "templateData": {
    "name": "Jane Doe",
    "resetLink": "https://app.example.com/reset?token=abc123"
  }
}
```

Example without template (custom body):
```json
{
  "to": "user@example.com",
  "subject": "Custom Notification",
  "body": "This is a custom email body."
}
```

## Configuration

### SMTP Settings (Placeholder)

Edit `appsettings.json`:
```json
{
  "SmtpSettings": {
    "Host": "smtp.example.com",
    "Port": 587,
    "Username": "your-username",
    "Password": "your-password",
    "FromEmail": "noreply@example.com",
    "FromName": "DC Platform"
  }
}
```

**Note**: SMTP integration is not yet implemented. The service currently simulates email delivery.

## Project Structure

```
services/notification/
├── src/
│   └── Notification.API/            # Main API project
│       ├── Controllers/             # API endpoints
│       ├── Middleware/              # Correlation ID, exception handling
│       ├── Models/                  # Request/Response DTOs
│       ├── Services/                # Notification service
│       ├── Properties/              # Launch settings
│       ├── appsettings.json
│       └── Program.cs
├── Notification.slnx                # Solution file
├── CLAUDE.md                        # Developer guide
└── README.md                        # This file
```

## Development

This is a lightweight service:
- No database or EF Core
- No Clean Architecture layers
- No message queue (yet)
- Simple template engine with `{{placeholder}}` syntax

For detailed development guidelines, see [CLAUDE.md](./CLAUDE.md).

## Future Features

- Real SMTP integration (SendGrid/ZeptoMail)
- Firebase Cloud Messaging for push notifications
- Database-backed template management
- Message queue for reliable delivery
- Retry logic with exponential backoff
- Rate limiting and deduplication

## Related Services

- **Gateway** - API entry point (port 5000)
- **Authentication** - User auth (port 5002)
- **Configuration** - Notification preferences (port 5006)
- **Audit** - Notification delivery audit trail (port 5004)

## Support

For questions or issues, refer to the main platform documentation at `docs/`.
