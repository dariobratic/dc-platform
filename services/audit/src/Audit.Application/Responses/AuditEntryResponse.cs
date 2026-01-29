using Audit.Domain.Entities;

namespace Audit.Application.Responses;

public sealed record AuditEntryResponse(
    Guid Id,
    DateTime Timestamp,
    Guid UserId,
    string? UserEmail,
    string Action,
    string EntityType,
    Guid EntityId,
    Guid? OrganizationId,
    Guid? WorkspaceId,
    string? Details,
    string? IpAddress,
    string? UserAgent,
    string ServiceName,
    string? CorrelationId)
{
    public static AuditEntryResponse FromEntity(AuditEntry entry)
    {
        return new AuditEntryResponse(
            entry.Id,
            entry.Timestamp,
            entry.UserId,
            entry.UserEmail,
            entry.Action,
            entry.EntityType,
            entry.EntityId,
            entry.OrganizationId,
            entry.WorkspaceId,
            entry.Details,
            entry.IpAddress,
            entry.UserAgent,
            entry.ServiceName,
            entry.CorrelationId);
    }
}
