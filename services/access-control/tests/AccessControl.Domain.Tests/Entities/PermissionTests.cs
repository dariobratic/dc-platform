using AccessControl.Domain.Entities;
using AccessControl.Domain.Exceptions;
using Xunit;

namespace AccessControl.Domain.Tests.Entities;

public class PermissionTests
{
    [Fact]
    public void Create_WithValidData_ShouldSetProperties()
    {
        var roleId = Guid.NewGuid();
        var action = "document:read";

        var permission = Permission.Create(roleId, action);

        Assert.NotEqual(Guid.Empty, permission.Id);
        Assert.Equal(roleId, permission.RoleId);
        Assert.Equal(action, permission.Action);
        Assert.True(permission.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Create_WithEmptyAction_ShouldThrow()
    {
        var roleId = Guid.NewGuid();

        Assert.Throws<DomainException>(() => Permission.Create(roleId, ""));
        Assert.Throws<DomainException>(() => Permission.Create(roleId, "   "));
    }
}
