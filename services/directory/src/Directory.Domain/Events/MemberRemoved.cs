namespace Directory.Domain.Events;

public sealed record MemberRemoved(
    Guid WorkspaceId,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
