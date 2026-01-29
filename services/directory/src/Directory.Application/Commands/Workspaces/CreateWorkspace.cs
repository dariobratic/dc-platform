using Directory.Application.Exceptions;
using Directory.Application.Interfaces;
using Directory.Application.Queries.Workspaces;
using Directory.Domain.Entities;
using Directory.Domain.ValueObjects;
using MediatR;

namespace Directory.Application.Commands.Workspaces;

public sealed record CreateWorkspaceCommand(
    Guid OrganizationId,
    string Name,
    string Slug) : IRequest<WorkspaceResponse>;

public sealed class CreateWorkspaceHandler : IRequestHandler<CreateWorkspaceCommand, WorkspaceResponse>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IOrganizationRepository _organizationRepository;

    public CreateWorkspaceHandler(
        IWorkspaceRepository workspaceRepository,
        IOrganizationRepository organizationRepository)
    {
        _workspaceRepository = workspaceRepository;
        _organizationRepository = organizationRepository;
    }

    public async Task<WorkspaceResponse> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException("Organization", request.OrganizationId);

        var slug = Slug.Create(request.Slug);

        if (await _workspaceRepository.SlugExistsInOrganizationAsync(request.OrganizationId, slug.Value, cancellationToken))
            throw new ConflictException("Workspace", "Slug", slug.Value);

        var workspace = Workspace.Create(request.OrganizationId, request.Name, slug);

        await _workspaceRepository.AddAsync(workspace, cancellationToken);

        return WorkspaceResponse.FromEntity(workspace);
    }
}
