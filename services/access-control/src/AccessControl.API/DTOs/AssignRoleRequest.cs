using AccessControl.Domain.Enums;

namespace AccessControl.API.DTOs;

public sealed record AssignRoleRequest(
    Guid UserId,
    Guid ScopeId,
    ScopeType ScopeType,
    Guid AssignedBy);
