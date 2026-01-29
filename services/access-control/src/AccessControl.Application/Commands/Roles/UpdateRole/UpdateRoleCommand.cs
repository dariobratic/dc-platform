using AccessControl.Application.Responses;
using MediatR;

namespace AccessControl.Application.Commands.Roles.UpdateRole;

public sealed record UpdateRoleCommand(
    Guid Id,
    string Name,
    string? Description,
    List<string> Permissions) : IRequest<RoleResponse>;
