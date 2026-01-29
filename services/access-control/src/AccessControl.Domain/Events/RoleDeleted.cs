namespace AccessControl.Domain.Events;

public sealed record RoleDeleted(Guid RoleId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
