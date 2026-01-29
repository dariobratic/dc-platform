using Audit.Application.Exceptions;
using Audit.Application.Interfaces;
using Audit.Application.Responses;
using MediatR;

namespace Audit.Application.Queries.GetAuditEntryById;

public sealed class GetAuditEntryByIdHandler : IRequestHandler<GetAuditEntryByIdQuery, AuditEntryResponse>
{
    private readonly IAuditEntryRepository _repository;

    public GetAuditEntryByIdHandler(IAuditEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task<AuditEntryResponse> Handle(GetAuditEntryByIdQuery request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entry is null)
            throw new NotFoundException($"Audit entry with ID {request.Id} not found.");

        return AuditEntryResponse.FromEntity(entry);
    }
}
