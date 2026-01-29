namespace Directory.Domain.Events;

public sealed record WorkspaceDeleted(
    Guid WorkspaceId,
    Guid OrganizationId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
