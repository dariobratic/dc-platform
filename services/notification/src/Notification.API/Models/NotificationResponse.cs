namespace Notification.API.Models;

public record NotificationResponse(
    Guid Id,
    string Status,
    DateTime SentAt
);
