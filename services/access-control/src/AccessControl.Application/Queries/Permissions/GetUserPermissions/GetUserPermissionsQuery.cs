using MediatR;

namespace AccessControl.Application.Queries.Permissions.GetUserPermissions;

public sealed record GetUserPermissionsQuery(
    Guid UserId,
    Guid ScopeId) : IRequest<List<string>>;
