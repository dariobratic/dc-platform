using Notification.API.Models;

namespace Notification.API.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly Dictionary<string, string> _emailTemplates;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;

        // Simple in-memory email templates
        _emailTemplates = new Dictionary<string, string>
        {
            ["welcome"] = "Welcome {{name}}! We're glad to have you on board.",
            ["password-reset"] = "Hello {{name}}, click here to reset your password: {{resetLink}}",
            ["invitation"] = "Hi {{name}}, you've been invited to join {{organizationName}}."
        };
    }

    public async Task<NotificationResponse> SendEmailAsync(EmailNotificationRequest request)
    {
        var notificationId = Guid.NewGuid();

        var body = request.Body;

        // If template is specified, apply template with data
        if (!string.IsNullOrEmpty(request.TemplateId) && _emailTemplates.TryGetValue(request.TemplateId, out var template))
        {
            body = template;
            if (request.TemplateData != null)
            {
                foreach (var (key, value) in request.TemplateData)
                {
                    body = body.Replace($"{{{{{key}}}}}", value);
                }
            }

            _logger.LogInformation(
                "Email notification queued with template {TemplateId}: {NotificationId} to {To} - {Subject}",
                request.TemplateId,
                notificationId,
                request.To,
                request.Subject
            );
        }
        else
        {
            _logger.LogInformation(
                "Email notification queued: {NotificationId} to {To} - {Subject}",
                notificationId,
                request.To,
                request.Subject
            );
        }

        // Simulate async send operation
        await Task.Delay(10);

        return new NotificationResponse(
            Id: notificationId,
            Status: "Sent",
            SentAt: DateTime.UtcNow
        );
    }

    public async Task<NotificationResponse> SendPushAsync(PushNotificationRequest request)
    {
        var notificationId = Guid.NewGuid();

        _logger.LogInformation(
            "Push notification queued: {NotificationId} for user {UserId} - {Title}",
            notificationId,
            request.UserId,
            request.Title
        );

        if (request.Data != null && request.Data.Any())
        {
            _logger.LogDebug(
                "Push notification {NotificationId} includes {DataCount} custom data fields",
                notificationId,
                request.Data.Count
            );
        }

        // Simulate async send operation
        await Task.Delay(10);

        // For now, return Queued status as push integration is placeholder
        return new NotificationResponse(
            Id: notificationId,
            Status: "Queued",
            SentAt: DateTime.UtcNow
        );
    }
}
