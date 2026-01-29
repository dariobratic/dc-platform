using AccessControl.Application.Responses;
using AccessControl.Domain.Enums;
using MediatR;

namespace AccessControl.Application.Queries.Roles.GetRolesByScope;

public sealed record GetRolesByScopeQuery(
    Guid ScopeId,
    ScopeType ScopeType) : IRequest<List<RoleResponse>>;
