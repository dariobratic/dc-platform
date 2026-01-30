namespace AdminApi.API.Models;

public record AuditEntrySummary(
    Guid Id,
    string Action,
    string EntityType,
    Guid? EntityId,
    Guid? UserId,
    string? UserEmail,
    string? ServiceName,
    DateTime Timestamp);
