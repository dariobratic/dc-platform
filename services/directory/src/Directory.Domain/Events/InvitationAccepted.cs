namespace Directory.Domain.Events;

public sealed record InvitationAccepted(
    Guid InvitationId,
    Guid WorkspaceId,
    string Email) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
