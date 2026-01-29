using Audit.Application.Responses;
using MediatR;

namespace Audit.Application.Queries.GetEntityAuditHistory;

public sealed record GetEntityAuditHistoryQuery(
    string EntityType,
    Guid EntityId) : IRequest<List<AuditEntryResponse>>;
