using Audit.Application.Responses;
using MediatR;

namespace Audit.Application.Queries.GetAuditEntryById;

public sealed record GetAuditEntryByIdQuery(Guid Id) : IRequest<AuditEntryResponse>;
