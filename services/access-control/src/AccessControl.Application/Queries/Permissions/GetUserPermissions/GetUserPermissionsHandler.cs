using AccessControl.Application.Interfaces;
using MediatR;

namespace AccessControl.Application.Queries.Permissions.GetUserPermissions;

public sealed class GetUserPermissionsHandler : IRequestHandler<GetUserPermissionsQuery, List<string>>
{
    private readonly IRoleAssignmentRepository _roleAssignmentRepository;

    public GetUserPermissionsHandler(IRoleAssignmentRepository roleAssignmentRepository)
    {
        _roleAssignmentRepository = roleAssignmentRepository;
    }

    public async Task<List<string>> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
    {
        var assignments = await _roleAssignmentRepository.GetByUserAndScopeAsync(
            request.UserId,
            request.ScopeId,
            cancellationToken);

        var permissions = assignments
            .SelectMany(a => a.Role.Permissions)
            .Select(p => p.Action)
            .Distinct()
            .ToList();

        return permissions;
    }
}
