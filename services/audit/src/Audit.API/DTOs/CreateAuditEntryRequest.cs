namespace Audit.API.DTOs;

public sealed record CreateAuditEntryRequest(
    Guid UserId,
    string Action,
    string EntityType,
    Guid EntityId,
    string ServiceName,
    string? UserEmail = null,
    Guid? OrganizationId = null,
    Guid? WorkspaceId = null,
    string? Details = null,
    string? IpAddress = null,
    string? UserAgent = null,
    string? CorrelationId = null);
