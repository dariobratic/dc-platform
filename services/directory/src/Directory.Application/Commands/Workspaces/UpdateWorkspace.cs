using Directory.Application.Exceptions;
using Directory.Application.Interfaces;
using Directory.Application.Queries.Workspaces;
using MediatR;

namespace Directory.Application.Commands.Workspaces;

public sealed record UpdateWorkspaceCommand(
    Guid Id,
    string Name) : IRequest<WorkspaceResponse>;

public sealed class UpdateWorkspaceHandler : IRequestHandler<UpdateWorkspaceCommand, WorkspaceResponse>
{
    private readonly IWorkspaceRepository _repository;

    public UpdateWorkspaceHandler(IWorkspaceRepository repository)
    {
        _repository = repository;
    }

    public async Task<WorkspaceResponse> Handle(UpdateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var workspace = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Workspace", request.Id);

        workspace.Update(request.Name);

        await _repository.UpdateAsync(workspace, cancellationToken);

        return WorkspaceResponse.FromEntity(workspace);
    }
}
