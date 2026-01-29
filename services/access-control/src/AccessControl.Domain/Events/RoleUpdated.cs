namespace AccessControl.Domain.Events;

public sealed record RoleUpdated(
    Guid RoleId,
    string Name) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
