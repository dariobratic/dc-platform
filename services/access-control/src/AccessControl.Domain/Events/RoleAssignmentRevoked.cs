namespace AccessControl.Domain.Events;

public sealed record RoleAssignmentRevoked(
    Guid RoleAssignmentId,
    Guid RoleId,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
