using AccessControl.Domain.Enums;

namespace AccessControl.Domain.Events;

public sealed record RoleCreated(
    Guid RoleId,
    string Name,
    Guid ScopeId,
    ScopeType ScopeType) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
