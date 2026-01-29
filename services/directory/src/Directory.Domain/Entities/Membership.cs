using Directory.Domain.Exceptions;

namespace Directory.Domain.Entities;

public class Membership
{
    public Guid Id { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public Guid UserId { get; private set; }
    public WorkspaceRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public Guid? InvitedBy { get; private set; }

    public Workspace Workspace { get; private set; } = null!;

    private Membership() { }

    internal static Membership Create(Guid workspaceId, Guid userId, WorkspaceRole role, Guid? invitedBy = null)
    {
        if (workspaceId == Guid.Empty)
            throw new DomainException("Workspace ID is required.");

        if (userId == Guid.Empty)
            throw new DomainException("User ID is required.");

        return new Membership
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            UserId = userId,
            Role = role,
            JoinedAt = DateTime.UtcNow,
            InvitedBy = invitedBy
        };
    }

    internal void ChangeRole(WorkspaceRole newRole)
    {
        Role = newRole;
    }
}
