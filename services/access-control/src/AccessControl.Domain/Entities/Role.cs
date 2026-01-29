using AccessControl.Domain.Enums;
using AccessControl.Domain.Events;
using AccessControl.Domain.Exceptions;

namespace AccessControl.Domain.Entities;

public class Role : BaseEntity
{
    private readonly List<Permission> _permissions = [];

    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public Guid ScopeId { get; private set; }
    public ScopeType ScopeType { get; private set; }
    public bool IsSystem { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyCollection<Permission> Permissions => _permissions.AsReadOnly();

    private Role()
    {
    }

    public static Role Create(string name, string? description, Guid scopeId, ScopeType scopeType)
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            ScopeId = scopeId,
            ScopeType = scopeType,
            IsSystem = false,
            CreatedAt = DateTime.UtcNow
        };

        role.RaiseDomainEvent(new RoleCreated(role.Id, role.Name, role.ScopeId, role.ScopeType));

        return role;
    }

    public void Update(string name, string? description)
    {
        if (IsSystem)
            throw new DomainException("Cannot update system roles.");

        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new RoleUpdated(Id, Name));
    }

    public void AddPermission(string action)
    {
        if (IsSystem)
            throw new DomainException("Cannot modify permissions of system roles.");

        if (_permissions.Any(p => p.Action == action))
            throw new DomainException($"Permission '{action}' already exists in role.");

        var permission = Permission.Create(Id, action);
        _permissions.Add(permission);
    }

    public void RemovePermission(string action)
    {
        if (IsSystem)
            throw new DomainException("Cannot modify permissions of system roles.");

        var permission = _permissions.FirstOrDefault(p => p.Action == action);
        if (permission == null)
            throw new DomainException($"Permission '{action}' not found in role.");

        _permissions.Remove(permission);
    }

    public void Delete()
    {
        if (IsSystem)
            throw new DomainException("Cannot delete system roles.");

        RaiseDomainEvent(new RoleDeleted(Id));
    }

    public bool HasPermission(string action) => _permissions.Any(p => p.Action == action);
}
