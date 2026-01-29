using Directory.Domain.Entities;

namespace Directory.Application.Queries.Invitations;

public sealed record InvitationResponse(
    Guid Id,
    Guid WorkspaceId,
    string Email,
    WorkspaceRole Role,
    string Token,
    DateTime ExpiresAt,
    InvitationStatus Status,
    DateTime CreatedAt,
    DateTime? AcceptedAt,
    Guid InvitedBy)
{
    public static InvitationResponse FromEntity(Invitation invitation)
    {
        return new InvitationResponse(
            invitation.Id,
            invitation.WorkspaceId,
            invitation.Email,
            invitation.Role,
            invitation.Token,
            invitation.ExpiresAt,
            invitation.Status,
            invitation.CreatedAt,
            invitation.AcceptedAt,
            invitation.InvitedBy);
    }
}
