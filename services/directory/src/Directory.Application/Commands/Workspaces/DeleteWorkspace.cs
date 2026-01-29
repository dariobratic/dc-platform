using Directory.Application.Exceptions;
using Directory.Application.Interfaces;
using MediatR;

namespace Directory.Application.Commands.Workspaces;

public sealed record DeleteWorkspaceCommand(Guid Id) : IRequest;

public sealed class DeleteWorkspaceHandler : IRequestHandler<DeleteWorkspaceCommand>
{
    private readonly IWorkspaceRepository _repository;

    public DeleteWorkspaceHandler(IWorkspaceRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(DeleteWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var workspace = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Workspace", request.Id);

        workspace.Delete();

        await _repository.UpdateAsync(workspace, cancellationToken);
    }
}
