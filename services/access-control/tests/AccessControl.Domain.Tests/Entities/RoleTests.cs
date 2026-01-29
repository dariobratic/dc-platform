using AccessControl.Domain.Entities;
using AccessControl.Domain.Enums;
using AccessControl.Domain.Events;
using AccessControl.Domain.Exceptions;
using Xunit;

namespace AccessControl.Domain.Tests.Entities;

public class RoleTests
{
    [Fact]
    public void Create_WithValidData_ShouldSetProperties()
    {
        var name = "Admin";
        var description = "Administrator role";
        var scopeId = Guid.NewGuid();
        var scopeType = ScopeType.Organization;

        var role = Role.Create(name, description, scopeId, scopeType);

        Assert.NotEqual(Guid.Empty, role.Id);
        Assert.Equal(name, role.Name);
        Assert.Equal(description, role.Description);
        Assert.Equal(scopeId, role.ScopeId);
        Assert.Equal(scopeType, role.ScopeType);
        Assert.False(role.IsSystem);
        Assert.True(role.CreatedAt <= DateTime.UtcNow);
        Assert.Null(role.UpdatedAt);
        Assert.Empty(role.Permissions);
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        var role = Role.Create("Viewer", "Viewer role", Guid.NewGuid(), ScopeType.Workspace);

        Assert.Single(role.DomainEvents);
        Assert.IsType<RoleCreated>(role.DomainEvents.First());
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateProperties()
    {
        var role = Role.Create("Old Name", "Old description", Guid.NewGuid(), ScopeType.Organization);
        role.ClearDomainEvents();

        role.Update("New Name", "New description");

        Assert.Equal("New Name", role.Name);
        Assert.Equal("New description", role.Description);
        Assert.NotNull(role.UpdatedAt);
    }

    [Fact]
    public void Update_ShouldRaiseDomainEvent()
    {
        var role = Role.Create("Admin", null, Guid.NewGuid(), ScopeType.Organization);
        role.ClearDomainEvents();

        role.Update("Super Admin", "Updated role");

        Assert.Single(role.DomainEvents);
        Assert.IsType<RoleUpdated>(role.DomainEvents.First());
    }

    [Fact]
    public void Update_SystemRole_ShouldThrow()
    {
        var role = Role.Create("System Role", null, Guid.NewGuid(), ScopeType.Organization);
        typeof(Role).GetProperty("IsSystem")!.SetValue(role, true);

        Assert.Throws<DomainException>(() => role.Update("New Name", null));
    }

    [Fact]
    public void Delete_ShouldRaiseDomainEvent()
    {
        var role = Role.Create("Test Role", null, Guid.NewGuid(), ScopeType.Workspace);
        role.ClearDomainEvents();

        role.Delete();

        Assert.Single(role.DomainEvents);
        Assert.IsType<RoleDeleted>(role.DomainEvents.First());
    }

    [Fact]
    public void Delete_SystemRole_ShouldThrow()
    {
        var role = Role.Create("System Role", null, Guid.NewGuid(), ScopeType.Organization);
        typeof(Role).GetProperty("IsSystem")!.SetValue(role, true);

        Assert.Throws<DomainException>(() => role.Delete());
    }

    [Fact]
    public void AddPermission_ShouldAddToCollection()
    {
        var role = Role.Create("Editor", null, Guid.NewGuid(), ScopeType.Workspace);

        role.AddPermission("document:read");
        role.AddPermission("document:write");

        Assert.Equal(2, role.Permissions.Count);
        Assert.Contains(role.Permissions, p => p.Action == "document:read");
        Assert.Contains(role.Permissions, p => p.Action == "document:write");
    }

    [Fact]
    public void AddPermission_Duplicate_ShouldThrow()
    {
        var role = Role.Create("Editor", null, Guid.NewGuid(), ScopeType.Workspace);
        role.AddPermission("document:read");

        Assert.Throws<DomainException>(() => role.AddPermission("document:read"));
    }

    [Fact]
    public void AddPermission_SystemRole_ShouldThrow()
    {
        var role = Role.Create("System Role", null, Guid.NewGuid(), ScopeType.Organization);
        typeof(Role).GetProperty("IsSystem")!.SetValue(role, true);

        Assert.Throws<DomainException>(() => role.AddPermission("test:action"));
    }

    [Fact]
    public void RemovePermission_ShouldRemoveFromCollection()
    {
        var role = Role.Create("Editor", null, Guid.NewGuid(), ScopeType.Workspace);
        role.AddPermission("document:read");
        role.AddPermission("document:write");

        role.RemovePermission("document:read");

        Assert.Single(role.Permissions);
        Assert.DoesNotContain(role.Permissions, p => p.Action == "document:read");
    }

    [Fact]
    public void RemovePermission_NotFound_ShouldThrow()
    {
        var role = Role.Create("Editor", null, Guid.NewGuid(), ScopeType.Workspace);

        Assert.Throws<DomainException>(() => role.RemovePermission("nonexistent:action"));
    }

    [Fact]
    public void HasPermission_ReturnsTrue_WhenExists()
    {
        var role = Role.Create("Editor", null, Guid.NewGuid(), ScopeType.Workspace);
        role.AddPermission("document:read");

        Assert.True(role.HasPermission("document:read"));
    }

    [Fact]
    public void HasPermission_ReturnsFalse_WhenNotExists()
    {
        var role = Role.Create("Editor", null, Guid.NewGuid(), ScopeType.Workspace);

        Assert.False(role.HasPermission("document:read"));
    }
}
