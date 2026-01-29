using Directory.Domain.Entities;

namespace Directory.Application.Queries.Memberships;

public sealed record MembershipResponse(
    Guid Id,
    Guid WorkspaceId,
    Guid UserId,
    WorkspaceRole Role,
    DateTime JoinedAt,
    Guid? InvitedBy)
{
    public static MembershipResponse FromEntity(Membership membership)
    {
        return new MembershipResponse(
            membership.Id,
            membership.WorkspaceId,
            membership.UserId,
            membership.Role,
            membership.JoinedAt,
            membership.InvitedBy);
    }
}
