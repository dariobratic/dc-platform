using Directory.Application.Exceptions;
using Directory.Application.Interfaces;
using Directory.Application.Queries.Organizations;
using Directory.Domain.ValueObjects;
using MediatR;

namespace Directory.Application.Commands.Organizations;

public sealed record UpdateOrganizationCommand(
    Guid Id,
    string Name,
    Dictionary<string, string>? Settings) : IRequest<OrganizationResponse>;

public sealed class UpdateOrganizationHandler : IRequestHandler<UpdateOrganizationCommand, OrganizationResponse>
{
    private readonly IOrganizationRepository _repository;

    public UpdateOrganizationHandler(IOrganizationRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrganizationResponse> Handle(UpdateOrganizationCommand request, CancellationToken cancellationToken)
    {
        var organization = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Organization", request.Id);

        var settings = request.Settings is not null
            ? OrganizationSettings.Create(request.Settings)
            : null;

        organization.Update(request.Name, settings);

        await _repository.UpdateAsync(organization, cancellationToken);

        return OrganizationResponse.FromEntity(organization);
    }
}
