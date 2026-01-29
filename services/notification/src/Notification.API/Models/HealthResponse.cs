namespace Notification.API.Models;

public record HealthResponse(
    string ServiceName,
    string Status,
    DateTime Timestamp
);
