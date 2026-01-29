using AccessControl.Application.Responses;
using AccessControl.Domain.Enums;
using MediatR;

namespace AccessControl.Application.Commands.RoleAssignments.AssignRole;

public sealed record AssignRoleCommand(
    Guid RoleId,
    Guid UserId,
    Guid ScopeId,
    ScopeType ScopeType,
    Guid AssignedBy) : IRequest<RoleAssignmentResponse>;
