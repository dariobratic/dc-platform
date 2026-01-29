using Directory.Application.Exceptions;
using Directory.Application.Interfaces;
using Directory.Application.Queries.Memberships;
using Directory.Domain.Entities;
using MediatR;

namespace Directory.Application.Commands.Memberships;

public sealed record ChangeMemberRoleCommand(
    Guid WorkspaceId,
    Guid UserId,
    WorkspaceRole Role) : IRequest<MembershipResponse>;

public sealed class ChangeMemberRoleHandler : IRequestHandler<ChangeMemberRoleCommand, MembershipResponse>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IMembershipRepository _membershipRepository;

    public ChangeMemberRoleHandler(
        IWorkspaceRepository workspaceRepository,
        IMembershipRepository membershipRepository)
    {
        _workspaceRepository = workspaceRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<MembershipResponse> Handle(ChangeMemberRoleCommand request, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.GetByIdWithMembershipsAsync(request.WorkspaceId, cancellationToken)
            ?? throw new NotFoundException("Workspace", request.WorkspaceId);

        workspace.ChangeMemberRole(request.UserId, request.Role);

        await _workspaceRepository.UpdateAsync(workspace, cancellationToken);

        var membership = await _membershipRepository.GetByWorkspaceAndUserAsync(request.WorkspaceId, request.UserId, cancellationToken)
            ?? throw new NotFoundException("Membership", $"WorkspaceId: {request.WorkspaceId}, UserId: {request.UserId}");

        return MembershipResponse.FromEntity(membership);
    }
}
