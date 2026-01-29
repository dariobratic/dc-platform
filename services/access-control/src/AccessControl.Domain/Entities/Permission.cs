using AccessControl.Domain.Exceptions;

namespace AccessControl.Domain.Entities;

public class Permission
{
    public Guid Id { get; private set; }
    public Guid RoleId { get; private set; }
    public string Action { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    private Permission()
    {
    }

    internal static Permission Create(Guid roleId, string action)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new DomainException("Permission action cannot be empty.");

        return new Permission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            Action = action,
            CreatedAt = DateTime.UtcNow
        };
    }
}
