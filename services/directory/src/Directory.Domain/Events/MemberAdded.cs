using Directory.Domain.Entities;

namespace Directory.Domain.Events;

public sealed record MemberAdded(
    Guid WorkspaceId,
    Guid UserId,
    WorkspaceRole Role) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
