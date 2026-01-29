using AccessControl.Application.Interfaces;
using AccessControl.Application.Responses;
using MediatR;

namespace AccessControl.Application.Queries.Roles.GetRolesByScope;

public sealed class GetRolesByScopeHandler : IRequestHandler<GetRolesByScopeQuery, List<RoleResponse>>
{
    private readonly IRoleRepository _roleRepository;

    public GetRolesByScopeHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<List<RoleResponse>> Handle(GetRolesByScopeQuery request, CancellationToken cancellationToken)
    {
        var roles = await _roleRepository.GetByScopeAsync(
            request.ScopeId,
            request.ScopeType,
            cancellationToken);

        return roles.Select(RoleResponse.FromEntity).ToList();
    }
}
