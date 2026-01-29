using Audit.Application.Interfaces;
using Audit.Application.Responses;
using MediatR;

namespace Audit.Application.Queries.GetAuditEntries;

public sealed class GetAuditEntriesHandler : IRequestHandler<GetAuditEntriesQuery, PagedResponse<AuditEntryResponse>>
{
    private readonly IAuditEntryRepository _repository;

    public GetAuditEntriesHandler(IAuditEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResponse<AuditEntryResponse>> Handle(GetAuditEntriesQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            request.OrganizationId,
            request.WorkspaceId,
            request.UserId,
            request.EntityType,
            request.Action,
            request.ServiceName,
            request.From,
            request.To,
            request.Skip,
            request.Take,
            cancellationToken);

        var responses = items.Select(AuditEntryResponse.FromEntity).ToList();

        return new PagedResponse<AuditEntryResponse>(
            responses,
            totalCount,
            request.Skip,
            request.Take);
    }
}
