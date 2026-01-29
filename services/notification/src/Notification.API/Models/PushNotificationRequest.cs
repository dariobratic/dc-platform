namespace Notification.API.Models;

public record PushNotificationRequest(
    Guid UserId,
    string Title,
    string Message,
    Dictionary<string, string>? Data = null
);
