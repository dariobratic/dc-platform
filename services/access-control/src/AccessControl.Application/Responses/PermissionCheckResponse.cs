namespace AccessControl.Application.Responses;

public sealed record PermissionCheckResponse(
    bool HasPermission,
    Guid UserId,
    Guid ScopeId,
    string Permission);
