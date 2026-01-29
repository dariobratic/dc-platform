using AccessControl.Domain.Entities;
using AccessControl.Domain.Enums;

namespace AccessControl.Application.Responses;

public sealed record RoleResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid ScopeId,
    ScopeType ScopeType,
    bool IsSystem,
    List<string> Permissions,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static RoleResponse FromEntity(Role role)
    {
        return new RoleResponse(
            role.Id,
            role.Name,
            role.Description,
            role.ScopeId,
            role.ScopeType,
            role.IsSystem,
            role.Permissions.Select(p => p.Action).ToList(),
            role.CreatedAt,
            role.UpdatedAt);
    }
}
