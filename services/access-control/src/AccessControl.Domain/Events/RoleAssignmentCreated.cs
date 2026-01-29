using AccessControl.Domain.Enums;

namespace AccessControl.Domain.Events;

public sealed record RoleAssignmentCreated(
    Guid RoleAssignmentId,
    Guid RoleId,
    Guid UserId,
    Guid ScopeId,
    ScopeType ScopeType) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
