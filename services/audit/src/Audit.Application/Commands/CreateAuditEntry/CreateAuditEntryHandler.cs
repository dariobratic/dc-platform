using Audit.Application.Interfaces;
using Audit.Application.Responses;
using Audit.Domain.Entities;
using MediatR;

namespace Audit.Application.Commands.CreateAuditEntry;

public sealed class CreateAuditEntryHandler : IRequestHandler<CreateAuditEntryCommand, AuditEntryResponse>
{
    private readonly IAuditEntryRepository _repository;

    public CreateAuditEntryHandler(IAuditEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task<AuditEntryResponse> Handle(CreateAuditEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = AuditEntry.Create(
            request.UserId,
            request.Action,
            request.EntityType,
            request.EntityId,
            request.ServiceName,
            request.UserEmail,
            request.OrganizationId,
            request.WorkspaceId,
            request.Details,
            request.IpAddress,
            request.UserAgent,
            request.CorrelationId);

        await _repository.AddAsync(entry, cancellationToken);

        return AuditEntryResponse.FromEntity(entry);
    }
}
