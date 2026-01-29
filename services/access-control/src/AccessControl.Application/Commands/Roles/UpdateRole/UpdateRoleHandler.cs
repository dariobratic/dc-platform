using AccessControl.Application.Exceptions;
using AccessControl.Application.Interfaces;
using AccessControl.Application.Responses;
using MediatR;

namespace AccessControl.Application.Commands.Roles.UpdateRole;

public sealed class UpdateRoleHandler : IRequestHandler<UpdateRoleCommand, RoleResponse>
{
    private readonly IRoleRepository _roleRepository;

    public UpdateRoleHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<RoleResponse> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdWithPermissionsAsync(request.Id, cancellationToken);
        if (role == null)
            throw new NotFoundException("Role", request.Id);

        var nameExists = await _roleRepository.NameExistsInScopeAsync(
            request.Name,
            role.ScopeId,
            role.ScopeType,
            role.Id,
            cancellationToken);

        if (nameExists)
            throw new ConflictException($"Role with name '{request.Name}' already exists in this scope.");

        role.Update(request.Name, request.Description);

        // Sync permissions
        var currentPermissions = role.Permissions.Select(p => p.Action).ToList();
        var permissionsToRemove = currentPermissions.Except(request.Permissions).ToList();
        var permissionsToAdd = request.Permissions.Except(currentPermissions).ToList();

        foreach (var permission in permissionsToRemove)
        {
            role.RemovePermission(permission);
        }

        foreach (var permission in permissionsToAdd)
        {
            role.AddPermission(permission);
        }

        await _roleRepository.UpdateAsync(role, cancellationToken);

        return RoleResponse.FromEntity(role);
    }
}
