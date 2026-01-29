using AccessControl.Application.Exceptions;
using AccessControl.Application.Interfaces;
using MediatR;

namespace AccessControl.Application.Commands.RoleAssignments.RevokeRole;

public sealed class RevokeRoleHandler : IRequestHandler<RevokeRoleCommand>
{
    private readonly IRoleAssignmentRepository _roleAssignmentRepository;

    public RevokeRoleHandler(IRoleAssignmentRepository roleAssignmentRepository)
    {
        _roleAssignmentRepository = roleAssignmentRepository;
    }

    public async Task Handle(RevokeRoleCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _roleAssignmentRepository.FindAsync(
            request.RoleId,
            request.UserId,
            request.ScopeId,
            cancellationToken);

        if (assignment == null)
            throw new NotFoundException("RoleAssignment", $"RoleId={request.RoleId}, UserId={request.UserId}, ScopeId={request.ScopeId}");

        assignment.Revoke();

        await _roleAssignmentRepository.DeleteAsync(assignment, cancellationToken);
    }
}
