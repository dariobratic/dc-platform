using Directory.Domain.Entities;

namespace Directory.Domain.Events;

public sealed record InvitationCreated(
    Guid InvitationId,
    Guid WorkspaceId,
    string Email,
    WorkspaceRole Role,
    Guid InvitedBy) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
