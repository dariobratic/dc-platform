using Audit.Application.Interfaces;
using Audit.Application.Responses;
using MediatR;

namespace Audit.Application.Queries.GetEntityAuditHistory;

public sealed class GetEntityAuditHistoryHandler : IRequestHandler<GetEntityAuditHistoryQuery, List<AuditEntryResponse>>
{
    private readonly IAuditEntryRepository _repository;

    public GetEntityAuditHistoryHandler(IAuditEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AuditEntryResponse>> Handle(GetEntityAuditHistoryQuery request, CancellationToken cancellationToken)
    {
        var entries = await _repository.GetByEntityAsync(
            request.EntityType,
            request.EntityId,
            cancellationToken);

        return entries.Select(AuditEntryResponse.FromEntity).ToList();
    }
}
