using AccessControl.Application.Responses;
using MediatR;

namespace AccessControl.Application.Queries.Permissions.CheckPermission;

public sealed record CheckPermissionQuery(
    Guid UserId,
    Guid ScopeId,
    string Permission) : IRequest<PermissionCheckResponse>;
