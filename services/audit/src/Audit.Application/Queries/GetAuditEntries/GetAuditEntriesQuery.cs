using Audit.Application.Responses;
using MediatR;

namespace Audit.Application.Queries.GetAuditEntries;

public sealed record GetAuditEntriesQuery(
    Guid? OrganizationId = null,
    Guid? WorkspaceId = null,
    Guid? UserId = null,
    string? EntityType = null,
    string? Action = null,
    string? ServiceName = null,
    DateTime? From = null,
    DateTime? To = null,
    int Skip = 0,
    int Take = 50) : IRequest<PagedResponse<AuditEntryResponse>>;
