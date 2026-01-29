using Directory.Application.Exceptions;
using Directory.Application.Interfaces;
using Directory.Application.Queries.Invitations;
using Directory.Domain.Entities;
using MediatR;

namespace Directory.Application.Commands.Invitations;

public sealed record CreateInvitationCommand(
    Guid WorkspaceId,
    string Email,
    WorkspaceRole Role,
    Guid InvitedBy) : IRequest<InvitationResponse>;

public sealed class CreateInvitationHandler : IRequestHandler<CreateInvitationCommand, InvitationResponse>
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IWorkspaceRepository _workspaceRepository;

    public CreateInvitationHandler(
        IInvitationRepository invitationRepository,
        IWorkspaceRepository workspaceRepository)
    {
        _invitationRepository = invitationRepository;
        _workspaceRepository = workspaceRepository;
    }

    public async Task<InvitationResponse> Handle(CreateInvitationCommand request, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(request.WorkspaceId, cancellationToken)
            ?? throw new NotFoundException("Workspace", request.WorkspaceId);

        var invitation = Invitation.Create(
            request.WorkspaceId,
            request.Email,
            request.Role,
            request.InvitedBy);

        await _invitationRepository.AddAsync(invitation, cancellationToken);

        return InvitationResponse.FromEntity(invitation);
    }
}
