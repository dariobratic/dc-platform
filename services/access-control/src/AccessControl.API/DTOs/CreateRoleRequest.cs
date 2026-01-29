using AccessControl.Domain.Enums;

namespace AccessControl.API.DTOs;

public sealed record CreateRoleRequest(
    string Name,
    string? Description,
    Guid ScopeId,
    ScopeType ScopeType,
    List<string> Permissions);
