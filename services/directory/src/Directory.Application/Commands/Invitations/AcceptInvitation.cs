using Directory.Application.Exceptions;
using Directory.Application.Interfaces;
using Directory.Application.Queries.Memberships;
using MediatR;

namespace Directory.Application.Commands.Invitations;

public sealed record AcceptInvitationCommand(
    string Token,
    Guid UserId) : IRequest<MembershipResponse>;

public sealed class AcceptInvitationHandler : IRequestHandler<AcceptInvitationCommand, MembershipResponse>
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IWorkspaceRepository _workspaceRepository;

    public AcceptInvitationHandler(
        IInvitationRepository invitationRepository,
        IWorkspaceRepository workspaceRepository)
    {
        _invitationRepository = invitationRepository;
        _workspaceRepository = workspaceRepository;
    }

    public async Task<MembershipResponse> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _invitationRepository.GetByTokenAsync(request.Token, cancellationToken)
            ?? throw new NotFoundException("Invitation", request.Token);

        invitation.Accept();

        var workspace = await _workspaceRepository.GetByIdWithMembershipsAsync(invitation.WorkspaceId, cancellationToken)
            ?? throw new NotFoundException("Workspace", invitation.WorkspaceId);

        var membership = workspace.AddMember(request.UserId, invitation.Role, invitation.InvitedBy);

        await _invitationRepository.UpdateAsync(invitation, cancellationToken);
        await _workspaceRepository.UpdateAsync(workspace, cancellationToken);

        return MembershipResponse.FromEntity(membership);
    }
}
