namespace Directory.Domain.Events;

public sealed record WorkspaceUpdated(
    Guid WorkspaceId,
    string Name) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
