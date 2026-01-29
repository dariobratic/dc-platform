using Directory.Application.Exceptions;
using Directory.Application.Interfaces;
using MediatR;

namespace Directory.Application.Commands.Organizations;

public sealed record DeleteOrganizationCommand(Guid Id) : IRequest;

public sealed class DeleteOrganizationHandler : IRequestHandler<DeleteOrganizationCommand>
{
    private readonly IOrganizationRepository _repository;

    public DeleteOrganizationHandler(IOrganizationRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(DeleteOrganizationCommand request, CancellationToken cancellationToken)
    {
        var organization = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Organization", request.Id);

        organization.Delete();

        await _repository.UpdateAsync(organization, cancellationToken);
    }
}
