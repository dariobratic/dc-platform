namespace Audit.Domain.Events;

public sealed record AuditEntryCreated(
    Guid AuditEntryId,
    string Action,
    string EntityType,
    Guid EntityId,
    string ServiceName) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
