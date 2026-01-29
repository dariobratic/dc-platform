using AccessControl.Application.Interfaces;
using AccessControl.Application.Responses;
using MediatR;

namespace AccessControl.Application.Queries.Permissions.CheckPermission;

public sealed class CheckPermissionHandler : IRequestHandler<CheckPermissionQuery, PermissionCheckResponse>
{
    private readonly IRoleAssignmentRepository _roleAssignmentRepository;

    public CheckPermissionHandler(IRoleAssignmentRepository roleAssignmentRepository)
    {
        _roleAssignmentRepository = roleAssignmentRepository;
    }

    public async Task<PermissionCheckResponse> Handle(CheckPermissionQuery request, CancellationToken cancellationToken)
    {
        var assignments = await _roleAssignmentRepository.GetByUserAndScopeAsync(
            request.UserId,
            request.ScopeId,
            cancellationToken);

        var hasPermission = assignments.Any(a => a.Role.HasPermission(request.Permission));

        return new PermissionCheckResponse(
            hasPermission,
            request.UserId,
            request.ScopeId,
            request.Permission);
    }
}
