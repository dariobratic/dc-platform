using AccessControl.Domain.Entities;
using AccessControl.Domain.Enums;

namespace AccessControl.Application.Responses;

public sealed record RoleAssignmentResponse(
    Guid Id,
    Guid RoleId,
    string RoleName,
    Guid UserId,
    Guid ScopeId,
    ScopeType ScopeType,
    DateTime AssignedAt,
    Guid AssignedBy)
{
    public static RoleAssignmentResponse FromEntity(RoleAssignment assignment)
    {
        return new RoleAssignmentResponse(
            assignment.Id,
            assignment.RoleId,
            assignment.Role.Name,
            assignment.UserId,
            assignment.ScopeId,
            assignment.ScopeType,
            assignment.AssignedAt,
            assignment.AssignedBy);
    }
}
