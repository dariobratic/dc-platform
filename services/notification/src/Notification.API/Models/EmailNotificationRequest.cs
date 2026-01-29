namespace Notification.API.Models;

public record EmailNotificationRequest(
    string To,
    string Subject,
    string Body,
    string? TemplateId = null,
    Dictionary<string, string>? TemplateData = null
);
