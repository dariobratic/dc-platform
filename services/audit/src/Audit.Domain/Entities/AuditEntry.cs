using Audit.Domain.Events;
using Audit.Domain.Exceptions;

namespace Audit.Domain.Entities;

public sealed class AuditEntry : BaseEntity
{
    public Guid Id { get; private set; }
    public DateTime Timestamp { get; private set; }
    public Guid UserId { get; private set; }
    public string? UserEmail { get; private set; }
    public string Action { get; private set; }
    public string EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public Guid? OrganizationId { get; private set; }
    public Guid? WorkspaceId { get; private set; }
    public string? Details { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string ServiceName { get; private set; }
    public string? CorrelationId { get; private set; }

    // EF Core requires a parameterless constructor
    private AuditEntry()
    {
        Action = string.Empty;
        EntityType = string.Empty;
        ServiceName = string.Empty;
    }

    public static AuditEntry Create(
        Guid userId,
        string action,
        string entityType,
        Guid entityId,
        string serviceName,
        string? userEmail = null,
        Guid? organizationId = null,
        Guid? workspaceId = null,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? correlationId = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty.");

        if (string.IsNullOrWhiteSpace(action))
            throw new DomainException("Action cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(entityType))
            throw new DomainException("EntityType cannot be null or empty.");

        if (entityId == Guid.Empty)
            throw new DomainException("EntityId cannot be empty.");

        if (string.IsNullOrWhiteSpace(serviceName))
            throw new DomainException("ServiceName cannot be null or empty.");

        var entry = new AuditEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            UserEmail = userEmail,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OrganizationId = organizationId,
            WorkspaceId = workspaceId,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            ServiceName = serviceName,
            CorrelationId = correlationId
        };

        entry.RaiseDomainEvent(new AuditEntryCreated(
            entry.Id,
            entry.Action,
            entry.EntityType,
            entry.EntityId,
            entry.ServiceName));

        return entry;
    }
}
