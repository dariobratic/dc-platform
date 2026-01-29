using Notification.API.Models;

namespace Notification.API.Services;

public interface INotificationService
{
    Task<NotificationResponse> SendEmailAsync(EmailNotificationRequest request);
    Task<NotificationResponse> SendPushAsync(PushNotificationRequest request);
}
