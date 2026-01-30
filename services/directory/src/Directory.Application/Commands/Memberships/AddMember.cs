using Directory.Application.Exceptions;
using Directory.Application.Interfaces;
using Directory.Application.Queries.Memberships;
using Directory.Domain.Entities;
using MediatR;

namespace Directory.Application.Commands.Memberships;

public sealed record AddMemberCommand(
    Guid WorkspaceId,
    Guid UserId,
    WorkspaceRole Role,
    Guid? InvitedBy = null) : IRequest<MembershipResponse>;

public sealed class AddMemberHandler : IRequestHandler<AddMemberCommand, MembershipResponse>
{
    private readonly IWorkspaceRepository _workspaceRepository;

    public AddMemberHandler(IWorkspaceRepository workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
    }

    public async Task<MembershipResponse> Handle(AddMemberCommand request, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.GetByIdWithMembershipsAsync(request.WorkspaceId, cancellationToken)
            ?? throw new NotFoundException("Workspace", request.WorkspaceId);

        if (workspace.Memberships.Any(m => m.UserId == request.UserId))
            throw new ConflictException("Membership", "UserId", request.UserId);

        var membership = workspace.AddMember(request.UserId, request.Role, request.InvitedBy);

        await _workspaceRepository.UpdateAsync(workspace, cancellationToken);

        return MembershipResponse.FromEntity(membership);
    }
}
