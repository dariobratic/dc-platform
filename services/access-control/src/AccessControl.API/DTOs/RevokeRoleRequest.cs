namespace AccessControl.API.DTOs;

public sealed record RevokeRoleRequest(
    Guid UserId,
    Guid ScopeId);
