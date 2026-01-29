using Directory.Application.Exceptions;
using Directory.Application.Interfaces;
using MediatR;

namespace Directory.Application.Commands.Memberships;

public sealed record RemoveMemberCommand(
    Guid WorkspaceId,
    Guid UserId) : IRequest;

public sealed class RemoveMemberHandler : IRequestHandler<RemoveMemberCommand>
{
    private readonly IWorkspaceRepository _workspaceRepository;

    public RemoveMemberHandler(IWorkspaceRepository workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
    }

    public async Task Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.GetByIdWithMembershipsAsync(request.WorkspaceId, cancellationToken)
            ?? throw new NotFoundException("Workspace", request.WorkspaceId);

        workspace.RemoveMember(request.UserId);

        await _workspaceRepository.UpdateAsync(workspace, cancellationToken);
    }
}
