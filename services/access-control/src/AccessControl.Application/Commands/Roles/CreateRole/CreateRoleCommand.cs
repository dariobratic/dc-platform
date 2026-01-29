using AccessControl.Application.Responses;
using AccessControl.Domain.Enums;
using MediatR;

namespace AccessControl.Application.Commands.Roles.CreateRole;

public sealed record CreateRoleCommand(
    string Name,
    string? Description,
    Guid ScopeId,
    ScopeType ScopeType,
    List<string> Permissions) : IRequest<RoleResponse>;
