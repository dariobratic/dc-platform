using Directory.Domain.Entities;

namespace Directory.Domain.Events;

public sealed record MemberRoleChanged(
    Guid WorkspaceId,
    Guid UserId,
    WorkspaceRole OldRole,
    WorkspaceRole NewRole) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
