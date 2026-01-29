namespace Directory.Domain.Events;

public sealed record OrganizationDeleted(
    Guid OrganizationId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
