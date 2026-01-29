using AccessControl.Domain.Entities;
using AccessControl.Domain.Enums;
using AccessControl.Domain.Events;
using Xunit;

namespace AccessControl.Domain.Tests.Entities;

public class RoleAssignmentTests
{
    [Fact]
    public void Create_WithValidData_ShouldSetProperties()
    {
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var scopeId = Guid.NewGuid();
        var scopeType = ScopeType.Organization;
        var assignedBy = Guid.NewGuid();

        var assignment = RoleAssignment.Create(roleId, userId, scopeId, scopeType, assignedBy);

        Assert.NotEqual(Guid.Empty, assignment.Id);
        Assert.Equal(roleId, assignment.RoleId);
        Assert.Equal(userId, assignment.UserId);
        Assert.Equal(scopeId, assignment.ScopeId);
        Assert.Equal(scopeType, assignment.ScopeType);
        Assert.Equal(assignedBy, assignment.AssignedBy);
        Assert.True(assignment.AssignedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        var assignment = RoleAssignment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            ScopeType.Workspace,
            Guid.NewGuid());

        Assert.Single(assignment.DomainEvents);
        Assert.IsType<RoleAssignmentCreated>(assignment.DomainEvents.First());
    }

    [Fact]
    public void Revoke_ShouldRaiseDomainEvent()
    {
        var assignment = RoleAssignment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            ScopeType.Workspace,
            Guid.NewGuid());
        assignment.ClearDomainEvents();

        assignment.Revoke();

        Assert.Single(assignment.DomainEvents);
        Assert.IsType<RoleAssignmentRevoked>(assignment.DomainEvents.First());
    }
}
