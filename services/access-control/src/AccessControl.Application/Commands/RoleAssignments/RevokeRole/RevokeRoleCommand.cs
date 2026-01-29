using MediatR;

namespace AccessControl.Application.Commands.RoleAssignments.RevokeRole;

public sealed record RevokeRoleCommand(
    Guid RoleId,
    Guid UserId,
    Guid ScopeId) : IRequest;
