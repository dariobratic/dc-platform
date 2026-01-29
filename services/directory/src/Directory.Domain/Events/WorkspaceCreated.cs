namespace Directory.Domain.Events;

public sealed record WorkspaceCreated(
    Guid WorkspaceId,
    Guid OrganizationId,
    string Name,
    string Slug) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
