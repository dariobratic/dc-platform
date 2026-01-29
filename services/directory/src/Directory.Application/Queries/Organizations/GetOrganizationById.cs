using Directory.Application.Exceptions;
using Directory.Application.Interfaces;
using MediatR;

namespace Directory.Application.Queries.Organizations;

public sealed record GetOrganizationByIdQuery(Guid Id) : IRequest<OrganizationResponse>;

public sealed class GetOrganizationByIdHandler : IRequestHandler<GetOrganizationByIdQuery, OrganizationResponse>
{
    private readonly IOrganizationRepository _repository;

    public GetOrganizationByIdHandler(IOrganizationRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrganizationResponse> Handle(GetOrganizationByIdQuery request, CancellationToken cancellationToken)
    {
        var organization = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Organization", request.Id);

        return OrganizationResponse.FromEntity(organization);
    }
}
