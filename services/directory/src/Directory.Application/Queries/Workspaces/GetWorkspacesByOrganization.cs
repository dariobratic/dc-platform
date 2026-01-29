using Directory.Application.Interfaces;
using MediatR;

namespace Directory.Application.Queries.Workspaces;

public sealed record GetWorkspacesByOrganizationQuery(Guid OrganizationId) : IRequest<IReadOnlyList<WorkspaceResponse>>;

public sealed class GetWorkspacesByOrganizationHandler : IRequestHandler<GetWorkspacesByOrganizationQuery, IReadOnlyList<WorkspaceResponse>>
{
    private readonly IWorkspaceRepository _repository;

    public GetWorkspacesByOrganizationHandler(IWorkspaceRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<WorkspaceResponse>> Handle(GetWorkspacesByOrganizationQuery request, CancellationToken cancellationToken)
    {
        var workspaces = await _repository.GetByOrganizationIdAsync(request.OrganizationId, cancellationToken);

        return workspaces.Select(WorkspaceResponse.FromEntity).ToList();
    }
}
