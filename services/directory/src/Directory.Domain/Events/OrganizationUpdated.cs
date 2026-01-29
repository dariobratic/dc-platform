namespace Directory.Domain.Events;

public sealed record OrganizationUpdated(
    Guid OrganizationId,
    string Name) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
