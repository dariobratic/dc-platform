using Directory.Application.Exceptions;
using Directory.Application.Interfaces;
using MediatR;

namespace Directory.Application.Queries.Workspaces;

public sealed record GetWorkspaceByIdQuery(Guid Id) : IRequest<WorkspaceResponse>;

public sealed class GetWorkspaceByIdHandler : IRequestHandler<GetWorkspaceByIdQuery, WorkspaceResponse>
{
    private readonly IWorkspaceRepository _repository;

    public GetWorkspaceByIdHandler(IWorkspaceRepository repository)
    {
        _repository = repository;
    }

    public async Task<WorkspaceResponse> Handle(GetWorkspaceByIdQuery request, CancellationToken cancellationToken)
    {
        var workspace = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Workspace", request.Id);

        return WorkspaceResponse.FromEntity(workspace);
    }
}
