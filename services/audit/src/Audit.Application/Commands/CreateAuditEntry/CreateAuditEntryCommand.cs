using Audit.Application.Responses;
using MediatR;

namespace Audit.Application.Commands.CreateAuditEntry;

public sealed record CreateAuditEntryCommand(
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
    string? CorrelationId = null) : IRequest<AuditEntryResponse>;
