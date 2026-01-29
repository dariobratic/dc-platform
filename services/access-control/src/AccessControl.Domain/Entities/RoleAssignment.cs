using AccessControl.Domain.Enums;
using AccessControl.Domain.Events;

namespace AccessControl.Domain.Entities;

public class RoleAssignment : BaseEntity
{
    public Guid Id { get; private set; }
    public Guid RoleId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ScopeId { get; private set; }
    public ScopeType ScopeType { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public Guid AssignedBy { get; private set; }

    public Role Role { get; private set; } = default!;

    private RoleAssignment()
    {
    }

    public static RoleAssignment Create(Guid roleId, Guid userId, Guid scopeId, ScopeType scopeType, Guid assignedBy)
    {
        var assignment = new RoleAssignment
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            UserId = userId,
            ScopeId = scopeId,
            ScopeType = scopeType,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = assignedBy
        };

        assignment.RaiseDomainEvent(new RoleAssignmentCreated(
            assignment.Id,
            assignment.RoleId,
            assignment.UserId,
            assignment.ScopeId,
            assignment.ScopeType));

        return assignment;
    }

    public void Revoke()
    {
        RaiseDomainEvent(new RoleAssignmentRevoked(Id, RoleId, UserId));
    }
}
