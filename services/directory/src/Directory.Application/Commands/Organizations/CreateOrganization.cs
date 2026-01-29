using Directory.Application.Exceptions;
using Directory.Application.Interfaces;
using Directory.Application.Queries.Organizations;
using Directory.Domain.Entities;
using Directory.Domain.ValueObjects;
using MediatR;

namespace Directory.Application.Commands.Organizations;

public sealed record CreateOrganizationCommand(
    string Name,
    string Slug,
    Dictionary<string, string>? Settings) : IRequest<OrganizationResponse>;

public sealed class CreateOrganizationHandler : IRequestHandler<CreateOrganizationCommand, OrganizationResponse>
{
    private readonly IOrganizationRepository _repository;

    public CreateOrganizationHandler(IOrganizationRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrganizationResponse> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        var slug = Slug.Create(request.Slug);

        if (await _repository.SlugExistsAsync(slug.Value, cancellationToken))
            throw new ConflictException("Organization", "Slug", slug.Value);

        var settings = request.Settings is not null
            ? OrganizationSettings.Create(request.Settings)
            : null;

        var organization = Organization.Create(request.Name, slug, settings);

        await _repository.AddAsync(organization, cancellationToken);

        return OrganizationResponse.FromEntity(organization);
    }
}
