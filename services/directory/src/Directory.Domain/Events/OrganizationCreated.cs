namespace Directory.Domain.Events;

public sealed record OrganizationCreated(
    Guid OrganizationId,
    string Name,
    string Slug) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
