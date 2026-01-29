using AccessControl.Application.Exceptions;
using AccessControl.Application.Interfaces;
using AccessControl.Application.Responses;
using MediatR;

namespace AccessControl.Application.Queries.Roles.GetRoleById;

public sealed class GetRoleByIdHandler : IRequestHandler<GetRoleByIdQuery, RoleResponse>
{
    private readonly IRoleRepository _roleRepository;

    public GetRoleByIdHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<RoleResponse> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdWithPermissionsAsync(request.Id, cancellationToken);
        if (role == null)
            throw new NotFoundException("Role", request.Id);

        return RoleResponse.FromEntity(role);
    }
}
